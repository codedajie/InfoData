using ChivaVR.Net.Packet;

namespace ChivaVR.Net.Core
{
    public class MessageItem : UserItem
    {
        /// <summary>
        /// 消息文本
        /// </summary>
        public string Message { get; internal set; }
        /// <summary>
        /// 消息ID
        /// </summary>
        public string MessageId { get; internal set; }
        /// <summary>
        /// 接收者ID
        /// </summary>
        public string ReceiveId { get; internal set; }
        /// <summary>
        /// 消息时间
        /// </summary>
        public string Time { get; internal set; }

        public static MessageItem From(ChatTextPt ctp)
        {
            return new MessageItem
            {
                Id = ctp.userid,
                Message = ctp.msg,
                MessageId = ctp.objectid,
                Name = ctp.username,
                ReceiveId = ctp.sendId,
                Time = ctp.sendTime
            };
        }
    }
}
