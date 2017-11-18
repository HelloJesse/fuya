//角色处理相关方法
mini.parse();
var pageid = 7;     //当前页ID
var base_tree = mini.get("#tree_base");
var grdbasedata = mini.get("#grdbasedata");
var tree_role = mini.get("#tree_role");
var basetableName = "";
var form = new mini.Form("#form1");
var _RoleIDControl = mini.get("#roleID");
var tree_menu = mini.get("#tree_menu");
base_tree.on("nodeclick", function (e) {
    var roleid =_RoleIDControl.value;
    var url = _Datapath + "main/RoleManager.ashx?method=getbasedata&tablekey=" + e.node.TableName + "&roleid=" + roleid;
    grdbasedata.setUrl(url);
    grdbasedata.clearRows();
    grdbasedata.load(null, function () {
        basetableName = e.node.TableName;

        setSelecteds();
        setButtonEnabled(["btn_SaveSearchFunctionPopedom", "btn_saveViewBaseID"], true);
        
        //mini.get("btn_saveViewBaseID").setEnabled(true);
    });

})
tree_role.on("nodeclick", function (e) {
    loadForm(e.node.ID);
    
})


//新增角色
function AddRole() {
    form.reset();
    tree_menu.uncheckAllNodes();//取消选中节点
    //mini.get("btn_saveViewMenu").setEnabled(false);
    setButtonEnabled(["btn_saveViewMenu"], false);
}
//数据保存方法
function DataSave() {
    form.validate();
    //验证数据
    form.loading("系统保存中...", "提示信息");
    if (form.isValid() == false) {
        var errorTexts = form.getErrorTexts();
        mini.alert(errorTexts);
        form.unmask();
        return;
    }
    //end验证数据
    var data = form.getData();      //获取表单多个控件的数据
    var json = mini.encode(data);   //序列化成JSON
    $.ajax({
        url: _Rootpath + "Action.ashx?method=s&id=21",
        type: "post",
        data: { submitData: json },
        success: function (text) {
            data = mini.decode(text);
            if (data.IsOK) {
                // var ids = mini.getData("#ID");
                var ids = mini.get("#roleID").value;
                showTips("保存成功。");
                loadForm(ids);
            }
            else {
                if (isLoginOut(data) == false) {
                    notify("保存失败：" + data.ErrMessage);
                }
            }
            form.unmask();
        }
    });

}
var userpopedomtype = [{ id: 0, text: '无权限' },{ id: 5, text: '所有' }, { id: 1, text: '本人' }, { id: 2, text: '部门' }, { id: 3, text: '公司' }, { id: 4, text: '公司与下属' }];
var companypopedomtype = [{ id: 0, text: '无权限' }, { id: 3, text: '所有' }, { id: 1, text: '本公司' }, { id: 2, text: '公司与下属' }];
tree_menu.on("nodeclick", function (e) {
    var menuid = e.node.IDFlag.substring(1);
    _grdsearchpopedom.loading("正在加载...");
    $.ajax({
        url: _Datapath + "main/RoleManager.ashx?method=GetMenuOpFields",
        type: "post",
        data: { submitData: mini.encode({ MenuID: menuid }) },
        success: function (text) {
            data = mini.decode(text);
            if (data.IsOK) {
                createPopedomGrid(data.Data, menuid);
            }
            else {
                if (isLoginOut(data) == false) {
                    notify("操作失败：" + data.ErrMessage);
                }
            }
            _grdsearchpopedom.unmask();
        }
    });
})
var opfields = null;
var _CurrentMenuID;
var _grdpopedom = mini.get("#grdpopedom");
var _grdsearchpopedom = mini.get("#grdSearchPopedom");
//根据后台数据，创建配置权限列表
function createPopedomGrid(rows, menuid) {

    var mycolumns = new Array(rows.length + 3);
    opfields = new Array(rows.length);
    mycolumns[0] = { type: "indexcolumn", header: "序号", headerAlign: "center" };
    mycolumns[1] = { headerAlign: "center", header: "ID", field: "ID", width: 60 };
    mycolumns[2] = { headerAlign: "center", header: "名称", field: "Name", width: 160 };
    mycolumns[3] = {
        type: "checkboxcolumn", headerAlign: "center", header: "固定人员", field: "FixedFlag", width: 60,
        editor: { type: "checkboxcolumn" }
    };

    for (var i = 0, l = rows.length; i < l; i++) {
        var row = rows[i];
        var comboxdata = "companypopedomtype";
        opfields[i] = row.OPField;

        if (row.IsOP) { comboxdata = "userpopedomtype"; }
            mycolumns[i + 4] = {
                type: "comboboxcolumn", headerAlign: "center", header: row.OPFieldDesc, field: row.OPField, autoShowPopup: "true", width: 160,
                editor: { type: "combobox", style: "width:100%", data: comboxdata }
            };
        
    }

    _grdpopedom.set({
        allowCellSelect: true,
        allowCellEdit: true,
        idField: "ID",
        columns: mycolumns
    });
    var roleid = _RoleIDControl.value;
    _grdpopedom.setUrl(_Datapath + "main/RoleManager.ashx?method=GetMenuSetting");
    _grdpopedom.load(mini.decode({ RoleID: roleid, MenuID: menuid }), function (e) {
        //加载成功，设置当前菜单ID
        _CurrentMenuID = menuid;
    });
    _grdOPUserList.clearRows(); //清除所有行
}
_grdpopedom.on("load", function (e) {
    var datas = mini.decode(e.text);
    _grdpopedom.setData(datas.Function);
    _grdsearchpopedom.setData(datas.Search);
    //设置保存功能项按钮可用
    //var btn_saveFunctionPopedom = mini.get("btn_saveFunctionPopedom");
    //btn_saveFunctionPopedom.setEnabled(true);
    
    setButtonEnabled(["btn_SaveSearchFunctionPopedom", "btn_saveFunctionPopedom"], true);
    
});
var grdUserList = mini.get("#grdUserList");
grdUserList.on("load", function (e) {
    //加载成功
    var data = mini.decode(e.text);
    if (isLoginOut(data) == false) {
        if (data.IsOK == false) {
            notify(data.ErrMessage);
        } else {
            grdUserList.setData(data.Data);
        }
    }
})
//加载用户列表
function loadUserList(roleid) {
    var grdUserList = mini.get("#grdUserList");
    grdUserList.setUrl(_Datapath + "main/RoleManager.ashx?method=GetUserList");
    grdUserList.load(mini.decode({ RoleID: roleid, PageID: pageid }));
    
}
function saveFunctionPopedom() {
    var data = _grdpopedom.data;
    var configs = "";
    for (var i = 0, l = data.length; i < l; i++) {
        var row = data[i];
        for (var f = 0, ll = opfields.length; f < ll;f++) {
            configs = configs + row.ID + "," + opfields[f] + "," + row[opfields[f]] + "," + (row.FixedFlag == true ? "1" : "0");
        }
        configs = configs + ";";
    }
    _grdpopedom.loading("正在保存权限配置....");
    $.ajax({
        url: _Datapath + "main/RoleManager.ashx?method=SaveFunctionSetting" ,
        data: { submitData: mini.encode({ RoleID: _RoleIDControl.value, MenuID: _CurrentMenuID, Config: configs }) },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);   //反序列化成对象
            if (isLoginOut(data) == false) {
                if (data.IsOK == false) {
                    mini.alert(data.ErrMessage);
                } else {
                    _grdpopedom.accept();
                }
                

            }
            _grdpopedom.unmask();
        }
    });
}
function saveSearchFunctionPopedom() {
    var data = _grdsearchpopedom.data;
    var configs = "";
    for (var i = 0, l = data.length; i < l; i++) {
        var row = data[i];
        configs = configs + row.OPField + "," + row.OPType + ";";
    }

    _grdsearchpopedom.loading("正在保存权限配置....");
    $.ajax({
        url: _Datapath + "main/RoleManager.ashx?method=SaveSearchFunctionSetting",
        data: { RoleID: _RoleIDControl.value, MenuID: _CurrentMenuID, Config: configs },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);   //反序列化成对象
            if (isLoginOut(data) == false) {
                if (data.IsOK == false) {
                    mini.alert(data.ErrMessage);
                } else {
                    _grdsearchpopedom.accept();
                }


            }
            _grdsearchpopedom.unmask();
        }
    });
}
//保存角色可见菜单
function saveViewMenu() {
    var viewmenu = tree_menu.getValue();
    tree_menu.loading("正在保存可见菜单....");
    $.ajax({
        url: _Datapath + "main/RoleManager.ashx?method=SaveViewMenu",
        data: { submitData: mini.encode({ RoleID: _RoleIDControl.value, ViewMenu: viewmenu }) },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);   //反序列化成对象
            if (isLoginOut(data) == false) {
                if (data.IsOK == false) {
                    mini.alert(data.ErrMessage);
                } else {
                    tree_menu.accept();
                }


            }
            tree_menu.unmask();
        }
    });
}

