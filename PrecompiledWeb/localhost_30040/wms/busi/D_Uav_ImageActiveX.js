mini.parse();

var _SetTimeYGYT = 200;//读取 ActiveX控件从 C:\TFHandle.txt 获取的摇杆信息
var _TimerYGYTIntreval = null;

var _InitNumYT = 32767;//摇杆初始值 
var _DownX = null; //记录同角度命令只发送一次
var _LeftX = null;
var _RightX = null;
var _JiaX = null;
var _JianX = null;

//摇杆开启关闭 开关
function onChangeYG() {

    var btnYGVal = $("#btnYG").val();
    if (btnYGVal == "开启摇杆") {//打开

        $("#btnYG").attr("value", "关闭摇杆");
        showCBInfo("云台摇杆操控已开启");
        StartIntervalYT();
    } else if (btnYGVal == "关闭摇杆") {

        $("#btnYG").attr("value", "开启摇杆");
        showCBInfo("云台摇杆操控已关闭");
        window.clearInterval(_TimerYGYTIntreval);
    }

    //清除
    _DownX = null;
    _LeftX = null;
    _RightX = null;
    _JiaX = null;
    _JianX = null;
}

function StartIntervalYT() {
    _TimerYGYTIntreval = window.setInterval("GetYGInfoYT()", _SetTimeYGYT);//每_SetTimeYGYT秒执行一次
}

//每200毫秒读取摇杆操作信息

function GetYGInfoYT() {

    //说明：归位值 32767
    //最小值： 0
    //最大值： 65535

    var axisA = YGObject.AxisA;//速度加减(飞控接口未开发)
    var axisB = YGObject.AxisB;//变倍加 or 变倍减
    var axisC = YGObject.AxisC;//云台 左 or 右
    var axisD = YGObject.AxisD;//云台 上 or 下

    var directionName = "";//命令名称
    var sVal = 0;

    //判断是否触发 上 or 下
    if (axisD < _InitNumYT) {
        if (null == _DownX || axisD != _DownX) {
            directionName = "下调";
            //计算 sVal 值
            sVal = GetActiveXValue(32767, axisD, 90, directionName);
            SendFlyOrderYT(directionName, sVal);//发送命令
        }
        _DownX = axisD;
    }

    //判断是否触发 左 or 右
    if (axisC != _InitNumYT) {

        if (axisC > _InitNumYT) {//右
            directionName = "右调";
            if (null == _RightX || axisC != _RightX) {
                //计算 sVal 值
                sVal = GetActiveXValue(32767, axisC, 180, directionName);
                SendFlyOrderYT(directionName, sVal);//发送命令
            }
            _RightX = axisC;

        } else if (axisC < _InitNumYT) {//左
            directionName = "左调";
            if (null == _LeftX || axisC != _LeftX) {
                //计算 sVal 值
                sVal = GetActiveXValue(32767, axisC, 180, directionName);
                SendFlyOrderYT(directionName, sVal);//发送命令
            }
            _LeftX = axisC;
        }
    }

    //判断是否触发 变倍加 or 变倍减
    if (axisB != _InitNumYT) {
        //变倍加减暂未有数值   计算 sVal 值
        if (axisB > _InitNumYT) {
            directionName = "变倍加";
            if (null == _JiaX || axisB != _JiaX) {
                sVal = GetActiveXValue(65535, axisB, 18, directionName);
                SendFlyOrderYT(directionName, sVal);//发送命令
            }
            _JiaX = axisB;
        } else if (axisB < _InitNumYT) {
            directionName = "变倍减";
            if (null == _JianX || axisB != _JianX) {
                sVal = GetActiveXValue(32767, axisB, 9, directionName);
                SendFlyOrderYT(directionName, sVal);//发送命令
            }
            _JianX = axisB;
        }
    }
}

//根据摇杆当前数值计算 度数
//maxXVal 摇杆最大值
//axisNum 当前摇杆操作的数值
//rangeNum 范围 (左：180-0，右：0-180) (上下：0-90) (变倍加减：0-18)--变倍暂无度数
//directionName 操控名称
function GetActiveXValue(maxXVal, axisNum, rangeNum, directionName) {

    if (directionName == "左调") {//axisNum 值越小，左调的度数就越大
        var leftRange = Math.round(maxXVal / rangeNum);

        return rangeNum - Math.round(axisNum / leftRange);
    }
    else if (directionName == "右调") {
        var rightRange = Math.round(maxXVal / rangeNum);//一度代表的值

        return Math.round((axisNum - maxXVal) / rightRange);//计算出的度数
    }
    else if (directionName == "下调") {
        var downRange = Math.round(maxXVal / rangeNum);

        return rangeNum- Math.round(axisNum / downRange);
    }
    else if (directionName == "变倍加") {
        var jiaRange = Math.round(maxXVal / rangeNum);

        return  Math.round(axisNum / jiaRange);
    }
    else if (directionName == "变倍减") {
        var jianRange = Math.round(maxXVal / rangeNum);

        return  Math.round(axisNum / jianRange);
    }
}

//发送调控命令
function SendFlyOrderYT(directionName, sVal) {

    //请求发送命令
    $.ajax({
        url: _Datapath + "Busi/DUavControlCode.aspx?method=onCameraControl",
        data: { Type: directionName, TaskID: _TaskID, UavID: _UavSetID, Direction: sVal },
        type: "post",
        async: false,//同步执行
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                //在指令列表中输出信息
                //判断是否为 升高or下降
                //if (directionName == "变倍加" || directionName == "变倍减") {
                //    resultInfo = "云台控制" + directionName + ":命令发送成功" + "...";
                //} else {
                    resultInfo = "云台控制" + directionName + ":" + sVal + "°命令发送成功" + "...";
                //}

                showOPInfo(resultInfo);
            } else {
                if (isLoginOut(data) == false) {
                    //if (directionName == "变倍加" || directionName == "变倍减") {
                    //    resultInfo = "云台控制" + directionName + ":命令发送失败" + "...";
                    //} else {
                        resultInfo = "云台控制" + directionName + ":" + sVal + "°命令发送失败" + "...";
                    //}
                    resultInfo = resultInfo + "\r\n" + "[ERR]: " + data.ErrMessage + "...";
                    showOPInfo(resultInfo);
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify(_Mlen_6 + jqXHR.responseText);
        }
    });
}





