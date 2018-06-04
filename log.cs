using static ChivaVR.Net.Core.Log;

namespace ChivaVR.Net.Core
{
    public delegate void AddLogEventHandler(Type type, string client, string evt, string text);

    public static class Log
    {
        public enum Type : ushort
        {
            Sys = 0,
            App = 1
        }

        public static event AddLogEventHandler Added = null;

        private static bool _enable = true;
        public static string SysTypeDesc = "sys";

        public static bool EnableSystemEvent
        {
            get { return _enable; }
            set { _enable = value; }
        }

        public static bool Usable
        {
            get { return Added != null; }
        }

        public static void AddLog(string app, string evt, string text)
        {
            Added?.Invoke(Type.App, app, evt, text);
        }

        public static void AddEvent(string evt, string text)
        {
            Added?.Invoke(Type.Sys, SysTypeDesc, evt, text);
        }
    }
}
