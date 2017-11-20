mini.parse();

var _SysTargetM = 300;//目标距离标识,跟踪目标小于_SysTargetM米后，则切入盘旋指令
var _TargetPointM = 300;//飞机距离当前航线最后一个航点距离标识
var _CurrentLineInfo = null;//状态提醒标识，用于却分是否已经提醒过

var _SysCycle = 3;//目标点盘旋默认 3圈
var _TargetDistanceM = 0;//飞机与跟踪船舶的目标距离

var _OrderTime = false;//停止发送命令，清除定时器标识
var _ServerConnected = false;

//时时[接收飞机遥测信号]
var _setNavFlyId = null;
function setGetNavFlySignals(taskID, uavID) {
    _setNavFlyId = window.setInterval(function () {
        getNavFlySignals(taskID, uavID);
    }, _SetTimeFly);//每_SetTimeFly秒调用一次,系统代码设置 3s
}

//定时器:时时更新[跟踪任务航线][新目标航线][船舶轨迹][新目标轨迹]
var _setShipId = null;
function setGetShipSignals(taskID, uavID, lineHeight, flyWay) {
    _setShipId = window.setInterval(function () {
        getShipSignals(taskID, uavID, lineHeight, flyWay);
    }, _TargetGps);//每_SetTimeFly秒调用一次,系统代码设置 30s
}

//时时获取飞机与船舶距离
var _setTargetDistanceId = null;
function setTargetDistance(taskID, uavID, flyWay) {
    _setTargetDistanceId = window.setInterval(function () {
        getTargetDistance(taskID, uavID, flyWay);
    }, _TargetDistanceTimer);//每_TargetDistanceTimer秒调用一次,系统代码设置 2s
}

//时时更新目标船舶盘旋点
var _setTargetCycleId = null;
function setTargetCycle(taskID, uavID) {
    _setTargetCycleId = window.setInterval(function () {
        getTargetCycle(taskID, uavID);
    }, _TargetCycleTimer);//每_TargetDistanceTimer秒调用一次,系统代码设置 2s
}

//定时器:时时更新[跟踪任务航线][新目标航线][船舶轨迹][新目标轨迹]
function getShipSignals(taskID, uavID, lineHeight, flyWay) {
    $.ajax({
        url: _Datapath + "Busi/DUavMainTaskCode.aspx?method=onGetShipSignals",
        data: { TaskID: taskID, UavID: uavID, LineHeight: lineHeight, FlyWay: flyWay },
        type: "post",
        async: false,//同步执行
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                //更新显示任务航线
                var FlyLine = data.DataList;//任务航线
                if (FlyLine != null && FlyLine.length > 0) {
                    for (var i = 0; i < FlyLine.length; i++) {
                        joinLine(i, FlyLine[i].Wd, FlyLine[i].Jd);
                    }
                    addPolyline(dataLatLng, "任务航线", "0xff3399");
                    resetLine();
                }

                //更新飞行过程中新船舶轨迹
                var FlyLineNew = data.DataListNew;//跟踪航线
                if (FlyLineNew != null && FlyLineNew.length > 0) {
                    for (var i = 0; i < FlyLineNew.length; i++) {
                        joinLine(i, FlyLineNew[i].Wd, FlyLineNew[i].Jd);
                    }
                    addPolyline(dataLatLng, "跟踪航线", "0xFF0000");
                    resetLine();
                }

                //船舶轨迹
                var shipList = data.shipLine;
                if (shipList != null && shipList.length > 0) {
                    for (var i = 0; i < shipList.length; i++) {
                        joinLine(i, shipList[i].lat, shipList[i].lon);
                    }
                    addPolyline(dataLatLng, "船舶轨迹", "0x330033");
                    resetLine();
                }

                //新目标轨迹
                var shipLineNew = data.shipLineNew;
                if (shipLineNew != null && shipLineNew.length > 0) {
                    for (var i = 0; i < shipLineNew.length; i++) {
                        joinLine(i, shipLineNew[i].lat, shipLineNew[i].lon);
                    }
                    addPolyline(dataLatLng, "新目标轨迹", "0x330033");
                    resetLine();
                }

                //.判断如果是测试船则添加船舶图标，否则不需要添加
                if (data.ShipDt != null && data.ShipDt.length > 0) {
                    var shipInfo = mini.decode(data.ShipDt[0]);
                    if (shipInfo.shipId == "111111111") {//测试船shipId
                        addShip(shipInfo);
                    }
                }

                //用于测试，如果是新目标是测试船
                //.判断如果是测试船则添加船舶图标，否则不需要添加
                if (data.ShipDtNew != null && data.ShipDtNew.length > 0) {
                    var shipInfoNew = mini.decode(data.ShipDtNew[0]);
                    if (shipInfoNew.shipId == "111111111") {//测试船shipId
                        addShip(shipInfoNew);
                    }
                }

                showTimerMessageInfo("定时器2:时时刷新飞行航线成功");
            } else {
                if (isLoginOut(data) == false) {
                    showTimerMessageInfo("定时器2:" + data.ErrMessage);
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            showTimerMessageInfo(_Mlen_6 + jqXHR.responseText);
        }
    });
}

