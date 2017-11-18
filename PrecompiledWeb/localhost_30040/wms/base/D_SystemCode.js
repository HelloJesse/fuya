
mini.parse();

var pageid = 6;

var _DetID = "";
var _ParentID = "";
var form = new mini.Form("form1");
var grdDetail = mini.get("grdListDetail");
var params = getCacheArgs();
var gridlist = mini.get("grdList");
//gridlist.hideColumn("Activated");
//gridlist.hideColumn("SystemFlag");
gridlist.load();

//加载新增时的按钮
createAddEditFormButton('toolbar', pageid, null);

function ClearData() {
    mini.get("ID").setValue("");
    mini.get("Code").setValue("");
    mini.get("Name").setValue("");
    mini.get("Name_E").setValue("");
    mini.get("Activated").setValue("");
    mini.get("CREATE_NAME").setValue("");
    mini.get("CREATE_DATE").setValue("");
    mini.get("UPDATE_NAME").setValue("");
    mini.get("UPDATE_DATE").setValue("");
    grdDetail.load({ ParentID: 0 });
}

function AddNewData() {
    ClearData();
  
    setButtonEnabled(["#btn_2_Save"], true);
    mini.get("CREATE_NAME").setValue("1");
    mini.get("CREATE_DATE").setValue(new Date());
    mini.get("#Code").setReadOnly(false);
    mini.get("#SystemFlag").setReadOnly(false);
    mini.get("#Activated").setValue("true");
}
//停用 主档和明细档已取停用
function RemoveData() {
    var Ids = "";
    var rows = gridlist.getSelecteds();
    if (rows.length <= 0) {
        return;
    }

    
    mini.confirm("确定删除记录？", "提示",
    function (action) {
        if (action != "ok") {
            return;
        }
        //删除选中的库区或库位信息
        for (var i = 0; i < rows.length; i++) {
            Ids += rows[i]["ID"] + ",";
        }
        Ids = Ids.substring(0, Ids.length - 1);
        $.ajax({
            url: _Datapath + "Base/EditDSystemCode.aspx?method=DeleteSystemInfo",
            data: {
                Ids: Ids
            },
            type: 'post',
            success: function (text) {
                var data = mini.decode(text);
                if (data.msg == "true") {
                    gridlist.reload();
                    grdDetail.reload();
                    ClearData();
                } else {
                    if (isLoginOut(data) == false) {
                        notify("保存失败：" + data.ErrMessage);
                    }
                }
            }
        });
    }
);
}


function addRow() {
    var newRow = { name: "New Row" ,Activated:"1"};
    grdDetail.addRow(newRow, 0);

    grdDetail.beginEditCell(newRow, "Code");
}

function removeRow() {
    var rows = grdDetail.getSelecteds();
    if (rows.length > 0) {
        grdDetail.removeRows(rows, true);
    }
}

function onSelectionChanged(e) {
    var grid1 = e.sender;
    var record = grid1.getSelected();
    if (record) {
        var row = gridlist.getRowByUID(record._uid);
        if (row) {
            var sysFlag = row.SystemFlag;
            var Active = row.Activated;
            if (sysFlag == true || Active==0) {
                //系统代码，不允许修改
                mini.get("btn_2_Save").setEnabled(false);
                mini.get("btn_3_Delete").setEnabled(false);
                mini.get("btnRowAdd").setEnabled(false);
                mini.get("btnRowDel").setEnabled(false);
                mini.get("btnRowSave").setEnabled(false);

            }
            else {
                mini.get("btn_2_Save").setEnabled(true);
                mini.get("btn_3_Delete").setEnabled(true);
                mini.get("btnRowAdd").setEnabled(true);
                mini.get("btnRowDel").setEnabled(true);
                mini.get("btnRowSave").setEnabled(true);
            }
            //保存完以后，会再次进来此处 导致系统代码GRID的编辑行变 待检查
            editRow(row.ID);
                    
            mini.get("#Code").setReadOnly(true);
        }
    }
    else {
        form.reset();
    }
}


function editRow(ID) {
    
    mini.get("#SystemFlag").setReadOnly(true);
    //表单加载员工信息
    _ParentID = ID

    form.loading();
    $.ajax({
        url: _Datapath + "Base/EditDSystemCode.aspx?method=SearchSystemData",
        data: { ID: ID },
        type: "post",
        success: function (text) {
            data = mini.decode(text);
            if (isLoginOut(data) == false) {
                //var o = mini.decode(text);
                form.setData(data);
                form.unmask();

                //加载系统代码明细                    
                grdDetail.load({ ParentID: _ParentID });
            }
        }
    });

    gridlist.doLayout();

}


