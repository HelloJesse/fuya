mini.parse();

var _SetTimeYG = 200;//读取 ActiveX控件从 C:\TFHandle.txt 获取的摇杆信息
var _TimerYGIntreval = null;

//StartInterval();
//范围内的 
var _InitMinNum = 30767;//摇杆初始值  小
var _InitMaxNum = 34767;//摇杆初始值  大

var _BoolType = false;//判断是否摇杆操作过 上 下
var _BoolTypeLR = false;//判断是否摇杆操作过 左 右

var _TypeNameLR = null;//控制方向 固定翼模式 点击事件
var _TypeIdLR = null;

//摇杆开启关闭 开关
function onChangeYG() {

    //先选择任务
    var row = _GrdList.getSelected();
    if (!row) {
        notify(_Mlen_80);
        return;
    }

    //验证是否已经最总确认 
    if (row.TrackCheckFlag != 1) {
        notify(_Mlen_88);
        return;
    }

    var btnYGVal = $("#btnYG").val();
    if (btnYGVal == "开启摇杆") {//打开

        $("#btnYG").attr("value", "关闭摇杆");
        showCBInfo("飞控摇杆操控已开启");
        StartInterval();
    } else if (btnYGVal == "关闭摇杆") {

        $("#btnYG").attr("value", "开启摇杆");
        showCBInfo("飞控摇杆操控已关闭");
        window.clearInterval(_TimerYGIntreval);
    }
}

function StartInterval() {
    _TimerYGIntreval = window.setInterval("GetYGInfo()", _SetTimeYG);//每_SetTimeYG秒执行一次
}

//每200毫秒读取摇杆操作信息
function GetYGInfo() {
     
    //说明：归位值 32767
    //最小值： 0
    //最大值： 65535

    var axisA = YGObject.AxisA;//速度加减(飞控接口未开发)
    var axisB = YGObject.AxisB;//偏航角ψ（yaw）左飞或右飞
    var axisC = YGObject.AxisC;//滚转角Φ（roll）(飞控接口未开发)
    var axisD = YGObject.AxisD;//爬高或下降/俯仰角
    //暂且只使用 axisB 和 axisD.(axisB可代替页面上的 左飞或右飞) (axisD可代替页面上的 爬高或下降) 

    if (axisB == null || axisD == null) {//摇杆未连接成功
        return;
    }
    //先选择任务
    var row = _GrdList.getSelected();
    if (!row) { return ;}
    if (row.TrackCheckFlag != 1) { return; }

    //俯仰角有变化，需要发送命令
    if (axisD < _InitMinNum || axisD > _InitMaxNum) {
        if (axisD > _InitMaxNum) {//上升

            _TypeName = "升高";
            _TypeId = 8;

        } else if (axisD < _InitMinNum) {//下降

            _TypeName = "下降";
            _TypeId = 16;
        }

        SendFlyOrder(row);
        _BoolType = true;//记录摇杆操作过
    }

    //偏航角有变化，需要发送命令
    if (axisB < _InitMinNum || axisB > _InitMaxNum) {
        if (axisB > _InitMaxNum) {//右飞

            _TypeNameLR = "右飞";
            _TypeIdLR = 2;

        } else if (axisB < _InitMinNum) {//左飞

            _TypeNameLR = "左飞";
            _TypeIdLR = 4;

        }

        SendFlyOrderLR(row);
        _BoolTypeLR = true;//记录摇杆操作过
    }

    //摇杆操作过，回到原位置。需要调用发送 0度方法
    if (_BoolType && axisD > _InitMinNum && axisD < _InitMaxNum) {
        StopSendFlyOrder(row);
        _BoolType = false;//设置原来未操作标识
    }
    //左右 
    if (_BoolTypeLR && axisB > _InitMinNum && axisB < _InitMaxNum) {
        StopSendFlyOrderLR(row);
        _BoolTypeLR = false;//设置原来未操作标识
    }
     
    //头盔视角
    //var pointOfViewCount = YGObject.pointOfViewCount;
    //var pointOfView = YGObject.pointOfView;
    //var buttonsStr = YGObject.buttonsStr;//触发摇杆上的按钮

    //showCBInfo("pointOfViewCount: " + pointOfViewCount);
    //showCBInfo("pointOfView: " + pointOfView);
    //showCBInfo("axisA: " + axisA);
    //showCBInfo("axisB: " + axisB);
    //showCBInfo("axisC: " + axisC);
    //showCBInfo("axisD: " + axisD);
    //showCBInfo("buttonsStr: " + YGObject.buttonsStr);

    //buttonsStr内容 1:False|2:False|3:False|4:False|5:False|6:False|7:False|8:False|9:False|10:False|11:False|12:False|13:False|14:False|15:False
    //|16:False|17:False|18:False|19:False|20:False|21:False|22:False|23:False|24:False|25:False|26:False|27:False|28:False|29:False|30:False|31:False
    //|32:False|33:False|34:False|35:False|36:False|37:False|38:False|39:False|40:False|41:False|42:False|43:False|44:False|45:False|46:False|47:False
    //|48:False|49:False|50:False|51:False|52:False|53:False|54:False|55:False|56:False|57:False|58:False|59:False|60:False|61:False|62:False|63:False
    //|64:False|65:False|66:False|67:False|68:False|69:False|70:False|71:False|72:False|73:False|74:False|75:False|76:False|77:False|78:False|79:False
    //|80:False|81:False|82:False|83:False|84:False|85:False|86:False|87:False|88:False|89:False|90:False|91:False|92:False|93:False|94:False|95:False
    //|96:False|97:False|98:False|99:False|100:False|101:False|102:False|103:False|104:False|105:False|106:False|107:False|108:False|109:False|110:False
    //|111:False|112:False|113:False|114:False|115:False|116:False|117:False|118:False|119:False|120:False|121:False|122:False|123:False|124:False
    //|125:False|126:False|127:False|128:False

    //pointOfView
    //1:-1
}

