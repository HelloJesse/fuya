/// <reference path="../../shipxyAPI.js" />
/// <reference path="../../shipxyMap.js" />

mini.parse();
var map;
var myApp;

var options = 0;       //点个数据
var dataLatLng = [];   //线数据,记录多个点的经纬度
var markerArray = [];  //记录点对象数组
var polyline;          //航线对象
var AreaPointCheck = 1;//区域点开关 默认开启状态
var seaLat = null; //配置地图中心坐标
var setlng = null;
var centerCode = null; //1 岳阳 2 上海 
var shipLat = 0;//上海测试船坐标
var shipLng = 0;

var baseStationLat = null; //基站经纬度
var baseStationLng = null;

(function () {
    //配置地图中心坐标点
    var InitCheckSeaMap = function () {
        $.ajax({
            url: _Datapath + "Busi/DUavMainTaskCode.aspx?method=AreaPointCheck",
            type: "post",
            async: false,
            success: function (text) {
                var data = mini.decode(text);
                if (isLoginOut(data) == false) {
                    if (data.IsOK) {
                        AreaPointCheck = 1;//打开开关
                    } else {
                        AreaPointCheck = 0;//关闭开关
                    }
                    //--海图中心点坐标 1：岳阳 2:上海
                    centerCode = parseInt(data.centerCode);
                    //--中心点坐标 经纬度
                    seaLat = parseFloat(data.lat);
                    setlng = parseFloat(data.lng);
                    //--上海 测试船经纬度
                    if (null != data.shipLat && "" != data.shipLat) {
                        shipLat = parseFloat(data.shipLat);
                    }
                    if (null != data.shipLng && "" != data.shipLng) {
                        shipLng = parseFloat(data.shipLng);
                    }
                    //基站经纬度
                    if (null != data.baseStationLat && "" != data.baseStationLat) {
                        baseStationLat = parseFloat(data.baseStationLat);
                    }
                    if (null != data.baseStationLng && "" != data.baseStationLng) {
                        baseStationLng = parseFloat(data.baseStationLng);
                    }
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                notify(jqXHR.responseText);
            }
        });
    };

    //初始化地图
    var initMap = function () {
        var mapOptions = new shipxyMap.MapOptions();
        //mapOptions.center = new shipxyMap.LatLng(29.48947, 113.10199);
        mapOptions.center = new shipxyMap.LatLng(seaLat, setlng);//调整
        mapOptions.zoom = 12;
        mapOptions.mapType = shipxyMap.MapType.CMAP;
        map = new shipxyMap.Map('mapDiv', mapOptions);
        //地图初始化完毕，flash组件会自动调用shipxyMap.mapReady函数
        //只有在mapReady函数被触发之后，才可以注册map事件和调用flash组件内部的方法

        shipxyMap.mapReady = function () {

            //岳阳段加载船舶信息
            //if (centerCode == null || centerCode == 1) {
            //    myApp.initService();
            //}

            myApp.initService();
            //上海测试点
            //if (centerCode != null && centerCode == 2) {
                addShip();
            //}

            //基站经纬度
            if (baseStationLat != null && baseStationLng != null) {
                addBaseStation(baseStationLat, baseStationLng);
            }

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
            InitCheckSeaMap();
            this.map = initMap();
            this.view.init();
        },
        //程序服务初始化：初始化船舶服务
        initService: function () {
            this.service.init();
        }
    };
})();

//注册添加点事件
function addOptions() {
    map.addEventListener(map, shipxyMap.Event.DOUBLE_CLICK, addOptionsEvent);

    //控制按钮
    setButtonEnabled(["#btnAddOption"], false);
    setButtonEnabled(["#btnOverOption", "#btnReset", "#btnStop"], true);

    //删除叠加物船
    map.removeOverlayByType(shipxyMap.OverlayType.SHIP);
}

//移除添加点事件
function OverOptions() {
    if (options == 0 || dataLatLng.length <=0) {
        notify(_Mlen_77); return;
    }

    //控制按钮
    setButtonEnabled(["#btnAddOption"], false);
    setButtonEnabled(["#btnReset", "#btnOverOption", "#btnStop"], true);

    //myApp.initService();//重新初始化船舶服务
    savePolyLine();//保存航线
}


//添加点验证事件
var addOptionsEvent = function (event) {
    var lat = event.latLng.lat;
    var lng = event.latLng.lng;

    //删除叠加物船
    map.removeOverlayByType(shipxyMap.OverlayType.SHIP);

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
                        addOptionsMethod(lat, lng);//验证成功在添加
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
        addOptionsMethod(lat, lng);//无需验证，直接添加
    }
    
};

