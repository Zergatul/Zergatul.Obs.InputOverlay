using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Zergatul.Obs.InputOverlay
{
    public interface IWebSocketHandler : IDisposable
    {
        Task HandleWebSocket(WebSocket ws);
    }
}