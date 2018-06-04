using ChivaVR.Net.Packet;
using System.Threading;
/// <summary>
/// 张志杰 20170119
/// 网络访问超时
/// </summary>
namespace ChivaVR.Net.Core
{
    public delegate void TimeoutEventHandler(object o);

    public class Deadline
    {
        /// <summary>
        /// 超时时长
        /// </summary>
        public static readonly int Timeout = 1000 * 100;

        public event TimeoutEventHandler OnTimeout = null;

        /// <summary>
        /// 超时的protobuf命令
        /// </summary>
        public ProtoBuffType Cmd { get { return _protoBuffCmd; } }

        private ProtoBuffType _protoBuffCmd = ProtoBuffType.Unknown;
        private Timer _timer;

        public Deadline(ProtoBuffType pbc, int timeout = 0)
        {
            _protoBuffCmd = pbc;
            _timer = new Timer(new TimerCallback(TimerProc), this, timeout == 0 ? Timeout : timeout, 0);
        }

        private void TimerProc(object o)
        {
            OnTimeout?.Invoke(this);
            Disopse();
        }

        public void Disopse()
        {
            _timer.Dispose();
            _timer = null;
        }
    }
}
