mini.parse();

var _PageID = 18;
var form = new mini.Form("#form1");
var _GrdList = mini.get("grdList");
var _GrdListLog = mini.get("grdListLog");
var _GrdListLogOrder = mini.get("grdListLogOrder");

loadDropDownData([["TaskStatus", "TaskStatus"], ["FlyWay", "FlyWay"], ["TaskSource", "TaskSource"]]);

setAutoCompleteData("BaseStationID", "D_Uav_BaseStation");
setAutoCompleteData("UavSetID", "D_Uav_UavSet");

var params = getCacheArgs();;
if (params.id) {
    loadForm(params.id);

    //航线航点
    _GrdList.load({ MainID: params.id });
    //点击命令发送日志
    _GrdListLog.load({ MainID: params.id });
    //定时器命令日志
    _GrdListLogOrder.load({ MainID: params.id});

} else {
    //加载新增时的按钮
    createAddEditFormButton('toolbar', _PageID, AddNewData);
}

function onCloseClick(e) {
    var obj = e.sender;
    obj.setText("");
    obj.setValue("");
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

function onDateTimeRenderer(e) {
    var value = e.value;
    if (value) return mini.formatDate(value, 'yyyy-MM-dd HH:mm:ss');
    return "";
}

//新增
function addRow() {
    var newRow = { name: "New Row" };
    _GrdList.addRow(newRow, 0);
    _GrdList.validateRow(newRow);  //加入新行，马上验证新行
}

//删除
function removeRow() {
    mini.confirm(_Mlen_10, _Mlen_4,
        function (action) {
            if (action == 'ok') {
                var rows = _GrdList.getSelecteds();
                if (rows.length > 0) {
                    _GrdList.removeRows(rows, true);
                }
            }
        });
}

//保存
function saveData() {
    _GrdList.validate();
    if (_GrdList.isValid() == false) {
        var error = _GrdList.getCellErrors()[0];
        _GrdList.beginEditCell(error.record, error.column);
        return;
    }

    //获取 任务 和 飞机ID
    var TaskID = mini.get("#BILLID").getValue();
    var UavID = mini.get("#UavSetID").getValue();
    if (TaskID == null || UavID == null) {
        notify(_Mlen_83);
        return;
    }

    var data = _GrdList.getChanges();
    if (data.length == 0) {
        notify(_Mlen_19);
        return;
    }

    var json = mini.encode(data);
    if (json == "") {
        return;
    }
    bodyLoading();

    $.ajax({
        url: _Datapath + "Busi/DUavMainTaskCode.aspx?method=SetD_Uav_MainTaskLine",
        data: { data: json, TaskID: TaskID, UavID: UavID },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (isLoginOut(data) == false) {
                if (data.IsOK) {
                    showTips("保存成功。");
                    _GrdList.load({ MainID: TaskID });
                } else {
                    notify(data.ErrMessage);
                }
            }
            mini.unmask();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify(jqXHR.responseText);
            mini.unmask();
        }
    });
}

//导入航线 航点信息
function importExcel() {
    mini.get("FileUpload").setText("");
    var win = mini.get("winImport");
    win.show();
}

function importOk() {
    var fileupload = mini.get("FileUpload");
    fileupload.startUpload();
}

function importCancel() {
    var win = mini.get("winImport");
    win.hide();
}

function onUploadError(e) {
    notify(_Mlen_40);
}

function onUploadSuccess(e) {
    //上传成功后导入e.serverData
    var TaskID = mini.get("#BILLID").getValue();
    var UavID = mini.get("#UavSetID").getValue();
    if (TaskID == null || UavID == null) {
        notify(_Mlen_83);
        return;
    }

    bodyLoading();
    $.ajax({
        url: _Datapath + "busi/DUavMainTaskCode.aspx?method=ImportD_Uav_FlyLineRecord",
        data: { TaskID: TaskID, UavID: UavID, FileDir: e.serverData },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                var win = mini.get("winImport");
                win.hide();
                showTips(_Mlen_39);
                _GrdList.load({ MainID: TaskID });
            }
            else {
                if (isLoginOut(data) == false) {
                    notify(data.ErrMessage);
                    var win = mini.get("winImport");
                    win.hide();
                }
            }
            mini.unmask();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify(_Mlen_40 + jqXHR.responseText);
            mini.unmask();
        }
    });
}

//航线航点 模板导出
function exportModule() {
    $("#excelmethod").val("DownLoadInExcel");
    var excelForm = document.getElementById("FormDown");
    excelForm.submit();
}

//批量设置航点高度
function setLineHeight() {
    var row = _GrdList.getSelecteds();
    if (row.length <= 0) {
        notify(_Mlen_8);
        return;
    }

    var TaskID = mini.get("#BILLID").getValue();
    if (TaskID == null || TaskID == "") {
        notify(_Mlen_83);
        return;
    }

    var ids = "";
    for (var i = 0; i < row.length; i++) {
        ids += row[i]["ID"] + ",";
    }
    ids = ids.substring(0, ids.length - 1);

    var height = "";
    mini.prompt(_Mlen_84, _Mlen_26,
        function (action, value) {
            if (action == "ok") {
                height = value;
                if (isNaN(height) || "" == height) {
                    notify(_Mlen_85);
                    return;
                }

                bodyLoading();
                $.ajax({
                    url: _Datapath + "Busi/DUavMainTaskCode.aspx?method=UpdateUav_FlyLineRecordHeight",
                    data: { ID: ids, Height: height, TaskID: TaskID },
                    type: "post",
                    success: function (text) {
                        var data = mini.decode(text);
                        if (data.IsOK) {
                            showTips(_Mlen_5);
                            _GrdList.reload();
                        } else {
                            if (isLoginOut(data) == false) {
                                notify(_Mlen_6 + data.ErrMessage);
                            }
                        }
                        mini.unmask();
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        notify(_Mlen_6 + jqXHR.responseText);
                        mini.unmask();
                    }
                });
            } else {
                return;
            }
        }
    );
}

//从新上传航线
function uploadLine() {
    var TaskID = mini.get("#BILLID").getValue();
    var UavID = mini.get("#UavSetID").getValue();
    if (TaskID == null || UavID == null) {
        notify(_Mlen_83);
        return;
    }
    if (_GrdList.data.length <= 0) {
        notify(_Mlen_87);
        return;
    }

    mini.confirm(_Mlen_86, _Mlen_4,
        function (action, value) {
            if (action == "ok") {

                bodyLoading();
                $.ajax({
                    url: _Datapath + "Busi/DUavMainTaskCode.aspx?method=UploadLineRecord",
                    data: { TaskID: TaskID, UavID: UavID },
                    type: "post",
                    success: function (text) {
                        var data = mini.decode(text);
                        if (data.IsOK) {
                            showTips(_Mlen_5);
                        } else {
                            if (isLoginOut(data) == false) {
                                notify(_Mlen_6 + data.ErrMessage);
                            }
                        }
                        mini.unmask();
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        notify(_Mlen_6 + jqXHR.responseText);
                        mini.unmask();
                    }
                });
            } else {
                return;
            }
        }
    );
}

//下载航线列表
function exportListForG() {
    var TaskID = mini.get("#BILLID").getValue();
    if (TaskID == null) {
        notify(_Mlen_83);
        return;
    }

    document.getElementById("ExcelBILLID").value = TaskID;
    var excelForm = document.getElementById("ExcelLineList");
    excelForm.submit();
    showTips("下载中，请稍等...");
}

