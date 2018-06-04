using System;
using System.Text;

namespace ChivaVR.Net.Toolkit
{
    public static class RC4
    {
        public static byte[] Convert(byte[] buff, int pos)
        {
            return RC4.RC4Custom(buff, RC4.GetComplexKey(_key), pos);
        }


        private static string _key = "*qifan*";

        public static byte[] GetComplexKey(string key)
        {
            byte[] S = initKey(key);
            byte[] K = new byte[S.Length];
            int i = 0, j = 0;

            for (int n = 0; n < S.Length; n++)
            {
                i = (i + 1) & 0xff;
                j = (j + (S[i] & 0xff)) & 0xff;
                byte tmp = S[i];
                S[i] = S[j];
                S[j] = tmp;
                K[n] = S[((S[i] & 0xff) + (S[j] & 0xff)) & 0xff];
            }
            return K;
        }

        public static byte[] RC4Custom(byte[] input, byte[] key, int pos)
        {
            byte[] result = new byte[input.Length];
            int keyPos = pos & 0xff;
            for (int i = 0; i < input.Length; i++, keyPos++)
            {
                if (keyPos > 0xff)
                {
                    keyPos = 0;
                }
                result[i] = (byte)(input[i] ^ key[keyPos]);
            }
            return result;
        }

        private static string DecryRC4(byte[] data, string key)
        {
            if (data == null || key == null)
            {
                return null;
            }
            return asString(RC4Base(data, key));
        }

        private static string DecryRC4(string data, string key)
        {
            if (data == null || key == null)
            {
                return null;
            }
            return Encoding.UTF8.GetString(RC4Base(HexString2Bytes(data), key));
        }

        private static byte[] EncryRC4Byte(string data, string key)
        {
            if (data == null || key == null)
            {
                return null;
            }
            byte[] b_data = Encoding.UTF8.GetBytes(data);
            return RC4Base(b_data, key);
        }

        private static string encry_RC4_string(string data, string key)
        {
            if (data == null || key == null)
            {
                return null;
            }
            return toHexString(asString(EncryRC4Byte(data, key)));
        }

        private static string asString(byte[] buf)
        {
            StringBuilder strbuf = new StringBuilder(buf.Length);
            for (int i = 0; i < buf.Length; i++)
            {
                strbuf.Append((char)buf[i]);
            }
            return strbuf.ToString();
        }

        private static byte[] initKey(string aKey)
        {
            byte[] b_key = Encoding.UTF8.GetBytes(aKey);
            byte[] state = new byte[256];

            for (int i = 0; i < 256; i++)
            {
                state[i] = (byte)i;
            }
            int index1 = 0;
            int j = 0;
            if (b_key == null || b_key.Length == 0)
            {
                return null;
            }
            for (int i = 0; i < 256; i++)
            {
                j = (j + (state[i] & 0xff) + (b_key[index1] & 0xff)) & 0xff;
                byte tmp = state[i];
                state[i] = state[j];
                state[j] = tmp;
                index1 = (index1 + 1) % b_key.Length;
            }
            return state;
        }

        private static string toHexString(string s)
        {
            string str = "";
            for (int i = 0; i < s.Length; i++)
            {
                int ch = (int)s[i];
                string s4 = String.Format("{0:X2}", ch);//. Integer.toHexString(ch & 0xFF);//sb.AppendFormat("{0:X2}", value);
                if (s4.Length == 1)
                {
                    s4 = '0' + s4;
                }
                str = str + s4;
            }
            return str;// 0x表示十六进制  
        }

        private static byte[] HexString2Bytes(string src)
        {
            int size = src.Length;
            byte[] ret = new byte[size / 2];
            byte[] tmp = Encoding.UTF8.GetBytes(src);
            for (int i = 0; i < size / 2; i++)
            {
                ret[i] = uniteBytes(tmp[i * 2], tmp[i * 2 + 1]);
            }
            return ret;
        }

        private static byte uniteBytes(byte src0, byte src1)
        {
            char _b0 = (char)src0;
            _b0 = (char)(_b0 << 4);
            char _b1 = (char)src1;
            byte ret = (byte)(_b0 ^ _b1);
            return ret;
        }

        private static byte[] RC4Base(byte[] input, string mKkey)
        {
            int x = 0;
            int y = 0;
            byte[] key = initKey(mKkey);
            int xorIndex;
            byte[] result = new byte[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                x = (x + 1) & 0xff;
                y = ((key[x] & 0xff) + y) & 0xff;
                byte tmp = key[x];
                key[x] = key[y];
                key[y] = tmp;
                xorIndex = ((key[x] & 0xff) + (key[y] & 0xff)) & 0xff;
                result[i] = (byte)(input[i] ^ key[xorIndex]);
            }
            return result;
        }
    }
}
