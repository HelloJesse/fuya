mini.parse();

//loadDropDownData([["FlyCheckFlag", "FlyCheckFlag"]]);
Date.prototype.format = function (format) {
    var date = {
        "M+": this.getMonth() + 1,
        "d+": this.getDate(),
        "h+": this.getHours(),
        "m+": this.getMinutes(),
        "s+": this.getSeconds(),
        "q+": Math.floor((this.getMonth() + 3) / 3),
        "S+": this.getMilliseconds()
    };
    if (/(y+)/i.test(format)) {
        format = format.replace(RegExp.$1, (this.getFullYear() + '').substr(4 - RegExp.$1.length));
    }
    for (var k in date) {
        if (new RegExp("(" + k + ")").test(format)) {
            format = format.replace(RegExp.$1, RegExp.$1.length == 1
                   ? date[k] : ("00" + date[k]).substr(("" + date[k]).length));
        }
    }
    return format;
}

//回显
function SetData(data) {
    if (data) {
        data = mini.clone(data);
        var ship = data.ship.data;
        mini.get("#name").setValue(ship.name);
        mini.get("#lat").setValue(ship.lat);
        mini.get("#lng").setValue(ship.lng);
        mini.get("#MMSI").setValue(data.ship.id);
        mini.get("#heading").setValue(ship.heading + "度");
        mini.get("#IMO").setValue(ship.imo);
        mini.get("#course").setValue(ship.course + "度");
        mini.get("#speed").setValue(ship.speed);

        if (ship.lastTime != null && ship.lastTime != "") {

            var timestamp3 = ship.lastTime;
            var newDate = new Date();
            newDate.setTime(timestamp3 * 1000);

            mini.get("#lastTime").setValue(newDate.format('yyyy-MM-dd h:m:s'));
        }
    }
}

//取消
function onCancel(e) {
    CloseWindow("cancel");
}

function CloseWindow(action) {
    if (window.CloseOwnerWindow) return window.CloseOwnerWindow(action);
    else window.close();
}








