mini.parse();
var map;
var myApp;

var seaLat = null; //配置地图中心坐标
var setlng = null;
var centerCode = null; //1 岳阳 2 上海 
var shipLat = 0;//上海测试船坐标
var shipLng = 0;

var baseStationLat = null; //基站经纬度
var baseStationLng = null;

var AreaPointCheck = 1;//区域点开关 默认开启状

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
        mapOptions.mapType = shipxyMap.MapType.CMAP; //GOOGLESATELLITE google卫星图  CMAP：海图
        map = new shipxyMap.Map('mapDiv', mapOptions);
        //地图初始化完毕，flash组件会自动调用shipxyMap.mapReady函数
        //只有在mapReady函数被触发之后，才可以注册map事件和调用flash组件内部的方法
        shipxyMap.mapReady = function () {

            myApp.initService();
            //上海测试点
            //if (centerCode != null && centerCode == 2) {
                addShipTest();
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

//添加船舶--用于上海测试
function addShipTest() {
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

function getCenter() {
    var center = map.getCenter();
    mini.alert('当前地图中心点坐标：纬度=' + center.lat + ',经度=' + center.lng);
}