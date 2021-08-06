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
            //NetTcpPort("net.tcp.port[www.beks.hu,443]");   //teszt

            //AgentCommunication.ZabbixAgentUtility.ZabbixAgent();
            string jsonData = String.Format(@"{{""request"": ""proxy config"", ""host"": ""{0}"", ""version"": ""{1}""}}", PROXY_NAME, PROXY_VERSION);
            string responseData = ConnectJson(jsonData);
            ProxyCommunication.ResponseJsonObject jsonObject = JsonConvert.DeserializeObject<ProxyCommunication.ResponseJsonObject>(responseData);

            Dictionary<int, List<string>> dictItem = new Dictionary<int, List<string>>();
            dictItem =MakeHostIdKeyDict(dictItem, jsonObject);

            List<ProxyCommunication.ItemCheckData> itemCheckDataObjectList = new List<ProxyCommunication.ItemCheckData>();

            /*
            foreach (var item in dictItem)
            {
                Console.WriteLine(item.Key);
                foreach (var item2 in item.Value)
                {
                    Console.WriteLine(item2);
                }
                Console.WriteLine();
            }
            */

            /* TESZT
            itemCheckDataObjectList.Add(new ProxyCommunication.ItemCheckData()
            {
                ItemId = 100000,
                HostId = 100000,
                Key = "net.tcp.port[www.beks.hu,443]",
                Value = ""
            });*/
            
            foreach (var item in jsonObject.items.data)
            {//TODO:hostid helyett host name a logoláshoz
                itemCheckDataObjectList.Add(new ProxyCommunication.ItemCheckData()
                {
                    ItemId = Convert.ToInt32(item[getPositionItemsItemId(jsonObject)]),
                    HostId = Convert.ToInt32(item[getPositionItemsHostid(jsonObject)]),
                    Key = item[getPositionItemsKey(jsonObject)],
                    Value = ""
                });
            }

            


            foreach (var item in itemCheckDataObjectList)
            {
                Console.WriteLine("itemid: {0}, hostid: {1}, key: {2}, value: {3}",item.ItemId,item.HostId,item.Key,item.Value );
                
            }

            string secondResponseData = String.Empty;
            ProxyCommunication.RequestJsonObject requestJsonObject = new ProxyCommunication.RequestJsonObject();
            //try
            //{
                if (itemCheckDataObjectList.Count > 0)
                {
                    int id = 1;
                    List<ProxyCommunication.RequestJsonData> listRequestJsonData = new List<ProxyCommunication.RequestJsonData>();
                    foreach (var item in itemCheckDataObjectList) 
                    {
                        
                        if (item.Key.Contains('['))
                        {
                            if (item.Key.Substring(0, item.Key.IndexOf('[')).Equals("web.page.get"))
                            {
                            string value = WebPageGet(item.Key);
                                if (!value.Equals(String.Empty))
                                {
                                    listRequestJsonData.Add(new ProxyCommunication.RequestJsonData()
                                    {
                                        itemid = item.ItemId,
                                        clock = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
                                        ns = 220525300,
                                        value = value,
                                        id = id
                                    });
                                    id++;
                                }
                            }
                        if (item.Key.Substring(0, item.Key.IndexOf('[')).Equals("net.tcp.port"))
                        {
                            string value = NetTcpPort(item.Key);
                            
                            listRequestJsonData.Add(new ProxyCommunication.RequestJsonData()
                            {
                                itemid = item.ItemId,
                                clock = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
                                ns = 220525300,
                                value = value,
                                id = id
                            });
                            id++;
                            
                        }
                    }
                    }
                    requestJsonObject.historydata = listRequestJsonData;
                    requestJsonObject.clock = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
                    requestJsonObject.ns = 220525300;
                    requestJsonObject.version = "3.4.13";

                    string serializedJson = JsonConvert.SerializeObject(requestJsonObject);
                //Console.WriteLine(serializedJson);
                    secondResponseData = ConnectJson(serializedJson);

                }
                else
                {
                    //Log.Error("MakeAgentDataMessage: Kaptunk választ, de a json data része üres ");
                    //return String.Empty;
                }
            //return secondResponseData;
            //}
            //catch (Exception e)
            //{
            //    Log.Error("MakeAgentDataMessage.Exception: ", e);
            //    return String.Empty;

            //}


            //string proxyteszt = String.Format(@"{{""request"":""proxy data"",""host"":""gyakornok_tf_proxy"",""session"":""7905fd85856fa1804dd9f27988d2e0b2"",""history data"":[{{""itemid"":""125720"",""value"":""proxy_teszt"",""id"":""1""}}],""clock"":""{0}"",""ns"":""220525300"",""version"":""3.4.13""}}", DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
            //string responseDataPROXY = Utility.ConnectJson(proxyteszt);



            /*
            WebClient client = new WebClient();
            string downloadString = client.DownloadString("https://www.beks.hu:443/");
            Console.WriteLine(downloadString);*/


            Console.ReadLine();

        }


        public static string WebPageGet(string key)
        {
            key=key.Substring(key.IndexOf('[')+1, key.IndexOf(']') - key.IndexOf('[')-1);
            var splitted = key.Split(',');
            string ip = splitted[0];
            string port = splitted[2];
            string dowloadstring = String.Format("{0}:{1}", ip, port);
            //Console.WriteLine(port);
            WebClient client = new WebClient(); 
            try
            {
                string downloadedString = client.DownloadString(dowloadstring);
                //Console.WriteLine(downloadedString);
                return downloadedString;
            }
            catch (Exception)
            {
                //Console.WriteLine("sikertelen");
                return "";
            }
        }

        public static string NetTcpPort(string key) 
        {
            key = key.Substring(key.IndexOf('[') + 1, key.IndexOf(']') - key.IndexOf('[') - 1);
            var splitted = key.Split(',');
            string ip = splitted[0];
            int port = Convert.ToInt32(splitted[1]);
            //Console.WriteLine(port);
            
            try
            {
                TcpClient client = new TcpClient(ip, port);
                client.Close();
                //Console.WriteLine("sikeres");
                return "1";
            }
            catch (Exception e)
            {
                //Console.WriteLine("sikertelen {0}",e);
                return "0";
            }
        }












    }
}
