using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using log4net;
using log4net.Config;
using System.Collections.Generic;
using static Zabbix_TCP_Application.Utility;

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

            List<ProxyCommunication.ItemCheckData> itemCheckDataObjectList = new List<ProxyCommunication.ItemCheckData>();
            

            foreach (var item in dictItem)
            {
                Console.WriteLine(item.Key);
                foreach (var item2 in item.Value)
                {
                    Console.WriteLine(item2);
                }
                Console.WriteLine();
            }

            foreach (var item in jsonObject.items.data)
            {
                itemCheckDataObjectList.Add(new ProxyCommunication.ItemCheckData()
                {
                    ItemId = item[getPositionItemsHostid(jsonObject)],
                    HostId = item[getPositionItemsItemId(jsonObject)],
                    Key = item[getPositionItemsKey(jsonObject)],
                    Value = ""
                });
                //Console.WriteLine("itemid: {0}, hostid: {1}, key: {2}, value: {3}", item[getPositionItemsHostid(jsonObject)], item[getPositionItemsItemId(jsonObject)], item[getPositionItemsKey(jsonObject)], "");
            }

            foreach (var item in itemCheckDataObjectList)
            {
                //Console.WriteLine("itemid: {0}, hostid: {1}, key: {2}, value: {3}",item.ItemId,item.HostId,item.Key,item.Value );
            }




            /*string proxyteszt= String.Format(@"{{""request"":""proxy data"",""host"":""gyakornok_tf_proxy"",""session"":""7905fd85856fa1804dd9f27988d2e0b2"",""history data"":[{{""itemid"":""125720"",""value"":""proxy_teszt"",""id"":""1""}}],""clock"":""{0}"",""ns"":""220525300"",""version"":""3.4.13""}}", DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
            string responseDataPROXY = Utility.ConnectJson(proxyteszt);*/



            /*
            WebClient client = new WebClient();
            string downloadString = client.DownloadString("https://www.beks.hu:443/");
            Console.WriteLine(downloadString);*/


            Console.ReadLine();

        }



        

        

        
        



    }
}
