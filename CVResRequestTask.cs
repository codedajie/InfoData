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
    public enum CVResRequestResult : ushort
    {
        /// <summary>
        /// 未知
        /// 
        /// </summary>
        Unknown,
        /// <summary>
        /// 请求成功，保存在本地
        /// string，本地目录文件名
        /// </summary>
        RequestFileSucceed,
        /// <summary>
        /// 请求成功，返回内存
        /// byte[]
        /// </summary>
        RequestDataSucceed,
        /// <summary>
        /// 未找到远程源文件
        /// string，问题消息
        /// </summary>
        SourceNotFound,
        /// <summary>
        /// 下载问题
        /// string，问题消息
        /// </summary>
        DownloadFailed,
        /// <summary>
        /// 文件校验问题
        /// string，问题消息
        /// </summary>
        CrcFailded,
        /// <summary>
        /// 拆包问题
        /// string，被拆包文件名
        /// </summary>
        UnpackageFailed,
        /// <summary>
        /// 写本地文件失败
        /// string，本地目录文件名
        /// </summary>
        WriteFailed,
        /// <summary>
        /// 未找到xml文件
        /// string，xml文件
        /// </summary>
        XmlNotExisted
    }
    #endregion

    public delegate void CVResRequestTaskEventHandler(CVResRequestResult r, object o);

    public class CVResRequestTask : IChivaVRNet, IDisposable
    {
        #region 属性
        /// <summary>
        /// 组件版本
        /// </summary>
        public string Version { get { return "1.3.0203.1106"; } }

        /// <summary>
        /// 是否存储为本地文件
        /// 默认为true
        /// </summary>
        public bool LocalFile { get; set; }

        #endregion

        #region 事件
        public event CVResRequestTaskEventHandler Notified = null;
        #endregion

        #region 构造
        public CVResRequestTask(Infocenter ic, Datacenter dc)
        {
            LocalFile = true;

            _ic = ic;
            _ic.Notified += _ic_Notified;

            _dc = dc;
            _dc.Ended += _dc_Ended;
        }

        public void Dispose()
        {
            _ic.Notified -= _ic_Notified;
            _dc.Ended -= _dc_Ended;
        }
        #endregion

        #region 方法
        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="id"></param>
        public void Download(string id)
        {
            ResourceOp rop = OpFactory<ResourceOp>.Create(_ic);
            rop.Get(id);
        }

        /// <summary>
        /// 下载，todo测试
        /// </summary>
        /// <param name="id"></param>
        /// <param name="folder"></param>
        /// <param name="version"></param>
        /// <param name="versionname"></param>
        public void Download(string id, int folder, int version, string versionname)
        {
            ResourceOp rop = OpFactory<ResourceOp>.Create(_ic);
            rop.Get(id);
        }

        /// <summary>
        /// 下载资源集文件内容
        /// </summary>
        /// <param name="pathfile"></param>
        public void DownloadResXml(string pathfile)
        {
            if (File.Exists(pathfile))
            {
                ResXmlReader rxr = new ResXmlReader(pathfile);
                foreach (XmlNode xn in rxr.Root.ChildNodes)
                {
                    ResXmlItem rxi = new ResXmlItem(xn);
                    Download(rxi.Id);
                }
            }
            else
            {
                Notified?.Invoke(CVResRequestResult.XmlNotExisted, pathfile);
            }
        }

        #endregion

        #region 响应

        private void _dc_Ended(IDataOpItem item)
        {
            if (item is DownloadDataItem)
            {
                DownloadDataItem ddi = item as DownloadDataItem;
                if (ddi.Success)
                {
                    string crc = FileMD5.Create(ddi.Target);
                    if (crc == ddi.Crc)
                    {
                        string localPathfile = _dc.GetLocalPathfile(ddi.Source);

                        try
                        {
                            if (LocalFile)
                            {
                                ResourcePackage.Unpackage(ddi.Target, localPathfile);
                                Notified?.Invoke(CVResRequestResult.RequestFileSucceed, localPathfile);
                            }
                            else
                            {
                                byte[] buff = ResourcePackage.Unpackage(File.ReadAllBytes(ddi.Target));
                                Notified?.Invoke(CVResRequestResult.RequestDataSucceed, buff);
                            }
                        }
                        catch (NullReferenceException)
                        {
                            Notified?.Invoke(CVResRequestResult.UnpackageFailed, localPathfile);
                        }
                        catch (IOException)
                        {
                            Notified?.Invoke(CVResRequestResult.WriteFailed, localPathfile);
                        }
                    }
                    else
                    {
                        Notified?.Invoke(CVResRequestResult.CrcFailded, ddi.Message);
                    }
                }
                else
                {
                    Notified?.Invoke(CVResRequestResult.DownloadFailed, ddi.Message);
                }
            }
        }

        private void _ic_Notified(CVResult result, object obj)
        {
            if (result == CVResult.ResourceOperationResponded)
            {
                if (obj is ResourcePt)
                {
                    ResourcePt r = obj as ResourcePt;

                    if (r.service == 4)
                    {
                        if (r.success == 0)
                        {
                            string tempPathfile = Kits.GetTempPathfile(r.name);

                            string pathfile = _dc.GetFtpPathfileWithoutRemoteDirectory(r.pathfile);
                            DownloadDataItem ddi = new DownloadDataItem()
                            {
                                Source = pathfile,
                                Target = tempPathfile,
                                Id = r.objectid,
                                Crc = r.crc
                            };
                            _dc.Add(ddi);
                        }
                        else
                        {
                            Notified?.Invoke(CVResRequestResult.SourceNotFound, r.msg);
                        }
                    }
                }
            }
        }
        #endregion

        #region 字段
        private Datacenter _dc = null;
        private Infocenter _ic = null;
        #endregion
    }
}
