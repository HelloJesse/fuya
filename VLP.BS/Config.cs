using System;
using System.Collections.Generic;
using System.Text;

namespace VLP.Search
{
    /// <summary>
    /// 查询配置,此为简单配置，普通文本框可以不需要配置
    /// </summary>
    public class Config
    {
        /// <summary>
        /// 前台传回的Key
        /// </summary>
        public string Key
        {
            get;
            set;
        }
        /// <summary>
        /// 对应DBSQL
        /// </summary>
        public string DBSQL
        {
            get;
            set;
        }
        public string GetWhereSQL(string value)
        {
            string sql = string.Empty;
            string txt = value.ToString().Replace("'", "''");
            string[] linetxts = txt.Split(Environment.NewLine.ToCharArray());

            System.Collections.Specialized.StringCollection items = new System.Collections.Specialized.StringCollection();
            if (linetxts.Length > 1)
            {
                foreach (string k in linetxts)
                {
                    if (string.IsNullOrEmpty(k) == false && k.Trim().Length > 0)
                    {
                        items.Add(k.Trim());
                    }
                }
            }

            if (items.Count <= 1)
            {
                if (string.IsNullOrEmpty(this.DBSQL))
                {
                    sql = string.Format(" {0} {1} '{2}'", Key, txt.IndexOf('%') == -1 ? "=" : "LIKE", txt);
                }
                else
                {
                    if (txt.IndexOf('%') == -1)
                    {
                        sql = string.Format(DBSQL, txt);
                    }
                    else
                    {
                        string tempsql = this.DBSQL.Replace("='{0}'", " LIKE '{0}'").Replace("= '{0}'", " LIKE '{0}'");
                        sql = string.Format(tempsql, txt);
                    }

                }
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                int tempi = 0;
                if (string.IsNullOrEmpty(this.DBSQL))
                {
                    foreach (string k in items)
                    {
                        if (tempi == 0)
                        {
                            sb.AppendFormat(" {0}='{1}'", Key, k);
                        }
                        else
                        {
                            sb.AppendFormat(" OR {0}='{1}'", Key, k);
                        }
                        tempi++;
                    }
                    sql = string.Format(" ({0})", sb.ToString());
                }
                else
                {
                    string[] nos = new string[items.Count];
                    items.CopyTo(nos, 0);
                    string tempsql = this.DBSQL.Replace("='{0}'", "     IN '{0}'").Replace("= '{0}'", " IN '{0}'");
                    sql = string.Format(tempsql, string.Join("','", nos));
                }
            }
            return sql;
        }
        /// <summary>
        /// 根据Key，Value返回查询条件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GetWhereSQL(object value)
        {
            if (value == null || value.ToString().Trim().Length == 0)
            {
                return string.Empty;
            }
            return GetWhereSQL(value.ToString());
            
        }
        ///// <summary>
        ///// 根据Key，Value返回查询条件
        ///// </summary>
        ///// <param name="key"></param>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public string GetWhereSQL_OLD(object value)
        //{
        //    if (value == null || value.ToString().Trim().Length == 0)
        //    {
        //        return string.Empty;
        //    }

        //    string sql = string.Empty;
        //    string txt = value.ToString().Replace("'", "''");

