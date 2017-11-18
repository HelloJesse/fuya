using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace VLP.BS
{
    /// <summary>
    /// 自动命令
    /// </summary>
    public class AutoCommand
    {
        public string Key = string.Empty;
        public string PKName = string.Empty;
        public SqlCommand InsertCommand = null;
        public SqlCommand UpdateCommand = null;
        public SqlCommand InitCommand = null;
        public SqlCommand DeleteCommand = null;
    }
    public class AutoCommands : System.Collections.ObjectModel.Collection<AutoCommand>
    {
        public AutoCommand GetCommand(string key)
        {
            foreach (AutoCommand t in this.Items)
            {
                if (t.Key.Equals(key))
                    return t;
            }
            return null;
        }
    }

    public class Result
    {
        public string PKName = string.Empty;
        public string PKValue = string.Empty;
        public string NoValue = string.Empty;
        public bool IsOK = false;
        public string ErrMessage = string.Empty;
    }
    /// <summary>
    /// 返回数据的Result
    /// </summary>
    public class DataResult
    {
        public System.Data.DataTable Data = null;
        public bool IsOK = false;
        public string ErrMessage = string.Empty;
    }
    /// <summary>
    /// 返回数据的Result
    /// </summary>
    public class SearchPageResult
    {
        public System.Data.DataTable ButtonData = null;
        public GridManager ColumnsData = null;
        public System.Data.DataSet DropdownData = null;
        public string FieldID = string.Empty;
        public bool IsOK = false;
        public string ErrMessage = string.Empty;
    }
    public class LoadFormResult
    {
        /// <summary>
        /// 数据
        /// </summary>
        public System.Data.DataTable BillData = null;
        /// <summary>
        /// 明细数据
        /// </summary>
        public System.Data.DataTable DtlData = null;
        /// <summary>
        /// 其它数据
        /// </summary>
        public object OtherData = null;
        /// <summary>
        /// 按钮
        /// </summary>
        public System.Data.DataTable ButtonData = null;
        /// <summary>
        /// 其它
        /// </summary>
        public object Other = null;
        public bool IsOK = false;
        public string ErrMessage = string.Empty;
    }
}
