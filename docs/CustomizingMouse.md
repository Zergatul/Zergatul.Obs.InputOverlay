# Customizing mouse/gamepad

Below is an example for mouse, but same logic applies to gamepad SVG.


All examples here are done in Inkscape. This is free and open-source vector graphics editor. You can use another editors, but you may need additional steps which are not covered here. In `wwwroot` directory you can find `mouse.svg`. You can open it in editor to understand how example mouse image works. In Objects window you can see some SVG paths are labeled Mouse1, Mouse2 and so on:

![ObjectsTree](https://github.com/Zergatul/Zergatul.Obs.InputOverlay/blob/master/docs/InkscapeObjectsTree.png?raw=true)

These labels are not required, but they will help you to quickly navigate inside SVG document. If you select one of these labeled paths, in Object Properties window you can see ID of an object:

![ObjectProperties](https://github.com/Zergatul/Zergatul.Obs.InputOverlay/blob/master/docs/InkscapeObjectProperties.png?raw=true)

These ID's are required. In javascript we search SVG elements by ID, and highlight them when buttons are pressed.

Now you can make some changes to example mouse, and try to replace existing mouse image in `default-mouse.html`. After you made some changes to SVG file, open it in any text editor. Copy entire `<svg>` element (this is entire file without first line). Open `default-mouse.html`, delete entire `<svg>` element and `Ctrl+V` another `<svg>` element. Save html file (maybe with different name) and check how it looks.