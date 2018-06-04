using ChivaVR.Net.Core;
using ChivaVR.Net.Toolkit;

namespace ChivaVR.Net.IC
{
    public sealed partial class Infocenter : IChivaVRNet
    {
        private TableOp _logTableOp = null;
        private string _localIp;
        private string _localName;

        internal void AddLog(Log.Type type, string app, string evt, string text)
        {
            if (_logTableOp != null)
            {
                if (type != Log.Type.Sys && string.Equals(app, Log.SysTypeDesc, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    //'sys' 系统保留使用
                    Notify(CVResult.LogFailed, "'" + app + "'系统保留使用");
                    return;
                }
                if (type == Log.Type.App || Log.EnableSystemEvent)
                {

                    string sql = @"INSERT INTO pub_log(ty,uid,uname,app,ip,machine,evt,text,tm)VALUES("
                    + (ushort)type + "," + Id + ",'" + Username + "','" + app + "','" + _localIp + "','"
                    + _localName + "','" + evt + "','" + text + "','"
                    + Kits.GetNowString() + "')";
                    _logTableOp.Token = type == 0 ? "add_evt" : "add_log";
                    _logTableOp.Insert(sql);
                }
            }
            else
            {
                Notify(CVResult.LogFailed, "日志系统无效");
            }
        }
    }
}
