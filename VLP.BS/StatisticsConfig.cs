using System;
using System.Collections.Generic;
using System.Text;

namespace VLP.Statistics
{



    /// <summary>
    /// 统计图表配置类
    /// </summary>
    public class Config
    {

        public string Type
        {
            get;
            set;
        }
        public string Title
        {
            get;
            set;
        }
        public string SQL
        {
            get;
            set;
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configFile">配置文件</param>
        public Config(string configFile)
        {
            using (System.IO.StreamReader r = System.IO.File.OpenText(configFile))
            {
                string json = r.ReadToEnd();
                System.Collections.ArrayList array = (System.Collections.ArrayList)VLP.JSON.Decode(json);
                foreach (object o in array)
                {
                    System.Collections.Hashtable hs = (System.Collections.Hashtable)o;
                    string city = hs["SQL"].ToString();
                }
            }
        }
    }
}
