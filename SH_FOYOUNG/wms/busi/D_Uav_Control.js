mini.parse();
var _Languagefolder = getLanguageFolder();
var ids = mini.get("#IDS");
var _GrdList = mini.get("grdList");
var _DivFly = mini.get("divFly");//

var _SetTimeGrid = null;//用来设置任务列表定时器时间
var _SetTimeFly = null;//飞机遥测信息
var _TargetGps = null;//船舶跟踪gps 或 其它跟踪目标
var _TargetCycleTimer = null;//目标船舶盘旋
var _TargetDistanceTimer = null;//飞机与船舶距离
var _DepartmentID = 34;//操作部门ID

var dataLatLng = [];
var marker_fly = "";//飞机图标
var addshipOverlay = "";//船舶图标

var _TaskId = null;//任务ID
var _UavID = null;//飞机ID
var _PlaneLine = null;//飞机实际飞行航线对象

var _TimeDC = 200;//每200毫秒发送命标识
var _TDCNum = 5;//每200毫秒发送指定度数标识
var _TDCNumZero = 0;//结束摇杆移动

var _OfftypeBool = false;//判断飞机试飞已起飞 --- 牵扯到模拟测试，暂未使用

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

_GrdList.on("beforeload", function (e) {
    e.data.IDS = ids.getValue();
})

setControlPro("请选择飞行任务，获取遥测信号和飞行按钮权限!");//初始化加载控件不可用

//加载参数
loadDropDownData([["", "SetTimeOut"]], function () {
    var timeOut = getCacheJsObject("SetTimeOut");
    if (timeOut.length > 0) {
        for (var i = 0; i < timeOut.length; i++) {
            if (timeOut[i].ID == 2) { //无人机任务(定时器时间)
                _SetTimeGrid = parseInt(timeOut[i].NAME);

                //默认调用查询方法
                SystemSearch();
                onOpenTimeOut();
            }
            else if (timeOut[i].ID == 3) {//飞机遥测信号(定时器时间)
                _SetTimeFly = parseInt(timeOut[i].NAME);
            }
            else if (timeOut[i].ID == 5) {//船舶跟踪gps
                _TargetGps = parseInt(timeOut[i].NAME);
            }
            else if (timeOut[i].ID == 6) {
                _TargetCycleTimer = parseInt(timeOut[i].NAME);
            }
            else if (timeOut[i].ID == 7) {
                _TargetDistanceTimer = parseInt(timeOut[i].NAME);
            }
        }
    }
});


// 定时查询遥控服务器连接状态 
function setGetServerConnected() {
    window.setInterval(function () {
        getNavCommandServerConnected();
    }, 3000);
}
setGetServerConnected();

//禁用控件
function setControlPro(msg) {
    var upDiv = mini.get("upDiv")
    upDiv.addCls("backColorDiv");
    upDiv.mask({
        cls: 'mini-mask-msg'
    });

    showCBInfo(msg);
}

//取消遮罩
function clearControlPro() {
    var upDiv = mini.get("upDiv")
    upDiv.removeCls("backColorDiv");
    upDiv.unmask();
}

//获取当前登录用户所属部门，用于判断是否有操作全下
if (mini.get("#departmentID").getValue() == "") {
    $.ajax({
        url: _Datapath + "Busi/DUavControlCode.aspx?method=GetNowUserDept",
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.departmentID) {
                mini.get("#departmentID").setValue(data.departmentID);
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {

        }
    });
}

//开启
var _setId;
function onOpenTimeOut() {
    if (_setId) {
        $("#btnCloseTimeOut").removeAttr("style");
        $("#btnOpenTimeOut").attr("style", "display:none");
    }
    _setId = window.setInterval("SystemSearch()", _SetTimeGrid);//每_SetTimeGrid秒调用一次,系统代码设置
}

function onCloseTimeOut() {
    window.clearInterval(_setId);
    $("#btnOpenTimeOut").removeAttr("style");
    $("#btnCloseTimeOut").attr("style", "display:none");
}

//默认调用查询方法
function SystemSearch() {
    ids.setValue("");

    var row = _GrdList.getSelected();
    if (row) {
        _GrdList.load({},function (e) {
            _GrdList.select(row, false);
        });
    } else {
        _GrdList.load();
    }
}

//设置排序
var setSortFlag = false;
_GrdList.set({
    onbeforeload: function (e) {
        var grd = e.sender;
        if (e.data.pageIndex == grd.pageIndex) {
            e.data.SortFlag = 1;
        } else {
            e.data.SortFlag = 0;
        }
    },
    onload: function (e) {
        var grd = e.sender;
        var result = grd.getResultObject();
        if (isLoginOut(result) == false) {
            if (result.errMessage > '') {
                notify(result.errMessage);
            }
            ids.setValue(result["Ids"]);
        }
        if (setSortFlag == false) {
            setSortFlag = true;
            var sorter = new MultiSort(grd);
        }
    }
});

//模式切换
function tabactivechanged(e) {
    if (this.activeIndex == 0 || e.name == "guding") {
        $("#panxuanId").removeAttr("style");
        $("#xuantingId").attr("style", "display:none");
    } else if (this.activeIndex == 1 || e.name == "xuanyi") {
        $("#xuantingId").removeAttr("style");
        $("#panxuanId").attr("style", "display:none");
    }

    mini.layout();
}

