<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Main.aspx.cs" Inherits="Main" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
<meta http-equiv="pragma" content="no-cache" />
<meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=0">
<script src="js/boot.js" type="text/javascript"></script>
<script src="js/core.js" type="text/javascript"></script>
<script src="http://int.dpool.sina.com.cn/iplookup/iplookup.php?format=js" type="text/ecmascript"></script>
<link rel="shortcut icon" type="image/x-icon" href="/favicon.ico" />
<title>复亚航空</title>
    <style type="text/css">
        body {
            font-family: Tahoma, Verdana, 宋体;
            font-size: 12px;
            line-height: 22px;
            padding: 0;
            border: 0;
            width: 100%;
            height: 100%;
            overflow: hidden;
        }
        .logo {
            font-family: "微软雅黑", "Helvetica Neue",​Helvetica,​Arial,​sans-serif;
            font-size: 24px;
            font-weight: bold;
            color: #336699;
            cursor: default;
            position: absolute;
            top: 5px;
            left: 20px;
            line-height: 28px;
        }
        .topNav {
            position: absolute;
            right: 8px;
            top: 10px;
            font-size: 12px;
            line-height: 25px;
        }

            .topNav a {
                text-decoration: none;
                color: #222;
                font-weight: normal;
                font-size: 12px;
                line-height: 25px;
                margin-left: 3px;
                margin-right: 3px;
            }

                .topNav a:hover {
                    text-decoration: underline;
                    color: Blue;
                }

        /*天气时间*/
        .weatherItem {
            font: 12px/20px "宋体","Arial Narrow",HELVETICA;
            color: #4c4c4c;
            position: relative;
            z-index: 102;
        }

        #weatherWrap {
            height: 20px;
            overflow: hidden;
        }

        #echoData {
            padding: 0 20px 0 20px;
            background: url(http://mat1.gtimg.com/news/news2013/img/icons.png) no-repeat -32px 3px;
        }

        #echoWeek {
            padding-right: 18px;
        }

        #echoWea {
            color: #4c4c4c;
        }

        #weatherItem span {
        }

        #logincom {
            padding-left: 200px;
        }

        #loginUser {
            padding-right: 18px;
            padding-left: 18px;
        }

        #weatherIco {
            margin: 0px 5px 0px;
        }

            #weatherIco img {
                width: 16px;
                height: 14px;
            } 
    </style>
