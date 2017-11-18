mini.parse();

//记录全局 
var _TaskID = null;
var _UavSetID = null;
var _ErrMsg = null;
var _ControlCheck = null;

// 初始化插件
// 全局保存当前选中窗口
var g_iWndIndex = 0; //可以不用设置这个变量，有窗口参数的接口中，不用传值，开发包会默认使用当前选择窗口
$(function () {
    // 检查插件是否已经安装过
    if (-1 == WebVideoCtrl.I_CheckPluginInstall()) {
        mini.alert("您还未安装过插件，双击开发包目录里的WebComponents.exe安装！否则无法正确获取监控任务");
        return;
    }

    // 初始化插件参数及插入插件
    WebVideoCtrl.I_InitPlugin('100%', '100%', {
        iWndowType: 1,//分屏类型：1- 1*1，2- 2*2，3- 3*3，4- 4*4，默认值为 1，单画面
        cbSelWnd: function (xmlDoc) {
            g_iWndIndex = $(xmlDoc).find("SelectWnd").eq(0).text();
            var szInfo = "当前选择的窗口编号：" + g_iWndIndex;
            showCBInfo(szInfo);
        }
    });
    WebVideoCtrl.I_InsertOBJECTPlugin("divPlugin");

    // 检查插件是否最新
    if (-1 == WebVideoCtrl.I_CheckPluginVersion()) {
        mini.alert("检测到新的插件版本，双击开发包目录里的WebComponents.exe升级！否则无法正确获取监控任务");
        return;
    }

    // 窗口事件绑定
    $(window).bind({
        resize: function () {
            var $Restart = $("#restartDiv");
            if ($Restart.length > 0) {
                var oSize = getWindowSize();
                $Restart.css({
                    width: oSize.width + "px",
                    height: oSize.height + "px"
                });
            }
        }
    });

    //初始化日期时间
    //var szCurTime = dateFormat(new Date(), "yyyy-MM-dd");
    //$("#starttime").val(szCurTime + " 00:00:00");
    //$("#endtime").val(szCurTime + " 23:59:59");

    //默认执行
    autoLogin();
});

