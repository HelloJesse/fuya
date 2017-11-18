mini.parse();
var form = new mini.Form("#form1");
var tree_role = mini.get("#tree_role");
tree_role.on("load", function (e) {

    e.sender.findNodes(function (node) {
        if (node.CHECKED == "1") {
            tree_role.checkNode(node);
            return true;
        }
    });
})
var tpwd = "";

loadDropDownData([["USER_TYPE", "UserType"], ["CompanyID", "Company"], ["CustomerID", "Customer"], ["BaseStationID", "BaseStation"]]);

var params = getCacheArgs();
if (params.id) {
    loadForm(params.id);
    tree_role.load(_Datapath + "Main/RoleManager.ashx?method=GetUserRoleTree&userid=" + params.id);
    ///处理不能修改的数据
    mini.get("#CODE").setReadOnly(true);
}
else {

}


//数据保存方法
function DataSave() {
    form.validate();
    //验证数据
    if (form.isValid() == false) {
        var errorTexts = form.getErrorTexts();
        notify(errorTexts);
        return;
    }

    //用户类型 基站 or 客户时，必须选择 所属客户or 所属基站
    var userType = mini.get("#USER_TYPE").getValue();
    if (userType == 4) {
        if (mini.get("#CustomerID").getValue() == "" || mini.get("#CustomerID").getValue() == null) {
            notify(_Mlen_76);
            return;
        }
    } else if (userType == 3) {
        if (mini.get("#BaseStationID").getValue() == "" || mini.get("#BaseStationID").getValue() == null) {
            notify(_Mlen_75);
            return;
        }
    }

   
    form.loading("系统保存中...", "提示信息");
    //end验证数据
    var data = form.getData();      //获取表单多个控件的数据
    var json = mini.encode(data);   //序列化成JSON
    $.ajax({
        url: _Rootpath + "Action.ashx?method=s&id=2",
        type: "post",
        data: { submitData: json },
        success: function (text) {
            data = mini.decode(text);
            if (data.IsOK) {
                // var ids = mini.getData("#ID");
                ids = data.PKValue;
                loadForm(ids);
                showSaveOK();
            }
            else {
                if (isLoginOut(data) == false) {
                    notify("保存失败：" + data.ErrMessage);
                }
            }

        }
    });
    form.unmask();
}



function saveViewRoleID() {
    var userid = mini.get("#ID").value;
    if (userid == "") {
        showTips("请选择用户."); return;
    }
    form.loading("保存拥有角色...", "提示信息");
    $.ajax({
        url: _Datapath + "Main/RoleManager.ashx?method=SaveUserViewRole&userid=" + userid,
        type: 'post',
        data: { submitData: mini.encode({ userid: userid, ids: tree_role.getValue(false) }) },
        success: function (text) {
            data = mini.decode(text);
            form.unmask();
            if (data.IsOK == "1") {
                showSaveOK();

            } else {
                if (isLoginOut(data) == false) {
                    notify("操作失败：" + data.ErrMessage);
                }
            }
        }
    });
    
}

