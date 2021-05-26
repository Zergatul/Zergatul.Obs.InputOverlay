using System;

namespace Zergatul.Obs.InputOverlay
{
    public class EmptyInputHook : IInputHook
    {
        public event EventHandler<ButtonEvent> ButtonAction;

        public void Dispose()
        {
            
        }
    }
}