        //    if (string.IsNullOrEmpty(this.DBSQL))
        //    {
        //        //假如多行,则按每行去精确匹配
        //        string[] linetxts = txt.Split(Environment.NewLine.ToCharArray());
        //        if (linetxts.Length > 1)
        //        {
        //            System.Collections.Specialized.StringCollection items = new System.Collections.Specialized.StringCollection();
        //            foreach (string k in linetxts)
        //            {
        //                if (string.IsNullOrEmpty(k) == false && k.Trim().Length > 0)
        //                {
        //                    items.Add(k.Trim());
        //                }
        //            }
        //            if (items.Count == 1)
        //            {
        //                sql = string.Format(" {0} {1} '{2}'", Key, txt.IndexOf('%') == -1 ? "=" : "LIKE", items[0]);
        //            }
        //            else
        //            {
        //                StringBuilder sb = new StringBuilder();
        //                int tempi = 0;
        //                foreach (string k in items)
        //                {
        //                    if (tempi == 0)
        //                    {
        //                        sb.AppendFormat(" {0}='{1}'", Key, k);
        //                    }
        //                    else
        //                    {
        //                        sb.AppendFormat(" OR {0}='{1}'", Key, k);
        //                    }
        //                    tempi++;
        //                }
        //                sql = string.Format(" ({0})",sb.ToString());
        //            }
        //        }
        //        else
        //        {
        //            sql = string.Format(" {0} {1} '{2}'", Key, txt.IndexOf('%') == -1 ? "=" : "LIKE", txt);
        //        }
        //    }
        //    else
        //    {
        //        if (txt.IndexOf('%') == -1)
        //        {

        //            sql = string.Format(this.DBSQL, txt);
        //        }
        //        else
        //        {
        //            string tempsql = this.DBSQL.Replace("='{0}'", " LIKE '{0}'").Replace("= '{0}'", " LIKE '{0}'");
        //            sql = string.Format(tempsql, txt);
        //        }
                
        //    }

        //    return sql;
        //}
    }
    public class ConfigCollection : System.Collections.ObjectModel.KeyedCollection<string, Config>
    {
        public static SearchConfig SearchConfigs = null;
        /// <summary>
        /// PageID
        /// </summary>
        public string PageID
        {
            get;
            set;
        }
        /// <summary>
        /// 默认条件
        /// </summary>
        public string DefaultFilter
        {
            get;
            set;
        }
        /// <summary>
        /// 默认排序SQL
        /// </summary>
        public string DefaultSortSQL
        {
            get;
            set;
        }
        protected override string GetKeyForItem(Config item)
        {
            return item.Key;
        }
    }

    public class SearchConfig : System.Collections.ObjectModel.KeyedCollection<string, ConfigCollection>
    {
        
        protected override string GetKeyForItem(ConfigCollection item)
        {
            return item.PageID;
        }
        /// <summary>
        /// 根据请求获取查询条件SQL
        /// </summary>
        /// <param name="pageid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetWhereSQL(string pageid, System.Web.HttpRequest request)
        {
            StringBuilder sb = new StringBuilder();
            ConfigCollection sc = null;
            VLP.BS.AutoCommandManager.AddSearchConfigToDB(pageid, request);
            if (ConfigCollection.SearchConfigs != null && ConfigCollection.SearchConfigs.Contains(pageid))
            {
                sc = ConfigCollection.SearchConfigs[pageid];
                foreach (Config c in sc)
                {
                    string v = request[c.Key];
                    if (string.IsNullOrEmpty(v)==false )
                    {
                        v = v.Trim();
                        if (v.Length > 0)
                        {
                            sb.AppendFormat(" AND {0}", c.GetWhereSQL(v));
                        }
                    }
                }
                //检查默认条件是否存在，若存在则添加
                if (string.IsNullOrEmpty(sc.DefaultFilter) == false)
                {
                    sb.AppendFormat(" AND {0}", sc.DefaultFilter);
                }
                return sb.ToString();
            }
            else
            {
                //系统自动添加

                
                throw new ApplicationException("未找到对应的配置.");
            }
        }
        /// <summary>
        /// 获取默认SQL
        /// </summary>
        /// <param name="pageid"></param>
        /// <returns></returns>
        public static string GetOrderBySQL(string pageid)
        {
            StringBuilder sb = new StringBuilder();
            ConfigCollection sc = null;
            if (ConfigCollection.SearchConfigs != null && ConfigCollection.SearchConfigs.Contains(pageid))
            {
                sc = ConfigCollection.SearchConfigs[pageid];
                //检查默认条件是否存在，若存在则添加
                if (string.IsNullOrEmpty(sc.DefaultSortSQL) == false)
                {
                    sb.AppendFormat(" ORDER BY {0}", sc.DefaultSortSQL);
                }
                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
