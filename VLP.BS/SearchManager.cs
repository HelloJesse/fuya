using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using Nordasoft.Data.Sql;

namespace VLP.BS
{
    /// <summary>
    /// 查询管理类
    /// </summary>
    public class SearchManager
    {
        private static System.Data.SqlClient.SqlCommand _GetGridDataCommand;

        private static System.Data.SqlClient.SqlCommand GetGridDataCommand()
        {
            if (_GetGridDataCommand == null)
            {
                _GetGridDataCommand = new System.Data.SqlClient.SqlCommand("sp_sys_GetGridData");
                _GetGridDataCommand.CommandType = System.Data.CommandType.StoredProcedure;
                _GetGridDataCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@PageID", System.Data.SqlDbType.Int));
                _GetGridDataCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@UserID", System.Data.SqlDbType.Int));
                _GetGridDataCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@PAGENO", System.Data.SqlDbType.SmallInt));
                _GetGridDataCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@PAGECOUNT", System.Data.SqlDbType.SmallInt));
                _GetGridDataCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@WHERESQL", System.Data.SqlDbType.NVarChar, -1));
                _GetGridDataCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@OrderBy", System.Data.SqlDbType.VarChar, 1000));
            }
            return _GetGridDataCommand.Clone();
        }
        /// <summary>
        /// 根据WhereSQL,获取查询命令
        /// </summary>
        /// <param name="pageID"></param>
        /// <param name="userID"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="whereSQL"></param>
        /// <returns></returns>
        public static SqlCommand GetSearchDataBySQLCommand(int pageID, string userID, short pageNo, short pageSize, string whereSQL,string orderBy)
        {
            SqlCommand cmmd = GetGridDataCommand();
            cmmd.Parameters["@PageID"].Value = pageID;
            cmmd.Parameters["@UserID"].Value = userID;
            cmmd.Parameters["@PAGENO"].Value = pageNo + 1;
            cmmd.Parameters["@PAGECOUNT"].Value = pageSize;
            cmmd.Parameters["@WHERESQL"].Value = whereSQL;
            cmmd.Parameters["@OrderBy"].Value = orderBy;
            return cmmd;
        }

        private static System.Data.SqlClient.SqlCommand _GetGridDataByIDCommand;

        private static System.Data.SqlClient.SqlCommand GetGridDataByIDCommand()
        {
            if (_GetGridDataByIDCommand == null)
            {
                _GetGridDataByIDCommand = new System.Data.SqlClient.SqlCommand("sp_Sys_GetGridDataByID");
                _GetGridDataByIDCommand.CommandType = System.Data.CommandType.StoredProcedure;

                _GetGridDataByIDCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@PageID", System.Data.SqlDbType.Int));
                _GetGridDataByIDCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@UserID", System.Data.SqlDbType.Int));
                _GetGridDataByIDCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@BILLID", System.Data.SqlDbType.VarChar, 8000));
                _GetGridDataByIDCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@PageNo", System.Data.SqlDbType.SmallInt));
                _GetGridDataByIDCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@PageCount", System.Data.SqlDbType.SmallInt));
            }
            return _GetGridDataByIDCommand.Clone();
        }
        /// <summary>
        /// 根据ID,获取Command命令
        /// </summary>
        /// <param name="pageID"></param>
        /// <param name="userID"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static SqlCommand GetSearchDataByIDCommand(int pageID, string userID, short pageNo, short pageSize, string ids)
        {
            SqlCommand cmmd = GetGridDataByIDCommand();
            cmmd.Parameters["@PageID"].Value = pageID;
            cmmd.Parameters["@UserID"].Value = userID;
            cmmd.Parameters["@PAGENO"].Value = pageNo + 1;
            cmmd.Parameters["@PAGECOUNT"].Value = pageSize;
            cmmd.Parameters["@BILLID"].Value = GetSearchIDStr(ids, pageNo, pageSize);
            return cmmd;
        }


        private static System.Data.SqlClient.SqlCommand _GetGridDataByOrderBySQLCommand;

