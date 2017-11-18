__CreateJSPath = function (js) {
    var scripts = document.getElementsByTagName("script");
    var path = "";
    for (var i = 0, l = scripts.length; i < l; i++) {
        var src = scripts[i].src;
        if (src.indexOf(js) != -1) {
            var ss = src.split(js);
            path = ss[0];
            break;
        }
    }
    var href = location.href;
    href = href.split("#")[0];
    href = href.split("?")[0];
    var ss = href.split("/");
    ss.length = ss.length - 1;
    href = ss.join("/");
    if (path.indexOf("https:") == -1 && path.indexOf("http:") == -1 && path.indexOf("file:") == -1 && path.indexOf("\/") != 0) {
        path = href + "/" + path;
    }
    return path;
}


var bootPATH = __CreateJSPath("boot.js");
var _Datapath = "http://" + location.host + "/data/";
var _Rootpath = "http://" + location.host + "/";
//debugger
mini_debugger = true;

//miniui
document.write('<script src="' + bootPATH + 'jquery-1.6.2.min.js" type="text/javascript"></sc' + 'ript>');
document.write('<script src="' + bootPATH + 'miniui/miniui.js" type="text/javascript" ></sc' + 'ript>');
document.write('<script src="' + bootPATH + 'MultiLanguage.js" type="text/javascript" ></sc' + 'ript>');
if (this.location.href.indexOf("/en/") > 0) {
    document.write('<script src="' + bootPATH + 'miniui/locale/en_US.js" type="text/javascript" ></sc' + 'ript>');
}

//var basedataUrl = getCookie_My("baseData");
//if ("undefined" != typeof basejs) {
//    var CompanyName = getCookie_My("CompanyName");
//    if (CompanyName) {
//        basejs.src = bootPATH + '/basedata/' + CompanyName + '_jsdata.js';
//    }
//}
document.write('<link href="' + bootPATH + 'miniui/themes/default/miniui.css" rel="stylesheet" type="text/css" />');
document.write('<link href="' + bootPATH + 'miniui/themes/icons.css" rel="stylesheet" type="text/css" />');




//skin
var skin = getCookie("miniuiSkin");
if (skin) {
    document.write('<link href="' + bootPATH + 'miniui/themes/' + skin + '/skin.css" rel="stylesheet" type="text/css" />');
}
else {
    //document.write('<link href="' + bootPATH + 'miniui/themes/blue2010/skin.css" rel="stylesheet" type="text/css" />');//默认加载蓝色的样式
    document.write('<link href="' + bootPATH + 'miniui/themes/metro/skin.css" rel="stylesheet" type="text/css" />');//默认加载metro
}


document.write('<link href="' + bootPATH + 'Style.css" rel="stylesheet" type="text/css" />');

//document.write('<link href="' + bootPATH + 'miniui/themes/bootstrap/skin.css" rel="stylesheet" type="text/css" />');
var _SessionFlag = true; //启用本地缓存
function getVerUrl(url) {
    var ver = "ver=20160912";
    if (url.indexOf('?') > 0)
        return url + "&" + ver;
    else
        return url + "?" + ver;
}
function ExportExcel(gridid) {
    var ids = mini.get("IDS").value;
    if (ids.length == 0) {
        //showTips("未找到需要导出的数据.");
        showTips(_Mlen_19);
        return;
    }
    var grid = mini.get(gridid);
    var columns = grid.getBottomColumns();

    function getColumns(columns) {
        columns = columns.clone();
        for (var i = columns.length - 1; i >= 0; i--) {
            var column = columns[i];
            if (!column.field) {
                columns.removeAt(i);
            } else {
                var c = { header: column.header, field: column.field };
                columns[i] = c;
            }
        }
        return columns;
    }

    var columns = getColumns(columns);
    var json = mini.encode({ Columns: columns, IDS: ids });
    document.getElementById("excelData").value = json;
    var excelForm = document.getElementById("excelForm");
    excelForm.submit();
}
////////////////////////////////////////////////////////////////////////////////////////
function getCookie(sName) {
    var aCookie = document.cookie.split("; ");
    var lastMatch = null;
    for (var i = 0; i < aCookie.length; i++) {
        var aCrumb = aCookie[i].split("=");
        if (sName == aCrumb[0]) {
            lastMatch = aCrumb;
        }
    }
    if (lastMatch) {
        var v = lastMatch[1];
        if (v === undefined) return v;
        return unescape(v);
    }
    return null;
}
///检查是否登录超时错误
function isLoginOut(jsondata) {
    if (jsondata.isOut) {
        //mini.alert("登录超时,请重新登录.");
        mini.alert(_Mlen_23);
        var url = "http://" + location.host + "/login.html";
        url = getVerUrl(url);
        window.open(url, "_top");
        //window.parent.location.reload();
    } else {
        return false;
    }
}
function loading(obj) {
    //obj.loading("正在加载....", "提示信息");
    obj.loading(_Mlen_24, _Mlen_35);
}
function handing(obj) {
    //obj.loading("正在处理....", "提示信息");
    obj.loading(_Mlen_25, _Mlen_35);
}

