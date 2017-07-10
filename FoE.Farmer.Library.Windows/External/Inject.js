
if (document.readyState == "complete") FoEInit();
else {
    document.addEventListener("DOMContentLoaded",
        () => {
            FoEInit();
        });
}

function FoEInit() {
    if (document.location.pathname == "/") FoELogin();
    else if (document.location.pathname == "/page/") FoEPlay();
}

function FoELogin() {
    document.getElementById("login_userid").value = FoELoginUserName;
    document.getElementById("login_userid").dispatchEvent(new CustomEvent("input"));
    document.getElementById("login_password").value = FoELoginPassword;
    document.getElementById("login_password").dispatchEvent(new CustomEvent("input"));
    //alert(FoELoginUserName + "_" + FoELoginPassword);
    FoFTimer(500).then(() => {
        document.getElementById("login_Login").click();
    });
}

function FoEPlay() {
    if (document.querySelector("#sidebar .widget.login_glps") != null) {
        document.querySelector("#sidebar .widget.login_glps").contentDocument.querySelector("#login_userid").value = FoELoginUserName;
        document.querySelector("#sidebar .widget.login_glps").contentDocument.querySelector("#login_password").value = FoELoginPassword;
        document.querySelector("#sidebar .widget.login_glps").contentDocument.querySelector("#login_Login").click();
        
        return;
    }

    document.querySelector("#play_now_button").click();
    setTimeout(() => {
            var el = void 0;
            if (FoEWordName)
                el = Array.from(document.querySelectorAll("#world_selection_content .world_select_button")).find(item => item.textContent == FoEWordName);
            else {
                var d = Array.from(document.querySelectorAll("#world_selection_content .world_select_button"));
                if (d.length > 0) el = d[0];
            }

            if (el) el.click();
        },
        1000);
}

function FoFTimer(time) {
    if (time == void 0) time = 500;
    return new Promise((res) => {
        setTimeout(() => { res(); }, time);
    });
}


function simulateKeyDown(element, key) {
    var keyboardEvent = document.createEvent("KeyboardEvent");
    var initMethod = typeof keyboardEvent.initKeyboardEvent !== 'undefined' ? "initKeyboardEvent" : "initKeyEvent";


    keyboardEvent[initMethod](
        "keydown", // event type : keydown, keyup, keypress
        true, // bubbles
        true, // cancelable
        window, // viewArg: should be window
        false, // ctrlKeyArg
        false, // altKeyArg
        false, // shiftKeyArg
        false, // metaKeyArg
        key.codePointAt(0), // keyCodeArg : unsigned long the virtual key code, else 0
        0 // charCodeArgs : unsigned long the Unicode character associated with the depressed key, else 0
    );
    element.dispatchEvent(keyboardEvent);
}

function simulateKeyUp(element, key) {
    var keyboardEvent = document.createEvent("KeyboardEvent");
    var initMethod = typeof keyboardEvent.initKeyboardEvent !== 'undefined' ? "initKeyboardEvent" : "initKeyEvent";


    keyboardEvent[initMethod](
        "keydown", // event type : keydown, keyup, keypress
        true, // bubbles
        true, // cancelable
        window, // viewArg: should be window
        false, // ctrlKeyArg
        false, // altKeyArg
        false, // shiftKeyArg
        false, // metaKeyArg
        key.codePointAt(0), // keyCodeArg : unsigned long the virtual key code, else 0
        0 // charCodeArgs : unsigned long the Unicode character associated with the depressed key, else 0
    );
    element.dispatchEvent(keyboardEvent);
}

function simulateKeyPress(element, key) {
    var keyboardEvent = document.createEvent("KeyboardEvent");
    var initMethod = typeof keyboardEvent.initKeyboardEvent !== 'undefined' ? "initKeyboardEvent" : "initKeyEvent";


    keyboardEvent[initMethod](
        "keydown", // event type : keydown, keyup, keypress
        true, // bubbles
        true, // cancelable
        window, // viewArg: should be window
        false, // ctrlKeyArg
        false, // altKeyArg
        false, // shiftKeyArg
        false, // metaKeyArg
        key.codePointAt(0), // keyCodeArg : unsigned long the virtual key code, else 0
        0 // charCodeArgs : unsigned long the Unicode character associated with the depressed key, else 0
    );
    element.dispatchEvent(keyboardEvent);
}

function simulateKey(element, key) {
    simulateKeyDown(element, key);
    return FoFTimer(10).then(() => {
        simulateKeyPress(element, key);
    }).then(() => FoFTimer(15)).then(() => {
        simulateKeyUp(element, key);
    });
}

async function simulateKeyText(element, text) {
    simulateMouse(element, "mousedown");
    element.click();
    element.focus();
    simulateMouse(element, "mouseup");

    for (var char of text) {
        await FoFTimer(10);
        await simulateKey(element, char);
        if (element.value) element.value += char;
    }
}

function simulateMouse(element, eventName) {
    var options = extend(defaultOptions, arguments[2] || {});
    var oEvent, eventType = null;

    for (var name in eventMatchers) {
        if (eventMatchers[name].test(eventName)) { eventType = name; break; }
    }

    if (!eventType)
        throw new SyntaxError('Only HTMLEvents and MouseEvents interfaces are supported');

    if (document.createEvent) {
        oEvent = document.createEvent(eventType);
        if (eventType == 'HTMLEvents') {
            oEvent.initEvent(eventName, options.bubbles, options.cancelable);
        }
        else {
            oEvent.initMouseEvent(eventName, options.bubbles, options.cancelable, document.defaultView,
                options.button, options.pointerX, options.pointerY, options.pointerX, options.pointerY,
                options.ctrlKey, options.altKey, options.shiftKey, options.metaKey, options.button, element);
        }
        element.dispatchEvent(oEvent);
    }
    else {
        options.clientX = options.pointerX;
        options.clientY = options.pointerY;
        var evt = document.createEventObject();
        oEvent = extend(evt, options);
        element.fireEvent('on' + eventName, oEvent);
    }
    return element;
}

function extend(destination, source) {
    for (var property in source)
        destination[property] = source[property];
    return destination;
}

var eventMatchers = {
    'HTMLEvents': /^(?:load|unload|abort|error|select|change|submit|reset|focus|blur|resize|scroll)$/,
    'MouseEvents': /^(?:click|dblclick|mouse(?:down|up|over|move|out))$/
}
var defaultOptions = {
    pointerX: 0,
    pointerY: 0,
    button: 0,
    ctrlKey: false,
    altKey: false,
    shiftKey: false,
    metaKey: false,
    bubbles: true,
    cancelable: true
}