//控制方向 固定翼模式 点击事件 
var _TypeName = null;
var _TypeId = null; 

var _setIntervalId;

//控制速度 旋翼模式
var _TypeName_X = null;
var _TypeId_X = null;

var _setIntervalId_X;

//当前模式 输入 or 点击
var _InputPattern = null;// 固定翼模式-操作
var _InputPattern_X = null;// 旋翼模式 -操作
var _PatternIn = "输入模式";
var _PatternClick = "点击模式";


//固定翼模式： 方向控制具体代码
function onControlDirectionCode(Id, type, controlType, row) {
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

    mini.get("#controlId").setValue(_TDCNum);
    //记录方向
    _TypeName = type;
    _TypeId = controlType;

    //发送命令
    $.ajax({
        url: _Datapath + "Busi/DUavControlCode.aspx?method=onFlyControl",
        data: { ControlNum: _TDCNum, Type: _TypeName, TypeId: _TypeId, TaskID: row.BILLID, UavID: row.UavID },
        type: "post",
        async: false,//同步执行
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                //在指令列表中输出信息
                //判断是否为 升高or下降
                if (_TypeId == 8 || _TypeId == 16) {
                    showCBInfo(_TypeName + " " + _TDCNum + "m命令发送成功");
                } else {
                    showCBInfo(_TypeName + " " + _TDCNum + "°度命令发送成功");
                }

            } else {
                if (isLoginOut(data) == false) {
                    if (_TypeId == 8 || _TypeId == 16) {
                        showCBInfo(_TypeName + " " + _TDCNum + "m命令发送失败");
                    } else {
                        showCBInfo(_TypeName + " " + _TDCNum + "°度命令发送失败" + "[ERR]: " + data.ErrMessage);
                    }
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

//四旋翼模式：速度控制
function onControlDirectionCode_X(Id, type, controlType) {
    //$("#plus").removeAttr("style").attr("style", "background-color:green");
    //$("#minus").removeAttr("style").attr("style", "background-color:green");
    //$("#" + Id).removeAttr("style").attr("style", "background-color:red");

    $("#plus").attr("src", _ImageUrl + _Plus_48px);
    $("#minus").attr("src", _ImageUrl + _Minus_48px);
    if (Id == "plus") {
        $("#plus").attr("src", _ImageUrl + _B_Plus_48px);
    } else if (Id == "minus") {
        $("#minus").attr("src", _ImageUrl + _B_Minus_48px);
    }

    if (_TypeName_X == null || _TypeId_X == null) {
        mini.get("#controlId_X").setValue(5);
    } else {
        if (_TypeName_X == type && _TypeId_X == controlType) {
            mini.get("#controlId_X").setValue(parseInt(mini.get("controlId_X").getValue()) + 5);
        } else {
            mini.get("#controlId_X").setValue(5);
        }
    }

    _TypeName_X = type;
    _TypeId_X = controlType;
}

//固定翼模式： 输入模式方向控制具体代码 
function onControlDirectionCodeForPattern(Id, type, controlType) {
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

    _TypeName = type;
    _TypeId = controlType;
}

//四旋翼模式：输入模式速度控制具体代码 
function onControlDirectionCodeForPattern_X(Id, type, controlType) {
    //$("#plus").removeAttr("style").attr("style", "background-color:green");
    //$("#minus").removeAttr("style").attr("style", "background-color:green");
    //$("#" + Id).removeAttr("style").attr("style", "background-color:red");

    $("#plus").attr("src", _ImageUrl + _Plus_48px);
    $("#minus").attr("src", _ImageUrl + _Minus_48px);
    if (Id == "plus") {
        $("#plus").attr("src", _ImageUrl + _B_Plus_48px);
    } else if (Id == "minus") {
        $("#minus").attr("src", _ImageUrl + _B_Minus_48px);
    }

    _TypeName_X = type;
    _TypeId_X = controlType;
}

//输入 or 点击模式切换，用于控制飞控
//patternId: btnInputPattern 固定翼模式操作   btnInputPattern_X 旋翼模式操作
//           controlId 固定翼模式操作值       controlId_X 旋翼模式操作值
function onChangeInputPattern(patternId, controlId) {

    if ($("#" + patternId).val() == "点击模式") {//打开
        $("#" + patternId).attr("value", "输入模式");
        mini.get("#" + controlId).setValue("");
        mini.get("#" + controlId).focus();

        if (patternId == "btnInputPattern") {//固定翼
            _InputPattern = _PatternIn;
        } else {
            _InputPattern_X = _PatternIn;
        }

    } else if ($("#" + patternId).val() == "输入模式") {
        $("#" + patternId).attr("value", "点击模式");
        mini.get("#" + controlId).setValue("移动信息");

        if (patternId == "btnInputPattern") {//固定翼
            _InputPattern = _PatternClick;
        } else {
            _InputPattern_X = _PatternClick;
        }
    }
}

//固定翼模式  
//方向控制:点击事件 发送用户输入的角度，进行发送命令
//        鼠标按住 or 摇杆控制(每200毫秒 发送 左飞、右飞5°命令 或 上升下降5°命令)
//Id: 图片按钮ID
//type:控制类型 升高、左飞、右飞、下降
//controlType:控制命令代码
function onMouseDownControl(Id, type, controlType) {

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
        //后面指定多少毫秒刷新一次
        onControlIdMethod(Id, type, controlType, row);
    }

    //继续巡航按钮可
    setBtnCruise();
}

//四旋翼模式 速度控制 鼠标按住 or 点击事件
function onMouseDownControl_X(Id, type, controlType) {

    //判断当前模式 输入模式
    if (_InputPattern_X != null && _InputPattern_X == _PatternIn) {
        //文本框离开光标事件
        mini.get("#controlId_X").blur();
        if (mini.get("#controlId_X").getValue() == null || mini.get("#controlId_X").getValue() == "") {
            notify(_Mlen_93);
            return;
        }
        onControlDirectionCodeForPattern_X(Id, type, controlType);
    } else {
        //清零
        mini.get("#controlId_X").setValue(0);

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
        onControlIdMethod_X(Id, type, controlType);
    }
}

//固定翼模式  按钮控制方向
function onControlIdMethod(Id, type, controlType, row) {

    //点击首先执行一次
    onControlDirectionCode(Id, type, controlType, row);

    _setIntervalId = window.setInterval(function () {
        onControlDirectionCode(Id, type, controlType, row);
    }, _TimeDC);//每200毫秒调用一次,发送指定方向命令
}

//
function onControlIdMethod_X(Id, type, controlType) {

    //点击首先执行一次
    onControlDirectionCode_X(Id, type, controlType);

    _setIntervalId_X = window.setInterval(function () {
        onControlDirectionCode_X(Id, type, controlType);
    }, 500);//每500毫秒调用一次
}

//固定翼模式： 移除定时器
//发送方向 0 度命令
function onMouseUpControl() {

    var controlNum = 0;

    //判断当前模式 null or 点击模式需要清除 定时器 
    if (_InputPattern == null || _InputPattern == _PatternClick) {
        window.clearInterval(_setIntervalId);

        controlNum = _TDCNumZero;
    } 
    else {
        //输入模式：移动的信息
        controlNum = mini.get("controlId").getValue();
    }

    //获取选择的任务
    var row = _GrdList.getSelected();
    if (!row) {
        notify(_Mlen_80);
        return;
    }

    //调用后台方法
    bodyLoading();
    $.ajax({
        url: _Datapath + "Busi/DUavControlCode.aspx?method=onFlyControl",
        data: { ControlNum: controlNum, Type: _TypeName, TypeId: _TypeId, TaskID: row.BILLID, UavID: row.UavID },
        type: "post",
        async: false,//同步执行
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                //在指令列表中输出信息
                //判断是否为 升高or下降
                if (_TypeId == 8 || _TypeId == 16) {
                    showCBInfo(_TypeName + " " + controlNum + "m命令发送成功");
                } else {
                    showCBInfo(_TypeName + " " + controlNum + "°度命令发送成功");
                }
            } else {
                if (isLoginOut(data) == false) {
                    if (_TypeId == 8 || _TypeId == 16) {
                        showCBInfo(_TypeName + " " + controlNum + "m命令发送失败" + "[ERR]: " + data.ErrMessage);
                    } else {
                        showCBInfo(_TypeName + " " + controlNum + "°度命令发送失败" + "[ERR]: " + data.ErrMessage);
                    }
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
 
//四旋翼模式 移除定时器
function onMouseUpControl_X() {

    //判断当前模式 null or 点击模式需要清除 定时器 
    if (_InputPattern_X == null || _InputPattern_X == _PatternClick) {
        window.clearInterval(_setIntervalId_X);
    }

    //获取选择的任务
    var row = _GrdList.getSelected();
    if (!row) {
        notify(_Mlen_80);
        return;
    }

    //移动的速度信息
    var controlNum = mini.get("controlId_X").getValue();

    //接口未确定 暂不调用接口
    showCBInfo(_TypeName_X + " " + controlNum + "m/s命令操作成功");
}

// 飞机功能按钮控制
// flag:获取控制=0, 起飞=1, 返航=2, 盘旋=3, 悬停=4, 降落=5
// flag:0
// typeName:获取控制
// offtype:0 起飞定高 1：巡航
var _offtype = null;
function onBtnControlType(flag, typeName) {
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

    var num = 0;
    if (flag == 3) {
        mini.prompt("请录入盘旋圈数：", "请输入",
            function (action, value) {
                if (action == "ok") {
                    if (isNaN(value)) {
                        notify(_Mlen_79);
                        return;
                    } 
                    onBtnControlTypeMethod(flag, typeName, value, _offtype, row);
                } else {
                    return
                }
            }
        );
    }else {
        //起飞
        if (flag == 1) {
            _offtype = 0;
        }
        else if (flag == 6) {//巡航
            _offtype = 1;
            flag = 1;
        }
        onBtnControlTypeMethod(flag, typeName, num, _offtype, row);
    }
}

//具体实现内容
function onBtnControlTypeMethod(flag, typeName, num, _offtype, row) {

    bodyLoading();
    $.ajax({
        url: _Datapath + "Busi/DUavControlCode.aspx?method=onBtnControlType",
        data: { Flag: flag, TypeName: typeName, Num: num, TaskID: row.BILLID, UavID: row.UavID, offtype: _offtype, LineHeight: row.LineHeight},
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                if (flag == 3) {
                    showCBInfo(typeName + num + "圈命令发送成功");
                }else if (flag == 2) {

                    //当前用户为操作部，才可进行清除操作
                    if (data.departmentID == _DepartmentID) {
                        //返航：删除定时器 1 2 3 并删除飞行过程中新跟踪的目标    
                        clearTimer1And2And3(row.BILLID);
                    }
                    showCBInfo(typeName + "命令发送成功");
                }else {
                    showCBInfo(typeName + "命令发送成功");
                }

            } else {
                if (isLoginOut(data) == false) {
                    if (flag == 3) {
                        showCBInfo(typeName + num + "圈命令发送失败");
                    } else {
                        showCBInfo(typeName + "命令发送失败" + "[ERR]:" + data.ErrMessage);
                    }
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

//设置背景色
//0 false: 绿色  1 true：红色
function setBackGroudColor(Id, bol) {
    if (!bol) {
        $("#"+ Id).attr("style", "background-color:green;");
    } else {
        $("#"+ Id).attr("style", "background-color:red");
    }
}

//显示飞机任务航线
function onViewFlyLine(e) {
    if (map == null || myApp == null) {
        _GrdList.clearSelect();
        notify(_Mlen_82);
        return;
    }

    var row = _GrdList.getSelected();
    if (!row) {
        notify(_Mlen_78);
        return;
    }
    try {
        showTips("开始接收显示飞机遥测信号,请稍后...");

        bodyLoading();
        $.ajax({
            url: _Datapath + "Busi/DUavControlCode.aspx?method=onViewFlyLine",
            data: { BILLID: row.BILLID, FlyWay: row.FlyWay },
            type: "post",
            success: function (text) {
                var data = mini.decode(text);
                if (data.IsOK) {
                    if (data.departmentID == _DepartmentID) {//当前用户所属操作部,才有权限开启定时器1 和定时器2
                        //启动定时器 时时获取飞机与船舶的距离
                        setTargetDistance(row.BILLID, row.UavID, row.FlyWay);
                        showTimerInfo("定时器1:时时更新[飞机与船舶的距离]已开启");

                        // 启动定时器 时时更新航线和船舶轨迹航线
                        setGetShipSignals(row.BILLID, row.UavID, row.LineHeight, row.FlyWay);
                        showTimerInfo("定时器2:时时更新[跟踪任务航线][新目标航线]已开启");
                    }
                    //选中的某个飞机 启动定时器 时时获取 飞机遥测信息
                    setGetNavFlySignals(row.BILLID, row.UavID);
                    showTimerInfo("定时器4:时时[接收飞机遥测信号]已开启");


                    //获取补点后的飞机规划任务航线，系统生成的航线1 和 航线 8
                    var FlyLine = data.FlyLine;
                    var dataLatLng1 = [];
                    var dataLatLng8 = [];

                    for (var i = 0; i < FlyLine.length; i++) {
                        //joinLine(i, FlyLine[i].Wd, FlyLine[i].Jd);
                        
                        if (FlyLine[i].LineCode == 1) {//航线 1
                            dataLatLng1[FlyLine[i].Sortid - 1] = new shipxyMap.LatLng(FlyLine[i].Wd, FlyLine[i].Jd);
                        } else if (FlyLine[i].LineCode == 8) {//航线 8
                            dataLatLng8[FlyLine[i].Sortid - 1] = new shipxyMap.LatLng(FlyLine[i].Wd, FlyLine[i].Jd);
                        }
                    }
                    if (dataLatLng1 != null && dataLatLng1.length > 0) {
                        addPolyline(dataLatLng1, "任务航线", "0xff3399");
                        dataLatLng1 = [];
                    }
                    if (dataLatLng8 != null && dataLatLng8.length > 0) {
                        addPolyline(dataLatLng8, "返航航线", "0x003300");
                        dataLatLng8 = [];
                    }

                    //获取其它 测试航线 比如 2-5 航线
                    var FlyLineOther = data.FlyLineOther;
                    if (FlyLineOther != null && FlyLineOther.length> 0) {

                        var lCode = 0;
                        for (var i = 0; i < FlyLineOther.length; i++) {
                            if (lCode == 0) {//给当前航线的 LineCode
                                lCode = FlyLineOther[i].LineCode;
                            }
                            if (lCode != FlyLineOther[i].LineCode) {
                                //显示航线
                                addPolyline(dataLatLng, "任务航线" + lCode, "0xff3399");
                                resetLine();

                                //从新赋值
                                lCode = FlyLineOther[i].LineCode;
                            }
                            //读取当前航线
                            joinLine(FlyLineOther[i].Sortid - 1, FlyLineOther[i].Wd, FlyLineOther[i].Jd);
                        }
                        //添加最后一条 测试航线
                        addPolyline(dataLatLng, "任务航线" + lCode, "0xff3399");
                        resetLine();
                    }
                    
                    //.判断如果是测试船则添加船舶图标，否则不需要添加
                    if (row.FlyWay == 0 && data.ShipDt != null && data.ShipDt.length > 0) {
                        var shipInfo = mini.decode(data.ShipDt[0]);
                        if (shipInfo.shipId == "111111111") {//测试船shipId
                            addShip(shipInfo);
                        }
                    }
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

    } catch (err) {
        notify(err.description);
        _GrdList.clearSelect();
    }
    
}

//拼接航线点坐标
function joinLine(options, lat, lng) {
    dataLatLng[options] = new shipxyMap.LatLng(lat, lng);
}

//重置航线坐标点
function resetLine() {
    dataLatLng = [];
}

//添加线
function addPolyline(dataLatLng, flag, colorFlag) {

    /*****线显示样式*****/
    var opts = new shipxyMap.PolylineOptions()
    opts.zoomlevels = [1, 18]; //显示级别
    opts.zIndex = 4; //是否显示label
    opts.isShowLabel = true; //是否显示label
    opts.isEditable = false; //是否可编辑
    /*线样式*/
    //opts.strokeStyle.color = 0xff3399;
    opts.strokeStyle.color = colorFlag;//颜色
    opts.strokeStyle.alpha = 0.8;//透明度
    opts.strokeStyle.thickness = 2;//粗细
    /*标签样式*/
    //标签线条
    opts.labelOptions.border = true; //有边框  
    opts.labelOptions.borderStyle.color = colorFlag;
    opts.labelOptions.borderStyle.alpha = 0.8;
    opts.labelOptions.borderStyle.thickness = 1;
    //标签文字
    opts.labelOptions.fontStyle.name = 'Verdana';
    opts.labelOptions.fontStyle.size = 12;
    opts.labelOptions.fontStyle.color = colorFlag;
    opts.labelOptions.fontStyle.bold = false;  //粗体
    opts.labelOptions.fontStyle.italic = false;  //斜体
    opts.labelOptions.fontStyle.underline = false;  //下划线
    //标签填充
    opts.labelOptions.background = true; //有背景  
    opts.labelOptions.backgroundStyle.color = 0xffccff;  //边框样式
    opts.labelOptions.backgroundStyle.alpha = 06;
    opts.labelOptions.zoomlevels = [1, 18]; //显示级别
    opts.labelOptions.text = flag;
    opts.labelOptions.labelPosition = new shipxyMap.Point(0, 0);
    /*****线*****/
    var polyline = new shipxyMap.Polyline('Polyline' + flag, dataLatLng, opts);
    if (flag == "飞行航线") {//记录这飞行航线 用于后面清除功能
        _PlaneLine = null;
        _PlaneLine = polyline;
    }
    //添加到地图上显示
    map.addOverlay(polyline);
}

//添加船舶
function addShip(ship) {

    if (addshipOverlay) {
        map.removeOverlay(addshipOverlay);
        //map.removeEventListener(addshipOverlay, shipxyMap.Event.CLICK, onShowShipInfo);
        addshipOverlay = null;
    }

    //船舶显示样式
    var option = new shipxyMap.ShipOptions();
    /*边框样式*/
    option.strokeStyle.color = 0xff0000;
    option.strokeStyle.alpha = 1;
    option.strokeStyle.thickness = 2;
    /*填充样式*/
    option.fillStyle.color = 0x00ff00;
    option.fillStyle.alpha = 1;
    /*标签样式*/
    //标签线条
    option.labelOptions.border = true; //有边框  
    option.labelOptions.borderStyle.color = 0x000000;
    option.labelOptions.borderStyle.alpha = 1;
    //标签文字
    option.labelOptions.fontStyle.name = "Verdana";
    option.labelOptions.fontStyle.size = "12";
    option.labelOptions.fontStyle.color = 0x000000;
    option.labelOptions.fontStyle.bold = true;  //粗体
    option.labelOptions.fontStyle.bold = true;  //斜体
    option.labelOptions.fontStyle.underline = true;  //下划线
    //标签填充
    option.labelOptions.background = true; //有背景  
    option.labelOptions.backgroundStyle.color = 0xffff66;  //边框样式
    option.labelOptions.backgroundStyle.alpha = 1;
    option.isShowLabel = true; //是否显示label
    option.isShowMiniTrack = true//船舶自带三分钟轨迹
    option.isSelected = false; //船舶框选
    option.zoomLevels = [1, 18]; //显示级别
    var data = new shipxyAPI.Ship();
    data.shipId = ship.shipId;
    data.name = ship.name;
    data.callsign = ship.callsign;
    data.imo = ship.IMO;
    data.shipType = ship.type;
    data.navStatus = ship.status;
    data.length = ship.length;
    data.beam = ship.beam;
    data.draught = ship.draught
    data.lat = ship.lat;
    data.lng = ship.lng;
    data.heading = ship.heading;
    data.course = ship.course;
    data.speed = ship.speed;
    data.dest = ship.dest;
    data.eta = ship.eta;
    data.lastTime = ship.lastTime;
    addshipOverlay = new shipxyMap.Ship(ship.shipId, data, option);
    map.addOverlay(addshipOverlay, true);

    //map.addEventListener(addshipOverlay, shipxyMap.Event.CLICK, onShowShipInfo);//监听点击事件
}

//添加飞机图标
function addMarker(lat, lng,fagnxiangdu) {

    if (marker_fly) {
        map.removeOverlay(marker_fly);
        marker_fly = null;
    }
    /*****面显示样式*****/
    var opts = new shipxyMap.MarkerOptions()
    opts.zoomlevels = [1, 18]; //显示级别
    opts.zIndex = 4;  //显示层级
    opts.isShowLabel = true; //是否显示label
    opts.isEditable = true; //是否可编辑
    opts.imageUrl = '../../images/uav/plane.png'; //图片URL
    if (fagnxiangdu)
    {
        if (fagnxiangdu <= 22 || fagnxiangdu>=337) {
            opts.imageUrl = '../../images/uav/plane.png'; //图片URL
        }
        else if (fagnxiangdu > 22 && fagnxiangdu <= 67) {
            opts.imageUrl = '../../images/uav/plane7.png'; //图片URL
        }
        else if (fagnxiangdu > 67 && fagnxiangdu <= 112) {
            opts.imageUrl = '../../images/uav/plane2.png'; //图片URL
        }
        else if (fagnxiangdu>112&&fagnxiangdu <= 157)
        {
            opts.imageUrl = '../../images/uav/plane5.png'; //图片URL
        }
        else if (fagnxiangdu > 157 && fagnxiangdu <= 202) {
            opts.imageUrl = '../../images/uav/plane3.png'; //图片URL
        }
        else if (fagnxiangdu > 202 && fagnxiangdu <= 247) {
            opts.imageUrl = '../../images/uav/plane8.png'; //图片URL
        }
        else if (fagnxiangdu > 247 && fagnxiangdu <= 292) {
            opts.imageUrl = '../../images/uav/plane4.png'; //图片URL
        }
        else if (fagnxiangdu > 292 && fagnxiangdu <= 337) {
            opts.imageUrl = '../../images/uav/plane6.png'; //图片URL
        }
    }

    opts.imagePos = new shipxyMap.Point(0, 0); //图片偏移量
    /*标签样式*/
    //标签线条
    opts.labelOptions.border = true; //有边框  
    opts.labelOptions.borderStyle.color = 0xff0000;
    opts.labelOptions.borderStyle.alpha = 0.8;
    opts.labelOptions.borderStyle.thickness = 1;
    //标签文字
    opts.labelOptions.fontStyle.name = 'Verdana';
    opts.labelOptions.fontStyle.size = 12;
    opts.labelOptions.fontStyle.color = 0xff33cc;
    opts.labelOptions.fontStyle.bold = true;  //粗体
    opts.labelOptions.fontStyle.italic = true;  //斜体
    opts.labelOptions.fontStyle.underline = true;  //下划线
    //标签填充
    opts.labelOptions.background = true; //有背景  
    opts.labelOptions.backgroundStyle.color = 0xffccff;  //边框样式
    opts.labelOptions.backgroundStyle.alpha = 0.6;
    opts.labelOptions.zoomlevels = [1, 18]; //显示级别
    //opts.labelOptions.text = '航点' + options;
    opts.labelOptions.text = "";
    opts.labelOptions.labelPosition = new shipxyMap.Point(0, 0);
    /*****点*****/
    var data = [];
  
    data[0] = new shipxyMap.LatLng(lat, lng);
    //DATAEND
    data = new shipxyMap.LatLng(data[0].lat, data[0].lng);
    marker_fly = new shipxyMap.Marker("fly0001", data, opts);

    map.addOverlay(marker_fly, true); //添加到地图上显示 优先显示
}

//设置当前地图加载区域为地图中心点坐标
function getCenter() {
    var center = map.getCenter();
    bodyLoading();
    $.ajax({
        url: _Datapath + "Busi/DUavControlCode.aspx?method=getSeaCenter",
        data: { lat: center.lat, lng: center.lng },
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
}

//执行方法，切换航线航点
function onChangeLine() {

    //获取选择的任务
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

    //必须是巡航任务，才能点击切换航线航点
    if (row.FlyWay != 1) {
        notify(_Mlen_98);
        return;
    }

    var cLine = mini.get("#cLine").getValue();
    var cPoint = mini.get("#cPoint").getValue();

    if (cLine == "" || cPoint == "") {
        notify(_Mlen_89);
        return;
    }

    bodyLoading();
    $.ajax({
        url: _Datapath + "Busi/NavChangeLineCode.aspx?method=updateCheckLine",
        data: { cLine: cLine, cPoint: cPoint,TaskID: row.BILLID, UavID: row.UavID},
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                //var trackCycleFlag = data.trackCycleFlag;//0 飞机飞行状态为目标跟踪  1 飞机飞行状态为目标盘旋  -1正常切入航线航点
                //if (trackCycleFlag == "1") {
                    //关闭定时器3.时时更新[目标船舶盘旋点]
                    //开启定时器1. 和定时器2. 用于关闭 飞行过程中目标跟踪

                //当前用户为操作部，才可进行清除操作
                if (data.departmentID == _DepartmentID) {
                    clearTimer3(row);
                    showTimerInfo("<<切入指定的航线航点--触发定时器>>");
                } else {
                    showCBInfo("<<该用户不是操作部们--目标盘旋触发定时器未停止>>");
                }

                //}
                //提示信息
                showCBInfo("执行切换航线 " + cLine + "航点" + cPoint + "命令发送成功");
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
}

//开启一次指点飞行
//点击一次，只能在海图上添加一个点
function onSetPoint() {
    //验证任务是否选中，并且最终确认
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

    _TaskId = row.BILLID;//任务ID(调用目标经纬度盘旋,可以传任务ID,暂未使用)
    _UavID = row.UavID;

    map.addEventListener(map, shipxyMap.Event.DOUBLE_CLICK, onSetPointEvent);
    //控制按钮只读
    setButtonEnabled(["#btnSetPoint"], false);

    //设置提示 已开启
    mini.get("#btnSetPoint").setText("已开启-指点飞行");

    $('#btnClosePoint').removeAttr("style");

    //继续巡航按钮可
    setBtnCruise();
}

//关闭指点飞行
function closeSetPoint() {
    $('#btnClosePoint').attr("style", "display:none");
    //关闭
    setPointStatus();
}

//添加点验证事件
var onSetPointEvent = function (event) {
    var lat = event.latLng.lat;
    var lng = event.latLng.lng;

    //添加点，验证是否在可点区域内
    //判断是否需要验证
    if (AreaPointCheck == 1) {
        $.ajax({
            url: _Datapath + "Busi/DUavMainTaskCode.aspx?method=CheckOptionsLatLng",
            data: { Lat: lat, Lng: lng },
            type: "post",
            success: function (text) {
                var data = mini.decode(text);
                if (isLoginOut(data) == false) {
                    if (data.IsOK) {
                        addMarkerPoint(lat, lng, "目标点");//验证成功在添加
                    } else {
                        notify(data.ErrMessage);
                    }
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                notify(jqXHR.responseText);
            }
        });
    } else {
        addMarkerPoint(lat, lng, "目标点");//无需验证，直接添加
    }

};

//海图上添加目标点
function addMarkerPoint(lat, lng, options) {

    /*****面显示样式*****/
    var opts = new shipxyMap.MarkerOptions()
    opts.zoomlevels = [1, 18]; //显示级别
    opts.zIndex = 4;  //显示层级
    opts.isShowLabel = true; //是否显示label
    opts.isEditable = false; //是否可编辑
    opts.imageUrl = '../../images/mark.png'; //图片URL
    opts.imagePos = new shipxyMap.Point(0, 0); //图片偏移量
    /*标签样式*/
    //标签线条
    opts.labelOptions.border = true; //有边框  
    opts.labelOptions.borderStyle.color = 0xff0000;
    opts.labelOptions.borderStyle.alpha = 0.8;
    opts.labelOptions.borderStyle.thickness = 1;
    //标签文字
    opts.labelOptions.fontStyle.name = 'Verdana';
    opts.labelOptions.fontStyle.size = 12;
    opts.labelOptions.fontStyle.color = 0x000033;
    opts.labelOptions.fontStyle.bold = true;  //粗体
    opts.labelOptions.fontStyle.italic = false;  //斜体
    opts.labelOptions.fontStyle.underline = false;  //下划线
    //标签填充
    opts.labelOptions.background = true; //有背景  
    opts.labelOptions.backgroundStyle.color = 0xffccff;  //边框样式
    opts.labelOptions.backgroundStyle.alpha = 0.6;
    opts.labelOptions.zoomlevels = [1, 18]; //显示级别
    //opts.labelOptions.text = '航点' + options;
    opts.labelOptions.text = options;
    opts.labelOptions.labelPosition = new shipxyMap.Point(0, 0);
    /*****点*****/
    var data = [];
    var marker = "Marker" + options;
    //DATASTART
    //data[0] = new shipxyMap.LatLng(37.2, 122);
    data[0] = new shipxyMap.LatLng(lat, lng);
    //DATAEND
    data = new shipxyMap.LatLng(data[0].lat, data[0].lng);
    marker = new shipxyMap.Marker(marker, data, opts);
    map.addOverlay(marker, true); //添加到地图上显示 优先显示
    //关闭
    setPointStatus();

    //调用目标点盘旋
    onControlCycleSetPoint(lng, lat);
}

//关闭已开启的指点飞行，删除双击添加点事件
function setPointStatus() {
    //控制按钮只读
    setButtonEnabled(["#btnSetPoint"], true);
    //设置提示 已开启
    mini.get("#btnSetPoint").setText("开启指点飞行");
    //移除点事件
    map.removeEventListener(map, shipxyMap.Event.DOUBLE_CLICK, onSetPointEvent);
}

//调用目标经纬度盘旋
function onControlCycleSetPoint(lng, lat) {
    var num = 3;//默认盘旋 3 圈

    if (lng == null || lat == null || lng == "" || lat == "") {
        notify(_Mlen_97);
        return;
    }

    bodyLoading();
    $.ajax({
        url: _Datapath + "Busi/NavChangeLineCode.aspx?method=updateControlCycle",
        data: { lng: lng, lat: lat, num: num },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                //飞机最新遥测信号
                var flyLng = data.flyLng;
                var flyLat = data.flyLat;
                if (flyLng != null && flyLat != null) {
                    //海图上，显示目标航线
                    joinLine(0, parseFloat(flyLat), parseFloat(flyLng));
                    joinLine(1, lat, lng);
                    addPolyline(dataLatLng, "指点飞行航线", "0x000033");
                    resetLine();
                }

                //显示指令信息
                showCBInfo("执行目标经纬度盘旋 " + num + "圈命令发送成功");
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
}

/*
拨号
no:手机号码
portno:串口号，如1、2、3
*/
function sendNo() {
    var no = mini.get("#no").getValue();
    var portno = mini.get("#portno").getValue();
    if (no == null || no == "") {
        notify(_Mlen_90);
        return;
    }
    if (portno == null || portno == "") {
        notify(_Mlen_91);
        return;
    }
    //返回错误提示
    var result = gsmobject.SendNo(no, portno);
    if (result.length > 0) {
        notify(result);
    }
}

/*
停止呼叫
portno:串口号，如1、2、3
*/
function stop() {
    var portno = mini.get("#portno").getValue();
    if (portno == null || portno == "") {
        notify(_Mlen_91);
        return;
    }

    //返回错误提示
    var result = gsmobject.Stop(portno);
    if (result.length > 0) {
        notify(result);
    }
}

//继续巡航按钮只有在以下模式才可点击
//1:指点飞行
//2:目标跟踪
//3:固定翼默认 飞机进行了方向控制 上下左右飞行后

//默认此按钮不可用
setButtonEnabled(["#btnCruise"], false);

//继续巡航按钮方法
function onConCruise() {

    //验证任务是否选中，并且最终确认
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

    setButtonEnabled(["#btnCruise"], false);

    bodyLoading();
    $.ajax({
        url: _Datapath + "Busi/DUavControlCode.aspx?method=onConCruiseLine",
        data: { TaskId: row.BILLID, UavID: row.UavID },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                //var trackCycleFlag = data.trackCycleFlag;//0 飞机飞行状态为目标跟踪  1 飞机飞行状态为目标盘旋  -1正常切入航线航点
                //if (trackCycleFlag == "1") {

                //关闭定时器3.时时更新[目标船舶盘旋点]
                //开启定时器1. 和定时器2. 用于关闭 飞行过程中目标跟踪

                //当前用户为操作部，才可进行清除操作
                if (data.departmentID == _DepartmentID) {
                    clearTimer3(row);
                    showTimerInfo("<<继续巡航--触发定时器>>");
                } else {
                    showCBInfo("<<该用户不是操作部们--未触发定时器1.-2.>>");
                }

                //}

                //显示指令信息
                showCBInfo("执行继续巡航命令发送成功");
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
}

//继续巡航按钮设为可用
function setBtnCruise() {
    setButtonEnabled(["#btnCruise"], true);
}

//禁止拖动图片，防止事件抬起鼠标事件错误
function setOndragstart() {
    onMouseUpControl();
    return false;
}

//显示指令信息
function showCBInfo(szInfo) {
    showMessageInfo(szInfo, "resultInfo");
}

//显示启动或关闭的的定时器信息
function showTimerInfo(tiInfo) {
    showMessageInfo(tiInfo, "timerInfo");
}

//显示的定时器执行的信息
function showTimerMessageInfo(shInfo) {
    showMessageInfo(shInfo, "mmInfo");
}

//消息统一方法
function showMessageInfo(msg, controlId) {
    var message = $("#" + controlId).val();
    message = message + "\r\n" + "[" + dateFormat(new Date(), "yyyy-MM-dd hh:mm:ss") + "]: " + msg + "...";
    $("#" + controlId).val(message);

    var area = $("#" + controlId);
    area.scrollTop(area[0].scrollHeight - area.height());
}

// 格式化时间
function dateFormat(oDate, fmt) {
    var o = {
        "M+": oDate.getMonth() + 1, //月份
        "d+": oDate.getDate(), //日
        "h+": oDate.getHours(), //小时
        "m+": oDate.getMinutes(), //分
        "s+": oDate.getSeconds(), //秒
        "q+": Math.floor((oDate.getMonth() + 3) / 3), //季度
        "S": oDate.getMilliseconds()//毫秒
    };
    if (/(y+)/.test(fmt)) {
        fmt = fmt.replace(RegExp.$1, (oDate.getFullYear() + "").substr(4 - RegExp.$1.length));
    }
    for (var k in o) {
        if (new RegExp("(" + k + ")").test(fmt)) {
            fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
        }
    }
    return fmt;
}

//清除航迹
function onClearTrack() {
    bodyLoading();
    $.ajax({
        url: _Datapath + "Busi/DUavControlCode.aspx?method=onClearTrack",
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                //清除页面上的航线
                if (_PlaneLine != null) {
                    map.removeOverlay(_PlaneLine);
                    _PlaneLine = null;
                }

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
    })
}

//定点盘旋
function onControlCycle() {
    var lng = mini.get("#lng").getValue();
    var lat = mini.get("#lat").getValue();
    //var num = mini.get("#num").getValue();
    var num = 3;//默认先给 3 圈

    if (lng == null || lat == null || lng == "" || lat == "") {
        notify(_Mlen_97);
        return;
    }
    bodyLoading();
    $.ajax({
        url: _Datapath + "Busi/NavChangeLineCode.aspx?method=updateControlCycle",
        data: { lng: lng, lat: lat, num: num },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {

                //显示指令信息
                showCBInfo("定点盘旋命令发送成功");
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
}