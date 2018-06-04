using System;
using System.Text;
using System.IO;

namespace ChivaVR.Net.Toolkit
{
    /// <summary>
    /// 资源包操作
    /// </summary>
    public static class ResourcePackage
    {
        /// <summary>
        /// 验证是否为打包后文件
        /// </summary>
        /// <param name="pathfile"></param>
        /// <returns></returns>
        public static bool Verify(string pathfile)
        {
            byte[] buff = File.ReadAllBytes(pathfile);
            return Encoding.UTF8.GetString(buff, 0, _key.Length) == _key;
        }

        /// <summary>
        /// 获取资源包时间戳
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        public static string GetLastWritenTime(byte[] buff)
        {
            if (Encoding.UTF8.GetString(buff, 0, _key.Length) != _key)
            {
                return null;
            }

            string tm = Encoding.UTF8.GetString(buff, 7, 19);
            return tm;
        }

        /// <summary>
        /// 打包
        /// </summary>
        /// <param name="inPathfile">源全路径文件名</param>
        /// <param name="outPathfile">目标全路径文件名</param>
        public static void Package(string inPathfile, string outPathfile)
        {
            byte[] buff = File.ReadAllBytes(inPathfile);
            string tms = Kits.GetLastWriteTime(inPathfile);
            byte[] outBuff = Package(buff, tms);
            File.WriteAllBytes(outPathfile, outBuff);
        }

        /// <summary>
        /// 打包
        /// </summary>
        /// <param name="buff">源文件字节数组</param>
        /// <param name="lastWritenTime">源文件最后修改日期</param>
        /// <returns>打包后的字节数组</returns>
        public static byte[] Package(byte[] buff, string lastWritenTime)
        {
            byte[] keyBuff = Encoding.UTF8.GetBytes(_key);
            byte[] desbuff = null;
            byte[] otherbuff = null;
            byte[] tmBuff = Encoding.UTF8.GetBytes(lastWritenTime);


            if (buff.Length < _stampLength)
            {
                desbuff = DesByte.Encrypt(buff);
            }
            else
            {
                byte[] cutbuff = new byte[_stampLength];
                Buffer.BlockCopy(buff, 0, cutbuff, 0, _stampLength);
                desbuff = DesByte.Encrypt(cutbuff);

                otherbuff = new byte[buff.Length - _stampLength];
                Buffer.BlockCopy(buff, _stampLength, otherbuff, 0, otherbuff.Length);
            }

            byte[] bodyLengthBuff = BitConverter.GetBytes((ulong)desbuff.Length);

            //to combine
            byte[] outBuff = new byte[keyBuff.Length + tmBuff.Length + bodyLengthBuff.Length + desbuff.Length + (otherbuff == null ? 0 : otherbuff.Length)];
            Buffer.BlockCopy(keyBuff, 0, outBuff, 0, keyBuff.Length);
            Buffer.BlockCopy(tmBuff, 0, outBuff, keyBuff.Length, tmBuff.Length);
            Buffer.BlockCopy(bodyLengthBuff, 0, outBuff, keyBuff.Length + tmBuff.Length, bodyLengthBuff.Length);
            Buffer.BlockCopy(desbuff, 0, outBuff, keyBuff.Length + tmBuff.Length + bodyLengthBuff.Length, desbuff.Length);
            if (otherbuff != null)
            {
                Buffer.BlockCopy(otherbuff, 0, outBuff, keyBuff.Length + tmBuff.Length + bodyLengthBuff.Length + desbuff.Length, otherbuff.Length);
            }

            return outBuff;
        }

        /// <summary>
        /// 拆包，并设置原始文件三个日期
        /// </summary>
        /// <param name="inPathfile">源全路径文件名</param>
        /// <param name="outPathfile">拆包后的原始文件全路径名</param>
        /// <returns>原始文件字节大小</returns>
        public static int Unpackage(string inPathfile, string outPathfile)
        {
            byte[] buff = File.ReadAllBytes(inPathfile);
            byte[] outBuff = Unpackage(buff);
            if (outBuff == null)
            {
                throw new NullReferenceException("资源包无效");
            }
            else
            {
                try
                {
                    File.WriteAllBytes(outPathfile, outBuff);

                    string tms = GetLastWritenTime(buff);

                    DateTime tm = DateTime.Parse(tms);
                    File.SetLastWriteTime(outPathfile, tm);
                    File.SetCreationTime(outPathfile, tm);
                    File.SetLastAccessTime(outPathfile, tm);

                    return outBuff.Length;
                }
                catch (IOException ioe)
                {
                    throw ioe;
                }
            }
        }

        /// <summary>
        /// 拆包
        /// </summary>
        /// <param name="buff">源文件字节数组</param>
        /// <returns>原始文件字节数组</returns>
        public static byte[] Unpackage(byte[] buff)
        {
            try
            {
                if (Encoding.UTF8.GetString(buff, 0, _key.Length) != _key)
                {
                    return null;
                }

                ulong l = BitConverter.ToUInt64(buff, _key.Length + 19);
                byte[] mbuf = new byte[l];
                Buffer.BlockCopy(buff, 15 + 19, mbuf, 0, (int)l);
                byte[] desbuff = DesByte.Decrypt(mbuf);

                byte[] outbuff = new byte[desbuff.Length + buff.Length - 15 - 19 - (int)l];
                Buffer.BlockCopy(desbuff, 0, outbuff, 0, desbuff.Length);
                Buffer.BlockCopy(buff, 15 + 19 + (int)l, outbuff, desbuff.Length, buff.Length - 15 - 19 - (int)l);

                return outbuff;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private static string _key = "CHIVAVR";
        private static int _stampLength = 1024;
    }
}
