myApp.XWin = (function () {
    var xwin = function () { this.init.apply(this, arguments) };
    xwin.prototype = {
        lock: true,
        modal: true,
        resizable: false,
        dragbody: false,
        animate: false,
        offset: false,
        alpha: 20,
        width: 600,
        minWidth: 100,
        minHeight: 20,
        position: 'center',
        ie6: /IE 6/.test(navigator.userAgent),
        isie: /MSIE/.test(navigator.userAgent),
        dragfilter: /^a|input|select|textarea|button|img|label$/i,
        init: function (p) {
            var _ = this, c, r, m = document.createElement('div'), o = m.cloneNode(0);
            for (var k in p) _[k] = p[k];
            o.innerHTML = (_.ie6 ? '<iframe frameborder="0" class="xwin_full xwin_alpha"></iframe>' : '')
            + '<table cellpadding="0" cellspacing="0" class="xwin_tb">'
            + '<tr><td class="xwin_tl xwin_e_nw"></td><td class="xwin_tm xwin_e_n"></td><td class="xwin_tr xwin_e_ne"></td></tr>'
            + '<tr><td class="xwin_hl xwin_e_w"></td><td class="xwin_hm xwin_e_move"><div class="xwin_title"></div></td><td class="xwin_hr xwin_e_e"></td></tr>'
            + '<tr><td class="xwin_ml xwin_e_w"></td><td class="xwin_mm">'
            + '<div class="xwin_body"><div class="xwin_content"></div><div class="xwin_full xwin_alpha" style="display:none"></div></div>'
            + '</td><td class="xwin_mr xwin_e_e"></td></tr>'
            + '<tr><td class="xwin_bl xwin_e_sw"></td><td class="xwin_bm xwin_e_s"></td><td class="xwin_br xwin_e_se"></td></tr>'
            + '</table>' + (_.resizebar ? '<div class="xwin_resizer xwin_e_se"></div>' : '') + '<a href="" class="xwin_close">[关闭]</a>';
            c = _.container || document.body;
            if (_.ie6) {
                r = document.createElement('iframe');
                r.style.display = 'none';
                r.setAttribute('frameborder', '0');
                r.className = 'xwin_full xwin_alpha';
                r.style.display = 'none';
                c.appendChild(r);
            }
            m.style.display = 'none';
            m.className = 'xwin_mask';
            c.appendChild(m);
            c.appendChild(o);
            c = o.getElementsByTagName('tr');
            _.o = o;
            _.el = {};
            _.el.tr = c;
            _.el.mask = m;
            _.el.mfrm = r || m;
            _.el.title = c[1].childNodes[1].childNodes[0];
            _.el.body = c[2].childNodes[1].childNodes[0];
            _.el.content = _.el.body.childNodes[0];
            _.el.lock = _.el.content.nextSibling;
            _.el.close = o.childNodes[o.childNodes.length - 1];
            if (_.ie6) _.el.iframe = o.childNodes[0];
            _.setParam(_);
            _.o.style.display = 'none';
            _.el.close.onclick = function () { _.hide(); return false };
            _.ae(window, 'resize', function () { _.resize() });
            _.ae(o, 'mousedown', function (e) { _.down(e) });
            _.ae(document, 'mouseup', function (e) { _.up() });
            _.ae(document, 'mousemove', function (e) { _.move(e) });
            _.ae(_.el.title, 'dblclick', function () { _.swap() });
            _.ae(m, 'click', function () { if (_.maskcolse) _.hide() });
        },
        ae: function (o, e, f) { if (window.attachEvent) { o.attachEvent('on' + e, f) } else { o.addEventListener(e, f, false) } },
        fe: function (e) { e = e || event; var b = document.body, d = document.documentElement; return { x: e.pageX || (e.clientX + Math.max(b.scrollLeft, d.scrollLeft)), y: e.pageY || (e.clientY + Math.max(b.scrollTop, d.scrollTop))} },
        fo: function (o) { var x = 0, y = 0; while (o) { x += o.offsetLeft; y += o.offsetTop; o = o.offsetParent } return { x: x, y: y} },
        setParam: function (p) {
            if (!p) return;
            var _ = this;
            if (p.fixed != undefined) { if (_.fixed) _.resizable = false }
            if (p.resizable != undefined) { _.o.className = 'xwin' + (_.className ? (' ' + _.className) : '') + (p.resizable ? ' xwin_rz' : '') }
            if (p.dragbody != undefined) { _.el.body.parentNode.className = p.dragbody ? 'xwin_mm xwin_e_move' : 'xwin_mm' }
            if (p.hideclose || p.hidehead || p.noborder) { _.el.close.style.display = 'none' }
            if (p.hidehead) { _.offset = false; var c = _.el.tr; c[0].childNodes[0].style.height = c[0].offsetHeight + 1 + 'px'; c[1].style.display = 'none' }
            if (p.noborder) { _.offset = false; var c = _.el.tr; c[0].style.display = c[1].style.display = c[3].style.display = c[2].childNodes[0].style.display = c[2].childNodes[2].style.display = 'none' }
            if (_.offset === false) { var c = _.el.tr; _.offset = { x: c[0].childNodes[0].offsetWidth * 2, y: c[0].offsetHeight * 2 + c[1].offsetHeight} }
            if (p.title != undefined) _.setTitle(p.title);
            if (p.content != undefined) _.setContent(p.content);
            if (p.src != undefined) _.setSrc(p.src);
            if (p.bind != undefined) { var c = _.el.content, a = c.childNodes; if (a.length == 1) { a[0].style.display = 'none' } else { c.innerHTML = '' }; c.appendChild(p.bind); p.bind.style.display = '' }
            if (p.width != undefined) { _.o.style.width = p.width + _.offset.x + 'px'; _.el.body.style.width = p.width + 'px' }
            if (p.height != undefined) { _.el.body.style.height = _.el.content.style.height = p.height + 'px'; p.hidden = _.hidden = false }
            if (p.hidden != undefined) { _.el.content.style.overflow = p.hidden ? 'hidden' : 'auto' }
            if (p.alpha != undefined || p.xalpha != undefined) { var n = p.alpha || p.xalpha; if (_.isie) _.el.mask.style.filter = 'alpha(opacity=' + n + ')'; else _.el.mask.style.opacity = n / 100 }
            if (p.background != undefined) { _.el.body.style.background = p.background }
        },
        setPosition: function (a) {
            var _ = this, o = _.o;
            if (typeof a != 'object') a = [(_.box.w - o.offsetWidth) / 2, (_.box.h - o.offsetHeight) / 2];
            if (a[1] < 0) a[1] = 0;
            o.style.left = a[0] + _.box.x + 'px';
            o.style.top = a[1] + _.box.y + 'px';
        },
        interval: function (f) { var _ = this, n = 0; clearInterval(_.mo); _.mo = setInterval(function () { n += (this.isie ? 0.35 : 0.15); if (n >= 1) { clearInterval(_.mo); f(1, 1); return; } f(n); }, 10) },
        setZIndex: function () { var _ = this; if (!xwin.zIndex) xwin.zIndex = 1000; _.o.style.zIndex = _.el.mask.style.zIndex = _.el.mfrm.style.zIndex = ++xwin.zIndex },
        setTitle: function (s) { this.el.title.innerHTML = s || '' },
        setContent: function (s) { var _ = this; _.el.content.innerHTML = s || '' },
        setSrc: function (s) { var _ = this; _.el.content.innerHTML = '<iframe frameborder="0" class="xwin_full"></iframe>'; setTimeout(function () { _.el.content.childNodes[0].src = s }) },
        hackie6: function () { var _ = this; _.el.iframe.style.height = _.el.body.offsetHeight + _.offset.y + 'px'; },
        ishide: function () { return this.o.style.display == 'none'; },
        show: function (p) {
            var _ = this, c, b = _.modal && _.animate;
            _.setZIndex();
            _.resize(true);
            _.o.style.display = '';
            if (p) { _.setParam(p) } else { p = {} }
            if (_.ie6) _.hackie6();
            if (!_.isinit && !p.position) { _.isinit = true; p.position = _.position; if (_.oninit) _.oninit() };
            if (_.modal && !p.position) p.position = 'center';
            if (p.position) _.setPosition(p.position);
            if (b) _.setParam({ xalpha: 0 }, true);
            _.el.mask.style.display = _.el.mfrm.style.display = _.modal ? '' : 'none';
            if (_.isie) window.focus();
            if (!b && _.onshow) _.onshow();
            if (!b) return;
            p = _.alpha;
            _.o.style.display = 'none';
            _.interval(function (n, z) { _.setParam({ xalpha: p * n }, true); if (z) { _.o.style.display = ''; if (_.onshow) _.onshow() } });
        },
        hide: function (b) {
            var _ = this;
            if (!b && _.onhide && _.onhide() === false) return;
            if (!_.modal || !_.animate) { _.hider(); return }
            b = _.alpha;
            _.interval(function (n, z) { _.setParam({ xalpha: b * (1 - n) }); if (z) { _.hider(); _.setParam({ xalpha: b }); } });
        },
        hider: function () { var _ = this; if (_.modal) _.el.mask.style.display = _.el.mfrm.style.display = 'none'; _.o.style.display = 'none'; },
        swap: function (q) {
            if (!this.resizable) return;
            var _ = this, d, t, o = _.o, p = _.max;
            if (p) { _.max = null } else { p = _.box; _.max = { x: o.offsetLeft, y: o.offsetTop, w: o.offsetWidth, h: _.height + _.offset.y} };
            if (q) { p.x = _.q.l = q.x - p.w / 2 };
            o.style.left = p.x + 'px';
            o.style.top = p.y + 'px';
            o.style.width = p.w + 'px';
            o.style.height = p.h ? (p.h + 'px') : 'auto';
            _.el.body.style.width = p.w - _.offset.x + 'px';
            _.el.content.style.height = _.el.body.style.height = p.h ? (p.h - _.offset.y + 'px') : 'auto';
            if (_.ie6) _.el.iframe.style.height = _.el.body.offsetHeight + 'px';
        },
        resize: function (b) {
            if (!b && this.ishide()) return;
            var _ = this, o = _.container, b = document.body, d = document.documentElement, n = Math[_.ie6 ? 'min' : 'max'];
            if (_.modal) _.el.mask.style.display = 'none';
            _.doc = o ? _.fo(o) : { x: 0, y: 0 };
            _.doc.w = o ? o.scrollWidth : Math.max(b.scrollWidth, d.scrollWidth);
            _.doc.h = o ? o.scrollHeight : Math.max(b.scrollHeight, d.scrollHeight);
            _.box = {
                x: o ? o.scrollLeft : Math.max(b.scrollLeft, d.scrollLeft),
                y: o ? o.scrollTop : Math.max(b.scrollTop, d.scrollTop),
                w: o ? o.clientWidth : n(b.clientWidth, d.clientWidth),
                h: o ? o.clientHeight : n(b.clientHeight, d.clientHeight)
            };
            if (_.modal) {
                _.el.mask.style.display = '';
                _.el.mask.style.width = _.el.mfrm.style.width = _.doc.w + 'px';
                _.el.mask.style.height = _.el.mfrm.style.height = _.doc.h + 'px';
            }
            if (_.lock) {
                o = _.o;
                var x = o.offsetLeft, y = o.offsetTop, w = o.offsetWidth, h = o.offsetHeight, m = _.box.x, n = _.box.y;
                if (x < m || x > _.box.w - w + m) o.style.left = _.box.w - w + m + 'px';
                if (y > _.box.h - h + n) o.style.top = _.box.h - h + n + 'px';
                if (y < n) o.style.top = n;
            }
            if (_.onresize) _.onresize();
        },
        down: function (e, o) {
            if (this.fixed) return;
            var _ = this, c, d, e = e || event, o = o || e.target || e.srcElement;
            if (/xwin_close/.test(o.className)) return;
            if (!_.modal) _.setZIndex();
            if (_.dragbody) if (_.dragfilter.test(o.tagName)) return;
            while (o) { if (/xwin_resizer/.test(o.className) || (o.tagName == 'TD' && o.parentNode.parentNode.parentNode.parentNode == _.o)) break; o = o.parentNode };
            if (!o || !/xwin_e_(\w+)/.test(o.className)) return;
            _.moving = RegExp.$1;
            _.resize(true);
            c = _.fe(e);
            d = _.fo(_.o);
            _.q = { x: c.x, y: c.y, l: _.o.offsetLeft, t: _.o.offsetTop, m: c.x - d.x, n: c.y - d.y, w: _.el.body.offsetWidth, h: _.el.body.offsetHeight };
            _.el.lock.style.display = _.el.mask.style.display = '';
            if (!_.modal) _.el.mask.style.display = '';
            document.onselectstart = function () { return false };
            if (e.preventDefault) e.preventDefault();
            e.cancelBubble = true;
            if (c = _.onmousedown) c(e);
            _.move(e, true);
        },
        move: function (e, isdown) {
            if (!this.moving) return;
            var _ = this, b = _.lock, c, x, y, t = _.moving, p = _.fe(e), q = _.q, w = q.w + _.offset.x, h = q.h + _.offset.y;
            c = _.box.x + _.doc.x;
            x = c + (b ? q.m : 0); if (p.x < x) p.x = x;
            x = c + (b ? (q.m + _.box.w - w) : _.box.w); if (p.x > x) p.x = x;
            c = _.box.y + _.doc.y;
            y = c + (b ? q.n : 0); if (p.y < y) p.y = y;
            y = c + (b ? (q.n + _.box.h - h) : _.box.h); if (p.y > y) p.y = y;
            if (t == 'move') {
                if (_.max && !isdown && !_.lock) _.swap(p);
                _.o.style.left = q.l + p.x - q.x + 'px';
                _.o.style.top = q.t + p.y - q.y + 'px';
            } else {
                if (_.resizable) {
                    if (/w|e/.test(t)) {
                        b = /w/.test(t);
                        p.x = (p.x - q.x) * (b ? -1 : 1);
                        if (q.w + p.x < _.minWidth) p.x = _.minWidth - q.w;
                        _.el.body.style.width = q.w + p.x + 'px';
                        _.o.style.width = w + p.x + 'px';
                        if (b) _.o.style.left = q.l - p.x + 'px';
                    }
                    if (/n|s/.test(t)) {
                        b = /n/.test(t);
                        p.y = (p.y - q.y) * (b ? -1 : 1);
                        if (q.h + p.y < _.minHeight) p.y = _.minHeight - q.h;
                        _.el.body.style.height = q.h + p.y + 'px';
                        y = q.h + p.y;
                        _.o.style.height = h + p.y + 'px';
                        if (b) _.o.style.top = q.t - p.y + 'px';
                        if (!_.height) {
                            _.height = y;
                            _.el.content.style.overflow = 'auto';
                        }
                        _.el.content.style.height = y + 'px';
                    }
                    if (_.max) { _.max = null; _.height = _.el.body.offsetHeight }
                }
            }
            _.el.lock.style.height = _.el.body.offsetHeight + 'px';
            if (_.ie6) _.hackie6();
        },
        up: function () {
            var _ = this;
            if (!_.moving) return;
            if (!_.height) _.o.style.height = _.el.body.style.height = 'auto';
            _.moving = null;
            _.el.lock.style.display = 'none';
            if (!_.modal) _.el.mask.style.display = _.el.mfrm.style.display = 'none';
            document.onselectstart = function () { return true };
        }
    };
    return xwin;
})();

