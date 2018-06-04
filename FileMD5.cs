using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ChivaVR.Net.Toolkit
{
    public static class FileMD5
    {
        //十六进制串
        private static string ByteArrayToHexString(byte[] values)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte value in values)
            {
                sb.AppendFormat("{0:X2}", value);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="pathfile">源文件全路径</param>
        /// <returns>MD5值，若为string.Empty则失败</returns>
        public static string Create(string pathfile)
        {
            string hashStr = string.Empty;
            try
            {
                FileStream fs = new FileStream(pathfile, FileMode.Open, FileAccess.Read, FileShare.Read);
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] hash = md5.ComputeHash(fs);
                hashStr = ByteArrayToHexString(hash);
                fs.Close();
                fs.Dispose();
            }
            catch { return string.Empty; }
            return hashStr;
        }

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="buff">源文件内容字节数组</param>
        /// <returns>MD5值，若为string.Empty则失败</returns>
        public static string Create(byte[] buff)
        {
            string hashStr = string.Empty;
            try
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] hash = md5.ComputeHash(buff);
                hashStr = ByteArrayToHexString(hash);
            }
            catch { return string.Empty; }
            return hashStr;
        }
    }
}