//创建编辑界面新增时的按钮
function createAddEditFormButton(toolbarid, pageid, setAddStatus) {
    var key = "addeditbtn" + toolbarid + "-" + pageid;
    var value = getSessionStorageValue(key);
    if (value) {
        var d = mini.decode(value);
        createButton(toolbarid, d);
        if (setAddStatus) {
            setAddStatus();
        }
    } else {
        $.ajax({
            url: "http://" + location.host + "/data/EditManager.ashx?method=GetEditFunction",
            type: "post",
            data: { PageID: pageid },
            success: function (text) {
                var data = mini.decode(text);   //反序列化成对象
                if (isLoginOut(data) == false) {
                    if (data.IsOK) {
                        setSessionStorageValue(key, mini.encode(data.Data));
                        createButton(toolbarid, data.Data);
                        if (setAddStatus) {
                            setAddStatus();
                        }
                    } else {
                        showNoFoundData();
                    }
                }
            }
        });
    }
}
//创建按钮
function createButton(toolbarid, data) {
    var tb = document.getElementById(toolbarid);
    if (tb) {
        tb.innerHTML = "";
        var pid = "";
        var menubutton;
        var tempmenu;
        var menuitems;
        var j = 0;
        
        var tb$ = $("#"+toolbarid);
        tb$.css("border-top", "solid 0px");
        tb$.css("border-left", "solid 0px");
        tb$.css("border-right", "solid 0px");
        for (var i = 0; i < data.length; i++) {
            var bconfig = data[i];
            if (bconfig.PID == null || bconfig.PID == "") {
                if (pid != "") {
                    tempmenu.setItems(menuitems);
                    menubutton.render(tb);
                    pid = "";
                }
                var btn = new mini.Button();
                btn.set({
                    id: data[i].BtnID,          //btn_{id}_{code}如：btn_4_Add
                    text: data[i].Text,
                    iconCls: data[i].Ico,
                    onclick: data[i].Click,
                    plan: true,
                    style: "margin-right:2px"
                });
                btn.render(tb);
            } else {
                if (bconfig.PID != pid) {
                    if (pid != "") {
                        tempmenu.setItems(menuitems);
                        menubutton.render(tb);
                    }
                    j = 0;
                    pid = bconfig.PID;
                    menubutton = new mini.MenuButton();

                    tempmenu = new mini.Menu();
                    //tempmenu.set({
                    //    id: pid,
                    //    name:pid
                    //});
                    menubutton.set({
                        text: bconfig.PText,
                        menu: tempmenu,
                        style: "margin-right:2px"
                    });
                    menuitems = new Array();
                    var menuitem = new mini.MenuItem();
                    menuitem.set({
                        id: data[i].BtnID,          //btn_{id}_{code}如：btn_4_Add
                        text: data[i].Text,
                        iconCls: data[i].Ico,
                        onclick: data[i].Click
                    });
                    menuitems[j] = menuitem;
                }
                else {
                    //添加子项
                    j = j + 1;
                    var menuitem = new mini.MenuItem();
                    menuitem.set({
                        id: data[i].BtnID,          //btn_{id}_{code}如：btn_4_Add
                        text: data[i].Text,
                        iconCls: data[i].Ico,
                        onclick: data[i].Click,
                        style: "margin-right:2px"
                    });
                    menuitems[j] = menuitem;
                }
            }
        }
        if (pid != "") {
            tempmenu.setItems(menuitems);
            menubutton.render(tb);

            pid = "";
        }
        mini.layout();
    } else {
        mini.alert('Not Found ToolBar');
    }
}
//成功时使用此方法提示
function showTips(message) {
    //notify(message); return;
    var x = "center";
    var y = "top";
    var state = "success";
    mini.showTips({
        //content: "<b>提示</b> <br/>" + message,
        content: "<b>" + _Mlen_4 + "</b> <br/>" + message,
        state: state,
        x: x,
        y: y,
        timeout: 1000
    });
}
//失败时使用此方法提示
function notify(message) {
    var win = new mini.Window;
    win.set({
        showCloseButton: true,
        showFooter: true,
        iconCls: "icon-no",
        allowDrag: true,
        style: "text-align:center",
        //title: '提示信息',
        title: _Mlen_35,
        width: 400,
        height: 250
    });
    var el = win.getBodyEl();
    el.innerText = message;
    var btn = new mini.Button();
    btn.set({
        text: "Close",
        plan: true,
        iconCls: "icon-close",
        style: "text-align:center;margin-right:2px",
        onclick: function () {
            win.hide();
        }
    });
    el = win.getFooterEl();
    btn.render(el);
    win.showAtPos("center", "middle");
    return;
    mini.alert(message); return;

    //var x = "center";//right
    //var y = "top";//bottom
    //var tempwidth = 400;
    //if (message.length > 60) {
    //    tempwidth = 800;
    //}
    //mini.showMessageBox({
    //    showModal: false,
    //    width: tempwidth,
    //    title: "提示",
    //    iconCls: "mini-messagebox-warning",
    //    message: message,
    //    timeout: 3000,
    //    x: x,
    //    y: y
    //});
}
function showNoFoundData() {
    //showTips("未找到有效的数据.");
    showTips(_Mlen_22);
}
function showSaveOK() {
    //showTips("保存成功.");
    showTips(_Mlen_17);//MultiLanguage.js里定义
}
function CloseTab() {
    top["win"].closeTab();
}