//消息框
myApp.Message = {
    loading: function () {
        var w = this.winlog, b = this.win;
        if (b && !b.ishide()) b.hide();
        if (!w) {
            w = new myApp.XWin({
                width: 260,
                height: 100,
                lock: true,
                hidden: true,
                hidehead: true,
                resizable: false,
                content: '<div class="xwin_loading"></div>'
            });
            this.winlog = w;
        }
        w.show();
    },
    getwin: function () {
        var w = this.win, c;
        if (!w) {
            w = new myApp.XWin({
                lock: true,
                modal: true,
                hidden: true,
                dragbody: true,
                resizable: false,
                title: '提示信息',
                content: '<div class="xwin_message"><div class="xwin_box"><div class="xwin_text"></div><div></div></div>'
                + '<div class="xwin_button"><a href="" hidefocus="true"></a><a href="" hidefocus="true"></a></div></div>'
            });
            c = w.el.content.childNodes[0].childNodes;
            w.el.box = c[0];
            w.el.text = c[0].childNodes[0];
            w.el.icon = c[0].childNodes[1];
            w.el.confirm = c[1].childNodes[0];
            w.el.cancel = c[1].childNodes[1];
            w.el.cancel.onclick = function () { w.hide(); return false };
            this.win = w;
        }
        return w;
    },
    show: function (p, q) {
        var w = this.getwin(), p = p || {}, q = q || { width: 350, modal: true }, b = this.winlog;
        if (b && !b.ishide()) b.hide();
        b = p.type == 'confirm';
        if (!b && !p.onconfirm) p.onconfirm = p.onhide;
        w.position = true;
        w.onhide = p.onhide || null;
        w.el.box.style.height = '70px';
        w.el.text.style.paddingTop = '10px';
        w.el.text.innerHTML = p.content;
        w.el.confirm.innerHTML = p.confirm || '确 定';
        w.el.cancel.innerHTML = p.cancel || '取 消';
        w.el.icon.className = 'xwin_icon xwin_' + (p.type || 'alert');
        w.el.confirm.onclick = function () { if (p.onconfirm) if (p.onconfirm() === false) return false; w.hide(1); return false };
        w.el.cancel.style.display = b ? '' : 'none';
        w.o.style.display = '';
        b = w.el.text.offsetHeight;
        if (b > 60) w.el.box.style.height = b + 10 + 'px';
        else w.el.text.style.paddingTop = (90 - b) / 2 + 'px';
        w.o.style.display = 'none';
        w.show(q);
        return w;
    },
    hide: function () { var _ = this; if (_.win) _.win.hide(); if (_.winlog) _.winlog.hide(); },
    alert: function (s, f) { this.show({ type: 'alert', content: s, onhide: f }) },
    success: function (s, f) { this.show({ type: 'success', content: s, onhide: f }) },
    error: function (s, f) { this.show({ type: 'error', content: s, onhide: f }) },
    confirm: function (s, f, x) { this.show({ type: 'confirm', content: s, onconfirm: f, onhide: x }) }
};