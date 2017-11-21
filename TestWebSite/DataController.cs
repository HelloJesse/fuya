using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetSingleShipInfo.Data;
using GetSingleShipInfo.Models;

namespace TestWebSite
{
    [Route("data")]
    public class DataController : Controller
    {
        private TrackInfoRepository _trackInfoRepository;

        public DataController()
        {
            _trackInfoRepository = new TrackInfoRepository();
        }


        [Route("latest")]
        [HttpPost]
        public IActionResult Latest(double date)
        {
            var lngDt = Convert.ToInt32(date);
            return Json(_trackInfoRepository.GetLatestByDate(lngDt ));
        }

    }
}