        private static System.Data.SqlClient.SqlCommand GetGridDataByOrderBySQLCommand()
        {
            if (_GetGridDataByOrderBySQLCommand == null)
            {
                _GetGridDataByOrderBySQLCommand = new System.Data.SqlClient.SqlCommand("sp_sys_GetGridDataByOrderBySQL");
                _GetGridDataByOrderBySQLCommand.CommandType = System.Data.CommandType.StoredProcedure;

                _GetGridDataByOrderBySQLCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@PageID", System.Data.SqlDbType.Int));
                _GetGridDataByOrderBySQLCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@UserID", System.Data.SqlDbType.Int));
                _GetGridDataByOrderBySQLCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@IDS", System.Data.SqlDbType.VarChar, -1));
                _GetGridDataByOrderBySQLCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@PageNo", System.Data.SqlDbType.SmallInt));
                _GetGridDataByOrderBySQLCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@PageCount", System.Data.SqlDbType.SmallInt));
                _GetGridDataByOrderBySQLCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@OrderBy", System.Data.SqlDbType.VarChar,1000));
                _GetGridDataByOrderBySQLCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@SubTables", System.Data.SqlDbType.VarChar, 1000));
            }
            return _GetGridDataByOrderBySQLCommand.Clone();
        }
        /// <summary>
        /// 根据ID,获取Command命令
        /// </summary>
        /// <param name="pageID"></param>
        /// <param name="userID"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static SqlCommand GetGridDataByOrderBySQLCommand(int pageID, string userID, short pageNo, short pageSize,
            string ids,string orderBySQL,string subTables)
        {
            SqlCommand cmmd = GetGridDataByOrderBySQLCommand();
            cmmd.Parameters["@PageID"].Value = pageID;
            cmmd.Parameters["@UserID"].Value = userID;
            cmmd.Parameters["@PAGENO"].Value = pageNo + 1;
            cmmd.Parameters["@PAGECOUNT"].Value = pageSize;
            cmmd.Parameters["@IDS"].Value = ids;
            cmmd.Parameters["@OrderBy"].Value = orderBySQL;
            cmmd.Parameters["@SubTables"].Value = subTables;
            return cmmd;
        }

        /// <summary>
        /// 获取编辑界面列表数据命令
        /// </summary>
        private static System.Data.SqlClient.SqlCommand _GetGridData_EditDtlCommand;
        
        private static System.Data.SqlClient.SqlCommand GetGridData_EditDtlCommand()
        {
            if (_GetGridData_EditDtlCommand == null)
            {

                _GetGridData_EditDtlCommand = new System.Data.SqlClient.SqlCommand("sp_sys_GetGridData_EditDtl");

                _GetGridData_EditDtlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                _GetGridData_EditDtlCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@PageID", System.Data.SqlDbType.Int));
                _GetGridData_EditDtlCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@MainID", System.Data.SqlDbType.Int));
                _GetGridData_EditDtlCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@OrderBy", System.Data.SqlDbType.VarChar, 100));
            }
            return _GetGridData_EditDtlCommand.Clone();
        }
        /// <summary>
        /// 获取编辑界面列表数据命令
        /// </summary>
        /// <param name="pageID"></param>
        /// <param name="userID"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static SqlCommand GetGridData_EditDtlCommand(int pageID, int mainID,  string orderby)
        {
            SqlCommand cmmd = GetGridData_EditDtlCommand();
            cmmd.Parameters["@PageID"].Value = pageID;
            cmmd.Parameters["@MainID"].Value = mainID;
            cmmd.Parameters["@OrderBy"].Value = orderby;
            return cmmd;
        }

        /// <summary>
        /// 返回查询的ID
        /// </summary>
        /// <param name="Ids">传入的IDS</param>
        /// <param name="PageIndex">页码</param>
        /// <param name="PageSize">每页数据量</param>
        /// <returns></returns>
        private static string GetSearchIDStr(string Ids, short PageIndex, short PageSize)
        {
            
            string[] id = Ids.Split(',');
            short begin = (short)(PageIndex * PageSize);
            short end = (short)((PageIndex + 1) * PageSize);
            //string IDss = "";
            //for (short i = 0; i < id.Length; i++)
            //{
            //    if (i >= begin && i < end)
            //    {
            //        IDss = IDss + id[i] + ",";
            //    }
            //}
            //if (IDss.Length > 0)
            //    IDss = IDss.Substring(0, IDss.Length - 1);
            //return IDss;
            if (id.Length < end)
                end = (short)id.Length;
            if (begin > end)
                begin = end;

            string[] tos = new string[end - begin];
            Array.Copy(id, begin, tos, 0, tos.Length);
            return string.Join(",", tos);
        }
        /// <summary>
        /// 初始化编辑列表和列表查询命令
        /// </summary>
        public static void InitCommand()
        {
            GetGridData_EditDtlCommand();
            GetGridData_EditDtlCommand();
            GetGridDataByOrderBySQLCommand();
            GetGridDataCommand();
            EditManager.GetBillAndDTLDataByIDCommand();
            EditManager.GetGridDataCommand();
        }

        /// <summary>
        /// 根据页面ID，IDS、用户ID，获取数据
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