//状态提醒
//当前飞机坐标与当前航线最后一个航点坐标距离小于 300M提示
function showTipsForExpand(targetPointM, currentLine, currentPoint) {
    if (targetPointM > 0 && targetPointM <= _TargetPointM) {
        var info = "<b>提示信息</b> <br/>当前航线【" + currentLine + "】,距离末尾点【" + currentPoint + "】不足" + _TargetPointM + "m,请准备下一步操作!" +
            "<br/><table style='width:100%;text-align: right;'><tr><td><button type='button' onclick='closeShowTipsForExpand(" + currentLine + ")' >关闭提醒</button></td></tr></table>"
        mini.showTips({
            content: info,
            state: 'danger',
            x: 'center',
            y: 'top',
            timeout: 2200
        });
    }
}

//关闭提醒 :增加已经提醒过标识
function closeShowTipsForExpand(currentLine) {
    _CurrentLineInfo = currentLine;
}

// 获取当前是否已经连上基站的遥控服务器
function getNavCommandServerConnected() {
    try {
        $.ajax({
            url: _Datapath + "healthcheck.ashx?method=get",
            type: "get",
            async: false,//同步执行
            success: function (text) {
                _ServerConnected = text;
                if (_ServerConnected == "False") {
                    setControlPro("遥控服务未连接，请联系管理员检查服务器!");
                    $("#CommandServerStatus").text("未连接").css("color","red");
                } else {
                    $("#CommandServerStatus").text("已连接").css("color", "green");
                }
            }
        });
    } catch (err) {
        notify(err.description);
        _GrdList.clearSelect();
    }
}

