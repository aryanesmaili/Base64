﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base64.Utility.Models
{
    public class VClientModel
    {
        public string id { get; set; }
        public string flow { get; set; }
        public string email { get; set; }
        public int limitIp { get; set; }
        public int totalGB { get; set; }
        public string expiryTime { get; set; }
    }
}
