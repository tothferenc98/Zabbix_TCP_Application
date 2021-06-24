using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace teszt21
{
    class Program
    {
        static void Main(string[] args)
        {
            #region konstansok
            const string HOSTNAME = "gyakornok_tf_app";
            const string ZABBIX_NAME = "zabbix.beks.hu";
            const int ZABBIX_PORT  = 10051;
            #endregion konstansok

            // TODO: TCP kapcsolat meghatározása  (TASK LIST)
            // TODO: TcpClient példakód átmásolása https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient?view=net-5.0

            // ZABBIX request string megépítése (1624524243) epoch időkonverzió - string.Format!!
            // Végtelen ciklus 20s-ként megszólítani a zabbix szervert (hibakezelés)
            // válasz kiíratása konzolra
            // Rövid függvények


        }
    }
}
