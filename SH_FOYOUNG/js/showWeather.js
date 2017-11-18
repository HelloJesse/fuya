/**
* @新闻天气数据显示
* @author annan#tencent.com
* @time 2012-12-04
* @version v1.0.0
*/
//城与城ID对应关系
Site = {};
Site.Weather = {
    defaultCity: "01010101",
    city: {
        "北京": {
            "_": "01010101",
            "北京": "01010101"
        },
        "上海": {
            "_": "01012601",
            "上海": "01012601"
        },
        "天津": {
            "_": "01012901",
            "天津": "01012901"
        },
        "重庆": {
            "_": "01010401",
            "重庆": "01010401"
        },
        "香港": {
            "_": "01013101",
            "香港": "01013101"
        },
        "澳门": {
            "_": "01010301",
            "澳门": "01010301"
        },
        "台湾": {
            "_": "01012801",
            "台北": "01012801",
            "高雄": "01012802",
            "台中": "01012803",
            "花莲": "01012801",
            "基隆": "01012801",
            "嘉义": "01012801",
            "金门": "01012801",
            "连江": "01012801",
            "苗栗": "01012801",
            "南投": "01012801",
            "澎湖": "01012801",
            "屏东": "01012801",
            "台东": "01012801",
            "台南": "01012801",
            "桃园": "01012801",
            "新竹": "01012801",
            "宜兰": "01012801",
            "云林": "01012801",
            "彰化": "01012801"
        },
        "安徽": {
            "_": "01010208",
            "安庆": "01010201",
            "蚌埠": "01010202",
            "亳州": "01010203",
            "巢湖": "01010204",
            "池州": "01010205",
            "滁州": "01010206",
            "阜阳": "01010207",
            "合肥": "01010208",
            "淮北": "01010209",
            "淮南": "01010210",
            "黄山": "01010211",
            "六安": "01010212",
            "马鞍山": "01010213",
            "铜陵": "01010214",
            "芜湖": "01010215",
            "宿州": "01010216",
            "宣城": "01010217"
        },
        "福建": {
            "_": "01010501",
            "福州": "01010501",
            "龙岩": "01010502",
            "南平": "01010503",
            "宁德": "01010504",
            "莆田": "01010505",
            "泉州": "01010506",
            "三明": "01010507",
            "厦门": "01010508",
            "漳州": "01010509"
        },
        "甘肃": {
            "_": "01010607",
            "白银": "01010601",
            "定西": "01010602",
            "甘南州": "01010603",
            "嘉峪关": "01010604",
            "金昌": "01010605",
            "酒泉": "01010606",
            "兰州": "01010607",
            "临夏": "01010608",
            "陇南": "01010609",
            "平凉": "01010610",
            "庆阳": "01010611",
            "天水": "01010612",
            "武威": "01010613",
            "张掖": "01010614"
        },
        "广东": {
            "_": "01010704",
            "潮州": "01010701",
            "东莞": "01010702",
            "佛山": "01010703",
            "广州": "01010704",
            "河源": "01010705",
            "惠州": "01010706",
            "江门": "01010707",
            "揭阳": "01010708",
            "茂名": "01010709",
            "梅州": "01010710",
            "清远": "01010711",
            "汕头": "01010712",
            "汕尾": "01010713",
            "韶关": "01010714",
            "深圳": "01010715",
            "阳江": "01010716",
            "云浮": "01010717",
            "湛江": "01010718",
            "肇庆": "01010719",
            "中山": "01010720",
            "珠海": "01010721"
        },
        "广西": {
            "_": "01010811",
            "百色": "01010801",
            "北海": "01010802",
            "崇左": "01010803",
            "防城港": "01010804",
            "贵港": "01010805",
            "桂林": "01010806",
            "河池": "01010807",
            "贺州": "01010808",
            "来宾": "01010809",
            "柳州": "01010810",
            "南宁": "01010811",
            "钦州": "01010812",
            "梧州": "01010813",
            "玉林": "01010814"
        },
        "贵州": {
            "_": "01010903",
            "安顺": "01010901",
            "毕节地区": "01010902",
            "贵阳": "01010903",
            "六盘水": "01010904",
            "黔东南州": "01010905",
            "黔南州": "01010906",
            "黔西南州": "01010907",
            "铜仁地区": "01010908",
            "遵义": "01010909"
        },
        "海南": {
            "_": "01011008",
            "澄迈": "01011004",
            "儋州": "01011005",
            "东方": "01011007",
            "海口": "01011008",
            "琼海": "01011012",
            "三亚": "01011014",
            "屯昌县": "01011015",
            "万宁": "01011016",
            "文昌": "01011017",
            "五指山": "01011018"
        },
        "河北": {
            "_": "01011108",
            "保定": "01011101",
            "沧州": "01011102",
            "承德": "01011103",
            "邯郸": "01011104",
            "衡水": "01011105",
            "廊坊": "01011106",
            "秦皇岛": "01011107",
            "石家庄": "01011108",
            "唐山": "01011109",
            "邢台": "01011110",
            "张家口": "01011111"
        },
        "河南": {
            "_": "01011216",
            "安阳": "01011201",
            "鹤壁": "01011202",
            "焦作": "01011203",
            "开封": "01011204",
            "洛阳": "01011206",
            "漯河": "01011205",
            "南阳": "01011207",
            "平顶山": "01011208",
            "濮阳": "01011209",
            "三门峡": "01011210",
            "商丘": "01011211",
            "济源": "0101121201",
            "新乡": "01011213",
            "信阳": "01011214",
            "许昌": "01011215",
            "郑州": "01011216",
            "周口": "01011217",
            "驻马店": "01011218"
        },
        "黑龙江": {
            "_": "01011303",
            "大庆": "01011301",
            "大兴安岭地区": "01011302",
            "哈尔滨": "01011303",
            "鹤岗": "01011304",
            "黑河": "01011305",
            "鸡西": "01011306",
            "佳木斯": "01011307",
            "牡丹江": "01011308",
            "齐齐哈尔": "01011309",
            "七台河": "01011310",
            "双鸭山": "01011311",
            "绥化": "01011312",
            "伊春": "01011313"
        },
        "湖北": {
            "_": "01011410",
            "鄂州": "01011401",
            "恩施州": "01011402",
            "黄冈": "01011403",
            "黄石": "01011404",
            "荆门": "01011405",
            "荆州": "01011406",
            "潜江": "0101140701",
            "神农架林区": "0101140702",
            "十堰": "01011408",
            "随州": "01011409",
            "武汉": "01011410",
            "天门": "0101140703",
            "仙桃": "0101140704",
            "咸宁": "01011411",
            "襄樊": "01011412",
            "孝感": "01011413",
            "宜昌": "01011414"
        },
        "湖南": {
            "_": "01011502",
            "常德": "01011501",
            "长沙": "01011502",
            "郴州": "01011503",
            "衡阳": "01011504",
            "怀化": "01011505",
            "娄底": "01011506",
            "邵阳": "01011507",
            "湘潭": "01011508",
            "湘西州": "01011509",
            "益阳": "01011510",
            "永州": "01011511",
            "岳阳": "01011512",
            "张家界": "01011513",
            "株洲": "01011514"
        },
        "吉林": {
            "_": "01011603",
            "白城": "01011601",
            "白山": "01011602",
            "长春": "01011603",
            "吉林": "01011604",
            "辽源": "01011605",
            "四平": "01011606",
            "松原": "01011607",
            "通化": "01011608",
            "延边州": "01011609"
        },
        "江苏": {
            "_": "01011704",
            "常州": "01011701",
            "淮安": "01011702",
            "连云港": "01011703",
            "南京": "01011704",
            "南通": "01011705",
            "苏州": "01011706",
            "宿迁": "01011707",
            "泰州": "01011708",
            "无锡": "01011709",
            "徐州": "01011710",
            "盐城": "01011711",
            "扬州": "01011712",
            "镇江": "01011713"
        },
        "江西": {
            "_": "01011806",
            "抚州": "01011801",
            "吉安": "01011803",
            "赣州": "01011802",
            "景德镇": "01011804",
            "九江": "01011805",
            "南昌": "01011806",
            "萍乡": "01011807",
            "上饶": "01011808",
            "新余": "01011809",
            "宜春": "01011810",
            "鹰潭": "01011811"
        },
        "辽宁": {
            "_": "01011912",
            "鞍山": "01011901",
            "本溪": "01011902",
            "朝阳": "01011903",
            "大连": "01011904",
            "丹东": "01011905",
            "抚顺": "01011906",
            "阜新": "01011907",
            "葫芦岛": "01011908",
            "锦州": "01011909",
            "辽阳": "01011910",
            "盘锦": "01011911",
            "沈阳": "01011912",
            "铁岭": "01011913",
            "营口": "01011914"
        },
        "内蒙古": {
            "_": "01012004",
            "包头": "01012001",
            "赤峰": "01012002",
            "鄂尔多斯": "01012003",
            "呼和浩特": "01012004",
            "呼伦贝尔": "01012005",
            "通辽": "01012006",
            "乌海": "01012007",
            "阿拉善盟": "01012008",
            "锡林郭勒盟": "01012009",
            "兴安盟": "01012010",
            "巴彦淖尔": "01012011",
            "乌兰察布": "01012012"
        },
        "宁夏": {
            "_": "01012104",
            "固原": "01012101",
            "石嘴山": "01012102",
            "吴忠": "01012103",
            "银川": "01012104",
            "中卫": "01012105"
        },
        "青海": {
            "_": "01012207",
            "果洛州": "01012201",
            "海东地区": "01012202",
            "海西州": "01012203",
            "海北州": "01012204",
            "海南州": "01012205",
            "黄南州": "01012206",
            "西宁": "01012207",
            "玉树州": "01012208"
        },
        "山东": {
            "_": "01012305",
            "济南": "01012305",
            "滨州": "01012301",
            "德州": "01012302",
            "东营": "01012303",
            "菏泽": "01012304",
            "济宁": "01012306",
            "莱芜": "01012307",
            "聊城": "01012308",
            "临沂": "01012309",
            "青岛": "01012310",
            "日照": "01012311",
            "泰安": "01012312",
            "威海": "01012313",
            "潍坊": "01012314",
            "烟台": "01012315",
            "枣庄": "01012316",
            "淄博": "01012317"
        },
        "山西": {
            "_": "01012408",
            "长治": "01012401",
            "大同": "01012402",
            "晋城": "01012403",
            "晋中": "01012404",
            "临汾": "01012405",
            "吕梁": "01012406",
            "朔州": "01012407",
            "太原": "01012408",
            "忻州": "01012409",
            "阳泉": "01012410",
            "运城": "01012411"
        },
        "陕西": {
            "_": "01012507",
            "安康": "01012501",
            "宝鸡": "01012502",
            "汉中": "01012503",
            "商洛": "01012504",
            "铜川": "01012505",
            "渭南": "01012506",
            "西安": "01012507",
            "咸阳": "01012508",
            "延安": "01012509",
            "榆林": "01012510"
        },
        "四川": {
            "_": "01012703",
            "阿坝州": "01012701",
            "巴中": "01012702",
            "成都": "01012703",
            "达州": "01012704",
            "德阳": "01012705",
            "甘孜州": "01012706",
            "广安": "01012707",
            "广元": "01012708",
            "乐山": "01012709",
            "凉山州": "01012710",
            "泸州": "01012711",
            "眉山": "01012712",
            "绵阳": "01012713",
            "内江": "01012715",
            "南充": "01012714",
            "攀枝花": "01012716",
            "遂宁": "01012717",
            "雅安": "01012718",
            "宜宾": "01012719",
            "资阳": "01012720",
            "自贡": "01012721"
        },
        "西藏": {
            "_": "01013003",
            "阿里地区": "01013001",
            "昌都地区": "01013002",
            "拉萨": "01013003",
            "林芝地区": "01013004",
            "那曲地区": "01013005",
            "日喀则地区": "01013006",
            "山南地区": "01013007"
        },
        "新疆": {
            "_": "01013213",
            "阿克苏地区": "01013201",
            "阿勒泰地区": "01013202",
            "巴音郭楞州": "01013203",
            "博尔塔拉州": "01013204",
            "昌吉州": "01013205",
            "哈密地区": "01013206",
            "和田地区": "01013207",
            "喀什地区": "01013208",
            "克拉玛依": "01013209",
            "克孜勒苏柯州": "01013210",
            "塔城地区": "01013211",
            "吐鲁番地区": "01013212",
            "乌鲁木齐": "01013213",
            "伊犁州": "01013214",
            "石河子": "0101321501",
            "阿拉尔": "0101321502"
        },
        "云南": {
            "_": "01013307",
            "保山": "01013301",
            "楚雄州": "01013302",
            "大理州": "01013303",
            "德宏州": "01013304",
            "迪庆州": "01013305",
            "红河州": "01013306",
            "昆明": "01013307",
            "丽江": "01013308",
            "临沧": "01013309",
            "怒江州": "01013310",
            "曲靖": "01013312",
            "思茅": "01013307",
            "文山州": "01013313",
            "西双版纳州": "01013314",
            "玉溪": "01013315",
            "昭通": "01013316"
        },
        "浙江": {
            "_": "01013401",
            "杭州": "01013401",
            "湖州": "01013402",
            "嘉兴": "01013403",
            "金华": "01013404",
            "丽水": "01013405",
            "宁波": "01013406",
            "衢州": "01013407",
            "绍兴": "01013408",
            "台州": "01013409",
            "温州": "01013410",
            "舟山": "01013411"
        }
    }
}


