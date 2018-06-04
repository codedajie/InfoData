using ChivaVR.Net.IC;
/// <summary>
/// 信息中心工具
/// ChivaVR.Net for unity
/// zzj
/// 20170804
/// 20180111
/// </summary>
namespace ChivaVR.Net.Unity
{
    public static class IcWrapper
    {
        //信息中心实例
        public static Infocenter Ic { get; set; }

        //信息中心属性
        public static string Ip { internal get; set; }
        public static int Port { internal get; set; }
        public static string User { internal get; set; }
        public static string Pwd { internal get; set; }

        /// <summary>
        /// 仅连接一次
        /// 若需要再次连接，需要Disconnect
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        public static Infocenter Connect(string ip, int port, string user, string pwd)
        {
            Ip = ip;
            Port = port;
            User = user;
            Pwd = pwd;

            //连接服务器
            Ic = Infocenter.Create(Ip, Port, user, pwd);
            Ic.Start();

            return Ic;
        }

        public static Infocenter Connect(string user, string pwd)
        {
            return Connect(Ip, Port, user, pwd);
        }

        public static Infocenter Connect()
        {
            return Connect(User, Pwd);
        }

        /// <summary>
        /// 断开链接
        /// 游戏对象销毁时，应调用
        /// </summary>
        public static void Disconnect()
        {
            if (Ic != null)
            {
                Ic.Shutdown();
                Ic = null;
            }
        }
    }
}