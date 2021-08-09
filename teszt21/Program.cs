using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using log4net;
using log4net.Config;
using System.Net;
using System.Collections.Generic;
using static Zabbix_TCP_Application.Utility;
using System.Net.Sockets;
using System.IO;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;
using System.Net.Configuration;

[assembly: XmlConfigurator(Watch = true)]

namespace Zabbix_TCP_Application
{
    class Program
    {

        #region konstansok
        //public static string HOSTNAME = Properties.Settings.Default.HOSTNAME;
        //public static string ZABBIX_NAME = Properties.Settings.Default.ZABBIX_NAME;
        //public static int ZABBIX_PORT = Properties.Settings.Default.ZABBIX_PORT;
        //public static int CONNECT_DELAY = Properties.Settings.Default.CONNECT_DELAY;
        //public static int BUFFER_SIZE = Properties.Settings.Default.BUFFER_SIZE;
        public static string PROXY_NAME = Properties.Settings.Default.PROXY_NAME;
        public static string PROXY_VERSION = Properties.Settings.Default.PROXY_VERSION;
        public static Encoding ENCODING = Encoding.ASCII;
        #endregion konstansok

        private static readonly ILog Log = LogManager.GetLogger("Log");

        
        static void Main(string[] args)
        {
            //AgentCommunication.ZabbixAgentUtility.ZabbixAgent();

            string jsonData = String.Format(@"{{""request"": ""proxy config"", ""host"": ""{0}"", ""version"": ""{1}""}}", PROXY_NAME, PROXY_VERSION);
            string responseData = ConnectJson(jsonData);
            ProxyCommunication.ResponseJsonObject jsonObject = JsonConvert.DeserializeObject<ProxyCommunication.ResponseJsonObject>(responseData);

            Dictionary<int, List<string>> dictItem = new Dictionary<int, List<string>>();
            dictItem =MakeHostIdKeyDict(dictItem, jsonObject);

            ProcessingAndRequest(jsonObject);

            Console.ReadLine();

        }
    }
}
