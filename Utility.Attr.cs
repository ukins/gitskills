
using CSV;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hunter
{
    public enum EAttrType
    {
        Hp = 100,
        MaxHp = 200,
        Speed = 300,
        Attack = 400,
        Defense = 500,
        Crit = 600,
        Tough = 700,
        Crit_Multi = 800,
        Break_Armour = 1100,
        Crit_Rate = 1600,
        Endurance = 2100,
        Endurance_Max = 2500,
        Endurance_Speed = 2700,
        Belt_Pocket = 2800,
        Weight_Limit = 2900,
        Pet_Endurance = 3000,   //宠物当前体力
        Pet_Hunger = 3100,      //宠物当前饥饿
        Pet_Mood = 3200,      //宠物当前心情
        Pet_Endurance_Max = 3300,//体力上限
        Pet_Hunger_Max = 3400,  //饥饿上限
        Pet_Mood_Max = 3500,    //心情上限
        Pet_Collect = 3600,
        Pet_Hunter = 3700,
        Pet_Explore = 3800,
        Pet_Steal = 3900,
        Dragon_Resist = 4000,   //龙吼抗性
        Magic_Damage_Ice = 18010,
        Magic_Damage_Fire = 18020,
        Magic_Damage_Thunder = 18030,
        Magic_Damage_Poison = 18040,
        Magic_Resist_Ice = 19010,
        Magic_Resist_Fire = 19020,
        Magic_Resist_Thunder = 19030,
        Magic_Resist_Poison = 19040
    }

    public class AttrItem
    {
        private CSVAttrDefine define;

        public int Id => define.iAttrID;
        public string Name => define.sAttrDefine;
        public string AblName => define.sAttrValueDefine;
        public string PctName => define.sAttrRateDefine;
        public string Desc => define.sAttrName;

        public string IconPath => define.sIconPath;

        public AttrItem(CSVAttrDefine define)
        {
            this.define = define;
        }
    }

    public class AttrPair
    {
        private AttrItem m_item;
        public int Id => m_item.Id;
        public string Name => m_item.Name;
        public string AblName => m_item.AblName;
        public string PctName => m_item.PctName;
        public string Desc => m_item.Desc;

        public string IconPath => m_item.IconPath;

        public float Value { get; set; }
        public string PctValue { get { return $"{Math.Round(Value * 100, 2)}%"; } }

        public string ParseUseDef;//解析时使用的属性定义

        public string StrValue
        {
            get
            {
                if (ParseUseDef.Contains("pct"))
                {
                    return $"+{PctValue}";
                }
                return ((int)Value).ToString();
            }
        }

        public string StrValue1
        {
            get
            {
                if (ParseUseDef.Contains("pct"))
                {
                    return $"+{PctValue}";
                }
                return Value.ToString();
            }
        }

        public AttrPair(AttrItem item, float value, string parse)
        {
            m_item = item;

            Value = value;

            ParseUseDef = parse;
        }
    }

    public static partial class Utility
    {
        public static class Attr
        {
            private static List<AttrItem> m_attr_arr = new List<AttrItem>();
            public static void Additem(CSVAttrDefine define)
            {
                AttrItem item = new AttrItem(define);
                if (!m_attr_arr.Contains(item))
                {
                    m_attr_arr.Add(item);
                }
            }
            public static AttrItem Find(int id)
            {
                foreach (var item in m_attr_arr)
                {
                    if (item.Id == id)
                    {
                        return item;
                    }
                }
                return null;
            }

            public static string GetDesc(string name)
            {
                foreach (var item in m_attr_arr)
                {
                    if (item.Name == name)
                    {
                        return item.Desc;
                    }
                }
                return null;
            }
            public static string GetIconPath(string name)
            {
                foreach (var item in m_attr_arr)
                {
                    if (item.Name == name)
                    {
                        return item.IconPath;
                    }
                }
                return null;
            }
            public static AttrItem Find(string name)
            {
                CSVAttrDefine.GetAll();
                foreach (var item in m_attr_arr)
                {
                    if (string.CompareOrdinal(item.Name, name) == 0)
                    {
                        return item;
                    }
                    if (string.CompareOrdinal(item.AblName, name) == 0)
                    {
                        return item;
                    }
                    if (string.CompareOrdinal(item.PctName, name) == 0)
                    {
                        return item;
                    }
                }
                return null;
            }

            public static AttrPair SingleParse(string attr)
            {
                string[] pair = attr.Split('=');
                if (pair.Length < 2)
                {
                    if (Debug.unityLogger.logEnabled == true)
                    {
                        Debug.LogError($"属性解析失败！ str = {attr}");
                    }
                    return null;
                }
                var attr_item = Find(pair[0]);
                if (attr_item == null)
                {
                    if (Debug.unityLogger.logEnabled == true)
                    {
                        Debug.LogError($"属性解析失败！ 属性未定义 = {pair[0]}");
                    }
                    return null;
                }
                float value = 0;
                if (float.TryParse(pair[1], out value) == false)
                {
                    if (Debug.unityLogger.logEnabled == true)
                    {
                        Debug.LogError($"属性解析失败！ 属性值非值类型 = {pair[0]}");
                    }
                    return null;
                }
                return new AttrPair(attr_item, value, pair[0]);
            }

            public static Dictionary<string, AttrPair> ParseDic(string attr_str)
            {
                Dictionary<string, AttrPair> dic = new Dictionary<string, AttrPair>();

                string[] attr_list = attr_str.Split(';');
                foreach (var item in attr_list)
                {
                    if (string.IsNullOrWhiteSpace(item) == true)
                    {
                        continue;
                    }
                    string[] pair = item.Split('=');
                    if (pair.Length < 2)
                    {
                        if (Debug.unityLogger.logEnabled == true)
                        {
                            Debug.LogError($"属性解析失败！ str = {attr_str}");
                        }
                        continue;
                    }

                    var attr_item = Find(pair[0]);
                    if (attr_item == null)
                    {
                        if (Debug.unityLogger.logEnabled == true)
                        {
                            Debug.LogError($"属性解析失败！ 属性未定义 = {pair[0]}");
                        }
                        continue;
                    }

                    float value = 0;
                    if (float.TryParse(pair[1], out value) == false)
                    {
                        if (Debug.unityLogger.logEnabled == true)
                        {
                            Debug.LogError($"属性解析失败！ 属性值非值类型 = {pair[0]}");
                        }
                        continue;
                    }
                    var attr = new AttrPair(attr_item, value, pair[0]);
                    dic[attr.Name] = attr;
                }

                return dic;
            }

            public static List<AttrPair> Parse(string attr_str)
            {
                List<AttrPair> lst = new List<AttrPair>();

                string[] attr_list = attr_str.Split(';');
                foreach (var item in attr_list)
                {
                    if (string.IsNullOrWhiteSpace(item) == true)
                    {
                        continue;
                    }
                    string[] pair = item.Split('=');
                    if (pair.Length < 2)
                    {
                        if (Debug.unityLogger.logEnabled == true)
                        {
                            Debug.LogError($"属性解析失败！ str = {attr_str}");
                        }
                        continue;
                    }

                    var attr_item = Find(pair[0]);
                    if (attr_item == null)
                    {
                        if (Debug.unityLogger.logEnabled == true)
                        {
                            Debug.LogError($"属性解析失败！ 属性未定义 = {pair[0]}");
                        }
                        continue;
                    }

                    float value = 0;
                    if (float.TryParse(pair[1], out value) == false)
                    {
                        if (Debug.unityLogger.logEnabled == true)
                        {
                            Debug.LogError($"属性解析失败！ 属性值非值类型 = {pair[0]}");
                        }
                        continue;
                    }
                    lst.Add(new AttrPair(attr_item, value, pair[0]));
                }

                return lst;
            }
        }
    }
}
