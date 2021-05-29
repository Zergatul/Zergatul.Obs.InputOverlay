# Zergatul.Obs.InputOverlay
Input overlay plugin for OBS. Supported systems: Windows 10 x64

# Installation
1. Download latest release here: https://github.com/Zergatul/Zergatul.Obs.InputOverlay/releases
1. Unzip it to any location you want

# Basic usage
1. Run `Zergatul.Obs.InputOverlay.exe`, and keep it open while you want to use input overlay in OBS
1. In OBS, add Source 🡒 Browser.
    - `URL`: `http://localhost:5001/keyboard.html`
    - `Width`: `600`, `Height`: `360`. Use can expiriment with another values
    - Check `Use custom framerate`, and set FPS to `60`, if you are streaming/recording in 60 FPS
    - Check `Shutdown source when not visible`. This will allow you to hide/show source after you restarted the server
    - Do the same for mouse: `URL`: `http://localhost:5001/mouse.html`, `Width`: `250`, `Height`: `350`

# Advanced usage
1. If you are running server application without elevated priviledges, it will be able to detect your inputs from applications running under administrator. Example: I have game started from Steam, and I command prompt, running as Administrator. Server will be able to detect inputs in game, but will show nothing when I type something in command prompt.
1. You can show more keys, change style, colors, animations if you are familiar with HTML and CSS. Check `wwwroot` folder within application folder. It contains HTML, CSS and JavaScript files.
1. If you need to restart server application, you will need to hide sources in OBS, and show them again.
