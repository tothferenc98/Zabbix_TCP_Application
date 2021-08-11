using System;
using System.Text;
using Newtonsoft.Json;
using log4net;
using log4net.Config;
using static Zabbix_TCP_Application.Utility;
using System.Threading.Tasks;
using System.Net.Sockets;

[assembly: XmlConfigurator(Watch = true)]

namespace Zabbix_TCP_Application
{
    class Program
    {

        #region konstansok
        //public static string HOSTNAME = Properties.Settings.Default.HOSTNAME;
        public static string ZABBIX_NAME = Properties.Settings.Default.ZABBIX_NAME;
        public static int ZABBIX_PORT = Properties.Settings.Default.ZABBIX_PORT;
        public static int CONNECT_DELAY = Properties.Settings.Default.CONNECT_DELAY;
        public static int BUFFER_SIZE = Properties.Settings.Default.BUFFER_SIZE;
        public static string PROXY_NAME = Properties.Settings.Default.PROXY_NAME;
        public static string PROXY_VERSION = Properties.Settings.Default.PROXY_VERSION;
        public static Encoding ENCODING = Encoding.ASCII;
        #endregion konstansok

        private static readonly ILog Log = LogManager.GetLogger("Log");


        static async Task Main(string[] args)
        {
            //AgentCommunication.ZabbixAgentUtility.ZabbixAgent();

            try
            {
                Log.Debug("Start");
                Log.Debug("Settings beállításai: PROXY_NAME: " + PROXY_NAME + ", ZABBIX_NAME: " + ZABBIX_NAME + ", ZABBIX_PORT: " + ZABBIX_PORT + ", CONNECT_DELAY: " + CONNECT_DELAY + ", BUFFER_SIZE: " + BUFFER_SIZE + ", PROXY_VERSION: " + PROXY_VERSION);
                string jsonData = String.Format(@"{{""request"": ""proxy config"", ""host"": ""{0}"", ""version"": ""{1}""}}", PROXY_NAME, PROXY_VERSION);
                string responseData = ConnectJson(jsonData);
                ProxyCommunication.ResponseJsonObject jsonObject = JsonConvert.DeserializeObject<ProxyCommunication.ResponseJsonObject>(responseData);

                ReplaceMacro(jsonObject);

                await ProcessingAndRequest(jsonObject);

                Console.ReadLine();
            }
            catch (Exception e)
            {
                Log.Error("Hiba a main függvényben: ", e);
            }
            

        }
    }
}
