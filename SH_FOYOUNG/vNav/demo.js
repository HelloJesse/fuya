var map;
var _Center_lat = 29.48947;  //加载的纬度
var _Center_Lng = 113.10199; //加载的经度   暂时默认为岳阳为中心点,后期可以扩展为根据不同的基站进行选择
var _zoom = 12; //海图显示的缩放级别
var _Timespan;//定时器
// 查询的范围坐标  左上角                   右上角                              右下角                                          左下角                                            
var _regionData = [{ lat: 29.49, lng: 113 }, { lat: 29.49, lng: 113.18 }, { lat: 29.39, lng: 113.18 }, { lat: 29.39, lng: 113 }]; //当前基站的控制范围，即获取此范围内的船只，后期可以扩展为根据基站不同加载
window.onload = function () {
    var mapOptions = new shipxyMap.MapOptions();
    mapOptions.center = new shipxyMap.LatLng(_Center_lat, _Center_Lng);
    mapOptions.zoom = _zoom;
    mapOptions.mapType = shipxyMap.MapType.CMAP;
    //mapDiv是一个DIV容器的id，用于放置flash地图组件
    map = new shipxyMap.Map('mapDiv', mapOptions); //创建地图实例
    //地图初始化完毕
    shipxyMap.mapReady = function () {
        ShipMap_AddShipsRegion();//默认打开的时候加载船舶
        ShipAutoRegion();//设置船舶的自动更新
    }
}


//自动区域数据的查询与更新，默认为30s
function ShipAutoRegion()
{
    var region = new shipxyAPI.Region(); //区域范围 
    region.data = _regionData;
    var regionObj = new shipxyAPI.AutoShips(region, shipxyAPI.Ships.INIT_REGION);
    regionObj.setAutoUpdateInterval(30);      //调用API的设置自动更新间隔=30 秒 
    regionObj.startAutoUpdate(function (status) {
        if (status == 0) {//成功
            if (this.data && this.data != null && this.data.length > 0) {
                //map.removeAllOverlay();
                var addshipOverlayArray = new Array();
                for (var i = 0; i < this.data.length; i++) {
                    var d = new shipxyAPI.Ship();
                    d = this.data[i];
                    var option = new shipxyMap.ShipOptions(); //默认option
                    var shipOverlay = new shipxyMap.Ship(d.shipId, d, option);
                    addshipOverlayArray.push(shipOverlay);
                }
                map.addOverlays(addshipOverlayArray);
            }
        }
    }); //调用API的开启自动更新 
}

  

//添加某一区域的船只的显示
function ShipMap_AddShipsRegion()
{
    var region = new shipxyAPI.Region();
    region.data = _regionData;
    var ships = new shipxyAPI.Ships(region, shipxyAPI.Ships.INIT_REGION), datas = {};
    ships.getShips(function (status) {
        if (status == 0) {
            if (this.data && this.data != null && this.data.length > 0) {
                //map.removeAllOverlay();
                var addshipOverlayArray = new Array();
                for (var i = 0; i < this.data.length; i++)
                {
                    var d = new shipxyAPI.Ship();
                    d = this.data[i];
                    var option = new shipxyMap.ShipOptions(); //默认option
                    var shipOverlay = new shipxyMap.Ship(d.shipId, d, option);
                    addshipOverlayArray.push(shipOverlay);
                }
                map.addOverlays(addshipOverlayArray);
            }
        }  
    });
    
}