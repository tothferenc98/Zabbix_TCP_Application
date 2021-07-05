﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;



#region feladatok
// .TODO: TCP kapcsolat meghatározása  (TASK LIST)
// .TODO: TcpClient példakód átmásolása https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient?view=net-5.0
// ZABBIX request string megépítése (1624524243) epoch időkonverzió - string.Format!!
// Végtelen ciklus 20s-ként megszólítani a zabbix szervert (hibakezelés)
// válasz kiíratása konzolra
// Rövid függvények

// TODO: szükséges információk lekérése a pc-től 
// https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.performancecountertype?view=net-5.0 (RateOfCountsPerSecond64)--> perf_counter[\2\16]
// TODO: json kezelése, key-ek kiszedése
// TODO: format.string kezelés
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
        #endregion konstansok

        static void Main(string[] args)
        {
            
            while (true)
            {
                string message = String.Format("{{\"request\":\"active checks\",\"host\":\"{0}\"}}", HOSTNAME);
                string responseData = MakePacketAndConnect(message);

                if (responseData.Contains("success"))
                {
                    MakeAgentDataMessage(responseData);
                }

                System.Threading.Thread.Sleep(CONNECT_DELAY * 1000);

            }
        }

        public static string MakeAgentDataMessage(string responseData) {
            #region változók
            var clock = DateTimeOffset.Now.ToUnixTimeSeconds();
            var rand = new Random();
            int ns = rand.Next(000000001, 999999999);

            string agentHostname = String.Format("{{\"host\":\"{0}\",\"key\":\"agent.hostname\",\"value\":\"{0}\",\"id\":1,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            string agentPing = String.Format("{{\"host\":\"{0}\",\"key\":\"agent.ping\",\"value\":\"1\",\"id\":2,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            string agentVersion = String.Format("{{\"host\":\"{0}\",\"key\":\"agent.version\",\"value\":\"TCP_program\",\"id\":3,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            string eventlogSystemMicrosoftWindowsKernelPower = String.Format("{{\"host\":\"{0}\",\"key\":\"eventlog[System,,,\\\"Microsoft-Windows-Kernel-Power\\\"]\",\"value\":\"A felhaszn..l..i ..zemm..d.. folyamat megpr..b..lta m..dos..tani a rendszer..llapotot a SetSuspendState vagy a SetSystemPowerState API h..v..s..val.\",\"lastlogsize\":10815,\"timestamp\":1624900534,\"source\":\"Microsoft-Windows-Kernel-Power\",\"severity\":1,\"eventid\":187,\"id\":4,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            string netIfList = String.Format("{{\"host\":\"{0}\",\"key\":\"net.if.list\",\"value\":\"Teszt\",\"id\":8,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            string perfCounter234Total1402 = String.Format("{{\"host\":\"{0}\",\"key\":\"perf_counter[\\\\234(_Total)\\\\1402]\",\"value\":\"0.019797\",\"id\":9,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns); //Average disk read queue length
            string perfCounter234Total1404 = String.Format("{{\"host\":\"{0}\",\"key\":\"perf_counter[\\\\234(_Total)\\\\1404]\",\"value\":\"0.006441\",\"id\":10,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns); //Average disk write queue length
            string perfCounter2_16 = String.Format("{{\"host\":\"{0}\",\"key\":\"perf_counter[\\\\2\\\\16]\",\"value\":\"17219.955967\",\"id\":11,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns); //File read bytes per second
            string perfCounter2_18 = String.Format("{{\"host\":\"{0}\",\"key\":\"perf_counter[\\\\2\\\\18]\",\"value\":\"113342.473095\",\"id\":12,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns); //File write bytes per second
            string perfCounter2_250 = String.Format("{{\"host\":\"{0}\",\"key\":\"perf_counter[\\\\2\\\\250]\",\"value\":\"{3}\",\"id\":13,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns, GetPerformanceCounter2_250()); //Number of threads
            string procNum = String.Format("{{\"host\":\"{0}\",\"key\":\"proc.num[]\",\"value\":\"{3}\",\"id\":14,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns, GetProcessNumber());
            string systemCpuLoadPerCpuAvg1 = String.Format("{{\"host\":\"{0}\",\"key\":\"system.cpu.load[percpu,avg1]\",\"value\":\"0.000000\",\"id\":15,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            string systemCpuLoadPerCpuAvg5 = String.Format("{{\"host\":\"{0}\",\"key\":\"system.cpu.load[percpu,avg5]\",\"value\":\"0.000000\",\"id\":16,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            string systemLocaltimeUtc = String.Format("{{\"host\":\"{0}\",\"key\":\"system.localtime[utc]\",\"value\":\"{1}\",\"id\":17,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            string systemRunIpconfigFindstrIPv4sort = String.Format("{{\"host\":\"{0}\",\"key\":\"system.run[ipconfig | findstr IPv4 | sort]\",\"value\":\"Unsupported item key.\",\"state\":1,\"id\":18,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            string systemRunSysteminfo = String.Format("{{\"host\":\"{0}\",\"key\":\"system.run[systeminfo,]\",\"value\":\"Unsupported item key.\",\"state\":1,\"id\":19,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            string systemSwapSizeFree = String.Format("{{\"host\":\"{0}\",\"key\":\"system.swap.size[,free]\",\"value\":\"1971662848\",\"id\":20,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            string systemSwapSizeTotal = String.Format("{{\"host\":\"{0}\",\"key\":\"system.swap.size[,total]\",\"value\":\"5637144576\",\"id\":21,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            string systemUname = String.Format("{{\"host\":\"{0}\",\"key\":\"system.uname\",\"value\":\"{3}\",\"id\":22,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns, GetSystemUname());
            string systemUptime = String.Format("{{\"host\":\"{0}\",\"key\":\"system.uptime\",\"value\":\"{3}\",\"id\":23,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns, GetUpTime());
            string vmMemorySizeFree = String.Format("{{\"host\":\"{0}\",\"key\":\"vm.memory.size[free]\",\"value\":\"{3}\",\"id\":24,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns, GetAvailableMemoryInBytes());
            string vmMemorySizeTotal = String.Format("{{\"host\":\"{0}\",\"key\":\"vm.memory.size[total]\",\"value\":\"{3}\",\"id\":25,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns, GetTotalMemoryInBytes());

            Dictionary<string, string> stringpair = new Dictionary<string, string>();
            stringpair.Add("agent.hostname", agentHostname);
            stringpair.Add("agent.ping", agentPing);
            stringpair.Add("agent.version", agentVersion);
            stringpair.Add("eventlog[System,,,\"Microsoft-Windows-Kernel-Power\"]", eventlogSystemMicrosoftWindowsKernelPower);
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
            stringpair.Add("system.run[ipconfig | findstr IPv4 | sort]", systemRunIpconfigFindstrIPv4sort);
            stringpair.Add("system.run[systeminfo,]", systemRunSysteminfo);
            stringpair.Add("system.swap.size[,free]", systemSwapSizeFree);
            stringpair.Add("system.swap.size[,total]", systemSwapSizeTotal);
            stringpair.Add("system.uname", systemUname);
            stringpair.Add("system.uptime", systemUptime);
            stringpair.Add("vm.memory.size[free]", vmMemorySizeFree);
            stringpair.Add("vm.memory.size[total]", vmMemorySizeTotal);

            #endregion változók

            List<JsonClass> jsonData = JsonConvert.DeserializeObject<List<JsonClass>>(ActiveCheckFilter(responseData));
            string secondResponseData = "";
            if (jsonData.Count > 0)
            {
                string secondMessage = "{\"request\":\"agent data\",\"session\":\"2dcf1bf2f6fc1c742812fbbf491e24f2\",\"data\":[";
                foreach (var item in jsonData)
                {
                    if (stringpair.ContainsKey(item.key))
                    {
                        secondMessage += stringpair[item.key] + ",";
                    }

                }
                secondMessage = secondMessage.Remove(secondMessage.Length - 1, 1);
                secondMessage += "],\"clock\":" + clock + ",\"ns\":" + ns + "}";


                secondResponseData = MakePacketAndConnect(secondMessage);

            }
            return secondResponseData;
        }
        public static string MakePacketAndConnect(string message) {
            byte[] packet = Packet(message);

            string responseData = "";
            try
            {
                responseData = Connect(ZABBIX_NAME, packet, message);

            }
            catch (Exception e)
            {
                Console.WriteLine("HIBA: {0}", e);
            }
            return responseData;
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
            Array.Copy(Encoding.ASCII.GetBytes(data), 0, packet, header.Length, data.Length);
            Array.Copy(Encoding.ASCII.GetBytes(data), 0, packet, header.Length, data.Length);

            return packet;
        }

        public static string Connect(String server, byte[] data, string message)
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = ZABBIX_PORT;
                TcpClient client = new TcpClient(server, port);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[2048];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}\n", responseData);

                // Close everything.
                stream.Close();
                client.Close();
                return responseData;
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
                return "";
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                return "";
            }

        }

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

        /*public static int GetSystemCpuLoadPerCpuAvg1()
        {
            var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", Environment.MachineName);
            cpuCounter.NextValue();
            System.Threading.Thread.Sleep(100); //This avoid that answer always 0
            return (int)cpuCounter.NextValue();
       
        }*/


    }
}
