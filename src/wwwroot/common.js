function attachSvgGlowDefinition(svgDoc) {

    let svg = svgDoc.rootElement;

    let defs = document.createElementNS('http://www.w3.org/2000/svg', 'defs');

    let filter = document.createElementNS('http://www.w3.org/2000/svg', 'filter');
    filter.id = 'glow';
    filter.setAttribute('filterUnits', 'userSpaceOnUse');
    filter.setAttribute('x', '-50%');
    filter.setAttribute('y', '-50%');
    filter.setAttribute('width', '200%');
    filter.setAttribute('height', '200%');

    let gaussianBlur = document.createElementNS('http://www.w3.org/2000/svg', 'feGaussianBlur');
    gaussianBlur.setAttribute('in', 'SourceGraphic');
    gaussianBlur.setAttribute('stdDeviation', 10);
    gaussianBlur.setAttribute('result', 'blur20');

    let colorMatrix = document.createElementNS('http://www.w3.org/2000/svg', 'feColorMatrix');
    colorMatrix.setAttribute('in', 'blur20');
    colorMatrix.setAttribute('type', 'matrix');
    colorMatrix.setAttribute('values',
        '1 0 0 2 0 ' +
        '0 1 0 2 0 ' +
        '0 0 1 2 0 ' +
        '0 0 0 1 0');
    colorMatrix.setAttribute('result', 'blur-colored');

    let merge = document.createElementNS('http://www.w3.org/2000/svg', 'feMerge');
    let mergeNode1 = document.createElementNS('http://www.w3.org/2000/svg', 'feMergeNode');
    mergeNode1.setAttribute('in', 'blur-colored');
    let mergeNode2 = document.createElementNS('http://www.w3.org/2000/svg', 'feMergeNode');
    mergeNode2.setAttribute('in', 'SourceGraphic');
    merge.appendChild(mergeNode1);
    merge.appendChild(mergeNode2);

    filter.appendChild(gaussianBlur);
    filter.appendChild(colorMatrix);
    filter.appendChild(merge);

    defs.appendChild(filter);

    svg.appendChild(defs);
}

function displayFps() {
    let interval = 1000;
    let list = [];
    let div = document.createElement('div');
    div.style.position = 'absolute';
    div.style.left = '0';
    div.style.top = '0';
    div.style.fontSize = '50px';
    div.style.color = 'white';
    document.body.appendChild(div);
    let callback = function () {
        let now = window.performance.now();
        if (list.length > 0) {
            let elapsed = now - list[0];
            let fps = 1000 * list.length / elapsed;
            div.innerHTML = 'FPS: ' + fps.toFixed(1);
            let count = 0;
            while (count < list.length && now - list[count] > interval) {
                count++;
            }
            if (count == list.length) {
                count--;
            }
            list.splice(0, count);
        }
        list.push(now);
        window.requestAnimationFrame(callback);
    };
    window.requestAnimationFrame(callback);
}