//获取登陆的配置信息
function autoLogin() {

    bodyLoading();
    $.ajax({
        url: _Datapath + "Busi/DUavImageCode.aspx?method=getMonitorConfig",
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                //获取成功，直接登陆
                _TaskID = data.TaskID;
                _UavSetID = data.UavSetID;

                //文件保存本地参数
                var dataList = data.dataTable;
                if (dataList != null && dataList.length > 0) {
                    for (var i = 0; i < dataList.length; i++) {
                        mini.get(dataList[i]["Name"]).setValue(dataList[i]["Name_E"]);
                    }
                }
                //登陆
                clickLogin(data.MonitorIP, data.MonitorPort, data.MonitorAc, data.MonitorPass);
                //设置本地参数
                clickSetLocalCfg();
                //获取本地参数
                clickGetLocalCfg();

            } else {
                if (isLoginOut(data) == false) {
                    _ErrMsg = data.ErrMessage;
                    showOPInfo(_ErrMsg);
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

// 设置本地参数
function clickSetLocalCfg() {
    var arrXml = [],
		szInfo = "";

    arrXml.push("<LocalConfigInfo>");
    arrXml.push("<PackgeSize>" + $("#packSize").val() + "</PackgeSize>");
    arrXml.push("<PlayWndType>" + $("#wndSize").val() + "</PlayWndType>");
    arrXml.push("<BuffNumberType>" + $("#netsPreach").val() + "</BuffNumberType>");
    arrXml.push("<RecordPath>" + $("#recordPath").val() + "</RecordPath>");
    arrXml.push("<CapturePath>" + $("#previewPicPath").val() + "</CapturePath>");
    arrXml.push("<PlaybackFilePath>" + $("#playbackFilePath").val() + "</PlaybackFilePath>");
    arrXml.push("<PlaybackPicPath>" + $("#playbackPicPath").val() + "</PlaybackPicPath>");
    arrXml.push("<DownloadPath>" + $("#downloadPath").val() + "</DownloadPath>");
    arrXml.push("<IVSMode>" + $("#rulesInfo").val() + "</IVSMode>");
    arrXml.push("<CaptureFileFormat>" + $("#captureFileFormat").val() + "</CaptureFileFormat>");
    arrXml.push("<ProtocolType>" + $("#protocolType").val() + "</ProtocolType>");
    arrXml.push("</LocalConfigInfo>");

    var iRet = WebVideoCtrl.I_SetLocalCfg(arrXml.join(""));

    if (0 == iRet) {
        szInfo = "本地配置设置成功！";
    } else {
        szInfo = "本地配置设置失败！";
    }
    showOPInfo(szInfo);
}

// 获取本地参数
function clickGetLocalCfg() {
    var xmlDoc = WebVideoCtrl.I_GetLocalCfg();

    $("#netsPreach").val($(xmlDoc).find("BuffNumberType").eq(0).text());
    $("#wndSize").val($(xmlDoc).find("PlayWndType").eq(0).text());
    $("#rulesInfo").val($(xmlDoc).find("IVSMode").eq(0).text());
    $("#captureFileFormat").val($(xmlDoc).find("CaptureFileFormat").eq(0).text());
    $("#packSize").val($(xmlDoc).find("PackgeSize").eq(0).text());
    $("#recordPath").val($(xmlDoc).find("RecordPath").eq(0).text());
    $("#downloadPath").val($(xmlDoc).find("DownloadPath").eq(0).text());
    $("#previewPicPath").val($(xmlDoc).find("CapturePath").eq(0).text());
    $("#playbackPicPath").val($(xmlDoc).find("PlaybackPicPath").eq(0).text());
    $("#playbackFilePath").val($(xmlDoc).find("PlaybackFilePath").eq(0).text());
    $("#protocolType").val($(xmlDoc).find("ProtocolType").eq(0).text());

    showOPInfo("本地配置获取成功！");
}

// 登录
function clickLogin(ip, port, username, password) {
    var szIP = ip,
		szPort = port,
		szUsername = username,
		szPassword = password;

    if ("" == szIP || "" == szPort) {
        return;
    }
    var iRet = WebVideoCtrl.I_Login(szIP, 1, szPort, szUsername, szPassword, {
        success: function (xmlDoc) {
            showOPInfo(szIP + " 登录成功！");
            //登陆成功标识
            _ControlCheck = "登录成功";

            $("#ip").prepend("<option value='" + szIP + "'>" + szIP + "</option>");
            setTimeout(function () {
                $("#ip").val(szIP);
                getChannelInfo();
            }, 10);
        },
        error: function () {
            showOPInfo(szIP + " 登录失败！");
        }
    });
    if (-1 == iRet) {
        showOPInfo(szIP + " 已登录过！");
    }
}


// 开始预览
function clickStartRealPlay() {
    //检查是否有任务
    if (_TaskID == null || _UavSetID == null) {
        showOPInfo(_Mlen_81); return;
    }
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex),
		szIP = $("#ip").val(),
		iStreamType = parseInt($("#streamtype").val(), 10),//码流类型
		iChannelID = parseInt($("#channels").val(), 10),
		bZeroChannel = $("#channels option").eq($("#channels").get(0).selectedIndex).attr("bZero") == "true" ? true : false,
		szInfo = "";

    if ("" == szIP) {
        return;
    }

    if (oWndInfo != null) {// 已经在播放了，先停止
        WebVideoCtrl.I_Stop();
    }

    var iRet = WebVideoCtrl.I_StartRealPlay(szIP, {
        iStreamType: iStreamType,
        iChannelID: iChannelID,
        bZeroChannel: bZeroChannel
    });

    if (0 == iRet) {
        szInfo = "开始预览成功！";
    } else {
        szInfo = "开始预览失败！";
    }

    showOPInfo(szIP + " " + szInfo);
}

// 停止预览
function clickStopRealPlay() {
    //检查是否有任务
    if (_TaskID == null || _UavSetID == null) {
        showOPInfo(_Mlen_81); return;
    }
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex),
		szInfo = "";

    if (oWndInfo != null) {
        var iRet = WebVideoCtrl.I_Stop();
        if (0 == iRet) {
            szInfo = "停止预览成功！";
            //停止
            _ControlCheck = null;
        } else {
            szInfo = "停止预览失败！";
        }
        showOPInfo(oWndInfo.szIP + " " + szInfo);
    }
}

// 显示操作信息
function showOPInfo(szInfo) {
    //显示信息
    showCBInfo(szInfo)
}

// 显示回调信息
function showCBInfo(szInfo) {
    var resultInfo = $("#resultInfo").val();
    resultInfo = resultInfo + "\r\n" + "[" + dateFormat(new Date(), "yyyy-MM-dd hh:mm:ss") + "]: " + szInfo + "...";
    $("#resultInfo").val(resultInfo);

    var area = $("#resultInfo");
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

// 获取通道
function getChannelInfo() {
    var szIP = $("#ip").val(),
		oSel = $("#channels").empty(),
		nAnalogChannel = 0;

    if ("" == szIP) {
        return;
    }

    // 模拟通道
    WebVideoCtrl.I_GetAnalogChannelInfo(szIP, {
        async: false,
        success: function (xmlDoc) {
            var oChannels = $(xmlDoc).find("VideoInputChannel");
            nAnalogChannel = oChannels.length;

            $.each(oChannels, function (i) {
                var id = parseInt($(this).find("id").eq(0).text(), 10),
					name = $(this).find("name").eq(0).text();
                if ("" == name) {
                    name = "Camera " + (id < 9 ? "0" + id : id);
                }
                oSel.append("<option value='" + id + "' bZero='false'>" + name + "</option>");
            });
            showOPInfo(szIP + " 获取模拟通道成功！");
        },
        error: function () {
            showOPInfo(szIP + " 获取模拟通道失败！");
        }
    });
    // 数字通道
    WebVideoCtrl.I_GetDigitalChannelInfo(szIP, {
        async: false,
        success: function (xmlDoc) {
            var oChannels = $(xmlDoc).find("InputProxyChannelStatus");

            $.each(oChannels, function (i) {
                var id = parseInt($(this).find("id").eq(0).text(), 10),
					name = $(this).find("name").eq(0).text(),
					online = $(this).find("online").eq(0).text();
                if ("false" == online) {// 过滤禁用的数字通道
                    return true;
                }
                if ("" == name) {
                    name = "IPCamera " + ((id - nAnalogChannel) < 9 ? "0" + (id - nAnalogChannel) : (id - nAnalogChannel));
                }
                oSel.append("<option value='" + id + "' bZero='false'>" + name + "</option>");
            });
            //showOPInfo(szIP + " 获取数字通道成功！");
        },
        error: function () {
            //showOPInfo(szIP + " 获取数字通道失败！");
        }
    });
    // 零通道
    WebVideoCtrl.I_GetZeroChannelInfo(szIP, {
        async: false,
        success: function (xmlDoc) {
            var oChannels = $(xmlDoc).find("ZeroVideoChannel");

            $.each(oChannels, function (i) {
                var id = parseInt($(this).find("id").eq(0).text(), 10),
					name = $(this).find("name").eq(0).text();
                if ("" == name) {
                    name = "Zero Channel " + (id < 9 ? "0" + id : id);
                }
                if ("true" == $(this).find("enabled").eq(0).text()) {// 过滤禁用的零通道
                    oSel.append("<option value='" + id + "' bZero='true'>" + name + "</option>");
                }
            });
            //showOPInfo(szIP + " 获取零通道成功！");
        },
        error: function () {
            //showOPInfo(szIP + " 获取零通道失败！");
        }
    });
}


// 抓图
function clickCapturePic() {
    //检查是否有任务
    if (_TaskID == null || _UavSetID == null) {
        showOPInfo(_Mlen_81); return;
    }
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex),
		szInfo = "";

    if (oWndInfo != null) {
        var szChannelID = $("#channels").val(),
			szPicName = oWndInfo.szIP + "_" + szChannelID + "_" + new Date().getTime(),
			iRet = WebVideoCtrl.I_CapturePic(szPicName);
        if (0 == iRet) {
            szInfo = "抓图成功！";
        } else {
            szInfo = "抓图失败！";
        }
        showOPInfo(oWndInfo.szIP + " " + szInfo);
    }
}

// 开始录像
function clickStartRecord() {
    //检查是否有任务
    if (_TaskID == null || _UavSetID == null) {
        showOPInfo(_Mlen_81); return;
    }
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex),
		szInfo = "";

    if (oWndInfo != null) {
        var szChannelID = $("#channels").val(),
			szFileName = oWndInfo.szIP + "_" + szChannelID + "_" + new Date().getTime(),
			iRet = WebVideoCtrl.I_StartRecord(szFileName);
        if (0 == iRet) {
            szInfo = "开始录像成功！";
        } else {
            szInfo = "开始录像失败！";
        }
        showOPInfo(oWndInfo.szIP + " " + szInfo);
    }
}

// 停止录像
function clickStopRecord() {
    //检查是否有任务
    if (_TaskID == null || _UavSetID == null) {
        showOPInfo(_Mlen_81); return;
    }
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex),
		szInfo = "";

    if (oWndInfo != null) {
        var iRet = WebVideoCtrl.I_StopRecord();
        if (0 == iRet) {
            szInfo = "停止录像成功！";
        } else {
            szInfo = "停止录像失败！";
        }
        showOPInfo(oWndInfo.szIP + " " + szInfo);
    }
}

