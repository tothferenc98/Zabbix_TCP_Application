using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics.Eventing.Reader;
using log4net;
using log4net.Config;

#region feladatok

// TODO: logolás
#endregion

namespace Zabbix_TCP_Application
{
    class Program
    {
        
        #region konstansok
        const string HOSTNAME = "gyakornok_tf_app";
        const string ZABBIX_NAME = "zabbix.beks.hu";
        const int ZABBIX_PORT = 10051;
        const int CONNECT_DELAY = 20;
        const int BUFFER_SIZE = 2048;
        #endregion konstansok

        

        static void Main(string[] args)
        {

            // TODO: log4net inicializálás
            // 1. logger a tcp kimenő, bemenő adatnak BYTEBAN!
            // 2. logger küldött és fogadott json 
            // 3. logger minden egyéb
            //baretail logelemzéshez


            while (true)
            {
                string message = String.Format("{{\"request\":\"active checks\",\"host\":\"{0}\"}}", HOSTNAME);
                //string message2 = String.Format(@"{{""request"":""active checks"",""host"":""{0}""}}", HOSTNAME);
                string responseData = ConnectJson(message);
                if (responseData.Contains("success"))  // TODO: deserializásás a jsonre és objektum létrehozása
                {
                    MakeAgentDataMessage(responseData);
                }

                System.Threading.Thread.Sleep(CONNECT_DELAY * 1000);

            }
        }