function CreateSearchButton(toolbarid, pageid) {
    alert('此方法不再使用,请程序员修改为InitSearchPage');
    var key = "searchbtn" + toolbarid + "-" + pageid;
    var value = getSessionStorageValue(key);
    if (value) {
        var d = mini.decode(value);
        createButton(toolbarid, d);
    }
    else {
        $.ajax({
            url: "http://" + location.host + "/data/main/RoleManager.ashx?method=GetSearchFunction",
            type: "post",
            data: { PageID: pageid },
            success: function (text) {
                var data = mini.decode(text);   //反序列化成对象
                if (isLoginOut(data) == false) {
                    if (data.IsOK) {
                        setSessionStorageValue(key, mini.encode(data.Data));
                        createButton(toolbarid, data.Data);
                    } else {
                        showNoFoundData();
                    }
                }
            }
        });
    }
}
//初始列表
function InitSearchGrid(pageid, grid) {

    $.ajax({
        url: "http://" + location.host + "/data/SearchManager.ashx?method=GetSearchGridConfig",
        type: "post",
        data: { PageID: pageid },
        success: function (text) {
            var data = mini.decode(text);   //反序列化成对象
            if (isLoginOut(data) == false) {
                if (data.IsOK) {
                    var key = "searchpage" + "-" + pageid;
                    var value = getSessionStorageValue(key);
                    if (value) {
                        var olddata = mini.decode(value);
                        olddata.ColumnsData = data.ColumnsData;
                        setSessionStorageValue(key, mini.encode(olddata));
                    }
                    //创建列表
                    CreateGrid(grid, data.ColumnsData.Columns, data.ColumnsData.FieldID);

                } else {
                    showNoFoundData();
                }
            }
        }
    });

}
/*
toolbarid:工具条
pageid:所属页
grid:明细列表对象
otherHandler:加载完执行的方法
dropdown：下拉框配置项
dropdownHandler：下拉加载完处理方法，针对列表中下拉处理
*/
function InitSearchPage(toolbarid, pageid, grid,otherHandler,dropdown,dropdownHandler) {
    var getgridflag = "";
    if (grid != null) getgridflag = "1";

    var key = "searchpage" + "-" + pageid;

    var value = getSessionStorageValue(key);
    if (value) {
        var d = mini.decode(value);
        createButton(toolbarid, d.ButtonData);
        setDropdownData(dropdown,d.DropdownData,dropdownHandler,false);//设置下拉数据
        if (getgridflag == "1") {
            CreateGrid(grid, d.ColumnsData.Columns, d.ColumnsData.FieldID);
        }
        if (otherHandler) {
            otherHandler();
        }
    }
    else {
        $.ajax({
            url: "http://" + location.host + "/data/SearchManager.ashx?method=GetSearchFunction",
            type: "post",
            data: { PageID: pageid, Dropdown: mini.encode( dropdown) },
            success: function (text) {
                var data = mini.decode(text);   //反序列化成对象
                if (isLoginOut(data) == false) {
                    if (data.IsOK) {
                        //setSessionStorageValue(key, mini.encode(data.ButtonData));
                        setSessionStorageValue(key, mini.encode(data)); //设置缓存
                        createButton(toolbarid, data.ButtonData);   //创建按钮
                        setDropdownData(dropdown, data.DropdownData, dropdownHandler, true);//设置下拉数据
                        //创建列表
                        if (getgridflag == "1") {
                            CreateGrid(grid, data.ColumnsData.Columns, data.ColumnsData.FieldID);
                        }
                    } else {
                        showNoFoundData();
                    }
                    if (otherHandler) {
                        otherHandler();
                    }
                }
            }
        });
    }
}
function SaveGridColumnsOrderWidth(pageid, gridid) {
    var grid = mini.get(gridid);
    var fieldidwidth = "";
    for (var i = 0, l = grid.columns.length; i < l; i++) {
        fieldidwidth = fieldidwidth + grid.columns[i].field + ":" + grid.columns[i].width.replace("px", "") + ";";
    }
    var mask = mini.loading(_Mlen_24);
    $.ajax({
        url: "http://" + location.host + "/data/SearchManager.ashx?method=GetSearchGridConfig",
        type: "post",
        data: { PageID: pageid, FieldWidth: fieldidwidth },
        success: function (text) {
            var data = mini.decode(text);   //反序列化成对象
            if (isLoginOut(data) == false) {
                if (data.IsOK) {
                    var key = "searchpage" + "-" + pageid;
                    var value = getSessionStorageValue(key);
                    if (value) {
                        var olddata = mini.decode(value);
                        olddata.ColumnsData = data.ColumnsData;
                        setSessionStorageValue(key, mini.encode(olddata));
                    }
                    //创建列表
                    CreateGrid(grid, data.ColumnsData.Columns, data.ColumnsData.FieldID, false);
                    mini.hideMessageBox(mask);//mini.unmask();

                } else {
                    showNoFoundData();

                    mini.hideMessageBox(mask);//mini.unmask();
                }
            }
        }
    });
}
//设置按钮状态,当没找到时则忽略
function setButtonEnabled(buttonids, enabled) {
    for (var i = 0; i < buttonids.length; i++) {
        var btn = mini.get(buttonids[i]);
        if (btn) {
            btn.setEnabled(enabled);
        }
    }
}
function setComboboxBaseData(comboboxID, data) {
    var cbo = mini.get(comboboxID);
    if (cbo) {
        cbo.setData(data);
    }
    else {
        //mini.alert("ID[" + comboboxID + "]无效.");
        mini.alert("ID[" + comboboxID + "]" + _Mlen_35);
    }
}
//设置联动控件URL
function setRefComboBoxUrl(id, type, pid) {
    var cbo = mini.get(id);
    if (cbo) {
        var strpvalue = "&ParamsID=" + mini.get(pid).value;
        var url = _Datapath + "ComboBoxManager.ashx?tablename=" + type + "&DataLoad=0" + strpvalue;
        cbo.set({
            url: url// "../../data/ComboBoxManager.ashx?tablename=" + type + "&DataLoad=0" + strpvalue
        });
    }
    else {
        //mini.alert("ID[" + id + "]无效.");
        mini.alert("ID[" + id + "]" + _Mlen_35);
    }
}

