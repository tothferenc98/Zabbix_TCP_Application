using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zabbix_TCP_Application.AgentCommunication
{
    public class RequestJsonData
    {
        public string host = Properties.Settings.Default.HOSTNAME; // HOSTNAME;
        public string key { get; set; }
        public string value { get; set; }
        public int id { get; set; }
        public int clock { get; set; }
        public int ns { get; set; }

    }
}
