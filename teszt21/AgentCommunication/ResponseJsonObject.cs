using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zabbix_TCP_Application.AgentCommunication
{
    public class ResponseJsonObject
    {
        public string response { get; set; }
        public List<ResponseJsonData> data { get; set; }

    }
}
