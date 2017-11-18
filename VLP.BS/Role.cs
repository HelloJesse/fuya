using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Nordasoft.Data.Sql;

namespace VLP.BS
{
    public class Role
    {
        public static DataTable GetBaseTableInfo(string roleid, DataBase db)
        {
            int roleid_int = 0;
            int.TryParse(roleid, out roleid_int);
            SqlCommand cmmd = new SqlCommand(@"
SELECT TableName,Remark+CASE WHEN LEN(IDS)>0 THEN '[已设]' ELSE '' END AS Remark,0 AS pid FROM Sys_Table_Info 
    LEFT JOIN dbo.Sys_Role_View_BaseDataID ON TableName=BaseTable AND RoleID=@RoleID WHERE IsSetViewBaseData=1");
            cmmd.Parameters.Add("@RoleID", SqlDbType.SmallInt);
            cmmd.Parameters["@RoleID"].Value = roleid_int;

            return db.ExecuteDataTable(cmmd);
        }
        /// <summary>
        /// 保存用户拥有角色
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="viewroleid"></param>
        public static void SaveUserViewRole(string userid, string viewroleid, DataBase db)
        {
            string[] roleids = viewroleid.Split(',');
            string sql = string.Empty;
            if (string.IsNullOrEmpty(viewroleid))
            {
                sql = "DELETE Sys_User_Role WHERE UserID=@USERID ";
            }
            else
            {
                sql = string.Format(@"
DELETE Sys_User_Role WHERE UserID=@USERID AND RoleID NOT IN({0})
INSERT INTO Sys_User_Role(UserID,RoleID)
SELECT @USERID,INDX FROM dbo.SplitIndx('{0}')T WHERE INDX NOT IN(SELECT RoleID FROM Sys_User_Role WHERE UserID=@USERID)
", viewroleid);
            }
            SqlCommand cmmd = new SqlCommand(sql);
            cmmd.Parameters.Add("@USERID", SqlDbType.Int, 4);
            cmmd.Parameters["@USERID"].Value = userid;
            db.ExecuteNonQuery(cmmd);
        }
        public static DataTable GetBaseTableInfo(string tableName, string roleid, DataBase db)
        {
            int roleid_int = 0;
            int.TryParse(roleid, out roleid_int);
            string sql = string.Format(@"
DECLARE @ids	VARCHAR(8000);
SELECT @ids=ids FROM [Sys_Role_View_BaseDataID] WHERE BaseTable='{0}' AND RoleID={1}
SELECT CASE WHEN dbo.SplitIndx.Id IS NULL THEN 0 ELSE 1 END AS Selected, M.ID,M.CODE,NAME FROM {0} M LEFT JOIN dbo.SplitIndx(@ids) ON M.ID = dbo.SplitIndx.Indx
", tableName, roleid_int);

            return db.ExecuteDataTable(sql);
        }
        public static DataTable GetRoleTable(DataBase db)
        {
            return db.ExecuteDataTable("SELECT 0 AS pid,ID,CODE,NAME FROM Sys_ROLE");
        }
        /// <summary>
        /// 获取用户对应的角色树
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public static DataTable GetUserRoleTree(string userid, DataBase db)
        {
            SqlCommand cmmd = new SqlCommand(@"
SELECT 0 AS pid,ID, CASE WHEN (SELECT 1 FROM Sys_User_Role WHERE UserID=@UserID  AND dbo.Sys_ROLE.ID = dbo.Sys_User_Role.RoleID) IS NULL THEN 0 ELSE 1 END AS CHECKED,
NAME FROM Sys_ROLE WHERE Activated=1");
            cmmd.Parameters.Add("@UserID", SqlDbType.Int, 4);
            cmmd.Parameters["@UserID"].Value = userid;
            return db.ExecuteDataTable(cmmd);
        }
        /// <summary>
        /// 保存可见基础数据
        /// </summary>
        /// <remarks>返回是否成功表信息</remarks>
        /// <param name="context"></param>
        public static DataTable SaveViewBaseData(string roleid, string tablename, string ids, DataBase db)
        {
            string sql = @"
DECLARE @ERR    NVARCHAR(1000);
--检查角色是否有效
IF NOT EXISTS(SELECT 1 FROM Sys_ROLE WHERE ID=@RoleID)
BEGIN
    SET @ERR='角色无效.';GOTO ERR;
END
IF EXISTS(SELECT 1 FROM Sys_Role_View_BaseDataID WHERE RoleID=@RoleID AND BaseTable=@BaseTable)
BEGIN
    UPDATE Sys_Role_View_BaseDataID SET IDS=@IDS WHERE RoleID=@RoleID AND BaseTable=@BaseTable
END
ELSE
BEGIN
    INSERT INTO [dbo].[Sys_Role_View_BaseDataID]
               ([RoleID]
               ,[BaseTable]
               ,[IDS])
         VALUES
               (@RoleID
               ,@BaseTable
               ,@IDS)
END
ERR:
SELECT @ERR
";
            SqlCommand cmmd = new SqlCommand(sql);
            cmmd.Parameters.Add("@RoleID", SqlDbType.SmallInt);
            cmmd.Parameters["@RoleID"].Value = roleid;
            cmmd.Parameters.Add("@BaseTable", SqlDbType.VarChar, 50);
            cmmd.Parameters["@BaseTable"].Value = tablename;
            cmmd.Parameters.Add("@IDS", SqlDbType.VarChar, 8000);
            cmmd.Parameters["@IDS"].Value = ids;
            return db.ExecuteDataTable(cmmd);
        }
        /// <summary>
        /// 保存可见基础数据
        /// </summary>
        /// <remarks>返回是否成功表信息</remarks>
        /// <param name="context"></param>
        public static DataTable GetMenuData(string roleid, DataBase db)
        {
            //            string sql = @"
            //SELECT ID,Name,Pid FROM(
            //SELECT ID,Name,-1 AS Pid,MenuOrder FROM dbo.Sys_SystemMenu WHERE Activated=1 --ORDER BY MenuOrder
            //UNION ALL
            //SELECT ID,Name,ParentID AS Pid,MenuOrder  FROM Sys_SystemMenu_Dtl WHERE Activated=1)t
            // ORDER BY Pid, MenuOrder
            //";
            //不用ID来做parentid，ID和子节点的ID有重复,用CODE来链接
            string sql = @"
SELECT T.* FROM 
(
	SELECT 'P'+CONVERT(VARCHAR,ID) AS IDFlag, Code AS ID, '' AS Pid,Name,	 MenuOrder
	FROM dbo.Sys_SystemMenu WHERE Activated=1  
	UNION
	SELECT 'D'+CONVERT(VARCHAR,A.ID) AS IDFlag,A.Code AS ID, B.Code AS Pid,A.Name,A.MenuOrder
	FROM dbo.Sys_SystemMenu_Dtl AS A
	INNER JOIN  dbo.Sys_SystemMenu AS B ON A.ParentID = B.ID
	WHERE A.Activated=1
) T
 ORDER BY T.Pid,T.MenuOrder";
            SqlCommand cmmd = new SqlCommand(sql);
            //cmmd.Parameters.Add("@RoleID", SqlDbType.SmallInt);
            //cmmd.Parameters["@RoleID"].Value = roleid;
            //cmmd.Parameters.Add("@BaseTable", SqlDbType.VarChar, 50);
            //cmmd.Parameters["@BaseTable"].Value = tablename;
            //cmmd.Parameters.Add("@IDS", SqlDbType.VarChar, 8000);
            //cmmd.Parameters["@IDS"].Value = ids;
            return db.ExecuteDataTable(cmmd);
        }
        /// <summary>
        /// 获取角色对应的菜单权限设置数据
        /// </summary>
        /// <param name="roleid"></param>
        /// <param name="menuid"></param>
        /// <returns>table0-function table1查询权限设置</returns>
        public static DataSet GetMenuSetting(string roleid, string menuid, DataBase db)
        {
            /*
             * 查询按钮权限，调整为新方式
             SELECT OPType,OPField,FunctionID,FixedFlag FROM dbo.Sys_Role_OP_Function INNER JOIN dbo.Sys_Module_OP ON MoudleOPID=ID WHERE RoleID=@RoleID 
    AND MoudleOPID IN(SELECT ID FROM Sys_Module_OP WHERE ModuleID=@ModuleID) AND FunctionID!=1  --查询单独设置,不加载查询 
             * 
             */

            string sql = @"
DECLARE @ModuleID	SMALLINT;
SELECT @ModuleID=ModuleID FROM Sys_SystemMenu_Dtl WHERE ID=@MenuID 
DECLARE @FunctionID	VARCHAR(8000);

SELECT @FunctionID =FunctionID FROM [dbo].[Sys_Module] WHERE ModuleID=@ModuleID;
SELECT ID,Name FROM dbo.Sys_Function WHERE ID IN(SELECT indx FROM dbo.SplitIndx(@FunctionID)) AND ID!=1 AND IsCheckPopedom=1
SELECT ID,OPField,OPFieldDesc,IsOP FROM dbo.Sys_Module_OP WHERE ModuleID=(SELECT ModuleID FROM [Sys_SystemMenu_Dtl] WHERE ID=@MenuID) ORDER BY SEQ_NO


--DECLARE @FunctionID	VARCHAR(8000);
--SELECT @FunctionID=FunctionID FROM dbo.Sys_Module WHERE ModuleID=@ModuleID;
SELECT ISNULL(OPType,0) AS OPType,OPField,ID AS FunctionID,ISNULL(FixedFlag,0) AS FixedFlag FROM(
	SELECT F.ID,OPField,Sys_Module_OP.ID AS MoudleOPID FROM dbo.Sys_Function F
	CROSS JOIN dbo.Sys_Module_OP 
	WHERE F.ID IN(SELECT Indx FROM dbo.SplitIndx(@FunctionID)) AND IsCheckPopedom=1 AND F.ID!=1 AND ModuleID=@ModuleID
)T LEFT JOIN dbo.Sys_Role_OP_Function OPF ON T.ID=OPF.FunctionID AND T.MoudleOPID=OPF.MoudleOPID
AND RoleID=@RoleID 

SELECT OPFieldDesc,M.OPField,OPType,CONVERT(BIT,CASE WHEN ViewID>'' THEN 1 ELSE 0 END) IsHaveFlag,M.IsOP FROM dbo.Sys_Module_OP M LEFT JOIN dbo.Sys_Role_View_UserID P ON M.ModuleID = P.ModuleID AND P.RoleID=@RoleID
WHERE M.ModuleID=@ModuleID 
";
            SqlCommand cmmd = new SqlCommand(sql);
            cmmd.Parameters.Add("@MenuID", SqlDbType.SmallInt, 2);
            cmmd.Parameters["@MenuID"].Value = menuid;
            cmmd.Parameters.Add("@RoleID", SqlDbType.SmallInt, 2);
            cmmd.Parameters["@RoleID"].Value = roleid;

            DataSet ds = db.ExecuteDataset(cmmd);

            DataTable dt_function = ds.Tables[0];    //模块对应的功能项
            DataTable dt_op = ds.Tables[1];    //模块操作OP
            DataTable dt_popedom = ds.Tables[2];    //权限设置
            DataTable dt_search = ds.Tables[3];     //查询设置
            dt_function.Columns.Add("FixedFlag", typeof(bool));

            foreach (DataRow oprow in dt_op.Rows)
            {
                dt_function.Columns.Add(oprow["OPField"].ToString(), typeof(string));
            }
            foreach (DataRow pr in dt_popedom.Rows)
            {
                DataRow[] rows = dt_function.Select(string.Format("ID={0}", pr["FunctionID"]));
                if (rows.Length == 1)
                {
                    DataRow row = rows[0];
                    row[pr["OPField"].ToString()] = pr["OPType"];
                    row["FixedFlag"] = pr["FixedFlag"];
                }
            }
            dt_function.TableName = "Function";
            dt_search.TableName = "Search";
            ds.Tables.RemoveAt(1);
            ds.Tables.RemoveAt(1);

            return ds;
        }
        /// <summary>
        /// 根据菜单ID，获取对应的操作项
        /// </summary>
        /// <param name="menuid"></param>
        /// <returns></returns>
        public static DataTable GetMenuOpFields(string menuid, DataBase db)
        {
            string sql = @"SELECT OPField,OPFieldDesc,IsOP FROM dbo.Sys_Module_OP WHERE ModuleID=(SELECT ModuleID FROM [Sys_SystemMenu_Dtl] WHERE ID=@ID) ORDER BY SEQ_NO";
            SqlCommand cmmd = new SqlCommand(sql);
            cmmd.Parameters.Add("@ID", SqlDbType.Int, 4);
            cmmd.Parameters["@ID"].Value = menuid;
            return db.ExecuteDataTable(cmmd);
        }
        /// <summary>
        /// 保存功能项权限设置
        /// </summary>
        /// <param name="menuID"></param>
        /// <param name="roleID"></param>
        /// <param name="configs"></param>
        public static void SaveFunctionSetting(string menuID, string roleID, string configs, DataBase db)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(@"
DECLARE @ModuleID	SMALLINT;
SELECT @ModuleID=ModuleID FROM Sys_SystemMenu_Dtl WHERE ID=@MenuID;
--删除之前所有配置项
BEGIN TRAN;
DELETE Sys_Role_OP_Function WHERE RoleID=@RoleID AND MoudleOPID IN(SELECT ID FROM Sys_Module_OP WHERE ModuleID=@ModuleID);
");

            foreach (string config in configs.Split(';'))
            {
                string[] vs = config.Split(',');
                if (vs.Length != 4)
                    break;
                string functionid = vs[0];
                string opfield = vs[1];
                string optye = vs[2];
                string fixedFlag = vs[3];
                if (optye.Equals("null")) optye = "0";

                if (optye.Equals("0") && fixedFlag == "0")
                    continue;
                sb.AppendFormat(@"
INSERT INTO Sys_Role_OP_Function(RoleID,MoudleOPID, FunctionID,OPType,FixedFlag)
SELECT @RoleID,(SELECT ID FROM dbo.Sys_Module_OP WHERE ModuleID=@ModuleID AND OPField='{0}'),{1},{2},{3}
", opfield, functionid, optye, fixedFlag);
            }
            //            sb.Append(@"
            //DECLARE @PAGEID SMALLINT;
            //SELECT @PAGEID=PAGEID FROM Sys_Search_Page WHERE  ModuleID=@ModuleID;
            //IF @PAGEID>0
            //BEGIN
            //    EXEC sp_Sys_UpdateRole_Popedom @RoleID,@PAGEID;
            //END
            //");
            sb.Append("COMMIT TRAN;");
            SqlCommand cmmd = new SqlCommand(sb.ToString());
            cmmd.Parameters.Add("@RoleID", SqlDbType.SmallInt);
            cmmd.Parameters["@RoleID"].Value = roleID;
            cmmd.Parameters.Add("@MenuID", SqlDbType.SmallInt);
            cmmd.Parameters["@MenuID"].Value = menuID;
            db.ExecuteNonQuery(cmmd);
        }
        /// <summary>
        /// 保存查询功能项权限设置
        /// </summary>
        /// <param name="menuID"></param>
        /// <param name="roleID"></param>
        /// <param name="configs"></param>
        public static void SaveSearchFunctionSetting(string menuID, string roleID, string configs, DataBase db)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(@"
DECLARE @ModuleID	SMALLINT;
SELECT @ModuleID=ModuleID FROM Sys_SystemMenu_Dtl WHERE ID=@MenuID;
--删除之前所有配置项
BEGIN TRAN;
DELETE Sys_Role_OP_Function WHERE RoleID=@RoleID AND MoudleOPID IN(SELECT ID FROM Sys_Module_OP WHERE ModuleID=@ModuleID);
");

            foreach (string config in configs.Split(';'))
            {
                string[] vs = config.Split(',');
                if (vs.Length != 2)
                    break;
                string opfield = vs[0];
                string optye = vs[1];
                if (optye.Equals("null"))
                {
                    optye = "0";
                }
                sb.AppendFormat(@"
IF EXISTS(SELECT 1 FROM Sys_Role_View_UserID WHERE RoleID=@RoleID AND ModuleID=@ModuleID)
BEGIN
	UPDATE dbo.Sys_Role_View_UserID SET OPType='{1}' WHERE RoleID=@RoleID AND ModuleID=@ModuleID
END
ELSE
BEGIN
	INSERT INTO dbo.Sys_Role_View_UserID
	        ( RoleID ,
	          ModuleID ,
	          OPField ,
	          ViewID ,
	          OPType
	        )
	VALUES  ( @RoleID , 
	          @ModuleID , 
	          '{0}' ,
	          '' ,
	          '{1}'
	        )
END
", opfield, optye);
            }
            sb.Append(@"
DECLARE @PAGEID SMALLINT;
SELECT @PAGEID=PAGEID FROM Sys_Search_Page WHERE  ModuleID=@ModuleID;
IF @PAGEID>0
BEGIN
    EXEC sp_Sys_UpdateRole_Popedom @RoleID,@PAGEID;
END
");
            sb.Append("COMMIT TRAN;");
            SqlCommand cmmd = new SqlCommand(sb.ToString());
            cmmd.Parameters.Add("@RoleID", SqlDbType.SmallInt);
            cmmd.Parameters["@RoleID"].Value = roleID;
            cmmd.Parameters.Add("@MenuID", SqlDbType.SmallInt);
            cmmd.Parameters["@MenuID"].Value = menuID;
            db.ExecuteNonQuery(cmmd);
        }

        /// <summary>
        ///保存可见菜单
        /// </summary>
        /// <param name="menuID"></param>
        /// <param name="roleID"></param>
        /// <param name="configs"></param>
        public static void SaveViewMenu(string roleID, string viewmenuid, DataBase db)
        {
            SqlCommand cmmd = new SqlCommand(@"UPDATE dbo.Sys_ROLE SET ViewMenuID=@ViewMenuID WHERE ID=@RoleID;");
            cmmd.Parameters.Add("@RoleID", SqlDbType.SmallInt);
            cmmd.Parameters["@RoleID"].Value = roleID;
            cmmd.Parameters.Add("@ViewMenuID", SqlDbType.VarChar, 8000);
            cmmd.Parameters["@ViewMenuID"].Value = viewmenuid;
            db.ExecuteNonQuery(cmmd);
        }
        /// <summary>
        /// 获取相关权限项
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="pageid"></param>
        /// <returns></returns>
        public static DataSet GetSearchFunctionPopedom(string userid, string pageid, DataBase db)
        {
            SqlCommand cmmd = new SqlCommand(@"
--DECLARE @PageID	SMALLINT;
--DECLARE @UserID	INT;
--SET @UserID=1
--SET @PageID=1;
DECLARE @ModuleID	SMALLINT;
DECLARE @SearchButtons	VARCHAR(8000);
DECLARE @EditButtons	VARCHAR(8000);
DECLARE @FunctionID		VARCHAR(8000);
DECLARE @USERROLE TABLE(RoleID	SMALLINT);
--读取用户对应的角色信息
INSERT INTO @USERROLE
	    ( RoleID )
SELECT RoleID FROM dbo.Sys_User_Role WHERE UserID=@UserID;
    
SELECT @ModuleID=dbo.Sys_Module.ModuleID,@SearchButtons=SearchButtons,@EditButtons=EditButtons,@FunctionID=FunctionID FROM dbo.Sys_Search_Page
INNER JOIN dbo.Sys_Module ON dbo.Sys_Search_Page.ModuleID = dbo.Sys_Module.ModuleID
WHERE PageID=@PageID;
--可见的功能权限
IF dbo.sys_IsAdmin(@UserID)=1
BEGIN
	SELECT Indx AS ID FROM dbo.SplitIndx(@FunctionID)
END
ELSE
BEGIN
	SELECT DISTINCT F.ID FROM dbo.Sys_Function F INNER JOIN dbo.Sys_Role_OP_Function ON FunctionID=F.ID
	INNER JOIN dbo.Sys_Module_OP ON dbo.Sys_Module_OP.ID=MoudleOPID WHERE ModuleID=@ModuleID AND RoleID IN(SELECT RoleID FROM @USERROLE) AND (OPType>0  OR FixedFlag=1)
	UNION
	SELECT DISTINCT 1 AS ID FROM dbo.Sys_Role_View_UserID WHERE ModuleID=@ModuleID AND RoleID IN(SELECT RoleID FROM @USERROLE) AND (OPType>0 OR ViewID>'')  --查询权限
END
SELECT @SearchButtons AS SearchButton,@EditButtons AS EditButton
");
            cmmd.Parameters.Add("@UserID", SqlDbType.Int);
            cmmd.Parameters["@UserID"].Value = userid;
            cmmd.Parameters.Add("@PageID", SqlDbType.SmallInt);
            cmmd.Parameters["@PageID"].Value = pageid;
            return db.ExecuteDataset(cmmd);

        }
        /// <summary>
        /// 获取相关权限项
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="pageid"></param>
        /// <returns></returns>
        public static DataSet GetViewPopedomByPageID(string userid, string pageid, DataBase db)
        {
            SqlCommand cmmd = new SqlCommand(@"
--DECLARE @PageID	SMALLINT;
--DECLARE @UserID	INT;
--SET @UserID=1
--SET @PageID=1;
DECLARE @ModuleID	SMALLINT;
DECLARE @SearchButtons	VARCHAR(8000);
DECLARE @EditButtons	VARCHAR(8000);
DECLARE @FunctionID		VARCHAR(8000);
DECLARE @USERROLE TABLE(RoleID	SMALLINT);
SELECT @ModuleID=ModuleID FROM dbo.Sys_Search_Page WHERE PageID=@PageID
--读取用户对应的角色信息
INSERT INTO @USERROLE
	    ( RoleID )
SELECT RoleID FROM dbo.Sys_User_Role WHERE UserID=@UserID;
--可见的功能权限
IF dbo.sys_IsAdmin(@UserID)=1
BEGIN
	SELECT @FunctionID=FunctionID FROM dbo.Sys_Module WHERE ModuleID=@ModuleID;
	SELECT F.ID,F.Code,F.Name,F.ICO,OPField,IsOP,CASE IsOP WHEN 1 THEN 5 ELSE 3 END AS OPType,CONVERT(BIT,0) AS FixedFlag  FROM dbo.Sys_Function F INNER JOIN DBO.SplitIndx(@FunctionID) T ON F.ID=T.Indx
		CROSS JOIN (SELECT * FROM Sys_Module_OP MP WHERE MP.ModuleID=@ModuleID)MP
END
ELSE
BEGIN
	SELECT F.ID,F.Code,F.Name,F.ICO,OPField,IsOP,MAX(OPType)AS OPType,FixedFlag AS FixedFlag FROM dbo.Sys_Role_OP_Function INNER JOIN dbo.Sys_Function F ON FunctionID=F.ID
	INNER JOIN dbo.Sys_Module_OP ON dbo.Sys_Module_OP.ID=MoudleOPID WHERE ModuleID=@ModuleID AND RoleID IN(SELECT RoleID FROM @USERROLE) AND (OPType>0 OR FixedFlag=1)
	GROUP BY F.ID,F.Code,F.Name,F.ICO,OPField,IsOP,FixedFlag	
END

SELECT ViewID FROM dbo.Sys_Role_View_UserID M INNER JOIN @USERROLE U ON M.RoleID = U.RoleID WHERE ModuleID=@ModuleID AND ViewID>''
");
            cmmd.Parameters.Add("@UserID", SqlDbType.Int);
            cmmd.Parameters["@UserID"].Value = userid;
            cmmd.Parameters.Add("@PageID", SqlDbType.SmallInt);
            cmmd.Parameters["@PageID"].Value = pageid;
            return db.ExecuteDataset(cmmd);

        }
        /// <summary>
        /// 获取相关权限项
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="pageid"></param>
        /// <returns></returns>
        public static DataTable GetUserList(string pageid, string roleID, string userid, DataBase db)
        {
            SqlCommand cmmd = new SqlCommand(@"
DECLARE @ISHAVE BIT;
EXEC [sp_Sys_CheckIsHavePopedom] @PageID,@UserID,@RoleID,@ISHAVE OUT
IF @ISHAVE=0
BEGIN
    SET @RoleID=0;
END
SELECT M.ID,M.CODE,M.NAME,p.Name AS DepartmentName,C.NAME AS CompanyName FROM dbo.Sys_D_USER M
	INNER JOIN dbo.Sys_D_Department P ON m.DepartmentID=P.ID
	INNER JOIN dbo.Sys_D_Company C ON c.ID=m.CompanyID
	INNER JOIN dbo.Sys_User_Role R ON M.ID = R.UserID AND RoleID=@RoleID

");
            cmmd.Parameters.Add("@PageID", SqlDbType.Int);
            cmmd.Parameters["@PageID"].Value = pageid;

            cmmd.Parameters.Add("@UserID", SqlDbType.Int);
            cmmd.Parameters["@UserID"].Value = userid;
            cmmd.Parameters.Add("@RoleID", SqlDbType.Int);
            cmmd.Parameters["@RoleID"].Value = roleID;
            return db.ExecuteDataTable(cmmd);

        }
        /// <summary>
        /// 获取所有用户树表
        /// </summary>
        /// <returns></returns>
        public static DataTable GetUserTreeData(string roleid, DataBase db)
        {
            SqlCommand cmmd = new SqlCommand(@"

SELECT  * FROM(
SELECT CONVERT(VARCHAR(10),ISNULL(PARENT_ID,'0')) AS PID,CONVERT(VARCHAR(10),ID) AS ID,CODE+'-'+NAME AS NAME FROM dbo.Sys_D_Company WHERE Activated=1
UNION
SELECT CONVERT(VARCHAR(10), CompanyID )AS PID,CONVERT(VARCHAR(10), ID)+'-'+CONVERT(VARCHAR(10),CompanyID), CODE+'-'+Name AS NAME FROM dbo.Sys_D_Department  WHERE Activated=1
UNION
SELECT CONVERT(VARCHAR(10), DepartmentID)+'-'+CONVERT(VARCHAR(10),CompanyID) AS PID,'U'+CONVERT(VARCHAR(10),ID),CODE+'-'+NAME AS NAME FROM dbo.Sys_D_USER WHERE Activated=1
    AND ID NOT IN(SELECT USERID FROM Sys_User_Role WHERE RoleID=@RoleID)
)T ORDER BY PID");
            if (string.IsNullOrEmpty(roleid)) roleid = "0";
            cmmd.Parameters.Add("@RoleID", SqlDbType.SmallInt);
            cmmd.Parameters["@RoleID"].Value = roleid;
            DataTable dt = db.ExecuteDataTable(cmmd);
            //return dt;
            DataTable dtnew = dt.Clone();
            DataRow[] companyrows = dt.Select("PID='0'");
            ManagerTalbe(dt, dtnew, companyrows);
            //for (int i = 0; i < companyrows.Length; i++)
            //{
            //    //处理公司
            //    dtnew.Rows.Add(companyrows[i].ItemArray);
            //    //查找公司下的部门
            //    DataRow[] departrows = dt.Select(string.Format("PID='{0}'", companyrows[i]["ID"]));
            //    for (int d = 0; d < departrows.Length; d++)
            //    {
            //        dtnew.Rows.Add(departrows[d].ItemArray);
            //        //添加人员
            //        DataRow[] userrows = dt.Select(string.Format("PID='{0}'",departrows[d]["ID"]));
            //        for (int u = 0; u < userrows.Length; u++)
            //        {
            //            dtnew.Rows.Add(userrows[u].ItemArray);
            //        }
            //    }
            //}
            return dtnew;
        }
        private static void ManagerTalbe(DataTable dtold, DataTable dtnew, DataRow[] companyrows)
        {
            for (int i = 0; i < companyrows.Length; i++)
            {
                dtnew.Rows.Add(companyrows[i].ItemArray);
                //检查下一节点
                DataRow[] departrows = dtold.Select(string.Format("PID='{0}'", companyrows[i]["ID"]));
                if (departrows.Length > 0)
                {
                    ManagerTalbe(dtold, dtnew, departrows);
                }

            }
        }
        /// <summary>
        /// 从角色中移除用户
        /// </summary>
        /// <param name="roleid"></param>
        /// <param name="userids"></param>
        public static void RemoveUser(string roleid, string userids, DataBase db)
        {
            SqlCommand cmmd = new SqlCommand(string.Format(@"
DELETE [dbo].[Sys_User_Role] WHERE RoleID=@RoleID AND UserID IN({0})
", userids));
            cmmd.Parameters.Add("@RoleID", SqlDbType.SmallInt);
            cmmd.Parameters["@RoleID"].Value = roleid;
            db.ExecuteNonQuery(cmmd);
        }
        /// <summary>
        /// 添加角色下的用户
        /// </summary>
        /// <param name="roleid"></param>
        /// <param name="userids"></param>
        public static void AddUser(string roleid, string userids, DataBase db)
        {
            SqlCommand cmmd = new SqlCommand(string.Format(@"
INSERT INTO [dbo].[Sys_User_Role](RoleID,UserID) 
SELECT @RoleID,INDX FROM dbo.SplitIndx('{0}')
", userids));
            cmmd.Parameters.Add("@RoleID", SqlDbType.SmallInt);
            cmmd.Parameters["@RoleID"].Value = roleid;
            db.ExecuteNonQuery(cmmd);
        }
        /// <summary>
        /// 设置角色中某操作可以查看的用户
        /// </summary>
        /// <param name="roleid"></param>
        /// <param name="userids"></param>
        public static DataTable GetOPUserList(string roleid, string opfield, string menuid, DataBase db)
        {
            SqlCommand cmmd = new SqlCommand(@"
DECLARE @USERIDS    VARCHAR(8000);
SELECT @USERIDS=ViewID FROM Sys_Role_View_UserID WHERE RoleID=@RoleID AND ModuleID=(SELECT ModuleID FROM dbo.Sys_SystemMenu_Dtl WHERE ID=@MenuID)
AND OPField=@OPField;
SELECT M.ID,M.CODE,M.NAME,p.Name AS DepartmentName,C.NAME AS CompanyName FROM dbo.Sys_D_USER M
	INNER JOIN dbo.Sys_D_Department P ON m.DepartmentID=P.ID
	INNER JOIN dbo.Sys_D_Company C ON c.ID=m.CompanyID
	INNER JOIN dbo.SplitIndx(@USERIDS) T ON M.ID=T.Indx
");
            cmmd.Parameters.Add("@RoleID", SqlDbType.SmallInt);
            cmmd.Parameters["@RoleID"].Value = roleid;
            cmmd.Parameters.Add("@OPField", SqlDbType.VarChar, 50);
            cmmd.Parameters["@OPField"].Value = opfield;
            cmmd.Parameters.Add("@MenuID", SqlDbType.Int);
            cmmd.Parameters["@MenuID"].Value = menuid;
            return db.ExecuteDataTable(cmmd);
        }
        /// <summary>
        /// 添加角色下的用户
        /// </summary>
        /// <param name="roleid"></param>
        /// <param name="userids"></param>
        public static void AddOPViewUser(string roleid, string opfield, string menuid, string userids, DataBase db)
        {
            SqlCommand cmmd = new SqlCommand(string.Format(@"
DECLARE @ModuleID	INT;
SELECT @ModuleID=ModuleID FROM dbo.Sys_SystemMenu_Dtl WHERE id=@MenuID;
SET @ModuleID=ISNULL(@ModuleID,0);
IF @ModuleID=0
BEGIN
	RAISERROR('请指定所属模块',16,1);RETURN;
END

DECLARE @USERIDS    VARCHAR(8000);
DECLARE @USERIDS_New    VARCHAR(8000);
SELECT @USERIDS=ViewID FROM Sys_Role_View_UserID WHERE RoleID=@RoleID AND ModuleID=@ModuleID
AND OPField=@OPField;
SET @USERIDS=ISNULL(@USERIDS,'')+','+CONVERT(VARCHAR(10),@AddUSERIDS);
SET @USERIDS_New='';
SELECT @USERIDS_New=@USERIDS_New+CONVERT(VARCHAR(10),INDX)+',' FROM (SELECT DISTINCT INDX FROM dbo.SplitIndx(@USERIDS) )T
IF LEN(@USERIDS_New)>0
BEGIN
    SET @USERIDS_New= SUBSTRING(@USERIDS_New,1,LEN(@USERIDS_New)-1);
END
UPDATE Sys_Role_View_UserID SET ViewID=@USERIDS_New  WHERE RoleID=@RoleID AND ModuleID=@ModuleID
AND OPField=@OPField;
", userids));
            cmmd.Parameters.Add("@RoleID", SqlDbType.SmallInt);
            cmmd.Parameters["@RoleID"].Value = roleid;
            cmmd.Parameters.Add("@OPField", SqlDbType.VarChar, 50);
            cmmd.Parameters["@OPField"].Value = opfield;
            cmmd.Parameters.Add("@MenuID", SqlDbType.Int);
            cmmd.Parameters["@MenuID"].Value = menuid;
            cmmd.Parameters.Add("@AddUserIDS", SqlDbType.VarChar, 8000);
            cmmd.Parameters["@AddUserIDS"].Value = userids;
            db.ExecuteNonQuery(cmmd);
        }
        /// <summary>
        /// 设置角色中某操作可以查看的用户
        /// </summary>
        /// <param name="roleid"></param>
        /// <param name="userids"></param>
        public static DataTable RemoveViewUserID(string roleid, string opfield, string menuid, string userid, DataBase db)
        {
            SqlCommand cmmd = new SqlCommand(@"
DECLARE @USERIDS    VARCHAR(8000);
DECLARE @USERIDS_New    VARCHAR(8000);
SELECT @USERIDS=ViewID FROM Sys_Role_View_UserID WHERE RoleID=@RoleID AND ModuleID=(SELECT ModuleID FROM dbo.Sys_SystemMenu_Dtl WHERE ID=@MenuID)
AND OPField=@OPField;
SET @USERIDS_New='';
SELECT @USERIDS_New=@USERIDS_New+CONVERT(VARCHAR(10),INDX)+',' FROM dbo.SplitIndx(@USERIDS) WHERE INDX!=@UserID;
IF LEN(@USERIDS_New)>0
BEGIN
    SET @USERIDS_New= SUBSTRING(@USERIDS_New,1,LEN(@USERIDS_New)-1);
END
UPDATE Sys_Role_View_UserID SET ViewID=@USERIDS_New  WHERE RoleID=@RoleID AND ModuleID=(SELECT ModuleID FROM dbo.Sys_SystemMenu_Dtl WHERE ID=@MenuID)
AND OPField=@OPField;
");
            cmmd.Parameters.Add("@RoleID", SqlDbType.SmallInt);
            cmmd.Parameters["@RoleID"].Value = roleid;
            cmmd.Parameters.Add("@OPField", SqlDbType.VarChar, 50);
            cmmd.Parameters["@OPField"].Value = opfield;
            cmmd.Parameters.Add("@MenuID", SqlDbType.Int);
            cmmd.Parameters["@MenuID"].Value = menuid;
            cmmd.Parameters.Add("@UserID", SqlDbType.Int);
            cmmd.Parameters["@UserID"].Value = userid;
            db.ExecuteNonQuery(cmmd);
            return GetOPUserList(roleid, opfield, menuid, db);
        }


        public static DataTable GetBaseDataMenuData(string userID, DataBase db)
        {
            string sql = @"EXEC sp_Sys_GetExcleTemplateTable";
            SqlCommand cmmd = new SqlCommand(sql);
            return db.ExecuteDataTable(cmmd);
        }

    }
}
