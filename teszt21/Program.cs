using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using log4net;
using log4net.Config;
using System.Collections.Generic;


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
            string responseData = Utility.ConnectJson(jsonData);
            //string teszt = "{\"globalmacro\":{\"fields\":[\"globalmacroid\",\"macro\",\"value\"],\"data\":[[2,\"{$SNMP_COMMUNITY}\",\"public\"]]}}";
            //string teszt = "{\"globalmacro\":{\"fields\":[\"globalmacroid\",\"macro\",\"value\"],\"data\":[[2,\"{$SNMP_COMMUNITY}\",\"public\"]]},\"hosts\":{\"fields\":[\"hostid\",\"host\",\"status\",\"ipmi_authtype\",\"ipmi_privilege\",\"ipmi_username\",\"ipmi_password\",\"name\",\"tls_connect\",\"tls_accept\",\"tls_issuer\",\"tls_subject\",\"tls_psk_identity\",\"tls_psk\"],\"data\":[[10407,\"Template App Zabbix Agent Active\",3,-1,2,\"\",\"\",\"Template App Zabbix Agent Active\",1,1,\"\",\"\",\"\",\"\"],[10408,\"Template OS Windows Active Agent\",3,-1,2,\"\",\"\",\"Template OS Windows Active Agent\",1,1,\"\",\"\",\"\",\"\"],[10624,\"beks_hw_192.168.193.245\",0,-1,2,\"\",\"\",\"beks_hw_192.168.193.245 (Nagy Andr??s DO)\",1,1,\"\",\"\",\"\",\"\"],[11445,\"beks_gapc_192.168.194.147\",0,-1,2,\"\",\"\",\"beks_gapc_192.168.194.147 (Varga Bandi asztal??n)\",1,1,\"\",\"\",\"\",\"\"],[11528,\"gyakornok_tf_pc\",0,-1,2,\"\",\"\",\"MON gyakornok_tf_pc\",1,1,\"\",\"\",\"\",\"\"],[11539,\"Welcome3 HW JSON monitoring Active\",3,-1,2,\"\",\"\",\"Welcome3 HW JSON monitoring Active\",1,1,\"\",\"\",\"\",\"\"]]}}";
            //Console.WriteLine(teszt);
            ProxyCommunication.ResponseJsonObject jsonObject = JsonConvert.DeserializeObject<ProxyCommunication.ResponseJsonObject>(responseData);

            int hostidPozicio = getPositionHostid(jsonObject);
            int keyPozicio = getPositionKey(jsonObject);
            Dictionary<int, List<string>> dictItem = new Dictionary<int, List<string>>();
            foreach (var item in jsonObject.items.data)
            {
                if (!dictItem.Keys.Contains(Convert.ToInt32(item[hostidPozicio])))
                {
                    List<string> listItem = new List<string>();
                    foreach (var item2 in jsonObject.items.data)
                    {
                        if (item[hostidPozicio].Equals(item2[hostidPozicio]))
                        {
                            if (Convert.ToString(item2[keyPozicio]).Contains("{$"))
                            {
                                string tempKey = Convert.ToString(item2[keyPozicio]);
                                tempKey = tempKey.Substring(tempKey.IndexOf("{"), (tempKey.IndexOf("}") + 1) - tempKey.IndexOf("{"));
                                string tempReplacedData = getReplaceData(Convert.ToInt32(item[hostidPozicio]), tempKey, jsonObject);
                                item2[keyPozicio] = Convert.ToString(item2[keyPozicio]).Replace(tempKey, tempReplacedData);
                            }

                            listItem.Add(Convert.ToString(item2[keyPozicio]));
                        }
                    }
                    dictItem.Add(Convert.ToInt32(item[hostidPozicio]), listItem);

                }
            }

            foreach (var item in dictItem)
            {
                Console.WriteLine(item.Key);
                foreach (var item2 in item.Value)
                {
                    Console.WriteLine(item2);
                }
                Console.WriteLine();
            }

            /*
            string proxyteszt= String.Format(@"{{""request"":""proxy data"",""host"":""gyakornok_tf_proxy"",""session"":""7905fd85856fa1804dd9f27988d2e0b2"",""history data"":[{{""itemid"":""125720"",""value"":""proxy_teszt"",""id"":""1""}}],""clock"":""{0}"",""ns"":""220525300"",""version"":""3.4.13""}}", DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
            string responseDataPROXY = Utility.ConnectJson(proxyteszt);*/


            
            /* 
            WebClient client = new WebClient();
            string downloadString = client.DownloadString("https://www.beks.hu/");
            Console.WriteLine(downloadString);*/


            Console.ReadLine();

        }

        public static string getReplaceData(int hostid, string macro, ProxyCommunication.ResponseJsonObject jsonObject)
        {
            foreach (var item in jsonObject.hostmacro.data)
            {
                if (hostid.Equals(Convert.ToInt32(item[1])) && macro.Equals(Convert.ToString(item[2])))
                {
                    return Convert.ToString(item[3]);
                }

            }
            return "";
        }

        public static int getPositionHostid(ProxyCommunication.ResponseJsonObject jsonObject)
        {
            int hostid_pos = 0;
            foreach (var item in jsonObject.items.fields)
            {
                if (item.Equals("hostid"))
                    return hostid_pos;
                else
                    hostid_pos++;
            }
            return -1;
        }
        public static int getPositionKey(ProxyCommunication.ResponseJsonObject jsonObject)
        {
            int key_pos = 0;
            foreach (var item in jsonObject.items.fields)
            {
                if (item.Equals("key_"))
                    return key_pos;
                else
                    key_pos++;
            }
            return -1;
        }
       
        

    }
}
