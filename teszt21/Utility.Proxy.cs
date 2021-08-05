using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zabbix_TCP_Application
{
    partial class Utility
    {
        public static Dictionary<int, List<string>> MakeHostIdKeyDict(Dictionary<int, List<string>> dictItem, ProxyCommunication.ResponseJsonObject jsonObject)
        {
            foreach (var item in jsonObject.items.data)
            {
                if (!dictItem.Keys.Contains(Convert.ToInt32(item[getPositionItemsHostid(jsonObject)])))
                {
                    List<string> listItem = new List<string>();

                    foreach (var item2 in jsonObject.items.data)
                    {

                        if (item[getPositionItemsHostid(jsonObject)].Equals(item2[getPositionItemsHostid(jsonObject)]))
                        {
                            if (Convert.ToString(item2[getPositionItemsKey(jsonObject)]).Contains("{$"))
                            {
                                string tempKey = Convert.ToString(item2[getPositionItemsKey(jsonObject)]);
                                string tempReplacedData = getReplaceData(Convert.ToInt32(item[getPositionItemsHostid(jsonObject)]), tempKey, jsonObject);
                                item2[getPositionItemsKey(jsonObject)] = Convert.ToString(item2[getPositionItemsKey(jsonObject)]).Replace(tempKey, tempReplacedData);
                            }

                            listItem.Add(Convert.ToString(item2[getPositionItemsKey(jsonObject)]));
                        }
                    }
                    dictItem.Add(Convert.ToInt32(item[getPositionItemsHostid(jsonObject)]), listItem);

                }
            }
            return dictItem;
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
    }
}
