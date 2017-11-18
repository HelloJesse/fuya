/// <reference path="../../shipxyAPI.js" />
/// <reference path="../../shipxyMap.js" />

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


var dataLatLng1 = [];  
var dataLatLng2 = [];  

//注册添加点事件
function addOptions() {
    //删除叠加物船
    //记录增加点的经纬度，添加线使用
    //dataLatLng[options] = new shipxyMap.LatLng(lat, lng);

    //记录增加点的经纬度，添加线使用
    dataLatLng1[1 - 1] = new shipxyMap.LatLng(29.369900, 112.545500);
    dataLatLng1[2 - 1] = new shipxyMap.LatLng(29.361700, 112.545800);
    dataLatLng1[3 - 1] = new shipxyMap.LatLng(29.355100, 112.548400);
    dataLatLng1[4 - 1] = new shipxyMap.LatLng(29.348700, 112.550900);
    dataLatLng1[5 - 1] = new shipxyMap.LatLng(29.341400, 112.553300);
    dataLatLng1[6 - 1] = new shipxyMap.LatLng(29.337200, 112.556300);
    dataLatLng1[7 - 1] = new shipxyMap.LatLng(29.328100, 112.559100);
    dataLatLng1[8 - 1] = new shipxyMap.LatLng(29.344500, 112.563400);
    dataLatLng1[9 - 1] = new shipxyMap.LatLng(29.305600, 112.565400);

    dataLatLng1[10 - 1] = new shipxyMap.LatLng(29.297200, 112.566800);
    dataLatLng1[11 - 1] = new shipxyMap.LatLng(29.288700, 112.568400);
    dataLatLng1[12 - 1] = new shipxyMap.LatLng(29.285000, 112.570800);
    dataLatLng1[13 - 1] = new shipxyMap.LatLng(29.282000, 112.574900);
    dataLatLng1[14 - 1] = new shipxyMap.LatLng(29.281000, 112.575600);
    dataLatLng1[15 - 1] = new shipxyMap.LatLng(29.282600, 112.579700);
    dataLatLng1[16 - 1] = new shipxyMap.LatLng(29.285100, 112.581900);
    dataLatLng1[17 - 1] = new shipxyMap.LatLng(29.290300, 112.583300);
    dataLatLng1[18 - 1] = new shipxyMap.LatLng(29.296200, 112.585300);
    dataLatLng1[19 - 1] = new shipxyMap.LatLng(29.312300, 113.006200);
    dataLatLng1[20 - 1] = new shipxyMap.LatLng(29.317300, 113.014900);
    dataLatLng1[21 - 1] = new shipxyMap.LatLng(29.318300, 113.026300);
    dataLatLng1[22 - 1] = new shipxyMap.LatLng(29.311900, 113.036900);
    dataLatLng1[23 - 1] = new shipxyMap.LatLng(29.304000, 113.039800);
    dataLatLng1[24 - 1] = new shipxyMap.LatLng(29.286800, 113.044300);
    dataLatLng1[25 - 1] = new shipxyMap.LatLng(29.276700, 113.045100);
    dataLatLng1[26 - 1] = new shipxyMap.LatLng(29.271600, 113.045700);
    dataLatLng1[27 - 1] = new shipxyMap.LatLng(29.266300, 113.048200);
    dataLatLng1[28 - 1] = new shipxyMap.LatLng(29.263700, 113.049900);
    dataLatLng1[29 - 1] = new shipxyMap.LatLng(29.262200, 113.051900);
    dataLatLng1[30 - 1] = new shipxyMap.LatLng(29.261100, 113.054400);
    dataLatLng1[31 - 1] = new shipxyMap.LatLng(29.261700, 113.056600);
    dataLatLng1[32 - 1] = new shipxyMap.LatLng(29.263600, 113.057700);
    dataLatLng1[33 - 1] = new shipxyMap.LatLng(29.266500, 113.058100);
    dataLatLng1[34 - 1] = new shipxyMap.LatLng(29.271900, 113.058700);
    dataLatLng1[35 - 1] = new shipxyMap.LatLng(29.279600, 113.056500);
    dataLatLng1[36 - 1] = new shipxyMap.LatLng(29.284900, 113.054800);
    dataLatLng1[37 - 1] = new shipxyMap.LatLng(29.289900, 113.052900);
    dataLatLng1[38 - 1] = new shipxyMap.LatLng(29.298100, 113.054800);
    dataLatLng1[39 - 1] = new shipxyMap.LatLng(29.298900, 113.067300);
    dataLatLng1[40 - 1] = new shipxyMap.LatLng(29.291300, 113.072800);
    dataLatLng1[41 - 1] = new shipxyMap.LatLng(29.278300, 113.080000);
    dataLatLng1[42 - 1] = new shipxyMap.LatLng(29.275100, 113.083000);
    dataLatLng1[43 - 1] = new shipxyMap.LatLng(29.273800, 113.086600);
    dataLatLng1[44 - 1] = new shipxyMap.LatLng(29.274200, 113.089300);
    dataLatLng1[45 - 1] = new shipxyMap.LatLng(29.277500, 113.093600);

    dataLatLng2[46 - 46] = new shipxyMap.LatLng(29.370200, 113.174300);
    dataLatLng2[47 - 46] = new shipxyMap.LatLng(29.353300, 113.164200);
    dataLatLng2[48 - 46] = new shipxyMap.LatLng(29.347600, 113.162200);
    dataLatLng2[49 - 46] = new shipxyMap.LatLng(29.342700, 113.157600);
    dataLatLng2[50 - 46] = new shipxyMap.LatLng(29.340000, 113.154400);
    dataLatLng2[51 - 46] = new shipxyMap.LatLng(29.333500, 113.143900);
    dataLatLng2[52 - 46] = new shipxyMap.LatLng(29.328000, 113.137600);
    dataLatLng2[53 - 46] = new shipxyMap.LatLng(29.325700, 113.135600);
    dataLatLng2[54 - 46] = new shipxyMap.LatLng(29.324200, 113.133600);
    dataLatLng2[55 - 46] = new shipxyMap.LatLng(29.322100, 113.131900);
    dataLatLng2[56 - 46] = new shipxyMap.LatLng(29.318300, 113.129300);
    dataLatLng2[57 - 46] = new shipxyMap.LatLng(29.313300, 113.127300);
    dataLatLng2[58 - 46] = new shipxyMap.LatLng(29.306700, 113.122200);
    dataLatLng2[59 - 46] = new shipxyMap.LatLng(29.303700, 113.119400);
    dataLatLng2[60 - 46] = new shipxyMap.LatLng(29.301900, 113.117900);
    dataLatLng2[61 - 46] = new shipxyMap.LatLng(29.295700, 113.112300);
    dataLatLng2[62 - 46] = new shipxyMap.LatLng(29.295200, 113.111900);
    dataLatLng2[63 - 46] = new shipxyMap.LatLng(29.291400, 113.110800);
    dataLatLng2[64 - 46] = new shipxyMap.LatLng(29.287600, 113.107500);
    dataLatLng2[65 - 46] = new shipxyMap.LatLng(29.283100, 113.102600);
    dataLatLng2[66 - 46] = new shipxyMap.LatLng(29.276100, 113.094500);
    dataLatLng2[67 - 46] = new shipxyMap.LatLng(29.273200, 113.092600);
    dataLatLng2[68 - 46] = new shipxyMap.LatLng(29.270400, 113.089000);
    dataLatLng2[69 - 46] = new shipxyMap.LatLng(29.272200, 113.086300);
    dataLatLng2[70 - 46] = new shipxyMap.LatLng(29.274500, 113.081500);
    dataLatLng2[71 - 46] = new shipxyMap.LatLng(29.277800, 113.078300);
    dataLatLng2[72 - 46] = new shipxyMap.LatLng(29.286500, 113.072300);
    dataLatLng2[73 - 46] = new shipxyMap.LatLng(29.292400, 113.069400);
    dataLatLng2[74 - 46] = new shipxyMap.LatLng(29.297100, 113.065900);
    dataLatLng2[75 - 46] = new shipxyMap.LatLng(29.298500, 113.063300);
    dataLatLng2[76 - 46] = new shipxyMap.LatLng(29.297800, 113.059700);
    dataLatLng2[77 - 46] = new shipxyMap.LatLng(29.294900, 113.056000);
    dataLatLng2[78 - 46] = new shipxyMap.LatLng(29.291700, 113.055700);
    dataLatLng2[79 - 46] = new shipxyMap.LatLng(29.275800, 113.060300);
    dataLatLng2[80 - 46] = new shipxyMap.LatLng(29.267100, 113.060000);
    dataLatLng2[81 - 46] = new shipxyMap.LatLng(29.261900, 113.058500);
    dataLatLng2[82 - 46] = new shipxyMap.LatLng(29.261200, 113.051000);
    dataLatLng2[83 - 46] = new shipxyMap.LatLng(29.263900, 113.048400);
    dataLatLng2[84 - 46] = new shipxyMap.LatLng(29.267000, 113.046100);
    dataLatLng2[85 - 46] = new shipxyMap.LatLng(29.274000, 113.043100);
    dataLatLng2[86 - 46] = new shipxyMap.LatLng(29.281300, 113.042500);
    dataLatLng2[87 - 46] = new shipxyMap.LatLng(29.291200, 113.040900);
    dataLatLng2[88 - 46] = new shipxyMap.LatLng(29.300700, 113.037500);
    dataLatLng2[89 - 46] = new shipxyMap.LatLng(29.308100, 113.036500);
    dataLatLng2[90 - 46] = new shipxyMap.LatLng(29.310600, 113.035500);

    addPolyline(dataLatLng1, "绿色上游开始");
    addPolyline(dataLatLng2, "红色下游开始上游");
    map.removeOverlayByType(shipxyMap.OverlayType.SHIP);
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
    opts.labelOptions.text = '航线'+flag;
    opts.labelOptions.labelPosition = new shipxyMap.Point(0, 0);
    /*****线*****/
    polyline = new shipxyMap.Polyline('Polyline'+flag, dataLatLng, opts);
    //添加到地图上显示
    map.addOverlay(polyline);
}



