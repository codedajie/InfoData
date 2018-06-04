using ChivaVR.Net.Core;
using ChivaVR.Net.Packet;
using ChivaVR.Net.Toolkit;
using System;
using System.Net.Sockets;
using System.Threading;
/// <summary>
/// 张志杰 20170117
/// 客户端
/// </summary>
namespace ChivaVR.Net.IC
{
    /// <summary>
    /// 信息中心
    /// 公共部分
    /// </summary>
    public sealed partial class Infocenter : IChivaVRNet
    {
        #region 属性

        /// <summary>
        /// 组件版本
        /// </summary>
        public string Version { get { return "2.0.0301.1201"; } }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get { return _user; } }

        /// <summary>
        /// 登录密码
        /// </summary>
        public string Password { get { return _pwd; } }

        /// <summary>
        /// 是否连接
        /// </summary>
        public bool Connected { get { return _connected; } }

        /// <summary>
        /// 客户端标识
        /// </summary>
        public int Id { get { return _id; } set { _id = value; } }

        /// <summary>
        /// 超级用户
        /// </summary>
        public static UserItem Admin = UserItem.From("chiva_admin", "admin_chiva");

        #endregion

        #region 事件
        /// <summary>
        /// 通知
        /// </summary>
        public event CVRNotifyEventHandler Notified = null;
        #endregion

        #region 方法
        /// <summary>
        /// 获取Infocenter实例
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public static Infocenter Create(string ip, int port, string user, string pwd)
        {
            if (_instance == null)
            {
                _instance = new Infocenter(ip, port, user, pwd);
            }
            else
            {
                _instance.Notified?.Invoke(CVResult.InfocenterAlreadyConnected, _instance);
            }

            return _instance;
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            if (_client == null)
            {
                //复位RC4输入输出增量值
                CustomRC4Key.CreateCustomRC4Key().BuildKey();
                _instance.Connect();

                Thread thread = new Thread(ParseMinaQueueThread);
                thread.IsBackground = true;
                thread.Start();

                //登录
                OpFactory<UserOp>.Create(_instance).Get(_user, _pwd, Id, "self-connecting logon");
            }
            else
            {
                _instance.Notified?.Invoke(CVResult.InfocenterAlreadyStarted, _instance);
            }
        }

        /// <summary>
        /// 向服务器发送命令
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool Execute<T>(int command, T t)
        {
            try
            {
                byte[] buff = ProtobufWrapper.Packaging<T>(command, t);
                _client?.Send(buff);

                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Shutdown()
        {
            try
            {
                if (_client == null)
                {
                    throw new NullReferenceException("尚未连接信息中心");
                }

                Bye();

                if (_parser != null)
                {
                    _parser.Notified -= _parser_Info;
                }

                _client.Shutdown(SocketShutdown.Both);
                _client.Close();

                Notified?.Invoke(CVResult.InfocenterDisconnected, this);
            }
            catch (NullReferenceException)
            {
                Notified?.Invoke(CVResult.InfocenterNotConnected, this);
            }
            catch (SocketException ex)
            {
                Notified?.Invoke(CVResult.SocketExceptionHappened, ex);
            }
            catch (ObjectDisposedException)
            {
                Notified?.Invoke(CVResult.InfocenterDisconnected, this);
            }
            finally
            {
                Reset();
            }
        }

        #endregion
    }
}