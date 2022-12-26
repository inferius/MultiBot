
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

async function FoELogin() {
    document.getElementById("login_userid").value = FoELoginUserName;
    document.getElementById("login_userid").dispatchEvent(new CustomEvent("input"));
    document.getElementById("login_password").value = FoELoginPassword;
    document.getElementById("login_password").dispatchEvent(new CustomEvent("input"));
    //alert(FoELoginUserName + "_" + FoELoginPassword);
    await FoFTimer(500);
    document.getElementById("login_Login").click();
    console.debug("Login button clicked...Waiting for errors.");
    await FoFTimer(5000);
    var errorData = document.querySelector("#login > div > form > div.validation-error > span > span:nth-child(2)");
    responseManager.setError(errorData?.textContent);
}

function FoEPlay() {
    if (document.querySelector("#sidebar .widget.login_glps") != null) {
        document.querySelector("#sidebar .widget.login_glps").contentDocument.querySelector("#login_userid").value = FoELoginUserName;
        document.querySelector("#sidebar .widget.login_glps").contentDocument.querySelector("#login_password").value = FoELoginPassword;
        document.querySelector("#sidebar .widget.login_glps").contentDocument.querySelector("#login_Login").click();
        
        return;
    }

    document.querySelector("#play_now_button").click();
    console.debug("Play button clicked");
    setTimeout(() => {
        console.debug("Searching " + FoEWordName);
        var el = void 0;
            if (FoEWordName) {
                el = Array.from(document.querySelectorAll("#world_selection_content .world_select_button")).find(item => item.textContent == FoEWordName);
                console.debug(`World: '${FoEWordName}' was found`);
            }
            else {
                console.debug(`World: '${FoEWordName}' not found`);
                var d = Array.from(document.querySelectorAll("#world_selection_content .world_select_button"));
                if (d.length > 0) el = d[0];
                console.debug(`World: '${el?.textContent ?? "No world"}' use instead`);
            }

           el?.click();
           console.debug("WorldButton Clicked");
    },
        1000);
}

function FoFTimer(time) {
    if (time == void 0) time = 500;
    return new Promise((res) => {
        setTimeout(() => { res(); }, time);
    });
}