mini.parse();

//开启服务器的按钮
function onStartTcpServer() {
    bodyLoading();
    $.ajax({
        url: _Datapath + "Busi/TcpControlCode.aspx?method=onStartTcpServer",
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                showCBInfo("开启TCP服务器成功");
            } else {
                if (isLoginOut(data) == false) {
                    showCBInfo("开启TCP服务器失败:" + data.ErrMessage);
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

//关闭服务器的按钮
function onStopTcpServer() {
    bodyLoading();
    $.ajax({
        url: _Datapath + "Busi/TcpControlCode.aspx?method=onStopTcpServer",
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                showCBInfo("关闭TCP服务器成功");
            } else {
                if (isLoginOut(data) == false) {
                    showCBInfo("关闭TCP服务器失败:" + data.ErrMessage);
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

//通话模块 flag: 0 讲话发射 1 接听接收
function callModule(flag) {

    bodyLoading();
    $.ajax({
        url: _Datapath + "Busi/TcpControlCode.aspx?method=onCallModule",
        data: {Flag: flag },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {

                //显示指令信息
                if (flag == 0) {
                    showCBInfo("讲话发射命令发送成功");
                }
                if (flag == 1) {
                    showCBInfo("接听接收命令发送成功");
                }
            } else {
                if (isLoginOut(data) == false) {
                    //notify(_Mlen_6 + data.ErrMessage);
                    if (flag == 0) {
                        showCBInfo("讲话发射命令发送失败:" + data.ErrMessage);
                    }
                    if (flag == 1) {
                        showCBInfo("接听接收命令发送失败:" + data.ErrMessage);
                    }
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


















