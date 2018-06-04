using System;

namespace ChivaVR.Net.Toolkit
{
    public class CustomRC4Key
    {
        //public  static int MAX_CONTINUOUS_STREAM_LIMIT = Integer.MAX_VALUE - 100000000;
        public static String SIGN = "ENCRYPT";

        private String clientKey;
        private String serverKey;
        private byte[] complexKey;
        //输入流增量
        private int inputCounter = 0;
        //输出流增量
        private int outputCounter = 0;
        private bool refresh;
        private static CustomRC4Key customRC4Key;
        private CustomRC4Key()
        {
            this.complexKey = new byte[] { 0 };
        }

        public static CustomRC4Key CreateCustomRC4Key()
        {
            if (customRC4Key == null)
            {
                customRC4Key = new CustomRC4Key();
                // customRC4Key.BuildDefaultKey();
            }
            return customRC4Key;

        }

        public String GetServerKey()
        {
            return serverKey;
        }

        public void SetServerKey(String serverKey)
        {
            this.serverKey = serverKey;
        }

        public String GetClientKey()
        {
            return clientKey;
        }

        public void SetClientKey(String clientKey)
        {
            this.clientKey = clientKey;
        }

        public byte[] GetComplexKey()
        {
            return complexKey;
        }

        public int GetOutputCounter()
        {
            return outputCounter;
            //		return 0;
        }

        public int GetInputCounter()
        {
            return inputCounter;
            //		return 0;
        }

        //public void InputCounterIncrease(int len)
        //{
        //    // System.out.println("iClient:len === " + len);
        //    this.inputCounter += len;
        //    //System.out.println("iClient:this.inputCounter===" + this.inputCounter);
        //}

        public void InputCounterDecrease(int len)
        {
            this.inputCounter -= len;
        }

        public void OutputCounterIncrease(int len)
        {
            //System.out.println("iClient：outputCounter === " + len);
            this.outputCounter += len;
            // System.out.println("iClient:outputCounter===" + this.outputCounter);
        }

        public bool IsRefresh()
        {
            return refresh;
        }

        public void NotifyRefreshOnEncode()
        {
            this.refresh = true;
        }

        /**
         *当重新连接服务器的时候需要输入和输出增量复位 
         */
        public void BuildKey()
        {
            this.inputCounter = 0;
            this.outputCounter = 0;
        }

        /**
         * 调用rc4
         *  <br> (非 Javadoc)
         * @param key
         */
        public void BuildDefaultKey(String key)
        {

            this.complexKey = RC4.GetComplexKey(key);
            this.refresh = false;
        }

        /**
         * 调用rc4获得initKey
         *  <br> (非 Javadoc)
         */
        public void BuildComplexKey()
        {
            // 考虑到非极端恶劣线程队列堵塞现象   
            //这个过程无需加锁.  
            //但如果一定要处理极端情况请按需对此过程进行加锁
            this.complexKey = RC4.GetComplexKey(clientKey + serverKey);
            this.inputCounter = 0;
            this.outputCounter = 0;
            this.refresh = false;
        }

        public static int GetDecRC4Len(byte[] src)
        {
            CustomRC4Key key = CustomRC4Key.CreateCustomRC4Key();

            byte[] blen = new byte[4];
            blen[0] = src[0];
            blen[1] = src[1];
            blen[2] = src[2];
            blen[3] = src[3];

            blen = RC4.Convert(blen, 0);
            //key.InputCounterIncrease(blen.Length);

            byte[] rblen = new byte[4];
            rblen[0] = blen[3];
            rblen[1] = blen[2];
            rblen[2] = blen[1];
            rblen[3] = blen[0];

            return BitConverter.ToInt32(rblen, 0);
        }

        public static byte[] DecRC4(byte[] src)
        {
            int len = GetDecRC4Len(src);

            if (len < 3)
            {
                return null;
            }

            byte[] body = new byte[len];

            //读取消息体
            Buffer.BlockCopy(src, 4, body, 0, len);
            //解密消息体
            body = RC4.Convert(body, 4);

            return body;
        }
    }
}
