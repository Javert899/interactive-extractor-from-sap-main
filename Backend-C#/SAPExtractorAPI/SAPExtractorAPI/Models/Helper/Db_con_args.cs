using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAPExtractorAPI.Models.Helper
{
    public class Db_con_args
    {
        public string hostname { get; set; }
        public string port { get; set; }
        public string sid { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }
}