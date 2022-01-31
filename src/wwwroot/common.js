const XInputButtons = {
    DPadUp: 0x0001,
    DPadDown: 0x0002,
    DPadLeft: 0x0004,
    DPadRight: 0x0008,
    Start: 0x0010,
    Back: 0x0020,
    L3: 0x0040,
    R3: 0x0080,
    LB: 0x0100,
    RB: 0x0200,
    A: 0x1000,
    B: 0x2000,
    X: 0x4000,
    Y: 0x8000
};

const XInputButtonNames = [
    'Start',
    'Back',
    'L3',
    'R3',
    'LB',
    'RB',
    'A',
    'B',
    'X',
    'Y'
];

const XInputButtonNamesWithDPad = [
    'Start',
    'Back',
    'L3',
    'R3',
    'LB',
    'RB',
    'A',
    'B',
    'X',
    'Y',
    'DPadUp',
    'DPadDown',
    'DPadLeft',
    'DPadRight'
];

function createDefaultGamepadState() {
    return {
        DPadUp: false,
        DPadDown: false,
        DPadLeft: false,
        DPadRight: false,
        Start: false,
        Back: false,
        L3: false,
        R3: false,
        LB: false,
        RB: false,
        A: false,
        B: false,
        X: false,
        Y: false,
        LX: 0,
        LY: 0,
        RX: 0,
        RY: 0,
        LT: 0,
        RT: 0
    };
}

function getDPadIndex(state) {
    if (state.DPadUp) {
        if (state.DPadLeft) {
            return 7;
        }
        if (state.DPadRight) {
            return 1;
        }
        return 0;
    }
    if (state.DPadDown) {
        if (state.DPadLeft) {
            return 5;
        }
        if (state.DPadRight) {
            return 3;
        }
        return 4;
    }
    if (state.DPadLeft) {
        return 6;
    }
    if (state.DPadRight) {
        return 2;
    }
    return null;
}

// dpad is separate
function applyStateChange(state, data, onDPadPressed, onDPadUnpressed, onButtonPressed, onButtonUnpressed, onLeftStickMove, onRightStickMove, onLeftTriggerMove, onRightTriggerMove) {
    // dpad
    let oldIndex = getDPadIndex(state);
    state.DPadUp = !!(data.buttons & XInputButtons.DPadUp);
    state.DPadDown = !!(data.buttons & XInputButtons.DPadDown);
    state.DPadLeft = !!(data.buttons & XInputButtons.DPadLeft);
    state.DPadRight = !!(data.buttons & XInputButtons.DPadRight);
    let newIndex = getDPadIndex(state);
    if (oldIndex != newIndex) {
        if (oldIndex != null) {
            onDPadUnpressed(oldIndex);
        }
        if (newIndex != null) {
            onDPadPressed(newIndex);
        }
    }
    // buttons
    for (let i = 0; i < XInputButtonNames.length; i++) {
        let name = XInputButtonNames[i];
        if (state[name] && !(data.buttons & XInputButtons[name])) {
            onButtonUnpressed(name);
        }
        if (!state[name] && (data.buttons & XInputButtons[name])) {
            onButtonPressed(name);
        }
        state[name] = !!(data.buttons & XInputButtons[name]);
    }
    // sticks
    if (state.LX != data.lx || state.LY != data.ly) {
        onLeftStickMove(data.lx, data.ly);
        state.LX = data.lx;
        state.LY = data.ly;
    }
    if (state.RX != data.rx || state.RY != data.ry) {
        onRightStickMove(data.rx, data.ry);
        state.RX = data.rx;
        state.RY = data.ry;
    }
    // triggers
    if (state.LT != data.lt) {
        onLeftTriggerMove(data.lt);
        state.LT = data.lt;
    }
    if (state.RT != data.rt) {
        onRightTriggerMove(data.rt);
        state.RT = data.rt;
    }
}

// dpad as buttons
function applyStateChange2(state, data, onButtonPressed, onButtonUnpressed, onLeftStickMove, onRightStickMove, onLeftTriggerMove, onRightTriggerMove) {
    // buttons
    for (let i = 0; i < XInputButtonNamesWithDPad.length; i++) {
        let name = XInputButtonNamesWithDPad[i];
        if (state[name] && !(data.buttons & XInputButtons[name])) {
            onButtonUnpressed(name);
        }
        if (!state[name] && (data.buttons & XInputButtons[name])) {
            onButtonPressed(name);
        }
        state[name] = !!(data.buttons & XInputButtons[name]);
    }
    // sticks
    if (state.LX != data.lx || state.LY != data.ly) {
        onLeftStickMove(data.lx, data.ly);
        state.LX = data.lx;
        state.LY = data.ly;
    }
    if (state.RX != data.rx || state.RY != data.ry) {
        onRightStickMove(data.rx, data.ry);
        state.RX = data.rx;
        state.RY = data.ry;
    }
    // triggers
    if (state.LT != data.lt) {
        onLeftTriggerMove(data.lt);
        state.LT = data.lt;
    }
    if (state.RT != data.rt) {
        onRightTriggerMove(data.rt);
        state.RT = data.rt;
    }
}

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