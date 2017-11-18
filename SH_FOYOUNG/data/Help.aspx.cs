using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
public partial class data_Help : BasePage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string idstr = Request["ID"];
        int id = 0;
        if (int.TryParse(idstr, out id))
        {
            SqlCommand cmmd = new SqlCommand("SELECT Helphtml FROM Sys_SystemMenu_Dtl WHERE ID=@ID");
            cmmd.Parameters.Add(new SqlParameter("@ID", System.Data.SqlDbType.Int));
            cmmd.Parameters["@ID"].Value = id;
            string vlpcnn = System.Configuration.ConfigurationManager.ConnectionStrings["config"].ConnectionString;
            Nordasoft.Data.Sql.DataBase db = new Nordasoft.Data.Sql.DataBase(vlpcnn);
            System.Data.DataTable dt = db.ExecuteDataTable(cmmd);
            if (dt.Rows.Count == 1)
            {
                this.Response.Write(dt.Rows[0][0].ToString());


            }
            else
            {
                this.Response.Write("未找到对应的帮助信息.");
            }
        }
        else
        {
            this.Response.Write("参数不对.");
        }
        this.Response.Flush();
    }
}