function setAutoCompleteData(id, type, pid) {
    var cbo = mini.get(id);
    var url = _Datapath + "ComboBoxManager.ashx";
    if (cbo) {
        // 设置显示名称
        cbo.on("valuechanged", function (e) {
            var a = e;
            if (e.selected) {
                e.sender.setText("[" + e.selected.CODE + "]" + e.selected.NAME);
            }
        });
        if (pid) {
            var strpvalue = "";
            mini.get(pid).on("valuechanged", function (e) {
                strpvalue = "&ParamsID=" + e.sender.value;
                cbo.set({
                    url: url + "?tablename=" + type + "&DataLoad=0" + strpvalue,
                    value: "",
                    text: "",
                    valueFromSelect: true
                });

            })

        } else {
            cbo.set({
                url: url + "?tablename=" + type + "&DataLoad=1",
                //emptyText: "请输入..."
                emptyText: _Mlen_26,
                valueFromSelect:true
            });
        }
    }
    else {
        //mini.alert("ID[" + id + "]无效.");
        mini.alert("ID[" + id + "]" + _Mlen_35);
    }
}
////设置
//var _BaseData = new Array();
////从缓存获取数据
//function getCacheComboboxBaseData(tableName) {
//    for (var i = 0; l = _BaseData.length, i < l; i++) {
//        if (_BaseData[i].TableName == tableName) {
//            return _BaseData[i];
//        }
//    }
//    return null;
//}
////从服务器获取数据
//function getComboboxServerBaseData(tableName,combobox) {

//}
//function setComboxBaseData(comboxid, tablename) {
//    var combox = mini.get(comboxid);
//    if (combox) {
//        var data = getCacheComboboxBaseData(tablename);
//        if (data == null) {
//            //读取数据
//            getComboboxServerBaseData(tablename, combox);
//        }
//        else {
//            combox.setData(data);
//        }
//    } else {
//        mini.alert("ID[" + comboxid + "]无效.");
//    }
//}
//var TT = "";
//function SetTT(v) {
//    TT = v;
//}
//function getTT() {
//    mini.alert(TT);
//}



///自动清空grid的属性列
function AutoClearGridCustomProperty(grdarray) {
    if (grdarray.length > 0) {
        for (i = 0; i < grdarray.length; i++) {
            var grid = mini.get(grdarray[i]);
            var oldcolumns = grid.getColumns();
            //清除原来有的PropertyGird列
            var deletePos = 0;
            var deleteCount = 0;
            for (k = 1; k <= 12; k++) {
                for (j = 0; j < oldcolumns.length; j++) {
                    if (oldcolumns[j].field == "Property" + k) {
                        if (deletePos == 0) {
                            deletePos = j;
                        }
                        deleteCount = deleteCount + 1;
                    }
                }
            }
            oldcolumns.splice(deletePos, deleteCount);
            grid.setColumns(oldcolumns);
        }
    }
}

///
///根据客户ID动态加载Gird列
///grdarray grid id 的数组 customid 客户的ID setNum 增加列的起始位置 isReadyonlys 是否只读 1：只读 0: 可编辑
//typenum:1：入库 2：库存 3：出库
function AutoSetGridCustomProperty(grdarray, customid, setNum, typenum, isReadyonlys) {
    if (grdarray.length > 0) {
        $.ajax({
            url: _Datapath + "PropertyAction.ashx",
            data: { id: customid, type: typenum, isReadyonly: isReadyonlys },
            type: 'post',
            success: function (txt) {
                var datas = mini.decode(txt);
                if (datas.iswrong == 0) {
                    var columnsnew = datas.newcols;
                    for (i = 0; i < grdarray.length; i++) {
                        var grid = mini.get(grdarray[i]);
                        var oldcolumns = grid.getColumns();
                        //清除原来有的PropertyGird列
                        var deletePos = 0;
                        var deleteCount = 0;
                        for (k = 1; k <= 12; k++) {
                            for (j = 0; j < oldcolumns.length; j++) {
                                if (oldcolumns[j].field == "Property" + k) {
                                    if (deletePos == 0) {
                                        deletePos = j;
                                    }
                                    deleteCount = deleteCount + 1;
                                }
                            }
                        }
                        oldcolumns.splice(deletePos, deleteCount);
                        if (setNum > oldcolumns.length) {
                            var columns = oldcolumns.concat(columnsnew);
                            grid.setColumns(columns);
                        }
                        else {
                            var fircols = oldcolumns.slice(0, setNum);
                            var lastcols = oldcolumns.slice(setNum, oldcolumns.length);
                            var nclos = fircols.concat(columnsnew).concat(lastcols);
                            grid.setColumns(nclos);
                        }
                    }
                }
            }
        });

    }
}

