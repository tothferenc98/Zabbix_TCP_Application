using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Newtonsoft.Json;
using log4net;

namespace Zabbix_TCP_Application
{
    partial class Utility
    {
        #region konstansok
        public static string HOSTNAME = Properties.Settings.Default.HOSTNAME;
        public static string ZABBIX_NAME = Properties.Settings.Default.ZABBIX_NAME;
        public static int ZABBIX_PORT = Properties.Settings.Default.ZABBIX_PORT; 
        public static int CONNECT_DELAY = Properties.Settings.Default.CONNECT_DELAY;
        public static int BUFFER_SIZE = Properties.Settings.Default.BUFFER_SIZE;
        public static string PROXY_NAME = Properties.Settings.Default.PROXY_NAME;
        public static string PROXY_VERSION = Properties.Settings.Default.PROXY_VERSION;
        public static Encoding ENCODING = Encoding.ASCII;
        #endregion konstansok

        private static readonly ILog ByteLog = LogManager.GetLogger("ByteLog");
        private static readonly ILog JsonLog = LogManager.GetLogger("JsonLog");
        private static readonly ILog Log = LogManager.GetLogger("Log");


        public static string MakeAgentDataMessage(AgentCommunication.ResponseJsonObject jsonObject)
        {
            #region változók + szótár
            var clock = DateTimeOffset.Now.ToUnixTimeSeconds();
            var rand = new Random();
            int ns = rand.Next(000000001, 999999999);
            Dictionary<string, string> dictKeyValue = new Dictionary<string, string>();
            dictKeyValue.Add("agent.hostname", HOSTNAME);
            dictKeyValue.Add("agent.ping", "1");
            dictKeyValue.Add("agent.version", "TCP_program");
            dictKeyValue.Add("net.if.list", AgentCommunication.ZabbixAgentUtility.GetProcessNumber().ToString());
            dictKeyValue.Add("perf_counter[\\234(_Total)\\1402]", AgentCommunication.ZabbixAgentUtility.GetAvgDiskReadQueueLength());
            dictKeyValue.Add("perf_counter[\\234(_Total)\\1404]", AgentCommunication.ZabbixAgentUtility.GetAvgDiskWriteQueueLength());
            dictKeyValue.Add("perf_counter[\\2\\16]", AgentCommunication.ZabbixAgentUtility.GetDiskReadsSec());
            dictKeyValue.Add("perf_counter[\\2\\18]", AgentCommunication.ZabbixAgentUtility.GetDiskWritesSec());
            dictKeyValue.Add("perf_counter[\\2\\250]", AgentCommunication.ZabbixAgentUtility.GetPerformanceCounter2_250().ToString());
            dictKeyValue.Add("proc.num[]", AgentCommunication.ZabbixAgentUtility.GetProcessNumber().ToString());
            dictKeyValue.Add("system.cpu.load[percpu,avg1]", "0.000000");
            dictKeyValue.Add("system.cpu.load[percpu,avg5]", "0.000000");
            dictKeyValue.Add("system.localtime[utc]", clock.ToString());
            dictKeyValue.Add("system.swap.size[,free]", AgentCommunication.ZabbixAgentUtility.GetAvailableVirtualMemoryInBytes().ToString());
            dictKeyValue.Add("system.swap.size[,total]", AgentCommunication.ZabbixAgentUtility.GetTotalVirtualMemoryInBytes().ToString());
            dictKeyValue.Add("system.uname", AgentCommunication.ZabbixAgentUtility.GetSystemUname());
            dictKeyValue.Add("system.uptime", AgentCommunication.ZabbixAgentUtility.GetUpTime());
            dictKeyValue.Add("vm.memory.size[free]", AgentCommunication.ZabbixAgentUtility.GetAvailableMemoryInBytes().ToString());
            dictKeyValue.Add("vm.memory.size[total]", AgentCommunication.ZabbixAgentUtility.GetTotalMemoryInBytes().ToString());
            #endregion változók

            string secondResponseData = String.Empty;
            AgentCommunication.RequestJsonObject requestJsonObject = new AgentCommunication.RequestJsonObject();
            try
            {
                if (jsonObject.data.Count > 0)
                {
                    int id = 1;
                    List<AgentCommunication.RequestJsonData> listRequestJsonData = new List<AgentCommunication.RequestJsonData>();
                    foreach (var item in jsonObject.data) //TODO: linQ
                    {
                        if (dictKeyValue.ContainsKey(item.key))
                        {
                            listRequestJsonData.Add(new AgentCommunication.RequestJsonData()
                            {
                                key = item.key,
                                value = dictKeyValue[item.key],
                                id = id,
                                clock = Convert.ToInt32(clock),
                                ns = ns
                            });
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
                    Log.Error("MakeAgentDataMessage: Kaptunk választ, de a json data része üres ");
                    return String.Empty;
                }
                return secondResponseData;
            }
            catch (Exception e)
            {
                Log.Error("MakeAgentDataMessage.Exception: ", e);
                return String.Empty;

            }

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
                    byte[] resultWithoutHeader;
                    if (result.Length > 13)
                    {
                        byte[] tempResultWithoutHeader = new byte[result.Length - 13];
                        resultWithoutHeader = tempResultWithoutHeader;
                    }
                    else
                    {
                        JsonLog.Error("Rövidebb a válasz, mint 13 byte (header hossza)");
                        return String.Empty;
                    }
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

                    JsonLog.InfoFormat("Received: {0}", resultJson);
                    Console.WriteLine("Received: {0}\n", resultJson);
                    // TODO: BitConverter (GetBytes)
                    if (((byte)(resultWithoutHeader.Length & 0xFF)).Equals(headerOriginal[5]) && ((byte)((resultWithoutHeader.Length >> 8) & 0xFF)).Equals(headerOriginal[6]) && ((byte)((resultWithoutHeader.Length >> 16) & 0xFF)).Equals(headerOriginal[7]) && ((byte)((resultWithoutHeader.Length >> 24) & 0xFF)).Equals(headerOriginal[8]))
                    {// Csomaghossz ellenőrzés
                        return resultJson;
                    }
                    else
                    {
                        JsonLog.ErrorFormat("ConnectJson: Nem érkezett meg a teljes csomag (A (byte)resultWithoutHeader.Length értéke: {0}, ehelyett a headerOriginal[5] értéke: {1}) Teljes válasz: {2}", (byte)resultWithoutHeader.Length, headerOriginal[5], resultJson);
                        return String.Empty;
                    }

                }
                else
                {
                    JsonLog.WarnFormat("ConnectJson: A csatlakozás során hiba lépett fel!");
                    return String.Empty;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("ConnectJson.Exception: ", e);
                JsonLog.Error("ConnectJson.Exception: ", e);
                return String.Empty;
            }
        }

        public static byte[] Connect(byte[] data)
        {
            byte[] error = new byte[0];
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

                // Hexadecimális értékek logolása
                ByteLog.InfoFormat("Sent:     {0}", String.Join(String.Empty, data.Select(d => String.Format("{0:X}", d))));

                try
                {
                    // Receive the TcpServer.response.

                    // Buffer to store the response bytes.
                    byte[] byteArrayData2 = new Byte[BUFFER_SIZE]; // TODO:Meg tudja mondani a stream, hogy mennyire van szükség? (availablebyte/readavailable)
                    //visszaolvasás pufferére másik data deklarálása
                    // Read the first batch of the TcpServer response bytes.
                    int bytes = stream.Read(byteArrayData2, 0, byteArrayData2.Length);
                    byteArrayData2 = TrimEnd(byteArrayData2);
                    //arraycopy

                    // Hexadecimális értékek logolása

                    ByteLog.InfoFormat("Received: {0}", String.Join(String.Empty, byteArrayData2.Select(d => String.Format("{0:X}", d))));
                    // Close everything.
                    stream.Close();
                    client.Close();
                    return byteArrayData2;
                }
                catch (Exception e)
                {
                    ByteLog.Error("Hiba a Connect válasz részénél: ", e);
                    return error;
                }


            }
            catch (ArgumentNullException e)
            {
                ByteLog.Error("Hiba a Connect küldés részénél: ArgumentNullException:", e);
                Console.WriteLine("Connect.ArgumentNullException: ", e);
                return error;
            }
            catch (SocketException e)
            {
                ByteLog.Error("Hiba a Connect küldés részénél: SocketException: ", e);
                Console.WriteLine("Connect.SocketException: ", e);
                return error;
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

        public static byte[] TrimEnd(byte[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);

            Array.Resize(ref array, lastIndex + 1);

            return array;
        }
    }
}
