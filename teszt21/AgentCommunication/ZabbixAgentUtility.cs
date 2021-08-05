﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using log4net;
using log4net.Config;
using System.Collections.Generic;

namespace Zabbix_TCP_Application.AgentCommunication
{
    class ZabbixAgentUtility
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

        public static int GetPerformanceCounter2_250()
        {
            var count = Process.GetProcesses().Sum(p => p.Threads.Count);
            return count;
        }

        public static string GetUpTime()
        {
            var atalakit = GetTickCount64() / 1000;

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
                if (!result.Equals(0))
                {
                    var result2 = Convert.ToString(result);
                    result2 = result2.Replace(',', '.');
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