//////////////////////////////////////////////////////////////////////////////////////////
mini.parse();
var tree = mini.get("tree_menu");
var grdList = mini.get("grdList");


//点击树节点事件
var _treeId = null;
var _treePid = null;
var _treeTxt=null;
function onNodeSelect(e) {
    var node = e.node;
    var treePid = node.PID;
    var editflag = node.EDITFLAG;
    if (treePid == -1 || treePid == null) {
        return;
    }
    //获取菜单ID 
    var treeId = node.ID;
    _treeId = treeId;
    _treePid = treePid;
    _treeTxt = node.Name;
    if (editflag == "0") {
        setButtonEnabled(["#btnImportExcel"], false)
    }
    else {
        setButtonEnabled(["#btnImportExcel"], true)
    }

    //需要获取显示已经保存的HelpHtml 内容
    $.ajax({
        url: _Datapath + "Main/RoleManager.ashx?method=GetBaseDataFields",
        data: { ID: _treeId },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (isLoginOut(data) == false) {
                grdList.setData(data);
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify("操作失败:" + jqXHR.responseText);
        }
    });
}
function onCellBeginEdit(e) {
    var record = e.record, field = e.field;
    if (field == "Checked" && (record.ImportFlag == "1"||record.ImportFlag == "10"||record.ImportFlag == "11")) {
        e.cancel = true;    //0--默认，代表可有可无1--代表必有2--代表不需要导入10--代表必须有值11--代表必须唯一
    }
}



function exportExcel() {
    var data = grdList.getData();
    var json = mini.encode({ data: data, tableName: _treeTxt });

    document.getElementById("excelData").value = json;
    var excelForm = document.getElementById("excelForm");
    excelForm.submit();
}

//导入
function importExcel() {
    mini.get("fileUpload").setText("");
    var win = mini.get("winImport");
    win.show();
}
function importOk() {
    var fileupload = mini.get("fileUpload");
    fileupload.startUpload();
}
function importCancel() {
    var win = mini.get("winImport");
    win.hide();
}

function onUploadSuccess(e) {
    $.ajax({
        url: _Datapath + "Upload.aspx?method=ImportBaseDataModuleExcel",
        data: { FileDir: e.serverData },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.msg == "true") {
                var win = mini.get("winImport");
                win.hide();
                showTips("导入成功");
            }
            else {
                if (isLoginOut(data) == false) {
                    notify(data.msg);
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify("文件导入出错:" + jqXHR.responseText);
        }
    });
}
function onUploadError(e) {
    notify("导入文件上传时出错");
}