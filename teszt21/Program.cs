using System;
using System.Text;
using Newtonsoft.Json;
using log4net;
using log4net.Config;

[assembly: XmlConfigurator(Watch = true)]

namespace Zabbix_TCP_Application
{
    class Program
    {
        public static Version version = new Version(0,10,1); 
        #region konstansok
        public static string ZABBIX_NAME = Properties.Settings.Default.ZABBIX_NAME;
        public static int ZABBIX_PORT = Properties.Settings.Default.ZABBIX_PORT;
        public static int BUFFER_SIZE = Properties.Settings.Default.BUFFER_SIZE;
        public static string PROXY_NAME = Properties.Settings.Default.PROXY_NAME;
        public static string PROXY_VERSION = Properties.Settings.Default.PROXY_VERSION;
        public static Encoding ENCODING = Encoding.ASCII;
        #endregion konstansok

        private static readonly ILog Log = LogManager.GetLogger("Log");

        static void Main(string[] args)
        {
            try
            {
                Log.DebugFormat("Start {0}", version);
                Log.Debug("Settings beállításai: PROXY_NAME: " + PROXY_NAME + ", ZABBIX_NAME: " + ZABBIX_NAME + ", ZABBIX_PORT: " + ZABBIX_PORT + ", BUFFER_SIZE: " + BUFFER_SIZE + ", PROXY_VERSION: " + PROXY_VERSION);
                string jsonData = String.Format(@"{{""request"": ""proxy config"", ""host"": ""{0}"", ""version"": ""{1}""}}", PROXY_NAME, PROXY_VERSION);
                string responseData = Utility.ConnectJson(jsonData);
                if (!responseData.Equals(String.Empty))
                {
                    ProxyCommunication.ResponseJsonObject jsonObject = JsonConvert.DeserializeObject<ProxyCommunication.ResponseJsonObject>(responseData);

                    Utility.ReplaceMacro(jsonObject);

                    Utility.ProcessingAndRequest(jsonObject);
                }
                else
                {
                    Log.Warn("A proxy config feldolgozása során hiba lépett fel!");
                }

            }
            catch (Exception e)
            {
                Log.Error("Hiba a main függvényben: ", e);
            }
        }
    }
}