//启用、停用
function UpdateActived(falg) {
    //'act':启用 'disact':'停用'
    var ids = mini.get("#roleID").value; //mini.getValue("#ID");
    //ids = ids.substring(0, ids.length - 1);
    $.ajax({
        url: "http://" + location.host + "/data/EditManager.ashx?method=DoActived",
        data: {
            tablename: "Sys_ROLE", tag: falg, ids: ids
        },
        type: 'post',
        dataType: 'json',
        success: function (data) {
            if (data.iswrong == "0") {
                showTips("操作成功。");
                loadForm(ids);
            } else {
                if (isLoginOut(data) == false) {
                    notify("操作失败：" + data.errmassage);
                }
            }
        }
    });
}
//加载角色
function loadForm(id) {
    //加载表单数据
    form.loading("数据正在加载...", "提示信息");
    //设置保存功能项按钮可用
    //var btn_saveFunctionPopedom = mini.get("btn_saveFunctionPopedom");
    //btn_saveFunctionPopedom.setEnabled(false);
    //mini.get("btn_saveViewBaseID").setEnabled(false);
    //mini.get("btn_saveViewMenu").setEnabled(true);

    setButtonEnabled(["btn_saveViewBaseID", "btn_saveFunctionPopedom", "btn_SaveSearchFunctionPopedom"
        , "btnAddUserToRole", "btnRemoveUserToRole"], false);
    setButtonEnabled(["btn_saveViewMenu",
        "btnAddUserToRole", "btnRemoveUserToRole", "btn_saveViewMenu"], true);
    
    $.ajax({
        url: "http://" + location.host + "/data/EditManager.ashx?PageID=7&id=" + id,
        type: "post",
        success: function (text) {
            var data = mini.decode(text);   //反序列化成对象
            if (isLoginOut(data) == false) {
                if (data.BillData.length == 1) {
                    form.setData(data.BillData[0]);             //设置多个控件数据
                } else {
                    showNoFoundData();
                }
                //创建按钮
                createButton("toolbar", data.ButtonData);

                var url = _Datapath + "main/RoleManager.ashx?method=getbasetree&roleid="+_RoleIDControl.value;
                base_tree.load(url);
                url = _Datapath + "main/RoleManager.ashx?method=GetMenuTree&roleid=" + _RoleIDControl.value;
                tree_menu.load(url);
                tree_menu.expandAll();
                tree_menu.setValue(data.BillData[0].ViewMenuID);
                grdbasedata.clearRows();
                //加载角色对应的用户
                loadUserList(_RoleIDControl.value);
                _grdpopedom.clearRows();
                _grdsearchpopedom.clearRows();
            }
            form.unmask();
        }
    });

}
//保存可看ID
function saveBaseViewID() {
    var rows = grdbasedata.getSelecteds();
    var s = "";
    for (var i = 0, l = rows.length; i < l; i++) {
        var row = rows[i];
        s += row.ID;
        if (i != l - 1) s += ",";
    }
    var roleid = mini.get("#roleID").value;

    var submitdata = { RoleID: roleid, BaseTable: basetableName, IDS: s };
    $.ajax({
        url: _Datapath + "main/RoleManager.ashx?method=SaveViewBaseData",
        type: "post",
        data: { submitData: mini.encode(submitdata) },
        success: function (text) {
            data = mini.decode(text);
            if (data.IsOK) {
                // var ids = mini.getData("#ID");
                showTips("保存成功。");
            }
            else {
                if (isLoginOut(data) == false) {
                    notify("保存失败：" + data.ErrMessage);
                }
            }
        }
    });
}//保存可看ID

