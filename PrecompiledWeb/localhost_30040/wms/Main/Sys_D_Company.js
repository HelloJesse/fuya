
mini.parse();
var grdList = mini.get("grdList");
var _Languagefolder = getLanguageFolder();
var _PageID = 4;

//CreateSearchButton('toolbar', 4);        //创建toolbar
InitSearchPage('toolbar', _PageID, grdList, null, [["PARENT_ID", "Company"], ["COUNTRY_ID", "Country"], ["PROVINCE_ID", "Province"], ["Activated", "Activated"]]);

LoadDefault();//加载默认值

//设置动态下拉框
setAutoCompleteData("CREATE_BY", "Sys_D_USER");
setAutoCompleteData("UPDATE_BY", "Sys_D_USER");

//loadDropDownData([["PARENT_ID", "Company"], ["COUNTRY_ID", "Country"], ["PROVINCE_ID", "Province"], ["Activated", "Activated"]]);


function Add() {
    GetTabsddNew('公司管理', '公司管理', _Languagefolder + 'wms/Main/Sys_D_Company_Edit.html')
}
function Edit() {
    GetEditFormModify('#grdList', '公司管理', _Languagefolder + 'wms/Main/Sys_D_Company_Edit.html')
}
grdList.on("rowdblclick", Edit);

function Act() {
    mini.confirm("确定要执行此操作？", "提示",
            function (action) {
                if (action == 'ok') {
                    UpdateActived(_Datapath + 'EditManager.ashx?method=DoActived', 'Sys_D_Company', 'act')
                }
            });
    
}
function DisAct() {
    mini.confirm("确定要执行此操作？", "提示",
            function (action) {
                if (action == 'ok') {
                    UpdateActived(_Datapath + 'EditManager.ashx?method=DoActived', 'Sys_D_Company', 'disact')
                }
            });
    
}


function onCloseClick(e) {
    var obj = e.sender;
    obj.setText("");
    obj.setValue("");
}

function onProviceChanged(e) {
    var proviceCombo = mini.get("#PROVINCE_ID");
    var cityCombo = mini.get("#CITY_ID");
    cityCombo.setValue("");
    var url = _Datapath +"ComboBoxManager.ashx?tablename=CityByProviceID&DataLoad=0&ParamsID=" + proviceCombo.getValue()
    cityCombo.setUrl(url);
}



function SetDefault() {
    var form = new mini.Form("form1");
    var data = form.getData(true, false);
    SetDefaultValue("Sys_D_Company", data, ["UPDATE_DATE_BEGIN", "UPDATE_DATE_END", "CREATE_DATE_BEGIN", "CREATE_DATE_END"]);
}
function LoadDefault() {
    var data = getDefaultValue("Sys_D_Company", ["UPDATE_DATE_BEGIN", "UPDATE_DATE_END", "CREATE_DATE_BEGIN", "CREATE_DATE_END"]);
    if (data) {
        var form = new mini.Form("form1");
        form.setData(data);
    } else {
        //用户未设定时，程序员在此设置默认值
    }
}
