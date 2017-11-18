using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GetSingleShipInfo.Models
{
    public class TrackInfo
    {
        public TrackInfo() { }

        public TrackInfo(Hashtable row)
        {
            ShipId = long.Parse(row["ShipID"].ToString());
            Latitude = int.Parse(row["lat"].ToString());
            Longitude = int.Parse(row["lon"].ToString());
            Lasttime = long.Parse(row["lasttime"].ToString());
            Speed = int.Parse(row["sog"].ToString());
            Heading = int.Parse(row["hdg"].ToString());
            Course = int.Parse(row["cog"].ToString());
        }

        public ObjectId Id { get; set; }

        public long ShipId { get; set; }

        public int Latitude { get; set; }

        public int Longitude { get; set; }

        /// <summary>
        /// 船首向
        /// </summary>
        public int Heading { get; set; }


        /// <summary>
        /// 航迹向
        /// </summary>
        public int Course { get; set; }

        /// <summary>
        /// 速度 毫米/秒
        /// </summary>
        public int Speed { get; set; }


        public long Lasttime { get; set; }
    }
}
