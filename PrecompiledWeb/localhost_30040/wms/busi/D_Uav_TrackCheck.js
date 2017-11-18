mini.parse();
var _Languagefolder = getLanguageFolder();
var _GrdList = mini.get("grdList");
var _SetTimeGrid;//用来设置航行检查列表定时器时间

loadDropDownData([["", "SetTimeOut"]], function () {
    var timeOut = getCacheJsObject("SetTimeOut");
    if (timeOut.length > 0) {
        for (var i = 0; i < timeOut.length; i++) {
            if (timeOut[i].ID == 4) { //无人机任务(定时器时间)
                _SetTimeGrid = parseInt(timeOut[i].NAME);

                //默认调用查询方法
                SystemSearch();
                onOpenTimeOut();
            }
        }
    }
});

//开启
var _setId;
function onOpenTimeOut() {
    if (_setId) {
        $("#btnCloseTimeOut").removeAttr("style");
        $("#btnOpenTimeOut").attr("style", "display:none");
    }
    _setId = window.setInterval("SystemSearch()", _SetTimeGrid);//每_SetTimeGrid秒调用一次,系统代码设置
}

function onCloseTimeOut() {
    window.clearInterval(_setId);
    $("#btnOpenTimeOut").removeAttr("style");
    $("#btnCloseTimeOut").attr("style", "display:none");
}

function onDateRenderer(e) {
    var value = e.value;
    if (value) return mini.formatDate(value, 'yyyy-MM-dd');
    return "";
}
function onDateTimeRenderer(e) {
    var value = e.value;
    if (value) return mini.formatDate(value, 'yyyy-MM-dd HH:mm:ss');
    return "";
}
 
//飞机前检查最终确认
function onTrackCheck(flag) {
    
    var row = _GrdList.getSelecteds();
    if (row.length <= 0) {
        notify(_Mlen_8);
        return;
    }
    var billids = "";
    for (var i = 0; i < row.length; i++) {
        billids += row[i]["BILLID"] + ",";
    }
    billids = billids.substring(0, billids.length - 1);

    mini.confirm(_Mlen_10, _Mlen_4,
        function (action) {
            if (action != "ok") {
                return;
            }
            bodyLoading();
            $.ajax({
                url: _Datapath + "Busi/DUavMainTaskCode.aspx?method=UpdateTaskTrackCheck",
                data: { BILLID: billids, TrackCheckType: flag},
                type: "post",
                success: function (text) {
                    var data = mini.decode(text);
                    if (data.IsOK) {
                        showTips(_Mlen_5);
                        SystemSearch();
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
    );
}

//飞前检查
function onFlyCheckFlag() {

    var row = _GrdList.getSelected();
    if (!row) {
        notify(_Mlen_8);
        return;
    }

    mini.open({
        url: _Rootpath + "wms/busi/FlyCheckBefore.html",
        title: "飞前检查", width: 220, height: 360,
        onload: function () {
            var iframe = this.getIFrameEl();
            var data = { CheckFlagStr: row.CheckFlagStr };
            iframe.contentWindow.SetData(data);
        },
        ondestroy: function (data) {
            if (data) {
                if (data == "cancel" || data == "close") {//取消直接返回
                    return;
                }
                data = mini.decode(data);
                var obj = data.obj;
                bodyLoading();     //加载遮罩
                $.ajax({
                    url: _Datapath + "Busi/DUavMainTaskCode.aspx?method=onFlyCheckFlag",
                    data: { BILLID: row.BILLID, Data: obj },
                    type: "post",
                    success: function (text) {
                        var data = mini.decode(text);   //反序列化成对象
                        if (isLoginOut(data) == false) {
                            if (data.IsOK) {
                                showTips(_Mlen_5);
                                //默认调用查询方法
                                SystemSearch();
                            } else {
                                notify(data.ErrMessage);
                            }
                        }
                        mini.unmask(); //取消遮罩
                    },
                    error: function () {
                        mini.unmask(); //取消遮罩
                    }
                });
            }
        }
    });
}

