using ChivaVR.Net.Core;
using ChivaVR.Net.DC;
using ChivaVR.Net.Toolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ChivaVR.Net.Task
{
    public enum CVRDownloadResult
    {
        /// <summary>
        /// 下载一个
        /// FinishedItem
        /// </summary>
        Finished,
        /// <summary>
        /// 完成所有下载
        /// int 下载数量
        /// </summary>
        Completed,
        /// <summary>
        /// 下载错误
        /// string 全路径文件名
        /// </summary>
        DownloadHappened,
        /// <summary>
        /// 拆包错误
        /// string
        /// </summary>
        UnpackageHappened,
        /// <summary>
        /// 远程目录错误
        /// string
        /// </summary>
        SourceHappened
    }

    public delegate void CVRDownloadEventHandler(CVRDownloadResult r, object o);

    public class FinishedItem
    {
        public string Name { get; internal set; }
        public long Length { get; internal set; }
    }

    public class CVRDownloadTask : IChivaVRNet, IDisposable
    {
        public event CVRDownloadEventHandler Notified = null;
        public string Target { internal get; set; }

        private List<string> _excludedItems = new List<string>();

        Datacenter _dc = null;
        int _requestedCount = 0;
        int _downedCount = 0;
        string _temp = ".temp";

        public string Version { get { return "1.3.0203.1106"; } }

        public CVRDownloadTask(Datacenter dc)
        {
            Target = dc.LocalDirectory;

            _dc = dc;
            _dc.Ended += _dc_Ended;
        }

        /// <summary>
        /// 启动下载
        /// </summary>
        /// <param name="dir">远程起始目录</param>
        public void Start(string dir)
        {
            _downedCount = _requestedCount = 0;

            Thread thread = new Thread(DownloadDirectory);
            thread.IsBackground = true;
            thread.Start(dir);
        }

        /// <summary>
        /// 增加排除项目
        /// </summary>
        /// <param name="i"></param>
        public void AddExcludedItem(string i)
        {
            _excludedItems.Add(i);
        }

        /// <summary>
        /// 清空排除项目
        /// </summary>
        public void ClearExcludedItem()
        {
            _excludedItems.Clear();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            if (_dc != null)
            {
                _dc.Ended -= _dc_Ended;
            }
        }

        public void Dispose()
        {
            Close();
        }

        private void _dc_Ended(IDataOpItem item)
        {
            if (item is DownloadDataItem)
            {
                DownloadDataItem ddi = item as DownloadDataItem;
                if (ddi.Success)
                {
                    if (_excludedItems.Contains(Kits.GetFilename(ddi.Target)))
                    {
                        return;
                    }
                    _downedCount++;
                    //unpackage
                    string target = ddi.Target.Replace(@"\" + _temp, "");

                    string dir = (new FileInfo(target)).DirectoryName;
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    try
                    {
                        int l = ResourcePackage.Unpackage(ddi.Target, target);
                        Notified?.Invoke(CVRDownloadResult.Finished, new FinishedItem { Name = target, Length = l });
                    }
                    catch
                    {
                        Notified?.Invoke(CVRDownloadResult.UnpackageHappened, target);
                    }

                    if (_requestedCount == _downedCount)
                    {
                        string targetemp = Path.Combine(Target, _temp);
                        if (Directory.Exists(targetemp))
                        {
                            Kits.DeleteDirectory(targetemp);
                        }

                        Notified?.Invoke(CVRDownloadResult.Completed, _downedCount);
                    }
                }
                else
                {
                    Notified?.Invoke(CVRDownloadResult.DownloadHappened, ddi.Source);
                }
            }
        }

        private void DownloadDirectory(object o)
        {
            string pathname = o as string;

            //获取当前目录所有文件
            GetFilesItem gfi = new GetFilesItem
            {
                Target = _dc.GetFtpPathfile(pathname)
            };
            _dc.Execute(gfi);
            if (gfi.Success == false)
            {
                Notified?.Invoke(CVRDownloadResult.SourceHappened, gfi.Target);
                return;
            }

            foreach (FileInfoItem fii in gfi.Files)
            {
                if (_excludedItems.Contains(fii.Name))
                {
                    continue;
                }

                string remote = _dc.GetFtpPathfile(Path.Combine(pathname, fii.Name));
                DownloadDataItem ddi = new DownloadDataItem
                {
                    Source = remote,
                    Target = _dc.GetDrivePathfile(Path.Combine(_temp, Path.Combine(pathname, fii.Name)))
                };

                string dir = (new FileInfo(ddi.Target)).DirectoryName;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                _requestedCount++;
                _dc.Add(ddi);
            }

            //获取当前目录所有子目录
            GetDirectoriesItem gdi = new GetDirectoriesItem
            {
                Target = _dc.GetFtpPathfile(pathname)
            };
            _dc.Execute(gdi);

            foreach (FileInfoItem fii in gdi.Directories)
            {
                DownloadDirectory(Path.Combine(pathname, fii.Name));
            }
        }
    }
}
