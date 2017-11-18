mini.parse();

//
function onCheckLine() {
    var cLine = mini.get("#cLine").getValue();
    var cPoint = mini.get("#cPoint").getValue();

    if (cLine == null || cPoint == null) {
         return;
    }
    bodyLoading();
    $.ajax({
        url: _Datapath + "Busi/NavChangeLineCode.aspx?method=updateCheckLine",
        data: { cLine: cLine, cPoint: cPoint, TaskID: "", UavID: ""},
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                showTips(_Mlen_5);
            } else {
                if (isLoginOut(data) == false) {
                    notify(_Mlen_6 + data.ErrMessage);
                }
            }
            mini.unmask();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify(_Mlen_6 + jqXHR.responseText);
            mini.unmask();
        }
    });
}

function onControlCycle() {
    var lng = mini.get("#lng").getValue();
    var lat = mini.get("#lat").getValue();
    var num = mini.get("#num").getValue();

    if (lng == null || lat == null) {
        return;
    }
    bodyLoading();
    $.ajax({
        url: _Datapath + "Busi/NavChangeLineCode.aspx?method=updateControlCycle",
        data: { lng: lng, lat: lat, num: num },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                showTips(_Mlen_5);
            } else {
                if (isLoginOut(data) == false) {
                    notify(_Mlen_6 + data.ErrMessage);
                }
            }
            mini.unmask();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify(_Mlen_6 + jqXHR.responseText);
            mini.unmask();
        }
    });
}

