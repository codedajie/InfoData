using ChivaVR.Net.Core;
using ChivaVR.Net.DC;
using ChivaVR.Net.IC;
using ChivaVR.Net.Packet;
using ChivaVR.Net.Toolkit;
using System;
using System.IO;
using System.Xml;

namespace ChivaVR.Net.Task
{
    #region 消息
    public enum CVResDownloadResult : ushort
    {
        /// <summary>
        /// 未知
        /// 
        /// </summary>
        Unknown,
        /// <summary>
        /// 请求资源信息问题
        /// string
        /// </summary>
        RequestResourceFailed,
        /// <summary>
        /// 目标目录问题
        /// string
        /// </summary>
        TargetDirectoryFailed,
        /// <summary>
        /// 存在目标，不下载
        /// string:target
        /// </summary>
        ExistedTarget
    }
    #endregion

    public delegate void CVResDownloadTaskEventHandler(CVResDownloadResult r, object o);
    public delegate void CVResDownloadTaskProgressEventHandler(IDataOpItem ddi, long length);
    public delegate void CVResDownloadTaskEndEventHandler(IDataOpItem ddi);
    public delegate void CVResDownloadTaskCompleteEventHandler();

    public class CVResDownloadTask : IChivaVRNet, IDisposable
    {

        #region 事件
        public event CVResDownloadTaskEventHandler Notified = null;
        public event CVResDownloadTaskProgressEventHandler Began = null;
        public event CVResDownloadTaskProgressEventHandler Stepped = null;
        public event CVResDownloadTaskEndEventHandler Ended = null;
        public event CVResDownloadTaskCompleteEventHandler Completed = null;
        #endregion

        #region 属性
        /// <summary>
        /// 组件版本
        /// </summary>
        public string Version { get { return "1.3.0203.1106"; } }

        public bool UseMemory { get; set; }

        #endregion

        #region 构析
        public CVResDownloadTask(Infocenter ic, Datacenter dc)
        {
            _ic = ic;
            _ic.Notified += _ic_Notified;

            _dc = dc;
            _dc.Began += _dc_Began;
            _dc.Stepped += _dc_Stepped;
            _dc.Ended += _dc_Ended;

            _to = OpFactory<TableOp>.Create(_ic);
            _to.Format = ResultFormat.Xml;

            UseMemory = false;
        }

        public void Dispose()
        {
            _ic.Notified -= _ic_Notified;
            _dc.Began -= _dc_Began;
            _dc.Stepped -= _dc_Stepped;
            _dc.Ended -= _dc_Ended;
        }
        #endregion

        #region 方法

        /// <summary>
        /// 条件下载
        /// </summary>
        /// <param name="where">下载条件</param>
        public void Download(string where, int pageSize = 20, int pageNo = 0)
        {
            _pageNo = pageNo;
            _pageSize = pageSize;
            _pageCount = 0;
            _curIndex = 0;
            _curNum = 0;

            if (Directory.Exists(_dc.LocalDirectory))
            {
                _sqlString = "SELECT objectid,pathfile,name,folder,version,versionname,lastwrite,remark,crc FROM " + _resTable + (where.Length == 0 ? "" : " WHERE " + where);
                _to.Token = _token = Guid.NewGuid().ToString("N");
                _to.Select(_sqlString, _pageSize, _pageNo);
            }
            else
            {
                Notified?.Invoke(CVResDownloadResult.TargetDirectoryFailed, _dc.LocalDirectory);
            }
        }

        /// <summary>
        /// 下载所有
        /// </summary>
        public void Download(int pageSize = 20, int pageNo = 0)
        {
            Download("", pageSize, pageNo);
        }

        /// <summary>
        /// 终止正在进行的下载
        /// </summary>
        public void Abort()
        {
            _dc?.End();
        }
        #endregion

        #region 事件

        private void _dc_Began(IDataOpItem item, long length)
        {
            string dir = (new FileInfo(item.Target)).DirectoryName;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            Began?.Invoke(item, length);
        }

        private void _dc_Stepped(IDataOpItem item, long segment)
        {
            Stepped?.Invoke(item, segment);
        }

        private void _dc_Ended(IDataOpItem item)
        {
            Ended?.Invoke(item);

            if (++_curIndex >= _curNum)
            {
                if (++_pageNo < _pageCount)
                {
                    _to.Select(_sqlString, _pageSize, _pageNo);
                }
                else
                {
                    Completed?.Invoke();
                }
            }
        }

        private void _ic_Notified(CVResult r, object o)
        {
            if (r == CVResult.TableQueryReceived)
            {
                if (o is QueryResultPt)
                {
                    QueryResultPt qrp = o as QueryResultPt;
                    if (qrp.token == _token)
                    {
                        if (qrp.success == 0)
                        {
                            TableXmlWrapper txw = new TableXmlWrapper(qrp);

                            _curNum = txw.Count;
                            _curIndex = 0;
                            _pageCount = txw.Count / _pageSize + (txw.Count % _pageSize == 0 ? 0 : 1);

                            foreach (XmlNode xn in txw.Nodes)
                            {
                                ResXmlNodeParser rxnp = new ResXmlNodeParser(xn);
                                string pathfile = rxnp.Pathfile.Remove(0, 1);
                                string file = pathfile.Replace(_dc.RemoteDirectory + @"/", "");

                                string source = @"ftp://" + _dc.Ip + @"/" + pathfile;
                                string target = Path.Combine(_dc.LocalDirectory, file).Replace("/", @"\");

                                DateTime tdt = File.Exists(target) ? File.GetLastWriteTime(target) : DateTime.MinValue;
                                if (!Kits.DateTimeEqual(rxnp.Lastwrite, tdt))
                                {
                                    if (UseMemory)
                                    {
                                        DownloadMemoryItem dmi = new DownloadMemoryItem
                                        {
                                            Id = rxnp.Id,
                                            Source = source,
                                            Target = target,
                                            Crc = rxnp.Crc,
                                            Tag = rxnp.Name
                                        };
                                        _dc.Add(dmi);
                                    }
                                    else
                                    {
                                        DownloadDataItem ddi = new DownloadDataItem
                                        {
                                            Id = rxnp.Id,
                                            Source = source,
                                            Target = target,
                                            Crc = rxnp.Crc,
                                            Tag = rxnp.Name
                                        };
                                        _dc.Add(ddi);
                                    }
                                }
                                else
                                {
                                    Notified?.Invoke(CVResDownloadResult.ExistedTarget, target);
                                    if (++_curIndex >= _curNum)
                                    {
                                        if (++_pageNo >= _pageCount)
                                        {
                                            Completed?.Invoke();
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Notified?.Invoke(CVResDownloadResult.RequestResourceFailed, qrp.msg);
                        }
                    }
                }
            }
        }
        #endregion

        #region 字段
        private Datacenter _dc = null;
        private Infocenter _ic = null;
        private TableOp _to = null;
        private string _token;

        private string _sqlString;
        private int _pageSize = 100;
        private int _pageNo = 0;
        private int _pageCount = 0;
        private int _curIndex = 0;
        private int _curNum = 0;

        private static string _resTable = "pub_resource";
        #endregion
    }
}
