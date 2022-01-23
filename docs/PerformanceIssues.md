# Performance issues

# Browser Source
Sometimes Browser Sources in OBS doesn't have enough system resources to render graphics. I thought that this is due to game takes priority over GPU resources and it leaves nothing for browser sources. But it turned out to be more complicated than that and I still don't understand how it works.

First you need to make sure you have performance issues with Browser Sources. To do this check if overlay works fine without running game. Start the game. If overlay "stopped" working, hold any button for 10+ seconds. If you see button is highlighed with delay, and also it fades out with delay and lagging - you definitely have performance issues.

Sometimes overlay works fine, but starts lagging when you encounter demanding scene in-game, which occupies 100% of GPU resources.

# Possible fixes
They are ordered by the chance of resolving issue.
- Set Browser Source custom frame rate to `30`. Works really well, and I don't understand why.
- Reduce GPU usage. Examples: set FPS limit, reduce graphics settings, turn on DLSS. Sometimes works, sometimes doesn't.
- For every visible Browser Source OBS starts `obs-browser-page.exe` process. Set its priority to high.
- Reduce browser source rendering resolution. In Browser Source settings add custom CSS rule `zoom: 0.5;`. Example: `body { background-color: rgba(0, 0, 0, 0); margin: 0px auto; overflow: hidden; zoom: 0.5; }`. After this reduce width and height settings by the same factor.