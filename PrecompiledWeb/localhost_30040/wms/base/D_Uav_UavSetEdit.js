mini.parse();

var _PageID = 10;
var form = new mini.Form("#form1");

setAutoCompleteData("BaseStationID", "D_Uav_BaseStation");
loadDropDownData([["Status", "UavStatus"], ["UavType", "UavType"]]);

var params = getCacheArgs();;
if (params.id) {
    loadForm(params.id);
    setReadOnlyText();
} else {
    //加载新增时的按钮
    createAddEditFormButton('toolbar', _PageID, AddNewData);
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
    mini.get("#Activated").setValue("true");
}

function SetDefault() {
    var form = new mini.Form("form1");
    var data = form.getData(true, false);
    SetDefaultValue("D_Uav_UavSet_Edit", data, null);
}

function LoadDefault() {
    var data = getDefaultValue("D_Uav_UavSet_Edit", null);
    if (data) {
        var form = new mini.Form("form1");
        form.setData(data);
    } else {
        //用户未设定时，程序员在此设置默认值
        mini.get("#Activated").setValue(true);
    }
}

//加载主表和明细表
function loadForm(id) {
    //加载表单数据
    $.ajax({
        url: _Datapath + "EditManager.ashx?PageID=" + _PageID + "&id=" + id,
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
        url: _Rootpath + "Action.ashx?method=s&id=95",
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
            tablename: "D_Uav_UavSet", tag: falg, ids: ids
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

