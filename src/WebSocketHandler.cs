using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Zergatul.Obs.InputOverlay
{
    public class WebSocketHandler : IWebSocketHandler
    {
        private readonly IInputHook _hook;
        private readonly ILogger _logger;
        private readonly object _syncObject = new object();
        private readonly Random _rnd = new Random();
        private List<WebSocketWrapper> _webSockets = new List<WebSocketWrapper>();
        private WebSocketWrapper[] _wsBuffer = new WebSocketWrapper[16];

        public WebSocketHandler(IInputHook hook, ILogger<WebSocketHandler> logger)
        {
            _hook = hook;
            _logger = logger;

            _hook.ButtonAction += Hook_ButtonAction;
        }

        private async void Hook_ButtonAction(object sender, ButtonEvent e)
        {
            int count;
            lock (_syncObject)
            {
                count = _webSockets.Count;
                if (count > _wsBuffer.Length)
                {
                    _logger.LogWarning("WebSockets count greater than buffer size.");
                    count = _wsBuffer.Length;
                }
                _webSockets.CopyTo(0, _wsBuffer, 0, count);
            }

            int eventType = e.Button < Button.Mouse1 ? 1 : 2;
            var json = JsonSerializer.Serialize(new JavascriptEvent
            {
                type = eventType,
                button = e.Button.ToString(),
                pressed = e.Pressed,
                count = e.Count
            });
            byte[] raw = Encoding.ASCII.GetBytes(json);

            for (int i = 0; i < count; i++)
            {
                var wrapper = _wsBuffer[i];
                if ((wrapper.EventMask & eventType) != 0)
                {
                    try
                    {
                        await wrapper.WebSocket.SendAsync(new ReadOnlyMemory<byte>(raw), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch (WebSocketException)
                    {
                        _logger?.LogInformation("WebSocketException on SendAsync.");
                        RemoveWebSocket(wrapper);
                    }
                }
            }
        }

        public async Task HandleWebSocket(WebSocket ws)
        {
            var wrapper = new WebSocketWrapper
            {
                WebSocket = ws,
                CancellationSource = new CancellationTokenSource()
            };
            lock (_syncObject)
            {
                _webSockets.Add(wrapper);
            }

            _logger?.LogInformation($"New WebSocket. Total WebSockets: {_webSockets.Count}.");

            var receive = ReceiveLoop(wrapper);
            var ping = PingLoop(wrapper);
            await Task.WhenAll(receive, ping);

            wrapper.CancellationSource.Dispose();
            _logger?.LogInformation("HandleWebSocket ended successfully.");
        }

        public void Dispose()
        {
            _hook.Dispose();

            int count;
            lock (_syncObject)
            {
                count = _webSockets.Count;
                _webSockets.CopyTo(_wsBuffer);
            }

            for (int i = 0; i < count; i++)
            {
                RemoveWebSocket(_wsBuffer[i]);
            }

            lock (_syncObject)
            {
                _webSockets.Clear();
            }
        }

        private void RemoveWebSocket(WebSocketWrapper wrapper)
        {
            lock (wrapper)
            {
                if (wrapper.Closing)
                    return;

                wrapper.Closing = true;
            }

            _logger?.LogInformation("WebSocket disconnected.");

            wrapper.CancellationSource.Cancel();

            lock (_syncObject)
            {
                int index = _webSockets.IndexOf(wrapper);
                if (index >= 0)
                {
                    _webSockets.RemoveAt(index);
                }
            }

            _logger?.LogInformation($"Total WebSockets: {_webSockets.Count}.");
        }

        private async Task ReceiveLoop(WebSocketWrapper wrapper)
        {
            try
            {
                while (true)
                {
                    var segment = new ArraySegment<byte>(new byte[256]);

                    WebSocketReceiveResult result;
                    try
                    {
                        result = await wrapper.WebSocket.ReceiveAsync(segment, wrapper.CancellationSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (WebSocketException)
                    {
                        _logger?.LogInformation("WebSocketException on ReceiveAsync.");
                        RemoveWebSocket(wrapper);
                        return;
                    }

                    if (result.Count == 0)
                    {
                        _logger?.LogInformation("Empty response.");
                        RemoveWebSocket(wrapper);
                        return;
                    }

                    var evt = JsonSerializer.Deserialize<ClientEvent>(segment.AsSpan(0, result.Count));
                    if (evt.eventMask != null)
                    {
                        wrapper.EventMask = evt.eventMask.Value;
                    }
                    if (evt.ping != null)
                    {
                        if (wrapper.LastPing != evt.ping)
                        {
                            _logger?.LogInformation("Receive ping data doesn't match.");
                            RemoveWebSocket(wrapper);
                            return;
                        }
                        wrapper.LastPing = null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("ReceiveLoop -> " + ex.GetType().ToString() + " " + ex.Message);
            }
        }

        private async Task PingLoop(WebSocketWrapper wrapper)
        {
            try
            {
                while (true)
                {
                    if (wrapper.LastPing != null)
                    {
                        _logger?.LogInformation("Ping response not received;");
                        RemoveWebSocket(wrapper);
                        return;
                    }

                    lock (_rnd)
                    {
                        wrapper.LastPing = _rnd.Next();
                    }

                    var json = JsonSerializer.Serialize(new JavascriptEvent
                    {
                        type = 0,
                        ping = wrapper.LastPing
                    });
                    byte[] raw = Encoding.ASCII.GetBytes(json);

                    try
                    {
                        await wrapper.WebSocket.SendAsync(new ReadOnlyMemory<byte>(raw), WebSocketMessageType.Text, true, wrapper.CancellationSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (WebSocketException)
                    {
                        _logger?.LogInformation("WebSocketException on SendAsync.");
                        RemoveWebSocket(wrapper);
                        return;
                    }

                    try
                    {
                        await Task.Delay(1000, wrapper.CancellationSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("PingLoop -> " + ex.GetType().ToString() + " " + ex.Message);
            }
        }

        private class WebSocketWrapper
        {
            public WebSocket WebSocket;
            public int EventMask;
            public CancellationTokenSource CancellationSource;
            public int? LastPing;
            public volatile bool Closing;
        }

        private class JavascriptEvent
        {
            public int type { get; set; }
            public string button { get; set; }
            public bool pressed { get; set; }
            public int? count { get; set; }
            public int? ping { get; set; }
        }

        private class ClientEvent
        {
            public int? eventMask { get; set; }
            public int? ping { get; set; }
        }
    }
}