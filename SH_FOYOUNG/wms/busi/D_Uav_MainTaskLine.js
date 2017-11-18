mini.parse();
var _PageID = 18;
var _Languagefolder = getLanguageFolder();

var grdList = mini.get("grdList");

setAutoCompleteData("CREATE_BY", "Sys_D_USER");
setAutoCompleteData("BaseStationID", "D_Uav_BaseStation");
setAutoCompleteData("UavSetID", "D_Uav_UavSet");

InitSearchPage('toolbar', _PageID, grdList, null, [["TaskStatus", "TaskStatus"], ["FlyWay", "FlyWay"], ["TaskSource", "TaskSource"]]);

LoadDefault();

var _EditName = "任务航线编辑";

function Edit() {
    GetEditFormModify('#grdList', _EditName, _Languagefolder + 'wms/busi/D_Uav_MainTaskLineEdit.html');
}

grdList.on("rowdblclick", Edit);

function onCloseClick(e) {
    var obj = e.sender;
    obj.setText("");
    obj.setValue("");
}

//默认值
function SetDefault() {
    var form = new mini.Form("form1");
    var data = form.getData(true, false);
    SetDefaultValue("D_Uav_MainTaskLine", data, ["CREATE_DATE_BEGIN", "CREATE_DATE_END"]);
}

function LoadDefault() {
    var data = getDefaultValue("D_Uav_MainTaskLine", ["CREATE_DATE_BEGIN", "CREATE_DATE_END"]);
    if (data) {
        var form = new mini.Form("form1");
        form.setData(data);
    } else {
        //用户未设定时，程序员在此设置默认值
        mini.get("#TaskStatus").setValue(3);//任务状态默认查询 已安排
    }
}

//下载飞机实际飞行航线
function LineDownLoad() {
    var rows = grdList.getSelecteds();
    if (rows.length <= 0) {
        notify(_Mlen_8);
        return;
    }

    var bids = [];
    for (var i = 0; i < rows.length; i++) {
        //去掉必须是已完成限制
        //if (rows[i].TaskStatus_NAME != "已完成") {
        //    notify(_Mlen_96);
        //    return;
        //}
        bids.push(rows[i].BILLID);
    }
    var billids = bids.join(',');

    document.getElementById("BillIDS").value = billids;
    var excelForm = document.getElementById("excelLineForm");
    excelForm.submit();
    showTips("下载中，请稍等...");
}
