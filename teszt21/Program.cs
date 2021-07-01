using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
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

namespace teszt21
{
    class Program
    {

        #region konstansok
        const string HOSTNAME = "gyakornok_tf_app";
        const string ZABBIX_NAME = "zabbix.beks.hu";
        const int ZABBIX_PORT = 10051;
        const int CONNECT_DELAY = 5;
        #endregion konstansok

        static void Main(string[] args)
        {

            while (true)
            {
                var clock = DateTimeOffset.Now.ToUnixTimeSeconds();
                var rand = new Random();
                int ns = rand.Next(000000001, 999999999);
                #region stringek
                string agentHostname = String.Format("{{\"host\":\"{0}\",\"key\":\"agent.hostname\",\"value\":\"{0}\",\"id\":1,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
                string agentPing = String.Format("{{\"host\":\"{0}\",\"key\":\"agent.ping\",\"value\":\"1\",\"id\":2,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
                string agentVersion = String.Format("{{\"host\":\"{0}\",\"key\":\"agent.version\",\"value\":\"TCP_program\",\"id\":3,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);


                string eventlogSystemMicrosoftWindowsKernelPower = String.Format("{{\"host\":\"gyakornok_tf_app\",\"key\":\"eventlog[System,,,\"Microsoft-Windows-Kernel-Power\"]\",\"value\":\"A felhaszn..l..i ..zemm..d.. folyamat megpr..b..lta m..dos..tani a rendszer..llapotot a SetSuspendState vagy a SetSystemPowerState API h..v..s..val.\",\"lastlogsize\":10815,\"timestamp\":1624900534,\"source\":\"Microsoft-Windows-Kernel-Power\",\"severity\":1,\"eventid\":187,\"id\":4,\"clock\":1624960474,\"ns\":186056600}}");
                string netIfList = String.Format("{{\"host\":\"gyakornok_tf_app\",\"key\":\"net.if.list\",\"value\":\"Ethernet                  enabled  - Realtek PCIe GbE Family Controller-WFP Native MAC Layer LightWeight Filter-0000\nEthernet                  enabled  - Realtek PCIe GbE Family Controller-Npcap Packet Driver (NPCAP)-0000\nEthernet                  enabled  - Realtek PCIe GbE Family Controller-VirtualBox NDIS Light-Weight Filter-0000\nEthernet                  enabled  - Realtek PCIe GbE Family Controller-QoS Packet Scheduler-0000\nEthernet                  enabled  - Realtek PCIe GbE Family Controller-WFP 802.3 MAC Layer LightWeight Filter-0000\nEthernet                  enabled  - WAN Miniport (IP)-WFP Native MAC Layer LightWeight Filter-0000\nEthernet                  enabled  - WAN Miniport (IP)-Npcap Packet Driver (NPCAP)-0000\nEthernet                  enabled  - WAN Miniport (IP)-QoS Packet Scheduler-0000\nEthernet                  enabled  - WAN Miniport (IPv6)-WFP Native MAC Layer LightWeight Filter-0000\nEthernet                  enabled  - WAN Miniport (IPv6)-Npcap Packet Driver (NPCAP)-0000\nEthernet                  enabled  - WAN Miniport (IPv6)-QoS Packet Scheduler-0000\nEthernet                  enabled  - WAN Miniport (Network Monitor)-WFP Native MAC Layer LightWeight Filter-0000\nEthernet                  enabled  - WAN Miniport (Network Monitor)-Npcap Packet Driver (NPCAP)-0000\nEthernet                  enabled  - WAN Miniport (Network Monitor)-QoS Packet Scheduler-0000\nEthernet                  unknown  - Microsoft Kernel Debug Network Adapter\nEthernet                  enabled  - Realtek PCIe GbE Family Controller\nEthernet                  unknown  - Bluetooth Device (Personal Area Network)\nEthernet                  enabled  - WAN Miniport (IP)\nEthernet                  enabled  - WAN Miniport (IPv6)\nEthernet                  enabled  - WAN Miniport (Network Monitor)\nEthernet                  unknown  - VirtualBox Host-Only Ethernet Adapter\nPPP                       enabled  - WAN Miniport (PPPOE)\nSoftware Loopback         enabled  127.0.0.1       Software Loopback Interface 1\nIEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-WFP Native MAC Layer LightWeight Filter-0000\nIEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-Npcap Packet Driver (NPCAP) (WiFi version)-0000\nIEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-Virtual WiFi Filter Driver-0000\nIEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-Native WiFi Filter Driver-0000\nIEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-Npcap Packet Driver (NPCAP)-0000\nIEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-VirtualBox NDIS Light-Weight Filter-0000\nIEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-QoS Packet Scheduler-0000\nIEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-WFP 802.3 MAC Layer LightWeight Filter-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-WFP Native MAC Layer LightWeight Filter-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-Npcap Packet Driver (NPCAP) (WiFi version)-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-Native WiFi Filter Driver-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-Npcap Packet Driver (NPCAP)-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-VirtualBox NDIS Light-Weight Filter-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-QoS Packet Scheduler-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-WFP 802.3 MAC Layer LightWeight Filter-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-WFP Native MAC Layer LightWeight Filter-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-Npcap Packet Driver (NPCAP) (WiFi version)-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-Native WiFi Filter Driver-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-Npcap Packet Driver (NPCAP)-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-VirtualBox NDIS Light-Weight Filter-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-QoS Packet Scheduler-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-WFP 802.3 MAC Layer LightWeight Filter-0000\nIEEE 802.11 Wireless      enabled  192.168.1.100   Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2\nTunnel type encapsulation unknown  - Microsoft Teredo Tunneling Adapter\nTunnel type encapsulation unknown  - Microsoft IP-HTTPS Platform Adapter\nTunnel type encapsulation unknown  - Microsoft 6to4 Adapter\nTunnel type encapsulation enabled  - WAN Miniport (SSTP)\nTunnel type encapsulation enabled  - WAN Miniport (IKEv2)\nTunnel type encapsulation enabled  - WAN Miniport (L2TP)\nTunnel type encapsulation enabled  - WAN Miniport (PPTP)\n\",\"id\":8,\"clock\":1624960474,\"ns\":238679600}}");
                string perfCounter234Total1402 = String.Format("{{\"host\":\"gyakornok_tf_app\",\"key\":\"perf_counter[\\234(_Total)\\1402]\",\"value\":\"0.019797\",\"id\":9,\"clock\":1624960475,\"ns\":252629400}}"); //Average disk read queue length
                string perfCounter234Total1404 = String.Format("{{\"host\":\"gyakornok_tf_app\",\"key\":\"perf_counter[\\234(_Total)\\1404]\",\"value\":\"0.006441\",\"id\":10,\"clock\":1624960476,\"ns\":260388900}}"); //Average disk write queue length
                string perfCounter2_16 = String.Format("{{\"host\":\"gyakornok_tf_app\",\"key\":\"perf_counter[\\2\\16]\",\"value\":\"17219.955967\",\"id\":11,\"clock\":1624960477,\"ns\":276593400}}"); //File read bytes per second
                string perfCounter2_18 = String.Format("{{\"host\":\"gyakornok_tf_app\",\"key\":\"perf_counter[\\2\\18]\",\"value\":\"113342.473095\",\"id\":12,\"clock\":1624960478,\"ns\":288443500}}"); //File write bytes per second
                string perfCounter2_250 = String.Format("{{\"host\":\"gyakornok_tf_app\",\"key\":\"perf_counter[\\2\\250]\",\"value\":\"2906.000000\",\"id\":13,\"clock\":1624960478,\"ns\":291858000}}");
                string procNum = String.Format("{{\"host\":\"gyakornok_tf_app\",\"key\":\"proc.num[]\",\"value\":\"247\",\"id\":14,\"clock\":1624960478,\"ns\":299047600}}");
                string systemCpuLoadPerCpuAvg1 = String.Format("{{\"host\":\"gyakornok_tf_app\",\"key\":\"system.cpu.load[percpu,avg1]\",\"value\":\"0.000000\",\"id\":15,\"clock\":1624960478,\"ns\":299154200}}");
                string systemCpuLoadPerCpuAvg5 = String.Format("{{\"host\":\"gyakornok_tf_app\",\"key\":\"system.cpu.load[percpu,avg5]\",\"value\":\"0.000000\",\"id\":16,\"clock\":1624960478,\"ns\":299159800}}");
                string systemLocaltimeUtc = String.Format("{{\"host\":\"gyakornok_tf_app\",\"key\":\"system.localtime[utc]\",\"value\":\"1624960478\",\"id\":17,\"clock\":1624960478,\"ns\":299168100}}");
                string systemRunIpconfigFindstrIPv4sort = String.Format("{{\"host\":\"gyakornok_tf_app\",\"key\":\"system.run[ipconfig | findstr IPv4 | sort]\",\"value\":\"Unsupported item key.\",\"state\":1,\"id\":18,\"clock\":1624960478,\"ns\":300200600}}");
                string systemRunSysteminfo = String.Format("{{\"host\":\"gyakornok_tf_app\",\"key\":\"system.run[systeminfo,]\",\"value\":\"Unsupported item key.\",\"state\":1,\"id\":19,\"clock\":1624960478,\"ns\":301352900}}");
                string systemSwapSizeFree = String.Format("{{\"host\":\"gyakornok_tf_app\",\"key\":\"system.swap.size[,free]\",\"value\":\"1971662848\",\"id\":20,\"clock\":1624960478,\"ns\":301386100}}");
                string systemSwapSizeTotal = String.Format("{{\"host\":\"gyakornok_tf_app\",\"key\":\"system.swap.size[,total]\",\"value\":\"5637144576\",\"id\":21,\"clock\":1624960478,\"ns\":301394100}}");
                string systemUname = String.Format("{{\"host\":\"gyakornok_tf_app\",\"key\":\"system.uname\",\"value\":\"Windows DESKTOP-H8HU6UC 10.0.19042 Microsoft Windows 10 Pro x64\",\"id\":22,\"clock\":1624960478,\"ns\":417437700}}");
                string systemUptime = String.Format("{{\"host\":\"gyakornok_tf_app\",\"key\":\"system.uptime\",\"value\":\"609411\",\"id\":23,\"clock\":1624960478,\"ns\":420357000}}");
                string vmMemorySizeFree = String.Format("{{\"host\":\"gyakornok_tf_app\",\"key\":\"vm.memory.size[free]\",\"value\":\"2073059328\",\"id\":24,\"clock\":1624960478,\"ns\":420366000}}");
                string vmMemorySizeTotal = String.Format("{{\"host\":\"gyakornok_tf_app\",\"key\":\"vm.memory.size[total]\",\"value\":\"8477478912\",\"id\":25,\"clock\":1624960478,\"ns\":420369200}}");

                #endregion stringek

                //Console.WriteLine(agentVersion);
                //Console.ReadLine();

                //string message = "{\"request\":\"agent data\",\"session\":\"2dcf1bf2f6fc1c742812fbbf491e24f2\",\"data\":[" + agentHostname + "," + agentPing + "," + agentVersion + "],\"clock\":" + clock + ",\"ns\":" + ns + "}";
                string message = "{\"request\":\"active checks\",\"host\":\"gyakornok_tf_app\"}";

                
                byte[] packet = Packet(message);

                foreach (var item in packet)
                {
                    //Console.WriteLine(item);
                }

                string responseData = "";
                try
                {
                    responseData = Connect(ZABBIX_NAME, packet, message);
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine("HIBA: {0}",e);
                }
                Console.WriteLine("Received: {0}\n", responseData);
                

                // TODO: Most active check-re működik, a request válaszát is elemezni kell jsonnal
                List<JsonClass> jsonData = JsonConvert.DeserializeObject<List<JsonClass>>(ActiveCheckFilter(responseData));
                foreach (var item in jsonData)
                {
                    Console.WriteLine(item.key);
                }

                System.Threading.Thread.Sleep(CONNECT_DELAY*1000);
                

                //Console.WriteLine("Várakozás, folytatáshoz nyomj entert");
                Console.ReadKey();
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
                //Console.WriteLine("Received: {0}\n", responseData);

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
    }
}
