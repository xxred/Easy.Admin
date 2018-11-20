window.autoLogin = function () {
    var authWrapper = this.document.querySelector('div.auth-wrapper');
    authWrapper.addEventListener("click", function (event) {
    })

};

window.addEventListener("load", function () {
    if (window.onload) {
        window.onload();
        window.onload = null;
        setTimeout(function () {
            window.autoLogin();
        }, 600);
    }
}); 