//添加点
function addOptionsMethod(lat, lng) {
    if (options == 0 || dataLatLng.length <= 0) {
        dataLatLng[options] = new shipxyMap.LatLng(lat, lng);//记录增加点的经纬度，添加线使用
        options = options + 1;
        addMarker(lat, lng, options);
        return;
    }
    
    //补点动作
    var jd1 = dataLatLng[options - 1].lng;
    var wd1 = dataLatLng[options - 1].lat;

    //判断区域点开关 0：关闭状态 无需补点
    if (AreaPointCheck == 1) {
        bodyLoading();
        $.ajax({
            url: _Datapath + "Busi/DUavMainTaskCode.aspx?method=GetLinepointMiddle",
            data: { jd1: jd1, wd1: wd1, jd2: lng, wd2: lat },
            type: "post",
            success: function (text) {
                var data = mini.decode(text);   //反序列化成对象
                if (isLoginOut(data) == false) {
                    if (data.IsOK) {
                        var DataList = data.DataList;
                        //补点
                        if (DataList.length > 0) {
                            for (var i = 0; i < DataList.length; i++) {
                                dataLatLng[options] = new shipxyMap.LatLng(DataList[i].Wd, DataList[i].Jd);
                                options = options + 1;
                                addMarker(DataList[i].Wd, DataList[i].Jd, options);
                            }
                        }

                        //补末尾点
                        dataLatLng[options] = new shipxyMap.LatLng(lat, lng);
                        options = options + 1;
                        addMarker(lat, lng, options);

                        //连线
                        addPolyline(dataLatLng);
                    } else {
                        notify(data.ErrMessage);
                    }
                }
                mini.unmask();
            },
            error: function () {
                mini.unmask();
            }
        });
    } else {
        //补点
        dataLatLng[options] = new shipxyMap.LatLng(lat, lng);
        options = options + 1;
        addMarker(lat, lng, options);

        //连线
        addPolyline(dataLatLng);
    }
}

function addMarker(lat, lng, options) {

    /*****面显示样式*****/
    var opts = new shipxyMap.MarkerOptions()
    opts.zoomlevels = [1, 18]; //显示级别
    opts.zIndex = 4;  //显示层级
    opts.isShowLabel = true; //是否显示label
    opts.isEditable = true; //是否可编辑
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

    markerArray.push(marker);//记录点叠加物

    map.addOverlay(marker, true); //添加到地图上显示 优先显示

    map.addEventListener(marker, shipxyMap.Event.MOUSE_UP, onMarkerMove);//监听点移动
}

var onMarkerMove = function (event) {
    var overlayId = event.overlayId;//叠加物id 例如： Marker1
    var mlat = event.latLng.lat;
    var mlng = event.latLng.lng;//移动后的经纬度

    //处理dataLatLng[] 数组，更新指定点叠加物 经纬度
    if (markerArray.length <= 0) {
        return;
    }
    dataLatLng = [];//清空，需从新赋值
    for (var i = 0; i < markerArray.length; i++) {
        if (markerArray[i].id == overlayId) {
            markerArray[i].data.lat = mlat;
            markerArray[i].data.lng = mlng;
        }
        dataLatLng[i] = new shipxyMap.LatLng(markerArray[i].data.lat, markerArray[i].data.lng);
    }

    //更新线
    addPolyline(dataLatLng);
}

