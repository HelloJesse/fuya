/// <reference path="../../shipxyAPI.js" />
/// <reference path="../../shipxyMap.js" />
/// <reference path="myApp.js" />
/// <reference path="util.js" />
/// <reference path="service.js" />
/// <reference path="xwin.js" />
mini.parse();
var _DataMainTask;

(function () {
    var flagsPath = 'http://api.shipxy.com/apiresource/flags/', //国旗图标服务器路径
        map = null, util = null, view = null, service = null,
        shipWin = null, //船舶信息框
        shipWinWidth = 420, //船舶信息框宽度
        shipInfo = null, //船舶信息
        shipTip = null, //船舶简单信息提示框
        trackTip = null, //轨迹点信息框
        pageSize = 20, //分页大小
        regionPaging = null, //区域分页
        searchPaging = null, //搜索分页
        filterWin = null, //筛选框
        filterWinWidth = 480;

    var country_zh = { 'ADL': '阿黛利地', 'ALB': '阿尔巴尼亚', 'DZA': '阿尔及利亚', 'AFG': '阿富汗', 'ARG': '阿根廷',
        'USA': '阿拉斯加', 'ARE': '阿联酋', 'ABW': '阿鲁巴', 'OMN': '阿曼', 'AZE': '阿塞拜疆',
        'ASL': '阿森松岛', 'EGY': '埃及', 'ETH': '埃塞俄比亚', 'IRL': '爱尔兰共和国', 'EST': '爱沙尼亚',
        'AND': '安道尔', 'AGO': '安哥拉', 'AIA': '安圭拉', 'ATG': '安提瓜和巴布达', 'AUT': '奥地利',
        'AUS': '澳大利亚', 'MAC': '中国澳门', 'BRB': '巴巴多斯', 'PNG': '巴布亚新几内亚', 'BHS': '巴哈马',
        'PAK': '巴基斯坦', 'PRY': '巴拉圭', 'PSE': '巴勒斯坦', 'BHR': '巴林', 'PAN': '巴拿马',
        'BRA': '巴西', 'BLR': '白俄罗斯', 'BMU': '百慕大', 'BGR': '保加利亚', 'PRK': '朝鲜',
        'MNP': '北马里亚纳群岛', 'BEN': '贝宁', 'BEL': '比利时', 'ISL': '冰岛', 'PRI': '波多黎各',
        'BIH': '波黑', 'POL': '波兰', 'BOL': '玻利维亚', 'BLZ': '伯利兹', 'BWA': '伯兹瓦纳',
        'BTN': '不丹', 'BFA': '布基纳法索', 'BDI': '布隆迪', 'GNQ': '赤道几内亚', 'DNK': '丹麦',
        'DNK': '丹麦', 'DEU': '德国', 'TGO': '多哥', 'DOM': '多米尼加共和国', 'DOM': '多米尼加联邦',
        'RUS': '俄罗斯联邦', 'ECU': '厄瓜多尔', 'ERI': '厄立特里亚', 'FRA': '法国', 'FRO': '法罗群岛',
        'PYF': '法属玻利尼西亚', 'VAT': '梵地冈', 'PHL': '菲律宾', 'FJI': '斐济', 'FIN': '芬兰',
        'CPV': '佛得角', 'FLK': '福克兰群岛', 'GMB': '冈比亚', 'COG': '刚果', 'COL': '哥伦比亚',
        'CRI': '哥斯达黎加', 'GRD': '格林纳达', 'GRL': '格陵兰群岛', 'GEO': '格鲁吉亚', 'CUB': '古巴',
        'GLP': '瓜德罗普岛（法属）', 'GUY': '圭亚那', 'KAZ': '哈萨克斯坦', 'HTI': '海地', 'NLD': '荷兰',
        'ANT': '荷兰属地', 'MNE': '黑山共和国', 'HND': '洪都拉斯', 'KIR': '基里巴斯', 'DJI': '吉布提',
        'KGZ': '吉尔吉斯斯坦', 'GIN': '几内亚', 'GNB': '几内亚比绍', 'CAN': '加拿大', 'GHA': '加纳',
        'GAB': '加蓬', 'KHM': '柬埔塞', 'CZE': '捷克共和国', 'ZWE': '津巴布韦', 'CMR': '喀麦隆',
        'QAT': '卡塔尔', 'CYM': '开曼群岛（英属）', 'ATF': '凯尔盖朗群岛', 'CCK': '科科斯群岛',
        'COM': '科摩罗', 'CIV': '科特迪瓦', 'KWT': '科威特', 'HRV': '克罗蒂亚', 'ATF': '克罗泽群岛',
        'KEN': '肯尼亚', 'COK': '库克群岛', 'LVA': '拉脱维亚', 'LSO': '莱索托', 'LAO': '老挝人民共和国',
        'LBN': '黎巴嫩', 'LTU': '立陶宛', 'LBR': '利比里亚', 'LBY': '利比亚', 'LIE': '列支敦士登',
        'REU': '留尼汪岛', 'LUX': '卢森堡', 'RWA': '卢旺达', 'ROU': '罗马尼亚', 'MDG': '马达加斯加',
        'MDR': '马德拉', 'MDV': '马尔代夫', 'MLT': '马尔他', 'MWI': '马拉维', 'MYS': '马来西亚',
        'MLI': '马里', 'MKD': '马其顿', 'MHL': '马绍尔群岛', 'MTQ': '马提尼克岛（法属）', 'MUS': '毛里求斯',
        'MRT': '毛里塔尼亚', 'USA': '美国', 'ASM': '美属萨摩亚', 'MNG': '蒙古', 'MSR': '蒙塞拉特岛',
        'BGD': '孟加拉国', 'PER': '秘鲁', 'FSM': '密克罗尼西亚', 'MMR': '缅甸', 'MDA': '摩尔多瓦',
        'MAR': '摩洛哥', 'MOZ': '莫桑比克', 'MEX': '墨西哥', 'NAM': '纳米比亚', 'KOR': '韩国',
        'ZAF': '南非', 'NRU': '瑙鲁', 'NPL': '尼泊尔', 'NIC': '尼加拉瓜', 'NER': '尼日尔',
        'NGA': '尼日利亚', 'NIU': '纽埃', 'NOR': '挪威', 'PLW': '帕劳', 'PCN': '皮特克恩岛',
        'PRT': '葡萄牙', 'JPN': '日本', 'SWE': '瑞典', 'SWE': '瑞典', 'SLV': '萨尔瓦多',
        'WSM': '萨摩亚', 'SRB': '塞尔维亚', 'SLE': '塞拉利昂', 'SEN': '塞内加尔', 'CYP': '塞浦路斯',
        'SYC': '塞舌尔', 'SAU': '沙特阿拉伯', 'ATF': '圣保罗阿姆斯特丹岛', 'CXR': '圣诞岛（英属）',
        'STP': '圣多美和普林西比', 'SHN': '圣海伦娜', 'KNA': '圣基茨和尼维斯', 'LCA': '圣露西亚岛',
        'SMR': '圣马力诺', 'SPM': '圣皮埃尔和密克隆', 'VCT': '圣文森特和格林纳丁斯', 'LKA': '斯里兰卡',
        'SVK': '斯洛伐克', 'SVN': '斯洛文尼亚', 'SWZ': '斯威士兰', 'SDN': '苏丹', 'SUR': '苏里南',
        'SLB': '所罗门群岛', 'SOM': '索马里', 'TWN': '台湾', 'THA': '泰国', 'TZA': '坦桑尼亚',
        'TZA': '坦桑尼亚', 'TON': '汤加', 'TCA': '特克斯和凯科斯群岛', 'TTO': '特立尼达和多巴哥',
        'TUN': '突尼斯', 'TUV': '图瓦卢', 'TUR': '土耳其', 'TKM': '土库曼斯坦', 'WLF': '瓦利斯和富图纳',
        'VUT': '瓦努阿图', 'GTM': '危地马拉', 'VGB': '维京群岛', 'VEN': '委内瑞拉', 'BRN': '文莱达鲁萨兰国',
        'UGA': '乌干达', 'UKR': '乌克兰', 'URY': '乌拉圭', 'UZB': '乌兹别克斯坦', 'ESP': '西班牙',
        'GRC': '希腊', 'HKG': '中国香港', 'SGP': '新加坡', 'NCL': '新喀里多尼亚', 'NZL': '新西兰',
        'HUN': '匈牙利', 'SYR': '叙利亚', 'JAM': '牙买加', 'ARM': '亚美尼亚', 'AZS': '亚速尔群岛',
        'YEM': '也门', 'IRQ': '伊拉克', 'IRN': '伊朗', 'ISR': '以色列', 'ITA': '意大利',
        'IND': '印度', 'IDN': '印度尼西亚', 'GBR': '英国', 'JOR': '约旦', 'VNM': '越南',
        'ZMB': '赞比亚', 'TCD': '乍得', 'GIB': '直布罗陀', 'CHL': '智利', 'CAF': '中非共和国',
        'CHN': '中国'
    };
    var shipTypeList = ['客船', '货船', '油轮', '拖轮', '捕捞', '拖引', '引航船', '搜救船', '执法艇', '医疗船', '娱乐船', '高速船', '潜水作业', '帆船航行', '地效应船', '参与军事行动', '疏浚或水下作业', '其他'];
    var cargoTypeList = ['A 类危险品', 'B 类危险品', 'C 类危险品', 'D 类危险品'];
    var navStatusList = ['锚泊', '靠泊', '失控', '搁浅', '操作受限', '吃水受限', '捕捞作业', '在航(主机推动)', '靠船帆提供动力'];
    //初始化布局
    var initLayout = function () {
        var body = document.body,
        //header = util.getElement('header'),
        //lister = util.getElement('lister'),
        mainer = util.getElement('mainer'),
        width = body.clientWidth  + 'px',
        height = body.clientHeight  + 'px';
        mainer.style.width = width;
        mainer.style.height = height;
    };

    //初始化左侧列表
    //var initList = function () {
    //    var box = util.getElement('listbox');
    //    util.addEvent(box, 'click', function (event) {
    //        util.stopPropagation(event);
    //        var target = event.target, tag = target.tagName, cmd;
    //        if (tag == 'A') {
    //            cmd = target.getAttribute('data-cmd');
    //            if (cmd == 'listhead') {
    //                toggleList(box, target.parentNode.parentNode);
    //            } else if (cmd == 'ship') {//船舶
    //                var selectedShipLink = util.getElement('selectedShip', box);
    //                if (selectedShipLink) {//若有被选择的定位项，先清除
    //                    selectedShipLink.className = '';
    //                    selectedShipLink.id = '';
    //                }
    //                target.className = 'sel';
    //                target.id = 'selectedShip';
    //                var shipId = target.getAttribute('data-shipId');
    //                var color = target.getAttribute('data-color');
    //                if (shipId) {
    //                    var opts = null;
    //                    if (color) opts = { color: color };
    //                    service.locateShip(shipId, opts);
    //                }
    //            } else if (cmd == 'track') {//轨迹
    //                var selectedTrackLink = util.getElement('selectedTrack', box);
    //                if (selectedTrackLink) {//若有被选择的定位项，先清除
    //                    selectedTrackLink.className = '';
    //                    selectedTrackLink.id = '';
    //                }
    //                target.className = 'sel';
    //                target.id = 'selectedTrack';
    //                var trackId = target.getAttribute('data-trackId');
    //                if (trackId) {
    //                    service.locateTrack(trackId);
    //                }
    //            }
    //        } else if (tag == 'I') {
    //            cmd = target.getAttribute('data-cmd');
    //            if (cmd == 'track') {//轨迹
    //                var trackId2 = target.getAttribute('data-trackId');
    //                if (trackId2) {
    //                    service.delTrack(trackId2);
    //                }
    //            }
    //        }
    //        util.preventDefault(event);
    //    });
    //    //初始化，打开区域列表
    //    view.showList('regionlist', null, true);
    //};

    //展开/闭合列表
    var toggleList = function (box, list) {
        if (!box || !list) return;
        var a = list.getElementsByTagName('A')[0];
        var b = a.className == 'on';
        a.className = b ? '' : 'on'; //+-图标切换
        var other = null, len = box.children.length;
        //关闭其他展开的列表
        for (var i = 0; i < len; i++) {
            other = box.children[i];
            if (other != list) {//非本身列表
                if (other.getAttribute('data-toggle') == 'on') {
                    arguments.callee(box, other); //递归
                }
            }
        }
        var ul = list.getElementsByTagName('UL')[0];
        if (ul) {
            ul.className = b ? '' : 'on'; //展开/闭合列表
        }
        list.setAttribute('data-toggle', b ? 'off' : 'on'); //更改展开状态
    };

    var getShipName = function (data) {
        return data.name ? data.name : data.MMSI;
    };

    //生成左侧列表内容
    var createList = function (ul, listname, data) {
        var s = '', i = 0, len = 0, d, parent;
        switch (listname) {
            case 'regionlist':
                parent = util.getElement('region_pagebox', ul);
                if (!parent) {
                    ul.innerHTML = '<div id="region_pagebox" class="pagebox"></div>';
                    parent = util.getElement('region_pagebox', ul);
                }
                if (data && data.length > 0) {
                    if (!regionPaging) {
                        regionPaging = new Paging({
                            size: pageSize,
                            parent: parent,
                            callback: function (pageData) {
                                s = '';
                                for (i = 0, len = pageData.length; i < len; i++) {
                                    d = pageData[i];
                                    s += '<li>';
                                    var src, title;
                                    if (d.country) {
                                        src = flagsPath + d.country + '.png';
                                        title = country_zh[d.country];
                                    } else {
                                        src = flagsPath + 'qita.png';
                                        title = '其他';
                                    }
                                    s += '<img src="' + src + '" title="' + title + '"/>';
                                    s += '<a href="javascript:void(0)" title="点击定位此船" data-cmd="ship" data-shipId="' + d.shipId + '"';
                                    if (service.selectedShipId == d.shipId) s += ' class="sel" id="selectedShip"';
                                    s += '>' + getShipName(d) + '</a></li>';
                                }
                                parent.innerHTML = s;
                            }
                        });
                    }
                    regionPaging.setValue(null, data);
                } else {
                    parent.innerHTML = '<li><a href="javascript:void(0)" title="点击定位此船" class="nodata" data-shipId="">船名</a></li>';
                }
                util.getElement('regioncount').innerHTML = '(' + (data ? data.length : 0) + ')';
                break;
            case 'tracklist':
                if (data && data.length > 0) {
                    s += '<div id="trackbox">';
                    for (i = 0, len = data.length; i < len; i++) {
                        d = data[i].data;
                        var ship = map.getOverlayById(d.shipId);
                        var name = ship ? getShipName(ship.data) : d.shipId;
                        var tip = util.formatTime(d.startTime) + ' - ' + util.formatTime(d.endTime);
                        s += '<li><a href="javascript:void(0)" title="' + tip + '" data-cmd="track" data-trackId="' + d.trackId + '">' + name + '</a>' +
                        '<i class="del" title="删除" data-cmd="track" data-trackId="' + d.trackId + '"></i></li>';
                    }
                    s += '</div>';
                } else {
                    s = '<li><a href="javascript:void(0)" class="nodata" data-trackId="">暂无轨迹</a></li>';
                }
                ul.innerHTML = s;
                util.getElement('trackcount').innerHTML = '(' + len + ')';
                break;
            case 'searchlist':
                parent = util.getElement('search_pagebox', ul);
                if (!parent) {
                    ul.innerHTML = '<div id="search_pagebox" class="pagebox"></div>';
                    parent = util.getElement('search_pagebox', ul);
                }
                if (data && data.length > 0) {
                    if (!searchPaging) {
                        searchPaging = new Paging({
                            size: pageSize,
                            parent: parent,
                            callback: function (pageData) {
                                s = '';
                                for (i = 0, len = pageData.length; i < len; i++) {
                                    d = pageData[i];
                                    s += '<li>';
                                    var src, title;
                                    if (d.country) {
                                        src = flagsPath + d.country + '.png';
                                        title = country_zh[d.country];
                                    } else {
                                        src = flagsPath + 'qita.png';
                                        title = '其他';
                                    }
                                    s += '<img src="' + src + '" title="' + title + '"/>';
                                    s += '<a href="javascript:void(0)" title="点击定位此船：' + d.shipId + '" data-cmd="ship" data-shipId="' + d.shipId + '"';
                                    if (service.selectedShipId == d.shipId) s += ' class="sel" id="selectedShip"';
                                    s += '>' + getShipName(d) + '</a></li>';
                                }
                                parent.innerHTML = s;
                            }
                        });
                    }
                    searchPaging.setValue(null, data);
                } else {
                    parent.innerHTML = '<li><a href="javascript:void(0)" class="nodata">暂无搜索结果</a></li>';
                }
                util.getElement('searchcount').innerHTML = '(' + (data ? data.length : 0) + ')';
                break;
        };
    };

    //分页组件
    var Paging = function (opts) {
        this.data = opts.data || []; //源数据
        this.now = opts.now || 1; //当前页，默认为第一页
        this.size = opts.size || 20; //页大小，默认为20个
        this.total = this.data.length; //总个数
        this.callback = opts.callback || function () { }; //页切换时触发的回调函数
        this.mini = 1;
        this.step = opts.step || 1; //步频，默认为1页1页的切换
        this.prev = opts.prev || '上页'; //上页
        this.next = opts.next || '下页'; //下页
        this.sum = Math.ceil(this.total / this.size); //总页数，由总个数/页大小计算得到
        this.parent = opts.parent || document; //分页组件将要添加到的父容器，若不指定，默认为document
        this.box = null; //组件
    };
    Paging.prototype = {
        //设置当前页值和源数据，将会自动刷新组件，并从源数据中切割出当前页部分的数据，最后触发callback，传出当前页数据
        //若只设置当前页，看做是切换页
        //若设置了源数据，看做是刷新整个组件，但会保持组件的当前页状态
        setValue: function (now, data) {
            if (now) this.now = now; //当前页
            if (data) {//更新源数据：重置总数、总页数
                this.data = data;
                this.total = this.data.length;
                this.sum = Math.ceil(this.total / this.size);
            }
            //切割当前页数据，然后触发callback
            this.callback.call(this, this.data.slice((this.now - 1) * this.size, this.now * this.size));
            if (!this.box) {
                this.box = document.createElement('DIV');
                this.box.className = 'pageLink';
                var that = this;
                util.addEvent(this.box, 'click', function (event) {
                    util.stopPropagation(event);
                    var target = event.target;
                    if (target.tagName == 'A') {
                        that.change(parseInt(target.getAttribute('data-page')));
                    }
                    util.preventDefault(event);
                });
            }
            this.box.innerHTML = this.create();
            this.parent.appendChild(this.box); //加入父容器
        },
        //生成组件
        create: function () {
            if (this.sum <= 1) return '';
            var that = this;
            var end, ret = '', len = this.step * 2 + 1, start = this.now - this.step, each = function (page, text, cls) {
                if (page === '') return '<span>' + text + '</span>';
                if (page < 1 || page > that.sum) return '';
                if (page == that.now) return text ? ('<span>' + text + '</span>') : ('<span>' + page + '</span>');
                return '<a href="javascript:void(0)" data-page="' + page + '" ' + (cls || '') + '>' + (text || page) + '</a>';
            };
            if (start < 1) start = 1;
            if (start + len > this.sum) start = this.sum - len;
            end = start + len;
            if (end > this.sum) end = this.sum;
            if (this.mini) ret += each(1, '首页');
            if (this.now == 1) ret += each('', this.prev);
            if (this.now > 1) ret += each(this.now - 1, this.prev, ' class="pre"');
            if (this.mini) {
                ret += '<em>' + this.now + '</font>/' + this.sum + '</em>';
            } else {
                if (start > 1) ret += each(1);
                if (start > 2) ret += each(Math.floor((1 + start) / 2), '...');
                for (var i = start; i < end; i++) ret += each(i);
                if (end < this.sum) ret += each(Math.floor((this.sum + end) / 2), '...');
                if (end < this.sum + 1) ret += each(this.sum);
            }
            if (this.now < this.sum) ret += each(this.now + 1, this.next, ' class="next"');
            if (this.now == this.sum) ret += each('', this.next);
            if (this.mini) ret += each(this.sum, '末页');
            return ret;
        },
        //切换页
        change: function (page) {
            this.setValue(page);
        }
    };

    //文本框占位文字
    var PlaceHolder = function () { this.init.apply(this, arguments) };
    PlaceHolder.prototype = {
        init: function (input, text) {
            if (typeof input == 'string') input = document.getElementById(input);
            var that = this; this.input = input; this.setText(text || input.getAttribute('placeholder') || '');
            input.onfocus = function () { if (!this.value || this.value == that.text) this.value = ''; this.style.color = 'black' };
            input.onblur = function () { if (!this.value) this.value = that.text; if (this.value == that.text) this.style.color = '#ccc' };
            input.onblur();
        },
        setText: function (text) { this.text = text; if (this.input.style.color) this.input.value = this.text }
    };

    //初始化搜索：搜索框占位文本、注册查询按钮点击事件(触发查询船舶服务)
    //var initSearch = function () {
    //    var input = util.getElement('searchKey');
    //    new PlaceHolder(input, '请输入船名、呼号、MMSI或IMO');
    //    util.addEvent(util.getElement('btnQuery'), 'click', function (event) {
    //        util.stopPropagation(event);
    //        service.searchShip(input.value); //查询船舶
    //        input.focus();
    //        util.preventDefault(event);
    //    });
    //};

    //构建船舶信息框数据区的html内容
    var createShipWinDataHtml = function (data) {

        _DataMainTask = data;

        var IMO = '', heading = '', course = '', speed = '', length = '', beam = '', draught = '', dest = '', lat = '', lng = '', lastTime = '';
        //无效IMO
        IMO = data.IMO == '2147483647' ? '' : data.IMO.toString();
        heading = (data.heading < 0 || data.heading > 360) ? '未知' : (data.heading + '度');
        course = (data.course < 0 || data.course > 360) ? '未知' : (data.course + '度');
        speed = isNaN(data.speed) ? '' : (data.speed == 0 ? '0.0' : (Math.round((data.speed) * 10) / 10) + '节'); //转成节
        if (data.length > 0) { length = data.length + '米'; }
        if (data.beam > 0) { beam = data.beam + '米'; }
        if (data.draught > 0) { draught = data.draught + '米'; }
        dest = data.dest.replace(/\</g, '&lt;').replace(/\>/g, '&gt;');
        if (!data.MMSI) {
            heading = '';
            course = '';
        }
        lat = isNaN(data.lat) ? '' : util.formatLat(data.lat);
        lng = isNaN(data.lng) ? '' : util.formatLng(data.lng);
        lastTime = isNaN(data.lastTime) ? '' : util.formatTime(data.lastTime);
        return '<table id="infoTable" class="xinfo"><tr>'
            + '<td class="ll">船名：</td><td><div class="o" title="' + data.name + '">' + data.name + '</div></td>'
            + '<td class="l">纬度：</td><td><div class="ro" title="' + lat + '">' + lat + '</div></td>'
            + '</tr><tr>'
            + '<td>呼号：</td><td><div class="o" title="' + data.callsign + '">' + data.callsign + '</div></td>'
            + '<td class="l">经度：</td><td><div class="ro" title="' + lng + '">' + lng + '</div></td>'
            + '</tr><tr>'
            + '<td>MMSI：</td><td><div class="o" title="' + data.MMSI + '">' + data.MMSI + '</div></td>'
            + '<td class="l">船首向：</td><td><div class="ro" title="' + heading + '">' + heading + '</div></td>'
            + '</tr><tr>'
            + '<td>IMO：</td><td><div class="o" title="' + IMO + '">' + IMO + '</div></td>'
            + '<td class="l">航迹向：</td><td><div class="ro" title="' + course + '">' + course + '</div></td>'
            + '</tr><tr>'
            + '<td>船籍：</td><td><div class="o" title="' + (country_zh[data.country] || '其他') + '">' + (country_zh[data.country] || '其他') + '</div></td>'
            + '<td class="l">航速：</td><td><div class="ro" title="' + speed + '">' + speed + '</div></td>'
            + '</tr><tr>'
            + '<td>类型：</td><td><div class="o" title="' + data.type + '">' + data.type + '</div></td>'
            + '<td class="l">货物类型：</td><td><div class="ro" title="' + data.cargoType + '">' + data.cargoType + '</div></td>'
            + '</tr><tr>'
            + '<td>状态：</td><td><div class="o" title="' + data.status + '">' + data.status + '</div></td>'
            + '<td class="l">目的地：</td><td><div class="ro" title="' + dest + '">' + dest + '</div></td>'
            + '</tr><tr>'
            + '<td>船长：</td><td><div class="o" title="' + length + '">' + length + '</div></td>'
            + '<td class="l">预到时间：</td><td><div class="ro" title="' + data.eta + '">' + data.eta + '</div></td>'
            + '</tr><tr>'
            + '<td>船宽：</td><td><div class="o" title="' + beam + '">' + beam + '</div></td>'
            + '<td class="l">最后时间：</td><td><div class="ro" title="' + lastTime + '">' + lastTime + '</div></td>'
            + '</tr><tr>'
            + '<td>吃水：</td><td><div class="o" title="' + draught + '">' + draught + '</div></td>'
            + '<td class="l"></td><td class="r"></td>'
            + '</tr></table>';
    };

    //构建船舶信息框内容：包括数据区、按钮区、轨迹查询区
    var createShipWinContent = function () {
        return '<div class="locusBox">'
            + '<div id="shipWinDiv">'
            + '<div class="swtitle">以下数据来自于AIS：</div>'
            + '<div id="dataHtmlDiv"></div>'
            + '<div id="trackSearchDiv">'
            + '<div><label for="startTime">开始时间：</label><input type="text" id="startTime"/></div>'
            + '<div><label for="endTime">结束时间：</label><input type="text" id="endTime"/></div>'
            + '<div class="btnLink">'
            + '<a href="javascript:void(0)" id="btnTrackSearch">查 询</a>'
            + '<a href="javascript:void(0)" id="btnTrackCancel">取 消</a>'
            + '</div>'
            + '</div>'
            + '<div class="trackMsg"><span id="trackMsg"></span></div>'
            + '<div class="btnLink" id="btnLinkDiv">'
            + '<a href="javascript:void(0)" id="btnLocate">定 位</a>'
            + '<a href="javascript:void(0)" id="btnTrack">轨 迹</a>'
            + '<a href="javascript:void(0)" id="btnShipTrack">船舶跟踪</a>'
            + '</div>'
            + '</div>'
            + '</div>';
    };

    //注册船舶信息框所有按钮的点击事件
    var addShipWinBtnEvent = function () {
        var parent = util.getElement('shipWinDiv');
        //定位到地图中心点、指定级别
        util.addEvent(util.getElement('btnLocate', parent), 'click', function (event) {
            util.stopPropagation(event);
            //定位船舶
            service.locateShip(shipInfo.shipId);
            util.preventDefault(event);
        });
        //进入轨迹查询区
        util.addEvent(util.getElement('btnTrack', parent), 'click', function (event) {
            util.stopPropagation(event);
            toggleTrackSearchDiv(true);
            util.preventDefault(event);
        });
        //查询轨迹
        util.addEvent(util.getElement('btnTrackSearch', parent), 'click', function (event) {
            util.stopPropagation(event);
            var bgn = util.getDateByString(util.getElement("startTime", parent).value); //生成开始Date对象
            var end = util.getDateByString(util.getElement("endTime", parent).value); //生成结束Date对象
            bgn = bgn.getTime() / 1000;
            end = end.getTime() / 1000;
            if (bgn >= end) {
                view.setTrackMsg('开始时间必须小于等于结束时间相同');
                return;
            }
            service.searchTrack(shipInfo.shipId, bgn, end);
            util.preventDefault(event);
        });
        //取消查询轨迹，退出轨迹查询区
        util.addEvent(util.getElement('btnTrackCancel', parent), 'click', function (event) {
            util.stopPropagation(event);
            service.abortSearchTrack();
            toggleTrackSearchDiv(false);
            util.preventDefault(event);
        });

        //创建船舶跟踪任务 --拓展
        util.addEvent(util.getElement('btnShipTrack', parent), 'click', function (event) {
            util.stopPropagation(event);

            var ShipInfo = mini.encode(shipInfo);
            shipWin.hide();
            //var shipName = _DataMainTask.name;//船名
            //var mmsi = _DataMainTask.MMSI;
            //var latFormat = isNaN(_DataMainTask.lat) ? '' : util.formatLat(_DataMainTask.lat);//格式化纬度
            //var lngFormat = isNaN(_DataMainTask.lng) ? '' : util.formatLng(_DataMainTask.lng);

            var lat = _DataMainTask.lat;//纬度未格式化
            var lng = _DataMainTask.lng;
            var lineTask = '{ lat: ' + lat + ', lng: ' + lng + ' }';

            var taskSource = mini.get("#TaskSource").getValue();//任务来源 0: 主控中心 1:基站 2:客户
            var flyWay = 0;//任务类型 0:船舶跟踪 1:航线巡航

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
                            data: { Data: data, LineTask: lineTask, ShipInfo: ShipInfo },
                            type: "post",
                            success: function (text) {
                                var data = mini.decode(text);   //反序列化成对象
                                if (isLoginOut(data) == false) {
                                    if (data.IsOK) {
                                        //默认调用查询方法
                                        SystemSearch();
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
        });
    };

    //切换轨迹查询区
    var toggleTrackSearchDiv = function (isTrack) {
        var parent = util.getElement('shipWinDiv'), trackMsg = util.getElement("trackMsg", parent);
        trackMsg.innerHTML = '';
        if (isTrack) {//轨迹查询
            //初始化日期与时间选择器
            var nDate = new Date();
            var sDateTSpan = nDate.setDate(nDate.getDate() - 6); //6天前
            var startDate = new Date(sDateTSpan);
            util.getElement('startTime', parent).value = util.formatDateToString(startDate).substr(0, 10) + ' 00:00:00';
            util.getElement('endTime', parent).value = util.formatDateToString(new Date());
            util.getElement('btnLinkDiv', parent).style.display = 'none';
            util.getElement('trackSearchDiv', parent).style.display = '';
            trackMsg.style.display = '';
        } else {
            trackMsg.style.display = 'none';
            util.getElement('trackSearchDiv', parent).style.display = 'none';
            util.getElement('btnLinkDiv', parent).style.display = '';
        }
    };

    ////初始化筛选
    //var initFilter = function () {
    //    util.addEvent(util.getElement('filterBtn'), 'click', function (event) {
    //        util.stopPropagation(event);
    //        view.showFilterWin();
    //        util.preventDefault(event);
    //    });
    //    util.addEvent(util.getElement('unfilterBtn'), 'click', function (event) {
    //        util.stopPropagation(event);
    //        service.unfilter();
    //        util.preventDefault(event);
    //    });
    //    util.addEvent(util.getElement('refreshBtn'), 'click', function (event) {
    //        util.stopPropagation(event);
    //        service.refresh();
    //        util.preventDefault(event);
    //    });
    //};

    //构建筛选框内容：筛选条件、按钮
    var createFilterWinContent = function () {
        var country, type, cargo, status, k, d;
        country = type = cargo = status = '<option selected="selected" value="所有">所有</option>';
        for (k in country_zh) {
            country += '<option value="' + k + '">' + country_zh[k] + '</option>';
        }
        for (k = 0; k < shipTypeList.length; k++) {
            d = shipTypeList[k];
            type += '<option value="' + d + '">' + d + '</option>';
        }
        for (k = 0; k < cargoTypeList.length; k++) {
            d = cargoTypeList[k];
            cargo += '<option value="' + d + '">' + d + '</option>';
        }
        for (k = 0; k < navStatusList.length; k++) {
            d = navStatusList[k];
            status += '<option value="' + d + '">' + d + '</option>';
        }
        return '<div id="filterCondition">'
        + '<table>'
        + '<tr>'
        + '<td><label for="slcCountry">船籍</label></td><td><select id="slcCountry">' + country + '</select></td>'
        + '<td><label for="slcShipType">船舶类型</label></td><td><select id="slcShipType">' + type + '</select></td>'
        + '</tr>'
        + '<tr>'
        + '<td><label for="slcCargoType">货物类型</label></td><td><select id="slcCargoType">' + cargo + '</select></td>'
        + '<td><label for="slcNavStatus">航行状态</label></td><td><select id="slcNavStatus">' + status + '</select></td>'
        + '</tr>'
        + '<tr>'
        + '<td><label for="iptLength">船长</label></td><td><input type="text" id="iptMinLength" /> 至 <input type="text" id="iptMaxLength" />米</td>'
        + '<td><label for="iptBeam">船宽</label></td><td><input type="text" id="iptMinBeam" /> 至 <input type="text" id="iptMaxBeam" />米</td>'
        + '</tr>'
        + '<tr>'
        + '<td><label for="iptDraught">吃水</label></td><td><input type="text" id="iptMinDraught" /> 至 <input type="text" id="iptMaxDraught" />米</td>'
        + '</tr>'
        + '</table>'//end table
        + '<div class="btnLink">'
        + '<a href="javascript:void(0)" id="btnFilter">筛 选</a>'
        + '</div>'//end btnLink
        + '</div>'//end filterCondition
    };

    //注册筛选按钮的点击事件
    var addFilterEvent = function () {
        var parent = util.getElement('filterCondition');
        util.addEvent(util.getElement('btnFilter', parent), 'click', function (event) {
            util.stopPropagation(event);
            //筛选船舶
            service.filter({
                country: util.getElement('slcCountry', parent).value,
                type: util.getElement('slcShipType', parent).value,
                cargoType: util.getElement('slcCargoType', parent).value,
                status: util.getElement('slcNavStatus', parent).value,
                length: [parseFloat(util.getElement('iptMinLength', parent).value) || 0, parseFloat(util.getElement('iptMaxLength', parent).value) || 0],
                beam: [parseFloat(util.getElement('iptMinBeam', parent).value) || 0, parseFloat(util.getElement('iptMaxBeam', parent).value) || 0],
                draught: [parseFloat(util.getElement('iptMinDraught', parent).value) || 0, parseFloat(util.getElement('iptMaxDraught', parent).value) || 0]
            });
            util.preventDefault(event);
        });
    };

    myApp.view = {
        //页面初始化
        init: function () {
            map = myApp.map;
            util = myApp.util;
            view = this;
            service = myApp.service;
            initLayout();
           // initList();
           // initSearch();
            //initFilter();
            util.addEvent(window, 'resize', initLayout);
        },

        //显示指定列表，同时会关闭其他列表，listname:'regionlist'、'searchlist'，若传入data，则用data刷新列表
        //isOpen：指示是否打开该列表，true打开，false不打开，维持现状
        showList: function (listname, data, isOpen) {
            if (!listname) { return; }
            var box = util.getElement('listbox'), list = util.getElement(listname, box);
            if (!box || !list) { return; }
            var ul = list.getElementsByTagName('UL')[0];
            createList(ul, listname, data);
            if (isOpen && list.getAttribute('data-toggle') != 'on') {
                toggleList(box, list);
            }
        },

        //关闭指定列表，listname:'regionlist'、'searchlist'
        hideList: function (listname) {
            if (!listname) { return; }
            var box = util.getElement('listbox'), list = util.getElement(listname, box);
            if (!box || !list) { return; }
            if (list.getAttribute('data-toggle') != 'off') {
                toggleList(box, list);
            }
        },

        //检测指定列表是否已打开
        isOpenedList: function (listname) {
            if (!listname) { return; }
            var list = util.getElement(listname);
            if (!list) { return; }
            if (list.getAttribute('data-toggle') == 'on') {
                return true;
            }
            return false;
        },

        //显示船舶信息框
        showShipWin: function (data) {
            if (!data) { return; }
            service.abortSearchTrack(); //取消轨迹查询，若有正在查询的轨迹
            if (!shipWin) {
                shipWin = new myApp.XWin({
                    modal: false,
                    dragbody: true,
                    width: shipWinWidth - 20,
                    position: [document.body.clientWidth - shipWinWidth - 10, 100], //[left,top]
                    title: '船舶信息',
                    content: createShipWinContent(), //信息框内容
                    onhide: function () {
                        service.abortSearchTrack(); //取消轨迹查询，若有正在查询的轨迹
                        service.unselectShip(shipInfo.shipId); //关闭船舶信息框之后，反选船舶
                        shipInfo = null;
                    }
                });
                addShipWinBtnEvent(); //注册所有按钮的点击事件
            }
            util.getElement('dataHtmlDiv').innerHTML = createShipWinDataHtml(data); //刷新数据区内容
            if (!shipInfo || shipInfo.shipId != data.shipId)//单单数据更新
                toggleTrackSearchDiv(false); //信息框打开初始化状态：轨迹查询区不显示，当点击轨迹按钮之后进入轨迹查询区
            shipWin.show(); //显示
            shipInfo = data; //缓存船舶信息
        },

        //关闭船舶信息框
        hideShipWin: function () {
            if (shipWin && !shipWin.ishide()) {
                shipWin.hide();
            }
        },

        //设置轨迹查询提示消息
        setTrackMsg: function (msg) {
            var trackMsg = util.getElement("trackMsg");
            trackMsg.innerHTML = msg;
            if (msg) {
                trackMsg.style.display = '';
            } else {//当是空消息，隐藏
                trackMsg.style.display = 'none';
            }
        },

        //显示船舶简单信息提示
        showShipTip: function (data, latLng) {
            var pos = map.fromLatLngToPoint(latLng ? latLng : new shipxyMap.LatLng(latLng.lat, data.lng));
            if (!shipTip) {
                shipTip = document.createElement('div');
                shipTip.className = 'shipTip';
                util.getElement('mainer').appendChild(shipTip);
            }
            var html = '';
            if (data.name) html += '船名：' + data.name + '<br/>';
            if (data.callsign) html += '呼号：' + data.callsign + '<br/>';
            if (data.MMSI) html += 'MMIS：' + data.MMSI + '<br/>';
            if (data.IMO) html += 'IMO：' + data.IMO + '<br/>';
            if (data.lastTime) html += '最后时间：' + util.formatTime(data.lastTime);
            shipTip.innerHTML = html;
            shipTip.style.display = 'block';
            shipTip.style.left = pos.x + 10 + 'px';
            shipTip.style.top = pos.y + 20 + 'px';
        },

        //隐藏船舶简单信息提示
        hideShipTip: function () {
            if (shipTip) {
                shipTip.style.display = 'none';
            }
        },

        //显示筛选框
        showFilterWin: function () {
            if (!filterWin) {
                filterWin = new myApp.XWin({
                    modal: false,
                    dragbody: true,
                    width: filterWinWidth - 20,
                    position: [320, 80], //[left,top]
                    title: '筛选船舶',
                    content: createFilterWinContent() //筛选框内容
                });
                addFilterEvent(); //注册筛选按钮的点击事件
            }
            filterWin.show(); //显示
        },

        //关闭筛选框
        hideFilterWin: function () {
            if (filterWin && !filterWin.ishide()) {
                filterWin.hide();
            }
        },

        //显示轨迹点信息提示
        //shipData:所属船舶的简单数据（船名：呼号、MMSI、IMO）
        //trackData:该轨迹点的数据（纬度、经度、速度、船向、时间）
        showTrackTip: function (shipData, trackData) {
            var pos = map.fromLatLngToPoint(new shipxyMap.LatLng(trackData.lat, trackData.lng));
            if (!trackTip) {
                trackTip = document.createElement('div');
                trackTip.className = 'trackTip';
                util.getElement('mainer').appendChild(trackTip);
            }
            var html = '';
            if (shipData.name) html += shipData.name + '<br/>';
            if (shipData.callsign) html += shipData.callsign + '<br/>';
            if (shipData.MMSI) html += shipData.MMSI + '<br/>';
            if (shipData.IMO) html += shipData.IMO + '<br/>';
            if (trackData.speed) html += trackData.speed.toFixed(1) + '节<br/>';
            if (trackData.course) html += trackData.course + '度<br/>';
            if (trackData.lastTime) html += util.formatTime(trackData.lastTime);
            trackTip.innerHTML = html;
            trackTip.style.display = 'block';
            trackTip.style.left = pos.x + 10 + 'px';
            trackTip.style.top = pos.y + 20 + 'px';
        },

        //隐藏轨迹点信息提示
        hideTrackTip: function () {
            if (trackTip) trackTip.style.display = 'none';
        }
    };
})();