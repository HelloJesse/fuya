mini.parse();
var _Languagefolder = getLanguageFolder();

var grdList = mini.get("grdList");

setAutoCompleteData("CREATE_BY", "Sys_D_USER");
setAutoCompleteData("COUNTRY_ID", "D_Country");

InitSearchPage('toolbar', 8, grdList, null, [["Activated", "Activated"]]);

LoadDefault();//默认值

function Add() {
    GetTabsddNew('客户信息编辑', '客户信息编辑', _Languagefolder + 'wms/base/CustomerEdit.html');
}
function Edit() {
    GetEditFormModify('#grdList', '客户信息编辑', _Languagefolder + 'wms/base/CustomerEdit.html');
}

grdList.on("rowdblclick", Edit);

function Act() {
    UpdateActived(_Datapath + 'EditManager.ashx?method=DoActived', 'D_Customer', 'act')
}
function DisAct() {
    UpdateActived(_Datapath + 'EditManager.ashx?method=DoActived', 'D_Customer', 'disact')
}

function onCloseClick(e) {
    var obj = e.sender;
    obj.setText("");
    obj.setValue("");
}

function GetTabsdd() {
    var tab = { name: "客户信息编辑", title: "客户信息编辑", url: _Languagefolder + 'wms/base/CustomerEdit.html', showCloseButton: true };
    top["win"].addTab_Child(tab);
}

//默认值
function SetDefault() {
    var form = new mini.Form("form1");
    var data = form.getData(true, false);
    SetDefaultValue("D_Customer", data, ["CREATE_DATE_BEGIN", "CREATE_DATE_END"]);
}

function LoadDefault() {
    var data = getDefaultValue("D_Customer", ["CREATE_DATE_BEGIN", "CREATE_DATE_END"]);
    if (data) {
        var form = new mini.Form("form1");
        form.setData(data);
    } else {
        //用户未设定时，程序员在此设置默认值
        mini.get("#Activated").setValue(1);
    }
}

