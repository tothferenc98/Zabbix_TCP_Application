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
        public static string HOSTNAME = Properties.Settings.Default.HOSTNAME;
        public static string ZABBIX_NAME = Properties.Settings.Default.ZABBIX_NAME;
        public static int ZABBIX_PORT = Properties.Settings.Default.ZABBIX_PORT;
        public static int CONNECT_DELAY = Properties.Settings.Default.CONNECT_DELAY;
        public static int BUFFER_SIZE = Properties.Settings.Default.BUFFER_SIZE;
        public static Encoding ENCODING = Encoding.ASCII;
        #endregion konstansok

        private static readonly ILog Log = LogManager.GetLogger("Log");

        static void Main(string[] args)
        {
            //ZabbixAgent();

            string jsonData = String.Format(@"{{""request"": ""proxy config"", ""host"": ""{0}"", ""version"": ""3.4.13""}}", "gyakornok_tf_proxy");
            string responseData = Utility.ConnectJson(jsonData); 
            //string teszt = "{\"globalmacro\":{\"fields\":[\"globalmacroid\",\"macro\",\"value\"],\"data\":[[2,\"{$SNMP_COMMUNITY}\",\"public\"]]}}";
            //string teszt = "{\"globalmacro\":{\"fields\":[\"globalmacroid\",\"macro\",\"value\"],\"data\":[[2,\"{$SNMP_COMMUNITY}\",\"public\"]]},\"hosts\":{\"fields\":[\"hostid\",\"host\",\"status\",\"ipmi_authtype\",\"ipmi_privilege\",\"ipmi_username\",\"ipmi_password\",\"name\",\"tls_connect\",\"tls_accept\",\"tls_issuer\",\"tls_subject\",\"tls_psk_identity\",\"tls_psk\"],\"data\":[[10407,\"Template App Zabbix Agent Active\",3,-1,2,\"\",\"\",\"Template App Zabbix Agent Active\",1,1,\"\",\"\",\"\",\"\"],[10408,\"Template OS Windows Active Agent\",3,-1,2,\"\",\"\",\"Template OS Windows Active Agent\",1,1,\"\",\"\",\"\",\"\"],[10624,\"beks_hw_192.168.193.245\",0,-1,2,\"\",\"\",\"beks_hw_192.168.193.245 (Nagy Andr??s DO)\",1,1,\"\",\"\",\"\",\"\"],[11445,\"beks_gapc_192.168.194.147\",0,-1,2,\"\",\"\",\"beks_gapc_192.168.194.147 (Varga Bandi asztal??n)\",1,1,\"\",\"\",\"\",\"\"],[11528,\"gyakornok_tf_pc\",0,-1,2,\"\",\"\",\"MON gyakornok_tf_pc\",1,1,\"\",\"\",\"\",\"\"],[11539,\"Welcome3 HW JSON monitoring Active\",3,-1,2,\"\",\"\",\"Welcome3 HW JSON monitoring Active\",1,1,\"\",\"\",\"\",\"\"]]}}";
            //Console.WriteLine(teszt);
            ProxyCommunication.ResponseJsonObject jsonObject = JsonConvert.DeserializeObject<ProxyCommunication.ResponseJsonObject>(responseData);

            int hostidPozicio=getPositionHostid(jsonObject);
            int keyPozicio = getPositionKey(jsonObject);
            Dictionary<int, List<string>> dictItem = new Dictionary<int, List<string>>();
            foreach (var item in jsonObject.items.data)
            {
                if (!dictItem.Keys.Contains(Convert.ToInt32(item[hostidPozicio]))){
                    List<string> listItem = new List<string>();
                    foreach (var item2 in jsonObject.items.data)
                    {
                        if (item[hostidPozicio].Equals(item2[hostidPozicio]))
                        {
                            if (Convert.ToString(item2[keyPozicio]).Contains("{$"))
                            {
                                string temp = Convert.ToString(item2[keyPozicio]);
                                temp=temp.Substring(temp.IndexOf("{"), (temp.IndexOf("}")+1)- temp.IndexOf("{"));
                                string temp2 = getReplaceData(Convert.ToInt32(item[hostidPozicio]), temp, jsonObject);
                                item2[keyPozicio]=Convert.ToString(item2[keyPozicio]).Replace(temp, temp2);
                                


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



            //Console.WriteLine(valami);
            //byteArrayData2.Select(d => String.Format("{0:X}", d))



            Console.ReadLine();

        }

        public static string getReplaceData(int hostid, string macro, ProxyCommunication.ResponseJsonObject jsonObject)
        {
            //Console.WriteLine("hostid: "+ hostid+", key: " + original);
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
        /*
       class Item
       {
           public List<Items> listItem { get; set; }
       }
       class Items
       {
           public int hostid { get; set; }
           public List<string> keys { get; set; }
       }
       */
        











        public static void ZabbixAgent()
        {
            Log.Debug("Start");
            Log.Debug("Settings beállításai: HOSTNAME: " + HOSTNAME + ", ZABBIX_NAME: " + ZABBIX_NAME + ", ZABBIX_PORT: " + ZABBIX_PORT + ", CONNECT_DELAY: " + CONNECT_DELAY + ", BUFFER_SIZE: " + BUFFER_SIZE);
            do
            {
                while (!Console.KeyAvailable)
                {
                    try
                    {
                        string jsonData = String.Format(@"{{""request"":""active checks"",""host"":""{0}""}}", HOSTNAME);
                        
                        string responseData = Utility.ConnectJson(jsonData);

                        if (!responseData.Equals(String.Empty))
                        {
                            AgentCommunication.ResponseJsonObject jsonObject = JsonConvert.DeserializeObject<AgentCommunication.ResponseJsonObject>(responseData);
                            if (jsonObject.response != null && jsonObject.response.Equals("success"))
                            {
                                Utility.MakeAgentDataMessage(jsonObject);
                            }
                            else
                            {
                                Log.Error(((jsonObject.response == null) ? "Az active checket a szerver nem tudta feldolgozni. Null az értéke a válasznak" : "Nem success a válasz értéke, hanem " + jsonObject.response + ". " + responseData));
                            }
                        }
                        else
                        {
                            Log.Warn("Az active check feldolgozása során hiba lépett fel!");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Log.Error("Hiba a main függvényben: ", e);
                    }

                    System.Threading.Thread.Sleep(CONNECT_DELAY * 1000);

                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            Log.Debug("STOP");
        }

        

        public static ulong GetTotalMemoryInBytes()
        {
            return new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
        }

        public static ulong GetAvailableMemoryInBytes()
        {
            return new Microsoft.VisualBasic.Devices.ComputerInfo().AvailablePhysicalMemory;
        }

        public static string GetSystemUname()
        {
            return new Microsoft.VisualBasic.Devices.Computer().Name + " " + new Microsoft.VisualBasic.Devices.ComputerInfo().OSFullName;
        }

        public static int GetProcessNumber()
        {
            int i = 0;
            foreach (var item in System.Diagnostics.Process.GetProcesses())
            {

                i++;
            }
            return i;
        }

        public static int GetPerformanceCounter2_250() {
            var count = Process.GetProcesses().Sum(p => p.Threads.Count);
            return count;
        }

        public static string GetUpTime()
        {
            var atalakit = GetTickCount64()/1000;
            
            return Convert.ToString(atalakit);
        }

        [DllImport("kernel32")]
        extern static UInt64 GetTickCount64();
        
        public static string GetMyIP()
        {
            string hostName = Dns.GetHostName(); 
            string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
            return myIP;
        }

        public static string GetDiskReadsSec()
        {
            PerformanceCounter disksReadSec = new PerformanceCounter("PhysicalDisk", "Disk Reads/sec", "_Total");
            while (true)
            {
                var result = disksReadSec.NextValue();
                if (!result.Equals(0)){
                    var result2= Convert.ToString(result);
                    result2=result2.Replace(',', '.');
                    return result2;
                }
            }
            //Console.WriteLine("Category: {0}", disksReadSec.CategoryName);
            //Console.WriteLine("Help text: {0}", disksReadSec.CounterHelp);
            //Console.WriteLine("Avg. Disk Bytes/Read: {0}", disksReadSec.NextValue());
            
        }

        public static string GetDiskWritesSec()
        {
            PerformanceCounter diskWritesSec = new PerformanceCounter("PhysicalDisk", "Disk Writes/sec", "_Total");
            while (true)
            {
                var result = diskWritesSec.NextValue();
                if (!result.Equals(0))
                {
                    var result2 = Convert.ToString(result);
                    result2 = result2.Replace(',', '.');
                    return result2;
                }
            }
            //Console.WriteLine("Category: {0}", diskWritesSec.CategoryName);
            //Console.WriteLine("Help text: {0}", diskWritesSec.CounterHelp);
            //Console.WriteLine("Avg. Disk Bytes/Read: {0}", diskWritesSec.NextValue());
            
        }

        public static string GetAvgDiskWriteQueueLength()
        {
            PerformanceCounter avgDiskWriteQueueLength = new PerformanceCounter("PhysicalDisk", "Avg. Disk Read Queue Length", "_Total");
            while (true)
            {
                var result = avgDiskWriteQueueLength.NextValue();
                if (!result.Equals(0))
                {
                    var result2 = Convert.ToString(result);
                    result2 = result2.Replace(',', '.');
                    return result2;
                }
            }
        }

        public static string GetAvgDiskReadQueueLength()
        {
            PerformanceCounter avgDiskReadQueueLength = new PerformanceCounter("PhysicalDisk", "Avg. Disk Read Queue Length", "_Total");
            while (true)
            {
                var result = avgDiskReadQueueLength.NextValue();
                if (!result.Equals(0))
                {
                    var result2 = Convert.ToString(result);
                    result2 = result2.Replace(',', '.');
                    return result2;
                }
            }
        }

        public static ulong GetTotalVirtualMemoryInBytes()
        {
            return new Microsoft.VisualBasic.Devices.ComputerInfo().TotalVirtualMemory;
        }

        public static ulong GetAvailableVirtualMemoryInBytes()
        {//javítást igényel
            return new Microsoft.VisualBasic.Devices.ComputerInfo().TotalVirtualMemory - new Microsoft.VisualBasic.Devices.ComputerInfo().AvailableVirtualMemory;
        }

    }
}
