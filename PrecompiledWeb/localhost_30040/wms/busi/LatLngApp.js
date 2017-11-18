/// <reference path="../../shipxyAPI.js" />
/// <reference path="../../shipxyMap.js" />
/// js用于区域坐标建设

mini.parse();
var map;
var myApp;

(function () {
    //初始化地图
    var initMap = function () {
        var mapOptions = new shipxyMap.MapOptions();
        //mapOptions.center = new shipxyMap.LatLng(29.48947, 113.10199);
        mapOptions.center = new shipxyMap.LatLng(29.440197, 113.123276);//调整
        mapOptions.zoom = 12;
        mapOptions.mapType = shipxyMap.MapType.CMAP;
        map = new shipxyMap.Map('mapDiv', mapOptions);
        //地图初始化完毕，flash组件会自动调用shipxyMap.mapReady函数
        //只有在mapReady函数被触发之后，才可以注册map事件和调用flash组件内部的方法
        shipxyMap.mapReady = function () {
            myApp.initService();
            
            //调用API的注册点线面事件接口
            map.addGraphEventListener(shipxyMap.Event.CLICK, function (event) {
                //从缓存中来获取数据  
                var graphId = event.overlayId;
            });
        }
        return map;
    };
    myApp = {
        map: null, //地图对象
        //程序视图初始化：页面初始化、地图初始化
        initView: function (key) {
            //shipxyAPI.key = shipxyMap.key = key;
            this.map = initMap();
            this.view.init();
        },
        //程序服务初始化：初始化船舶服务
        initService: function () {
            this.service.init();
        }
    };
})();


var dataLatLng = [];
//注册添加点事件
function addOptions() {
    //删除叠加物船
    //记录增加点的经纬度，添加线使用
    //dataLatLng[options] = new shipxyMap.LatLng(lat, lng);

    //取出航道图坐标点
    bodyLoading();
    $.ajax({
        url: _Datapath + "Busi/DUavMainTaskCode.aspx?method=GetLaneLatLng",
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (data.IsOK) {
                var DataListLat = data.DataListLat;//纬度
                var DataListLng = data.DataListLng;//经度

                for (var i = 0; i < DataListLat.length; i++) {//纬度
                    for (var j = 0; j < DataListLng.length; j++) {//经度
                        joinLine(j, DataListLat[i].Lat, DataListLng[j].Lng);
                    }
                    //添加线 横线
                    addPolyline(dataLatLng, "横"+i);
                    resetLine();
                }

                for (var i = 0; i < DataListLng.length; i++) {
                    for (var j = 0; j < DataListLat.length; j++) {//经度
                        joinLine(j, DataListLat[j].Lat, DataListLng[i].Lng);
                    }
                    //添加线 竖线
                    addPolyline(dataLatLng, "竖"+i);
                    resetLine();
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

    map.removeOverlayByType(shipxyMap.OverlayType.SHIP);
}

//拼接航线点坐标
function joinLine(options,lat,lng) {
    dataLatLng[options] = new shipxyMap.LatLng(lat, lng);
}

function resetLine() {
    dataLatLng = [];
}

//添加线
function addPolyline(dataLatLng,flag) {

    /*****线显示样式*****/
    var opts = new shipxyMap.PolylineOptions()
    opts.zoomlevels = [1, 18]; //显示级别
    opts.zIndex = 4; //是否显示label
    opts.isShowLabel = true; //是否显示label
    opts.isEditable = false; //是否可编辑
    /*线样式*/
    opts.strokeStyle.color = 0xff3399;
    opts.strokeStyle.alpha = 0.8;
    opts.strokeStyle.thickness = 2;
    /*标签样式*/
    //标签线条
    opts.labelOptions.border = true; //有边框  
    opts.labelOptions.borderStyle.color = 0xff0000;
    opts.labelOptions.borderStyle.alpha = 0.8;
    opts.labelOptions.borderStyle.thickness = 1;
    //标签文字
    opts.labelOptions.fontStyle.name = 'Verdana';
    opts.labelOptions.fontStyle.size = 14;
    opts.labelOptions.fontStyle.color = 0xff33cc;
    opts.labelOptions.fontStyle.bold = true;  //粗体
    opts.labelOptions.fontStyle.italic = true;  //斜体
    opts.labelOptions.fontStyle.underline = true;  //下划线
    //标签填充
    opts.labelOptions.background = true; //有背景  
    opts.labelOptions.backgroundStyle.color = 0xffccff;  //边框样式
    opts.labelOptions.backgroundStyle.alpha = 06;
    opts.labelOptions.zoomlevels = [1, 18]; //显示级别
    opts.labelOptions.text = flag;
    opts.labelOptions.labelPosition = new shipxyMap.Point(0, 0);
    /*****线*****/
    polyline = new shipxyMap.Polyline('Polyline'+flag, dataLatLng, opts);
    //添加到地图上显示
    map.addOverlay(polyline);
}

var _valText = "";
function aaaa() {

    mini.prompt("请输入区域编码：", "请输入",
        function (action, value) {
            if (action == "ok") {
                _valText = value;
            } 
        }
    );

    map.drawAreaBound();
}

function bbbb() {
    var latlngArray = map.getAreaBound();
    if (latlngArray.length<=0) {
        return;
    }

    var json = mini.encode(latlngArray);
    bodyLoading();

    $.ajax({
        url: _Datapath + "Busi/DUavMainTaskCode.aspx?method=SaveLaneLatLngArray",
        data: { data: json, code: _valText },
        type: "post",
        success: function (text) {
            var data = mini.decode(text);
            if (isLoginOut(data) == false) {
                if (data.IsOK) {
                    showSaveOK();
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