// PTZ控制 9为自动，1,2,3,4,5,6,7,8为方向PTZ
var g_bPTZAuto = false;
function mouseDownPTZControl(iPTZIndex) {
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex),
		bZeroChannel = $("#channels option").eq($("#channels").get(0).selectedIndex).attr("bZero") == "true" ? true : false,
		iPTZSpeed = $("#ptzspeed").val(),
		bStop = false;

    if (bZeroChannel) {// 零通道不支持云台
        return;
    }

    if (oWndInfo != null) {
        if (9 == iPTZIndex && g_bPTZAuto) {
            iPTZSpeed = 0;// 自动开启后，速度置为0可以关闭自动
            bStop = true;
        } else {
            g_bPTZAuto = false;// 点击其他方向，自动肯定会被关闭
            bStop = false;
        }

        WebVideoCtrl.I_PTZControl(iPTZIndex, bStop, {
            iPTZSpeed: iPTZSpeed,
            success: function (xmlDoc) {
                if (9 == iPTZIndex) {
                    g_bPTZAuto = !g_bPTZAuto;
                }
                showOPInfo(oWndInfo.szIP + " 开启云台成功！");
            },
            error: function () {
                showOPInfo(oWndInfo.szIP + " 开启云台失败！");
            }
        });
    }
}

