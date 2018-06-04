using ChivaVR.Net.Packet;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChivaVR.Net.Core
{
    public class CVRJsonParser
    {
        private List<Dictionary<string, string>> _jsons = new List<Dictionary<string, string>>();

        public CVRJsonParser(QueryResultPt qrp)
            : this(qrp.result)
        {
            Count = qrp.count;
            Token = qrp.token;
        }

        public CVRJsonParser(string json)
        {
            try
            {
                //去掉[]
                StringBuilder sb = new StringBuilder(json);
                if (json.StartsWith("["))
                {
                    sb.Remove(0, 1);
                }
                if (json.EndsWith("]"))
                {
                    sb.Remove(sb.Length - 1, 1);
                }
                //若null记录
                sb.Replace(":null,", ":\"\",");

                //分组
                string[] js = sb.ToString().Split(new char[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);

                //创建词典
                char[] sep1 = new char[] { ',' };
                char[] sep2 = new char[] { '"', ':' };
                foreach (string s in js)
                {
                    if (s != ",")
                    {
                        Dictionary<string, string> items = new Dictionary<string, string>();
                        string[] ss = s.Split(sep1);
                        foreach (string sss in ss)
                        {
                            string[] ssss = sss.Split(sep2);
                            items.Add(ssss[1], ssss[4]);
                        }
                        if (items.Count != 0)
                        {
                            _jsons.Add(items);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("JSON字符串包含非法字符:" + json, ex);
            }
        }

        /// <summary>
        /// 操作标识
        /// </summary>
        public string Token
        {
            get; set;
        }

        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="i">行</param>
        /// <param name="name">列</param>
        /// <returns>值</returns>
        public string this[int i, string name]
        {
            get
            {
                if (i < _jsons.Count)
                {
                    if (_jsons[i].ContainsKey(name))
                    {
                        return _jsons[i][name];
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="i">行</param>
        /// <returns>一行记录[字段,值]</returns>
        public Dictionary<string, string> this[int i]
        {
            get
            {
                if (i < _jsons.Count)
                {
                    return _jsons[i];
                }
                return null;
            }
        }

        /// <summary>
        /// 当前查询记录数量
        /// </summary>
        public int Num
        {
            get
            {
                return _jsons.Count;
            }
        }

        /// <summary>
        /// 记录集是否为空
        /// </summary>
        public bool Empty
        {
            get
            {
                return _jsons.Count == 0;
            }
        }

        /// <summary>
        /// 所有记录数量
        /// </summary>
        public int Count
        {
            get; set;
        }
    }
}
