mini.parse();

//控制方向 点击事件 
var _TypeName = null;
var _TypeId = null;

var _setIntervalId;
var _setTimeId;

//当前模式 输入 or 点击
var _InputPattern = null;// 固定翼模式-操作
var _PatternIn = "输入模式";
var _PatternClick = "点击模式";

var _ImageUrl = "../../images/uavNew/";//控制按钮图片路径
var _B_Down_Round = "B_Down_Round.png";
var _B_Left_Round = "B_Left_Round.png";
var _B_Minus_48px = "B_Minus_48px.png";
var _B_Plus_48px = "B_Plus_48px.png";
var _B_Right_Round = "B_Right_Round.png";
var _B_Up_Round = "B_Up_Round.png";

var _Down_Round = "Down_Round.png";
var _Left_Round = "Left_Round.png";
var _Minus_48px = "Minus_48px.png";
var _Plus_48px = "Plus_48px.png";
var _Right_Round = "Right_Round.png";
var _Up_Round = "Up_Round.png";

//固定翼模式： 方向控制具体代码
function onControlDirectionCode(Id, type, controlType) {
    //$("#upId").removeAttr("style").attr("style", "background-color:green");
    //$("#leftId").removeAttr("style").attr("style", "background-color:green");
    //$("#rightId").removeAttr("style").attr("style", "background-color:green");
    //$("#downId").removeAttr("style").attr("style", "background-color:green");
    //$("#" + Id).removeAttr("style").attr("style", "background-color:red");

    $("#upId").attr("src", _ImageUrl + _Up_Round);
    $("#leftId").attr("src", _ImageUrl + _Left_Round);
    $("#rightId").attr("src", _ImageUrl + _Right_Round);
    $("#downId").attr("src", _ImageUrl + _Down_Round);
    if (Id == "upId") {
        $("#upId").attr("src", _ImageUrl + _B_Up_Round);
    } else if (Id == "leftId") {
        $("#leftId").attr("src", _ImageUrl + _B_Left_Round);
    } else if (Id == "rightId") {
        $("#rightId").attr("src", _ImageUrl + _B_Right_Round);
    } else if (Id == "downId") {
        $("#downId").attr("src", _ImageUrl + _B_Down_Round);
    }

    if (_TypeName == null || _TypeId == null) {
        if (controlType == 8 || controlType == 16) {//第一次点击判断是否为 升高 or 下降
            mini.get("#controlId").setValue(5);
        } else {
            mini.get("#controlId").setValue(2);//左飞 or 右飞 
        }
    } else {
        if (_TypeName == type && _TypeId == controlType) {

            //判断是否为 升高or下降
            if (_TypeId == 8 || _TypeId == 16) {
                mini.get("#controlId").setValue(parseInt(mini.get("controlId").getValue()) + 5);
            } else {
                mini.get("#controlId").setValue(parseInt(mini.get("controlId").getValue()) + 2);
            }
        } else {
            if (controlType == 8 || controlType == 16) {//第一次点击判断是否为 升高 or 下降
                mini.get("#controlId").setValue(5);
            } else {
                mini.get("#controlId").setValue(2);//左飞 or 右飞 
            }
        }
    }
    _TypeName = type;
    _TypeId = controlType;
}

//鼠标按住事件
function onMouseDownControl(Id, type, controlType) {
    //检查是否有任务
    if (_TaskID == null || _UavSetID == null) {
        showOPInfo(_Mlen_81); return;
    }

    //判断当前模式 输入模式
    if (_InputPattern != null && _InputPattern == _PatternIn) {
        //文本框离开光标事件
        mini.get("#controlId").blur();
        if (mini.get("#controlId").getValue() == null || mini.get("#controlId").getValue() == "") {
            notify(_Mlen_92);
            return;
        }
        onControlDirectionCodeForPattern(Id, type, controlType);

    } else {
        //清零
        mini.get("#controlId").setValue(0);

        onControlDirectionCode(Id, type, controlType);
        //注册定时器
        _setTimeId = window.setTimeout(function () {
            onControlIdMethod(Id, type, controlType);
        }, 500);
    }

}

