<%@ page language="C#" autoeventwireup="true" inherits="AS_Report, App_Web_be3en2aq" %>
<%@ Register TagPrefix="activereportsweb" Namespace="DataDynamics.ActiveReports.Web"
    Assembly="ActiveReports.Web, Version=5.1.0.0158, Culture=neutral, PublicKeyToken=cc4967777c49a3ff" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<head id="Head1" runat="server">
    <title></title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <script src="../js/boot.js" type="text/javascript"></script>
    <script src="../js/core.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div id="div">
        <activereportsweb:WebViewer ID="arvWebMain" runat="server" Width="100%" Height="100%"
            CodeBase="arview2.cab#Version=2,3,3,1276">
        </activereportsweb:WebViewer>
        </div>
    </form>
    <script type="text/javascript">
        function setsize () {
            $("#div").width($(window).width() );
            $("#div").height($(window).height() );
            $("#arvWebMain_controlDiv").height($(window).height() );
        }
        $(setsize);
        $(window).resize(setsize);
    </script>
</body>
</html>