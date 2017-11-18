//工具类，提供一些跨浏览器解决方案和工具方法
(function () {
    //结尾补零
    var fixed = function (input, length, chart) {
        var ret = input.slice(0, length);
        for (var i = 0; i < length - ret.length; i++) {
            ret = chart + ret;
        }
        return ret;
    };
    myApp.util = {
        //通过ID获取元素
        getElement: function (id, parent) {
            if (document.querySelector) {
                this.getElement = function (id, parent) {
                    parent = parent || document;
                    return parent.querySelector('#' + id);
                }
            } else {
                this.getElement = function (id) {
                    return document.getElementById(id);
                }
            }
            return this.getElement(id, parent);
        },
        //元素注册事件
        addEvent: function (target, type, handler) {
            if (target.addEventListener) {//DOM2规范
                this.addEvent = function (target, type, handler) {
                    target.addEventListener(type, handler, false);
                }
            } else if (target.attachEvent) {//IE
                this.addEvent = function (target, type, handler) {
                    target['e' + type + handler] = handler;
                    target[type + handler] = function () {//以使this对象指向target，否则会指向window全局对象
                        var e = window.event;
                        if (!e.target) {
                            e.target = e.srcElement; //给IE的事件模型加入target属性
                        }
                        target['e' + type + handler](e); //将event对象作为参数传递给处理函数
                    }
                    target.attachEvent('on' + type, target[type + handler]);
                }
            } else {//原始方式
                this.addEvent = function (target, type, handler) {
                    target['on' + type] = handler;
                }
            }
            this.addEvent(target, type, handler);
        },
        //元素移除事件
        removeEvent: function (target, type, handler) {
            if (target.removeEventListener) {//DOM2规范
                this.removeEvent = function (target, type, handler) {
                    target.removeEventListener(type, handler, false);
                }
            } else if (target.detachEvent) {//IE
                this.removeEvent = function (target, type, handler) {
                    target.detachEvent('on' + type, target[type + handler]);
                    target['e' + type + handler] = null;
                    target[type + handler] = null;
                    delete target['e' + type + handler];
                    delete target[type + handler];
                }
            } else {
                this.removeEvent = function (target, type, handler) {
                    target['on' + type] = null;
                }
            }
            this.removeEvent(target, type, handler);
        },
        //阻止冒泡
        stopPropagation: function (event) {
            if (event.stopPropagation) {
                this.stopPropagation = function (event) {
                    event.stopPropagation();
                }
            } else {
                this.stopPropagation = function (event) {
                    event.cancelBubble = true;
                }
            }
            this.stopPropagation(event);
        },
        //阻止默认行为
        preventDefault: function (event) {
            if (event.preventDefault) {
                this.preventDefault = function (event) {
                    event.preventDefault();
                }
            } else {
                this.preventDefault = function (event) {
                    event.returnValue = false;
                }
            }
            this.preventDefault(event);
        },
        //UTC时间格式化为YYYY-MM-DD HH:MM:SS
        formatTime: function (time, len) {
            var t = new Date(time * 1000);
            t = this.formatDateToString(t);
            if (len) { t = t.substr(0, len); }
            return t;
        },
        //日期格式化为YYYY-MM-DD HH:MM:SS
        formatDateToString: function (date) {
            if (!(date instanceof Date)) return '';
            var m = date.getMonth() + 1, d = date.getDate(), H = date.getHours(), M = date.getMinutes(), S = date.getSeconds();
            if (m < 10) { m = '0' + m; }
            if (d < 10) { d = '0' + d; }
            if (H < 10) { H = '0' + H; }
            if (M < 10) { M = '0' + M; }
            if (S < 10) { S = '0' + S; }
            return [date.getFullYear(), '-', m, '-', d, ' ', H, ':', M, ':', S].join('');
        },
        //日期格式化为YYYY年MM月DD日 HH:MM:SS
        formatDateToCNString: function (date) {
            if (!(date instanceof Date)) return '';
            var m = date.getMonth() + 1, d = date.getDate(), H = date.getHours(), M = date.getMinutes(), S = date.getSeconds();
            if (m < 10) { m = '0' + m; }
            if (d < 10) { d = '0' + d; }
            if (H < 10) { H = '0' + H; }
            if (M < 10) { M = '0' + M; }
            if (S < 10) { S = '0' + S; }
            return [date.getFullYear(), '年', m, '月', d, '日 ', H, ':', M, ':', S].join('');
        },
        //将年月日、时、分 转换为Date对象
        getDate: function (date, hour, minute) {
            var ds = date.split('-');
            var eYear = parseInt(ds[0], 10),
                eMonth = parseInt(ds[1], 10) - 1,
                eDay = parseInt(ds[2], 10),
                eHour = parseInt(hour, 10),
                eMinute = parseInt(minute, 10);
            return new Date(eYear, eMonth, eDay, eHour, eMinute, 00);
        },
        //YYYY-MM-DD HH:MM:SS转化为Date对象
        getDateByString: function (dateString) {
            return new Date(parseInt(dateString.substr(0, 4), 10),
                parseInt(dateString.substr(5, 2), 10) - 1,
                parseInt(dateString.substr(8, 2), 10),
                parseInt(dateString.substr(11, 2), 10),
                parseInt(dateString.substr(14, 2), 10),
                parseInt(dateString.substr(17, 2), 10));
        },
        //纬度格式化为度分：xx-xx.xxxN/S
        formatLat: function (lat) {
            var latAbs = Math.abs(lat);
            if (latAbs > 90) { return ''; }
            var preNum = Math.floor(latAbs), lastNum = (latAbs - preNum) * 60;
            var ret = preNum + '-' + fixed(lastNum.toFixed(3), 6, '0');
            return lat >= 0 ? ret + 'N' : ret + 'S';
        },
        //经度格式化为度分：xx-xx.xxxE/W
        formatLng: function (lng) {
            var lngAbs = Math.abs(lng);
            if (lngAbs > 180) { return ''; }
            var preNum = Math.floor(lngAbs), lastNum = (lngAbs - preNum) * 60;
            var ret = preNum + '-' + fixed(lastNum.toFixed(3), 6, '0');
            return lng >= 0 ? ret + 'E' : ret + 'W';
        }
    }
})();