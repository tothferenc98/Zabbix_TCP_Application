using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


// TODO: TCP kapcsolat meghatározása  (TASK LIST)
// TODO: TcpClient példakód átmásolása https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient?view=net-5.0
// ZABBIX request string megépítése (1624524243) epoch időkonverzió - string.Format!!
// Végtelen ciklus 20s-ként megszólítani a zabbix szervert (hibakezelés)
// válasz kiíratása konzolra
// Rövid függvények

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

                #endregion stringek

                //Console.WriteLine(agentVersion);
                //Console.ReadLine();

                string message = "{\"request\":\"agent data\",\"session\":\"2dcf1bf2f6fc1c742812fbbf491e24f2\",\"data\":[" + agentHostname + "," + agentPing + "," + agentVersion + "],\"clock\":" + clock + ",\"ns\":" + ns + "}";
                //string message = "{\"request\":\"active checks\",\"host\":\"gyakornok_tf_app\"}";

                
                byte[] packet = Packet(message);

                foreach (var item in packet)
                {
                    //Console.WriteLine(item);
                }


                try
                {
                    Connect(ZABBIX_NAME, packet, message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("HIBA: {0}",e);
                }

             
                System.Threading.Thread.Sleep(CONNECT_DELAY*1000);


                //Console.WriteLine("Várakozás, folytatáshoz nyomj entert");
                //Console.ReadKey();
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
    
        static void Connect(String server, byte[] data, string message)
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
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }
    }
}
