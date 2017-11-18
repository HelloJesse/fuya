using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using Nordasoft.Data.Sql;

namespace VLP.BS
{
    public class SettingManager
    {
        /// <summary>
        /// 根据配置，获取按钮表
        /// </summary>
        /// <param name="btnconfigs"></param>
        /// <returns></returns>
        public static System.Data.DataTable GetButtonTable(System.Collections.ArrayList btnconfigs, System.Web.SessionState.HttpSessionState session, string language)
        {
            System.Data.DataTable dtviewbtn = new System.Data.DataTable();
            dtviewbtn.Columns.Add("PID", typeof(string));   //父结点
            dtviewbtn.Columns.Add("PText", typeof(string));   //父结点
            dtviewbtn.Columns.Add("BtnID", typeof(string));
            dtviewbtn.Columns.Add("Text", typeof(string));
            dtviewbtn.Columns.Add("Ico", typeof(string));
            dtviewbtn.Columns.Add("Click", typeof(string));
            
            foreach (string c in btnconfigs)
            {
                //判断是否为多级菜单
                string[] mconfig = c.Split(':');
                if (mconfig.Length == 2)
                {
                    //多级菜单
                    string[] config = mconfig[1].Split('|');
                    foreach (string mc in config)
                    {
                        string[] mcc = mc.Split('.');
                        if (mcc.Length == 2)
                        {
                            InitButtonRow(dtviewbtn, mcc, mconfig[0], language);
                        }
                        else
                            continue;
                    }

                }
                else
                {
                    string[] config = c.Split('.');
                    if (config.Length == 2)
                    {
                        InitButtonRow(dtviewbtn, config, string.Empty, language);
                    }
                }

            }
            return dtviewbtn;
        }

        private static void InitButtonRow(System.Data.DataTable dtviewbtn, string[] config, string pid, string language)
        {
            System.Data.DataTable functionTable = CacheManager.FunctionTable;
            System.Data.DataRow[] rows = functionTable.Select(string.Format("ID={0}", config[0]));
            if (rows.Length != 1)
            {
                throw new ApplicationException(string.Format("未找到ID为[{0}]的功能项.", config[0]));
            }
            string namefield = "Name";
            if (string.IsNullOrEmpty(language) == false)
            {
                namefield = string.Format("{0}_{1}", namefield, language);
            }
            string name = rows[0][namefield].ToString();
            if (string.IsNullOrEmpty(name))
            {
                name = rows[0]["Name"].ToString();
            }
            string ico = rows[0]["ICO"].ToString();
            string code = rows[0]["Code"].ToString();

            System.Data.DataRow row = dtviewbtn.NewRow();

            if (string.IsNullOrEmpty(pid) == false)
            {
                //读取名称
                System.Data.DataRow[] prows = functionTable.Select(string.Format("ID={0}", pid));
                if (prows.Length != 1)
                {
                    throw new ApplicationException(string.Format("未找到ID为[{0}]的功能项.", pid));
                }
                row["PID"] = string.Format("btn_{0}_{1}", pid, prows[0]["Code"]);
                //row["PID"] = string.Format("{0}", pid);
                //row["PID"] = "8";
                //row["PID"] = pid;
                row["PText"] = prows[0]["Name"];
            }
            else
            {
                row["PID"] = string.Empty;
            }
            row["BtnID"] = string.Format("btn_{0}_{1}", config[0], code);
            row["Text"] = name;
            row["Ico"] = ico;
            row["click"] = config[1];
            dtviewbtn.Rows.Add(row);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageid"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static System.Data.DataTable GetDataByIDS(string pageid, string ids, string userid, DataBase db)
        {
            string sql = string.Format(@"
	DECLARE @SQL	NVARCHAR(MAX);
    DECLARE @V      NVARCHAR(MAX);
	DECLARE @PKNAME	VARCHAR(50);
	SELECT @SQL=SU.SELECTSQL,@PKNAME=S.PKName FROM dbo.Sys_Search_Page_User SU INNER JOIN dbo.Sys_Search_Page S ON S.PageID=SU.PageID
		WHERE SU.UserID=@UserID AND SU.PageID=@PageID;
    SET @V='INNER JOIN dbo.SplitIndx(''{0}'')excelIndx  ON [excelIndx].INDX=M.'+@PKNAME
	+' ORDER BY [excelIndx].ID'
    SET @SQL=@SQL+@V;
	EXEC sys.sp_executesql @SQL

", ids);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(sql);
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@UserID", System.Data.SqlDbType.Int));
            cmd.Parameters["@UserID"].Value = userid;
            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@PageID", System.Data.SqlDbType.SmallInt));
            cmd.Parameters["@PageID"].Value = pageid;
            //cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@IDS", System.Data.SqlDbType.VarChar, -1));
            //cmd.Parameters["@IDS"].Value = ids;
            cmd.CommandText = sql;
            return db.ExecuteDataTable(cmd);
        }
    }
}
