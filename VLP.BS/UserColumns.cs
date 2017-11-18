using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.ObjectModel;
using Nordasoft.Data.Sql;
using System.Configuration;  


namespace VLP.BS
{
    /// <summary>
    /// 获取用户对应的列信息
    /// </summary>
    public class UserColumns
    {
        /// <summary>
        /// 获取用户定义列信息
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="language"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static DataTable GetUserColumnData(string pageId, string userId, string language, DataBase db)
        {
            SqlCommand cmmd = new SqlCommand("sp_GetUserColumnsData");
            cmmd.CommandType = CommandType.StoredProcedure;

            cmmd.Parameters.Add("@PageID", SqlDbType.Int);
            cmmd.Parameters["@PageID"].Value = pageId;
            cmmd.Parameters.Add("@USERID", SqlDbType.Int);
            cmmd.Parameters["@USERID"].Value = userId;
            cmmd.Parameters.Add("@Language", SqlDbType.NVarChar,10);
            cmmd.Parameters["@Language"].Value = language;
            DataTable dt = db.ExecuteDataTable(cmmd);
            return dt;
        }

        /// <summary>
        /// 获取必填列
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static DataTable GetNecessaryFields(string pageId, DataBase db)
        {
            SqlCommand cmmd = new SqlCommand(@"SELECT FieldID,ShowName,ColOrder,Width FROM Sys_Search_Page_ShowField 
	                WHERE PageID = @PageID AND ShowName IN (SELECT VALUE FROM dbo.SplitString((SELECT NecessaryFields FROM Sys_Search_Page WHERE PageID =@PageID),',',1)); ");
            cmmd.Parameters.Add("@PageID", SqlDbType.Int);
            cmmd.Parameters["@PageID"].Value = pageId;
            DataTable dt = db.ExecuteDataTable(cmmd);
            return dt;
        }

        /// <summary>
        ///更新用户自定义列信息 
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="fieldID"></param>
        /// <param name="fileldIDOrder"></param>
        /// <param name="fileldIDWidth"></param>
        /// <param name="userId"></param>
        /// <param name="db"></param>
        public static void UpdateUserColumnsInfo(string pageId, string fieldID, string fileldIDWidth, string userId, string language, DataBase db)
        {
            SqlCommand cmmd = new SqlCommand("sp_UpdateUserColumnsInfo");
            cmmd.CommandType = CommandType.StoredProcedure;
            cmmd.Parameters.Add("@PageID", SqlDbType.Int);
            cmmd.Parameters["@PageID"].Value = pageId;
            cmmd.Parameters.Add("@FieldID", SqlDbType.NVarChar, -1);
            cmmd.Parameters["@FieldID"].Value = fieldID;
            cmmd.Parameters.Add("@FileldIDWidth", SqlDbType.NVarChar, -1);
            cmmd.Parameters["@FileldIDWidth"].Value = fileldIDWidth;
            cmmd.Parameters.Add("@USERID", SqlDbType.Int);
            cmmd.Parameters["@USERID"].Value = userId;
            cmmd.Parameters.Add("@Language", SqlDbType.NVarChar, 10);
            cmmd.Parameters["@Language"].Value = language;
            db.ExecuteNonQuery(cmmd);
        }
                
    }
}
