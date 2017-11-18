mini.parse();

var _PageID = 9;
var form = new mini.Form("#form1");
var _GrdListPlane = mini.get("grdListPlane");
var _GrdListCustomer = mini.get("grdListCustomer");
var _GrdListPloc= mini.get("grdListPloc");

var LineType;

loadDropDownData([["", "LineType"]],
function () {
    LineType = getCacheJsObject("LineType");
});

setAutoCompleteData("CityID", "D_City");

var params = getCacheArgs();;
if (params.id) {
    loadForm(params.id);

    //加载飞机信息
    _GrdListPlane.load({ MainID: params.id });
    //加载客户信息
    _GrdListCustomer.load({ MainID: params.id });
    //游标
    _GrdListPloc.load({ MainID: params.id });

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
    SetDefaultValue("D_Uav_BaseStation_Edit", data, null);
}

function LoadDefault() {
    var data = getDefaultValue("D_Uav_BaseStation_Edit", null);
    if (data) {
        var form = new mini.Form("form1");
        form.setData(data);
    } else {
        //用户未设定时，程序员在此设置默认值
        mini.get("#Activated").setValue(true);
        mini.get("#BaseEdiCode").setValue(15);
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

function fixed(input, length, chart) {
    var ret = input.slice(0, length);
    for (var i = 0; i < length - ret.length; i++) {
        ret = chart + ret;
    }
    return ret;
}

function formatLat(lat) {
    var latAbs = Math.abs(lat);
    if (latAbs > 90) { return ''; }
    var preNum = Math.floor(latAbs), lastNum = (latAbs - preNum) * 60;
    var ret = preNum + '-' + fixed(lastNum.toFixed(3), 6, '0');
    return lat >= 0 ? ret + 'N' : ret + 'S';
}
//经度格式化为度分：xx-xx.xxxE/W
function formatLng(lng) {
    var lngAbs = Math.abs(lng);
    if (lngAbs > 180) { return ''; }
    var preNum = Math.floor(lngAbs), lastNum = (lngAbs - preNum) * 60;
    var ret = preNum + '-' + fixed(lastNum.toFixed(3), 6, '0');
    return lng >= 0 ? ret + 'E' : ret + 'W';
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

    //格式化经纬度
    var Latitude = mini.get("#Latitude").getValue();
    var Longitude = mini.get("#Longitude").getValue();
    var lat = isNaN(Latitude) ? '' : formatLat(Latitude);
    var lng = isNaN(Longitude) ? '' : formatLng(Longitude);
    
    mini.get("#LatitudeFromat").setValue(lat);
    mini.get("#LongitudeFromat").setValue(lng);

    //end验证数据
    var data = form.getData();      //获取表单多个控件的数据
    var json = mini.encode(data);   //序列化成JSON
    $.ajax({
        url: _Rootpath + "Action.ashx?method=s&id=94",
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
            tablename: "D_Uav_BaseStation", tag: falg, ids: ids
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

function onDateTimeRenderer(e) {
    var value = e.value;
    if (value) return mini.formatDate(value, 'yyyy-MM-dd HH:mm:ss');
    return "";
}


//新增
function addRow() {
    var newRow = { name: "New Row" };
    _GrdListCustomer.addRow(newRow, 0);
    _GrdListCustomer.validateRow(newRow);  //加入新行，马上验证新行
}

//删除
function removeRow() {
    mini.confirm(_Mlen_10, _Mlen_4,
        function (action) {
            if (action == 'ok') {
                var rows = _GrdListCustomer.getSelecteds();
                if (rows.length > 0) {
                    _GrdListCustomer.removeRows(rows, true);
                }
            }
        });
}

//保存客户关系明细信息
function saveData() {
    _GrdListCustomer.validate();
    if (_GrdListCustomer.isValid() == false) {
        var error = _GrdListCustomer.getCellErrors()[0];
        _GrdListCustomer.beginEditCell(error.record, error.column);
        return;
    }

    var Id = mini.get("#ID").getValue();
    if (null == Id || "" == Id) {
        notify(_Mlen_37);
        return;
    }

    var data = _GrdListCustomer.getChanges();
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
        url: _Datapath + "Base/D_Uav_BaseStationCode.aspx?method=UpdateD_Uav_BaseForCustomer",
        data: { data: json, BaseStationID: Id },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (isLoginOut(data) == false) {
                if (data.IsOK) {
                    showTips("保存成功。");
                    //加载客户信息
                    _GrdListCustomer.load({ MainID: Id });
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
//*****************************游标

//新增
function addPlocRow() {
    var newRow = { name: "New Row" };
    _GrdListPloc.addRow(newRow, 0);
    _GrdListPloc.validateRow(newRow);  //加入新行，马上验证新行
}

//删除
function removePlocRow() {
    mini.confirm(_Mlen_10, _Mlen_4,
        function (action) {
            if (action == 'ok') {
                var rows = _GrdListPloc.getSelecteds();
                if (rows.length > 0) {
                    _GrdListPloc.removeRows(rows, true);
                }
            }
        });
}

//保存
function savePlocData() {
    _GrdListPloc.validate();
    if (_GrdListPloc.isValid() == false) {
        var error = _GrdListPloc.getCellErrors()[0];
        _GrdListPloc.beginEditCell(error.record, error.column);
        return;
    }

    var Id = mini.get("#ID").getValue();
    if (null == Id || "" == Id) {
        notify(_Mlen_37);
        return;
    }

    var data = _GrdListPloc.getChanges();
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
        url: _Datapath + "Base/D_Uav_BaseStationCode.aspx?method=UpdateD_Uav_BaseForPloc",
        data: { data: json, BaseStationID: Id },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (isLoginOut(data) == false) {
                if (data.IsOK) {
                    showTips("保存成功。");
                    _GrdListPloc.load({ MainID: Id });
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

