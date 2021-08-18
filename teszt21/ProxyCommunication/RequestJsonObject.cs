using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zabbix_TCP_Application.ProxyCommunication
{
    public class RequestJsonObject
    {
        public string request = "proxy data";
        public string host = Properties.Settings.Default.PROXY_NAME;
        public string session { get; set; } 
        [JsonProperty("history data")]
        public List<RequestJsonData> historydata { get; set; }
        public int clock { get; set; }
        public int ns { get; set; }
        public string version { get; set; }
        
}
}
