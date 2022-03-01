using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace Zergatul.Obs.InputOverlay.XInput
{
    using static WinApi.XInput;

    public class XInputHandler : IXInputHandler
    {
        private const int PollingFrequency = 60;
        private const int PollingDelay = 1000 / PollingFrequency;

        public event Action<GamepadState> OnStateChanged;

        private readonly ILogger _logger;
        private Thread _pollingThread;
        private volatile bool _stopRequested;
        private XINPUT_GAMEPAD?[] _gamepads;

        public XInputHandler(ILogger<XInputHandler> logger)
        {
            _logger = logger;

            _gamepads = new XINPUT_GAMEPAD?[XUSER_MAX_COUNT];

            _pollingThread = new Thread(PollingThreadFunc);
            _pollingThread.Start();
        }

        public void Dispose()
        {
            _stopRequested = true;
            if (_pollingThread != null)
            {
                _pollingThread.Join();
                _pollingThread = null;
            }

            _logger.LogDebug("Disposed");
        }

        private void PollingThreadFunc()
        {
            int counter = 0;

            while (!_stopRequested)
            {
                if (++counter == PollingFrequency)
                {
                    counter = 0;

                    for (int i = 0; i < XUSER_MAX_COUNT; i++)
                    {
                        if (_gamepads[i] == null)
                        {
                            if (XInputGetState(i, out var state) == WinApi.Win32Error.ERROR_SUCCESS)
                            {
                                _gamepads[i] = state.Gamepad;
                                OnStateChanged?.Invoke(new GamepadState(i, state));

                                _logger.LogInformation($"Detected new XInput gamepad at index {i}.");
                            }
                        }
                    }
                }

                for (int i = 0; i < XUSER_MAX_COUNT; i++)
                {
                    if (_gamepads[i] == null)
                    {
                        continue;
                    }

                    if (XInputGetState(i, out var state) == WinApi.Win32Error.ERROR_SUCCESS)
                    {
                        if (!state.Gamepad.Equals(_gamepads[i].Value))
                        {
                            _gamepads[i] = state.Gamepad;
                            OnStateChanged?.Invoke(new GamepadState(i, state));
                        }
                    }
                    else
                    {
                        _gamepads[i] = null;
                        OnStateChanged?.Invoke(new GamepadState { Index = i });

                        _logger.LogInformation($"XInput gamepad at index {i} detached.");
                    }
                }

                Thread.Sleep(PollingDelay);
            }
        }
    }
}