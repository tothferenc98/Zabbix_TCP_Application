﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Zabbix_TCP_Application
{
    partial class Utility
    {
        public static string ProcessingAndRequest(ProxyCommunication.ResponseJsonObject jsonObject)
        {
            List<ProxyCommunication.ItemCheckData> itemCheckDataObjectList = new List<ProxyCommunication.ItemCheckData>();
            MakeItemCheckDataObject(jsonObject, itemCheckDataObjectList);

            foreach (var item in itemCheckDataObjectList)
            { Console.WriteLine(item.ToString()); }
            Console.WriteLine();

            return MakeRequest(itemCheckDataObjectList);
        }

        public static void MakeItemCheckDataObject(ProxyCommunication.ResponseJsonObject jsonObject, List<ProxyCommunication.ItemCheckData> itemCheckDataObjectList)
        {
            foreach (var item in jsonObject.items.data)
            {
                string hostname = String.Empty;
                foreach (var hostData in jsonObject.hosts.data) //Hostname logoláshoz
                {
                    if (item[getPositionItemsHostid(jsonObject)].Equals(hostData[getPositionHostsHostId(jsonObject)]))
                    {
                        hostname = hostData[getPositionHostsHostName(jsonObject)];
                    }
                }

                itemCheckDataObjectList.Add(new ProxyCommunication.ItemCheckData()
                {
                    ItemId = Convert.ToInt32(item[getPositionItemsItemId(jsonObject)]),
                    HostId = Convert.ToInt32(item[getPositionItemsHostid(jsonObject)]),
                    HostName = hostname,
                    Key = item[getPositionItemsKey(jsonObject)],
                });
            }
        }

        public static string MakeRequest(List<ProxyCommunication.ItemCheckData> itemCheckDataObjectList)
        {
            string secondResponseData = String.Empty;
            ProxyCommunication.RequestJsonObject requestJsonObject = new ProxyCommunication.RequestJsonObject();
            try
            {
                if (itemCheckDataObjectList.Count > 0)
                {
                    List<ProxyCommunication.RequestJsonData> listRequestJsonData = new List<ProxyCommunication.RequestJsonData>();
                    Int32 id = 1;

                    Parallel.ForEach(itemCheckDataObjectList, async item =>
                    {
                        string keyName = item.Key;
                        Boolean added = false;
                        if (item.Key.Contains('['))
                            keyName = item.Key.Substring(0, item.Key.IndexOf('['));

                        switch (keyName)
                        {
                            case "web.page.get":
                                WEBPAGEGET(item, listRequestJsonData, itemCheckDataObjectList, id, added);
                                break;
                            case "net.tcp.port":
                                await NETTCPPORT(item, listRequestJsonData, itemCheckDataObjectList, id, added);
                                break;
                            default:
                                break;
                        }

                        if (!added)
                        {
                            Log.InfoFormat("{0} A feldolgozás sikertelen. (Hostname: {1})", item.Key, item.HostName);
                        }
                    });

                    requestJsonObject.historydata = listRequestJsonData;
                    requestJsonObject.clock = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
                    requestJsonObject.ns = getNanosec();
                    requestJsonObject.version = PROXY_VERSION;

                    string serializedJson = JsonConvert.SerializeObject(requestJsonObject);
                    secondResponseData = ConnectJson(serializedJson);
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

        public static void WEBPAGEGET(ProxyCommunication.ItemCheckData item, List<ProxyCommunication.RequestJsonData> listRequestJsonData, List<ProxyCommunication.ItemCheckData> itemCheckDataObjectList, Int32 id, Boolean added)
        {
            string value = WebPageGet(item.Key);
            //AddValueItemCheckDataObjectList(itemCheckDataObjectList, item.Key, value);
            if (!value.Equals(String.Empty))
            {
                listRequestJsonData.Add(new ProxyCommunication.RequestJsonData()
                {
                    itemid = item.ItemId,
                    clock = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
                    ns = getNanosec(),
                    value = value,
                    id = id
                });
                id++;
                added = true;

            }
        }

        public static string WebPageGet(string key)
        {
            string key2 = key.Substring(key.IndexOf('[') + 1, key.IndexOf(']') - key.IndexOf('[') - 1);
            var splitted = key2.Split(',');
            string ip = splitted[0];
            int port = Convert.ToInt32(splitted[2]);
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
                Log.Debug("WebPageGet: ",e);
                return "";
            }
        }
        

        public static string WebPageGetConnectJson(string name, int webpagePort)
        {
            try
            {
                byte[] jsonBytes = new byte[BUFFER_SIZE]; 
                string json = String.Format(@"GET HTTP/1.1");
                Array.Copy(ENCODING.GetBytes(json), jsonBytes, json.Length);
                jsonBytes = TrimEnd(jsonBytes);

                byte[] jsonBytesRespond = WebPageGetConnect(jsonBytes, name, webpagePort);
                if (!jsonBytesRespond.SequenceEqual(new byte[0]))
                {
                    string resultJson = ENCODING.GetString(jsonBytesRespond, 0, jsonBytesRespond.Length);
                    if (resultJson.Contains("HTTP/1.1 200 OK"))
                    {
                        resultJson = resultJson.Substring(resultJson.IndexOf('{'));
                        return resultJson;
                    }
                    else
                    {
                        JsonLog.Debug("WebPageGetConnectJson: nem HTTP/1.1 200 OK a válasz");
                    }
                }
                return "";
            }
            catch (Exception e)
            {
                Console.WriteLine("Hiba a WebPageGetConnectJson-ben");
                JsonLog.Debug("WebPageGetConnectJson: Hiba: ",e);
                return "";
            }
        }


        public static byte[] WebPageGetConnect(byte[] data, string name, int webpagePort)
        {
            byte[] error = new byte[0];
            try
            {

                Int32 port = webpagePort;
                String server = name;
                TcpClient client = new TcpClient(server, port);
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);

                // Hexadecimális értékek logolása
                //ByteLog.InfoFormat("Sent:     {0}", String.Join(String.Empty, data.Select(d => String.Format("{0:X}", d))));

                try
                {
                    byte[] byteArrayData2 = new Byte[BUFFER_SIZE];
                    int bytes = stream.Read(byteArrayData2, 0, byteArrayData2.Length);
                    byteArrayData2 = TrimEnd(byteArrayData2);
                    // Hexadecimális értékek logolása
                    //ByteLog.InfoFormat("Received: {0}", String.Join(String.Empty, byteArrayData2.Select(d => String.Format("{0:X}", d))));
                    stream.Close();
                    client.Close();
                    return byteArrayData2;
                }
                catch (Exception e)
                {
                    //ByteLog.Error("Hiba a Connect válasz részénél: ", e);
                    Log.Debug("WebPageGetConnect: belső try-ban hiba: ", e);
                    return error;
                }
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("Connect.ArgumentNullException: ", e);
                ByteLog.Debug("WebPageGetConnect: hiba: ", e);
                return error;
            }
            catch (SocketException e)
            {
                //ByteLog.Error("Hiba a Connect küldés részénél: SocketException: ", e);
                Console.WriteLine("Connect.SocketException: ", e);
                ByteLog.Debug("WebPageGetConnect: ", e);
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

        public static async Task NETTCPPORT(ProxyCommunication.ItemCheckData item, List<ProxyCommunication.RequestJsonData> listRequestJsonData, List<ProxyCommunication.ItemCheckData> itemCheckDataObjectList, Int32 id, Boolean added)
        {
            string value = await TcpPortTest(item.Key);
            //AddValueItemCheckDataObjectList(itemCheckDataObjectList, item.Key, value);
            if (value.Equals("0"))
            {
                Log.InfoFormat("{0} csatlakozás sikertelen. (Hostname: {1})", item.Key, item.HostName);
            }
            listRequestJsonData.Add(new ProxyCommunication.RequestJsonData()
            {
                itemid = item.ItemId,
                clock = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
                ns = getNanosec(),
                value = value,
                id = id
            });
            id++;
            added = true;
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

        /*public static void AddValueItemCheckDataObjectList(List<ProxyCommunication.ItemCheckData> itemCheckDataObjectList, string key, string value)
        {
            foreach (var item in itemCheckDataObjectList)
            {
                if (item.Key.Equals(key))
                {
                    item.Value = value;
                }
            }
        }*/

        
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
        public static int getNanosec() {
            string value = Convert.ToString(Math.Abs(100000000 * Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds().ToString())));
            if (value.Length>=9)
            {
                return Convert.ToInt32(value.Substring(0, 9));
            }
            else
            {
                int temp = 9 - value.Length;
                string value2 = value;
                for (int i = 0; i < temp; i++)
                {
                    value2+='0';
                }
                return Convert.ToInt32(value2);
            }
        }
    }
}