//启用、停用
function sysUpdateActived(flag, id, tableName) {

    var flag = (flag == "1" ? "act" : "disact");

    //'act':启用 'disact':'停用'
    var ids = mini.get("#" + id).value; //mini.getValue("#ID");
    //ids = ids.substring(0, ids.length - 1);
    $.ajax({
        url: _Datapath + "EditManager.ashx?method=DoActived",
        data: {
            tablename: tableName, tag: flag, ids: ids
        },
        type: 'post',
        success: function (data) {
            var datas = mini.decode(data);
            if (datas.iswrong == "0") {
                //showTips("操作成功。");
                showTips(_Mlen_5);
                loadForm(ids);
            } else {
                if (isLoginOut(datas) == false) {
                    //notify("操作失败：" + datas.errmassage);
                    notify(_Mlen_6 + datas.errmassage);
                }
            }
        }
    });
}
//打印 id--报表id,wheresql--条件
function PReport(id, wheresql) {
    var url = "http://" + location.host + "/data/Report.aspx?ID=" + id + "&SQL=" + wheresql;

    //var tab = { name: "打印", title: "打印", url: url, showCloseButton: true };
    var tab = { name: _Mlen_27, title: _Mlen_27, url: url, showCloseButton: true };
    if (top["win"]) {
        top["win"].addTab_Child(tab);
    } else {
        window.open(url.split("/")[url.split("/").length - 1]);
    }
}
function getCookie_My(c_name) {
    if (document.cookie.length > 0) {
        c_start = document.cookie.indexOf(c_name + "=")
        if (c_start != -1) {
            c_start = c_start + c_name.length + 1
            c_end = document.cookie.indexOf(";", c_start)
            if (c_end == -1) c_end = document.cookie.length
            return unescape(document.cookie.substring(c_start, c_end))
        }
    }
    return ""
}
function setCookie(c_name, value, expiredays) {
    var exdate = new Date()
    exdate.setDate(exdate.getDate() + expiredays)
    document.cookie = c_name + "=" + escape(value) +
    ((expiredays == null) ? "" : ";expires=" + exdate.toGMTString())
}

function SetDefaultValue(name, data, datefields) {
    //mini.confirm("确定要设置默认值？", "确定？",
    mini.confirm(_Mlen_28, _Mlen_29,
             function (action) {
                 if (action == "ok") {
                     var datenow = getNowDate();
                     for (var i = 0, l = datefields.length; i < l; i++) {
                         if (data[datefields[i]] == getLocaleDateString(datenow, "")) {
                             data[datefields[i]] = "1900-02-01";    //代表当天
                         } else if (data[datefields[i]] == "1900-01-01"
                             || data[datefields[i]] == "1900-1-1") {
                             data[datefields[i]] = "1900-01-01";    //代表当月第一天
                         } else if (data[datefields[i]] != "") {
                             var temp = new Date(1950, 0, 1);
                             var diff = datenow.DateDiff('d', data[datefields[i]]);
                             temp = temp.DateAdd('d', diff);
                             data[datefields[i]] = getLocaleDateString(temp, "");
                         }
                     }
                     if (data.IDS) {
                         data.IDS = "";
                     }
                     localStorage.setItem(name, mini.encode(data));
                     //setCookie(name, mini.encode(data), 100);
                 }
             }
         );

}
//获取默认值
function getDefaultValue(name, datefields) {
    var s = localStorage.getItem(name);// getCookie_My(name);
    if (s) {
        var data = mini.decode(s);
        for (var i = 0, l = datefields.length; i < l; i++) {
            if (data[datefields[i]] == "1900-1-1" ||
                data[datefields[i]] == "1900-01-01") {
                data[datefields[i]] = GetDateByFlag("1");
            }
            else if (data[datefields[i]] == "1900-2-1" ||
                data[datefields[i]] == "1900-02-01") {
                data[datefields[i]] = GetDateByFlag("");
            }
            else if (data[datefields[i]] != "") {
                var date = new Date(1950, 0, 1);
                var datenow = getNowDate();
                data[datefields[i]] = getLocaleDateString(datenow.DateAdd('d', date.DateDiff('d', data[datefields[i]])), "");
            }
        }
        return data;
    }
    return null;
}
//清除cookie  
function clearCookie(name) {
    localStorage.setItem(name, "");
    setCookie(name, "", -1);
}
// flag=1为当前一号,其它为当前日期
function GetDateByFlag(flag) {

    var now = new Date();
    return getLocaleDateString(now, flag);
}
Date.prototype.DateAdd = function (strInterval, Number) {
    var dtTmp = this;
    switch (strInterval) {
        case 's': return new Date(Date.parse(dtTmp) + (1000 * Number));
        case 'n': return new Date(Date.parse(dtTmp) + (60000 * Number));
        case 'h': return new Date(Date.parse(dtTmp) + (3600000 * Number));
        case 'd': return new Date(Date.parse(dtTmp) + (86400000 * Number));
        case 'w': return new Date(Date.parse(dtTmp) + ((86400000 * 7) * Number));
        case 'q': return new Date(dtTmp.getFullYear(), (dtTmp.getMonth()) + Number * 3, dtTmp.getDate(), dtTmp.getHours(), dtTmp.getMinutes(), dtTmp.getSeconds());
        case 'm': return new Date(dtTmp.getFullYear(), (dtTmp.getMonth()) + Number, dtTmp.getDate(), dtTmp.getHours(), dtTmp.getMinutes(), dtTmp.getSeconds());
        case 'y': return new Date((dtTmp.getFullYear() + Number), dtTmp.getMonth(), dtTmp.getDate(), dtTmp.getHours(), dtTmp.getMinutes(), dtTmp.getSeconds());
    }
}
//+---------------------------------------------------  
//| 比较日期差 dtEnd 格式为日期型或者 有效日期格式字符串  
//+---------------------------------------------------  
Date.prototype.DateDiff = function (strInterval, dtEnd) {
    var dtStart = this;
    if (typeof dtEnd == 'string')//如果是字符串转换为日期型  
    {
        var temps = dtEnd.split("-");
        dtEnd = new Date(temps[0], temps[1] - 1, temps[2]);
        if (dtEnd.toLocaleDateString() == "NaN") {
            //mini.alert("请设置服务器日期格式为:yyyy-MM-dd");
            mini.alert(_Mlen_30 + ":yyyy-MM-dd");
        }
    }
    switch (strInterval) {
        case 's': return parseInt((dtEnd - dtStart) / 1000);
        case 'n': return parseInt((dtEnd - dtStart) / 60000);
        case 'h': return parseInt((dtEnd - dtStart) / 3600000);
        case 'd': return parseInt((dtEnd - dtStart) / 86400000);
        case 'w': return parseInt((dtEnd - dtStart) / (86400000 * 7));
        case 'm': return (dtEnd.getMonth() + 1) + ((dtEnd.getFullYear() - dtStart.getFullYear()) * 12) - (dtStart.getMonth() + 1);
        case 'y': return dtEnd.getFullYear() - dtStart.getFullYear();
    }
}
function getLocaleDateString(tempdate, flag) {
    var year = tempdate.getFullYear();       //年
    var month = tempdate.getMonth() + 1;     //月
    var day = tempdate.getDate();            //日
    if (flag == "1") {
        day = "1";
    }
    var clock = year + "-";

    if (month < 10)
        clock += "0";

    clock += month + "-";

    if (day < 10)
        clock += "0";

    clock += day;

    return (clock);
    //var tt = LocaleDateString.replace(/年|月/g, "-").replace(/日/g, " ").replace(/\//g, "-");
    //var temps = tt.split("-");
    //if (temps[1].length == 1) {
    //    temps[1] = "0" + temps[1];
    //}
    //if (temps[2].length == 1) {
    //    temps[2] = "0" + temps[2];
    //}
    //return temps[0]+"-"+temps[1]+"-"+temps[2];
    //if (tt.split("-")[1] < 10 && tt.split("-")[2] > 9) {
    //    tt = tt.split("-")[0] + "-0" + tt.split("-")[1] + "-" + tt.split("-")[2];
    //} else if (tt.split("-")[2] < 10 && tt.split("-")[1] > 9) {
    //    tt = tt.split("-")[0] + "-" + tt.split("-")[1] + "-0" + tt.split("-")[2];
    //} else if (tt.split("-")[1] < 10 && tt.split("-")[2] < 10) {
    //    tt = tt.split("-")[0] + "-0" + tt.split("-")[1] + "-0" + tt.split("-")[2];
    //}
    //return tt;
}
function getNowDate() {
    var now = new Date();

    return new Date(now.getFullYear(), now.getMonth(), now.getDate());
}
var _NoSetDefault_01 = "编辑状态下不能设置默认值."; //_Mlen_31;

