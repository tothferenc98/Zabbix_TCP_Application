using System;
using System.Text;
using Newtonsoft.Json;
using log4net;
using log4net.Config;
using System.Diagnostics;

[assembly: XmlConfigurator(Watch = true)]

namespace Zabbix_TCP_Application
{
    class Program
    {
        public static Version version = new Version(0,14,1); 
        #region konstansok
        public static string ZABBIX_NAME = Properties.Settings.Default.ZABBIX_NAME;
        public static int ZABBIX_PORT = Properties.Settings.Default.ZABBIX_PORT;
        public static string PROXY_NAME = Properties.Settings.Default.PROXY_NAME; //krones_w3proxy
        public static string PROXY_VERSION = Properties.Settings.Default.PROXY_VERSION;
        public static Encoding ENCODING = Encoding.ASCII;
        #endregion konstansok

        private static readonly ILog Log = LogManager.GetLogger("Log");

        static void Main(string[] args)
        {
            try
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                Log.InfoFormat("Start {0}", version);
                Log.Debug("Settings beállításai: PROXY_NAME: " + PROXY_NAME + ", ZABBIX_NAME: " + ZABBIX_NAME + ", ZABBIX_PORT: " + ZABBIX_PORT + ", PROXY_VERSION: " + PROXY_VERSION);
                string jsonData = String.Format(@"{{""request"": ""proxy config"", ""host"": ""{0}"", ""version"": ""{1}""}}", PROXY_NAME, PROXY_VERSION);
                string responseData = Utility.ConnectJson(jsonData);
                if (!responseData.Equals(String.Empty))
                {
                    ProxyCommunication.ResponseJsonObject jsonObject = JsonConvert.DeserializeObject<ProxyCommunication.ResponseJsonObject>(responseData);

                    Utility.ReplaceMacro(jsonObject);

                    Utility.ProcessingAndRequest(jsonObject, stopWatch);
                }
                else
                {
                    Log.Warn("A proxy config feldolgozása során hiba lépett fel!");
                }
                Log.InfoFormat("Stop {0}", Utility.StopWatch(stopWatch));
            }
            catch (Exception e)
            {
                Log.Error("Hiba a main függvényben: ", e);
            }
        }
    }
}
