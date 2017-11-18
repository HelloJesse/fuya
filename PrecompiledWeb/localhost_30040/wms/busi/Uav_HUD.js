mini.parse();
var _SetTimeFlyPanel = null;//飞机遥测信息间隔时间

//加载参数
loadDropDownData([["", "SetTimeOut"]], function () {
    var timeOut = getCacheJsObject("SetTimeOut");
    if (timeOut.length > 0) {
        for (var i = 0; i < timeOut.length; i++) {
            if (timeOut[i].ID == 3) { //(定时器时间)
                _SetTimeFlyPanel = parseInt(timeOut[i].NAME);

                //默认调用查询方法
                SearchFlyPanel();
            }
        }
    }
});

function SearchFlyPanel() {
    window.setInterval("GetNavFlyPanel()", _SetTimeFlyPanel);//每_SetTimeFlyPanel秒执行一次
}

function GetNavFlyPanel() {
    $.ajax({
        url: _Datapath + "Busi/NavHUDCode.aspx?method=onGetNavFlyPanel",
        type: "post",
        async: false,//同步执行
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                //注意角度不能是小数，否则不显示
                flyobject.SettingFly(Math.round(data.high), Math.round(data.speed), Math.round(data.balance),
                    Math.round(data.angle), Math.round(data.direction), data.lat, data.lng);

            } else {
                if (isLoginOut(data) == false) {
                    notify(_Mlen_6 + data.ErrMessage);
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify(_Mlen_6 + jqXHR.responseText);
        }
    });
}






















