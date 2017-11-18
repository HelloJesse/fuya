using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace VLP.BS
{
    public class EditManager
    {
        private static System.Data.SqlClient.SqlCommand _GetBillDataByIDCommand;

        internal static System.Data.SqlClient.SqlCommand GetGridDataCommand()
        {
            if (_GetBillDataByIDCommand == null)
            {
                _GetBillDataByIDCommand = new System.Data.SqlClient.SqlCommand("sp_Sys_GetBillDataByID");
                _GetBillDataByIDCommand.CommandType = System.Data.CommandType.StoredProcedure;
                _GetBillDataByIDCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@PageID", System.Data.SqlDbType.Int));
                _GetBillDataByIDCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@ID", System.Data.SqlDbType.Int));
                _GetBillDataByIDCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@UserID", System.Data.SqlDbType.Int));
            }
            return _GetBillDataByIDCommand.Clone();
        }
        /// <summary>
        /// 根据WhereSQL,获取查询命令
        /// </summary>
        /// <param name="pageID"></param>
        /// <param name="userID"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SqlCommand GetGetBillDataByIDCommand(int pageID, int id, string userID)
        {
            SqlCommand cmmd = GetGridDataCommand();
            cmmd.Parameters["@PageID"].Value = pageID;
            cmmd.Parameters["@ID"].Value = id;
            cmmd.Parameters["@UserID"].Value = userID;
            return cmmd;
        }

        private static System.Data.SqlClient.SqlCommand _GetBillAndDTLDataByIDCommand;

        internal static System.Data.SqlClient.SqlCommand GetBillAndDTLDataByIDCommand()
        {
            if (_GetBillAndDTLDataByIDCommand == null)
            {
                _GetBillAndDTLDataByIDCommand = new System.Data.SqlClient.SqlCommand(@"
EXEC sp_Sys_GetBillDataByID @PageID,@ID,@UserID
EXEC sp_sys_GetGridData_EditDtl @DTLPID,@ID,''");
                _GetBillAndDTLDataByIDCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@PageID", System.Data.SqlDbType.Int));
                _GetBillAndDTLDataByIDCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@ID", System.Data.SqlDbType.Int));
                _GetBillAndDTLDataByIDCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@UserID", System.Data.SqlDbType.Int));
                _GetBillAndDTLDataByIDCommand.Parameters.Add(new System.Data.SqlClient.SqlParameter("@DTLPID", System.Data.SqlDbType.Int));
            }
            return _GetBillAndDTLDataByIDCommand.Clone();
        }
        /// <summary>
        /// 获取单据主信息及明细信息
        /// </summary>
        /// <param name="pageID"></param>
        /// <param name="id"></param>
        /// <param name="userID"></param>
        /// <param name="dtlPageID"></param>
        /// <returns></returns>
        public static SqlCommand GetBillAndDTLDataByIDCommand(int pageID, int id, string userID, int dtlPageID)
        {
            SqlCommand cmmd = GetBillAndDTLDataByIDCommand();
            cmmd.Parameters["@PageID"].Value = pageID;
            cmmd.Parameters["@ID"].Value = id;
            cmmd.Parameters["@UserID"].Value = userID;
            cmmd.Parameters["@DTLPID"].Value = dtlPageID;
            return cmmd;
        }
        /// <summary>
        /// 获取单据主信息及明细信息
        /// </summary>
        /// <param name="pageID"></param>
        /// <param name="id"></param>
        /// <param name="userID"></param>
        /// <param name="dtlPageID"></param>
        /// <returns></returns>
        public static SqlCommand GetBillAndDTLDataByIDCommand(int pageID, int id, string userID, string dtlPageIDS)
        {
            string[] dtlids = dtlPageIDS.Split(',');
            SqlCommand cmmd = GetBillAndDTLDataByIDCommand();
            cmmd.Parameters["@PageID"].Value = pageID;
            cmmd.Parameters["@ID"].Value = id;
            cmmd.Parameters["@UserID"].Value = userID;
            cmmd.Parameters["@DTLPID"].Value = int.Parse(dtlids[0]);
            if (dtlids.Length > 1)
            {
                StringBuilder sb = new StringBuilder();
                string dtlpidname = string.Empty;
                for (int i = 1; i < dtlids.Length; i++)
                {
                    int dtlid = 0;
                    if (int.TryParse(dtlids[i], out dtlid) == false)
                        continue;
                    dtlpidname = string.Format("@DTLPID{0}", i);
                    sb.AppendFormat("{0}EXEC sp_sys_GetGridData_EditDtl {1},@ID,''", Environment.NewLine, dtlpidname);
                    cmmd.Parameters.Add(new SqlParameter(dtlpidname, SqlDbType.Int)).Value = dtlid;
                }
                if (sb.Length > 0)
                {
                    cmmd.CommandText = string.Format("{0}{1}", cmmd.CommandText, sb.ToString());
                }
            }
            return cmmd;
        }
    }
}
