using System;
using System.Security.Cryptography;
using System.Text;

namespace ChivaVR.Net.Toolkit
{
    public static class DesByte
    {
        private static readonly string _key = "ChivaVRC";

        #region DES
        /// <summary>
        /// 进行DES加密。
        /// </summary>
        /// <param name="toencrypt">要加密的字符串。</param>
        /// <param name="key">密钥，且必须为8位。</param>
        /// <returns>以Base64格式返回的加密字符串。</returns>
        public static byte[] Encrypt(byte[] inputByteArray)
        {
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                des.Key = ASCIIEncoding.ASCII.GetBytes(_key);
                des.IV = ASCIIEncoding.ASCII.GetBytes(_key);
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                }
                ms.Close();
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 进行DES解密。
        /// </summary>
        /// <param name="todecrypt">要解密的以Base64</param>
        /// <param name="key">密钥，且必须为8位。</param>
        /// <returns>已解密的字符串。</returns>
        public static byte[] Decrypt(byte[] inputByteArray)
        {
            try
            {
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    des.Key = ASCIIEncoding.ASCII.GetBytes(_key);
                    des.IV = ASCIIEncoding.ASCII.GetBytes(_key);
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        cs.Close();
                    }
                    ms.Close();
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
