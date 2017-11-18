mini.parse();
var grdList = mini.get("grdList");
var _Languagefolder = getLanguageFolder();
var _PageID = 1;


//CreateSearchButton('toolbar', 1);        //创建toolbar
InitSearchPage('toolbar', _PageID, grdList, null, [["USER_TYPE", "UserType"], ["CompanyID", "Company"]]);

//loadDropDownData([["USER_TYPE", "UserType"], ["CompanyID", "Company"]]);


//设置动态下拉框
setAutoCompleteData("CREATE_BY", "Sys_D_USER");

function onCompanyChanged(e) {
    var companyCombo = mini.get("#CompanyID");
    var deptCombo = mini.get("#DepartmentID");
    deptCombo.setValue("");
    var url = _Datapath +"ComboBoxManager.ashx?tablename=DeptByCompanyID&DataLoad=0&ParamsID=" + companyCombo.getValue()
    deptCombo.setUrl(url);
}
function Add() {
    GetTabsddNew('岗位信息', '岗位信息', _Languagefolder + 'wms/Main/Sys_D_User_Edit.html');
}
function Edit() {
    GetEditFormModify('#grdList', '岗位信息', _Languagefolder + 'wms/Main/Sys_D_User_Edit.html');
}
grdList.on("rowdblclick", Edit);
function UpdateActivedSech(flag) {
    if (flag == "act") {
        UpdateActived(_Datapath + 'EditManager.ashx?method=DoActived', 'Sys_D_USER', 'act');
    } else {
        UpdateActived(_Datapath + 'EditManager.ashx?method=DoActived', 'Sys_D_USER', 'disact');
    }
}



function onCloseClick(e) {
    var obj = e.sender;
    obj.setText("");
    obj.setValue("");
}
