using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zabbix_TCP_Application.ProxyCommunication
{
    class ResponseJsonData
    {
        public List<string> fields { get; set; }
        public List<List<object>> data { get; set; }
    }
}