// 方向PTZ停止
function mouseUpPTZControl() {
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex);

    if (oWndInfo != null) {
        WebVideoCtrl.I_PTZControl(1, true, {
            success: function (xmlDoc) {
                showOPInfo(oWndInfo.szIP + " 停止云台成功！");
            },
            error: function () {
                showOPInfo(oWndInfo.szIP + " 停止云台失败！");
            }
        });
    }
}

function PTZZoomIn() {
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex);

    if (oWndInfo != null) {
        WebVideoCtrl.I_PTZControl(10, false, {
            iWndIndex: g_iWndIndex,
            success: function (xmlDoc) {
                showOPInfo(oWndInfo.szIP + " 调焦+成功！");
            },
            error: function () {
                showOPInfo(oWndInfo.szIP + "  调焦+失败！");
            }
        });
    }
}

function PTZZoomout() {
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex);

    if (oWndInfo != null) {
        WebVideoCtrl.I_PTZControl(11, false, {
            iWndIndex: g_iWndIndex,
            success: function (xmlDoc) {
                showOPInfo(oWndInfo.szIP + " 调焦-成功！");
            },
            error: function () {
                showOPInfo(oWndInfo.szIP + "  调焦-失败！");
            }
        });
    }
}

function PTZZoomStop() {
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex);

    if (oWndInfo != null) {
        WebVideoCtrl.I_PTZControl(11, true, {
            iWndIndex: g_iWndIndex,
            success: function (xmlDoc) {
                showOPInfo(oWndInfo.szIP + " 调焦停止成功！");
            },
            error: function () {
                showOPInfo(oWndInfo.szIP + "  调焦停止失败！");
            }
        });
    }
}

function PTZFocusIn() {
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex);

    if (oWndInfo != null) {
        WebVideoCtrl.I_PTZControl(12, false, {
            iWndIndex: g_iWndIndex,
            success: function (xmlDoc) {
                showOPInfo(oWndInfo.szIP + " 聚焦+成功！");
            },
            error: function () {
                showOPInfo(oWndInfo.szIP + "  聚焦+失败！");
            }
        });
    }
}

