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
        public string HostName { get; set; }
        public string Key { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ItemCheckData data &&
                   ItemId == data.ItemId &&
                   HostId == data.HostId &&
                   HostName == data.HostName &&
                   Key == data.Key;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("itemid: {0}, hostname: {1}, key: {2} ", ItemId, HostName, Key);
        }
    }
}
