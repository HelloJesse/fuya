mini.parse();
var _Languagefolder = getLanguageFolder();
var ids = mini.get("#IDS");
var _GrdList = mini.get("grdList");

_GrdList.on("beforeload", function (e) {
    e.data.IDS = ids.getValue();
})

setAutoCompleteData("BaseStationID", "D_Uav_BaseStation");
setAutoCompleteData("UavSetID", "D_Uav_UavSet");

//绑定表单
var db = new mini.DataBinding();
db.bindForm("editForm1", _GrdList);

var editForm1 = new mini.Form("#editForm1");//用于清空表单

var params = GetParams(location.href, "?");//getCacheArgs();
var _TaskSource = params.TaskSource;//0：主控中心 1：基站  2：客户
mini.get("#TaskSource").setValue(_TaskSource);
var _FormDataMini;

//0：主控中心, 可以查询任务状态为： 0 已提交 1 已接受 3已安排的任务
//1：基站 可以查询 该基站下的所有状态任务清单
//2：客户 可以查询 该客户下的所有状态任务清单 
if (_TaskSource == '0') {
    _FormDataMini = { TaskStatus: "0,1,3" };

} else if (_TaskSource == '1') {
    _FormDataMini = { TaskSource: _TaskSource, TaskStatus: "0,1,2,3,4" };
    $("#otherID").removeAttr("style");
    $("#mainID").attr("style", "display:none");

} else if (_TaskSource == '2') {
    _FormDataMini = { TaskSource: _TaskSource, TaskStatus: "0,1,2,3,4" };
    $("#otherID").removeAttr("style");
    $("#mainID").attr("style", "display:none");

}


var _SetTimeGrid;//用来设置主控任务列表定时器时间
loadDropDownData([["FlyWay", "FlyWay"], ["Status", "UavStatus"], ["", "SetTimeOut"]], function () {
    var timeOut = getCacheJsObject("SetTimeOut");
    if (timeOut.length > 0) {
        for (var i = 0; i < timeOut.length; i++) {
            if (timeOut[i].ID == 1) { //主控中心任务列表定时器时间)
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

//默认调用查询方法
function SystemSearch() {
    ids.setValue("");
    _GrdList.load(_FormDataMini);
    editForm1.clear();//查询后，清空表单
}

//设置排序
var setSortFlag = false;
_GrdList.set({
    onbeforeload: function (e) {
        var grd = e.sender;
        if (e.data.pageIndex == grd.pageIndex) {
            e.data.SortFlag = 1;
        } else {
            e.data.SortFlag = 0;
        }
    },
    onload: function (e) {
        var grd = e.sender;
        var result = grd.getResultObject();
        if (isLoginOut(result) == false) {
            if (result.errMessage > '') {
                notify(result.errMessage);
            }
            ids.setValue(result["Ids"]);
        }
        if (setSortFlag == false) {
            setSortFlag = true;
            var sorter = new MultiSort(grd);
        }
    }
});

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

//接受任务--修改任务状态 flag = 1
//任务完成--修改任务状态 flag = 4
function onAcceptTask(flag) {
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
                url: _Datapath + "Busi/DUavMainTaskCode.aspx?method=UpdateTaskStatus",
                data: { BILLID: billids, TaskStatus: flag, Reason: ''},
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

//拒绝接受任务
function onNoAcceptTask(flag) {
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
    var reason ="";
    mini.prompt(_Mlen_72, _Mlen_26,
        function (action, value) {
            if (action == "ok") {
                reason = value;

                bodyLoading();
                $.ajax({
                    url: _Datapath + "Busi/DUavMainTaskCode.aspx?method=UpdateTaskStatus",
                    data: { BILLID: billids, TaskStatus: flag, Reason: reason },
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
            } else {
                return;
            }
        },true
    );
}

//安排任务
function onPlanTask() {
    var row = _GrdList.getSelecteds();
    if (row.length <= 0) {
        notify(_Mlen_8);
        return;
    }
    if (row.length > 1) {
        notify(_Mlen_71);
        return;
    }
    var billid = row[0]["BILLID"];
    var baseStationID = row[0]["BaseStationID"];//基站ID 用于回显 已勾选的基站
    var taskStatus = row[0]["TaskStatus"];
    var flyWay = row[0]["FlyWay"];//0：船舶跟踪 1：巡航任务
    var lineHeight = row[0]["LineHeight"];//航线高度

    if (taskStatus != 1 && taskStatus != 3) {//不是已接受和已安排任务，不允许点击
        notify(_Mlen_74);
        return;
    }

    mini.open({
        url: _Rootpath + "wms/busi/PlanMainTask.html",
        title: "调度安排", width: 830, height: 260,
        onload: function () {
            var iframe = this.getIFrameEl();
            var data = { BaseStationID: baseStationID };
            iframe.contentWindow.SetData(data);
        },
        ondestroy: function (data) {
            if (data) {
                if (data == "cancel" || data == "close") {//取消直接返回
                    return;
                }
                data = mini.decode(data);
                var BaseStationID = data.BaseStationID;
                var UavSetID = data.UavSetID;
                bodyLoading();     //加载遮罩
                $.ajax({
                    url: _Datapath + "Busi/DUavMainTaskCode.aspx?method=PlanMainTask",
                    data: { BILLID: billid, BaseStationID: BaseStationID, UavSetID: UavSetID, FlyWay: flyWay, LineHeight: lineHeight },
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


//初始化按钮状态
setButtonEnabled(["#btnOverOption", "#btnReset", "#btnStop"], false);
setButtonEnabled(["#btnAddOption"], true);

//选择基站加载，当前基站的经纬度
//function onBaseStationIDChanged(e) {

//    var BaseStationID = mini.get("#BaseStationID").getValue();
//    if (BaseStationID == null || "" == BaseStationID) {
//        return;
//    }
//    $.ajax({
//        url: _Datapath + "Busi/DUavMainTaskCode.aspx?method=GetBaseStationInfo",
//        data: { BaseStationID: BaseStationID },
//        type: "post",
//        success: function (text) {
//            var data = mini.decode(text);
//            if (data.IsOK) {
//                if (data.DataList.length == 1) {
//                    mini.get("#Latitude").setValue(data.DataList[0]["Latitude"]);
//                    mini.get("#Longitude").setValue(data.DataList[0]["Longitude"]);
//                }
//            } else {
//                if (isLoginOut(data) == false) {
//                    notify(_Mlen_6 + data.ErrMessage);
//                }
//            }
//        },
//        error: function (jqXHR, textStatus, errorThrown) {
//            notify(_Mlen_6 + jqXHR.responseText);
//        }
//    });
//}