//启用、停用
function UpdateActivedSech(falg) {
    //'act':启用 'disact':'停用'
    var ids = mini.get("#ID").value; //mini.getValue("#ID");
    //ids = ids.substring(0, ids.length - 1);
    $.ajax({
        url: _Datapath + "EditManager.ashx?method=DoActived",
        data: {
            tablename: "Sys_D_USER", tag: falg, ids: ids
        },
        type: 'post',
        success: function (text) {
            var data = mini.decode(text);
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
function Add() {
    EditAddTabs('岗位信息', '岗位信息', 'wms/Main/Sys_D_User_Edit.html');
}

function EditAddTabs(name, title, url) {
    //验证是否需要先保存
    //验证结束
    var tab = { name: name, title: title, url: url, showCloseButton: true };
    if (top["win"]) {
        top["win"].addTab_Child(tab);
    } else {
        window.open(url.split("/")[url.split("/").length - 1]);
    }
}
function onCompanyChanged(e) {
    var companyCombo = mini.get("#CompanyID");
    var deptCombo = mini.get("#DepartmentID");
    deptCombo.setValue("");
    var url = _Datapath +"ComboBoxManager.ashx?tablename=DeptByCompanyID&DataLoad=0&ParamsID=" + companyCombo.getValue()
    deptCombo.setUrl(url);
}
function onCloseClick(e) {
    var obj = e.sender;
    obj.setText("");
    obj.setValue("");
}

//function SetPwd(flag,pass)
//{
//    if (flag == "Decrypt") {
//        $.ajax({
//            url: "../../data/Main/ChangePwd.aspx?method=GetDecryptPwd",
//            data: {
//                pwd: pass
//            },
//            type: 'post',
//            success: function (text) {
//                var data = mini.decode(text);
//                if (data.msg == "true") {
//                    mini.get("#USER_PWD").value = data.pwd;
//                }
//                else {
//                    if (isLoginOut(data) == false) {
//                        mini.alert("密码加载失败：" + data.msg);
//                    }
//                }
//            },
//            error: function (jqXHR, textStatus, errorThrown) {
//                mini.alert("密码加载异常:" + jqXHR.responseText);
//            }
//        });
//    }
//    else if (flag == "Encrypt") {
//        $.ajax({
//            url: "../../data/Main/ChangePwd.aspx?method=GetEncryptPwd",
//            data: {
//                pwd: pass
//            },
//            type: 'post',
//            success: function (text) {
//                var data = mini.decode(text);
//                if (data.msg == "true") {
//                    mini.get("#USER_PWD").value = data.pwd;
//                }
//                else {
//                    if (isLoginOut(data) == false) {
//                        mini.alert("密码加载失败：" + data.msg);
//                    }
//                }
//            },
//            error: function (jqXHR, textStatus, errorThrown) {
//                mini.alert("密码加载异常:" + jqXHR.responseText);
//            }
//        });
//    }
   
//}

function loadForm(id) {
    //加载表单数据
    $.ajax({
        url: _Datapath + "EditManager.ashx?PageID=1&id=" + id,
        type: "post",
        success: function (text) {
            var data = mini.decode(text);   //反序列化成对象
            if (isLoginOut(data) == false) {
                if (data.IsOK) {
                    if (data.BillData.length == 1) {
                        form.setData(data.BillData[0]);             //设置多个控件数据
                        

                    } else {
                        showNoFoundData();
                        return;
                    }
                    //创建按钮
                    createButton("toolbar", data.ButtonData);
                    var companyCombo = mini.get("#CompanyID");
                    var deptCombo = mini.get("#DepartmentID");
                    var url = _Datapath +"ComboBoxManager.ashx?tablename=DeptByCompanyID&DataLoad=0&ParamsID=" + data.BillData[0].CompanyID;
                    deptCombo.setUrl(url);
                    // companyCombo.setValue(data.CompanyID);
                    deptCombo.setValue(data.BillData[0].DepartmentID);
                    //deptCombo.setText(data.DepartmentName);
                } else {
                    notify(data.ErrMessage);
                }
            }
        }
    });

}


///客户下拉按钮变化
function OnClientValueChange(e) {
    var obj = e.sender;
    var ids = obj.getValue();
    $.ajax({
        url: _Datapath + "Main/ChangePwd.aspx?method=GetEncryptPwd",
        data: {
            pwd: ids
        },
        type: 'post',
        success: function (text) {
            var data = mini.decode(text);
            if (data.msg == "true") {
                mini.get("#USER_PWD").setValue(data.pwd);
            }
            else {
                if (isLoginOut(data) == false) {
                    mini.alert("密码加载失败：" + data.msg);
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            mini.alert("密码加载异常:" + jqXHR.responseText);
        }
    });
    
}
