using ChivaVR.Net.DC;

namespace ChivaVR.Net.Unity
{
    public static class DcWrapper
    {
        //数据中心实例
        public static Datacenter Dc { get; set; }

        //属性
        public static string Ip { internal get; set; }
        public static int Port { internal get; set; }
        public static string User { internal get; set; }
        public static string Pwd { internal get; set; }
        public static string Remote { internal get; set; }
        public static string Local { internal get; set; }

        public static void Connect(string ip, int port, string user, string pwd, string remote, string local)
        {
            Disconnect();

            Ip = ip;
            Port = port;
            User = user;
            Pwd = pwd;
            Remote = remote;
            Local = local;

            Dc = Datacenter.Create(Ip, Port, user, pwd, remote, local);
            Dc.Start();
        }

        public static void Connect(string user, string pwd, string remote, string local)
        {
            Disconnect();

            User = user;
            Pwd = pwd;
            Remote = remote;
            Local = local;

            Dc = Datacenter.Create(Ip, Port, user, pwd, remote, local);
            Dc.Start();
        }

        public static void Connect()
        {
            Connect(User, Pwd, Remote, Local);
        }

        public static void Disconnect()
        {
            if (Dc != null)
            {
                Dc.Shutdown();
                Dc = null;
            }
        }
    }
}
