mini.parse();

var pageid = 8;
var form = new mini.Form("#form1");

setAutoCompleteData("COUNTRY_ID", "D_Country");

var params = getCacheArgs();;
if (params.id) {
    loadForm(params.id);
    setReadOnlyText();
} else {
    //加载新增时的按钮
    createAddEditFormButton('toolbar', pageid, AddNewData);
}

function setReadOnlyText() {
    ///处理不能修改的数据
    mini.get("#Code").setReadOnly(true);
}

function onCloseClick(e) {
    var obj = e.sender;
    obj.setText("");
    obj.setValue("");
}

function tabactivechanged(e) {
    // 重新布局，使此TAB页自动调整大小
    mini.layout();
}

//新增
function AddNewData() {
    var form = new mini.Form("#form1");
    form.clear();
    LoadDefault();
    mini.get("#Code").setReadOnly(false);
    mini.get("#LockedFlag").setChecked("false");
    mini.get("#Activated").setValue("true");
}

function SetDefault() {
    var form = new mini.Form("form1");
    var data = form.getData(true, false);
    SetDefaultValue("D_Customer_Edit", data, null);
}

function LoadDefault() {
    var data = getDefaultValue("D_Customer_Edit", null);
    if (data) {
        var form = new mini.Form("form1");
        form.setData(data);
    } else {
        //用户未设定时，程序员在此设置默认值
        mini.get("#Activated").setValue(true);
    }
}

//启用、停用
function UpdateActived(falg) {
    mini.confirm("确定要执行此操作？", "提示",
            function (action) {
                if (action == 'ok') {
                    var ids = mini.get("#ID").value; //mini.getValue("#ID");
                    $.ajax({
                        url: _Datapath + "EditManager.ashx?method=DoActived",
                        data: {
                            tablename: "D_Customer", tag: falg, ids: ids
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
            });
   
}

//加载主表和明细表
function loadForm(id) {
    //加载表单数据
    $.ajax({
        url: _Datapath + "EditManager.ashx?PageID=" + pageid + "&id=" + id,
        type: "post",
        success: function (text) {
            var data = mini.decode(text);   //反序列化成对象
            //form.setData(data);             //设置多个控件数据
            if (isLoginOut(data) == false) {
                if (data.IsOK) {
                    if (data.BillData.length == 1) {
                        form.setData(data.BillData[0]);             //设置多个控件数据
                        //创建按钮
                        createButton("toolbar", data.ButtonData);
                        setButtonStatus();
                    } else {
                        showNoFoundData();
                    }

                } else {
                    notify(data.ErrMessage);
                }
            }
        }
    });
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
    //end验证数据
    var data = form.getData();      //获取表单多个控件的数据
    var json = mini.encode(data);   //序列化成JSON
    $.ajax({
        url: _Rootpath + "Action.ashx?method=s&id=96",
        type: "post",
        data: { submitData: json },
        success: function (text) {
            data = mini.decode(text);
            if (data.IsOK) {
                var ids = data.PKValue;
                mini.get("#ID").setValue(ids);
                loadForm(ids);
                setReadOnlyText();
                showTips("保存成功");
            }
            else {
                if (isLoginOut(data) == false) {
                    notify("保存失败" + data.ErrMessage);
                }
            }
        }
    });
}
//启用、停用
function UpdateActived(falg) {
    var ids = mini.get("#ID").value; 
    if (null == ids || "" == ids) {
        notify(_Mlen_37); return;
    }
    $.ajax({
        url: _Datapath + "EditManager.ashx?method=DoActived",
        data: {
            tablename: "D_Customer", tag: falg, ids: ids
        },
        type: 'post',
        success: function (text) {
            var data = mini.decode(text);
            if (data.iswrong == "0") {
                showTips("操作成功。");
                loadForm(ids);
            } else {
                dataOut = mini.decode(data);
                if (isLoginOut(dataOut) == false) {
                    notify("操作失败：" + data.errmassage);
                }
            }
        }
    });
}


function updateStatus(flag)
{
    mini.confirm("确定要执行此操作？", "提示",
        function (action) {
            if (action == 'ok') {

                var id = mini.get("#ID").value;
                if (id == null || id == "") {
                    notify("没有找到操作的数据");
                    return;
                }

                $.ajax({
                    url: _Datapath + "Base/CustomerManager.aspx?method=UpdateLockStatus",
                    data: { Ids: id, FLAG: flag },
                    type: "post",
                    success: function (text) {
                        var data = mini.decode(text);
                        if (data.msg == "true") {
                            loadForm(id);
                            showTips("操作成功");

                        } else {
                            if (isLoginOut(data) == false) {
                                notify("操作失败:" + data.msg);
                            }
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        notify("操作失败:" + jqXHR.responseText);
                    }
                });
            }
        });
}


//设置按钮状态
function setButtonStatus() {
    //锁定状态不可以修改
    if (mini.get("#LockedFlag").value == "true" || mini.get("#LockedFlag").value == "1") {
        setButtonEnabled(["#btnAddRowGood", '#btnRemoveRowGood', '#btnSaveDataGood', "#btnAddRow", "#btnRemoveRow", "#btnSaveData", "#btn_2_Save", "#btn_10_Disabled", "btn_16_Act", "btn_17_DisAct", "btn_33_Lock"], false);
        //取消锁定 可用
        setButtonEnabled(["btn_37_UnLock"], true);

    }
    else {
        setButtonEnabled(["#btnAddRowGood", '#btnRemoveRowGood', '#btnSaveDataGood', "#btnAddRow", "#btnRemoveRow", "#btnSaveData", "#btn_2_Save", "#btn_10_Disabled", "btn_16_Act", "btn_17_DisAct", "btn_33_Lock"], true);
        //取消锁定 不可用
        setButtonEnabled(["btn_37_UnLock"], false);
    }
}



