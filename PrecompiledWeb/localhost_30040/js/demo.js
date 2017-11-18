mini.parse();
var _url = "";
var _name = "";
$(function () {
    //InitializeImpact();
    LoadNeedDealThing();
});

//初始化界面UI效果
$(window).load(resizeself());
$(window).resize(function () {
    resizeself();
});

function resizeself() {
    var bodyheight = $(document.body).height();
    var bodywidth = $(document.body).width();
    var oneheight = ((bodyheight) - 110) / 2;
    if (oneheight < 300) {
        oneheight = 300;
        bodyheight = 900;
        bodywidth = 1500;
        document.getElementById("bodys").style.overflow = "auto";
    }
    else {
        bodyheight = bodyheight - 5;
        bodywidth = bodywidth - 5;
        document.getElementById("bodys").style.removeProperty("overflow");
    }
    document.getElementById("maindiv").style.height = bodyheight + 'px';
    document.getElementById("maindiv").style.width = bodywidth + 'px';
    document.getElementById("needDealThing").style.height = oneheight + 'px';
    document.getElementById("boxdetailList").style.height = oneheight + 'px';
    document.getElementById("chartscontainer").style.height = oneheight + 'px';
    document.getElementById("echartcontainer").style.height = oneheight + 'px';
}

//加载处理待处理的事项
function LoadNeedDealThing() {
    $.ajax({
        url: _Datapath + "Main/MenuManager.aspx?method=GetNeedDealThing",
        data: {},
        type: "post",
        success: function (text) {
            var data = mini.decode(text);   //反序列化成对象
            if (isLoginOut(data) == false) {
                if (data.msg == "true") {
                    document.getElementById("needDealThing").innerHTML = data.needAlertThing;
                    document.getElementById("chartscontainer").innerHTML = data.echartsThing;
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify("操作失败:" + jqXHR.responseText);
        }
    });
}


//加载明细信息
function LoadDetailThing(e) {
    _url = "";
    _name = ""; //chartscontainer
    document.getElementById("detailList").innerHTML = ""; //待办事项
    var id = e.getAttribute("value");
    _url = e.getAttribute("LinkUrl");
    _name = e.getAttribute("ShowName");
    document.getElementById("needname").innerText = "具体待办-"+_name;
    if (id) {
        $.ajax({
            url: _Datapath + "Main/MenuManager.aspx?method=GetNeedDealThingDetail",
            data: { ID: id },
            type: "post",
            success: function (text) {
                var data = mini.decode(text);   //反序列化成对象
                if (isLoginOut(data) == false) {
                    if (data.msg == "true") {
                        document.getElementById("detailList").innerHTML = data.sbstring;
                    }
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                notify("操作失败:" + jqXHR.responseText);
            }
        });
    }
}

///点击明细进入单子
function DetailClick(e) {
    if (e.id == "BILLNO") {
        var TRElement = e.parentElement;
        var id = TRElement.cells["BILLID"].innerText;
        var billno = TRElement.cells["BILLNO"].innerText;
        if (id) {
            if (_url && _url != "") {
                var languagefolder = getLanguageFolder();
                GetTabsddNew2(_name + id, _name + ":" + billno, languagefolder + _url, { id: id })
            }
        }
    }
}

function GetTabsddNew2(name, title, url, args) {
    //var tab = { name: "岗位信息", title: "岗位信息", url: 'wms/Main/Sys_D_User_Edit.html', showCloseButton: true };
    if (args) setCacheArgs(args);

    var tab = { name: name, title: title, url: url, showCloseButton: true };
    if (top["win"]) {
        top["win"].addTab_Child(tab);
    } else {
        window.open(url.split("/")[url.split("/").length - 1]);
    }

}

var _tuxingid = "";
var _tuxingname = "";
//处理处理图表
function EchartsDetial(e)
{
   
    _tuxingid = e.getAttribute("value");
    _tuxingname = e.getAttribute("ShowName");
    //document.getElementById("echartname").innerText = "图表-" + _tuxingname;
    GetEchartData(2);
}

//获取图表显示的具体数据
function GetEchartData(e) {
    if (_tuxingid == "")
    {
        return;
    }
    if (e == 0) {
        document.getElementById("selftimes").value = "";
        document.getElementById("selftimee").value = "";
    }
    var charttype = "pie";
    var monthType = "week";
    var selftimes = "";
    var selftimee = "";
    if (document.getElementById("bing").checked) {
        charttype = "pie";
    }
    else if (document.getElementById("zhexian").checked) {
        charttype = "line";
    }
    if (document.getElementById("selftimes").value != "" && document.getElementById("selftimee").value != "") {
        monthType = "selftime";
        selftimes = document.getElementById("selftimes").value;
        selftimee = document.getElementById("selftimee").value;
    }
    else if (document.getElementById("yizhou").checked) {
        monthType = "week";
    }
    else if (document.getElementById("yiyue").checked) {
        monthType = "month";
    }
    else if (document.getElementById("yinian").checked) {
        monthType = "year";
    }

    var myChart = echarts.init(document.getElementById('echartcontainer'));
    $.ajax({
        url: _Datapath + "Main/MenuManager.aspx?method=GetEchartsData",
        data: { ID: _tuxingid, Charttype: charttype, MonthType: monthType, NameType: _tuxingname, Selftimes: selftimes, Selftimee: selftimee },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);   //反序列化成对象
            if (isLoginOut(data) == false) {
                if (data.msg == "true") {
                    var option = mini.decode(data.option);
                    myChart.setOption(option);
                } else if (data.msg) {
                    if (data.msg != "") {
                        notify(data.msg);
                        return;
                    }
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            notify("操作失败:" + jqXHR.responseText);
        }
    });

}