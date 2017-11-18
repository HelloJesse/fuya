mini.parse();
var _GrdListPlane = mini.get("grdListPlane");
var _GrdListShip = mini.get("grdListShip");


_GrdListPlane.on("beforeload", function (e) {
    e.data.IDS = "";
});

_GrdListShip.on("beforeload", function (e) {
    e.data.IDS = "";
});

var _SetTimerSignal = null;//用来设置接收信号列表定时器时间

//加载参数
loadDropDownData([["", "SetTimeOut"]], function () {
    var timeOut = getCacheJsObject("SetTimeOut");
    if (timeOut.length > 0) {
        for (var i = 0; i < timeOut.length; i++) {
            if (timeOut[i].ID == 8) { //(定时器时间)
                _SetTimerSignal = parseInt(timeOut[i].NAME);

                //默认调用查询方法
                selectPlaneSignal();
            }
        }
    }
});

//开启
function selectPlaneSignal() {
    window.setInterval("SystemSearch()", _SetTimerSignal);//每_SetTimerSignal秒调用一次,系统代码设置
}

//默认调用查询方法
function SystemSearch() {
    _GrdListPlane.load();
    _GrdListShip.load();
}


//设置排序
var setSortFlag = false;
_GrdListPlane.set({
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
            //ids.setValue(result["Ids"]);
        }
        if (setSortFlag == false) {
            setSortFlag = true;
            var sorter = new MultiSort(grd);
        }
    }
});

//设置排序
var setSortFlagShip = false;
_GrdListShip.set({
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
            //idsship.setValue(result["Ids"]);
        }
        if (setSortFlagShip == false) {
            setSortFlagShip = true;
            var sorter = new MultiSort(grd);
        }
    }
});
