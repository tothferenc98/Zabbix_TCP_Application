using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace teszt21
{
    class Program
    {
        

        static void Main(string[] args)
        {
            
          
            #region konstansok
            const string HOSTNAME = "gyakornok_tf_app";
            const string ZABBIX_NAME = "zabbix.beks.hu";
            const int ZABBIX_PORT = 10051;
            #endregion konstansok

           
            var clock = DateTimeOffset.Now.ToUnixTimeSeconds();
            var rand = new Random();
            int ns = rand.Next(000000001, 999999999);
            #region stringek
            string agentHostname = String.Format("{{\"host\":\"{0}\",\"key\":\"agent.hostname\",\"value\":\"{0}\",\"id\":1,\"clock\":{1},\"ns\":{2}}}", HOSTNAME,clock,ns);
            string agentPing = String.Format("{{\"host\":\"{0}\",\"key\":\"agent.ping\",\"value\":\"1\",\"id\":2,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            string agentVersion = String.Format("{{\"host\":\"{0}\",\"key\":\"agent.version\",\"value\":\"TCP_program\",\"id\":3,\"clock\":{1},\"ns\":{2}}}", HOSTNAME, clock, ns);
            
            #endregion stringek

            //Console.WriteLine(agentVersion);
            Console.ReadLine();
            
            string message = "{\"request\":\"agent data\",\"session\":\"2dcf1bf2f6fc1c742812fbbf491e24f2\",\"data\":["+agentHostname+"," + agentVersion + "],\"clock\":"+clock+",\"ns\":351876400}";
            //string message = "ZBXD.. ......{\"request\":\"agent data\",\"session\":\"c30d9504a47c4f7a0f13dced89bdea92\",\"data\":[{\"host\":\"gyakornok_tf_pc\",\"key\":\"agent.hostname\",\"value\":\"gyakornok_tf_pc\",\"id\":1,\"clock\":1624538172,\"ns\":415175300},{\"host\":\"gyakornok_tf_pc\",\"key\":\"agent.ping\",\"value\":\"1\",\"id\":2,\"clock\":1624538172,\"ns\":415179200},{\"host\":\"gyakornok_tf_pc\",\"key\":\"agent.version\",\"value\":\"5.4.1\",\"id\":3,\"clock\":1624538172,\"ns\":415181600},{\"host\":\"gyakornok_tf_pc\",\"key\":\"eventlog[System,,,\"Microsoft - Windows - Kernel - Power\"]\",\"lastlogsize\":10269,\"id\":4,\"clock\":1624538172,\"ns\":469497300},{\"host\":\"gyakornok_tf_pc\",\"key\":\"net.if.list\",\"value\":\"Ethernet                  enabled  - Realtek PCIe GbE Family Controller-WFP Native MAC Layer LightWeight Filter-0000\nEthernet                  enabled  - Realtek PCIe GbE Family Controller-Npcap Packet Driver (NPCAP)-0000\nEthernet                  enabled  - Realtek PCIe GbE Family Controller-VirtualBox NDIS Light-Weight Filter-0000\nEthernet                  enabled  - Realtek PCIe GbE Family Controller-QoS Packet Scheduler-0000\nEthernet                  enabled  - Realtek PCIe GbE Family Controller-WFP 802.3 MAC Layer LightWeight Filter-0000\nEthernet                  enabled  - WAN Miniport (IP)-WFP Native MAC Layer LightWeight Filter-0000\nEthernet                  enabled  - WAN Miniport (IP)-Npcap Packet Driver (NPCAP)-0000\nEthernet                  enabled  - WAN Miniport (IP)-QoS Packet Scheduler-0000\nEthernet                  enabled  - WAN Miniport (IPv6)-WFP Native MAC Layer LightWeight Filter-0000\nEthernet                  enabled  - WAN Miniport (IPv6)-Npcap Packet Driver (NPCAP)-0000\nEthernet                  enabled  - WAN Miniport (IPv6)-QoS Packet Scheduler-0000\nEthernet                  enabled  - WAN Miniport (Network Monitor)-WFP Native MAC Layer LightWeight Filter-0000\nEthernet                  enabled  - WAN Miniport (Network Monitor)-Npcap Packet Driver (NPCAP)-0000\nEthernet                  enabled  - WAN Miniport (Network Monitor)-QoS Packet Scheduler-0000\nEthernet                  unknown  - Microsoft Kernel Debug Network Adapter\nEthernet                  enabled  - Realtek PCIe GbE Family Controller\nEthernet                  unknown  - Bluetooth Device (Personal Area Network)\nEthernet                  enabled  - WAN Miniport (IP)\nEthernet                  enabled  - WAN Miniport (IPv6)\nEthernet                  enabled  - WAN Miniport (Network Monitor)\nEthernet                  unknown  - VirtualBox Host-Only Ethernet Adapter\nPPP                       enabled  - WAN Miniport (PPPOE)\nSoftware Loopback         enabled  127.0.0.1       Software Loopback Interface 1\nIEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-WFP Native MAC Layer LightWeight Filter-0000\nIEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-Npcap Packet Driver (NPCAP) (WiFi version)-0000\nIEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-Virtual WiFi Filter Driver-0000\nIEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-Native WiFi Filter Driver-0000\nIEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-Npcap Packet Driver (NPCAP)-0000\nIEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-VirtualBox NDIS Light-Weight Filter-0000\nIEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-QoS Packet Scheduler-0000\nIEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-WFP 802.3 MAC Layer LightWeight Filter-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-WFP Native MAC Layer LightWeight Filter-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-Npcap Packet Driver (NPCAP) (WiFi version)-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-Native WiFi Filter Driver-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-Npcap Packet Driver (NPCAP)-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-VirtualBox NDIS Light-Weight Filter-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-QoS Packet Scheduler-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-WFP 802.3 MAC Layer LightWeight Filter-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-WFP Native MAC Layer LightWeight Filter-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-Npcap Packet Driver (NPCAP) (WiFi version)-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-Native WiFi Filter Driver-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-Npcap Packet Driver (NPCAP)-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-VirtualBox NDIS Light-Weight Filter-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-QoS Packet Scheduler-0000\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-WFP 802.3 MAC Layer LightWeight Filter-0000\nIEEE 802.11 Wireless      enabled  192.168.1.102   Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter\nIEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2\nTunnel type encapsulation unknown  - Microsoft Teredo Tunneling Adapter\nTunnel type encapsulation unknown  - Microsoft IP-HTTPS Platform Adapter\nTunnel type encapsulation unknown  - Microsoft 6to4 Adapter\nTunnel type encapsulation enabled  - WAN Miniport (SSTP)\nTunnel type encapsulation enabled  - WAN Miniport (IKEv2)\nTunnel type encapsulation enabled  - WAN Miniport (L2TP)\nTunnel type encapsulation enabled  - WAN Miniport (PPTP)\n\",\"id\":5,\"clock\":1624538172,\"ns\":488188300},{\"host\":\"gyakornok_tf_pc\",\"key\":\"perf_counter[\\234(_Total)\\1402]\",\"value\":\"0.004993\",\"id\":6,\"clock\":1624538173,\"ns\":503320500},{\"host\":\"gyakornok_tf_pc\",\"key\":\"perf_counter[\\234(_Total)\\1404]\",\"value\":\"0.000000\",\"id\":7,\"clock\":1624538174,\"ns\":517149100},{\"host\":\"gyakornok_tf_pc\",\"key\":\"perf_counter[\\2\\16]\",\"value\":\"37906.076659\",\"id\":8,\"clock\":1624538175,\"ns\":530532000},{\"host\":\"gyakornok_tf_pc\",\"key\":\"perf_counter[\\2\\18]\",\"value\":\"81786.979021\",\"id\":9,\"clock\":1624538176,\"ns\":550000000},{\"host\":\"gyakornok_tf_pc\",\"key\":\"perf_counter[\\2\\250]\",\"value\":\"3257.000000\",\"id\":10,\"clock\":1624538176,\"ns\":552477800},{\"host\":\"gyakornok_tf_pc\",\"key\":\"proc.num[]\",\"value\":\"271\",\"id\":11,\"clock\":1624538176,\"ns\":557789600},{\"host\":\"gyakornok_tf_pc\",\"key\":\"system.cpu.load[percpu,avg1]\",\"value\":\"0.075000\",\"id\":12,\"clock\":1624538176,\"ns\":557822300},{\"host\":\"gyakornok_tf_pc\",\"key\":\"system.cpu.load[percpu,avg5]\",\"value\":\"0.075000\",\"id\":13,\"clock\":1624538176,\"ns\":557827300},{\"host\":\"gyakornok_tf_pc\",\"key\":\"system.localtime[utc]\",\"value\":\"1624538176\",\"id\":14,\"clock\":1624538176,\"ns\":557834000},{\"host\":\"gyakornok_tf_pc\",\"key\":\"system.run[ipconfig | findstr IPv4 | sort]\",\"value\":\"Unsupported item key.\",\"state\":1,\"id\":15,\"clock\":1624538176,\"ns\":558760900},{\"host\":\"gyakornok_tf_pc\",\"key\":\"system.run[systeminfo,]\",\"value\":\"Unsupported item key.\",\"state\":1,\"id\":16,\"clock\":1624538176,\"ns\":560000000},{\"host\":\"gyakornok_tf_pc\",\"key\":\"system.swap.size[,free]\",\"value\":\"1161785344\",\"id\":17,\"clock\":1624538176,\"ns\":560031800},{\"host\":\"gyakornok_tf_pc\",\"key\":\"system.swap.size[,total]\",\"value\":\"5637144576\",\"id\":18,\"clock\":1624538176,\"ns\":560037600},{\"host\":\"gyakornok_tf_pc\",\"key\":\"system.uname\",\"value\":\"Windows DESKTOP-H8HU6UC 10.0.19042 Microsoft Windows 10 Pro x64\",\"id\":19,\"clock\":1624538176,\"ns\":646712600},{\"host\":\"gyakornok_tf_pc\",\"key\":\"system.uptime\",\"value\":\"187111\",\"id\":20,\"clock\":1624538176,\"ns\":649014800},{\"host\":\"gyakornok_tf_pc\",\"key\":\"vm.memory.size[free]\",\"value\":\"905641984\",\"id\":21,\"clock\":1624538176,\"ns\":649024600},{\"host\":\"gyakornok_tf_pc\",\"key\":\"vm.memory.size[total]\",\"value\":\"8477478912\",\"id\":22,\"clock\":1624538176,\"ns\":649027300}],\"clock\":1624538177,\"ns\":777134100}";
            //string message = "{\"request\":\"active checks\",\"host\":\"gyakornok_tf_app\"}";

            //string message = "{\"request\":\"agent data\",\"session\":\"7905fd85856fa1804dd9f27988d2e0b2\",\"data\":[{\"host\":\"gyakornok_tf_app\",\"key\":\"agent.hostname\",\"value\":\"gyakornok_tf_app\",\"id\":1,\"clock\":1624960474,\"ns\":121725800},{\"host\":\"gyakornok_tf_app\",\"key\":\"agent.ping\",\"value\":\"1\",\"id\":2,\"clock\":1624960474,\"ns\":121730600},{\"host\":\"gyakornok_tf_app\",\"key\":\"agent.version\",\"value\":\"5.4.1\",\"id\":3,\"clock\":1624960474,\"ns\":121733200},{\"host\":\"gyakornok_tf_app\",\"key\":\"eventlog[System,,,\"Microsoft-Windows-Kernel-Power\"]\",\"value\":\"A felhaszn..l..i ..zemm..d.. folyamat megpr..b..lta m..dos..tani a rendszer..llapotot a SetSuspendState vagy a SetSystemPowerState API h..v..s..val.\",\"lastlogsize\":10815,\"timestamp\":1624900534,\"source\":\"Microsoft-Windows-Kernel-Power\",\"severity\":1,\"eventid\":187,\"id\":4,\"clock\":1624960474,\"ns\":186056600},{\"host\":\"gyakornok_tf_app\",\"key\":\"eventlog[System,,,\"Microsoft-Windows-Kernel-Power\"]\",\"value\":\"A rendszer alv.. ..llapotba l..p.\r\"\r\"Alv.. ..llapot oka: Application API\",\"lastlogsize\":10816,\"timestamp\":1624900537,\"source\":\"Microsoft-Windows-Kernel-Power\",\"severity\":1,\"eventid\":42,\"id\":5,\"clock\":1624960474,\"ns\":187095300},{\"host\":\"gyakornok_tf_app\",\"key\":\"eventlog[System,,,\"Microsoft-Windows-Kernel-Power\"]\",\"value\":\"A rendszer visszat..rt alv.. ..llapotb..l.\",\"lastlogsize\":10817,\"timestamp\":1624900541,\"source\":\"Microsoft-Windows-Kernel-Power\",\"severity\":1,\"eventid\":107,\"id\":6,\"clock\":1624960474,\"ns\":188426200},{\"host\":\"gyakornok_tf_app\",\"key\":\"eventlog[System,,,\"Microsoft-Windows-Kernel-Power\"]\",\"lastlogsize\":10844,\"id\":7,\"clock\":1624960474,\"ns\":220525300},{\"host\":\"gyakornok_tf_app\",\"key\":\"net.if.list\",\"value\":\"Ethernet                  enabled  - Realtek PCIe GbE Family Controller-WFP Native MAC Layer LightWeight Filter-0000\"Ethernet                  enabled  - Realtek PCIe GbE Family Controller-Npcap Packet Driver (NPCAP)-0000\"Ethernet                  enabled  - Realtek PCIe GbE Family Controller-VirtualBox NDIS Light-Weight Filter-0000\"Ethernet                  enabled  - Realtek PCIe GbE Family Controller-QoS Packet Scheduler-0000\"Ethernet                  enabled  - Realtek PCIe GbE Family Controller-WFP 802.3 MAC Layer LightWeight Filter-0000\"Ethernet                  enabled  - WAN Miniport (IP)-WFP Native MAC Layer LightWeight Filter-0000\"Ethernet                  enabled  - WAN Miniport (IP)-Npcap Packet Driver (NPCAP)-0000\"Ethernet                  enabled  - WAN Miniport (IP)-QoS Packet Scheduler-0000\"Ethernet                  enabled  - WAN Miniport (IPv6)-WFP Native MAC Layer LightWeight Filter-0000\"Ethernet                  enabled  - WAN Miniport (IPv6)-Npcap Packet Driver (NPCAP)-0000\"Ethernet                  enabled  - WAN Miniport (IPv6)-QoS Packet Scheduler-0000\"Ethernet                  enabled  - WAN Miniport (Network Monitor)-WFP Native MAC Layer LightWeight Filter-0000\"Ethernet                  enabled  - WAN Miniport (Network Monitor)-Npcap Packet Driver (NPCAP)-0000\"Ethernet                  enabled  - WAN Miniport (Network Monitor)-QoS Packet Scheduler-0000\"Ethernet                  unknown  - Microsoft Kernel Debug Network Adapter\"Ethernet                  enabled  - Realtek PCIe GbE Family Controller\"Ethernet                  unknown  - Bluetooth Device (Personal Area Network)\"Ethernet                  enabled  - WAN Miniport (IP)\"Ethernet                  enabled  - WAN Miniport (IPv6)\"Ethernet                  enabled  - WAN Miniport (Network Monitor)\"Ethernet                  unknown  - VirtualBox Host-Only Ethernet Adapter\"PPP                       enabled  - WAN Miniport (PPPOE)\"Software Loopback         enabled  127.0.0.1       Software Loopback Interface 1\"IEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-WFP Native MAC Layer LightWeight Filter-0000\"IEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-Npcap Packet Driver (NPCAP) (WiFi version)-0000\"IEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-Virtual WiFi Filter Driver-0000\"IEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-Native WiFi Filter Driver-0000\"IEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-Npcap Packet Driver (NPCAP)-0000\"IEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-VirtualBox NDIS Light-Weight Filter-0000\"IEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-QoS Packet Scheduler-0000\"IEEE 802.11 Wireless      enabled  - Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC-WFP 802.3 MAC Layer LightWeight Filter-0000\"IEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-WFP Native MAC Layer LightWeight Filter-0000\"IEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-Npcap Packet Driver (NPCAP) (WiFi version)-0000\"IEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-Native WiFi Filter Driver-0000\"IEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-Npcap Packet Driver (NPCAP)-0000\"IEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-VirtualBox NDIS Light-Weight Filter-0000\"IEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-QoS Packet Scheduler-0000\"IEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter-WFP 802.3 MAC Layer LightWeight Filter-0000\"IEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-WFP Native MAC Layer LightWeight Filter-0000\"IEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-Npcap Packet Driver (NPCAP) (WiFi version)-0000\"IEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-Native WiFi Filter Driver-0000\"IEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-Npcap Packet Driver (NPCAP)-0000\"IEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-VirtualBox NDIS Light-Weight Filter-0000\"IEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-QoS Packet Scheduler-0000\"IEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2-WFP 802.3 MAC Layer LightWeight Filter-0000\"IEEE 802.11 Wireless      enabled  192.168.1.100   Realtek 8821AE Wireless LAN 802.11ac PCI-E NIC\"IEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter\"IEEE 802.11 Wireless      enabled  - Microsoft Wi-Fi Direct Virtual Adapter #2\"Tunnel type encapsulation unknown  - Microsoft Teredo Tunneling Adapter\"Tunnel type encapsulation unknown  - Microsoft IP-HTTPS Platform Adapter\"Tunnel type encapsulation unknown  - Microsoft 6to4 Adapter\"Tunnel type encapsulation enabled  - WAN Miniport (SSTP)\"Tunnel type encapsulation enabled  - WAN Miniport (IKEv2)\"Tunnel type encapsulation enabled  - WAN Miniport (L2TP)\"Tunnel type encapsulation enabled  - WAN Miniport (PPTP)\"\",\"id\":8,\"clock\":1624960474,\"ns\":238679600},{\"host\":\"gyakornok_tf_app\",\"key\":\"perf_counter[\\234(_Total)\\1402]\",\"value\":\"0.019797\",\"id\":9,\"clock\":1624960475,\"ns\":252629400},{\"host\":\"gyakornok_tf_app\",\"key\":\"perf_counter[\\234(_Total)\\1404]\",\"value\":\"0.006441\",\"id\":10,\"clock\":1624960476,\"ns\":260388900},{\"host\":\"gyakornok_tf_app\",\"key\":\"perf_counter[\\2\\16]\",\"value\":\"17219.955967\",\"id\":11,\"clock\":1624960477,\"ns\":276593400},{\"host\":\"gyakornok_tf_app\",\"key\":\"perf_counter[\\2\\18]\",\"value\":\"113342.473095\",\"id\":12,\"clock\":1624960478,\"ns\":288443500},{\"host\":\"gyakornok_tf_app\",\"key\":\"perf_counter[\\2\\250]\",\"value\":\"2906.000000\",\"id\":13,\"clock\":1624960478,\"ns\":291858000},{\"host\":\"gyakornok_tf_app\",\"key\":\"proc.num[]\",\"value\":\"247\",\"id\":14,\"clock\":1624960478,\"ns\":299047600},{\"host\":\"gyakornok_tf_app\",\"key\":\"system.cpu.load[percpu,avg1]\",\"value\":\"0.000000\",\"id\":15,\"clock\":1624960478,\"ns\":299154200},{\"host\":\"gyakornok_tf_app\",\"key\":\"system.cpu.load[percpu,avg5]\",\"value\":\"0.000000\",\"id\":16,\"clock\":1624960478,\"ns\":299159800},{\"host\":\"gyakornok_tf_app\",\"key\":\"system.localtime[utc]\",\"value\":\"1624960478\",\"id\":17,\"clock\":1624960478,\"ns\":299168100},{\"host\":\"gyakornok_tf_app\",\"key\":\"system.run[ipconfig | findstr IPv4 | sort]\",\"value\":\"Unsupported item key.\",\"state\":1,\"id\":18,\"clock\":1624960478,\"ns\":300200600},{\"host\":\"gyakornok_tf_app\",\"key\":\"system.run[systeminfo,]\",\"value\":\"Unsupported item key.\",\"state\":1,\"id\":19,\"clock\":1624960478,\"ns\":301352900},{\"host\":\"gyakornok_tf_app\",\"key\":\"system.swap.size[,free]\",\"value\":\"1971662848\",\"id\":20,\"clock\":1624960478,\"ns\":301386100},{\"host\":\"gyakornok_tf_app\",\"key\":\"system.swap.size[,total]\",\"value\":\"5637144576\",\"id\":21,\"clock\":1624960478,\"ns\":301394100},{\"host\":\"gyakornok_tf_app\",\"key\":\"system.uname\",\"value\":\"Windows DESKTOP-H8HU6UC 10.0.19042 Microsoft Windows 10 Pro x64\",\"id\":22,\"clock\":1624960478,\"ns\":417437700},{\"host\":\"gyakornok_tf_app\",\"key\":\"system.uptime\",\"value\":\"609411\",\"id\":23,\"clock\":1624960478,\"ns\":420357000},{\"host\":\"gyakornok_tf_app\",\"key\":\"vm.memory.size[free]\",\"value\":\"2073059328\",\"id\":24,\"clock\":1624960478,\"ns\":420366000},{\"host\":\"gyakornok_tf_app\",\"key\":\"vm.memory.size[total]\",\"value\":\"8477478912\",\"id\":25,\"clock\":1624960478,\"ns\":420369200}],\"clock\":1624960479,\"ns\":458639800}";

            byte[] teszt = header(message);
            
            foreach (var item in teszt)
            {
                Console.WriteLine(item);
            }

           
            try {
                Connect(ZABBIX_NAME, teszt, message);
            }
            catch(Exception e)
            { 
                Console.WriteLine("HIBA"); }
            // TODO: TCP kapcsolat meghatározása  (TASK LIST)
            // TODO: TcpClient példakód átmásolása https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient?view=net-5.0


            // ZABBIX request string megépítése (1624524243) epoch időkonverzió - string.Format!!
            // Végtelen ciklus 20s-ként megszólítani a zabbix szervert (hibakezelés)
            // válasz kiíratása konzolra
            // Rövid függvények


        }
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        
        
        public static byte[] header(string data)
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
    
        static void Connect(String server, byte[] data, string message)
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = 10051;
                TcpClient client = new TcpClient(server, port);

                // Translate the passed message into ASCII and store it as a Byte array.
                //Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

              

                
                foreach (var item in data)
                {
                    //Console.WriteLine(item);
                }
                
                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);

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

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }
    }
}
