using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zabbix_TCP_Application.AgentCommunication
{
    public class RequestJsonObject
    {
        public string request = "agent data";
        public string session = "2dcf1bf2f6fc1c742812fbbf491e24f2";
        public List<RequestJsonData> data { get; set; }
        public int clock { get; set; }
        public int ns { get; set; }
    }
}
