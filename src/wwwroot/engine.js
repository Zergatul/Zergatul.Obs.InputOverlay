(function () {
    var addSvgReleased = function (element) {
        var animate = document.createElementNS('http://www.w3.org/2000/svg', 'animate');
        animate.setAttribute('id', element.id + '-animate');
        animate.setAttribute('attributeType', 'XML');
        animate.setAttribute('attributeName', 'fill-opacity');
        animate.setAttribute('values', '1.0; 0.0');
        animate.setAttribute('dur', '250ms');
        animate.setAttribute('fill', 'freeze');
    
        element.appendChild(animate);
        animate.beginElement();
    };

    var initScroll = function () {
        let scroll = document.getElementById('MouseScrollUp');
        if (scroll) {
            scroll.setAttribute('opacity', 0);
        }
        scroll = document.getElementById('MouseScrollDown');
        if (scroll) {
            scroll.setAttribute('opacity', 0);
        }
    };

    var removeSvgAnimations = function (element) {
        for (let i = 0; i < element.children.length; i++) {
            if (element.children[i].tagName == 'animateTransform' || element.children[i].tagName == 'animate') {
                element.removeChild(element.children[i]);
                i--;
            }
        }
    };

    let scrollUpTimeoutId = 0;
    let scrollDownTimeoutId = 0;

    let processScroll = function (id, dy, setupTimeout) {
        const delay = 250;
        let scroll = document.getElementById(id);
        removeSvgAnimations(scroll);

        let animateTransform = document.createElementNS('http://www.w3.org/2000/svg', 'animateTransform');
        animateTransform.setAttribute('attributeName', 'transform');
        animateTransform.setAttribute('attributeType', 'XML');
        animateTransform.setAttribute('type', 'translate');
        animateTransform.setAttribute('from', '0 0');
        animateTransform.setAttribute('to', '0 ' + dy);
        animateTransform.setAttribute('dur', delay + 'ms');
        animateTransform.setAttribute('repeatCount', '1');

        var animate = document.createElementNS('http://www.w3.org/2000/svg', 'animate');
        animate.setAttribute('attributeType', 'XML');
        animate.setAttribute('attributeName', 'opacity');
        animate.setAttribute('values', '1.0; 0.0');
        animate.setAttribute('dur', delay + 'ms');
        animate.setAttribute('repeatCount', '1');

        scroll.appendChild(animateTransform);
        scroll.appendChild(animate);
        animateTransform.beginElement();
        animate.beginElement();

        setupTimeout(function () {
            removeSvgAnimations(scroll);
            scroll.setAttribute('opacity', 0);
        }, delay);
    };

    let scrollUp = function () {
        processScroll('MouseScrollUp', '-10', function (callback, delay) {
            clearTimeout(scrollUpTimeoutId);
            scrollUpTimeoutId = setTimeout(callback, delay);
        });
    };

    let scrollDown = function () {
        processScroll('MouseScrollDown', '10', function (callback, delay) {
            clearTimeout(scrollDownTimeoutId);
            scrollDownTimeoutId = setTimeout(callback, delay);
        });
    };

    var state = {};

    window.listenEvents = function (eventType) {
        var ws = new WebSocket('ws://' + location.host + '/ws');
        ws.onopen = function () {
            ws.send(JSON.stringify({ eventMask: eventType }));
        };
        ws.onmessage = function (event) {
            var data = JSON.parse(event.data);
        
            if (data.type == 0) {
                ws.send(JSON.stringify({ ping: data.ping }));
            }
        
            if (eventType == 1 && data.type == 1) {
                if (data.pressed) {
                    state[data.button] = true;
                    var element = document.getElementById(data.button);
                    if (element) {
                        element.classList.remove('released');
                        element.classList.add('pressed');
                    }
                } else {
                    if (state[data.button]) {
                        delete state[data.button];
                        var element = document.getElementById(data.button);
                        if (element) {
                            element.classList.remove('pressed');
                            element.classList.add('released');
                        }
                    }
                }
            }
        
            if (eventType == 2 && data.type == 2) {
                if (data.button == 'MouseWheelUp') {
                    scrollUp();
                } else if (data.button == 'MouseWheelDown') {
                    scrollDown();
                } else {
                    if (data.pressed) {
                        state[data.button] = true;
                        var element = document.getElementById(data.button);
                        if (element) {
                            element.style.fill = '#2DA026';
                            element.style.filter = 'url(#glow)';
                            removeSvgAnimations(element);
                        }
                    } else {
                        if (state[data.button]) {
                            delete state[data.button];
                            var element = document.getElementById(data.button);
                            if (element) {
                                element.style.fill = '#96CF13';
                                element.style.filter = 'none';
                                addSvgReleased(element);
                            }
                        }
                    }
                }
            }
        };
    };

    initScroll();

})();