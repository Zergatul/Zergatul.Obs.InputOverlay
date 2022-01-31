# Multiple gamepads

To use multiple gamepad overlays you should better choose XInput API (html files ending with `XInput`).

### XInput
XInput support up to 4 gamepads. Whenever you attach gamepad, it assigns number to it in order. In the log you should see next message: `Detected new XInput gamepad at index 0.`. It order to display input from single gamepad, just add `?0` to the end of URL. Example:
- `http://localhost:5001/XBoxSeriesX-XInput.html?0` - for first gamepad
- `http://localhost:5001/XBoxSeriesX-XInput.html?1` - for second gamepad

### RawInput
If you want to show multiple gamepads, check application log for "Gamepad added" events. Below this line you should see `HDevice=0x0000000035F70E29`. Copy hexadecimal string (`0x0000000035F70E29`), and instead of default URL use `http://localhost:5001/XBoxSeriesX-RawInput.html?0x0000000035F70E29`. Every gamepad has its own `HDevice` number. However this number changes every time you disconnect gamepad or restart PC.