//定时器:时时[接收飞机遥测信号]
function getNavFlySignals(taskID, uavID) {
    // 当遥控服务未连接的时候，不获取遥测信号。
    if (_ServerConnected == "False") return;
    try {
        $.ajax({
            url: _Datapath + "Busi/DUavControlCode.aspx?method=onGetNavFlySignals",
            data: { TaskID: taskID, UavID: uavID },
            type: "post",
            async: false,//同步执行
            success: function (text) {



                var data = mini.decode(text);
                if (data.IsOK) {
                    //飞机状态提醒，如果从未提醒过，或者不是当前航线，则弹出框提醒
                    if (_CurrentLineInfo == null || _CurrentLineInfo != data.currentLine) {
                        //当前用户所属操作部,则进行操作提示
                        if (data.departmentID == _DepartmentID) {
                            showTipsForExpand(data.targetPointM, data.currentLine, data.currentPoint);
                        }
                    }

                    //显示飞机遥测信号数据
                    var dataList = data.dataTable[0];
                    var trackDate = new Date(Date.parse(data.TrackDate.replace(/-/g, "/")));
                    var nowDate = new Date();

                    //如果当前时间大于飞机遥测信号最新时间60秒，则停止发送任何命令
                    if (nowDate.getTime() - 60000 > trackDate.getTime()) {
                        if (!_OrderTime) {
                            _OrderTime = true;
                            if (_setShipId) {
                                window.clearInterval(_setShipId);
                            }
                            if (_setTargetDistanceId) {
                                window.clearInterval(_setTargetDistanceId);
                            }
                            if (_setTargetCycleId) {
                                window.clearInterval(_setTargetCycleId);
                            }

                            showTimerInfo("定时器1:时时更新[飞机与船舶的距离]已关闭");
                            showTimerInfo("定时器2:时时更新[跟踪任务航线][新目标航线]已关闭");
                            showTimerInfo("定时器3:时时更新[目标船舶盘旋点]已关闭");

                            //增加遮罩
                            if ($("#upDiv").attr("class").indexOf("backColorDiv") <= -1) {
                                setControlPro("未接收到最新的遥测信号，关闭控制按钮权限!");
                            }
                        }
                        showCBInfo("当前时间获取不到新的飞机遥测信号,命令已停止!");

                        showTimerMessageInfo("定时器4:未有新遥测信号入库");
                    } else {
                        // 遥控服务必须已经连接上
                        if (_ServerConnected) {
                            if (_OrderTime) {
                                
                                _OrderTime = false;

                                // 启动定时器 时时更新航线和船舶轨迹航线
                                setGetShipSignals(row.BILLID, row.UavID, row.LineHeight, row.FlyWay);
                                showTimerInfo("定时器2:时时更新[跟踪任务航线][新目标航线]已开启");

                                //启动定时器 时时获取飞机与船舶的距离
                                setTargetDistance(row.BILLID, row.UavID, row.FlyWay);
                                showTimerInfo("定时器1:时时更新[飞机与船舶的距离]已开启");

                            }
                            //关闭遮罩
                            if ($("#upDiv").attr("class").indexOf("backColorDiv") > -1) {
                                clearControlPro();
                            }

                            showTimerMessageInfo("定时器4:新遥测信号入库成功");
                        }
                    }

                    //时时显示飞机实际飞行航线
                    if (data.uavTrackLine != null && data.uavTrackLine.length > 0) {
                        var uavTrackLine = data.uavTrackLine;

                        for (var i = 0; i < uavTrackLine.length; i++) {
                            joinLine(i, uavTrackLine[i].Loction_Latitude, uavTrackLine[i].Loction_Longitude);
                        }
                        addPolyline(dataLatLng, "飞行航线", "0x0000FF");
                        resetLine();
                    }
                    //添加飞机图标
                    addMarker(dataList.Loction_Latitude, dataList.Loction_Longitude, dataList.NoseAzimuth);

                    mini.get("#Loction_Longitude").setValue(dataList.Loction_Longitude);
                    mini.get("#Loction_Latitude").setValue(dataList.Loction_Latitude);
                    mini.get("#DistanceCornering").setValue(dataList.DistanceCornering);
                    mini.get("#DistanceTarget").setValue(dataList.DistanceTarget);
                    mini.get("#DistanceOrigin").setValue(dataList.DistanceOrigin);
                    mini.get("#GPS_H").setValue(dataList.GPS_H);
                    mini.get("#GPS_M").setValue(dataList.GPS_M);
                    mini.get("#GPS_S").setValue(dataList.GPS_S);
                    mini.get("#PitchingAngle").setValue(dataList.PitchingAngle);
                    mini.get("#TargetPitchingAngle").setValue(dataList.TargetPitchingAngle);
                    mini.get("#RollAngle").setValue(dataList.RollAngle);
                    mini.get("#TargetRollAngle").setValue(dataList.TargetRollAngle);
                    mini.get("#NoseAzimuth").setValue(dataList.NoseAzimuth);
                    mini.get("#TargetNoseAzimuth").setValue(dataList.TargetNoseAzimuth);
                    mini.get("#GPSTrack").setValue(dataList.GPSTrack);
                    mini.get("#GroundHeight").setValue(dataList.GroundHeight);
                    mini.get("#TargetGroundHeight").setValue(dataList.TargetGroundHeight);
                    mini.get("#GroundSpeed").setValue(dataList.GroundSpeed);
                    mini.get("#AirSpeed").setValue(dataList.AirSpeed);
                    mini.get("#TargetSpeed").setValue(dataList.TargetSpeed);
                    mini.get("#LiftingRate").setValue(dataList.LiftingRate);
                    //状态
                    setBackGroudColor("Alarm_MainV_Low", dataList.Alarm_MainV_Low);
                    setBackGroudColor("Alarm_ServoV_Low", dataList.Alarm_ServoV_Low);
                    setBackGroudColor("Alarm_Electricity_Low", dataList.Alarm_Electricity_Low);

                    setBackGroudColor("Alarm_Status1", dataList.Alarm_Status1);
                    setBackGroudColor("Alarm_AbnormalSpeed", dataList.Alarm_AbnormalSpeed);
                    setBackGroudColor("Alarm_SpaceVelocityAnomaly", dataList.Alarm_SpaceVelocityAnomaly);

                    setBackGroudColor("Alarm_Status2", dataList.Alarm_Status2);
                    //setBackGroudColor("ControlUnlocked", dataList.ControlUnlocked);
                    if (dataList.ControlUnlocked) {
                        mini.get("#ControlUnlocked").setValue("锁定");
                        mini.get("#Standby_isOff").setValue("不可以起飞");
                    } else {
                        mini.get("#ControlUnlocked").setValue("未锁定");
                        mini.get("#Standby_isOff").setValue("可以起飞");
                    }
                    //if (dataList.Standby_isOff) {
                    //    mini.get("#Standby_isOff").setValue("可以起飞");
                    //} else {
                    //    mini.get("#Standby_isOff").setValue("不可以起飞");
                    //}
                    setBackGroudColor("Alarm_GPSLoction_Low", dataList.Alarm_GPSLoction_Low);

                    setBackGroudColor("Alarm_PositionSensor", dataList.Alarm_PositionSensor);
                    setBackGroudColor("Alarm_GrandHigh", dataList.Alarm_GrandHigh);
                    setBackGroudColor("Alarm_BeyondSecurityFence", dataList.Alarm_BeyondSecurityFence);

                    //setBackGroudColor("Alarm_ControlRCDistance", dataList.Alarm_ControlRCDistance);
                    if (dataList.Alarm_ControlRCDistance) {
                        mini.get("#Alarm_ControlRCDistance").setValue("超距");
                    } else {
                        mini.get("#Alarm_ControlRCDistance").setValue("未超距");
                    }
                    setBackGroudColor("Alarm_LiftingSpeed", dataList.Alarm_LiftingSpeed);
                    setBackGroudColor("Alarm_ControlUpward", dataList.Alarm_ControlUpward);

                    setBackGroudColor("Alarm_FlightControlHardware", dataList.Alarm_FlightControlHardware);
                    setBackGroudColor("Switch_GeneratorInterconnection", dataList.Switch_GeneratorInterconnection);
                    setBackGroudColor("Switch_EngineStart", dataList.Switch_EngineStart);
                    if (dataList.VehicleModal == 0) {
                        mini.get("#VehicleModal").setValue("固定翼模式");
                    } else if (dataList.VehicleModal == 2) {
                        mini.get("#VehicleModal").setValue("多旋翼模式");
                    } else if (dataList.VehicleModal == 1) {
                        mini.get("#VehicleModal").setValue("过渡期");
                    }

                } else {
                    if (isLoginOut(data) == false) {
                        //notify(_Mlen_6 + data.ErrMessage);
                        showTimerMessageInfo("定时器4:" + data.ErrMessage);
                    }
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                showTimerMessageInfo(_Mlen_6 + jqXHR.responseText);
            }
        });
    } catch (err) {
        notify(err.description);
        _GrdList.clearSelect();
    }

}

//定时器:时时获取飞机与船舶距离
function getTargetDistance(taskID, uavID, flyWay) {
    $.ajax({
        url: _Datapath + "Busi/DUavControlCode.aspx?method=onGetTargetDistance",
        data: { TaskID: taskID, UavID: uavID, FlyWay:flyWay},
        type: "post",
        async: false,//同步执行
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                var targetDistanceM = data.targetDistanceM;
                //1.与目标距离小于300M，清除刷新【跟踪航线定时器】和【时时获取飞机与船舶距离】
                //2.启动【时时更新目标盘旋点】定时器
                if (targetDistanceM > 0 && targetDistanceM <= _SysTargetM) {
                    clearTimer1And2();

                    //切入盘旋航线，执行以下定时器2的方法，显示船舶最新的gps轨迹信息
                    //(暂未添加)

                    //启动定时器 时时跟新目标船舶盘旋点
                    setTargetCycle(taskID, uavID);
                    showTimerInfo("定时器3:时时更新[目标船舶盘旋点]已开启");
                } else {
                    showTimerMessageInfo("定时器1:时时获取飞机与船舶距离成功");
                }
            } else {
                if (isLoginOut(data) == false) {
                    showTimerMessageInfo("定时器1:" + data.ErrMessage);
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            showTimerMessageInfo(_Mlen_6 + jqXHR.responseText);
        }
    });
}

//时时跟新目标船舶盘旋点
function getTargetCycle(taskID, uavID) {

    $.ajax({
        url: _Datapath + "Busi/DUavControlCode.aspx?method=onGetTargetCycle",
        data: { TaskID: taskID, UavID: uavID, CycleNum : _SysCycle},
        type: "post",
        async: false,//同步执行
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {

                //船舶轨迹
                var shipList = data.shipLine;
                if (shipList != null && shipList.length > 0) {
                    for (var i = 0; i < shipList.length; i++) {
                        joinLine(i, shipList[i].lat, shipList[i].lon);
                    }
                    addPolyline(dataLatLng, "船舶轨迹", "0x330033");
                    resetLine();
                }

                //新目标轨迹
                var shipLineNew = data.shipLineNew;
                if (shipLineNew != null && shipLineNew.length > 0) {
                    for (var i = 0; i < shipLineNew.length; i++) {
                        joinLine(i, shipLineNew[i].lat, shipLineNew[i].lon);
                    }
                    addPolyline(dataLatLng, "新目标轨迹", "0x330033");
                    resetLine();
                }

                //.判断如果是测试船则添加船舶图标，否则不需要添加
                if (data.ShipDt != null && data.ShipDt.length > 0) {
                    var shipInfo = mini.decode(data.ShipDt[0]);
                    if (shipInfo.shipId == "111111111") {//测试船shipId
                        addShip(shipInfo);
                    }
                }

                //用于测试，如果是新目标是测试船
                //.判断如果是测试船则添加船舶图标，否则不需要添加
                if (data.ShipDtNew != null && data.ShipDtNew.length > 0) {
                    var shipInfoNew = mini.decode(data.ShipDtNew[0]);
                    if (shipInfoNew.shipId == "111111111") {//测试船shipId
                        addShip(shipInfoNew);
                    }
                }

                showTimerMessageInfo("定时器3:时时更新目标船舶盘旋点成功");
                showTimerMessageInfo("盘旋经度:" + data.cycleLng + "盘旋纬度:" + data.cycleLat);
            } else {
                if (isLoginOut(data) == false) {
                    showTimerMessageInfo("定时器3:" + data.ErrMessage);
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            showTimerMessageInfo(_Mlen_6 + jqXHR.responseText);
        }
    });
}

//清楚 关闭定时器 1 和定时器 2
function clearTimer1And2() {
    window.clearInterval(_setShipId);
    window.clearInterval(_setTargetDistanceId);

    showTimerInfo("定时器2:时时更新[跟踪任务航线][新目标航线]已关闭");
    showTimerInfo("定时器1:时时更新[飞机与船舶的距离]已关闭");
}

//适用于：切入航线/继续巡航
//关闭定时器3.时时更新[目标船舶盘旋点]
//开启定时器1. 和定时器2. 用于关闭 飞行过程中目标跟踪
function clearTimer3(row) {
    if (_setTargetCycleId) {
        window.clearInterval(_setTargetCycleId);
    }

    //删除掉原先飞行过程中跟踪的目标
    $.ajax({
        url: _Datapath + "Busi/DUavControlCode.aspx?method=delNewTarget",
        data: { TaskID: row.BILLID},
        type: "post",
        async: false,//同步执行
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                showCBInfo("已清理跟踪目标！");
                showTimerInfo("定时器3:时时更新[目标船舶盘旋点]已关闭");
            } else {
                if (isLoginOut(data) == false) {
                    showTimerInfo("定时器3:" + data.ErrMessage);
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            showTimerInfo(_Mlen_6 + jqXHR.responseText);
        }
    });

    //从新启动定时器：
    showTimerInfo("从新启动定时器任务!");
    clearTimer1And2();

    // 启动定时器 时时更新航线和船舶轨迹航线
    setGetShipSignals(row.BILLID, row.UavID, row.LineHeight, row.FlyWay);
    showTimerInfo("定时器2:时时更新[跟踪任务航线][新目标航线]已开启");

    //启动定时器 时时获取飞机与船舶的距离
    setTargetDistance(row.BILLID, row.UavID, row.FlyWay);
    showTimerInfo("定时器1:时时更新[飞机与船舶的距离]已开启");
}

//切入航线：删除定时器 3 开始定时器 1 2 并删除飞行过程中新跟踪的目标
//继续巡航：和切入返航一样


//返航：删除定时器 1 2 3 并删除飞行过程中新跟踪的目标
function clearTimer1And2And3(taskID) {

    window.clearInterval(_setShipId);
    window.clearInterval(_setTargetDistanceId);
    window.clearInterval(_setTargetCycleId);

    //删除掉原先飞行过程中跟踪的目标
    $.ajax({
        url: _Datapath + "Busi/DUavControlCode.aspx?method=delNewTarget",
        data: { TaskID: taskID },
        type: "post",
        async: false,//同步执行
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                showTimerInfo("定时器1:时时更新[飞机与船舶的距离]已关闭");
                showTimerInfo("定时器2:时时更新[跟踪任务航线][新目标航线]已关闭");
                showTimerInfo("定时器3:时时更新[目标船舶盘旋点]已关闭");
            } else {
                if (isLoginOut(data) == false) {
                    showTimerInfo(data.ErrMessage);
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            showTimerInfo(_Mlen_6 + jqXHR.responseText);
        }
    });
}




