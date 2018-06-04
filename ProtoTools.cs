using ChivaVR.Net.Toolkit;
using System;
using System.IO;
/// <summary>
/// 张志杰 20170114
/// 王光
/// Protocol Buffer工具
/// </summary>
namespace ChivaVR.Net.Packet
{
    /// <summary>
    /// Protocol Buffer 数据类型
    /// </summary>
    public enum ProtoBuffType : ushort
    {
        Unknown = 0x0000,

        C2SBye = 0x401,

        C2SChatSendTextMsg = 0x501,//发送消息
        S2CChatReceiveTextMsg = 0x50a,//接收消息指令
        S2CChatStatusReceived= 0x50c,//接收聊天状态

        C2SLogin = 0x601,//登陆用户增删改查
        S2CLogin = 0x60a,//返回登陆用户增删改查信息
        S2CLogonUser = 0x60b,//获取系统其他用户登录或下线信息

        S2CMessage = 0x30a,//通用消息

        C2SGetOnlineUser = 0x603,//请求获取当前在线用户集合
        S2CGetOnlineUserResult = 0x60c,//获取当前在线用户集合

        C2SResourceEdit = 0x701,//资源增删改查
        S2CResourceEditResult = 0x70a,//返回资源增删改查信息

        C2SResourceQuery = 0x702,//组合条件查询资源
        S2CResourceQueryResult = 0x70b,//返回资源集合

        C2STableQuery = 0x201,//请求查询表
        S2CTableQueryResult = 0x20a,// 用户接受查询内容

        C2STableUpdate = 0x202,//修改、删除、插入
        S2CTableUpdateResult = 0x20b,//操作是否成功
    }

    public static class ProtobufWrapper
    {
        /// <summary>
        /// 将类型和protobuff打包成流
        /// 4B+2B+protobuffer.Length
        /// protobuffer.Length+2 cmd protobuffer
        /// </summary>
        /// <param name="type"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static byte[] Packaging<T>(int type, T t)
        {
            byte[] src = ProtobufWrapper.ByteSerialize<T>(t);

            byte[] buff = new byte[4 + 2 + src.Length];

            byte[] headerbuf = BitConverter.GetBytes(src.Length + 2);

            byte[] header = new byte[4] { headerbuf[3], headerbuf[2], headerbuf[1], headerbuf[0] };
            byte[] cmdbuff = BitConverter.GetBytes(type);
            byte[] cmd = new byte[2] { cmdbuff[1], cmdbuff[0] };

            //header
            Buffer.BlockCopy(header, 0, buff, 0, header.Length);
            //cmd
            Buffer.BlockCopy(cmd, 0, buff, header.Length, cmd.Length);
            //protocolbuffer
            Buffer.BlockCopy(src, 0, buff, header.Length + cmd.Length, src.Length);

            /**
             *wg:update 
             */
            //以下两个行为Java客户端的请求加密方式
            // byte[] ciphertext = RC4.RC4Custom(plaintext, key.getComplexKey(), key.getOutputCounter());
            //key.outputCounterIncrease(ciphertext.length);
            //一下为获取增量
            CustomRC4Key key = CustomRC4Key.CreateCustomRC4Key();
            byte[] rc4byte = RC4.Convert(buff, 0);// key.GetOutputCounter());
            key.OutputCounterIncrease(rc4byte.Length);
            //end
            return rc4byte;
        }

        /// <summary>
        /// 获取数据流类型
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static int GetPacketCmd(byte[] src)
        {
            //第4、5字节位cmd位
            byte[] buff = new byte[2] { src[1], src[0] };
            return BitConverter.ToUInt16(buff, 0);
        }

        /// <summary>
        /// 将对象实例序列到流
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static byte[] ByteSerialize<T>(T t)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    //ProtoBuf工具的序列化
                    ProtoBuf.Serializer.Serialize<T>(ms, t);

                    byte[] result = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(result, 0, result.Length);

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 将流序列化到对象实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static T ByteDeserialize<T>(byte[] msg, int len)
        {
            try
            {
                //去除header和cmd
                byte[] sizebuff = new byte[4] { msg[3], msg[2], msg[1], msg[0] };
                int size = BitConverter.ToInt32(sizebuff, 0);

                byte[] buff = new byte[len];
                Buffer.BlockCopy(msg, 2, buff, 0, buff.Length);

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(buff, 0, buff.Length);
                    ms.Position = 0;

                    //使用工具反序列化对象
                    T result = ProtoBuf.Serializer.Deserialize<T>(ms);
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 将对象实例序列化到文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pathfile"></param>
        /// <param name="t"></param>
        public static void FileSerialize<T>(string pathfile, T t)
        {
            try
            {
                using (var file = File.Create(pathfile))
                {
                    ProtoBuf.Serializer.Serialize(file, t);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 将文件内容序列化到对象实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pathfile"></param>
        /// <returns></returns>
        public static T FileDeserialize<T>(string pathfile)
        {
            if (!File.Exists(pathfile))
            {
                throw new FileNotFoundException("未发现序列文件", pathfile);
            }

            T t = default(T);

            using (var file = File.OpenRead(pathfile))
            {
                t = ProtoBuf.Serializer.Deserialize<T>(file);
            }

            return t;
        }
    }
}
