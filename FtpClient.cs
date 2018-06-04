using System;
using System.Net;
using System.IO;
using System.Text;
using System.Net.Sockets;

namespace ChivaVR.Net.DC
{
    /// <summary>
    /// FTP Client
    /// </summary>
    public class FtpClient
    {
        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        protected FtpClient(string ip, string remoteDir, string user, string pwd, int port)
        {
            Ip = ip;
            RemoteDirectory = remoteDir;
            User = user;
            Pwd = pwd;
            Port = port;
            Connected = false;
        }
        #endregion

        #region 属性
        /// <summary>
        /// FTP服务器IP地址
        /// </summary>
        public string Ip
        {
            get; set;
        }
        /// <summary>
        /// FTP服务器端口
        /// </summary>
        public int Port
        {
            get; set;
        }
        /// <summary>
        /// 当前服务器目录
        /// </summary>
        public string RemoteDirectory
        {
            get; set;
        }
        /// <summary>
        /// 登录用户账号
        /// </summary>
        public string User
        {
            internal get; set;
        }
        /// <summary>
        /// 用户登录密码
        /// </summary>
        public string Pwd
        {
            internal get; set;
        }

        /// <summary>
        /// 是否登录
        /// </summary>
        public bool Connected
        {
            get; internal set;
        }
        #endregion

        #region 链接
        /// <summary>
        /// 建立连接 
        /// </summary>
        protected void Connect()
        {
            if (Connected)
            {
                return;
            }
            lock (this)
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ep = new IPEndPoint(IPAddress.Parse(Ip), Port);
                //链接
                try
                {
                    _socket.Connect(ep);
                }
                catch (Exception)
                {
                    throw new IOException("Couldn't connect to remote server");
                }

                //获取应答码
                ReadReply();
                if (_code != 220)
                {
                    Disconnect();
                    throw new IOException(_reply.Substring(4));
                }

                //登陆
                SendCommand("USER " + User);
                if (!(_code == 331 || _code == 230))
                {
                    CloseSocketConnect();//关闭连接
                    throw new IOException(_reply.Substring(4));
                }
                if (_code != 230)
                {
                    SendCommand("PASS " + Pwd);
                    if (!(_code == 230 || _code == 202))
                    {
                        CloseSocketConnect();//关闭连接
                        throw new IOException(_reply.Substring(4));
                    }
                }
                Connected = true;
                //切换到目录
                ChDir(RemoteDirectory);
            }
        }


        /// <summary>
        /// 关闭连接
        /// </summary>
        protected void Disconnect()
        {
            if (_socket != null)
            {
                SendCommand("QUIT");
            }
            CloseSocketConnect();
        }

        #endregion

        #region 传输模式

        /// <summary>
        /// 传输模式:二进制类型、ASCII类型
        /// </summary>
        protected enum TransferType { Binary, ASCII };

        /// <summary>
        /// 设置传输模式
        /// </summary>
        /// <param name="ttType">传输模式</param>
        protected void SetTransferType(TransferType ttType)
        {
            if (ttType == TransferType.Binary)
            {
                SendCommand("TYPE I");//binary类型传输
            }
            else
            {
                SendCommand("TYPE A");//ASCII类型传输
            }
            if (_code != 200)
            {
                throw new IOException(_reply.Substring(4));
            }
            else
            {
                _type = ttType;
            }
        }


        /// <summary>
        /// 获得传输模式
        /// </summary>
        /// <returns>传输模式</returns>
        protected TransferType GetTransferType()
        {
            return _type;
        }

        public string GetServerOS()
        {
            // 建立链接
            if (!Connected)
            {
                Connect();
            }

            //建立进行数据连接的socket
            Socket socketData = CreateDataSocket();

            //传送命令

            SendCommand("SYST");

            //分析应答代码
            if (_code != 215)
            {
                throw new IOException(_reply.Substring(4));
            }

            //获得结果
            char[] seperator = { '\n', '\r' };
            string[] strs = _reply.Substring(4).Split(seperator, StringSplitOptions.RemoveEmptyEntries);
            _msg = strs[0];

            socketData.Close();//数据socket关闭时也会有返回码

            return _msg;
        }

        #endregion

