using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Specialized;

namespace VLP.BS
{

    public class GridManager
    {
        string _PageID;
        string _UserID;
        string _LanguageFlag;
        public GridManager(string pageID, string userID)
            : this(pageID, userID, "CN")
        {
            
        }
        public GridManager(string pageID, string userID,string language)
        {
            _PageID = pageID;
            _UserID = userID;
            _LanguageFlag = language;
        }


        public void Init(string cnnstr)
        {
            DataSet ds = new DataSet();
            SqlCommand cmmd = new SqlCommand();
            cmmd.Connection = new SqlConnection(cnnstr);
            cmmd.CommandText = @"sp_Sys_GetGridConfig";
            cmmd.CommandType = CommandType.StoredProcedure;
            cmmd.Parameters.Add("@PageID", SqlDbType.Int).Value = _PageID;
            cmmd.Parameters.Add("@UserID", SqlDbType.Int).Value = _UserID;
            cmmd.Parameters.Add("@Language", SqlDbType.VarChar, 2).Value = _LanguageFlag;
            SqlDataAdapter da = new SqlDataAdapter(cmmd);
            da.Fill(ds);
            FieldID = ds.Tables[0].Rows[0]["PKField"].ToString();
            string fieldwidth = ds.Tables[0].Rows[0]["FieldIDWidth"].ToString();
            NameValueCollection nvs = GetFieldWidth(fieldwidth);
 
            Columns = new Collection<GridColumn>();
            foreach (DataRow row in ds.Tables[1].Rows)
            {
                GridColumn c = new GridColumn();
                
                c.HeaderText = row["HeaderText"].ToString();
                c.FieldName = row["ShowName"].ToString();
                c.Hide = (bool)row["Hide"];
                if (nvs[c.FieldName] == null)
                {
                    c.Width = int.Parse(row["Width"].ToString());
                }
                else
                {
                    c.Width = int.Parse(nvs[c.FieldName]);
                }
                if (c.Width == 0) c.Width = 100;

                c.Format = row["Format"].ToString();
                Columns.Add(c);
            }
        }
        public void SaveColumnsWidth(string cnnstr,string language,string fieldWidth)
        {
            DataSet ds = new DataSet();

            SqlCommand cmmd = new SqlCommand();
            cmmd.Connection = new SqlConnection(cnnstr);

            cmmd.CommandText = @"
DECLARE @IDS VARCHAR(8000);
DECLARE @SQL	NVARCHAR(MAX);
SELECT @IDS=FieldID FROM dbo.Sys_Search_Page_User WHERE  PageID=@PageID AND UserID=@UserID


SET @SQL=N'SELECT [FieldID]
      ,[ShowName]
  FROM [dbo].[Sys_Search_Page_ShowField] WHERE PageID=@PageID AND FieldID IN('+@IDS+')'

EXEC sys.sp_executeSQL @SQL,N'@PageID AS  int',@PageID";
            cmmd.Parameters.Add("@PageID", SqlDbType.Int).Value = _PageID;
            cmmd.Parameters.Add("@UserID", SqlDbType.Int).Value = _UserID;

            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(cmmd);
            da.Fill(dt);

            StringBuilder sb = new StringBuilder();

            string[] items = new string[dt.Rows.Count];
            int count = 0;
            NameValueCollection nvs = GetFieldWidth(fieldWidth);
            sb.AppendFormat(@"
UPDATE Sys_Search_Page_ShowField SET ColOrder=255 WHERE PageID=@PageID;
");
            foreach (string k in nvs.Keys)
            {
                DataRow[] rows = dt.Select(string.Format("ShowName='{0}'", k));
                if (rows.Length == 1)
                {
                    items[count] = rows[0]["FieldID"].ToString();
                    sb.AppendFormat(@"
UPDATE Sys_Search_Page_ShowField SET Width={1},ColOrder={2} WHERE FieldID={0} 
", items[count],nvs[k],count+1);
                    count++;
                    dt.Rows.Remove(rows[0]);
                }
            }
            foreach (DataRow row in dt.Rows)
            {
                items[count] = row["FieldID"].ToString();
                count++;
            }
            string fieldid = string.Join(",", items);
            cmmd.CommandText = @"UPDATE dbo.Sys_Search_Page_User SET FieldIDWidth=@FieldIDWidth,FieldID=@FieldID WHERE PageID=@PageID AND UserID=@UserID";
            if (language.ToUpper() == "E")
            {
                cmmd.CommandText = @"UPDATE dbo.Sys_Search_Page_User SET FieldIDWidth_E=@FieldIDWidth,FieldID_E=@FieldID WHERE PageID=@PageID AND UserID=@UserID";
            }
            //cmmd.Parameters.Add("@PageID", SqlDbType.Int).Value = _PageID;
            //cmmd.Parameters.Add("@UserID", SqlDbType.Int).Value = _UserID;
            cmmd.Parameters.Add("@FieldIDWidth", SqlDbType.VarChar, -1).Value = fieldWidth;
            cmmd.Parameters.Add("@FieldID", SqlDbType.VarChar, 8000).Value = fieldid;
            cmmd.CommandText = string.Format(@"{0}{1}
EXEC sp_sys_UpdateSELECTSQL @PageID,@UserID,0,@Language
", cmmd.CommandText,sb.ToString());
            cmmd.Parameters.Add("@Language", SqlDbType.VarChar, 2).Value = language;
            using (cmmd.Connection)
            {
                cmmd.Connection.Open();
                cmmd.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// 主键
        /// </summary>
        public string FieldID;
        
        public System.Collections.Specialized.NameValueCollection GetFieldWidth(string fieldwidth)
        {
            NameValueCollection nvs = new NameValueCollection();
            foreach (string f in fieldwidth.Split(';'))
            {
                if (string.IsNullOrEmpty(f)) continue;
                string[] item = f.Split(':');
                if (item.Length == 2)
                {
                    nvs.Add(item[0], item[1]);
                }
            }
            return nvs;
        }
        public Collection<GridColumn> Columns;
        

    }

    public class GridColumn
    {
        public int Width;
        public string HeaderText;
        public string FieldName;
        public bool Hide;
        public string Format = "";
    }
}
