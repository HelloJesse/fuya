<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" %>
<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
<meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1">
<title>复亚飞控系统</title>
<meta name="description" content="">
<meta name="keywords" content="">
<meta http-equiv="Pragma" content="no-cache">
<meta http-equiv="Cache-Control" content="no-cache">
<meta http-equiv="Expires" content="-1"> 

<link href="css/new/register-login.css" rel="stylesheet" />
</head>
<body>

   <div id="box"></div>


    <div class="cent-box">
        <div class="cent-box-header">
            <h1 class="main-title">复亚飞控系统</h1>
            <h2 class="sub-title">潜心科技，创新为您</h2>
        </div>

        <div class="cont-main clearfix">
            <div class="index-tab">
                <div class="index-slide-nav">
                    <a href="login.html" class="active">登录</a>
                    <div class="slide-bar"></div>
                </div>
            </div>

            <div class="login form">
                <div class="group">
                    <div class="group-ipt email">
                        <input id="CompanyName" type="text" placeholder="公司名称" onkeydown="onKeyDown(1)" value="复亚飞控系统" style="display:none" />
                        <input id="UserCode"  type="text" placeholder="账号" onkeydown="onKeyDown(2)" value="" class="ipt" required />
                    </div>
                    <div class="group-ipt password">
                        <input id="Password" type="password" placeholder="密码" onkeydown="onKeyDown(3)" class="ipt" value="" required />
                    </div>
                </div>
            </div>


            <div class="button">
                <button type="submit" class="login-btn register-btn" id="button" onclick="submitForm()">登录</button>
            </div>

            <div class="remember clearfix">
                <label class="remember-me"><span class="icon"><span class="zt"></span></span>
                <input type="checkbox" name="remember-me" id="remember-me" class="remember-mecheck" checked>记住我
                </label>
            </div>
        </div>
    </div>


    <script src="js/new/particles.js"></script>
    <script src="js/new/background.js"></script>
    <script src="js/jquery-1.6.2.min.js"></script>
    <script src="js/new/layer/layer.js"></script>
    <script src="js/boot.js?v=20171103"></script>

    <script type="text/javascript">


        
        $(function () {
            $(document.body).height($(window).height());
            var companyname_t = getCookie_My("CompanyName");
            var usercode_t = getCookie_My("UserCode");

            if (usercode_t) {
                $("#UserCode").val(usercode_t);
                $("#Password").focus();
            }
            else {
                $("#UserCode").focus();
            }
        });

        function onKeyDown(type) {
            if (event.keyCode == 13) {
                if (type == 1) {
                    $("#UserCode").focus();
                } else if (type == 2) {
                    $("#Password").focus();
                }
                else if (type == 3) {
                    submitForm();
                }
            }
        }

        function submitForm() {
            var companyName = $("#CompanyName").val();
            var userCode = $("#UserCode").val();
            var password = $("#Password").val();
            if (companyName == "") {
                mini.alert("公司名称不能为空");
                $("#CompanyName").focus();
                return;
            }
            if (userCode == "") {
                mini.alert("账号不能为空");
                $("#UserCode").focus();
                return;
            }
            if (password == "") {
                mini.alert("密码不能为空");
                $("#Password").focus();
                return;
            }
            
            $.ajax({
                url: "data/Main/MenuManager.aspx?method=Login",
                type: "post",
                data: { userCode: userCode, password: password, userLan: "CN", CompanyName: companyName },
                success: function (text) {
                    var data = mini.decode(text);
                    if (data.msg == "true") {
                        if (document.getElementById("remember-me").checked) {
                            setCookie("CompanyName", companyName, 7);
                            setCookie("UserCode", userCode, 7);
                            setCookie("UserName", data.userName, 7);
                            setCookie("CustomerName", data.CustomerName, 7);
                            //setCookie("baseData", location.host + '/js/basedata/' + companyName + '_jsdata.js');
                        } else {
                            setCookie("CompanyName", "", 7);
                            setCookie("UserCode", "", 7);
                            setCookie("UserName", "", 7);
                            setCookie("CustomerName", "", 7);
                        }
                        //setTimeout(function () {
                        window.location = "main.aspx";
                        //}, 1500);
                    }
                    else {
                        mini.alert("用户名或密码不正确");
                    }

                }
            });
        }

        $("#remember-me").click(function () {
            var n = document.getElementById("remember-me").checked;
            if (n) {
                $(".zt").show();
            } else {
                $(".zt").hide();
            }
        });
      
          </script>
</body>
</html>