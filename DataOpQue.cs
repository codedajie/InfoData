using System.Collections.Generic;
using System.Timers;
/// <summary>
/// 张志杰 20170124
/// ftp任务管理
/// 操作放于队列中排队执行
/// 
/// </summary>
namespace ChivaVR.Net.DC
{
    public class DataOpQue
    {
        private Datacenter _datacenter = null;
        private Queue<IDataOpItem> _datus = new Queue<IDataOpItem>();

        private Timer _timer = null;

        public DataOpQue(Datacenter dc)
        {
            _datacenter = dc;
        }

        public void Start()
        {
            if (_timer == null)
            {
                _timer = new Timer(300);
                _timer.Elapsed += _timer_Elapsed;
                _timer.Start();
            }
        }
        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Close();
                _timer.Dispose();
                _timer = null;
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_datus.Count != 0)
            {
                IDataOpItem d = _datus.Dequeue();
                d.Action(_datacenter);
            }
        }

        public void Add(IDataOpItem d)
        {
            _datus.Enqueue(d);
        }

        public void Clear()
        {
            _datus.Clear();
        }
    }
}