function PTZFoucusOut() {
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex);

    if (oWndInfo != null) {
        WebVideoCtrl.I_PTZControl(13, false, {
            iWndIndex: g_iWndIndex,
            success: function (xmlDoc) {
                showOPInfo(oWndInfo.szIP + " 聚焦-成功！");
            },
            error: function () {
                showOPInfo(oWndInfo.szIP + "  聚焦-失败！");
            }
        });
    }
}

function PTZFoucusStop() {
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex);

    if (oWndInfo != null) {
        WebVideoCtrl.I_PTZControl(12, true, {
            iWndIndex: g_iWndIndex,
            success: function (xmlDoc) {
                showOPInfo(oWndInfo.szIP + " 聚焦停止成功！");
            },
            error: function () {
                showOPInfo(oWndInfo.szIP + "  聚焦停止失败！");
            }
        });
    }
}

function PTZIrisIn() {
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex);

    if (oWndInfo != null) {
        WebVideoCtrl.I_PTZControl(14, false, {
            iWndIndex: g_iWndIndex,
            success: function (xmlDoc) {
                showOPInfo(oWndInfo.szIP + " 光圈+成功！");
            },
            error: function () {
                showOPInfo(oWndInfo.szIP + "  光圈+失败！");
            }
        });
    }
}

function PTZIrisOut() {
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex);

    if (oWndInfo != null) {
        WebVideoCtrl.I_PTZControl(15, false, {
            iWndIndex: g_iWndIndex,
            success: function (xmlDoc) {
                showOPInfo(oWndInfo.szIP + " 光圈-成功！");
            },
            error: function () {
                showOPInfo(oWndInfo.szIP + "  光圈-失败！");
            }
        });
    }
}

function PTZIrisStop() {
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex);

    if (oWndInfo != null) {
        WebVideoCtrl.I_PTZControl(14, true, {
            iWndIndex: g_iWndIndex,
            success: function (xmlDoc) {
                showOPInfo(oWndInfo.szIP + " 光圈停止成功！");
            },
            error: function () {
                showOPInfo(oWndInfo.szIP + "  光圈停止失败！");
            }
        });
    }
}

