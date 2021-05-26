using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using static Zergatul.Obs.InputOverlay.WinApi;

namespace Zergatul.Obs.InputOverlay
{
    sealed class WindowsHook : IDisposable
    {
        public IntPtr HookHandle { get; private set; }

        private readonly object _syncObject = new object();
        private readonly ILogger _logger;
        private HookProc _proc;

        public WindowsHook(int type, HookProc proc, ILogger logger = null)
        {
            if (proc == null)
                throw new ArgumentNullException(nameof(proc));

            _proc = proc;
            _logger = logger;

            using (var process = Process.GetCurrentProcess())
            using (var module = process.MainModule)
            {
                HookHandle = SetWindowsHookEx(type, _proc, GetModuleHandle(module.ModuleName), 0);
            }

            if (HookHandle == IntPtr.Zero)
                throw new InvalidOperationException("Cannot set hook.");
            else
                _logger?.LogInformation("Hook set: " + FormatIntPtr(HookHandle));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (HookHandle != IntPtr.Zero)
            {
                lock (_syncObject)
                {
                    if (HookHandle != IntPtr.Zero)
                    {
                        if (UnhookWindowsHookEx(HookHandle))
                            _logger?.LogInformation("Hook released.");
                        else
                            _logger.LogWarning("Cannot release hook.");

                        HookHandle = IntPtr.Zero;
                    }
                }
            }
        }

        ~WindowsHook()
        {
            Dispose(false);
        }

        private static string FormatIntPtr(IntPtr ptr)
        {
            return "0x" + ptr.ToInt64().ToString("x2").PadLeft(16, '0');
        }
    }
}