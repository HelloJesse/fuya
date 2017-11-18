mini.parse();
var grdList = mini.get("grdList");
var _Languagefolder = getLanguageFolder();
var _PageID = 2;

//CreateSearchButton('toolbar', 2);        //创建toolbar
InitSearchPage('toolbar', _PageID, grdList, null, [["CompanyID", "Company"], ["Activated", "Activated"]]);

//loadDropDownData([["CompanyID", "Company"], ["Activated", "Activated"]]);


function onCloseClick(e) {
    var obj = e.sender;
    obj.setText("");
    obj.setValue("");
}

function Act() {
    mini.confirm("确定要执行此操作？", "提示",
        function (action) {
            if (action == 'ok') {
                UpdateActived(_Datapath + 'EditManager.ashx?method=DoActived', 'Sys_D_Department', 'act')
            }
        });
}
function DisAct() {
    mini.confirm("确定要执行此操作？", "提示",
        function (action) {
            if (action == 'ok') {
                UpdateActived(_Datapath + 'EditManager.ashx?method=DoActived', 'Sys_D_Department', 'disact')
            }
        });
}

function Add() {
    GetTabsddNew('部门信息', '部门信息', _Languagefolder + 'wms/Main/DepartmentEdit.html');
}
function Edit() {
    GetEditFormModify('#grdList', '部门信息', _Languagefolder + 'wms/Main/DepartmentEdit.html')
}
grdList.on("rowdblclick", Edit);