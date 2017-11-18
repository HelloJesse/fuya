function GetParams(url, c) {
    if (!url) url = location.href;
    if (!c) c = "?";
    url = url.split(c)[1];
    var params = {};
    if (url) {
        var us = url.split("&");
        for (var i = 0, l = us.length; i < l; i++) {
            var ps = us[i].split("=");
            params[ps[0]] = decodeURIComponent(ps[1]);
        }
    }
    return params;
}

function onIFrameLoad() {
    if (!CanSet) return;
    var mainTabs = mini.get("mainTabs");
    if (mainTabs) {
        mainTabs.setActiveIndex(0);
    }
    //url#src=...html
    var iframe = document.getElementById("mainframe");
    var src = "";
    try {
        var url = iframe.contentWindow.location.href;
        var ss = url.split("/");
        var s1 = ss[ss.length - 2];
        if (s1 != "demo") {
            src = s1 + "/" + ss[ss.length - 1];
        } else {
            src = ss[ss.length - 1];
        }
    } catch (e) {
    }
    if (src && src != "overview.html") {

        window.location.hash = "src=" + src;

    }
}
function onTabsActiveChanged(e) {
    if (this.activeIndex == 1) {
        var url = document.getElementById("mainframe").contentWindow.location.href;
        var codeframe = document.getElementById("codeframe");
        codeframe.src = "runCode/codeview.html?url=" + url;
    }
        var tabs = e.sender;
        var tab = tabs.getActiveTab();
        if (tab && tab._nodeid) {

            var node = tree.getNode(tab._nodeid);
            if (node) {
                tree.selectNode(node);
            }// && !tree.isSelectedNode(node)
        }
}

function onSkinChange(skin) {
    mini.Cookie.set('miniuiSkin', skin);
    //mini.Cookie.set('miniuiSkin', skin, 100);//100天过期的话，可以保持皮肤切换
    window.location.reload()
}
function AddCSSLink(id, url, doc) {
    doc = doc || document;
    var link = doc.createElement("link");
    link.id = id;
    link.setAttribute("rel", "stylesheet");
    link.setAttribute("type", "text/css");
    link.setAttribute("href", url);

    var heads = doc.getElementsByTagName("head");
    if (heads.length)
        heads[0].appendChild(link);
    else
        doc.documentElement.appendChild(link);
}

