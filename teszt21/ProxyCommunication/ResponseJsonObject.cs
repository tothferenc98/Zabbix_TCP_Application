using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zabbix_TCP_Application.ProxyCommunication
{
    class ResponseJsonObject
    {
        //public ResponseJsonData globalmacro { get; set; }
        public ResponseJsonData hosts { get; set; }
        public ResponseJsonData hostmacro { get; set; }
        [JsonProperty("interface")]
        public ResponseJsonData interface_ { get; set; }
        public ResponseJsonData items { get; set; }
    }
}
