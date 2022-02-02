# Modules

## Mouse speed (`mouse-speed.html`)
Displays mouse speed overlay. Open file in text editor to adjust settings. Browser source parameters: Width: `500`, Height: `100`.

![mouse-speed.html example](https://github.com/Zergatul/Zergatul.Obs.InputOverlay/blob/master/docs/MouseSpeed.png?raw=true)

## Mouse movement (`mouse-movement.html`)
Displays circle indicating current mouse movement direction. Open file in text editor to adjust settings. Browser source parameters: Width: `800`, Height: `800`.

![mouse-movement.html example](https://github.com/Zergatul/Zergatul.Obs.InputOverlay/blob/master/docs/MouseMovement.png?raw=true)

## Flicks (`mouse-flick.html`)
Displays text when you perform flick in FPV games (in other words when mouse moves some distance horizontally at some period of time). Open file in text editor to adjust settings. Browser source parameters: Width: `300`, Height: `100`.

![mouse-flick.html example](https://github.com/Zergatul/Zergatul.Obs.InputOverlay/blob/master/docs/MouseFlick.png?raw=true)

## Full keyboard (`keyboard-full.html`)
Full keyboard.

## Gamepads
You can find multiple gamepads files. Usually they looks like this `<gamepad_name>-<api>.html`. You should prefer XInput API for gamepads if your gamepad is XInput compatible. There are also example how to use RawInput for gamepads. Default XBox raw input gamepad driver reports triggers as single axis, meaning you cannot detect when user holds 2 triggers at the same time. XInput API works fine with this. First part of gamepad file resresents visual representation. You can play on XBox controller, but use Dual Sense file for visual representation and vice versa. [Using multiple gamepads](docs/MultipleGamepads.md).

- **XBoxSeriesX-RawInput.html** - you should prefer XInput version for XInput compatibe gamepad, use this as example for customizing
- **XBoxSeriesX-XInput.html** - preferred file to use with XInput compatible devices
- **DualSense-RawInput.html** - DualSense is not XInput compatible, this file is configured specifically for DualSense
- **DualSense-XInput.html** - This doesn't work with DualSense! Use if it for XInput compatible gamepad, but if you want visual representation of DualSense