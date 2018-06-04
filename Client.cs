using ChivaVR.Net.Core;
using ChivaVR.Net.Toolkit;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
/// <summary>
/// 张志杰 20170117
/// 信息中心
/// </summary>
namespace ChivaVR.Net.IC
{
    /// <summary>
    /// 信息中心
    /// 封装部分
    /// </summary>
    public sealed partial class Infocenter : IChivaVRNet
    {
        private static Infocenter _instance = null;

        private Socket _client = null;
        private Parser _parser;
        private IPEndPoint _remoteEP;

        private int _id = 0;
        private string _ip = "";
        private int _port = 0;
        private string _user;
        private string _pwd;

        private bool _connected = false;

        MinaQueue _minaQueue = new MinaQueue();

        /// <summary>
        /// socket数据接收缓冲大小
        /// </summary>
        private static int _bufferSize = 2048 * 64;

        /// <summary>
        /// 客户端实例
        /// </summary>
        /// <param name="ip">信息中心ip，应保证其合法性</param>
        /// <param name="port"></param>
        private Infocenter(string ip, int port, string user, string pwd)
        {
            _ip = ip;
            _port = port;
            _user = user;
            _pwd = pwd;

            if (string.IsNullOrEmpty(_ip)
                || string.IsNullOrEmpty(_user)
                || string.IsNullOrEmpty(_pwd)
                || _port == 0
                )
            {
                Notified?.Invoke(CVResult.InfocenterParameterHappened, "服务器参数配置问题");
            }

            _connected = false;
            _remoteEP = new IPEndPoint(IPAddress.Parse(ip), port);

            //本机特征
            _localIp = Kits.GetIp();
            _localName = Environment.MachineName;
            Log.Added += AddLog;
        }

        private void _parser_Responded(object obj)
        {
            Notified?.Invoke(CVResult.ResourceOperationResponded, obj);
        }

        private Infocenter(Socket c) { _client = c; }

        private void _parser_Info(CVResult result, object obj)
        {
            Notified?.Invoke(result, obj);
        }

        private void Connect()
        {
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _client.BeginConnect(_remoteEP, new AsyncCallback(ConnectCallback), _client);
            _connected = true;
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);

                //连接成功，创建剖析器
                _parser = new InfocenterParser(this);
                _parser.Notified += _parser_Info;

                //创建日志读写操作
                _logTableOp = OpFactory<TableOp>.Create(_instance);

                while (true)
                {
                    if (_client != null)
                    {
                        byte[] buff = new byte[_bufferSize];
                        int bytesRead = _client.Receive(buff);
                        _minaQueue.Enqueue(buff, bytesRead);
                    }
                    else
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
            }
            catch (SocketException ex)
            {
                Reset();

                Notified?.Invoke(ex.SocketErrorCode == SocketError.TimedOut
                    ? CVResult.ServerConnectTimeouted : CVResult.ConnectFailed, ex);
            }
        }

        private void ParseMinaQueueThread()
        {
            try
            {
                while (true)
                {
                    byte[] body = _minaQueue.Dequeue();

                    if (body != null)
                    {
                        _parser.Do(RC4.Convert(body, 4));
                    }

                    Thread.Sleep(10);
                }
            }
            catch (SocketException ex)
            {
                Shutdown();
                Notified?.Invoke(CVResult.ReceiveCallbackFailed, ex);
            }
            catch (ObjectDisposedException)
            {
                _connected = false;
                Notified?.Invoke(CVResult.InfocenterNotConnected, this);
            }
            catch (Exception ex)
            {
                Notified?.Invoke(CVResult.ExceptionHappened, ex);
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="buff"></param>
        internal void Send(byte[] buff)
        {
            try
            {
                if (_client == null)
                {
                    throw new NullReferenceException("未连接");
                }

                _client.BeginSend(buff, 0, buff.Length, 0, new AsyncCallback(SendCallback), _client);
            }
            catch (SocketException sex)
            {
                _connected = false;
                Notified?.Invoke(CVResult.SendFailed, sex);
            }
            catch (NullReferenceException)
            {
                _connected = false;
                Notified?.Invoke(CVResult.InfocenterNotConnected, this);
            }
        }

        private void Reset()
        {
            this.Id = 0;
            _client = null;
            _instance = null;
            _connected = false;
        }

        private void Bye()
        {
            try
            {
                OpFactory<ByeOp>.Create(_instance).Say(Id, "shutdown itself");
            }
            catch
            {

            }
        }

        internal void Notify(CVResult r, object o)
        {
            Notified?.Invoke(r, o);
        }

        private void SendCallback(IAsyncResult ar)
        {
            Socket client = (Socket)ar.AsyncState;
            if (client.Connected)
            {
                int bytesSent = client.EndSend(ar);
            }
        }
    }
}
