using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zabbix_TCP_Application.ProxyCommunication
{
    class ItemCheckData
    {
        public int ItemId { get; set; }
        public int HostId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ItemCheckData data &&
                   ItemId == data.ItemId &&
                   HostId == data.HostId &&
                   Key == data.Key &&
                   Value == data.Value;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