//移除定时器
function onMouseUpControl() {
    //检查是否有任务
    if (_TaskID == null || _UavSetID == null) {
       return;
    }

    //判断当前模式 null or 点击模式需要清除 定时器 
    if (_InputPattern == null || _InputPattern == _PatternClick) {
        window.clearTimeout(_setTimeId);
        window.clearInterval(_setIntervalId);
    }

    //移动的信息
    var controlNum = mini.get("controlId").getValue();

    //显示信息
    var resultInfo = "";

    //调用后台方法
    bodyLoading();
    $.ajax({
        url: _Datapath + "Busi/DUavControlCode.aspx?method=onFlyControl",
        data: { ControlNum: controlNum, Type: _TypeName, TypeId: _TypeId, TaskID: _TaskID, UavID: _UavSetID },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                //在指令列表中输出信息
                //判断是否为 升高or下降
                if (_TypeId == 8 || _TypeId == 16) {
                    resultInfo = _TypeName + " " + controlNum + " m 命令发送成功" + "...";
                } else {
                    resultInfo = _TypeName + " " + controlNum + " °度命令发送成功" + "...";
                }

                showOPInfo(resultInfo);
            } else {
                if (isLoginOut(data) == false) {
                    if (_TypeId == 8 || _TypeId == 16) {
                        resultInfo = _TypeName + " " + controlNum + " m 命令发送失败" + "...";
                    } else {
                        resultInfo = _TypeName + " " + controlNum + " °度命令发送失败" + "...";
                    }
                    resultInfo = resultInfo + "\r\n" + "[ERR]: " + data.ErrMessage + "...";
                    showOPInfo(resultInfo);
                }
            }
            mini.unmask();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify(_Mlen_6 + jqXHR.responseText);
            mini.unmask();
        }
    });
}

//
function onControlIdMethod(Id, type, controlType) {
    _setIntervalId = window.setInterval(function () {
        onControlDirectionCode(Id, type, controlType);//直接执行代码
    }, 500);//每500毫秒调用一次
}

//云台控制
//角度标识 变量
var _UpDownIdName = null;//记录上一次上或下 角度名称
var _UpDownIdNum = 0; //记录上一次 上或下 角度值

var _LeftRightIdName = null;//记录上一次 左或右 角度名称
var _LeftRightIdNum = 0; //记录上一次 左或右 角度值