//添加线
function addPolyline(dataLatLng) {

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
    opts.labelOptions.text = '航线';
    opts.labelOptions.labelPosition = new shipxyMap.Point(0, 0);
    /*****线*****/
    polyline = new shipxyMap.Polyline('Polyline', dataLatLng, opts);
    //添加到地图上显示
    map.addOverlay(polyline);
}

//保存航线
function savePolyLine() {
    if (options == 0 || dataLatLng.length <= 0) {
        notify(_Mlen_77); return;
    }

    var lineTask = mini.encode(dataLatLng);
    lineTask = lineTask.replace(/"/g, "").replace("[", "").replace("]", "");

    var taskSource = mini.get("#TaskSource").getValue();//任务来源 0: 主控中心 1:基站 2:客户
    var flyWay = 1;//任务类型 0:船舶跟踪 1:航线巡航

    mini.open({
        url: _Rootpath + "wms/busi/CreateMainTask.html",
        title: "规划任务", width: 430, height: 420,
        onload: function () {
            var iframe = this.getIFrameEl();
            var data = { taskSource: taskSource, flyWay: flyWay };
            iframe.contentWindow.SetData(data);
        },
        ondestroy: function (data) {
            if (data) {
                if (data == "cancel" || data == "close") {//取消直接返回
                    return;
                }
                data = mini.clone(data);
                bodyLoading();     //加载遮罩
                $.ajax({
                    url: _Datapath + "Busi/DUavMainTaskCode.aspx?method=SaveMainTask",
                    data: { Data: data, LineTask: lineTask },
                    type: "post",
                    success: function (text) {
                        var data = mini.decode(text);   //反序列化成对象
                        if (isLoginOut(data) == false) {
                            if (data.IsOK) {
                                //默认调用查询方法
                                showSaveOK();
                                SystemSearch();
                                setButtonEnabled(["#btnOverOption", "#btnReset", "#btnStop"], false);
                                setButtonEnabled(["#btnAddOption"], true);

                                //重置 清空
                                resetPolyline();

                                //移除监听事件
                                map.removeEventListener(map, shipxyMap.Event.DOUBLE_CLICK, addOptionsEvent);
                            } else {
                                notify(data.ErrMessage);
                            }
                        }
                        mini.unmask(); //取消遮罩
                    },
                    error: function () {
                        mini.unmask(); //取消遮罩
                    }
                });
            }
        }
    });
    
}

//重置
function resetPolyline() {
    if (markerArray.length > 0) {
        //先移除点
        map.removeOverlays(markerArray);
        //删除点的移动监听事件
        for (var i = 0; i < markerArray.length; i++) {
            map.removeEventListener(markerArray[i], shipxyMap.Event.MOUSE_UP, onMarkerMove);
        }

        dataLatLng = [];
        markerArray = [];
    }

    if (polyline) {//删除线
        map.removeOverlay(polyline);
        polyline = null;
    }

    options = 0;

    //删除叠加物船
    map.removeOverlayByType(shipxyMap.OverlayType.SHIP);
}

//放弃规划
function stopPolyline() {
    if (markerArray.length > 0) {
        //先移除点
        map.removeOverlays(markerArray);
        //删除点的移动监听事件
        for (var i = 0; i < markerArray.length; i++) {
            map.removeEventListener(markerArray[i], shipxyMap.Event.MOUSE_UP, onMarkerMove);
        }

        dataLatLng = [];
        markerArray = [];
    }

    if (polyline) {//删除线
        map.removeOverlay(polyline);
        polyline = null;
    }
    options = 0;
    //修改按钮状态
    setButtonEnabled(["#btnOverOption", "#btnReset", "#btnStop"], false);
    setButtonEnabled(["#btnAddOption"], true);
    //添加船叠加物
    myApp.initService();//重新初始化船舶服
}


//添加船舶--用于上海测试
function addShip() {
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
    data.shipId = "111111111";
    data.name = "虚拟测试船";
    data.callsign = "";
    data.imo = "111111111";
    data.shipType = "货船";
    data.navStatus = "在航(主机推动)";
    data.length = 99;
    data.beam = 17;
    data.draught = 0;
    data.lat = shipLat;
    data.lng = shipLng;
    data.heading = 1;
    data.course = 1;
    data.speed = 5;
    data.dest = "";
    data.eta = "";
    data.lastTime = 1483686471;
    addshipOverlay = new shipxyMap.Ship("-1", data, option);
    map.addOverlay(addshipOverlay, true);

    //map.addEventListener(addshipOverlay, shipxyMap.Event.CLICK, onAddshipOverlayClick);//监听
}

//var onAddshipOverlayClick = function (event) {

//    var ShipInfo = '{"shipId":"413812939","MMSI":"413812939","IMO":"","name":"虚拟测试船","callsign":"",'+
//        '"type":"货船","status":"在航(主机推动)","length":99,"beam":17,"left":9,"trail":9,"draught":0,"country":"CHN",'+
//        '"cargoType":"","lng":' + shipLng + ',"lat":' + shipLat + ',"heading":168,"course":168.7,"speed":5.503891050583658,"rot":-2.01,"dest":"","eta":"","lastTime":1483497630}';

//    var lat = event.latLng.lat;
//    var lng = event.latLng.lng;//经纬度

//    var lineTask = '{ lat: ' + lat + ', lng: ' + lng + ' }';
//    var taskSource = mini.get("#TaskSource").getValue();//任务来源 0: 主控中心 1:基站 2:客户
//    var flyWay = 0;//任务类型 0:船舶跟踪 1:航线巡航

//    mini.open({
//        url: "wms/busi/CreateMainTask.html",
//        title: "规划任务", width: 430, height: 400,
//        onload: function () {
//            var iframe = this.getIFrameEl();
//            var data = { taskSource: taskSource, flyWay: flyWay };
//            iframe.contentWindow.SetData(data);
//        },
//        ondestroy: function (data) {
//            if (data) {
//                if (data == "cancel" || data == "close") {//取消直接返回
//                    return;
//                }
//                data = mini.clone(data);
//                bodyLoading();     //加载遮罩
//                $.ajax({
//                    url: "../../data/Busi/DUavMainTaskCode.aspx?method=SaveMainTask",
//                    data: { Data: data, LineTask: lineTask, ShipInfo: ShipInfo },
//                    type: "post",
//                    success: function (text) {
//                        var data = mini.decode(text);   //反序列化成对象
//                        if (isLoginOut(data) == false) {
//                            if (data.IsOK) {
//                                //默认调用查询方法
//                                SystemSearch();
//                            } else {
//                                notify(data.ErrMessage);
//                            }
//                        }
//                        mini.unmask(); //取消遮罩
//                    },
//                    error: function () {
//                        mini.unmask(); //取消遮罩
//                    }
//                });
//            }
//        }
//    });
//}

//添加基站图标
function addBaseStation(lat, lng) {

    /*****面显示样式*****/
    var opts = new shipxyMap.MarkerOptions()
    opts.zoomlevels = [1, 18]; //显示级别
    opts.zIndex = 4;  //显示层级
    opts.isShowLabel = true; //是否显示label
    opts.isEditable = true; //是否可编辑
    opts.imageUrl = '../../images/uav/house.png'; //图片URL
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
    var marker = "";
    data[0] = new shipxyMap.LatLng(lat, lng);
    //DATAEND
    data = new shipxyMap.LatLng(data[0].lat, data[0].lng);
    marker = new shipxyMap.Marker(marker, data, opts);

    map.addOverlay(marker, true); //添加到地图上显示 优先显示
}