// 搜索录像 后面回放录像***************************************************************
var iSearchTimes = 0;
function clickRecordSearch(iType) {
    var szIP = $("#ip").val(),
		iChannelID = $("#channels").val(),
		bZeroChannel = $("#channels option").eq($("#channels").get(0).selectedIndex).attr("bZero") == "true" ? true : false,
		szStartTime = $("#starttime").val(),
		szEndTime = $("#endtime").val();

    if ("" == szIP) {
        return;
    }

    if (bZeroChannel) {// 零通道不支持录像搜索
        return;
    }

    if (0 == iType) {// 首次搜索
        $("#searchlist").empty();
        iSearchTimes = 0;
    }

    WebVideoCtrl.I_RecordSearch(szIP, iChannelID, szStartTime, szEndTime, {
        iSearchPos: iSearchTimes * 40,
        success: function (xmlDoc) {
            if ("MORE" === $(xmlDoc).find("responseStatusStrg").eq(0).text()) {

                for (var i = 0, nLen = $(xmlDoc).find("searchMatchItem").length; i < nLen; i++) {
                    var szPlaybackURI = $(xmlDoc).find("playbackURI").eq(i).text();
                    if (szPlaybackURI.indexOf("name=") < 0) {
                        break;
                    }
                    var szStartTime = $(xmlDoc).find("startTime").eq(i).text();
                    var szEndTime = $(xmlDoc).find("endTime").eq(i).text();
                    var szFileName = szPlaybackURI.substring(szPlaybackURI.indexOf("name=") + 5, szPlaybackURI.indexOf("&size="));

                    var objTr = $("#searchlist").get(0).insertRow(-1);
                    var objTd = objTr.insertCell(0);
                    objTd.id = "downloadTd" + i;
                    objTd.innerHTML = iSearchTimes * 40 + (i + 1);
                    objTd = objTr.insertCell(1);
                    objTd.width = "30%";
                    objTd.innerHTML = szFileName;
                    objTd = objTr.insertCell(2);
                    objTd.width = "30%";
                    objTd.innerHTML = (szStartTime.replace("T", " ")).replace("Z", "");
                    objTd = objTr.insertCell(3);
                    objTd.width = "30%";
                    objTd.innerHTML = (szEndTime.replace("T", " ")).replace("Z", "");
                    objTd = objTr.insertCell(4);
                    objTd.width = "10%";
                    objTd.innerHTML = "<a href='javascript:;' onclick='clickStartDownloadRecord(" + i + ");'>下载</a>";
                    $("#downloadTd" + i).data("playbackURI", szPlaybackURI);
                }

                iSearchTimes++;
                clickRecordSearch(1);// 继续搜索
            } else if ("OK" === $(xmlDoc).find("responseStatusStrg").eq(0).text()) {
                var iLength = $(xmlDoc).find("searchMatchItem").length;
                for (var i = 0; i < iLength; i++) {
                    var szPlaybackURI = $(xmlDoc).find("playbackURI").eq(i).text();
                    if (szPlaybackURI.indexOf("name=") < 0) {
                        break;
                    }
                    var szStartTime = $(xmlDoc).find("startTime").eq(i).text();
                    var szEndTime = $(xmlDoc).find("endTime").eq(i).text();
                    var szFileName = szPlaybackURI.substring(szPlaybackURI.indexOf("name=") + 5, szPlaybackURI.indexOf("&size="));

                    var objTr = $("#searchlist").get(0).insertRow(-1);
                    var objTd = objTr.insertCell(0);
                    objTd.id = "downloadTd" + i;
                    objTd.innerHTML = iSearchTimes * 40 + (i + 1);
                    objTd = objTr.insertCell(1);
                    objTd.width = "30%";
                    objTd.innerHTML = szFileName;
                    objTd = objTr.insertCell(2);
                    objTd.width = "30%";
                    objTd.innerHTML = (szStartTime.replace("T", " ")).replace("Z", "");
                    objTd = objTr.insertCell(3);
                    objTd.width = "30%";
                    objTd.innerHTML = (szEndTime.replace("T", " ")).replace("Z", "");
                    objTd = objTr.insertCell(4);
                    objTd.width = "10%";
                    objTd.innerHTML = "<a href='javascript:;' onclick='clickStartDownloadRecord(" + i + ");'>下载</a>";
                    $("#downloadTd" + i).data("playbackURI", szPlaybackURI);
                }
                showOPInfo(szIP + " 搜索录像文件成功！");
            } else if ("NO MATCHES" === $(xmlDoc).find("responseStatusStrg").eq(0).text()) {
                setTimeout(function () {
                    showOPInfo(szIP + " 没有录像文件！");
                }, 50);
            }
        },
        error: function () {
            showOPInfo(szIP + " 搜索录像文件失败！");
        }
    });
}

// 开始回放
function clickStartPlayback() {
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex),
		szIP = $("#ip").val(),
		bZeroChannel = $("#channels option").eq($("#channels").get(0).selectedIndex).attr("bZero") == "true" ? true : false,
		iChannelID = $("#channels").val(),
		szStartTime = $("#starttime").val(),
		szEndTime = $("#endtime").val(),
		szInfo = "",
		bChecked = $("#transstream").prop("checked"),
		iRet = -1;

    if ("" == szIP) {
        return;
    }

    if (bZeroChannel) {// 零通道不支持回放
        return;
    }

    if (oWndInfo != null) {// 已经在播放了，先停止
        WebVideoCtrl.I_Stop();
    }

    if (bChecked) {// 启用转码回放
        var oTransCodeParam = {
            TransFrameRate: "16",// 0：全帧率，5：1，6：2，7：4，8：6，9：8，10：10，11：12，12：16，14：15，15：18，13：20，16：22
            TransResolution: "2",// 255：Auto，3：4CIF，2：QCIF，1：CIF
            TransBitrate: "23"// 2：32K，3：48K，4：64K，5：80K，6：96K，7：128K，8：160K，9：192K，10：224K，11：256K，12：320K，13：384K，14：448K，15：512K，16：640K，17：768K，18：896K，19：1024K，20：1280K，21：1536K，22：1792K，23：2048K，24：3072K，25：4096K，26：8192K
        };
        iRet = WebVideoCtrl.I_StartPlayback(szIP, {
            iChannelID: iChannelID,
            szStartTime: szStartTime,
            szEndTime: szEndTime,
            oTransCodeParam: oTransCodeParam
        });
    } else {
        iRet = WebVideoCtrl.I_StartPlayback(szIP, {
            iChannelID: iChannelID,
            szStartTime: szStartTime,
            szEndTime: szEndTime
        });
    }

    if (0 == iRet) {
        szInfo = "开始回放成功！";
    } else {
        szInfo = "开始回放失败！";
    }
    showOPInfo(szIP + " " + szInfo);
}