function DoInterface(directionName, controlId)
{
    //检查是否有任务
    if (_TaskID == null || _UavSetID == null) {
        showOPInfo(_Mlen_81); return;
    }
    ////验证是否登陆成功
    //if (_ControlCheck == null) {
    //    showOPInfo(_Mlen_94); return;
    //}

    //获取调整的角度值
    var directionNum = mini.get("#"+ controlId).getValue();

    if (directionName == "上调" || directionName == "下调") {

       if (_UpDownIdName == directionName) {

            //相同角度命令 判断角度值是否有变化
            if(_UpDownIdNum == directionNum){
                notify("同角度，变化相同的角度值为无效命令，操作已返回");
                return;
            } 
        }else {
            
            //命令不同 需验证下降角度值必须为负数， 上升角度值必须为正数
            if (directionName == "下调") {
                if (directionNum > 0) {
                    notify("下调角度，变化的角度必须小于0");
                    return;
                }
            }else if (directionName == "上调") {
                if (directionNum < 0) {
                    notify("上调角度，变化的角度必须大于0");
                    return;
                }
            }
        }
        //正确命令
        _UpDownIdName = directionName;
        _UpDownIdNum =  directionNum;
    } else if (directionName == "左调" || directionName == "右调") {

       if (_LeftRightIdName == directionName) {

            //相同角度命令 判断角度值是否有变化
            if (_LeftRightIdNum == directionNum) {
                notify("同角度，变化相同的角度值为无效命令，操作已返回");
                return;
            }
        } else {

            //命令不同 需验证下降角度值必须为负数， 上升角度值必须为正数
            if (directionName == "左调") {
                if (directionNum > 0) {
                    notify("左调角度，变化的角度必须小于0");
                    return;
                }
            }else if (directionName == "右调") {
                if (directionNum < 0) {
                    notify("右调角度，变化的角度必须大于0");
                    return;
                }
            }
        }
        //正确命令
        _LeftRightIdName = directionName;
        _LeftRightIdNum = directionNum;
    }

    ////累计角度
    //var flag = getDirection(type);

    //显示信息
    var resultInfo = "";
    //调用后台方法
    bodyLoading();
    $.ajax({
        url: _Datapath + "Busi/DUavControlCode.aspx?method=onCameraControl",
        data: { Type: directionName, TaskID: _TaskID, UavID: _UavSetID, Direction: Math.abs(directionNum)},
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                //在指令列表中输出信息
                //判断是否为 升高or下降
                if (directionName == "变倍加" || directionName == "变倍减") {
                    resultInfo = "云台控制" + directionName + ":命令发送成功"+"...";
                } else {
                    resultInfo = "云台控制" + directionName + ":" + directionNum +"°命令发送成功" + "...";
                }

                showOPInfo(resultInfo);
            } else {
                if (isLoginOut(data) == false) {
                    if (directionName == "变倍加" || directionName == "变倍减") {
                        resultInfo = "云台控制" + directionName + ":命令发送失败" + "...";
                    } else {
                        resultInfo = "云台控制" + directionName + ":" + directionNum + "°命令发送失败" + "...";
                    }
                    resultInfo = resultInfo + "\r\n" + "[ERR]: " + data.ErrMessage + "...";
                    showOPInfo(resultInfo);
                }
            }
            mini.unmask();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify(_Mlen_6 + jqXHR.responseText);
            mini.unmask();
        }
    });
}

////累计角度 每次累计加 5度
//function getDirection(type) {
//    if (_DirectionName == type) {
//        _DirectionFlag += 5;
//    } else {
//        _DirectionName = type;
//        _DirectionFlag = 5;
//    }

//    return _DirectionFlag;
//}

//滚动显示到textarea的最后一行的方法
function controlScrollForTextArea() {
    var area = $("#resultInfo");
    area.scrollTop(area[0].scrollHeight - area.height());
}

//输入 or 点击模式切换，用于控制飞控
//patternId: btnInputPattern 固定翼模式操作   btnInputPattern_X 旋翼模式操作
//           controlId 固定翼模式操作值       controlId_X 旋翼模式操作值
function onChangeInputPattern(patternId, controlId) {

    if ($("#" + patternId).val() == "点击模式") {//打开
        $("#" + patternId).attr("value", "输入模式");
        mini.get("#" + controlId).setValue("");
        mini.get("#" + controlId).focus();

        _InputPattern = _PatternIn;

    } else if ($("#" + patternId).val() == "输入模式") {
        $("#" + patternId).attr("value", "点击模式");
        mini.get("#" + controlId).setValue("移动信息");

        _InputPattern = _PatternIn;
    }
}

//固定翼模式： 输入模式方向控制具体代码 
function onControlDirectionCodeForPattern(Id, type, controlType) {
    $("#upId").removeAttr("style").attr("style", "background-color:green");
    $("#leftId").removeAttr("style").attr("style", "background-color:green");
    $("#rightId").removeAttr("style").attr("style", "background-color:green");
    $("#downId").removeAttr("style").attr("style", "background-color:green");
    $("#" + Id).removeAttr("style").attr("style", "background-color:red");

    _TypeName = type;
    _TypeId = controlType;
}

//禁止拖动图片，防止事件抬起鼠标事件错误
function setOndragstart() {
    onMouseUpControl();
    return false;
}




























