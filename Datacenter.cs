using ChivaVR.Net.Core;
using ChivaVR.Net.Toolkit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
/// <summary>
/// 张志杰 20170123
/// 数据中心
/// </summary>
namespace ChivaVR.Net.DC
{
    /// <summary>
    /// 数据处理进度条事件
    /// </summary>
    public delegate void BeginEventHandler(IDataOpItem item, long length);
    public delegate void StepEventHandler(IDataOpItem item, long segment);
    public delegate void EndEventHandler(IDataOpItem item);

    public sealed partial class Datacenter
    {
        #region 属性
        public string Version { get { return "1.2.0624.1434"; } }
        public FtpServerType ServerType { get; internal set; }
        public string LocalDirectory { get; set; }
        /// <summary>
        /// 当前ftp全路径
        /// </summary>
        public string CurrentRemotePath
        {
            get
            {
                return @"ftp://" + Ip + (RemoteDirectory == "" ? "" : (Path.AltDirectorySeparatorChar + RemoteDirectory));
            }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 数据中心实例
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="remoteDirectory"></param>
        /// <param name="localDirectory"></param>
        /// <returns></returns>
        public static Datacenter Create(string ip, int port, string user, string pwd, string remoteDirectory, string localDirectory)
        {
            _instance = new Datacenter(ip, port, user, pwd, remoteDirectory, localDirectory);
            return _instance;
        }

        public static Datacenter Create(string ip, string user, string pwd)
        {
            _instance = new Datacenter(ip, 21, user, pwd, "", "");
            return _instance;
        }

        /// <summary>
        /// 启动队列、检查目录
        /// </summary>
        public void Start()
        {
            try
            {
                //链接&改变目录
                Connect();
                //检查目录
                if (!Directory.Exists(LocalDirectory))
                {
                    Notified?.Invoke(CVResult.LocalDirectoryException, LocalDirectory);
                    throw new Exception("没有找到本地目录 " + LocalDirectory);
                }

                _dataOpQue = new DataOpQue(this);
                //启动操作队列
                Begin();

                Notified?.Invoke(CVResult.DatacenterReady, this);
            }
            catch (IOException ioe)
            {
                Notified?.Invoke(CVResult.RemoteException, ioe.Message);
            }
            catch (Exception ex)
            {
                Notified?.Invoke(CVResult.DatacenterNotConnected, ex.Message);
            }
        }

        /// <summary>
        /// 增加任务队列内容
        /// </summary>
        /// <param name="di"></param>
        /// <param name="replied">默认回执</param>
        public void Add(IDataOpItem di, bool replied = true)
        {
            di.Replied = replied;
            _dataOpQue?.Add(di);
        }

        /// <summary>
        /// 执行操作
        /// </summary>
        /// <param name="di"></param>
        /// <param name="replied">默认不回执</param>
        public void Execute(IDataOpItem di, bool replied = false)
        {
            di.Replied = replied;
            di.Action(this);
        }

        /// <summary>
        /// 开始执行任务队列
        /// </summary>
        public void Begin()
        {
            _dataOpQue.Start();
        }

        /// <summary>
        /// 结束执行任务队列
        /// </summary>
        public void End()
        {
            _dataOpQue.Clear();
            _dataOpQue.Stop();

            if (_thread != null && _thread.IsAlive)
            {
                _thread.Abort();
                _thread = null;
            }
        }

        /// <summary>
        /// 关闭资源中心
        /// </summary>
        public void Shutdown()
        {
            End();
            Disconnect();
            Notified?.Invoke(CVResult.DatacenterNotConnected, "Shutdowned");
        }
        #endregion

        #region 事件
        public event BeginEventHandler Began = null;
        public event StepEventHandler Stepped = null;
        public event EndEventHandler Ended = null;
        public event CVRNotifyEventHandler Notified = null;
        #endregion

        #region 路径名工具
        /// <summary>
        /// 合成ftp远程全路径名
        /// </summary>
        /// <param name="filename">文件名，可带相对路径</param>
        /// <returns></returns>
        public string GetFtpPathfile(string filename)
        {
            string remotePathfile = @"ftp://" + Ip + Kits.GetDashPathname(RemoteDirectory, filename);
            return remotePathfile;
        }

        /// <summary>
        /// 合成ftp远程全路径名，但不包含远程目录
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string GetFtpPathfileWithoutRemoteDirectory(string filename)
        {
            string remotePathfile = @"ftp://" + Ip + Kits.GetSafedPathname(filename);
            return remotePathfile;
        }

        /// <summary>
        /// 根据本地全路径文件名，获取远程全路径文件名
        /// 远程目标=远程目录+本地相对路径文件名
        /// </summary>
        /// <param name="localPathfile">待上传本地文件全路径名</param>
        /// <returns></returns>
        public string GetRemotePathfile(string localPathfile)
        {
            string localFilename = localPathfile.Substring(LocalDirectory.Length + 1);
            string remotePathfile = GetFtpPathfile(localFilename);
            return remotePathfile.Replace(@"\", "/");
        }

        /// <summary>
        /// 获取本地文件名
        /// </summary>
        /// <param name="pathname">相对文件名</param>
        /// <returns></returns>
        public string GetDrivePathfile(string pathname)
        {
            return LocalDirectory + @"\" + pathname;
        }

        /// <summary>
        /// 根据远程全路径名，获取本地全路径文件名
        /// 本地目标=本地目录+远程相对路径文件名
        /// </summary>
        /// <param name="remotePathfile">远程全路径文件名</param>
        /// <returns></returns>
        public string GetLocalPathfile(string remotePathfile)
        {
            string ip = GetFtpPathfile("");
            string remoteFilename = remotePathfile.Substring(ip.Length + 1);
            string localPathfile = LocalDirectory + @"\" + remoteFilename;
            return localPathfile;
        }

        /// <summary>
        /// 获取本地全路径文件名，改相对路径文件名
        /// </summary>
        /// <param name="remotePathfile">远程全路径文件名</param>
        /// <param name="localFilename">本地相对文件名</param>
        /// <returns></returns>
        public string GetLocalPathfile(string remotePathfile, string localFilename)
        {
            string localPathfile = LocalDirectory + @"\" + localFilename;
            return localPathfile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="remotePathfile"></param>
        /// <returns></returns>
        public string GetPathfileByRemotePathfile(string remotePathfile)
        {
            //去掉ftp头
            int i = remotePathfile.IndexOf("/", 13);
            string pathfile = remotePathfile.Substring(i, remotePathfile.Length - i);

            return pathfile;
        }
        #endregion

        #region 操作

        /// <summary>
        /// 获取是否存在指定远程文件
        /// </summary>
        /// <param name="edi"></param>
        /// <returns></returns>
        public bool ExistFile(ExistDataItem edi)
        {
            edi.Exist = false;
            try
            {
                string filename = edi.Target.Replace(CurrentRemotePath + Path.AltDirectorySeparatorChar, "");
                string[] s = List(filename);
                if (s.Length == 0)
                {
                    throw new IOException("not found " + filename);
                }

                edi.Success = true;
                edi.Exist = true;
                if (edi.Replied) Ended?.Invoke(edi);
            }
            catch (IOException ex)
            {
                edi.Message = ex.Message;
                edi.Success = false;
                edi.Exist = false;
            }

            return edi.Exist;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="target">远程全路径文件名</param>
        public void DeleteFile(DeleteDataItem ddi)
        {
            try
            {
                string filename = ddi.Target.Replace(CurrentRemotePath + Path.AltDirectorySeparatorChar, "");
                Delete(filename);
                ddi.Success = true;

                if (ddi.Replied) Ended?.Invoke(ddi);
            }
            catch (IOException ex)
            {
                ddi.Message = ex.Message;
                ddi.Success = false;
                Notified?.Invoke(CVResult.DeleteFileExceptionHappened, ddi);
            }
        }

        /// <summary>
        /// 获取远程文件大小
        /// </summary>
        /// <param name="dsi"></param>
        /// <returns>-1:则获取失败</returns>
        public long GetFileSize(DataSizeItem dsi)
        {
            dsi.Size = -1;
            try
            {
                string filename = dsi.Target.Replace(CurrentRemotePath + Path.AltDirectorySeparatorChar, "");
                dsi.Size = GetFileSize(filename);
                dsi.Success = true;

                if (dsi.Replied) Ended?.Invoke(dsi);
            }
            catch (IOException ex)
            {
                dsi.Message = ex.Message;
                dsi.Success = false;
                Notified?.Invoke(CVResult.GetFileSizeExceptionHappened, dsi);
            }

            return dsi.Size;
        }

        /// <summary>
        /// 重命名
        /// </summary>
        /// <param name="ri"></param>
        public void Rename(RenameItem ri)
        {
            string remotePathfile = ri.Source.Replace(CurrentRemotePath + Path.AltDirectorySeparatorChar, "");
            string filename = ri.Target;
            try
            {
                Rename(remotePathfile, filename);

                ri.Success = true;
                if (ri.Replied) Ended?.Invoke(ri);
            }
            catch (IOException ex)
            {
                ri.Message = ex.Message;
                ri.Success = false;
                Notified?.Invoke(CVResult.RenameExceptionHappened, ri);
            }
        }

        /// <summary>
        /// 创建远程目录
        /// </summary>
        /// <param name="mdi"></param>
        public void MakeDirectory(MakeDirectoryItem mdi)
        {
            string ipfolder = mdi.Target.Replace(CurrentRemotePath + Path.AltDirectorySeparatorChar, "");
            try
            {
                MkDir(ipfolder);
                mdi.Success = true;

                if (mdi.Replied) Ended?.Invoke(mdi);
            }
            catch (IOException ex)
            {
                mdi.Message = ex.Message;
                mdi.Success = false;
                Notified?.Invoke(CVResult.MakeDirectoryExceptionHappened, mdi);
            }
        }

        /// <summary>
        /// 删除远程目录
        /// </summary>
        /// <param name="rdi"></param>
        public void RemoveDirectory(RemoveDirectoryItem rdi)
        {
            string ipfolder = rdi.Target.Replace(CurrentRemotePath + Path.AltDirectorySeparatorChar, "");
            try
            {
                RmDir(ipfolder);
                rdi.Success = true;

                if (rdi.Replied) Ended?.Invoke(rdi);
            }
            catch (IOException ex)
            {
                rdi.Message = ex.Message;
                rdi.Success = false;
                Notified?.Invoke(CVResult.MakeDirectoryExceptionHappened, rdi);
            }
        }

        /// <summary>
        /// 获取远程文件时间 todo
        /// </summary>
        /// <param name="tsi"></param>
        /// <returns></returns>
        public DateTime GetTimeStamp(TimeStampItem tsi)
        {
            tsi.TimeStamp = DateTime.MinValue;
            try
            {
                string filename = tsi.Target.Replace(CurrentRemotePath + Path.AltDirectorySeparatorChar, "");
                string[] s = List(filename);
                if (s.Length == 0)
                {
                    throw new IOException("not found " + filename);
                }
                this.ServerType = GetServerType(this.ServerType, s);
                if (ServerType == FtpServerType.Windows)
                {
                    string[] strs = s[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string ss = (strs[0] + " " + strs[1]).Replace("PM", "").Replace("AM", "");
                    tsi.TimeStamp = DateTime.ParseExact(ss, "MM-dd-yy HH:mm", CultureInfo.InvariantCulture);
                }
                else
                {
                    string[] strs = s[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string ss = strs[5] + " " + strs[6] + " " + strs[7];
                    tsi.TimeStamp = DateTime.Parse(ss);//todo
                }

                tsi.Success = true;
                if (tsi.Replied) Ended?.Invoke(tsi);
            }
            catch (IOException ex)
            {
                tsi.Message = ex.Message;
                tsi.Success = false;
            }

            return tsi.TimeStamp;
        }

        /// <summary>
        /// 获取远程目录下文件
        /// </summary>
        /// <param name="gfi"></param>
        public void GetFiles(GetFilesItem gfi)
        {
            gfi.Files = new List<FileInfoItem>();
            gfi.Success = false;

            try
            {
                List<string> lines = ListDirectory(gfi.Target, gfi.Filter);
                if (lines.Count == 0)
                {
                    gfi.Success = true;
                    if (gfi.Replied) Ended?.Invoke(gfi);
                    return;
                }

                ServerType = GetServerType(this.ServerType, lines.ToArray());
                if (ServerType == FtpServerType.Unknown)
                {
                    Notified?.Invoke(CVResult.DatacenterUnknownServer, "未识别的FTP服务器类型");
                    return;
                }

                if (ServerType == FtpServerType.Windows)
                {
                    foreach (string line in lines)
                    {
                        string[] strs = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (strs[2] == "<DIR>")
                        {
                            //是子目录
                        }
                        else
                        {
                            FileInfoItem rii = new FileInfoItem
                            {
                                DateTime = strs[0] + " " + strs[1],
                                Size = long.Parse(strs[2]),
                                Name = line.Substring(39)
                            };
                            gfi.Files.Add(rii);
                        }
                    }
                }
                else
                {
                    foreach (string line in lines)
                    {
                        string[] strs = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (strs[0].StartsWith("d") || strs[0].StartsWith("t"))
                        {
                            //是子目录,"t"
                        }
                        else
                        {
                            FileInfoItem rii = new FileInfoItem
                            {
                                DateTime = strs[5] + " " + strs[6] + " " + strs[7],
                                Size = long.Parse(strs[4]),
                                Name = line.Substring(55)
                            };
                            gfi.Files.Add(rii);
                        }
                    }
                }

                gfi.Success = true;
                if (gfi.Replied) Ended?.Invoke(gfi);
            }
            catch (IOException ex)
            {
                gfi.Message = ex.Message;
                Notified?.Invoke(CVResult.GetFilesExceptionHappened, gfi);
            }
        }

        /// <summary>
        /// 获取远程目录下子目录
        /// </summary>
        /// <param name="gdi"></param>
        public void GetDirectories(GetDirectoriesItem gdi)
        {
            gdi.Success = false;
            gdi.Directories = new List<FileInfoItem>();

            try
            {
                List<string> lines = ListDirectory(gdi.Target, "");
                if (lines.Count == 0)
                {
                    gdi.Success = true;
                    if (gdi.Replied) Ended?.Invoke(gdi);
                    return;
                }

                ServerType = GetServerType(this.ServerType, lines.ToArray());
                if (ServerType == FtpServerType.Unknown)
                {
                    Notified?.Invoke(CVResult.DatacenterUnknownServer, "未识别的FTP服务器类型");
                    return;
                }

                if (ServerType == FtpServerType.Windows)
                {
                    foreach (string line in lines)
                    {
                        string[] strs = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (strs[2] == "<DIR>")
                        {
                            string name = string.Empty;
                            for (int i = 3; i < strs.Length; ++i)
                            {
                                name += strs[i] + " ";
                            }
                            name = name.TrimEnd();
                            FileInfoItem rii = new FileInfoItem
                            {
                                DateTime = strs[0] + " " + strs[1],
                                Name = name
                            };
                            gdi.Directories.Add(rii);
                        }
                    }
                }
                else
                {
                    List<string> files = ListDirectory(gdi.Target, "");

                    if (files.Count == 0)
                    {
                        gdi.Success = true;
                        if (gdi.Replied) Ended?.Invoke(gdi);
                        return;
                    }
                    foreach (string file in files)
                    {
                        string[] strs = file.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (strs[0].StartsWith("d") && !strs[8].StartsWith("."))
                        {
                            FileInfoItem rii = new FileInfoItem
                            {
                                DateTime = strs[5] + " " + strs[6] + " " + strs[7],
                                Size = long.Parse(strs[4]),
                                Name = strs[8]
                            };
                            gdi.Directories.Add(rii);
                        }
                    }
                }

                gdi.Success = true;
                if (gdi.Replied) Ended?.Invoke(gdi);
            }
            catch (IOException ex)
            {
                gdi.Message = ex.Message;
                Notified?.Invoke(CVResult.GetDirectoriesExceptionHappened, gdi);
            }
        }
        #endregion
    }
}
