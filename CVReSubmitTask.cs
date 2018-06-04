using ChivaVR.Net.Core;
using ChivaVR.Net.DC;
using ChivaVR.Net.IC;
using ChivaVR.Net.Packet;
using ChivaVR.Net.Toolkit;
using System.IO;

namespace ChivaVR.Net.Task
{
    #region 消息
    public enum CVReSubmitResult : ushort
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown,
        /// <summary>
        /// 提交成功
        /// string，提交文件路径名
        /// </summary>
        SubmitSucceed,
        /// <summary>
        /// 提交源文件问题
        /// string，问题信息
        /// </summary>
        SourceNotFound,
        /// <summary>
        /// 上传失败
        /// string，问题信息
        /// </summary>
        UploadFailed
    }
    #endregion

    public delegate void CVReSubmitTaskEventHandler(CVReSubmitResult r, object obj);

    public class CVReSubmitTask : IChivaVRNet
    {
        public string Version { get { return "1.2.0624.1434"; } }

        public event CVReSubmitTaskEventHandler Notified = null;

        public CVReSubmitTask(Infocenter ic, Datacenter dc)
        {
            _ic = ic;
            _ic.Notified += _ic_Notified;

            _dc = dc;
            _dc.Ended += _dc_Ended;
        }

        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="localPathfile">本地全路径文件名</param>
        /// <param name="version"></param>
        /// <param name="group"></param>
        /// <param name="remark"></param>
        public void Upload(string localPathfile, string version = "", int group = 0, string remark = "")
        {
            try
            {
                string tempPathfile = Kits.GetTempPathfile(Kits.GetFilename(localPathfile));
                ResourcePackage.Package(localPathfile, tempPathfile);

                UploadDataItem ud = new UploadDataItem()
                {
                    Source = tempPathfile,
                    Target = _dc.GetRemotePathfile(localPathfile),
                    Version = version,
                    Group = group,
                    Remark = remark,
                    Tag = localPathfile
                };
                _dc.Add(ud);
            }
            catch(IOException ioe)
            {
                Notified?.Invoke(CVReSubmitResult.SourceNotFound, ioe.Message);
            }
        }

        private void _dc_Ended(IDataOpItem item)
        {
            if (item is UploadDataItem)
            {
                UploadDataItem udi = item as UploadDataItem;

                if (udi.Success)
                {
                    string source = udi.Tag as string;
                    string crc = FileMD5.Create(udi.Source);
                    string name =Kits.GetFilename(source);

                    string pathname = _dc.GetPathfileByRemotePathfile(udi.Target);

                    ResourceOp rop =OpFactory<ResourceOp>.Create(_ic);
                    rop.Add(pathname, name, udi.Group, 0,udi.Version,Kits.GetLastWriteTime(source), udi.Remark, "cvraddresource", crc);
                }
                else
                {
                    Notified?.Invoke(CVReSubmitResult.UploadFailed, udi.Message);
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

                    if (r.service == 1)
                    {
                        if (r.success == 0)
                        {
                            Notified?.Invoke(CVReSubmitResult.SubmitSucceed, r.pathfile);
                        }
                        else
                        {
                            Notified?.Invoke(CVReSubmitResult.UploadFailed, r.msg);
                        }
                    }
                }
            }
        }

        private Datacenter _dc = null;
        private Infocenter _ic = null;
    }
}
