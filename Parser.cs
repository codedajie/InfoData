using ChivaVR.Net.Core;
/// <summary>
/// 张志杰 20170117
/// Socket流解释器
/// </summary>
namespace ChivaVR.Net.IC
{
    internal class Parser
    {
        protected Infocenter _client = null;
        public event CVRNotifyEventHandler Notified = null;

        protected void Notify(CVResult result, object obj)
        {
            Notified?.Invoke(result, obj);
        }

        public Parser(Infocenter c)
        {
            _client = c;
        }

        public virtual void Do(byte[] src)
        {
        }
    }
}