//检查系统代码的code和name
function checkCodeName() {
    //如果是新增才判断
    var returnMsg = "true";
    var idsT = mini.get("#ID").value;
    if (idsT == null || idsT == "") {
        var codeP = mini.get("#Code").value;
        var nameP = mini.get("#Name").value;

        //验证CODE和NAME是否重复
        $.ajax({
            url: _Datapath + "Base/EditDSystemCode.aspx?method=CheckCodeAndName",
            data: { CODE: codeP, NAME: nameP },
            type: "post",
            async: false,
            success: function (text) {
                var data = mini.decode(text);
                returnMsg = data.msg;
            },
            error: function (jqXHR, textStatus, errorThrown) {
                returnMsg = jqXHR.responseText;
            }
        });

    }
    return returnMsg;
}


//数据保存方法
function DataSave() {
    //var form = new mini.Form("form1");
    form.validate();
    //验证数据
    if (form.isValid() == false) {
        var errorTexts = form.getErrorTexts();
        notify(errorTexts);
        return;
    }

    //判断系统代码中的编码和中文名称是否已经存在
    var remsg = checkCodeName();
    if (remsg != "true") {
        notify(remsg);
        return;
    }


    //end验证数据
    var data = form.getData();      //获取表单多个控件的数据
    var json = mini.encode(data);   //序列化成JSON
    $.ajax({
        url: _Rootpath + "Action.ashx?method=s&id=12",
        type: "post",
        data: { submitData: json },
        success: function (text) {
            data = mini.decode(text);
            if (data.IsOK) {                
                var ids = data.PKValue;
                mini.get("#ID").setValue(ids);
                //editRow(ids);
                gridlist.reload();
                showTips("保存成功");
                      
                //grdDetail.validate();
                //if (grdDetail.isValid() == false) {
                //    //alert("请校验输入单元格内容");
                //    var error = grdDetail.getCellErrors()[0];
                //    grdDetail.beginEditCell(error.record, error.column);
                //    return;
                //}

                ////保存明细
                //var dataDetail = grdDetail.getChanges();
                //var jsonDetail = mini.encode(dataDetail);

                //if (jsonDetail != null) {
                //    $.ajax({
                //        url: "../../data/Base/EditDSystemCode.aspx?method=SaveSystemDtl",
                //        data: { data: jsonDetail, ParentID: ids },
                //        type: "post",
                //        success: function (text) {
                //            if (isLoginOut(text) == false) {
                //                grdDetail.reload();
                //                grid.reload();
                //                //mini.alert("保存成功");
                //                showSaveOK();
                //            }
                //        },
                //        error: function (jqXHR, textStatus, errorThrown) {
                //            notify("保存失败:" + jqXHR.responseText);
                //        }
                //    });
                //}
                                         

            }
            else {
                if (isLoginOut(data) == false) {
                    notify("保存失败：" + data.ErrMessage);
                }
            }
        }
    });
            
}


//保存入库明细
function DataSaveDetail() {
    var ids = mini.get("#ID").value;
    if (ids == "" || ids == "0") {
        notify("请先保存主档");
        return;
    }

    grdDetail.validate();
    if (grdDetail.isValid() == false) {
        //alert("请校验输入单元格内容");
        var error = grdDetail.getCellErrors()[0];
        grdDetail.beginEditCell(error.record, error.column);
        return;
    }

    //保存明细
    var dataDetail = grdDetail.getChanges();
    var jsonDetail = mini.encode(dataDetail);

    if (jsonDetail != null) {
        $.ajax({
            url: _Datapath + "Base/EditDSystemCode.aspx?method=SaveSystemDtl",
            data: { data: jsonDetail, parentID: ids },
            type: "post",
            success: function (text) {
                var data = mini.decode(text);
                if (data.msg == "true") {
                    grdDetail.load({ ParentID: ids });
                    showTips("保存明细成功");

                } else {
                    if (isLoginOut(data) == false) {
                        notify("保存明细失败" + data.msg);
                    }

                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                notify("保存明细异常:" + jqXHR.responseText);
            }
        });
    }

}


//单元格开始编辑事件
function OnCellBeginEdit(e) {
    var grid = e.sender;
    var record = e.record;
    var field = e.field, value = e.value;
    var editor = e.editor;

    
}

//单元格结束编辑
function OnCellCommitEdit(e) {
    var grid = e.sender;
    var record = e.record;
    var field = e.field, value = e.value;

}
