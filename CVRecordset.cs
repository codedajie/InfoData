using ChivaVR.Net.Core;
using ChivaVR.Net.IC;
using System.Collections.Generic;

namespace ChivaVR.Net.Task
{
    /// <summary>
    /// 表同步操作
    /// 
    /// </summary>
    public class CVRecordset : CVRTableTask
    {
        public new string Version { get { return "1.0.0504.1105"; } }

        public CVRecordset(Infocenter ic, string table) : base(ic)
        {
            _table = table;
        }

        /// <summary>
        /// 获取记录集数量
        /// 针对大数据集采用排序可以提高效率
        /// </summary>
        /// <param name="field">排序字段</param>
        /// <returns>记录集数量，-1则错误</returns>
        public int GetCount(string field = "")
        {
            CVRJsonParser jp = base.SelectJsonSync("SELECT " + (field == "" ? "*" : field) + " FROM "
                + _table + (field == "" ? "" : " ORDER BY " + field));
            return jp.Count;
        }

        /// <summary>
        /// 获取记录集
        /// </summary>
        /// <param name="where">WHERE条件</param>
        /// <param name="pageNum">页容量</param>
        /// <param name="page">当前页</param>
        /// <param name="fields">字段集，第一个字段用来排序</param>
        /// <returns>记录集，获取失败则为 null||Count==0</returns>
        public List<Dictionary<string, string>> Get(string where, int pageNum, int page, params string[] fields)
        {
            string field = string.Empty;
            foreach (string f in fields)
            {
                field += f + ",";
            }
            field = field.Remove(field.Length - 1);

            string sql = "SELECT " + field + " FROM " + _table
                + (where == "" ? "" : " WHERE " + where) + (fields[0] == "*" ? "" : " ORDER BY " + fields[0]);
            CVRJsonParser jp = base.SelectJsonSync(sql, pageNum, page);
            if (jp != null)
            {
                List<Dictionary<string, string>> rss = new List<Dictionary<string, string>>();
                for (int i = 0; i < jp.Count; ++i)
                {
                    if (jp[i] != null)
                    {
                        rss.Add(jp[i]);
                    }
                }
                return rss;
            }

            return null;
        }

        /// <summary>
        /// 增加一条记录，例如：Add("name=zzj","sex=male")
        /// </summary>
        /// <param name="fieldValues">字段=值</param>
        /// <returns>是否成功</returns>
        public bool Add(params string[] fieldValues)
        {
            string fields = "";
            string values = "";
            foreach (string fv in fieldValues)
            {
                int i = fv.IndexOf("=");
                string f = fv.Substring(0, i);
                string v = fv.Substring(i + 1);
                fields += "`" + f + "`,";
                values += "'" + v + "',";
            }
            fields = fields.Remove(fields.Length - 1);
            values = values.Remove(values.Length - 1);

            string sql = "INSERT INTO `" + _table + "`(" + fields + ")VALUES(" + values + ")";
            UpdateItem ui = base.InsertSync(sql);

            return ui.Succeed;
        }

        /// <summary>
        /// 改变字段指定值
        /// </summary>
        /// <param name="field">字段</param>
        /// <param name="oldValue">旧值</param>
        /// <param name="newValue">新值</param>
        /// <param name="where">额外条件</param>
        /// <returns>是否成功</returns>
        public bool Update(string field, string oldValue, string newValue, string where = "")
        {
            string sql = "UPDATE `" + _table + "` SET `" + field + "` = '" + newValue + "' WHERE `" + field + "` = '" + oldValue + "'"
                + (where == "" ? "" : " AND " + where);
            UpdateItem ui = base.UpdateSync(sql);

            return ui.Succeed;
        }

        /// <summary>
        /// 改变字段值
        /// </summary>
        /// <param name="field">字段</param>
        /// <param name="newValue">新值</param>
        /// <param name="where">额外条件</param>
        /// <returns>是否成功</returns>
        public bool Set(string field, string newValue, string where = "")
        {
            string sql = "UPDATE `" + _table + "` SET `" + field + "` = '" + newValue
                + (where == "" ? "'" : "' WHERE " + where);
            UpdateItem ui = base.UpdateSync(sql);

            return ui.Succeed;
        }

        /// <summary>
        /// 条件删除记录
        /// </summary>
        /// <param name="where">条件</param>
        /// <returns>是否成功</returns>
        public new bool Delete(string where)
        {
            string sql = "DELETE FROM `" + _table + "` WHERE " + where;
            UpdateItem ui = base.DeleteSync(sql);

            return ui.Succeed;
        }

        /// <summary>
        /// 清空记录集
        /// </summary>
        /// <returns>是否成功</returns>
        public bool Clear()
        {
            string sql = "DELETE FROM `" + _table + "`";
            UpdateItem ui = base.DeleteSync(sql);

            return ui.Succeed;
        }

        /// <summary>
        /// 恢复AI字段从1开始
        /// </summary>
        public void ResetAi(int i = 0)
        {
            string sql = "ALTER TABLE " + _table + " AUTO_INCREMENT = " + i;
            base.Update(sql);
        }

        #region 字段
        private string _table = string.Empty;
        #endregion
    }
}