        public static string MakeAgentDataMessage(string responseData) {
            #region változók + szótár
            var clock = DateTimeOffset.Now.ToUnixTimeSeconds();
            var rand = new Random();
            int ns = rand.Next(000000001, 999999999);

            string agentHostname = String.Format("{{\"host\":\"{0}\",\"key\":\"agent.hostname\",\"value\":\"{0}\",\"id\":1,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            string agentPing = String.Format("{{\"host\":\"{0}\",\"key\":\"agent.ping\",\"value\":\"1\",\"id\":2,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            string agentVersion = String.Format("{{\"host\":\"{0}\",\"key\":\"agent.version\",\"value\":\"TCP_program\",\"id\":3,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            //string eventlogSystemMicrosoftWindowsKernelPower = String.Format("{{\"host\":\"{0}\",\"key\":\"eventlog[System,,,\\\"Microsoft-Windows-Kernel-Power\\\"]\",\"value\":\"Teszt\",\"lastlogsize\":10815,\"timestamp\":1624900534,\"source\":\"Microsoft-Windows-Kernel-Power\",\"severity\":1,\"eventid\":187,\"id\":4,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            string netIfList = String.Format("{{\"host\":\"{0}\",\"key\":\"net.if.list\",\"value\":\"Teszt\",\"id\":8,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            string perfCounter234Total1402 = String.Format("{{\"host\":\"{0}\",\"key\":\"perf_counter[\\\\234(_Total)\\\\1402]\",\"value\":\"{3}\",\"id\":9,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns, GetAvgDiskReadQueueLength()); //Average disk read queue length
            string perfCounter234Total1404 = String.Format("{{\"host\":\"{0}\",\"key\":\"perf_counter[\\\\234(_Total)\\\\1404]\",\"value\":\"{3}\",\"id\":10,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns, GetAvgDiskWriteQueueLength()); //Average disk write queue length
            string perfCounter2_16 = String.Format("{{\"host\":\"{0}\",\"key\":\"perf_counter[\\\\2\\\\16]\",\"value\":\"{3}\",\"id\":11,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns, GetDiskReadsSec()); //File read bytes per second
            string perfCounter2_18 = String.Format("{{\"host\":\"{0}\",\"key\":\"perf_counter[\\\\2\\\\18]\",\"value\":\"{3}\",\"id\":12,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns, GetDiskWritesSec()); //File write bytes per second
            string perfCounter2_250 = String.Format("{{\"host\":\"{0}\",\"key\":\"perf_counter[\\\\2\\\\250]\",\"value\":\"{3}\",\"id\":13,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns, GetPerformanceCounter2_250()); //Number of threads
            string procNum = String.Format("{{\"host\":\"{0}\",\"key\":\"proc.num[]\",\"value\":\"{3}\",\"id\":14,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns, GetProcessNumber());
            string systemCpuLoadPerCpuAvg1 = String.Format("{{\"host\":\"{0}\",\"key\":\"system.cpu.load[percpu,avg1]\",\"value\":\"0.000000\",\"id\":15,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            string systemCpuLoadPerCpuAvg5 = String.Format("{{\"host\":\"{0}\",\"key\":\"system.cpu.load[percpu,avg5]\",\"value\":\"0.000000\",\"id\":16,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            string systemLocaltimeUtc = String.Format("{{\"host\":\"{0}\",\"key\":\"system.localtime[utc]\",\"value\":\"{1}\",\"id\":17,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            //string systemRunIpconfigFindstrIPv4sort = String.Format("{{\"host\":\"{0}\",\"key\":\"system.run[ipconfig | findstr IPv4 | sort]\",\"value\":\"{3}.\",\"state\":1,\"id\":18,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns, GetMyIP());  // TODO: helyes value? Eredetileg Unsupported item key. Nem használja a program
            //string systemRunSysteminfo = String.Format("{{\"host\":\"{0}\",\"key\":\"system.run[systeminfo,]\",\"value\":\"teszt2\",\"state\":1,\"id\":19,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns); //Eredetileg Unsupported item key.
            string systemSwapSizeFree = String.Format("{{\"host\":\"{0}\",\"key\":\"system.swap.size[,free]\",\"value\":\"{3}\",\"id\":20,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns, GetAvailableVirtualMemoryInBytes());
            string systemSwapSizeTotal = String.Format("{{\"host\":\"{0}\",\"key\":\"system.swap.size[,total]\",\"value\":\"{3}\",\"id\":21,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns, GetTotalVirtualMemoryInBytes());
            string systemUname = String.Format("{{\"host\":\"{0}\",\"key\":\"system.uname\",\"value\":\"{3}\",\"id\":22,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns, GetSystemUname());
            string systemUptime = String.Format("{{\"host\":\"{0}\",\"key\":\"system.uptime\",\"value\":\"{3}\",\"id\":23,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns, GetUpTime());
            string vmMemorySizeFree = String.Format("{{\"host\":\"{0}\",\"key\":\"vm.memory.size[free]\",\"value\":\"{3}\",\"id\":24,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns, GetAvailableMemoryInBytes());
            string vmMemorySizeTotal = String.Format("{{\"host\":\"{0}\",\"key\":\"vm.memory.size[total]\",\"value\":\"{3}\",\"id\":25,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns, GetTotalMemoryInBytes());

            Dictionary<string, string> stringpair = new Dictionary<string, string>();
            stringpair.Add("agent.hostname", agentHostname);
            stringpair.Add("agent.ping", agentPing);
            stringpair.Add("agent.version", agentVersion);
            //stringpair.Add("eventlog[System,,,\"Microsoft-Windows-Kernel-Power\"]", eventlogSystemMicrosoftWindowsKernelPower);
            stringpair.Add("net.if.list", netIfList);
            stringpair.Add("perf_counter[\\234(_Total)\\1402]", perfCounter234Total1402);
            stringpair.Add("perf_counter[\\234(_Total)\\1404]", perfCounter234Total1404);
            stringpair.Add("perf_counter[\\2\\16]", perfCounter2_16);
            stringpair.Add("perf_counter[\\2\\18]", perfCounter2_18);
            stringpair.Add("perf_counter[\\2\\250]", perfCounter2_250);
            stringpair.Add("proc.num[]", procNum);
            stringpair.Add("system.cpu.load[percpu,avg1]", systemCpuLoadPerCpuAvg1);
            stringpair.Add("system.cpu.load[percpu,avg5]", systemCpuLoadPerCpuAvg5);
            stringpair.Add("system.localtime[utc]", systemLocaltimeUtc);
            //stringpair.Add("system.run[ipconfig | findstr IPv4 | sort]", systemRunIpconfigFindstrIPv4sort);
            //stringpair.Add("system.run[systeminfo,]", systemRunSysteminfo);
            stringpair.Add("system.swap.size[,free]", systemSwapSizeFree);
            stringpair.Add("system.swap.size[,total]", systemSwapSizeTotal);
            stringpair.Add("system.uname", systemUname);
            stringpair.Add("system.uptime", systemUptime);
            stringpair.Add("vm.memory.size[free]", vmMemorySizeFree);
            stringpair.Add("vm.memory.size[total]", vmMemorySizeTotal);

            Dictionary<string, string> dictKeyValue = new Dictionary<string, string>();
            dictKeyValue.Add("agent.hostname", HOSTNAME);
            dictKeyValue.Add("agent.ping", "1");
            dictKeyValue.Add("agent.version", "TCP_program");
            dictKeyValue.Add("net.if.list", GetProcessNumber().ToString());
            /*
            dictKeyValue.Add("perf_counter[\\234(_Total)\\1402]", GetAvgDiskReadQueueLength());
            dictKeyValue.Add("perf_counter[\\234(_Total)\\1404]", GetAvgDiskWriteQueueLength());
            dictKeyValue.Add("perf_counter[\\2\\16]", GetDiskReadsSec());
            dictKeyValue.Add("perf_counter[\\2\\18]", GetDiskWritesSec());
            dictKeyValue.Add("perf_counter[\\2\\250]", GetPerformanceCounter2_250().ToString());*/
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

            List<JsonClass> jsonData = JsonConvert.DeserializeObject<List<JsonClass>>(ActiveCheckFilter(responseData));
            string secondResponseData = "";
            
            if (jsonData.Count > 0)
            {
                string secondMessage = "{\"request\":\"agent data\",\"session\":\"2dcf1bf2f6fc1c742812fbbf491e24f2\",\"data\":[";
                // TODO: StringBuilder használata
                foreach (var item in jsonData)
                {
                    if (dictKeyValue.ContainsKey(item.key))
                    {
                        //secondMessage += stringpair[item.key] + ",";
                        secondMessage += String.Format(@"{{""host"":""{0}"",""key"":""{4}"",""value"":""{3}"",""id"":14,""clock"":{1},""ns"":{2}}}", HOSTNAME, clock, ns, dictKeyValue[item.key], item.key) + ",";
                        
                        // LEGFONTOSABB, EZZEL KELL KEZDENI, A PEREKKEL BAJ VAN!!!!!
                        ;
                        // TODO: dictKeyValue használata, ide jön a String.Format

                    }

                }
                secondMessage = secondMessage.Remove(secondMessage.Length - 1, 1);
                secondMessage += "],\"clock\":" + clock + ",\"ns\":" + ns + "}";


                secondResponseData = ConnectJson(secondMessage);

            }
            return secondResponseData;
        }
        /*
        public static string MakePacketAndConnect(string message) {
            byte[] packet = Packet(message);
            // TODO: message átnevezése jsonmessage-re
            string responseData = "";
            try
            {
                responseData = ConnectJson(packet);

            }
            catch (Exception e)
            {
                Console.WriteLine("HIBA: {0}", e);
            }
            return responseData;
        }*/

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
            Array.Copy(Encoding.ASCII.GetBytes(data), 0, packet, header.Length, data.Length);

            return packet;
        }

        
        public static string ConnectJson(string jsonData) // TODO: itt történne meg a zbxd header képzés és visszaad egy headertelenítétt jsont, connectet hívja meg
        {
            byte[] bytePacket = Packet(jsonData);
            string resultJson = String.Empty;
            try
            {
                byte[] result = Connect(bytePacket);

                byte[] resultWithoutHeader = new byte[result.Length -12];
                Array.Copy(result, 12, resultWithoutHeader, 0, resultWithoutHeader.Length);
                resultJson = Encoding.Default.GetString(resultWithoutHeader);
            }
            catch (Exception e)
            {
                Console.WriteLine("HIBA: {0}", e);
            }
            return resultJson;




        }
        
        public static byte[] Connect(byte[] data) // TODO: message kiszedése, és string helyett byte tömb visszaadása
        {
            try
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
                String requestData = String.Empty;
                requestData = System.Text.Encoding.ASCII.GetString(data, 0, data.Length);
                Console.WriteLine("Sent: {0}",requestData);


                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[BUFFER_SIZE]; // TODO:Meg tudja mondani a stream, hogy mennyire van szükség? (availablebyte/readavailable)

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                int bytes = stream.Read(data, 0, data.Length);
                
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}\n", responseData);

                /* Hexadecimális értékek kiíratása
                foreach (var item in data)
                {
                    int vOut = Convert.ToInt32(item);
                    string hexValue = vOut.ToString("X");
                    // Convert the hex string back to the number
                    int intAgain = int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);

                    Console.Write(" "+ hexValue);
                }
                */

                // Close everything.
                stream.Close();
                client.Close();
                
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            return data;

        }
        //Legfelső réteg: objektum szint
        //Középső réteg: json szint
        //Alatta réteg: ZBXD csomag
        //Alsó réteg : Byte tömb


        public class JsonClass
        {
            public string key { get; set; }
            public int delay { get; set; }
            public int lastlogsize { get; set; }
            public int mtime { get; set; }

        }

        public static string ActiveCheckFilter(string data)
        {
            if (data.Contains("[") && data.Contains("]"))
            {
                int index = data.IndexOf('[');
                int lastindex = data.LastIndexOf(']');
                /*
                string jsonString =

                @"
                [
                {""key"":""agent.hostname"",""delay"":3600,""lastlogsize"":0,""mtime"":0},
                {""key"":""vm.memory.size[total]"",""delay"":3600,""lastlogsize"":0,""mtime"":0}
                ]
                "; csak ilyen [] közötti adatokat tudja feldolgozni a program!!
                */
                return data.Substring(index, lastindex - index + 1);
            }
            else
            {
                return "";
            }
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
            //return new Microsoft.VisualBasic.Devices.ComputerInfo().OSFullName;
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

        // TODO: javítást igényel
        public static ulong GetAvailableVirtualMemoryInBytes()
        {
            return new Microsoft.VisualBasic.Devices.ComputerInfo().TotalVirtualMemory - new Microsoft.VisualBasic.Devices.ComputerInfo().AvailableVirtualMemory;
        }

    }
}