//加载帮助文档
function GetHelp(id, name) {
    var urls = "http://" + location.host + "/data/Help.aspx?ID=" + id;
    var tabs = top["win"].mini.get("mainTabs");
    var ids = "tab$" + name + id;
    var tab = tabs.getTab(ids);
    if (!tab) {
        //tab = { name: ids, title: name + "帮助", url: urls, showCloseButton: true };
        tab = { name: ids, title: name + _Mlen_32, url: urls, showCloseButton: true };
        tabs.addTab(tab);
    }
    tabs.activeTab(tab);
    //if (top["win"]) {
    //    top["win"].addTab_Child(tab);
    //} else {
    //    window.open(url.split("/")[url.split("/").length - 1]);
    //}
}
function enterToTab() {
    if (event.srcElement.type != 'submit' && event.srcElement.type != "image" && event.srcElement.type != 'textarea'
    && event.keyCode == 13)
        event.keyCode = 9;
}
//tiler 注释不再使用，使用此方法，在列表编辑实时下拉中选择会有问题
//document.onkeydown = enterToTab;



function getSessionStorageValue(key) {
    if (_SessionFlag == true) {
        if (sessionStorage) {
            //添加语言标记
            var languageflag = "";
            if (this.location.href.indexOf("/en/") > 0) {
                languageflag = "_E";
            }
            key = key + languageflag;
            return sessionStorage.getItem(key);
        }
        else
            return undefined;
    }
    else
        return undefined;
}
function loadDropDownData(names, otherManager) {

    if (!window.localStorage) {
        min.alert("浏览暂不支持localStorage,请使用更高版本."); return;
    }
    var nocacheDropdown = getNoCacheDropdown(names);
    if (nocacheDropdown.length > 0) {
        $.ajax({
            url: "http://" + location.host + "/data/ComboBoxManager.ashx?method=GetDropDownData",
            type: "post",
            data: { data: mini.encode(nocacheDropdown) },
            success: function (text) {
                var data = mini.decode(text);   //反序列化成对象
                if (isLoginOut(data) == false) {
                    setDropdownData(nocacheDropdown, data, otherManager,true);
                }

            }
        });
    }
    else {
        if (otherManager) otherManager();
    }
}
//下拉相关代码。。。。。。。。。。。。。。。。。。。。。BEGIN..................................................
function getNoCacheDropdown(dropdowns) {
    var nocacheDropdown = new Array();
    var j = 0;
    for (var i = 0, l = dropdowns.length; i < l; i++) {
        var key = dropdowns[i][0];
        var datakey = dropdowns[i][1];
        var value = getSessionStorageValue(datakey);
        if (value) {
            if (key == "") {
                continue;
            }
            var d = mini.decode(value);
            bindDropdownData(key, d);
        }
        else {//没有缓存，去服务器取
            //nocacheDropdown[j] = { key: key, datakey: datakey };
            nocacheDropdown[j] = [key,datakey];
            j = j + 1;
        }
    }
    return nocacheDropdown;
}
/*
keys:需要绑定的下拉信息
datas:数据集
otherManager:绑定后要执行的方法
*/
function setDropdownData(keys, datas, otherManager, needCache) {
    if (keys) {
        for (var i = 0, l = keys.length; i < l; i++) {
            var datakeyname = keys[i][1];
            var key = keys[i][0];
            var value = mini.encode(datas[datakeyname]);
            if (needCache) setCacheJsObject(datakeyname, value);

            if (key == "") {
                continue;
            }
            bindDropdownData(key, datas[datakeyname]);
        }
        if (otherManager) otherManager();
    }
}
//绑定下拉数据
function bindDropdownData(id, data) {
    var c = mini.get(id);
    if (c) {
        if (data.length > 0) {
            if (data[0].NAME.indexOf("[" + data[0].CODE + "]") == -1) {
                for (var i = 0; i < data.length; i++) {
                    data[i].NAME = "[" + data[i].CODE + "]" + data[i].NAME;
                }
            }
        }
        
        c.set({
            data: data,
            pinyinField: "CODE",
            valueFromSelect: true
        });
    }
}
//下拉相关代码。。。。。。。。。。。。。。。。。。。。。END..................................................

