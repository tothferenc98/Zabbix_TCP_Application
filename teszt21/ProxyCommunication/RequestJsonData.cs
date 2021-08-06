using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zabbix_TCP_Application.ProxyCommunication
{
    public class RequestJsonData
    {
        public int itemid { get; set; }
        public int clock { get; set; }
        public int ns { get; set; }
        public string value { get; set; }
        public int id { get; set; }
    }
}
