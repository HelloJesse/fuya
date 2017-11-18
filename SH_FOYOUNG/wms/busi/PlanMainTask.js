mini.parse();
var _GrdListFly = mini.get("grdListFly");
var ids = mini.get("#IDS");
var form = new mini.Form("#form1");

_GrdListFly.on("beforeload", function (e) {
    e.data.IDS = ids.getValue();
})

var _BaseStationID = "";

//回显已勾选的任务
function SetData(data) {
    if (data) {
        data = mini.clone(data);
        _BaseStationID = data.BaseStationID;
        SystemSearch();//调用查询方法
    }
}

//默认调用查询方法
function SystemSearch() {
    ids.setValue("");
    //var data = form.getData(true);
    //条件过滤 Status:0 正常待飞，或已安排任务的 
    //条件配置 or
    var data;
    if (_BaseStationID == "" || _BaseStationID == null) {
        data = { Status: 0 };
    } else {
        data = { BaseStationID: _BaseStationID };
    }
    _GrdListFly.load(data, function (e) {
        //回显数据
        if (_BaseStationID == "" || _BaseStationID == null) {
            return;
        }
        setSelected(_BaseStationID);
    });
}

//设置排序
var setSortFlag = false;
_GrdListFly.set({
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

function setSelected() {
    var rows = _GrdListFly.findRows(function (row) {
        if (row.ID == _BaseStationID) return true;
        else return false
    });
    _GrdListFly.selects(rows);
}

//取消
function onCancel(e) {
    CloseWindow("cancel");
}

function CloseWindow(action) {
    if (window.CloseOwnerWindow) return window.CloseOwnerWindow(action);
    else window.close();
}

//保存任务
function btnOK() {

    //获取已勾选的基站和飞机ID
    var row = _GrdListFly.getSelected();
    if (row == null || row.length < 1) {
        notify(_Mlen_73);
        return;
    }

    var data = { BaseStationID: row["ID"], UavSetID: row["UavID"] };
    data = mini.encode(data);   //序列化成JSON
    CloseWindow(data);
}