// 停止回放
function clickStopPlayback() {
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex),
		szInfo = "";

    if (oWndInfo != null) {
        var iRet = WebVideoCtrl.I_Stop();
        if (0 == iRet) {
            szInfo = "停止回放成功！";
        } else {
            szInfo = "停止回放失败！";
        }
        showOPInfo(oWndInfo.szIP + " " + szInfo);
    }
}

// 开始倒放
function clickReversePlayback() {
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex),
		szIP = $("#ip").val(),
		bZeroChannel = $("#channels option").eq($("#channels").get(0).selectedIndex).attr("bZero") == "true" ? true : false,
		iChannelID = $("#channels").val(),
		szStartTime = $("#starttime").val(),
		szEndTime = $("#endtime").val(),
		szInfo = "";

    if ("" == szIP) {
        return;
    }

    if (bZeroChannel) {// 零通道不支持回放
        return;
    }

    if (oWndInfo != null) {// 已经在播放了，先停止
        WebVideoCtrl.I_Stop();
    }

    var iRet = WebVideoCtrl.I_ReversePlayback(szIP, {
        iChannelID: iChannelID,
        szStartTime: szStartTime,
        szEndTime: szEndTime
    });

    if (0 == iRet) {
        szInfo = "开始倒放成功！";
    } else {
        szInfo = "开始倒放失败！";
    }
    showOPInfo(szIP + " " + szInfo);
}

// 单帧
function clickFrame() {
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex),
		szInfo = "";

    if (oWndInfo != null) {
        var iRet = WebVideoCtrl.I_Frame();
        if (0 == iRet) {
            szInfo = "单帧播放成功！";
        } else {
            szInfo = "单帧播放失败！";
        }
        showOPInfo(oWndInfo.szIP + " " + szInfo);
    }
}

// 暂停
function clickPause() {
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex),
		szInfo = "";

    if (oWndInfo != null) {
        var iRet = WebVideoCtrl.I_Pause();
        if (0 == iRet) {
            szInfo = "暂停成功！";
        } else {
            szInfo = "暂停失败！";
        }
        showOPInfo(oWndInfo.szIP + " " + szInfo);
    }
}

// 恢复
function clickResume() {
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex),
		szInfo = "";

    if (oWndInfo != null) {
        var iRet = WebVideoCtrl.I_Resume();
        if (0 == iRet) {
            szInfo = "恢复成功！";
        } else {
            szInfo = "恢复失败！";
        }
        showOPInfo(oWndInfo.szIP + " " + szInfo);
    }
}

// 慢放
function clickPlaySlow() {
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex),
		szInfo = "";

    if (oWndInfo != null) {
        var iRet = WebVideoCtrl.I_PlaySlow();
        if (0 == iRet) {
            szInfo = "慢放成功！";
        } else {
            szInfo = "慢放失败！";
        }
        showOPInfo(oWndInfo.szIP + " " + szInfo);
    }
}

// 快放
function clickPlayFast() {
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex),
		szInfo = "";

    if (oWndInfo != null) {
        var iRet = WebVideoCtrl.I_PlayFast();
        if (0 == iRet) {
            szInfo = "快放成功！";
        } else {
            szInfo = "快放失败！";
        }
        showOPInfo(oWndInfo.szIP + " " + szInfo);
    }
}

// OSD时间
function clickGetOSDTime() {
    var oWndInfo = WebVideoCtrl.I_GetWindowStatus(g_iWndIndex);

    if (oWndInfo != null) {
        var szTime = WebVideoCtrl.I_GetOSDTime();
        if (szTime != -1) {
            $("#osdtime").val(szTime);
            showOPInfo(oWndInfo.szIP + " 获取OSD时间成功！");
        } else {
            showOPInfo(oWndInfo.szIP + " 获取OSD时间失败！");
        }
    }
}