        #region 文件操作
        /// <summary>
        /// 获得文件列表
        /// </summary>
        /// <param name="strMask">文件名的匹配字符串</param>
        /// <returns></returns>
        protected string[] Dir(string strMask)
        {
            // 建立链接
            if (!Connected)
            {
                Connect();
            }

            //建立进行数据连接的socket
            Socket socketData = CreateDataSocket();

            //传送命令

            SendCommand("NLST " + strMask);

            //分析应答代码
            if (!(_code == 150 || _code == 125 || _code == 226))
            {
                throw new IOException(_reply);
            }

            //获得结果
            _msg = "";
            while (true)
            {
                int iBytes = socketData.Receive(_buffer, _buffer.Length, 0);
                _msg += _encoding.GetString(_buffer, 0, iBytes);
                if (iBytes < _buffer.Length)
                {
                    break;
                }
            }
            char[] seperator = { '\n', '\r' };
            string[] strsFileList = _msg.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
            socketData.Close();//数据socket关闭时也会有返回码

            if (_code != 226)
            {
                ReadReply();
                if (_code != 226)
                {
                    throw new IOException(_reply);
                }
            }
            return strsFileList;
        }

        /// <summary>
        /// LIST <name>	
        /// </summary>
        /// <param name="name">如果是文件名列出文件信息，如果是目录则列出文件列表</param>
        /// <returns></returns>
        protected string[] List(string name)
        {
            // 建立链接
            if (!Connected)
            {
                Connect();
            }

            //建立进行数据连接的socket
            Socket socketData = CreateDataSocket();

            //传送命令

            SendCommand("LIST " + name);

            //分析应答代码
            if (!(_code == 150 || _code == 125 || _code == 226))
            {
                throw new IOException(_reply);
            }

            //获得结果
            _msg = "";
            while (true)
            {
                int iBytes = socketData.Receive(_buffer, _buffer.Length, 0);
                _msg += _encoding.GetString(_buffer, 0, iBytes);
                if (iBytes < _buffer.Length)
                {
                    break;
                }
            }
            char[] seperator = { '\n', '\r' };
            string[] strsFileList = _msg.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
            socketData.Close();//数据socket关闭时也会有返回码

            if (_code != 226)
            {
                ReadReply();
                if (_code != 226)
                {
                    throw new IOException(_reply);
                }
            }
            return strsFileList;
        }


        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="strFileName">文件名</param>
        /// <returns>文件大小</returns>
        protected long GetFileSize(string strFileName)
        {
            if (!Connected)
            {
                Connect();
            }
            SendCommand("SIZE " + strFileName);
            long lSize = 0;
            if (_code == 213)
            {
                lSize = Int64.Parse(_reply.Substring(4));
            }
            else
            {
                throw new IOException(_reply);
            }
            return lSize;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="filename">待删除文件名</param>
        protected void Delete(string filename)
        {
            if (!Connected)
            {
                Connect();
            }
            SendCommand("DELE " + filename);
            if (_code != 250)
            {
                throw new IOException(_reply);
            }
        }

        /// <summary>
        /// 重命名(如果新文件名与已有文件重名,将覆盖已有文件)
        /// </summary>
        /// <param name="strOldFileName">旧文件名</param>
        /// <param name="strNewFileName">新文件名</param>
        protected void Rename(string strOldFileName, string strNewFileName)
        {
            if (!Connected)
            {
                Connect();
            }
            SendCommand("RNFR " + strOldFileName);
            if (_code != 350)
            {
                throw new IOException(_reply);
            }
            //  如果新文件名与原有文件重名,将覆盖原有文件
            SendCommand("RNTO " + strNewFileName);
            if (_code != 250)
            {
                throw new IOException(_reply);
            }
        }
        #endregion

        #region 目录操作
        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="strDirName">目录名</param>
        protected void MkDir(string strDirName)
        {
            if (!Connected)
            {
                Connect();
            }
            SendCommand("MKD " + strDirName);
            if (!(_code == 257 || _code == 250))
            {
                throw new IOException(_reply.Substring(4));
            }
        }


        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="strDirName">目录名</param>
        protected void RmDir(string strDirName)
        {
            if (!Connected)
            {
                Connect();
            }
            SendCommand("RMD " + strDirName);
            if (_code != 250)
            {
                throw new IOException(_reply.Substring(4));
            }
        }


        /// <summary>
        /// 改变目录
        /// </summary>
        /// <param name="strDirName">新的工作目录名</param>
        protected void ChDir(string strDirName, bool createDirectory = false)
        {
            if (strDirName.Equals(".") || strDirName.Equals(""))
            {
                return;
            }
            if (!Connected)
            {
                Connect();
            }
            char[] split_item = new char[1];
            split_item[0] = '/';
            string[] arr = strDirName.Split(split_item, StringSplitOptions.RemoveEmptyEntries);
            foreach (string a in arr)
            {
                SendCommand("CWD " + a);
                if (_code != 250)
                {
                    if (createDirectory)
                    {
                        this.MkDir(a);
                        SendCommand("CWD " + a);
                    }
                    else
                    {
                        throw new IOException(_reply);
                    }
                }
            }
            SendCommand("CWD " + "/");
            SendCommand("CWD " + strDirName);
            if (_code != 250)
            {
                throw new IOException("ChDir " + _reply);
            }
        }

        #endregion

        #region 内部变量
        /// <summary>
        /// 服务器返回的应答信息(包含应答码)
        /// </summary>
        protected string _msg;
        /// <summary>
        /// 服务器返回的应答信息(包含应答码)
        /// </summary>
        protected string _reply;
        /// <summary>
        /// 服务器返回的应答码

        /// </summary>
        protected int _code;
        /// <summary>
        /// 进行控制连接的socket
        /// </summary>
        protected Socket _socket;
        /// <summary>
        /// 传输模式
        /// </summary>
        protected TransferType _type;
        /// <summary>
        /// 接收和发送数据的缓冲区
        /// </summary>
        protected static int BufferSize = 512;
        protected byte[] _buffer = new byte[BufferSize];
        /// <summary>
        /// 编码方式
        /// </summary>
        protected Encoding _encoding = Encoding.Default;
        #endregion

        #region 内部函数
        /// <summary>
        /// 将一行应答字符串记录在strReply和strMsg
        /// 应答码记录在iReplyCode
        /// </summary>
        protected void ReadReply()
        {
            _msg = "";
            _reply = ReadLine();
            _code = Int32.Parse(_reply.Substring(0, 3));
        }

        /// <summary>
        /// 建立进行数据连接的socket
        /// </summary>
        /// <returns>数据连接socket</returns>
        protected Socket CreateDataSocket()
        {
            SendCommand("PASV");
            if (_code != 227)
            {
                throw new IOException(_reply.Substring(4));
            }

            int index1 = _reply.IndexOf('(');
            int index2 = _reply.IndexOf(')');
            string ipData =
             _reply.Substring(index1 + 1, index2 - index1 - 1);
            int[] parts = new int[6];
            int len = ipData.Length;
            int partCount = 0;
            string buf = "";
            for (int i = 0; i < len && partCount <= 6; i++)
            {
                char ch = Char.Parse(ipData.Substring(i, 1));
                if (Char.IsDigit(ch))
                    buf += ch;
                else if (ch != ',')
                {
                    throw new IOException("Malformed PASV strReply: " + _reply);
                }
                if (ch == ',' || i + 1 == len)
                {
                    try
                    {
                        parts[partCount++] = Int32.Parse(buf);
                        buf = "";
                    }
                    catch (Exception)
                    {
                        throw new IOException("Malformed PASV strReply: " + _reply);
                    }
                }
            }
            string ipAddress = parts[0] + "." + parts[1] + "." + parts[2] + "." + parts[3];
            int port = (parts[4] << 8) + parts[5];
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            try
            {
                s.Connect(ep);
            }
            catch (Exception)
            {
                throw new IOException("Can't connect to remote server");
            }
            return s;
        }

        /// <summary>
        /// 关闭socket连接(用于登录以前)
        /// </summary>
        private void CloseSocketConnect()
        {
            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }
            Connected = false;
        }