function setSessionStorageValue(key, value) {
    setSessionStorage(key, value, _SessionFlag);
}
//设置缓存 cacheflag为true时，必须设置
function setSessionStorage(key, value, cacheflag) {
    if (_SessionFlag == true || cacheflag == true) {
        if (sessionStorage) {
            var languageflag = "";
            if (this.location.href.indexOf("/en/") > 0) {
                languageflag = "_E";
            }
            key = key + languageflag;
            sessionStorage.setItem(key, value);
        }
    }
}
//设置缓存
function setCacheJsObject(key, value) {
    setSessionStorage(key, value, true);    
}
function getCacheJsObject(name) {
    if (sessionStorage) {
        return mini.decode(sessionStorage.getItem(name));
    }
    else {
        mini.alert("浏览器版本不支持SessionStorage，请使用高版本浏览器.");
    }
}
function getCacheArgs() {
    var id = sessionStorage.getItem("hrf_args");

    if (id && id.toString().length > 0) {
        sessionStorage.setItem("hrf_args", "");
        return mini.decode(id);
    }
    else
        return new Object();

}
function setCacheArgs(args) {

    sessionStorage.setItem("hrf_args", mini.encode(args));
}
function getLanguageFolder() {
    if (location.href.indexOf("/en/") > 0) {
        return "en/";
    }
    return "";
}

///自定义列
function UserColumns(pageId, gridid) {
    if (PageId = "") {
        //mini.alert("页面 PageID 无效,请检查!");
        mini.alert(_Mlen_33);
        return;
    }
    var grid = mini.get(gridid);
    //自定义列页面
    var languageUrl = getLanguageFolder();
    mini.open({
        url: "http://" + location.host + "/" + languageUrl + "wms/base/UserColumns.html",
        //title: "自定义列", width: 500, height: 600,
        title: _Mlen_34, width: 500, height: 600,
        onload: function () {
            var iframe = this.getIFrameEl();
            var data = { pageId: pageId };
            iframe.contentWindow.SetData(data);
        },
        ondestroy: function (action) {
            if (action == "save") {
                //重新加载列表
                InitSearchGrid(pageId, grid);
                //grid.reload();
            }
        }
    });
}
//根据配置，创建列表
function CreateGrid(grd, items, idfield, cleardata) {
    var mycolumns = new Array(items.length + 2);
    var columns = new Array();
    mycolumns[0] = { type: "indexcolumn", header: "No", headerAlign: "center", width: 60 };
    columns.push(mycolumns[0]);
    mycolumns[1] = { type: "checkcolumn", header: "", headerAlign: "center", width: 40 };
    columns.push(mycolumns[1]);
    
    var count = 2;
    for (var i = 0, l = items.length; i < l; i++) {
        var item = items[i];
        if (item.Hide) {
            continue;
        }
        if (item.Format == "D") {
            mycolumns[count] = { headerAlign: "center", header: item.HeaderText, field: item.FieldName, width: item.Width, allowSort: true, dataType: "date", dateFormat: "yyyy-MM-dd", align: "right" };
        } else if (item.Format == "DT") {
            mycolumns[count] = { headerAlign: "center", header: item.HeaderText, field: item.FieldName, width: item.Width, allowSort: true, dataType: "date", dateFormat: "yyyy-MM-dd HH:mm:ss" };
        }
        else if (item.Format == "M") {
            mycolumns[count] = { headerAlign: "center", header: item.HeaderText, field: item.FieldName, width: item.Width, allowSort: true, dataType: "currency", align: "right" };
        }
        else if (item.Format == "N") {
            mycolumns[count] = { headerAlign: "center", header: item.HeaderText, field: item.FieldName, width: item.Width, allowSort: true, dataType: "float", align: "right" };
        }
        else if (item.Format == "B") {
            mycolumns[count] = { type: "checkboxcolumn", headerAlign: "center", header: item.HeaderText, field: item.FieldName, allowSort: true, width: 50 };
        }
        else {
            mycolumns[count] = { headerAlign: "center", header: item.HeaderText, field: item.FieldName, width: item.Width, allowSort: true };
        }
        columns.push(mycolumns[count]);
        count++;
    }
    if (cleardata == undefined) {
        grd.set({
            allowCellSelect: true,
            idField: idfield,
            columns: columns,
            allowSortColumn:false,
            data: null,
            frozenStartColumn: 0,
            frozenEndColumn:3
        });
    }
    else {
        grd.set({
            allowCellSelect: true,
            idField: idfield,
            allowSortColumn: false,
            columns: columns,
            frozenStartColumn: 0,
            frozenEndColumn: 3
        });
    }
    
}

