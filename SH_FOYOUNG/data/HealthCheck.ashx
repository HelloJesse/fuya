<%@ WebHandler Language="C#" Class="HealthCheck" %>

using System;
using System.Web;
using System.Net;
using System.Net.Sockets;
using System.Configuration;


public class HealthCheck : IHttpHandler {



    public void ProcessRequest (HttpContext context) {
        context.Response.ContentType = "text/plain";

        var method = context.Request.QueryString["method"];
        switch (method)
        {
            case "get":
                {
                    GetHealthCheckStatus(context);
                    return;
                }


            case "update":
                {
                    UpdateHealthCheckStatus(context);
                    return;
                }
        }
    }

    private void GetHealthCheckStatus(HttpContext context)
    {
        context.Response.Write(Config.IsYaoKongTcpConnected);
        context.Response.End();
    }

    private void UpdateHealthCheckStatus(HttpContext context)
    {
        var status = context.Request.QueryString["status"];
        Config.IsYaoKongTcpConnected = status =="1"? true:false;

        context.Response.End();
    }


    public bool IsReusable {
        get {
            return false;
        }
    }

}