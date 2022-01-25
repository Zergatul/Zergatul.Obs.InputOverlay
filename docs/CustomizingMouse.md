# Customizing mouse/gamepad

Below is an example for mouse, but same logic applies to gamepad SVG.

All examples here are done in Inkscape. This is free and open-source vector graphics editor. But you can use any SVG editor. In `wwwroot` directory you can find `mouse.svg`. You can open it in editor to understand how example mouse image works. In Objects window you can see some SVG paths are labeled Mouse1, Mouse2 and so on:

![ObjectsTree](https://github.com/Zergatul/Zergatul.Obs.InputOverlay/blob/master/docs/InkscapeObjectsTree.png?raw=true)

These labels are not required, but they will help you to quickly navigate inside SVG document. If you select one of these labeled paths, in Object Properties window you can see ID of an object:

![ObjectProperties](https://github.com/Zergatul/Zergatul.Obs.InputOverlay/blob/master/docs/InkscapeObjectProperties.png?raw=true)

These ID's are required. In javascript we search SVG elements by ID, and highlight them when buttons are pressed. If javascript code cannot find button by ID, it will not be able to highlight it. Once you make some changes to the SVG, you can save it and check how it looks in browser or OBS. Don't forget to uncomment `background-color` CSS style if you want to check it in browser.