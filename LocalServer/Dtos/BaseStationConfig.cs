using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LocalServer.Dtos
{
    public class BaseStationConfig
    {

        public int LocalServerPort { get; set; }
        public string ServerHost { get; set; }
        public string ServerIp { get; set; }

        public int CommandPort { get; set; }
        public string CommandPassword { get; set; }

        public int InfoPort { get; set; }
        public string InfoPassword { get; set; }
    }
}
