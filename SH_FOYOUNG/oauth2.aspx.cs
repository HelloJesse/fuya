using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;


public partial class oauth2 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string code = this.Request["code"];


        string aa = GetResponse(string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid=wxc1396289a265ef39&secret=b669b715a6ecc42f078f5c2455e5d4fa&code={0}&grant_type=authorization_code", code));

        aa = aa.Trim().Trim('{').Trim('}');
        string[] temp = aa.Split(':');

        
        int i = 0;
        string token = temp[1].Split(',')[0].Trim('"');
        string openid = temp[4].Split(',')[0].Trim('"');
        string url = string.Format("https://api.weixin.qq.com/sns/userinfo?access_token={0}&openid={1}",token,openid);

        this.Response.Write(GetResponse(url));

        //foreach (string a in temp)
        //{
        //    i++;

        //    this.Response.Write(string.Format("{0}:{1}",i,a));
        //}

        //this.Response.Write(string.Format("{0}:{1}", "token", token));
        //this.Response.Write(string.Format("{0}:{1}", "openid", openid));
        
        //if (string.IsNullOrEmpty(code))
        //{
        //    this.Response.Write(this.Request.Url.ToString());
        //}
        //else
        //{
        //    this.Response.Write(code);
        //}
    }
    private string GetResponse(string url)
    {
        System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)HttpWebRequest.Create(url);
        System.IO.Stream stream = request.GetResponse().GetResponseStream();
        byte[] bytes = new byte[1024];
        int total = 0;
        System.Collections.ObjectModel.Collection<byte[]> bts = new System.Collections.ObjectModel.Collection<byte[]>();
        int count = stream.Read(bytes, 0, bytes.Length);
        while (count > 0)
        {
            total = total + count;
            Byte[] tempbyte = new byte[count];
            //bytes.CopyTo(tempbyte, count);
            Array.Copy(bytes, 0, tempbyte, 0, count);
            bts.Add(tempbyte);
            count = stream.Read(bytes, 0, bytes.Length);
        }
        stream.Close();
        //
        Byte[] totalbyte = new byte[total];
        count = 0;
        foreach (byte[] b in bts)
        {
            b.CopyTo(totalbyte, count);
            count = count + b.Length;
        }
        return System.Text.UTF8Encoding.UTF8.GetString(totalbyte);
    }
}