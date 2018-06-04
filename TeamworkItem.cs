#define Using_Unity3D
using System;
#if Using_Unity3D
using UnityEngine;
#endif
/// <summary>
/// 张志杰 20170305
/// 协同信息转换
/// </summary>
namespace ChivaVR.Net.Core
{
    /// <summary>
    /// 属性类型
    /// </summary>
    public enum PropertyType : ushort
    {
        Unknown = 0,

        Int = 1,
        Float = 2,
        String = 3,
        Vector2 = 4,
        Vector3 = 5,
        Vector4 = 6,
        List = 7
    }

    /// <summary>
    /// 协同信息项
    /// </summary>
    public class TeamworkItem
    {
        #region 属性
        /// <summary>
        /// 场景
        /// </summary>
        public string Scene { get; set; }

        /// <summary>
        /// 目标类型
        /// </summary>
        public string ObjectType { get; set; }

        /// <summary>
        /// 目标id
        /// </summary>
        public string ObjectId { get; set; }

        /// <summary>
        /// 属性
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// 标志
        /// </summary>

        public string Flag { get; set; }

        /// <summary>
        /// 属性类型
        /// </summary>
        public PropertyType Type { get; set; }

        /// <summary>
        /// 协同属性
        /// </summary>
        public MessageItem Item { get; set; }

        /// <summary>
        /// 值及其类型
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 消息体
        /// </summary>
        public string Boby
        {
            get { return Scene + "\r" + ObjectType + "\r" + ObjectId + "\r" + Property + "\r" + Flag + "\r" + Value; }
        }

        #endregion

        #region 构造
        public TeamworkItem(string scene, string objectType, string objectId, string property)
        {
            Scene = scene;
            ObjectType = objectType;
            ObjectId = objectId;
            Property = property;
            Flag = "none";
            Type = PropertyType.Unknown;
        }

        public static TeamworkItem FromMsg(string msg)
        {
            try
            {
                string[] strs = msg.Split('\r');
                TeamworkItem ti = new TeamworkItem(strs[0], strs[1], strs[2], strs[3]);
                ti.Flag = strs[4];
                ti.Type = (PropertyType)Enum.Parse(typeof(PropertyType), strs[5]);
                ti.Value = ti.Type + "\r" + strs[6];

                return ti;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region from
        public void From(params object[] os)
        {
            Type = PropertyType.List;
            foreach (object o in os)
            {
                Value += o.ToString() + ",";
            }
            Value = Type.ToString() + "\r" + Value.Remove(Value.Length - 1);
        }

        public void From(string str)
        {
            Type = PropertyType.String;
            Value = Type.ToString() + "\r" + str;
        }

        public void From(int i)
        {
            Type = PropertyType.Int;
            Value = Type.ToString() + "\r" + i.ToString();
        }

        public void From(float f)
        {
            Type = PropertyType.Float;
            Value = Type.ToString() + "\r" + f.ToString();
        }

        public void From(float f1, float f2)
        {
            Type = PropertyType.Vector2;
            Value = Type.ToString() + "\r" + f1.ToString() + "," + f2.ToString();
        }

        public void From(float f1, float f2, float f3)
        {
            Type = PropertyType.Vector3;
            Value = Type.ToString() + "\r" + f1.ToString() + "," + f2.ToString() + "," + f3.ToString();
        }

        public void From(float f1, float f2, float f3, float f4)
        {
            Type = PropertyType.Vector4;
            Value = Type.ToString() + "\r" + f1.ToString() + "," + f2.ToString() + "," + f3.ToString() + "," + f4.ToString();
        }
#if Using_Unity3D
        public void From(Vector2 v2)
        {
            From(v2.x, v2.y);
        }
        public void From(Vector3 v3)
        {
            From(v3.x, v3.y, v3.z);
        }
        public void From(Vector4 v4)
        {
            From(v4.x, v4.y, v4.z, v4.w);
        }
#endif
        #endregion

        #region to
        public string[] ToList()
        {
            try
            {
                if (Type == PropertyType.List)
                {
                    int i = Value.IndexOf('\r');
                    string str = Value.Substring(i + 1);
                    string[] strs = str.Split(',');

                    return strs;
                }
                throw new ArgumentException("类型不正确");
            }
            catch
            {
                throw new ArgumentException("参数转换不正确");
            }
        }
        public int ToInt()
        {
            if (Type == PropertyType.Int)
            {
                return int.Parse(Value.Substring(Value.IndexOf('\r') + 1));
            }
            throw new ArgumentException("类型不正确");
        }
        public float ToFloat()
        {
            if (Type == PropertyType.Float)
            {
                return float.Parse(Value.Substring(Value.IndexOf('\r') + 1));
            }
            throw new ArgumentException("类型不正确");
        }
        public new string ToString()
        {
            if (Type == PropertyType.String)
            {
                return Value.Substring(Value.IndexOf('\r') + 1);
            }
            throw new ArgumentException("类型不正确");
        }
#if Using_Unity3D
        public Vector2 ToVector2()
        {
            try
            {
                if (Type == PropertyType.Vector2)
                {
                    string[] strs = Value.Split(',', '\r');
                    return new Vector2(float.Parse(strs[1]), float.Parse(strs[2]));
                }
                throw new ArgumentException("类型不正确");
            }
            catch
            {
                throw new ArgumentException("参数转换不正确");
            }
        }
        public Vector3 ToVector3()
        {
            try
            {
                if (Type == PropertyType.Vector3)
                {
                    string[] strs = Value.Split(',', '\r');
                    return new Vector3(float.Parse(strs[1]), float.Parse(strs[2]), float.Parse(strs[3]));
                }
                throw new ArgumentException("类型不正确");
            }
            catch
            {
                throw new ArgumentException("参数转换不正确");
            }
        }
        public Vector4 ToVector4()
        {
            try
            {
                if (Type == PropertyType.Vector4)
                {
                    string[] strs = Value.Split(',', '\r');
                    return new Vector4(float.Parse(strs[1]), float.Parse(strs[2]), float.Parse(strs[3]), float.Parse(strs[4]));
                }
                throw new ArgumentException("类型不正确");
            }
            catch
            {
                throw new ArgumentException("参数转换不正确");
            }
        }
#endif
        #endregion
    }
}
