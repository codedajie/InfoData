using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace ChivaVR.Net.Toolkit
{
    public static class Kits
    {
        #region ShellExecute Open operation
        [DllImport("shell32.dll")]
        //HINSTANCE ShellExecute(HWND hwnd, LPCTSTR lpOperation, LPCTSTR lpFile, LPCTSTR lpParameters, LPCTSTR lpDirectory, INT nShowCmd);
        //preprotype
        internal static extern bool ShellExecute(IntPtr hWnd
            , string lpOperation
            , string lpFile
            , string lpParameters
            , string lpDirectory
            , int nShowCmd);

        public static void ShellOpen(string file)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(ShellOpenCallback));
            thread.IsBackground = true;
            thread.Start(file);
        }
        private static void ShellOpenCallback(object o)
        {
            IntPtr hWnd = new IntPtr(-1);
            ShellExecute(hWnd, "open", (string)o, null, null, 1);//#define SW_SHOWNORMAL 1
        }
        #endregion

        /// <summary>
        /// 获取文件在系统执行目录的绝对路径
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string ToBin(string name)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name);
        }

        /// <summary>
        /// 获取第一个ip
        /// todo
        /// </summary>
        /// <returns></returns>
        public static string GetIp()
        {
            foreach (System.Net.IPAddress ia in System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList)
            {
                if (ia.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ia.ToString();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取并标准化文件修改日期
        /// </summary>
        /// <param name="pathfile"></param>
        /// <returns></returns>
        public static string GetLastWriteTime(string pathfile)
        {
            try
            {
                DateTime dt = File.GetLastWriteTime(pathfile);
                return dt.Year + "-" + dt.Month.ToString("00") + "-" + dt.Day.ToString("00") + " " + dt.Hour.ToString("00") + ":" + dt.Minute.ToString("00") + ":" + dt.Second.ToString("00");
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// 设置文件三个日期
        /// </summary>
        /// <param name="pathfile"></param>
        /// <param name="dt"></param>
        public static void SetFileDateTime(string pathfile, DateTime dt)
        {
            try
            {
                File.SetLastAccessTime(pathfile, dt);
                File.SetLastWriteTime(pathfile, dt);
                File.SetCreationTime(pathfile, dt);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 格式化字节值
        /// </summary>
        /// <param name="byteCount"></param>
        /// <returns></returns>
        public static string ByteToString(long byteCount)
        {
            string size = "0B";
            if (byteCount >= 1099511627776.0)
                size = String.Format("{0:##.###}", byteCount / 1099511627776.0) + "TB";
            if (byteCount >= 1073741824.0)
                size = String.Format("{0:##.###}", byteCount / 1073741824.0) + "GB";
            else if (byteCount >= 1048576.0)
                size = String.Format("{0:##.###}", byteCount / 1048576.0) + "MB";
            else if (byteCount >= 1024.0)
                size = String.Format("{0:##.###}", byteCount / 1024.0) + "KB";
            else if (byteCount > 0 && byteCount < 1024.0)
                size = byteCount.ToString() + "B";

            return size.ToString();
        }

        public static string DateTimeDesc(DateTime dt)
        {
            return dt.Month.ToString("00") + "-" + dt.Day.ToString("00") + " " + dt.Hour.ToString("00") + ":" + dt.Minute.ToString("00");
        }

        public static string GetNowString()
        {
            DateTime dt = DateTime.Now;
            return dt.Year + "-" + dt.Month.ToString("00") + "-" + dt.Day.ToString("00") + " " + dt.Hour.ToString("00") + ":" + dt.Minute.ToString("00") + ":" + dt.Second.ToString("00");
        }

        public static string GetDateTimeString(DateTime? d)
        {
            DateTime dt = d.GetValueOrDefault();
            return dt.Year + "-" + dt.Month.ToString("00") + "-" + dt.Day.ToString("00") + " " + dt.Hour.ToString("00") + ":" + dt.Minute.ToString("00") + ":" + dt.Second.ToString("00");
        }

        public static bool DateTimeEqual(DateTime dt1, DateTime dt2)
        {
            return dt1.Minute == dt2.Minute
                && dt1.Hour == dt2.Hour
                && dt1.Day == dt2.Day
                && dt1.Month == dt2.Month
                && dt1.Year == dt2.Year;
        }

        public static string GetFilename(string pathfile)
        {
            return (new FileInfo(pathfile)).Name;
        }

        public static string SafedString(string str, string def = "")
        {
            return str == null || str.Length == 0 ? def : str;
        }

        public static string GetSafedPathname(string pathname)
        {
            if (pathname==null)
            {
                return "";
            }
            if (pathname.StartsWith("/"))
            {
                return pathname;
            }
            return pathname.Length == 0 ? "" : "/" + pathname;
        }

        public static string GetDashPathname(params string[] paths)
        {
            string pathname = string.Empty;
            foreach(string s in paths)
            {
                pathname += GetSafedPathname(s);
            }

            return pathname.Replace(@"\", "/");
        }

        public static string GetTempPathfile(string pathname)
        {
            string tempath = Path.GetTempPath();
            return Path.Combine(tempath, pathname);
        }

        /// <summary>
        /// 删除目录及其子目录所有内容
        /// </summary>
        /// <param name="dir"></param>
        public static void DeleteDirectory(string dir)
        {
            if (Directory.GetDirectories(dir).Length == 0 && Directory.GetFiles(dir).Length == 0)
            {
                try { Directory.Delete(dir); } catch { }
                return;
            }
            foreach (string d in Directory.GetDirectories(dir))
            {
                DeleteDirectory(d);
            }
            foreach (string f in Directory.GetFiles(dir))
            {
                try { File.Delete(f); } catch { }
            }
            try { Directory.Delete(dir); } catch { }
        }

        public static string RegexXmlText(string text)
        {
            return System.Text.RegularExpressions.Regex.Replace(text, @"[&></\\]", "");
        }
    }
}
