using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zabbix_TCP_Application.AgentCommunication
{
    public class ResponseJsonData
    {
        public string key { get; set; }
        public int delay { get; set; }
        public int lastlogsize { get; set; }
        public int mtime { get; set; }

    }
}
