using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Zabbix_TCP_Application
{
    partial class Utility
    {
        public static string ProcessingAndRequest(ProxyCommunication.ResponseJsonObject jsonObject, Stopwatch stopWatch)
        {
            WebPageGetLog.Debug("Start");
            List<ProxyCommunication.ItemCheckData> itemCheckDataObjectList = new List<ProxyCommunication.ItemCheckData>();
            MakeItemCheckDataObject(jsonObject, itemCheckDataObjectList);

            foreach (var item in itemCheckDataObjectList)
            { Console.WriteLine(item.ToString()); }
            Console.WriteLine();

            return MakeRequest(itemCheckDataObjectList, stopWatch);
        }

        public static void MakeItemCheckDataObject(ProxyCommunication.ResponseJsonObject jsonObject, List<ProxyCommunication.ItemCheckData> itemCheckDataObjectList)
        {
            foreach (var item in jsonObject.items.data)
            {
                string hostname = String.Empty;
                string hoststatus = String.Empty;
                foreach (var hostData in jsonObject.hosts.data) //Hostname logoláshoz
                {
                    if (item[getPositionItemsHostid(jsonObject)].Equals(hostData[getPositionHostsHostId(jsonObject)]))
                    {
                        hostname = hostData[getPositionHostsHostName(jsonObject)];
                        hoststatus = hostData[getPositionHostsHostStatus(jsonObject)];
                    }
                }

                itemCheckDataObjectList.Add(new ProxyCommunication.ItemCheckData()
                {
                    ItemId = Convert.ToInt32(item[getPositionItemsItemId(jsonObject)]),
                    HostId = Convert.ToInt32(item[getPositionItemsHostid(jsonObject)]),
                    HostName = hostname,
                    HostStatus = hoststatus,
                    Key = item[getPositionItemsKey(jsonObject)],
                });
            }
        }

        public static string MakeRequest(List<ProxyCommunication.ItemCheckData> itemCheckDataObjectList, Stopwatch stopWatch)
        {
            string secondResponseData = String.Empty;
            ProxyCommunication.RequestJsonObject requestJsonObject = new ProxyCommunication.RequestJsonObject();
            try
            {
                if (itemCheckDataObjectList.Count > 0)
                {
                    Guid guid = Guid.NewGuid();
                    requestJsonObject.session = guid.ToString().Replace("-", "");

                    List<string> listUnfinishedTasks = new List<string>();

                    Task exitApplicationTask = new Task(() => ExitApplication(stopWatch, listUnfinishedTasks));
                    exitApplicationTask.Start();

                    List<ProxyCommunication.RequestJsonData> listRequestJsonData = new List<ProxyCommunication.RequestJsonData>();
                    
                    Parallel.ForEach(itemCheckDataObjectList, async item =>
                    {
                            string keyName = item.Key;

                            if (item.Key.Contains('['))
                                keyName = item.Key.Substring(0, item.Key.IndexOf('['));

                            switch (keyName)
                            {
                                case "web.page.get":
                                    WebPageGet(item, listRequestJsonData, listUnfinishedTasks);
                                    break;
                                case "net.tcp.port":
                                    await NetTcpPort(item, listRequestJsonData, listUnfinishedTasks);
                                    break;
                                default:
                                    Log.WarnFormat("{0} A feldolgozás sikertelen. (Hostname: {1})", item.Key, item.HostName);
                                    break;
                            }
                    });

                    requestJsonObject.historydata = listRequestJsonData;
                    requestJsonObject.clock = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
                    requestJsonObject.ns = getNanosec();
                    requestJsonObject.version = PROXY_VERSION;

                    string serializedJson = JsonConvert.SerializeObject(requestJsonObject);

                    if (listRequestJsonData.Count > 0)
                        secondResponseData = ConnectJson(serializedJson);
                    else
                        JsonLog.WarnFormat("Üres a küldeni kívánt lista: {0}", serializedJson);
                }
                else
                {
                    Log.Error("ProcessingAndRequest: Kaptunk választ, de a json data része üres ");
                    return String.Empty;
                }
                return secondResponseData;
            }
            catch (Exception e)
            {
                Log.Error("ProcessingAndRequest.Exception: ", e);
                return String.Empty;

            }
        }
        
        public static void WebPageGet(ProxyCommunication.ItemCheckData item, List<ProxyCommunication.RequestJsonData> listRequestJsonData, List<string> listUnfinishedTasks)
        {
            if (item.HostStatus.Equals("0"))
            {
                listUnfinishedTasks.Add(item.Key);
                string value = WebPageGetProcessing(item.Key);
                if (!value.Equals(String.Empty))
                {
                    listRequestJsonData.Add(new ProxyCommunication.RequestJsonData()
                    {
                        itemid = item.ItemId,
                        clock = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
                        ns = getNanosec(),
                        value = value,
                        id = listRequestJsonData.Count+1
                    });
                }
                else
                {
                    Log.WarnFormat("{0} A letöltés sikertelen. (Hostname: {1})", item.Key, item.HostName);
                }
                listUnfinishedTasks.Remove(item.Key);
            }
            else
            {
                Log.DebugFormat("Nem 0 a host státusza: key: {0}, status: {1}", item.Key, item.HostStatus);
            }
        }

        public static string WebPageGetProcessing(string key)
        {
            string ip=String.Empty;
            int port=0;
            try
            {
                string key2 = key.Substring(key.IndexOf('[') + 1, key.IndexOf(']') - key.IndexOf('[') - 1);
                var splitted = key2.Split(',');
                ip = splitted[0];
                port = Convert.ToInt32(splitted[2]);
            }
            catch (Exception)
            {
                Log.WarnFormat("Hiba a {0} WebPageGetProcessing-ben a key és ip kiszedésénél (alapértelmezett: web.page.get[ip,,port])", key);
                return "";
            }
            
            //Console.WriteLine(port);
            try
            {
                string downloadedString = WebPageGetConnectJson(ip, port);
                //Console.WriteLine(downloadedString);
                return downloadedString;
            }
            catch (Exception e)
            {
                //Console.WriteLine("sikertelen");
                Log.Debug("WebPageGetProcessing: ", e);
                WebPageGetLog.WarnFormat("WebPageGetProcessing: ", e);
                return "";
            }
        }
        

        public static string WebPageGetConnectJson(string name, int webpagePort)
        {
            try
            {
                string json = String.Format(@"GET HTTP/1.1");
                byte[] jsonBytes = ENCODING.GetBytes(json);
                WebPageGetLog.InfoFormat("WebPageGetConnectJson: Sent: {0}, web.page.get[{1},,{2}]", json, name, webpagePort);

                byte[] jsonBytesRespond = WebPageGetConnect(jsonBytes, name, webpagePort, 1);

                if (jsonBytesRespond.SequenceEqual(new byte[1] { 1 }))
                    jsonBytesRespond = WebPageGetConnect(jsonBytes, name, webpagePort, 2);

                if (jsonBytesRespond.SequenceEqual(new byte[1] { 1 }))
                    jsonBytesRespond = WebPageGetConnect(jsonBytes, name, webpagePort, 3);


                if (!jsonBytesRespond.SequenceEqual(new byte[0]))
                {
                    string resultJson = ENCODING.GetString(jsonBytesRespond, 0, jsonBytesRespond.Length);
                    WebPageGetLog.InfoFormat("WebPageGetConnectJson: web.page.get[{1},,{2}] Received: {0}", resultJson, name, webpagePort);
                    if (resultJson.Contains("HTTP/1.1 200 OK"))
                    {
                        resultJson = resultJson.Substring(resultJson.IndexOf('{'));
                        return resultJson;
                    }
                    else
                    {
                        JsonLog.Warn("WebPageGetConnectJson: nem HTTP/1.1 200 OK a válasz");
                        WebPageGetLog.WarnFormat("WebPageGetConnectJson: nem HTTP/1.1 200 OK a válasz. web.page.get[{0},,{1}]", name, webpagePort);
                    }
                }
                return "";
            }
            catch (Exception e)
            {
                Console.WriteLine("Hiba a WebPageGetConnectJson-ben");
                JsonLog.WarnFormat("WebPageGetConnectJson: Hiba: web.page.get[{0},,{1}], {2}", name, webpagePort, e);
                WebPageGetLog.WarnFormat("WebPageGetConnectJson: Hiba: web.page.get[{0},,{1}], {2}", name, webpagePort, e);
                return "";
            }
        }


        public static byte[] WebPageGetConnect(byte[] data, string name, int webpagePort,int numberOfTries)
        {
            byte[] error = new byte[0];
            try
            {
                WebPageGetLog.DebugFormat("WebPageGetConnect: 1. lépés web.page.get[{0},,{1}]", name, webpagePort);
                Stopwatch stopWatchConnect = new Stopwatch();
                stopWatchConnect.Start();

                Int32 port = webpagePort;
                String server = name;
                TcpClient client = new TcpClient(server, port);
                WebPageGetLog.DebugFormat("WebPageGetConnect: 2. lépés web.page.get[{0},,{1}]", name, webpagePort);
                NetworkStream stream = client.GetStream();
                WebPageGetLog.DebugFormat("WebPageGetConnect: 3. lépés web.page.get[{0},,{1}]", name, webpagePort);
                stream.Write(data, 0, data.Length);

                // Hexadecimális értékek logolása
                //WebPageGetLog.InfoFormat("WebPageGetConnect: Sent:     {0}", String.Join(String.Empty, data.Select(d => String.Format("{0:X}", d))));

                try
                {

                    //var throwExceptionTask = Task.Run(() =>
                    //{
                    //    var nested1 = Task.Run(() =>
                    //    {

                    //        Thread.Sleep(25000);
                    //        TimeSpan tsConnect = stopWatchConnect.Elapsed;

                    //        stopWatchConnect.Stop();
                    //        Log.ErrorFormat("Több mint 25 másodperce vár a válaszra");
                    //        throw new IOException();

                    //    });
                    //    nested1.Wait();
                    //});
                    //throwExceptionTask.Wait();

                    List<byte> readedBytesInList = new List<byte>();
                    byte[] byteArrayData2 = new Byte[BUFFER_SIZE];
                    while (true)
                    {
                        // Read the first batch of the TcpServer response bytes.
                        //Thread.Sleep(25000);
                        WebPageGetLog.DebugFormat("WebPageGetConnect: 4. lépés web.page.get[{0},,{1}]", name, webpagePort);
                        int readedBytes = stream.Read(byteArrayData2, 0, byteArrayData2.Length);
                        WebPageGetLog.DebugFormat("WebPageGetConnect: 5. lépés web.page.get[{0},,{1}]", name, webpagePort);
                        byte[] tempByteArrayData = new byte[readedBytes];
                        Array.Copy(byteArrayData2, tempByteArrayData, readedBytes);
                        readedBytesInList.AddRange(tempByteArrayData);

                        if (readedBytes == 0)
                        {
                            WebPageGetLog.DebugFormat("WebPageGetConnect: 6. lépés web.page.get[{0},,{1}]", name, webpagePort);
                            stopWatchConnect.Stop();
                            break;
                        }
                            
                    }
                    byteArrayData2 = readedBytesInList.ToArray();
                    WebPageGetLog.DebugFormat("WebPageGetConnect: 7. lépés web.page.get[{0},,{1}]", name, webpagePort);
                    // Hexadecimális értékek logolása
                    //WebPageGetLog.InfoFormat("WebPageGetConnect: Received: {0}", String.Join(String.Empty, byteArrayData2.Select(d => String.Format("{0:X}", d))));
                    stream.Close();
                    client.Close();
                    return byteArrayData2;
                }
                catch (IOException e)
                {
                    if (!numberOfTries.Equals(3)) //hibás letöltésnél 3x próbálja meg letölteni, harmadiknál errort dob vissza.
                    {
                        ByteLog.WarnFormat("WebPageGetConnect: belső try-ban hiba ({3}. IOException): web.page.get[{0},,{1}]   {2}", name, webpagePort, e, numberOfTries);
                        WebPageGetLog.WarnFormat("WebPageGetConnect: belső try-ban hiba ({3}. IOException): web.page.get[{0},,{1}]   {2}", name, webpagePort, e, numberOfTries);
                        return new byte[1] { 1 };
                    }
                    else
                    {
                        ByteLog.ErrorFormat("WebPageGetConnect: belső try-ban hiba ({3}. IOException): web.page.get[{0},,{1}]   {2}", name, webpagePort, e, numberOfTries);
                        WebPageGetLog.ErrorFormat("WebPageGetConnect: belső try-ban hiba ({3}. IOException): web.page.get[{0},,{1}]   {2}", name, webpagePort, e, numberOfTries);
                        return error;
                    }
                }
                catch (Exception e)
                {
                    ByteLog.WarnFormat("WebPageGetConnect: belső try-ban hiba ({3}. Exception): web.page.get[{0},,{1}]   {2}", name, webpagePort, e, numberOfTries);
                    WebPageGetLog.WarnFormat("WebPageGetConnect: belső try-ban hiba ({3}. Exception): web.page.get[{0},,{1}]   {2}", name, webpagePort, e, numberOfTries);
                    return error;
                }
            }
            catch (ArgumentNullException e)
            {
                ByteLog.WarnFormat("WebPageGetConnect: hiba ({3}. ArgumentNullException): web.page.get[{0},,{1}]: {2}", name, webpagePort, e, numberOfTries);
                WebPageGetLog.WarnFormat("WebPageGetConnect: hiba ({3}. ArgumentNullException): web.page.get[{0},,{1}]: {2}", name, webpagePort, e, numberOfTries);
                return error;
            }
            catch (SocketException e)
            {
                ByteLog.WarnFormat("WebPageGetConnect: hiba ({3}. SocketException):  web.page.get[{0},,{1}]:  {2}", name, webpagePort, e, numberOfTries);
                WebPageGetLog.WarnFormat("WebPageGetConnect ({3}. SocketException): hiba a küldés részénél:  web.page.get[{0},,{1}]:  {2}", name, webpagePort, e, numberOfTries);
                return error;
            }
        }

        public static void ReplaceMacro(ProxyCommunication.ResponseJsonObject jsonObject)
        {
            foreach (var item in jsonObject.items.data)
            {
                List<string> listItem = new List<string>();
                string tempKey = Convert.ToString(item[getPositionItemsKey(jsonObject)]);
                if (tempKey.Contains(Properties.Settings.Default.MACRO_START))
                {
                    string tempReplacedData = getReplaceData(Convert.ToInt32(item[getPositionItemsHostid(jsonObject)]), tempKey, jsonObject);
                    item[getPositionItemsKey(jsonObject)] = Convert.ToString(item[getPositionItemsKey(jsonObject)]).Replace(tempKey, tempReplacedData);
                }

                listItem.Add(Convert.ToString(item[getPositionItemsKey(jsonObject)]));
            } 
        }

        public static async Task NetTcpPort(ProxyCommunication.ItemCheckData item, List<ProxyCommunication.RequestJsonData> listRequestJsonData, List<string> listUnfinishedTasks)
        {
            if (item.HostStatus.Equals("0"))
            {
                listUnfinishedTasks.Add(item.Key);
                string value = await TcpPortTest(item.Key);
                if (value.Equals("0"))
                {
                    Log.WarnFormat("{0} csatlakozás sikertelen. (Hostname: {1})", item.Key, item.HostName);
                }
                listRequestJsonData.Add(new ProxyCommunication.RequestJsonData()
                {
                    itemid = item.ItemId,
                    clock = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
                    ns = getNanosec(),
                    value = value,
                    id = listRequestJsonData.Count + 1
                });
                listUnfinishedTasks.Remove(item.Key);

            }
            else
            {
                Log.DebugFormat("Nem 0 a host státusza: key: {0}, status: {1}", item.Key, item.HostStatus);
            }
        }

        static async Task<string> TcpPortTest(string key)
        {
            key = key.Substring(key.IndexOf('[') + 1, key.IndexOf(']') - key.IndexOf('[') - 1);
            var splitted = key.Split(',');
            string ip = splitted[0];
            int port = Convert.ToInt32(splitted[1]);

            TcpClientWrapper tcpClientWrapper = new TcpClientWrapper();

            try
            {
                await tcpClientWrapper.ConnectAsync(ip,port,TimeSpan.FromSeconds(1));
                //Console.WriteLine("{0}:{1} tested - it's open",ip,port);
                return "1";
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"{ip}:{port} tested - it's not open. Exception: {ex.Message}");
                return "0";
            }
        }
    

        public class TcpException : Exception
        {
            public TcpException(string msg) : base(msg) { }
        }
        public class TcpClientWrapper
        {
            public async Task ConnectAsync(string ip, int port, TimeSpan connectTimeout)
            {
                using (var tcpClient = new TcpClient())
                {
                    var cancelTask = Task.Delay(connectTimeout);
                    var connectTask = tcpClient.ConnectAsync(ip, port);

                    //double await so if cancelTask throws exception, this throws it
                    await Task.WhenAny(connectTask, cancelTask);

                    if (cancelTask.IsCompleted)
                    {
                        //If cancelTask and connectTask both finish at the same time,
                        //we'll consider it to be a timeout. 
                        throw new TcpException("Timed out");
                    }
                };
            }
        }

        public static string getReplaceData(int hostid, string macro, ProxyCommunication.ResponseJsonObject jsonObject)
        {
            foreach (var item in jsonObject.hostmacro.data)
            {
                if (hostid.Equals(Convert.ToInt32(item[getPositionHostmacroHostid(jsonObject)])) && macro.Contains(Convert.ToString(item[getPositionHostmacroMacro(jsonObject)])))
                {
                    string newMacro = macro.Replace(Convert.ToString(item[getPositionHostmacroMacro(jsonObject)]), Convert.ToString(item[getPositionHostmacroValue(jsonObject)]));
                    return newMacro;
                }

            }
            return "";
        }
        public static int getPositionItemsItemId(ProxyCommunication.ResponseJsonObject jsonObject)
        {
            int itemid_pos = 0;
            foreach (var item in jsonObject.items.fields)
            {
                if (item.Equals("itemid"))
                    return itemid_pos;
                else
                    itemid_pos++;
            }
            return -1;
        }
        public static int getPositionItemsHostid(ProxyCommunication.ResponseJsonObject jsonObject)
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
        public static int getPositionItemsKey(ProxyCommunication.ResponseJsonObject jsonObject)
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
        public static int getPositionHostmacroHostid(ProxyCommunication.ResponseJsonObject jsonObject)
        {
            int hostid_pos = 0;
            foreach (var item in jsonObject.hostmacro.fields)
            {
                if (item.Equals("hostid"))
                    return hostid_pos;
                else
                    hostid_pos++;
            }
            return -1;
        }
        public static int getPositionHostmacroMacro(ProxyCommunication.ResponseJsonObject jsonObject)
        {
            int macro_pos = 0;
            foreach (var item in jsonObject.hostmacro.fields)
            {
                if (item.Equals("macro"))
                    return macro_pos;
                else
                    macro_pos++;
            }
            return -1;
        }
        public static int getPositionHostmacroValue(ProxyCommunication.ResponseJsonObject jsonObject)
        {
            int value_pos = 0;
            foreach (var item in jsonObject.hostmacro.fields)
            {
                if (item.Equals("value"))
                    return value_pos;
                else
                    value_pos++;
            }
            return -1;
        }
        public static int getPositionHostsHostId(ProxyCommunication.ResponseJsonObject jsonObject)
        {
            int hostid_pos = 0;
            foreach (var item in jsonObject.hosts.fields)
            {
                if (item.Equals("hostid"))
                    return hostid_pos;
                else
                    hostid_pos++;
            }
            return -1;
        }
        public static int getPositionHostsHostName(ProxyCommunication.ResponseJsonObject jsonObject)
        {
            int hostname_pos = 0;
            foreach (var item in jsonObject.hosts.fields)
            {
                if (item.Equals("name"))
                    return hostname_pos;
                else
                    hostname_pos++;
            }
            return -1;
        }
        public static int getPositionHostsHostStatus(ProxyCommunication.ResponseJsonObject jsonObject)
        {
            int hoststatus_pos = 0;
            foreach (var item in jsonObject.hosts.fields)
            {
                if (item.Equals("status"))
                    return hoststatus_pos;
                else
                    hoststatus_pos++;
            }
            return -1;
        }
        public static int getNanosec() {
            int length = 9;
            string value = Convert.ToString(Math.Abs(100000000 * Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds().ToString())));
            if (value.Length>=length)
            {
                return Convert.ToInt32(value.Substring(0, length));
            }
            else
            {
                string value2 = value;
                value2=value2.PadRight(length, '0');
                return Convert.ToInt32(value2);
            }
        }
        public static string StopWatch(Stopwatch stopWatch) {
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0}m {1}s {2}ms", ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            return elapsedTime;
        }

        public static void ExitApplication(Stopwatch stopWatch, List<string> listUnfinishedTasks)
        {

            while (true)
            {
                TimeSpan ts = stopWatch.Elapsed;
                //string elapsedTime = String.Format("{0}m {1}s", ts.Minutes, ts.Seconds);
                //Console.WriteLine(elapsedTime);
                Thread.Sleep(1000);
                if (ts.Minutes >= 5)
                {
                    Log.ErrorFormat("Kényszerített bezárás {0}  Nem sikerült befejezni: {1}", Utility.StopWatch(stopWatch), String.Join(", ", listUnfinishedTasks.ToArray()));
                    Environment.Exit(0);
                }
            }
        }

    }
}
