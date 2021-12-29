# Customizing mouse

All examples here are done in Inkscape. This is free and open-source vector graphics editor. You can use another editors, but you may have additional problems which are not covered here. In `wwwroot` directory you can find `mouse.svg`. You can open it in editor to understand how example mouse image works. In Objects window you can see some SVG paths are labeled Mouse1, Mouse2 and so on:

![ObjectsTree](https://github.com/Zergatul/Zergatul.Obs.InputOverlay/blob/master/docs/InkscapeObjectsTree.png?raw=true)

These labels are not required, but they will help you to quickly navigate inside SVG document. If you select one of these labeled paths, in Object Properties window you can see ID of an object:

![ObjectProperties](https://github.com/Zergatul/Zergatul.Obs.InputOverlay/blob/master/docs/InkscapeObjectProperties.png?raw=true)

These ID's are required. In javascript we search SVG elements by ID, and highlight them when buttons are pressed.

Now you can make some changes to example mouse, and try to replace existing mouse image in `default-mouse.html`. After you made some changes to SVG file, open it in any text editor. Copy entire `<svg>` element (this is entire file without first line). Open `default-mouse.html`, delete entire `<svg>` element and `Ctrl+V` another `<svg>` element. Now remove `width` and `height` attributes of `svg` element. In below example lines `width="210mm"` and `height="297mm"` must be removed:

```XML
<svg
   xmlns:dc="http://purl.org/dc/elements/1.1/"
   xmlns:cc="http://creativecommons.org/ns#"
   xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
   xmlns:svg="http://www.w3.org/2000/svg"
   xmlns="http://www.w3.org/2000/svg"
   xmlns:sodipodi="http://sodipodi.sourceforge.net/DTD/sodipodi-0.dtd"
   xmlns:inkscape="http://www.inkscape.org/namespaces/inkscape"
   width="210mm"
   height="297mm"
   viewBox="0 0 210 297"
   version="1.1"
   id="svg222"
   sodipodi:docname="mouse.svg"
   inkscape:version="1.0.2 (e86c870879, 2021-01-15, custom)">
```

After `<metadata>` element, or before first `<g>` element insert glow filter definition:

```XML
<defs>
    <filter id="glow" filterUnits="userSpaceOnUse"
            x="-50%" y="-50%" width="200%" height="200%">
        <feGaussianBlur in="SourceGraphic" stdDeviation="10" result="blur20" />
        <feColorMatrix in="blur20" result="blur-colored" type="matrix"
                        values="1 0 0 2 0
                                0 1 0 2 0
                                0 0 1 2 0
                                0 0 0 1 0" />
        <feMerge>
            <feMergeNode in="blur-colored" />
            <feMergeNode in="SourceGraphic" />
        </feMerge>
    </filter>
</defs>
```

You can also find it inside original [default-mouse.html](../src/wwwroot/default-mouse.html). Now save updated `default-mouse.html` with different name and check how it looks.
