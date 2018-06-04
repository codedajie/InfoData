using ChivaVR.Net.Packet;
using ChivaVR.Net.Toolkit;

namespace ChivaVR.Net.Core
{
    public class TableXmlWrapper : XmlStringWrapper
    {
        /// <summary>
        /// 表操作标识
        /// </summary>
        public string Token { get; internal set; }
        /// <summary>
        /// 表记录总数量
        /// </summary>
        public int Count { get; internal set; }
        /// <summary>
        /// 表记录当前数量
        /// </summary>
        public int Num { get { return this.Root.ChildNodes.Count; } }
        /// <summary>
        /// 查询是否成功
        /// </summary>
        public bool Success { get; internal set; }
        /// <summary>
        /// 查询结果信息
        /// </summary>
        public string Message { get; internal set; }

        public TableXmlWrapper(QueryResultPt qrp) : base(qrp.result)
        {
            Token = qrp.token;
            Count = qrp.count;
            Message = qrp.msg;
            Success = qrp.success == 0;
        }
    }
}