function getSelecteds() {
    var rows = grdbasedata.getSelecteds();
    var s = "";
    for (var i = 0, l = rows.length; i < l; i++) {
        var row = rows[i];
        s += row.ID;
        if (i != l - 1) s += ",";
    }
    alert(s);
}
function setSelecteds() {

    var rows = grdbasedata.findRows(function (row) {
        if (row.Selected == 1) return true;
        else return false
    });
    grdbasedata.selects(rows);
}
function AddUserToRole() {
    if (_RoleIDControl.value == "") {
        showTips('请选择角色.'); return;
    }
    mini.open({
        url: _Rootpath + "wms/main/RoleAddUser.html",
        title: "添加用户", width: 600, height: 360,
        onload: function () {
            var iframe = this.getIFrameEl();
            var data = { action: "role", id: _RoleIDControl.value };
            iframe.contentWindow.SetData(data);

        },
        ondestroy: function (action) {
            var grdUserList = mini.get("grdUserList");
            grdUserList.reload();

        }
    });
}
function RemoveUserToRole() {
    if (_RoleIDControl.value == "") {
        showTips('请选择角色.'); return;
    }
    var grdUserList=mini.get("grdUserList");
    var userids=GetSelectedIDS('grdUserList','ID');
    if(userids==""){
        showTips('请选择要移除的用户.'); return;
    }
    handing(grdUserList);
    $.ajax({
        url: _Datapath + "main/RoleManager.ashx?method=RemoveUser",
        type: "post",
        data: { RoleID: _RoleIDControl.value, UserIDS: userids },
        success: function (text) {
            grdUserList.unmask();
            data = mini.decode(text);
            if (data.IsOK) {
                grdUserList.reload();
                showTips("移除成功。");
            }
            else {
                if (isLoginOut(data) == false) {
                    notify("移除失败：" + data.ErrMessage);
                }
            }
        }
    });
}
function GetSelectedIDS(grdid,idname){
    var grd= mini.get(grdid);
    var rows = grd.getSelecteds();
    var s = "";
    for (var i = 0, l = rows.length; i < l; i++) {
        var row = rows[i];
        s += row[idname];
        if (i != l - 1) s += ",";
    }
    return s;
}
//设置查询权限设置按钮显示
function onGrdSearchActionRenderer(e) {
    var grid = e.sender;
    var record = e.record;
    var uid = record.OPField;
    var rowIndex = e.rowIndex;

    var s = ' <a class="CellButton" href="javascript:addViewUserID(\'' + uid + '\')" >Add</a>';
    return s;
}
//设置查询权限设置按钮显示
function onGrdOPUserListRenderer(e) {
    var grid = e.sender;
    var record = e.record;
    var userid = record.ID;
    var rowIndex = e.rowIndex;

    var s = ' <a class="CellButton" href="javascript:removeViewUserID(\'' + userid + '\')" > Remove </a>';
    return s;
}

