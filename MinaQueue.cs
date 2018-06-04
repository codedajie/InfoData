using ChivaVR.Net.Toolkit;
using System;

namespace ChivaVR.Net.IC
{
    public class MinaQueue
    {
        private byte[] _buff = null;

        public void Enqueue(byte[] buff, int bytesRead)
        {
            lock (this)
            {
                if (_buff == null)
                {
                    _buff = new byte[bytesRead];
                    Buffer.BlockCopy(buff, 0, _buff, 0, bytesRead);
                }
                else
                {
                    byte[] temp = new byte[bytesRead + _buff.Length];
                    Buffer.BlockCopy(_buff, 0, temp, 0, _buff.Length);
                    Buffer.BlockCopy(buff, 0, temp, _buff.Length, bytesRead);
                    _buff = temp;
                }
            }
        }

        public byte[] Dequeue()
        {
            lock (this)
            {
                if (_buff == null || _buff.Length < 4)
                {
                    return null;
                }

                //整包长度
                int len = CustomRC4Key.GetDecRC4Len(_buff);
                //是否整包
                if (_buff.Length < len)
                {
                    return null;
                }
                //整包
                byte[] body = new byte[len];
                Buffer.BlockCopy(_buff, 4, body, 0, len);

                if (_buff.Length - len >= 4)
                {
                    //剩余包
                    byte[] temp = new byte[_buff.Length - len - 4];
                    Buffer.BlockCopy(_buff, len + 4, temp, 0, _buff.Length - len - 4);
                    _buff = temp;

                    //可用包
                    return body;
                }
                else
                {
                    //没有可用包
                    return null;
                }
            }
        }
    }
}
