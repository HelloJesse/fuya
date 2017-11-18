
mini.parse();

var pageid = 2;
var form = new mini.Form("#form1");


//设置下拉数据
loadDropDownData([["CompanyID", "Company"]]);



var params = getCacheArgs();
if (params.id) {
    loadForm(params.id);
    ///处理不能修改的数据
    //mini.get("#CODE").setReadOnly(true);
}
else {
    //加载新增时的按钮
    createAddEditFormButton('toolbar', pageid, Add);
}

//新增
function Add() {
    var form = new mini.Form("#form1");
    form.clear();
}

//数据保存方法
function DataSave() {
    form.validate();
    //验证数据
    if (form.isValid() == false) {
        var errorTexts = form.getErrorTexts();
        mini.alert(errorTexts);
        return;
    }
    //end验证数据
    var data = form.getData();      //获取表单多个控件的数据
    var json = mini.encode(data);   //序列化成JSON
    $.ajax({
        url: _Rootpath + "Action.ashx?method=s&id=15",
        type: "post",
        data: { submitData: json },
        success: function (text) {
            data = mini.decode(text);
            if (data.IsOK) {
                var ids = data.PKValue;
                mini.get("#ID").setValue(ids);
                showSaveOK();
            }
            else {
                notify("保存失败" + data.ErrMessage);
            }
        }
    });
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
function onCloseClick(e) {
    var obj = e.sender;
    obj.setText("");
    obj.setValue("");
}
function loadForm(id) {
    //加载表单数据
    $.ajax({
        url: _Datapath + "EditManager.ashx?PageID=" + pageid + "&id=" + id,
        type: "post",
        success: function (text) {
            var data = mini.decode(text);   //反序列化成对象
            form.setData(data.BillData[0]);             //设置多个控件数据
            //创建按钮
            createButton("toolbar", data.ButtonData);
        }
    });
}

//启用、停用
function UpdateActived(falg) {   
    //mini.confirm("确定要执行此操作？", "提示",
            //function (action) {
            //    if (action == 'ok') {
                    //'act':启用 'disact':'停用'
                    var ids = mini.get("#ID").value; //mini.getValue("#ID");
                    //ids = ids.substring(0, ids.length - 1);
                    $.ajax({
                        url: _Datapath + "EditManager.ashx?method=DoActived",
                        data: {
                            tablename: "Sys_D_Department", tag: falg, ids: ids
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
            //    }
            //}
    //);
}