function addViewUserID(opfield) {
    var roleid = _RoleIDControl.value;
    if (roleid == "") {
        showTips('请选择角色.'); return;
    }
    mini.open({
        url: _Rootpath + "wms/main/RoleAddUser.html",
        title: "添加用户", width: 600, height: 360,
        onload: function () {
            var iframe = this.getIFrameEl();
            var data = { action: "addviewuserid", id: _RoleIDControl.value,opfield: opfield,menuid:_CurrentMenuID};
            iframe.contentWindow.SetData(data);

        },
        ondestroy: function (action) {
            _grdOPUserList.reload();

        }
    });
}
function removeViewUserID(userid) {
    var roleid = _RoleIDControl.value;
    if (roleid == "") {
        showTips('请选择角色.'); return;
    }
    //读取当前OPField
    var opfield = _grdsearchpopedom.getSelected().OPField;

    $.ajax({
        url: _Datapath + "main/RoleManager.ashx?method=RemoveViewUserID",
        type: "post",
        data: { RoleID: roleid,OPField:opfield,UserID:userid,MenuID:_CurrentMenuID },
        success: function (text) {
            data = mini.decode(text);
            if (data.IsOK) {
                _grdOPUserList.reload();
                showTips("保存成功。");
            }
            else {
                if (isLoginOut(data) == false) {
                    notify("保存失败：" + data.ErrMessage);
                }
            }
        }
    });
}
var _grdOPUserList = mini.get("grdOPUserList");
_grdOPUserList.on("load", function (e) {
    var data = mini.decode(e.text);
    if (data.IsOK) {
        _grdOPUserList.setData(data.Data);
    }
    else {
        if (isLoginOut(data) == false) {
            notify("加载失败：" + data.ErrMessage);
        }
    }
});
function onSearchPopedomSelectionChanged(e) {
    var roleid = _RoleIDControl.value;
    if (roleid == "") {
        showTips('请选择角色.'); return;
    }
    var grid = e.sender;
    var record = grid.getSelected();
    if (record) {
        
        _grdOPUserList.setUrl(_Datapath + "main/RoleManager.ashx?method=GetOPUserList");
        _grdOPUserList.load({ RoleID: roleid, OPField: record.OPField, MenuID: _CurrentMenuID });
    }
}


function OngrdSearchPopedomCellBeginEdit(e) {
    var grid = e.sender;
    var record = e.record;
    var field = e.field, value = e.value;
    var editor = e.editor;
    if (field == "OPType") {
        var isop = record.IsOP;
        if (isop) {
            if (isop == true ) {
                editor.setData(userpopedomtype);
            } else {
                editor.setData(companypopedomtype);
            }

        } else {
            e.cancel = true;
        }

    }
}
//创建按钮
//CreateSearchButton("toolbar",7);
InitSearchPage('toolbar', 7, null);