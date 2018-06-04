using ChivaVR.Net.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ChivaVR.Net.DC
{
    public sealed partial class Datacenter : FtpClient, IChivaVRNet
    {
        private static Datacenter _instance = null;

        private DataOpQue _dataOpQue;
        private Thread _thread = null;

        private Datacenter(string ip, int port, string user, string pwd, string remoteDirectory, string localDirectory)
            : base(ip, remoteDirectory, user, pwd, port)
        {
            LocalDirectory = localDirectory;
        }

        public void UploadFile(UploadDataItem udi)
        {
            lock (this)
            {
                udi.Success = false;

                string localpathfile = udi.Source;
                string remotepathfile = udi.Target;

                try
                {
                    if (!Connected)
                    {
                        Connect();
                    }

                    if (!File.Exists(localpathfile))
                    {
                        throw new FileNotFoundException("找不到文件:" + Path.GetFileName(localpathfile));
                    }
                    if (udi.Replied) Began?.Invoke(udi, (new FileInfo(localpathfile)).Length);

                    string name = udi.Target.Replace(Path.GetFileName(udi.Target), "");
                    string dir = name.Replace(CurrentRemotePath + Path.AltDirectorySeparatorChar, "");
                    string filename = Path.Combine(dir, Path.GetFileName(udi.Target));

                    Socket socketData = CreateDataSocket();
                    SendCommand("STOR " + filename);
                    if (!(_code == 125 || _code == 150))
                    {
                        throw new IOException(_reply);
                    }
                    FileStream input = new FileStream(localpathfile, FileMode.Open);
                    int iBytes = 0;
                    while ((iBytes = input.Read(_buffer, 0, _buffer.Length)) > 0)
                    {
                        socketData.Send(_buffer, iBytes, 0);
                        if (udi.Replied) Stepped?.Invoke(udi, iBytes);
                    }
                    input.Close();
                    if (socketData.Connected)
                    {
                        socketData.Close();
                    }
                    if (!(_code == 226 || _code == 250))
                    {
                        ReadReply();
                        if (!(_code == 226 || _code == 250))
                        {
                            throw new IOException(_reply.Substring(4));
                        }
                    }

                    udi.Success = true;
                }
                catch (FileNotFoundException fnfe)
                {
                    udi.Message = fnfe.Message;
                    Notified?.Invoke(CVResult.FileNotFoundExceptionHanppened, fnfe);
                }
                catch (IOException ex)
                {
                    udi.Message = ex.Message;
                    Notified?.Invoke(CVResult.UploadExceptionHappened, udi);
                }
                catch (Exception ex)
                {
                    throw new IOException(ex.Message);
                }
                finally
                {
                    if (udi.Replied) Ended?.Invoke(udi);
                }
            }
        }
        public void DownloadFile(DownloadDataItem ddi)
        {
            lock (this)
            {
                ddi.Success = false;

                string strRemoteFileName = ddi.Source.Replace(CurrentRemotePath + Path.AltDirectorySeparatorChar, "");

                string strFolder = Path.GetDirectoryName(ddi.Target);
                string strLocalFileName = Path.GetFileName(ddi.Target);

                string localpathfile = ddi.Target;

                try
                {
                    if (!Connected)
                    {
                        Connect();
                    }

                    if (ddi.CreateDirectory && !Directory.Exists(strFolder))
                    {
                        Directory.CreateDirectory(strFolder);
                    }

                    long size = GetFileSize(strRemoteFileName);
                    if (ddi.Replied) Began?.Invoke(ddi, size);

                    SetTransferType(TransferType.Binary);
                    if (strLocalFileName.Equals(""))
                    {
                        strLocalFileName = strRemoteFileName;
                    }
                    Socket socketData = CreateDataSocket();
                    SendCommand("RETR " + strRemoteFileName);
                    if (!(_code == 150 || _code == 125
                    || _code == 226 || _code == 250))
                    {
                        throw new IOException(_reply);
                    }
                    FileStream output = new FileStream(strFolder + "\\" + strLocalFileName, FileMode.Create);
                    while (true)
                    {
                        int iBytes = socketData.Receive(_buffer, _buffer.Length, 0);
                        output.Write(_buffer, 0, iBytes);
                        if (ddi.Replied) Stepped?.Invoke(ddi, iBytes);
                        if (iBytes <= 0)
                        {
                            break;
                        }
                    }
                    output.Close();
                    if (socketData.Connected)
                    {
                        socketData.Close();
                    }
                    if (!(_code == 226 || _code == 250))
                    {
                        ReadReply();
                        if (!(_code == 226 || _code == 250))
                        {
                            throw new IOException(_reply);
                        }
                    }

                    ddi.Success = true;
                }
                catch (IOException ioex)
                {
                    ddi.Message = ioex.Message;
                    Notified?.Invoke(CVResult.IOExceptionHappened, ddi);
                }
                catch (Exception ex)
                {
                    ddi.Message = ex.Message;
                    Notified?.Invoke(CVResult.DownloadExceptionHappened, ddi);
                }
                finally
                {
                    if (ddi.Replied) Ended?.Invoke(ddi);
                }
            }
        }

        public void DownloadMemory(DownloadMemoryItem dmi)
        {
            lock (this)
            {
                dmi.Success = false;

                string strRemoteFileName = dmi.Source.Replace(CurrentRemotePath + Path.AltDirectorySeparatorChar, "");

                string strFolder = Path.GetDirectoryName(dmi.Target);
                string strLocalFileName = Path.GetFileName(dmi.Target);

                string localpathfile = dmi.Target;

                try
                {
                    if (!Connected)
                    {
                        Connect();
                    }

                    if (dmi.CreateDirectory && !Directory.Exists(strFolder))
                    {
                        Directory.CreateDirectory(strFolder);
                    }

                    long size = GetFileSize(strRemoteFileName);
                    if (dmi.Replied) Began?.Invoke(dmi, size);

                    SetTransferType(TransferType.Binary);
                    if (strLocalFileName.Equals(""))
                    {
                        strLocalFileName = strRemoteFileName;
                    }
                    Socket socketData = CreateDataSocket();
                    SendCommand("RETR " + strRemoteFileName);
                    if (!(_code == 150 || _code == 125
                    || _code == 226 || _code == 250))
                    {
                        throw new IOException(_reply.Substring(4));
                    }

                    FileStream output = new FileStream(strFolder + "\\" + strLocalFileName, FileMode.Create);

                    int bytesTotalRead = 0;
                    dmi.Buffer = new byte[size];

                    while (true)
                    {
                        int iBytes = socketData.Receive(_buffer, _buffer.Length, 0);
                        output.Write(_buffer, 0, iBytes);
                        if (dmi.Replied) Stepped?.Invoke(dmi, iBytes);
                        if (iBytes <= 0)
                        {
                            break;
                        }
                        Buffer.BlockCopy(_buffer, 0, dmi.Buffer, bytesTotalRead, iBytes);
                        bytesTotalRead += iBytes;
                    }
                    output.Close();
                    if (socketData.Connected)
                    {
                        socketData.Close();
                    }
                    if (!(_code == 226 || _code == 250))
                    {
                        ReadReply();
                        if (!(_code == 226 || _code == 250))
                        {
                            throw new IOException(_reply);
                        }
                    }

                    dmi.Success = true;
                }
                catch (IOException ioex)
                {
                    dmi.Message = ioex.Message;
                    Notified?.Invoke(CVResult.IOExceptionHappened, dmi);
                }
                catch (Exception ex)
                {
                    dmi.Message = ex.Message;
                    Notified?.Invoke(CVResult.DownloadExceptionHappened, dmi);
                }
                finally
                {
                    if (dmi.Replied) Ended?.Invoke(dmi);
                }
            }
        }

        #region 工具
        /// <summary>
        /// 获取目录内容
        /// </summary>
        /// <param name="ip">ftp全路径</param>
        /// <returns></returns>
        private List<string> ListDirectory(string ip, string filter)
        {
            List<string> lines = new List<string>();
            try
            {
                string dir = ip + Path.AltDirectorySeparatorChar.ToString() + filter;
                dir = dir.EndsWith(Path.AltDirectorySeparatorChar.ToString()) ? dir : dir + Path.AltDirectorySeparatorChar;

                dir = dir.Replace(CurrentRemotePath + Path.AltDirectorySeparatorChar, "");
                string[] files = List(dir);
                lines.AddRange(files);
                return lines;
            }
            catch (WebException wex)
            {
                throw wex;
            }
        }

        /// <summary>
        /// 判断ftp服务器类型，
        /// 类型为Unknown时才执行
        /// </summary>
        /// <param name="lines">文件信息列表</param>
        private FtpServerType GetServerType(FtpServerType fst, string[] lines)
        {
            if (fst == FtpServerType.Unknown)
            {
                foreach (string l in lines)
                {
                    if (l.Length > 10
                        && System.Text.RegularExpressions.Regex.IsMatch(l.Substring(0, 10), "(-|d)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)"))
                    {
                        return fst = FtpServerType.Unix;
                    }
                    else if (l.Length > 8
                            && System.Text.RegularExpressions.Regex.IsMatch(l.Substring(0, 8), "[0-9][0-9]-[0-9][0-9]-[0-9][0-9]"))
                    {
                        return fst = FtpServerType.Windows;
                    }
                }
            }
            return fst;
        }

        #endregion
    }
}