</head>
<body>
    <div id="layout1" class="mini-layout" style="width:100%;height:100%;" >
        <!--<div class="header" region="north" height="70" showsplit="false" showheader="false">
            <h1 style="margin:0;padding:15px;cursor:default;font-family:'Trebuchet MS',Arial,sans-serif;">折叠菜单（左侧）</h1>
        </div>-->
        <div title="north" region="north" class="app-header" splitSize="0" style="border-bottom:0"  height="40" showheader="false" showsplit="false">
            <div class="logo">复亚飞控系统</div>
           
            <div class="topNav">
                <!--<label id="UserName">登录</label> |-->
                <!--<a href="http://www.foyoung.com.cn/" target="_blank">官方网站</a> |-->
                <a class="mini-button" onclick="UpdatePwdEdit()"  plain="true">更改密码</a> |
                <a onclick="layout()" target="_self"><span style="color:Red;font-family:Tahoma">安全退出</span></a> |
                <!--<a href="wms/base/TableStructure.html" >表结构</a>-->
                <a class="help" onclick="TabAddHelp()">帮助</a> |
                <!--<a href="Vwms.url">快捷方式</a> |-->
            <!--</div>
            <div style="position: absolute; right: 12px; bottom: 5px; font-size: 12px; line-height: 25px; font-weight: normal;">-->
                <!--<span style="color:Red;font-family:Tahoma"></span>选择风格：-->
                <select id="selectSkin" onchange="onSkinChange(this.value)" style="width:100px;display:none">
                    <!--<option value="">Default</option>-->
                    <option value="metro">metro</option>
                    <option value="blue2010">Blue2010</option>
                    <option value="blue">Blue</option>
                    <option value="gray">Gray</option>
                    <option value="olive2003">Olive2003</option>
                    <option value="blue2003">Blue2003</option>
                    <option value="bootstrap">Bootstrap</option>

                    <option value="metro-green">metro-green</option>
                    <option value="metro-orange">metro-orange</option>

                    <!--<option value="jqueryui-uilightness">jqueryui-uilightness</option>
                    <option value="jqueryui-humanity">jqueryui-humanity</option>
                    <option value="jqueryui-excitebike">jqueryui-excitebike</option>
                    <option value="jqueryui-cupertino">jqueryui-cupertino</option>-->
                </select> 
                
                <span style="color:Red;font-family:Tahoma"></span>菜单显示方式：
                <select id="selectTab" style="width:80px;">
                    <!--<option value="">Default</option>-->
                    <option value="tab">新TAB页</option>
                    <option value="ka">新选项卡</option>
                </select> 
            </div>
        </div>


            <div title="south" region="south" showsplit="false" showheader="false" height="20">
                <div style="line-height:20px;text-align:right;cursor:default;width:45%;float:left"></div>
                <div id="weatherItem" style="float:right;width:55%">
                    <div id="weatherWrap">
                        <span id="logincom"></span>
                        <span id="loginUser"></span>
                        <span id="echoData"></span>
                        <span id="echoWeek"></span>
                        <a href="http://weather.news.qq.com/" target="_blank" style="text-decoration:none" bosszone="weather">
                            <span id="wCity"></span>
                            <span id="weatherIco"></span>
                            <span id="wTp"></span>
                        </a>
                    </div>
                </div>
            </div>
            
        <div title="center" region="center" style="border:0;" bodystyle="overflow:hidden;">
            <!--Splitter-->
            <div class="mini-splitter" style="width:100%;height:100%;" borderstyle="border:0;">
                <div size="190" maxsize="250" minsize="100" showcollapsebutton="true" style="border:0">
                    <!--OutlookTree onnodeclick="onNodeSelect-->
                    <div id="leftTree" class="mini-outlooktree" url="../data/Main/MenuManager.aspx?method=GetMenuList" onnodeclick="onNodeSelect"
                         idfield="id" parentfield="pid" textfield="text" borderstyle="border:0">
                    </div>

                </div>
                <div showcollapsebutton="false" style="border:0;">
                    <!--Tabs-->
                    <div id="mainTabs" class="mini-tabs" activeindex="0" style="width:100%;height:100%;" bodyStyle="padding:0;border-top:0" cleartimestamp="true"
                         contextmenu="#tabsMenu" plain="false" onactivechanged="onTabsActiveChanged" arrowposition="side" shownavmenu="true">
                        <div id="first" name="shouyetab" title="首页" url="../Demo.html"></div>
                    </div>
                </div>
            </div>
        </div>


          <!--  <div showheader="false" region="west" width="180" maxwidth="250" minwidth="100">-->

                <!--<div id="leftTree" class="mini-outlooktree" url="../data/Main/MenuManager.aspx?method=GetMenuList" onnodeclick="onNodeSelect"
                     idfield="id" parentfield="pid" textfield="text" borderstyle="border:0">
                </div>
            </div>-->
        <!--<div showcollapsebutton="false" style="border:0;">-->
            <!--Tabs
            <div id="mainTabs" class="mini-tabs" activeindex="0" style="width:100%;height:100%;" clearTimeStamp = "true"
                 contextmenu="#tabsMenu" plain="false" onactivechanged="onTabsActiveChanged" arrowposition="side" shownavmenu="true">
                <div id="first" title="首页" url="../Demo.html">
                </div>
            </div>
            </div>-->
   </div>
    <ul id="tabsMenu" class="mini-contextmenu" onbeforeopen="onBeforeOpen"  >
        <li onclick="closeTab">关闭当前</li>
        <li class="separator"></li>
        <li onclick="closeAll">关闭所有</li>
        <li class="separator"></li>
        <li onclick="closeAllBut">除此之外全部关闭</li>
    </ul>

    <script type="text/javascript">
        mini.parse();
        function layout() {
            $.ajax({
                url: "../data/Main/MenuManager.aspx?method=Layout",
                type: 'post',
                dataType: 'json',
                success: function (txt) {
                    //var timestamp = new Date().getTime();
                    location.href = "login.aspx" 
                }
            });
            
        }
        $(window).load(function () {
            $.ajax({
                url: "../data/Main/MenuManager.aspx?method=GetCurrentUserInfo",
                type: 'post',
                dataType: 'json',
                success: function (txt) {
                    if (txt != null && txt.CODE != "") {
                        //if (txt.UserLan == "CN") {
                           
                        //    $("#UserName").text("当前用户: " + txt.Name);
                           
                        //}
                        //else
                        //    $("#UserName").text("Currenct user: " + txt.Name_E);
                    }
                    else
                        location.href = "LogIn.aspx";
                }
            });
        });
        
        var tabs1 = mini.get("mainTabs");
        var currentTab = null;
        top["win"] = window;
       
        function addTab_Child(tab) {
            //添加新tab页的方法
            
            if (tab) {
                tab.url = getVerUrl(tab.url);
                
                tabs1.addTab(tab);
                tabs1.activeTab(tab);
            }
        }
        function onBeforeOpen(e) {
            currentTab = tabs1.getTabByEvent(e.htmlEvent);
            if (!currentTab) {
                e.cancel = true;
            }
        }

        ///////////////////////////
        function closeTab() {
            this.focus();
            tabs1.removeTab(tabs1.getTab(tabs1.activeIndex));
        }
        function closeAllBut() {
            tabs1.removeAll(currentTab);
        }
        function closeAll() {
            var tabss = tabs1.getTab("shouyetab");
            tabs1.removeAll(tabss);
        }
        var tree = mini.get("leftTree");

        function showTab(node) {
            //判断菜单打开方式
            var tabValue = $("#selectTab").val();
            if (tabValue == "ka") {
                window.open(node.url, "_blank"); return;
            }

            var tabs = mini.get("mainTabs");
           
            var id = "tab$" + node.id;
            if (node.id == "CheckBillToPick") {
                window.open(getVerUrl(node.url),node.text);
            }
            else if (node.id == "B_StockOut_Weight") {
                window.open(getVerUrl(node.url), node.text);
            }else {
                var tab = tabs.getTab(id);
                if (!tab) {
                    tab = {};
                    tab._nodeid = node.id;
                    tab.name = id;
                    tab.title = node.text;
                    tab.showCloseButton = true;
                    //这里拼接了url，实际项目，应该从后台直接获得完整的url地址
                    tab.url = getVerUrl(node.url); // mini_JSPath + "../../docs/api/" + node.id + ".html";

                    tabs.addTab(tab);
                }

                tabs.activeTab(tab);
            }
        }
        function onItemSelect(e)
        {
            var item = e.item;
            showTab(item);

        }
        function onNodeSelect(e) {
            var node = e.node;
            var isLeaf = e.isLeaf;
            if (isLeaf) {
                showTab(node);
            }
        }

        function onClick(e) {
            var text = this.getText();
            alert(text);
        }
        function onQuickClick(e) {
            tree.expandPath("datagrid");
            tree.selectNode("datagrid");
        }

        function onTabsActiveChanged(e) {
            var tabs = e.sender;
            var tab = tabs.getActiveTab();
            if (tab && tab._nodeid) {

                var node = tree.getNode(tab._nodeid);
                if (node && !tree.isSelectedNode(node)) {
                    tree.selectNode(node);
                }
            }
        }        //function onSkinChange(skin) {
        //    mini.Cookie.set('miniuiSkin', skin);
        //    //mini.Cookie.set('miniuiSkin', skin, 100);//100天过期的话，可以保持皮肤切换
        //    window.location.reload()
        //}
        
        var CanSet = false;
        window.onload = function () {
            var skin = mini.Cookie.get("miniuiSkin");
            if (skin) {
                var selectSkin = document.getElementById("selectSkin");
                selectSkin.value = skin;
            }

            var frame = document.getElementById("mainframe");
            var demoTree = mini.get("demoTree");

            var url = window.location.href;

            var params = GetParams(location.href, "#");
            if (params.ui) {
                var url = URLS[params.ui];
                if (url) {
                    frame.src = url;
                }
            } else if (params.app) {

                var node = demoTree.getNode(params.app);
                if (node) {
                    demoTree.expandNode(node);
                    demoTree.selectNode(node);

                    var url = URLS[params.app];
                    if (url) {
                        frame.src = url;
                    }
                }

            } else if (params.src) {

                frame.src = params.src;
            }
            CanSet = true;
        }

        //密码更新
        function UpdatePwdEdit() {
            mini.open({
                url: "wms/Main/ChangePwd.html",
                title: "更改密码", width: 400, height: 250,
                onload: function () {
                   
                },
                ondestroy: function (action) {
                    if (action == "OK") {
                        showTips("密码更新成功!");
                    }
                }
            });

           

        }
        //if (!window.WebSocket) {
        //    alert("WebSocket not supported by this browser!");
        //}
        var ws;
        function SendText() {
            ws.send("123");
        }
        function display() {
            //var valueLabel = document.getElementById("valueLabel"); 
            //valueLabel.innerHTML = ""; 
            try {
                //ws = new WebSocket('ws://localhost:30000');
                //ws = new WebSocket('ws://localhost:38071/SocketHandler.ashx');
                ws = new WebSocket('ws://112.124.121.191:30000');
            } catch (e) {
                alert(e.Message);
            }
            //监听消息
            ws.onmessage = function (event) {
                //valueLabel.innerHTML+ = event.data; 
                alert(event.data);
                //log(event.data);
            };
            // 打开WebSocket 
            ws.onclose = function (event) {
                //WebSocket Status:: Socket Closed
            };
            // 打开WebSocket
            ws.onopen = function (event) {
                //WebSocket Status:: Socket Open
                //// 发送一个初始化消息
                ws.send("Hello, Server0!");
            };
            ws.onerror = function (event) {
                //WebSocket Status:: Error was reported
            };
        }
        //var log = function (s) {
        //    if (document.readyState !== "complete") {
        //        log.buffer.push(s);
        //    } else {
        //        document.getElementById("contentId").innerHTML += (s + "\n");
        //    }
        //}
        //function sendMsg() {
        //    var msg = document.getElementById("messageId");
        //    //alert(msg.value);
        //    ws.send(msg.value);
        //}
        //display();
        
        function TabAddHelp() {
            var tabs = mini.get("mainTabs");

            var id = "tab$HelpPager";
            var tab = tabs.getTab(id);
            if (!tab) {
                tab = {};
                tab._nodeid = "HelpPager";
                tab.name = "HelpPager";
                tab.title = "帮助文档";
                tab.showCloseButton = true;
                tab.url = "./../wms/Main/Help_Demo_Show.html";

                tabs.addTab(tab);
            }

            tabs.activeTab(tab);
        }
    </script>
    <script src="js/showWeather.js" type="text/javascript"></script>
</body>
</html>
