mini.parse();
var _PageID = 10;
var _Languagefolder = getLanguageFolder();

var grdList = mini.get("grdList");

setAutoCompleteData("CREATE_BY", "Sys_D_USER");
setAutoCompleteData("BaseStationID", "D_Uav_BaseStation");

InitSearchPage('toolbar', _PageID, grdList, null, [["Activated", "Activated"], ["UavType", "UavType"], ["Status", "UavStatus"]]);

LoadDefault();

var _EditName = "飞机信息编辑";

function Add() {
    GetTabsddNew(_EditName, _EditName, _Languagefolder + 'wms/base/D_Uav_UavSetEdit.html');
}
function Edit() {
    GetEditFormModify('#grdList', _EditName, _Languagefolder + 'wms/base/D_Uav_UavSetEdit.html');
}

grdList.on("rowdblclick", Edit);

function Act() {
    UpdateActived(_Datapath + 'EditManager.ashx?method=DoActived', 'D_Uav_UavSet', 'act')
}
function DisAct() {
    UpdateActived(_Datapath + 'EditManager.ashx?method=DoActived', 'D_Uav_UavSet', 'disact')
}

function onCloseClick(e) {
    var obj = e.sender;
    obj.setText("");
    obj.setValue("");
}

//默认值
function SetDefault() {
    var form = new mini.Form("form1");
    var data = form.getData(true, false);
    SetDefaultValue("D_Uav_UavSet", data, ["CREATE_DATE_BEGIN", "CREATE_DATE_END"]);
}

function LoadDefault() {
    var data = getDefaultValue("D_Uav_UavSet", ["CREATE_DATE_BEGIN", "CREATE_DATE_END"]);
    if (data) {
        var form = new mini.Form("form1");
        form.setData(data);
    } else {
        //用户未设定时，程序员在此设置默认值
        mini.get("#Activated").setValue(1);
    }
}


