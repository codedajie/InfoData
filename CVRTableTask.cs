using ChivaVR.Net.Core;
using ChivaVR.Net.IC;
using ChivaVR.Net.Packet;
using System;
using System.Timers;

namespace ChivaVR.Net.Task
{
    #region 消息
    public enum CVRTableResult : ushort
    {
        /// <summary>
        /// 未知
        /// object
        /// </summary>
        Unknown,

        /// <summary>
        /// 查询成功，返回XmlNode
        /// TableXmlWrapper
        /// </summary>
        SelectXmlReceived,
        /// <summary>
        /// 查询成功，返回DataSet
        /// DataSet
        /// </summary>
        SelectDataReceived,
        /// <summary>
        /// 查询成功，返回JSON
        /// CVRJsonParser
        /// </summary>
        SelectJsonReceived,
        /// <summary>
        /// 查询失败
        /// string
        /// </summary>
        SelectFailed,
        /// <summary>
        /// update、insert、delete操作完成
        /// UpdateItem
        /// </summary>
        UpdateResultReceived,
        /// <summary>
        /// 同步操作超时
        /// 操作的Token
        /// </summary>
        OperationTimeout
    }
    #endregion

    /// <summary>
    /// 更新操作返回
    /// </summary>
    public class UpdateItem
    {
        public bool Succeed { get; internal set; }
        public string Id { get; internal set; }
        public string Msg { get; internal set; }
    }

    public delegate void CVRTableTaskEventHandler(CVRTableResult r, object o);

    /// <summary>
    /// 数据库表操作
    /// </summary>
    public class CVRTableTask : IChivaVRNet, IDisposable
    {
        /// <summary>
        /// 操作标识
        /// </summary>
        public string Token { get { return _to.Token; } set { _to.Token = value; } }

        /// <summary>
        /// 查询结果格式
        /// </summary>
        public ResultFormat Format { get { return _to.Format; } set { _to.Format = value; } }

        /// <summary>
        /// 事件
        /// </summary>
        public event CVRTableTaskEventHandler Notified = null;

        /// <summary>
        /// 组件版本
        /// </summary>
        public string Version { get { return "2.0.0301.1052"; } }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="ic"></param>
        public CVRTableTask(Infocenter ic)
        {
            _timer.Elapsed += _timer_Elapsed;

            _ic = ic;
            _ic.Notified += _ic_Notified;
            _to = OpFactory<TableOp>.Create(_ic);
        }

        public void Dispose()
        {
            _timer.Elapsed -= _timer_Elapsed;
            _ic.Notified -= _ic_Notified;
            _timer.Dispose();
        }

        /// <summary>
        /// 查询表
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="pageSize">单页记录数量</param>
        /// <param name="pageNo">查询页</param>
        /// <returns>Token值</returns>
        public string Select(string sql, int pageSize = 100, int pageNo = 0)
        {
            return _to.Select(sql, pageSize, pageNo);
        }

        /// <summary>
        /// 同步查询表，Xml结果
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="pageSize">单页记录数量</param>
        /// <param name="pageNo">查询页</param>
        /// <returns>若为空，则失败</returns>
        public TableXmlWrapper SelectXmlSync(string sql, int pageSize = 100, int pageNo = 0)
        {
            _timer.Start();

            _txw = null;
            _to.Format = ResultFormat.Xml;
            Token = _to.Select(sql, pageSize, pageNo);

            while (_txw == null) { if (_timeout) break; }

            _timer.Stop();

            return _txw;
        }

        /// <summary>
        /// 同步查询表，Json结果
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageNo"></param>
        /// <returns>若为空，则失败</returns>
        public CVRJsonParser SelectJsonSync(string sql, int pageSize = 100, int pageNo = 0)
        {
            _timer.Start();

            _cjp = null;
            _to.Format = ResultFormat.Json;
            Token = _to.Select(sql, pageSize, pageNo);

            while (_cjp == null) { if (_timeout) break; }

            _timer.Stop();

            return _cjp;
        }

        /// <summary>
        /// 插入记录
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>操作token值</returns>
        public string Insert(string sql)
        {
            return _to.Insert(sql);
        }

        /// <summary>
        /// 同步插入记录
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>操作token值</returns>
        public UpdateItem InsertSync(string sql)
        {
            _timer.Start();

            _ui = null;
            _to.Insert(sql);

            while (_ui == null) { if (_timeout) break; }

            _timer.Stop();

            return _ui;
        }

        /// <summary>
        /// 更新表
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public string Update(string sql)
        {
            return _to.Update(sql);
        }

        /// <summary>
        /// 同步更新表
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>若为空，则失败</returns>
        public UpdateItem UpdateSync(string sql)
        {
            _timer.Start();

            _ui = null;
            _to.Update(sql);

            while (_ui == null) { if (_timeout) break; }

            _timer.Stop();

            return _ui;
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>操作token值</returns>
        public string Delete(string sql)
        {
            return _to.Delete(sql);
        }

        /// <summary>
        /// 同步删除表记录
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>若为空，则失败</returns>
        public UpdateItem DeleteSync(string sql)
        {
            _timer.Start();

            _ui = null;
            _to.Delete(sql);

            while (_ui == null) { if (_timeout) break; }

            _timer.Stop();

            return _ui;
        }

        #region 操作
        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timeout = true;
            _timer.Stop();

            Notified?.Invoke(CVRTableResult.OperationTimeout, Token);
        }

        private void _ic_Notified(CVResult r, object o)
        {
            if (r == CVResult.TableQueryReceived)
            {
                if (o is QueryResultPt)
                {
                    QueryResultPt qrp = o as QueryResultPt;
                    if (qrp.success == 0)
                    {
                        if (qrp.resultFmart == "xml")
                        {
                            _txw = new TableXmlWrapper(qrp);
                            Notified?.Invoke(CVRTableResult.SelectXmlReceived, _txw);
                        }
                        else if (qrp.resultFmart == "json")
                        {
                            _cjp = new CVRJsonParser(qrp.result)
                            {
                                Token = qrp.token,
                                Count = qrp.count
                            };
                            Notified?.Invoke(CVRTableResult.SelectJsonReceived, _cjp);
                        }
                    }
                    else
                    {
                        Notified?.Invoke(CVRTableResult.SelectFailed, qrp.msg);
                    }
                }
            }
            else if (r == CVResult.TableUpdateReceived)
            {
                if (o is ExecuteResultPt)
                {
                    ExecuteResultPt erp = o as ExecuteResultPt;
                    _ui = new UpdateItem
                    {
                        Id = erp.token,
                        Succeed = erp.success == 0,
                        Msg = erp.msg
                    };
                    Notified?.Invoke(CVRTableResult.UpdateResultReceived, _ui);
                }
            }
        }
        #endregion

        #region 字段
        protected Infocenter _ic = null;
        protected TableOp _to = null;
        /// <summary>
        /// 同步操作相关
        /// </summary>
        private Timer _timer = new Timer(3000);//3sec超时
        private bool _timeout = false;
        private TableXmlWrapper _txw = null;
        private CVRJsonParser _cjp = null;
        private UpdateItem _ui = null;
        #endregion
    }
}
