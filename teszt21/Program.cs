using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using log4net;
using log4net.Config;

[assembly: XmlConfigurator(Watch = true)]

namespace Zabbix_TCP_Application
{
    class Program
    {
        
        #region konstansok
        public static string HOSTNAME = Properties.Settings.Default.HOSTNAME;
        public static string ZABBIX_NAME = Properties.Settings.Default.ZABBIX_NAME;
        public static int ZABBIX_PORT = Properties.Settings.Default.ZABBIX_PORT; //pl 80
        public static int CONNECT_DELAY = Properties.Settings.Default.CONNECT_DELAY;
        public static int BUFFER_SIZE = Properties.Settings.Default.BUFFER_SIZE;
        public static Encoding ENCODING = Encoding.ASCII;


        #endregion konstansok


        private static readonly ILog ByteLog = LogManager.GetLogger("ByteLog");
        private static readonly ILog JsonLog = LogManager.GetLogger("JsonLog");
        private static readonly ILog Log = LogManager.GetLogger("Log");
        static void Main(string[] args)
        {
            //Zabbix_TCP_Application.Properties.Settings.Default.HOSTNAME
            //TODO: kiírni logba a appconfig értékét
            Log.Debug("Start");
            Log.Debug("Settings beállításai: HOSTNAME: " + HOSTNAME+ ", ZABBIX_NAME: "+ ZABBIX_NAME+ ", ZABBIX_PORT: "+ ZABBIX_PORT+ ", CONNECT_DELAY: " + CONNECT_DELAY+ ", BUFFER_SIZE: "+ BUFFER_SIZE);
            do
            {
                while (!Console.KeyAvailable)
                {
                    try
                    {
                        //string jsonData = String.Format(@"{{""request"":""active checks"",""host"":""{0}""}}", HOSTNAME);
                        string jsonData = String.Format(@"{{""request"": ""proxy config"", ""host"": ""{0}"", ""version"": ""3.4.13""}}", "gyakornok_tf_proxy");

                        string responseData = ConnectJson(jsonData);
                        /*
                        if (!responseData.Equals(String.Empty))
                        {
                            ResponseJsonObject jsonObject = JsonConvert.DeserializeObject<ResponseJsonObject>(responseData);
                            if (jsonObject.response != null && jsonObject.response.Equals("success"))
                            {
                                MakeAgentDataMessage(jsonObject);
                            }
                            else
                            {
                                Log.Error("Az active checket a szerver nem tudta feldolgozni. " + ((jsonObject.response == null) ? " Null az értéke a válasznak" : "Nem success a válasz értéke, hanem: " + jsonObject.response+ ". "+responseData));
                            }
                        }
                        else {
                            Log.Warn("Az active check feldolgozása során hiba lépett fel!");
                        }*/

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Log.Error("Hiba a main függvényben: " + e);
                    }

                    System.Threading.Thread.Sleep(CONNECT_DELAY * 1000);
                    
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            Log.Debug("STOP");
            //Console.WriteLine("STOP");
        }

        public static string MakeAgentDataMessage(ResponseJsonObject jsonObject) {
            #region változók + szótár
            var clock = DateTimeOffset.Now.ToUnixTimeSeconds();
            var rand = new Random();
            int ns = rand.Next(000000001, 999999999);
            Dictionary<string, string> dictKeyValue = new Dictionary<string, string>();
            dictKeyValue.Add("agent.hostname", HOSTNAME);
            dictKeyValue.Add("agent.ping", "1");
            dictKeyValue.Add("agent.version", "TCP_program");
            dictKeyValue.Add("net.if.list", GetProcessNumber().ToString());
            dictKeyValue.Add("perf_counter[\\234(_Total)\\1402]", GetAvgDiskReadQueueLength()); 
            dictKeyValue.Add("perf_counter[\\234(_Total)\\1404]", GetAvgDiskWriteQueueLength());
            dictKeyValue.Add("perf_counter[\\2\\16]", GetDiskReadsSec());
            dictKeyValue.Add("perf_counter[\\2\\18]", GetDiskWritesSec());
            dictKeyValue.Add("perf_counter[\\2\\250]", GetPerformanceCounter2_250().ToString());
            dictKeyValue.Add("proc.num[]", GetProcessNumber().ToString());
            dictKeyValue.Add("system.cpu.load[percpu,avg1]", "0.000000");
            dictKeyValue.Add("system.cpu.load[percpu,avg5]", "0.000000");
            dictKeyValue.Add("system.localtime[utc]", clock.ToString());
            dictKeyValue.Add("system.swap.size[,free]", GetAvailableVirtualMemoryInBytes().ToString());
            dictKeyValue.Add("system.swap.size[,total]", GetTotalVirtualMemoryInBytes().ToString());
            dictKeyValue.Add("system.uname", GetSystemUname());
            dictKeyValue.Add("system.uptime", GetUpTime());
            dictKeyValue.Add("vm.memory.size[free]", GetAvailableMemoryInBytes().ToString());
            dictKeyValue.Add("vm.memory.size[total]", GetTotalMemoryInBytes().ToString());
            #endregion változók

            string secondResponseData = String.Empty;
            RequestJsonObject requestJsonObject = new RequestJsonObject();
            try
            {
                if (jsonObject.data.Count > 0)
                {
                    int id = 1;
                    List<RequestJsonData> listRequestJsonData = new List<RequestJsonData>();
                    foreach (var item in jsonObject.data)
                    {
                        if (dictKeyValue.ContainsKey(item.key))
                        {
                            #region RequestJsonData lista felépítése
                            RequestJsonData requestJsonData = new RequestJsonData();
                            requestJsonData.key = item.key;
                            requestJsonData.value = dictKeyValue[item.key];
                            requestJsonData.id = id;
                            requestJsonData.clock = Convert.ToInt32(clock);
                            requestJsonData.ns = ns;
                            listRequestJsonData.Add(requestJsonData);
                            #endregion RequestJsonData lista felépítése
                            id++;
                        }
                    }
                    requestJsonObject.data = listRequestJsonData;
                    requestJsonObject.clock = Convert.ToInt32(clock);
                    requestJsonObject.ns = ns;

                    string serializedJson = JsonConvert.SerializeObject(requestJsonObject);

                    secondResponseData = ConnectJson(serializedJson);

                }
                else
                {
                    throw new Exception("Kaptunk választ, de a json data része üres");
                }
                return secondResponseData;
            }
            catch (Exception e)
            {
                Log.Error("Exception: " + e);
                return String.Empty;

            }
            
        }
        
        public static byte[] Packet(string data)
        {

            byte[] header = new byte[] {
            (byte)'Z', (byte)'B', (byte)'X', (byte)'D', (byte)0x00+1,
            (byte)(data.Length & 0xFF),
            (byte)((data.Length >> 8) & 0xFF),
            (byte)((data.Length >> 16) & 0xFF),
            (byte)((data.Length >> 24) & 0xFF),
            (byte)'\0', (byte)'\0', (byte)'\0', (byte)'\0'};

            byte[] packet = new byte[header.Length + data.Length];
            Array.Copy(header, 0, packet, 0, header.Length);
            Array.Copy(ENCODING.GetBytes(data), 0, packet, header.Length, data.Length);

            return packet;
        }

        
        public static string ConnectJson(string jsonData) 
        {     
            JsonLog.Debug("Sending:  " + jsonData);
            byte[] bytePacket = Packet(jsonData);
            try
            {
                byte[] result = Connect(bytePacket);
                if (!result.SequenceEqual(new byte[0]))
                {
                    string resultJson = ENCODING.GetString(result, 0, result.Length);
                    Console.WriteLine("Sent: {0}", jsonData);
                    JsonLog.Info("Sent:     " + jsonData);

                    byte[] resultWithoutHeader = new byte[result.Length - 13];
                    Array.Copy(result, 13, resultWithoutHeader, 0, resultWithoutHeader.Length);
                    resultJson = ENCODING.GetString(resultWithoutHeader);
                    byte[] headerOriginal = new byte[13];
                    Array.Copy(result, 0, headerOriginal, 0, headerOriginal.Length);
                    #region headerosszehasonlító (kommentelve)
                    /*foreach (var item in headerOriginal)
                    {
                        Console.WriteLine(item);
                    }
                    byte[] header = new byte[] {
                    (byte)'Z', (byte)'B', (byte)'X', (byte)'D', (byte)0x00+1,
                    (byte)(resultWithoutHeader.Length & 0xFF),
                    (byte)((resultWithoutHeader.Length >> 8) & 0xFF),
                    (byte)((resultWithoutHeader.Length >> 16) & 0xFF),
                    (byte)((resultWithoutHeader.Length >> 24) & 0xFF),
                    (byte)'\0', (byte)'\0', (byte)'\0', (byte)'\0'};
                    foreach (var item in header)
                    {
                        Console.WriteLine(item);
                    }*/
                    #endregion headerosszehasonlító (kommentelve)

                    JsonLog.Info("Received: " + resultJson);
                    Console.WriteLine("Received: {0}\n", resultJson);
                    if (((byte)(resultWithoutHeader.Length & 0xFF)).Equals(headerOriginal[5]) && ((byte)((resultWithoutHeader.Length >> 8) & 0xFF)).Equals(headerOriginal[6]) && ((byte)((resultWithoutHeader.Length >> 16) & 0xFF)).Equals(headerOriginal[7]) && ((byte)((resultWithoutHeader.Length >> 24) & 0xFF)).Equals(headerOriginal[8]))
                    {// Csomaghossz ellenőrzés
                        return resultJson;
                    }
                    else
                    {
                        //Console.WriteLine((byte)resultWithoutHeader.Length);
                        //Console.WriteLine(headerOriginal[5]);
                        //JsonLog.Error("Nem érkezett meg a teljes csomag ("+ (byte)resultWithoutHeader.Length + " helyett "+ headerOriginal[5] + " byte)" + resultJson);
                        throw new InvalidOperationException("Nem érkezett meg a teljes csomag (" + (byte)resultWithoutHeader.Length + " helyett " + headerOriginal[5] + " byte)" + resultJson);
                    }

                }
                else
                {
                    //JsonLog.Error("A csatlakozás során hiba lépett fel!");
                    throw new InvalidOperationException("A csatlakozás során hiba lépett fel!");
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
                JsonLog.Error("Exception: "+ e);
                return String.Empty;
            }
            
            

        }
        
        public static byte[] Connect(byte[] data) 
        {
            byte[] error = new byte[0];
            try //TODO: szétszedni a try-okat
            {

                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = ZABBIX_PORT;
                String server = ZABBIX_NAME;
                TcpClient client = new TcpClient(server, port);
                
                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);

                // Hexadecimális értékek logolása
                string hexValue = "";
                foreach (var item in data)
                {
                    int vOut = Convert.ToInt32(item);
                    hexValue += vOut.ToString("X");
                }
                ByteLog.Info("Sent:     " + hexValue);
                try
                {
                    // Receive the TcpServer.response.

                    // Buffer to store the response bytes.
                    data = new Byte[BUFFER_SIZE]; // TODO:Meg tudja mondani a stream, hogy mennyire van szükség? (availablebyte/readavailable)

                    // Read the first batch of the TcpServer response bytes.
                    int bytes = stream.Read(data, 0, data.Length);
                    data = TrimEnd(data);

                    // Hexadecimális értékek logolása
                    hexValue = "";
                    foreach (var item in data)
                    {
                        int vOut = Convert.ToInt32(item);
                        hexValue += vOut.ToString("X");
                    }
                    ByteLog.Info("Received: " + hexValue);

                    // Close everything.
                    stream.Close();
                    client.Close();
                    return data;
                }
                catch (Exception e)
                {
                    ByteLog.Error("Hiba a Connect válasz részénél: " + e);
                    return error;
                }
                

            }
            catch (ArgumentNullException e)
            {
                ByteLog.Error("Hiba a Connect küldés részénél: ArgumentNullException:" + e);
                Console.WriteLine("ArgumentNullException: {0}", e);
                return error;
            }
            catch (SocketException e)
            {
                ByteLog.Error("Hiba a Connect küldés részénél: SocketException: " + e);
                Console.WriteLine("SocketException: {0}", e);
                return error;
            }
            

        }
        public class RequestJsonObject
        {
            public string request = "agent data";
            public string session = "2dcf1bf2f6fc1c742812fbbf491e24f2";
            public List<RequestJsonData> data { get; set; }
            public int clock { get; set; }
            public int ns { get; set; }
        }
        public class RequestJsonData
        {
            public string host = HOSTNAME;
            public string key { get; set; }
            public string value { get; set; }
            public int id { get; set; }
            public int clock { get; set; }
            public int ns { get; set; }

        }

        public class ResponseJsonObject
        {
            public string response { get; set; }
            public List<ResponseJsonData> data { get; set; }

        }
        public class ResponseJsonData
        {
            public string key { get; set; }
            public int delay { get; set; }
            public int lastlogsize { get; set; }
            public int mtime { get; set; }

        }
        public static byte[] TrimEnd(byte[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);

            Array.Resize(ref array, lastIndex + 1);

            return array;
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

        private static string GetDiskReadsSec()
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

        private static string GetDiskWritesSec()
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

        private static string GetAvgDiskWriteQueueLength()
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

        private static string GetAvgDiskReadQueueLength()
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
