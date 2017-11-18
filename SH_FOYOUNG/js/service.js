/// <reference path="../../shipxyAPI.js" />
/// <reference path="../../shipxyMap.js" />
/// <reference path="myApp.js" />
/// <reference path="view.js" />
(function () {
    var map = null, util = null, view = null, service = null,
    searchMaxCount = 100, //查询结果最大返回数
    searchOj = null, //API查询工具对象
    trackObj = null, //API轨迹工具对象
    trackObjs = [], //缓存轨迹列表
    regionObj = null, //API区域对象
    regionData = [], //区域内的船舶数据，源数据
    regionIdHash = {}, //区域内船舶数据的id索引，哈希表
    regionShipList = {}, //区域内船舶Ship对象列表
    //filterData = null, //存储筛选出来的船舶
    initialized = false; //区域船舶数据是否初始化完毕

    //初始化区域
    var initRegion = function () {
        var region = new shipxyAPI.Region(); //区域范围
        //区域范围数据
        //region.data = [{ lat: 29.49, lng: 113 }, { lat: 29.49, lng: 113.18 }, { lat: 29.39, lng: 113.18 }, { lat: 29.39, lng: 113 }];

        //上海区域
        //region.data = [{ lat: 30.870757, lng: 121.61674 }, { lat: 30.870757, lng: 121.928345 }, { lat: 30.727818, lng: 121.928345 }, { lat: 30.727818, lng: 121.61674 }];

        //武汉测试区域
        //region.data = [{ lat: 30.35467, lng: 113.88813 }, { lat: 30.35467, lng: 114.296033 }, { lat: 30.278683, lng: 114.296033 }, { lat: 30.278683, lng: 113.88813 }];

        region.data = [{ lat: 30.73476, lng: 113.58764}, { lat: 30.73476, lng: 114.63682}, { lat: 29.838256, lng: 114.63682}, { lat: 29.838256, lng: 113.58764}];

        // 当前测试区域为 山东半岛附近   ，请根据业务需要修改您的需要监控的区域。
        // 或者联系我们商务 帮您来划定区域。 
        regionObj = new shipxyAPI.AutoShips(region, shipxyAPI.Ships.INIT_REGION); //API区域对象
        regionObj.getShips(regionCallback); //调用API的请求批量船舶数据接口
        regionObj.setAutoUpdateInterval(30); //调用API的设置自动更新间隔=30秒
        regionObj.startAutoUpdate(regionCallback); //调用API的开启自动更新
        /***画出多边形区域***/
        //var opts = new shipxyMap.PolygonOptions();
        //opts.zoomlevels = [11, 12];
        //opts.zIndex = shipxyMap.ZIndexConst.MAP_LAYER; //多边形区域绘制在Map层，船舶层之下，以防止船舶被遮盖
        //var polygon = new shipxyMap.Polygon('myRegion1', region.data, opts);
        //map.addOverlay(polygon);
        //map.locateOverlay(polygon, 12); //初始化定位到10级
    };
    //数据请求返回
    var regionCallback = function (status) {
        if (status == 0) {//成功
            var datas = this.data;
            if (datas && datas.length > 0) {
                var i = 0, len = datas.length, d;
                for (; i < len; i++) {
                    d = datas[i];
                    if (regionIdHash[d.shipId]) {//更新的，通过Id哈希表映射判断
                        var sd = regionIdHash[d.shipId];
                        for (var k in d) {//更新该船的数据内容
                            sd[k] = d[k];
                        }
                    } else {//新增的
                        regionIdHash[d.shipId] = d;
                        regionData.push(d);
                    }
                }
                //未初始化，区域列表显示全部船舶，一旦初始化完毕，不会自动刷新列表，只有当在点击刷新按钮和筛选的时候才会手动刷新列表
                if (!initialized) {
                   // view.showList('regionlist', regionData, true);
                    initialized = true;
                }
                //当有筛选船舶数据，优先显示筛选的船舶
                showShips(regionData);
            }
        } else {//错误
            errorTip(status);
        }
    };

    //根据船舶数据数组显示船舶
    var showShips = function (shipDatas) {
        if (!shipDatas) return;
        var i = 0, len = shipDatas.length, d, opts, ship, hasSelected = false;
        for (; i < len; i++) {
            d = shipDatas[i];
            ship = regionShipList[d.shipId];
            if (!ship) {//新增的
                opts = new shipxyMap.ShipOptions();
                opts.zoomlevels = service.showShipZooms;
                opts.labelOptions.zoomlevels = service.showShipLabelZooms;
                ship = new shipxyMap.Ship(d.shipId, d, opts);
                regionShipList[d.shipId] = ship;
            } else {//更新的
                ship.data = d;
            }
            map.addOverlay(ship);
            //有被选择的船，显示船舶信息框
            if (d.shipId == service.selectedShipId) {
                view.showShipWin(d);
                hasSelected = true;
            }
        }
        //无被选择的船，关闭船舶信息框
        if (!hasSelected) {
            view.hideShipWin();
        }
    };

    //船舶点击事件处理函数
    var addShipClickEvent = function () {
        var EventObj = shipxyMap.Event;
        //调用API的注册船舶事件接口
        map.addShipEventListener(EventObj.CLICK, function (event) {
            //从缓存中来获取数据
            var shipId = event.overlayId;
            var ship = map.getOverlayById(shipId);
            if (ship) {
                service.selectShip(shipId); //框选
                view.showShipWin(ship.data); //显示船舶信息框
            }
            //请求最新数据来展示
            var that = this;
            var ships = new shipxyAPI.Ships(shipId, shipxyAPI.Ships.INIT_SHIPID);
            ships.getShips(function (status) {
                if (status == 0) {
                    var data = this.data[0];
                    if (data) {
                        if (ship) ship.data = data;
                        else ship = new shipxyMap.Ship(data.shipId, data);
                        service.selectShip(shipId); //框选
                        view.showShipWin(ship.data); //显示船舶信息框
                    }
                }
            });
        });
        map.addShipEventListener(EventObj.MOUSE_OVER, function (event) {
            var shipId = event.overlayId;
            var ship = map.getOverlayById(shipId);
            view.showShipTip(ship.data, event.latLng); //显示船舶简单信息提示
        });
        map.addShipEventListener(EventObj.MOUSE_OUT, function (event) {
            view.hideShipTip(); //隐藏船舶简单信息提示
        });
    };

    //轨迹点鼠标移上事件处理函数
    var trackover = function (event) {
        var track = map.getOverlayById(event.overlayId);
        var shipData = map.getOverlayById(track.data.shipId).data;
        view.showTrackTip({ name: shipData.name, callsign: shipData.callsign, MMSI: shipData.MMSI, IMO: shipData.IMO }, event.extendData); //显示轨迹点信息提示
    };
    //轨迹点鼠标移出事件处理函数
    var trackout = function (event) {
        view.hideTrackTip(); //隐藏轨迹点信息提示
    };

    //定位该船
    var locate = function (ship) {
        map.addOverlay(ship, true); //显示船
        service.selectShip(ship.data.shipId); //框选船
        map.locateOverlay(ship, service.locateShipZoom); //定位船舶
        view.showShipWin(ship.data); //显示船舶信息框
    };

    //错误提示
    var errorTip = function (errorCode) {
        switch (errorCode) {
            case 1:
                myApp.Message.error('网站服务出错！错误原因：服务过期。错误代码：1。');
                break;
            case 2:
                myApp.Message.error('网站服务出错！错误原因：服务无效或被锁定。错误代码：2。');
                break;
            case 3:
                myApp.Message.error('网站服务出错！错误原因：域名错误。错误代码：3。');
                break;
            case 4:
                myApp.Message.error('网站服务出错！错误原因：请求的数据量过大。错误代码：4。');
                break;
            case 100:
                myApp.Message.error('网站服务出错！错误原因：未知。错误代码：100。');
                break;
        }
    };

    myApp.service = {
        //初始化服务，包括注册事件、初始化请求等等
        init: function () {
            map = myApp.map;
            util = myApp.util;
            view = myApp.view;
            service = this;
            initRegion();
            addShipClickEvent();
        },
        selectedShipId: '-1', //被选择的船舶shipId
        locateShipZoom: 12, //船舶定位级别
        showShipZooms: [1, 18], //显示船舶的级别序列：10到18级
        showShipLabelZooms: [1, 18], //显示船舶标签的级别序列：15到18级

        //选择船舶
        selectShip: function (shipId) {
            if (shipId == this.selectedShipId) return;
            //先清除原先被选择船舶的选择框
            this.unselectShip(this.selectedShipId);
            var ship = map.getOverlayById(shipId);
            if (ship) {
                ship.options.isSelected = true;
                ship.options.zoomlevels = [1, 18]; //选择的船，所有级别都显示
                map.addOverlay(ship, true);
                this.selectedShipId = shipId;
            }
        },

        //反选船舶
        unselectShip: function (shipId) {
            if (shipId == '-1') return;
            var ship = map.getOverlayById(shipId);
            if (ship) {
                ship.options.isSelected = false;
                ship.options.zoomlevels = this.showShipZooms;
                map.addOverlay(ship);
                this.selectedShipId = -1; //清除被选船舶shipId缓存
            }
        },

        //定位一条船
        locateShip: function (shipId) {
            var ship = map.getOverlayById(shipId);
            if (ship) {//缓存里存在，定位
                locate(ship);
            }
            //请求最新数据来定位
            var that = this;
            var ships = new shipxyAPI.Ships(shipId, shipxyAPI.Ships.INIT_SHIPID);
            ships.getShips(function (status) {
                if (status == 0) {
                    var data = this.data[0];
                    if (data) {
                        if (ship) ship.data = data;
                        else ship = new shipxyMap.Ship(data.shipId, data);
                        locate(ship);
                    } else {
                        service.unselectShip(that.selectedShipId); //框选船舶
                        view.showShipWin(that.getEmptyShipInfo(shipId)); //显示船舶信息框
                    }
                }
            });
        },

        ////根据关键字查询船舶
        //searchShip: function (key) {
        //    if (!key || key == '请输入船名、呼号、MMSI或IMO') { return; }
        //    if (!searchOj) {
        //        searchOj = new shipxyAPI.Search(); //构建API查询工具对象
        //    }
        //    var that = this;
        //    //调用API查询船舶接口
        //    searchOj.searchShip({ keyword: key, max: searchMaxCount }, function (status) {
        //        var data = this.data;
        //        if (status == 0 && data && data.length > 0) {//当有结果，先定位第一条结果到醒目位置
        //            that.locateShip(data[0].shipId);
        //        }
        //        view.showList('searchlist', data, true); //刷新搜索结果列表
        //    });
        //},

        //查询轨迹
        searchTrack: function (shipId, btime, etime) {
            this.abortSearchTrack();
            var that = this, EventObj = shipxyMap.Event;
            view.setTrackMsg('正在查询轨迹，请稍候...');
            trackObj = new shipxyAPI.Tracks();
            //调用API的查询轨迹接口
            trackObj.getTrack(shipId, btime, etime, function (status) {
                //显示轨迹
                var trackData = this.data;
                if (status == 0 && trackData && trackData.data && trackData.data.length > 0) {
                    var trackId = trackData.trackId;
                    view.setTrackMsg('');
                    var len = trackObjs.length, td;
                    while (len--) {
                        td = trackObjs[len].data;
                        //重复查询的，先从缓存里删除
                        if (td && td.trackId == trackId) {
                            trackObjs.splice(len, 1);
                            that.delTrack(trackId);
                            break;
                        }
                    }
                    var opts = new shipxyMap.TrackOptions();
                    opts.strokeStyle.color = 0x0000ff;
                    opts.pointStyle.strokeStyle.color = 0x0000ff;
                    opts.labelOptions.borderStyle.color = 0x0000ff;
                    var track = new shipxyMap.Track(trackId, trackData, opts);
                    map.addOverlay(track);
                    //注册轨迹点事件
                    map.addEventListener(track, EventObj.TRACKPOINT_MOUSEOVER, trackover);
                    map.addEventListener(track, EventObj.TRACKPOINT_MOUSEOUT, trackout);
                    trackObjs.push(trackObj); //存储当前的轨迹
                    view.showList("tracklist", trackObjs, true); //显示轨迹列表
                    trackObj = null;
                } else {
                    view.setTrackMsg('暂无轨迹');
                }
            });
        },

        //销毁轨迹查询
        abortSearchTrack: function () {
            if (trackObj) {
                trackObj.abort(); //销毁当前轨迹的请求
                trackObj = null;
                view.setTrackMsg('');
            }
        },

        //删除轨迹
        delTrack: function (trackId) {
            var track = map.getOverlayById(trackId), EventObj = shipxyMap.Event;
            if (track) {
                //移除轨迹点事件
                map.removeEventListener(track, EventObj.MOUSE_OVER, trackover);
                map.removeEventListener(track, EventObj.MOUSE_OUT, trackout);
                map.removeOverlay(track); //删除轨迹显示
                var len = trackObjs.length, td;
                //删除轨迹数据缓存
                while (len--) {
                    td = trackObjs[len].data;
                    if (td && td.trackId == trackId) {
                        trackObjs.splice(len, 1);
                        break;
                    }
                }
                //刷新列表
                view.showList("tracklist", trackObjs, true);
            }
        },

        //定位轨迹
        locateTrack: function (trackId) {
            var track = map.getOverlayById(trackId);
            if (track) {
                map.locateOverlay(track); //定位
            }
        },

        ////筛选船舶，options：筛选条件集合
        //filter: function (options) {
        //    filterData = [];
        //    var country = options.country, type = options.type, status = options.status, cargoType = options.cargoType;
        //    var length = options.length, beam = options.beam, draught = options.draught;
        //    //当所有条件都为默认值（所有），为全部数据，不筛选
        //    if (country == '所有' && type == '所有' && status == '所有' && cargoType == '所有' && length[0] == 0 && length[1] == 0 && beam[0] == 0 && beam[1] == 0 && draught[0] == 0 && draught[1] == 0) {
        //        filterData = regionData;
        //    } else {//否则，按条件筛选
        //        if (length[0] > length[1] || beam[0] > beam[1] || draught[0] > draught[1]) {
        //            myApp.Message.alert('船长、船宽或吃水的起始条件值不能大于结束条件值。');
        //            return;
        //        }
        //        var i = 0, len = regionData.length, d;
        //        for (; i < len; i++) {
        //            d = regionData[i];
        //            if (country != '所有') {
        //                if (d.country != country) continue;
        //            }
        //            if (type != '所有') {
        //                if (d.type != type) continue;
        //            }
        //            if (status != '所有') {
        //                if (d.status != status) continue;
        //            }
        //            if (cargoType != '所有') {
        //                if (d.cargoType != cargoType) continue;
        //            }
        //            if (length[0] >= 0 && length[1] > 0) {
        //                if (d.length < length[0] || d.length > length[1]) continue;
        //            }
        //            if (beam[0] >= 0 && beam[1] > 0) {
        //                if (d.beam < beam[0] || d.beam > beam[1]) continue;
        //            }
        //            if (draught[0] >= 0 && draught[1] > 0) {
        //                if (d.draught < draught[0] || d.draught > draught[1]) continue;
        //            }
        //            filterData.push(d); //执行到此处的，已经满足了所有条件，筛选出来
        //        }
        //    }
        //    view.showList('regionlist', filterData, true); //刷新列表：筛选数据
        //    this.unselectShip(this.selectedShipId); //清除选择船
        //    map.removeOverlayByType(shipxyMap.OverlayType.SHIP); //删除所有船舶
        //    showShips(filterData); //用筛选数据刷新
        //},

        ////取消筛选，恢复到全部船舶
        //unfilter: function () {
        //    filterData = null;
        //    view.showList('regionlist', regionData, true); //刷新列表：全部数据
        //    showShips(regionData);
        //},

        ////区域列表刷新，手动点击刷新按钮，取出最新的船舶数据刷新区域列表
        //refresh: function () {
        //    view.showList('regionlist', filterData || regionData, true); //当前数据：筛选或者全部
        //},

        //生成空船舶信息
        getEmptyShipInfo: function (shipId) {
            return { shipId: shipId, name: "", callsign: "", MMSI: "", IMO: "", type: "", status: "", length: NaN, beam: NaN, draught: NaN, lat: NaN, lng: NaN,
                heading: NaN, course: NaN, speed: NaN, rot: NaN, dest: "", eta: "", lastTime: NaN, country: "", cargoType: ""
            };
        }
    };
})();