Site.weatherTxt = ['晴', '多云', '阴', '阵雨', '雷阵雨', '雷阵雨并伴有冰雹', '雨夹雪', '小雨', '中雨', '大雨', '暴雨', '大暴雪', '特大暴雪', '阵雪', '小雪', '中雪', '大雪', '暴雪', '雾', '冻雨', '沙尘暴', '小雨-中雨', '中雨-大雨', '大雨-暴雨', '暴雨-大暴雨', '大暴雨-特大暴雨', '小雪-中雪', '中雪-大雪', '大雪-暴雪', '浮尘', '扬沙', '强沙尘暴', '飑', '龙卷风', '弱高吹雪', '轻雾']

Site.weatherimgIco = { 'defaultUrl': 'http://mat1.gtimg.com/weather/weatherIco/imgIco/', 'ico': [{ 'img': '0.png' }, { 'img': '1.png' }, { 'img': '2.png' }, { 'img': '3.png' }, { 'img': '4.png' }, { 'img': '5.png' }, { 'img': '6.png' }, { 'img': '7.png' }, { 'img': '8.png' }, { 'img': '9.png' }, { 'img': '10.png' }, { 'img': '10.png' }, { 'img': '10.png' }, { 'img': '13.png' }, { 'img': '14.png' }, { 'img': '16.png' }, { 'img': '16.png' }, { 'img': '17.png' }, { 'img': '18.png' }, { 'img': '19.png' }, { 'img': '20.png' }, { 'img': '8.png' }, { 'img': '9.png' }, { 'img': '10.png' }, { 'img': '10.png' }, { 'img': '10.png' }, { 'img': '16.png' }, { 'img': '16.png' }, { 'img': '17.png' }, { 'img': '29.png' }, { 'img': '30.png' }, { 'img': '20.png' }, { 'img': '32.png' }, { 'img': '33.png' }, { 'img': '14.png' }, { 'img': '18.png'}] };
Site.weatherIcon = {
    "00": { //晴
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/sun.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/sunnight.png"
    },
    "01": { //多云
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/cloudy.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/cloudynight.png"
    },
    "02": { //阴
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/overcast.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/cloudynight.png"
    },
    "03": { //阵雨
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/rain.sun.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/rain.sun.png"
    },
    "04": { //雷阵雨
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/rainstorm.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/rainstorm.png"
    },
    "05": { //雷阵雨并伴有冰雹
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/rainstorm.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/rainstorm.png"
    },
    "06": { //雨夹雪
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/sleet.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/sleet.png"
    },
    "07": { //小雨
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/drizzle.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/drizzle.png"
    },
    "08": { //中雨
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/rainy2.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/rainy2.png"
    },
    "09": { //大雨
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/rainy1.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/rainy1.png"
    },
    "10": { //暴雨
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/showers.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/showers.png"
    },
    "11": { //大暴雪
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/snowstorm.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/snowstorm.png"
    },
    "12": { //特大暴雪
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/snowstorm.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/snowstorm.png"
    },
    "13": { //阵雪
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/snow1.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/snow1.png"
    },
    "14": { //小雪
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/snow1.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/snow1.png"
    },
    "15": { //中雪
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/snow2.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/snow2.png"
    },
    "16": { //大雪
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/snowstorm.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/snowstorm.png"
    },
    "17": { //暴雪
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/snowstorm.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/snowstorm.png"
    },
    "18": { //雾
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/mist.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/mist.png"
    },
    "19": { //冻雨
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/sleet.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/sleet.png"
    },
    "20": { //沙尘暴
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/sand.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/sand.png"
    },
    "21": { //小雨-中雨
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/drizzle.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/drizzle.png"
    },
    "22": { //中雨-大雨
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/rainy2.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/rainy2.png"
    },
    "23": { //大雨-暴雨
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/rainy1.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/rainy1.png"
    },
    "24": { //暴雨-大暴雨
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/showers.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/showers.png"
    },
    "25": { //大暴雨-特大暴雨
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/showers.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/showers.png"
    },
    "26": { //小雪-中雪
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/snow1.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/snow1.png"
    },
    "27": { //中雪-大雪
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/snow2.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/snow2.png"
    },
    "28": { //大雪-暴雪
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/snowstorm.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/snowstorm.png"
    },
    "29": { //浮尘
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/sand.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/sand.png"
    },
    "30": { //扬沙
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/sand.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/sand.png"
    },
    "31": { //强沙尘暴
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/sand.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/sand.png"
    },
    "32": { //飑
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/rainy2.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/rainy2.png"
    },
    "33": { //龙卷风
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/sand.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/sand.png"
    },
    "34": { //弱高吹雪
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/snow2.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/snow2.png"
    },
    "35": { //轻雾
        "day": "http://mat1.gtimg.com/news/newsweather/wIco/mist.png",
        "night": "http://mat1.gtimg.com/news/newsweather/wIco/mist.png"
    }
}