        /// <summary>
        /// 读取Socket返回的所有字符串
        /// </summary>
        /// <returns>包含应答码的字符串行</returns>
        private string ReadLine()
        {
            while (true)
            {
                int iBytes = _socket.Receive(_buffer, _buffer.Length, 0);
                _msg += _encoding.GetString(_buffer, 0, iBytes);
                if (iBytes < _buffer.Length)
                {
                    break;
                }
            }
            char[] seperator = { '\n' };
            string[] mess = _msg.Split(seperator);
            if (_msg.Length > 2)
            {
                _msg = mess[mess.Length - 2];
                //seperator[0]是10,换行符是由13和0组成的,分隔后10后面虽没有字符串,
                //但也会分配为空字符串给后面(也是最后一个)字符串数组,
                //所以最后一个mess是没用的空字符串
                //但为什么不直接取mess[0],因为只有最后一行字符串应答码与信息之间有空格

            }
            else
            {
                _msg = mess[0];
            }
            if (!_msg.Substring(3, 1).Equals(" "))//返回字符串正确的是以应答码(如220开头,后面接一空格,再接问候字符串)
            {
                return ReadLine();
            }
            return _msg;
        }


        /// <summary>
        /// 发送命令并获取应答码和最后一行应答字符串
        /// </summary>
        protected void SendCommand(String strCommand)
        {
            try
            {
                Byte[] cmdBytes = _encoding.GetBytes((strCommand + "\r\n").ToCharArray());
                _socket.Send(cmdBytes, cmdBytes.Length, 0);
                ReadReply();
            }
            catch (Exception ex)
            {
                throw new IOException(ex.Message);
            }
        }

        #endregion
    }
}