//给飞机端发送控制命令
function SendFlyOrder(row) {
    //发送命令
    $.ajax({
        url: _Datapath + "Busi/DUavControlCode.aspx?method=onFlyControl",
        data: { ControlNum: _TDCNum, Type: _TypeName, TypeId: _TypeId, TaskID: row.BILLID, UavID: row.UavID },
        type: "post",
        async: false,//同步执行
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                showCBInfo(_TypeName + " " + _TDCNum + "m命令发送成功");
            } else {
                if (isLoginOut(data) == false) {
                    showCBInfo(_TypeName + " " + _TDCNum + "m命令发送失败");
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify(_Mlen_6 + jqXHR.responseText);
        }
    });
}

//给飞机端发送控制命令 左飞 or 右飞
function SendFlyOrderLR(row) {
    //发送命令
    $.ajax({
        url: _Datapath + "Busi/DUavControlCode.aspx?method=onFlyControl",
        data: { ControlNum: _TDCNum, Type: _TypeNameLR, TypeId: _TypeIdLR, TaskID: row.BILLID, UavID: row.UavID },
        type: "post",
        async: false,//同步执行
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                showCBInfo(_TypeNameLR + " " + _TDCNum + "°度命令发送成功");
            } else {
                if (isLoginOut(data) == false) {
                    showCBInfo(_TypeNameLR + " " + _TDCNum + "°度命令发送失败" + "[ERR]: " + data.ErrMessage);
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify(_Mlen_6 + jqXHR.responseText);
        }
    });
}

//给飞机端发送结束控制命令
function StopSendFlyOrder(row) {

    $.ajax({
        url: _Datapath + "Busi/DUavControlCode.aspx?method=onFlyControl",
        data: { ControlNum: _TDCNumZero, Type: _TypeName, TypeId: _TypeId, TaskID: row.BILLID, UavID: row.UavID },
        type: "post",
        async: false,//同步执行
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                showCBInfo(_TypeName + " " + _TDCNumZero + "m命令发送成功");
            } else {
                if (isLoginOut(data) == false) {
                    showCBInfo(_TypeName + " " + _TDCNumZero + "m命令发送失败" + "[ERR]: " + data.ErrMessage);
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify(_Mlen_6 + jqXHR.responseText);
        }
    });
}

//给飞机端发送结束控制命令 左飞 or 右飞
function StopSendFlyOrderLR(row) {

    $.ajax({
        url: _Datapath + "Busi/DUavControlCode.aspx?method=onFlyControl",
        data: { ControlNum: _TDCNumZero, Type: _TypeNameLR, TypeId: _TypeIdLR, TaskID: row.BILLID, UavID: row.UavID },
        type: "post",
        async: false,//同步执行
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                showCBInfo(_TypeNameLR + " " + _TDCNumZero + "°度命令发送成功");
            } else {
                if (isLoginOut(data) == false) {
                    showCBInfo(_TypeNameLR + " " + _TDCNumZero + "°度命令发送失败" + "[ERR]: " + data.ErrMessage);
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify(_Mlen_6 + jqXHR.responseText);
        }
    });
}