function weatherInfo() { }

weatherInfo.prototype = {
    //获取准确城ID
    getCityID: function () {
        var proviceName = ""; //IPData[2];
        var cityName = ""; //IPData[3];
        //console.log(proviceName,cityName);
        var ciytId = null;
        if ((proviceName != '')) {
            if (cityName == '' || cityName == '未知') {
                ciytId = Site.Weather.city[proviceName]['_'];
            } else {
                ciytId = Site.Weather.city[proviceName][cityName];
            }
        } else {
            ciytId = Site.Weather.defaultCity;
        }
        this.setcookie
        return ciytId;
    },
    //加载js
    loadJs: function (url, charsetMode, jsName, callback) {
        var script = document.createElement('script');
        script.charset = charsetMode;
        script.id = jsName;
        script.src = url;
        script.type = 'text/javascript';
        var head = document.getElementsByTagName('head')[0];
        head.appendChild(script);
        if (script.attachEvent) {
            script.attachEvent('onreadystatechange', function () {
                if (script.readyState == 4 || script.readyState == 'complete' || script.readyState == 'loaded') {
                    callback();
                }
            });
        } else if (script.addEventListener) {
            script.addEventListener('load', callback, false)
        }
    },
    //删除js
    removeJs: function (jsName) {
        var script = document.getElementById(jsName);
        var head = document.getElementsByTagName('head')[0];
        head.removeChild(script);
    },
    //获取天气信息，例如气温等
    getWeatherInfo: function () {
        // var cityId = this.getCityID();
        var cityId = Site.Weather.city[remote_ip_info["province"]][remote_ip_info["city"]];
        //console.log(cityId);
        var _url = 'http://weather.gtimg.cn/city/' + cityId + '.js?ref=qqnews';
        var that = this;

        this.loadJs(_url, 'gb2312', 'weatherJs', function () {
            var wInfo = __weather_city;
            //console.log(wInfo);
            if (document.getElementById('wCity') != null) {
                document.getElementById('wCity').innerHTML = wInfo.bi_name;
            }

            if (document.getElementById('wTp') != null) {
                document.getElementById('wTp').innerHTML = wInfo.sk_tp + '℃';
            }
            //处理当前日期
            var dates = new Date();
            var sTime1 =dates.getFullYear()+'年'+(dates.getMonth()+1)+"月"+ dates.getDate()+"日";
            var sTime2 = dates.getDay();
            var sMday = {
                1: "一",
                2: "二",
                3: "三",
                4: "四",
                5: "五",
                6: "六",
                0: "日"
            }

            document.getElementById("echoWeek").innerHTML = "星期" + sMday[sTime2];
            document.getElementById("echoData").innerHTML = sTime1;
            //document.getElementById("logincom").innerHTML = "登录公司：" + getCookie_My("CompanyName") ;
            document.getElementById("logincom").innerHTML = "登录单位：" + getCookie_My("CustomerName");//所属公司
            document.getElementById("loginUser").innerHTML = "登录人：" + getCookie_My("UserCode") + "[" + getCookie_My("UserName") + "]";
            var wTime = {};
            /*wTime.currentHours = '13';*/
            wTime.currentHours = parseInt(dates.getDate(), 10) + 8;


            var imgIco = "";
            if (Site.weatherIcon[wInfo.sk_wt]) {
                imgIco = Site.weatherIcon[wInfo.sk_wt].day;
                if ((wTime.currentHours < 4) || (wTime.currentHours >= 20)) {
                    imgIco = Site.weatherIcon[wInfo.sk_wt].night;
                }
            }
            else {
                imgIco = Site.weatherIcon["00"].day;
                if ((wTime.currentHours < 4) || (wTime.currentHours >= 20)) {
                    imgIco = Site.weatherIcon["00"].night;
                }
            }

            var ie6 = ! -[1, ] && !window.XMLHttpRequest;

            if (document.getElementById('weatherIco') != null) {
                if (ie6) {
                    document.getElementById('weatherIco').style.filter = 'progid:DXImageTransform.Microsoft.AlphaImageLoader(src="' + imgIco + '" ,sizingMethod="noscale")';
                } else {
                    document.getElementById('weatherIco').innerHTML = '<img src="' + imgIco + '" /> ';
                }
            }
            that.removeJs('weatherJs');
        });
    }
}
try{
    weatherInfo.prototype.getWeatherInfo(); /*  |xGv00|1317ee8c770dc95730e73aa357e389ee */
}
catch(ex){

};