//设置FieldSet自动收缩功能
function toggleFieldSet(ck, id) {
    var dom = document.getElementById(id);
    dom.className = !ck.checked ? "hideFieldset" : "";
    mini.layout();
}

//生成结算单
function CreateBalanceBoot(billNo, typeId) {
    $.ajax({
        url: _Datapath + "EditManager.ashx?method=CreateBalance",
        data: { BillNo: billNo, TypeId: typeId },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.msg.length == 0) {
                showTips("生成成功，已提交财务.");
                return;
            }
            else
            {
                notify("生成失败:"+data.msg);
                return;
            }
           // if (flg[0] == "true") {
           //     mini.confirm("生成成功，是否打开该结算单？", "确定？",
           //       function (action) {
           //           if (action == "ok") {
           //               var languagefolder = getLanguageFolder();
           //               toSendSettleEdit('结算单编辑', languagefolder + 'wms/busi/F_Settle_Bill_Edit.html', flg[1]);
           //           }
           //       }
           //  );
           // } else {
           //     mini.confirm("已生成过结算单，是否打开该结算单？", "确定？",
           //       function (action) {
           //           if (action == "ok") {
           //               var languagefolder = getLanguageFolder();
           //               toSendSettleEdit('结算单编辑', languagefolder + 'wms/busi/F_Settle_Bill_Edit.html', flg[1]);
           //           }
           //       }
           //);
           // }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            _Form.unmask();
            notify("生成失败:" + jqXHR.responseText);
        }
    });
}

function toSendSettleEdit(caption, url, id) {
    
    setCacheArgs({id:id});
    var tab = { name: caption, title: caption, url: url, showCloseButton: true };
    if (top["win"]) {
        top["win"].addTab_Child(tab);
    } else {
        window.open(url.split("/")[url.split("/").length - 1]);
    }
}
//复制新增
function CopyAdd(indxid, codeid, copyAddAction) {
    var c_indx = mini.get(indxid);
    if (c_indx) {
        c_indx.setValue("");
    }
    var c_code = mini.get(codeid);
    if (c_code) {
        c_code.setValue("");
    }
    if (copyAddAction) {
        copyAddAction();
    }
}
//处理列表数据加载是否成功
function handleGridLoadErr(grd) {
    grd.set({
        onload: function (e) {
            var grd = e.sender;
            var result = grd.getResultObject();
            if (isLoginOut(result) == false) {
                if (result.errMessage > '') {
                    notify(result.errMessage);
                }
                else {
                    if (result.data.length >= 100) {
                        grd.set(
                            {
                                virtualScroll:true
                            });
                    }
                }
            }
        }
    });
}
//处理列表为客户端排序
function setGridClientSort(grd) {
    grd.set({
        sortMode: "client"
    });
    for (var i = 0, l = grd.columns.length; i < l; i++) {
        if (grd.columns[i].vtype.indexOf("float") >= 0) {
            grd.columns[i].dataType = "float";
        }
        else if (grd.columns[i].vtype.indexOf("date")>=0) {
            grd.columns[i].dataType = "date";
        }
        grd.columns[i].allowSort = true;
    }
}
//导出列表内容 gridid 列表ID filename 文件名称
function exportGridData(gridid, filename) {
    var grid = mini.get(gridid);
    var columns = grid.getBottomColumns();

    function getColumns(columns) {
        columns = columns.clone();
        for (var i = columns.length - 1; i >= 0; i--) {
            var column = columns[i];
            if (!column.field) {
                columns.removeAt(i);
            } else {
                var c = { header: column.header, field: column.displayField ? column.displayField : column.field };
                columns[i] = c;
            }
        }
        return columns;
    }
    var data = grid.getDataView();
    if (data.length <= 0) {
        notify("没有数据可导出");
        return;
    }
    var columns = getColumns(columns);
    var json = mini.encode(columns);
    var datajson = mini.encode(data);
    document.getElementById("excelColumn").value = json;
    document.getElementById("excelData").value = datajson;
    document.getElementById("excelName").value = filename;
    var excelForm = document.getElementById("excelForm");
    excelForm.submit();
}
function bodyLoading() {
    mini.mask({
        el: document.body,
        cls: 'mini-mask-loading',
        html: 'loading...'
    });
}