using System;
using System.IO;
using System.Xml;

namespace ChivaVR.Net.Toolkit
{
    public class SealedXml
    {
        #region 属性
        public XmlNode Root { get; set; }
        public XmlDocument Doc { get; set; }
        #endregion

        #region 构造
        /// <summary>
        /// 构造，并指定是否为加密
        /// </summary>
        /// <param name="sealable"></param>
        public SealedXml(bool sealable)
        {
            _sealable = sealable;
        }
        #endregion

        #region 方法
        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <param name="pathfile"></param>
        /// <returns></returns>
        public XmlDocument ReadFile(string pathfile)
        {
            _pathfile = pathfile;
            try
            {
                string text = File.ReadAllText(pathfile);
                return ReadString(text);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 读取配置文本
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public XmlDocument ReadString(string text)
        {
            try
            {
                if (_sealable)
                {
                    text = DesString.Decrypt(text);
                }

                Doc = new XmlDocument();
                Doc.LoadXml(text);

                if (Doc != null)
                {
                    Root = Doc.SelectSingleNode("root");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Doc;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="pathfile"></param>
        public void Save(string pathfile)
        {
            string text = Doc.InnerXml;

            if (_sealable)
            {
                text = DesString.Encrypt(text);
            }

            File.WriteAllText(pathfile,text);
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            if(_pathfile == null || _pathfile==string.Empty)
            {
                throw new NullReferenceException("路径文件名为空");
            }
            Save(_pathfile);
        }
        #endregion

        #region 工具
        /// <summary>
        /// 加密文本
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string SealStringToString(string text)
        {
            return DesString.Encrypt(text);
        }

        /// <summary>
        /// 加密文件
        /// </summary>
        /// <param name="infile"></param>
        /// <param name="outfile"></param>
        public static void SealFileToFile(string infile, string outfile)
        {
            File.WriteAllText(outfile, DesString.Encrypt(File.ReadAllText(infile)));
        }

        /// <summary>
        /// 解密文本
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string UnsealStringToString(string text)
        {
            return DesString.Decrypt(text);
        }

        /// <summary>
        /// 解密文件
        /// </summary>
        /// <param name="infile"></param>
        /// <param name="outfile"></param>
        public static void UnsealFileToFile(string infile, string outfile)
        {
            File.WriteAllText(outfile, DesString.Decrypt(File.ReadAllText(infile)));
        }
        #endregion

        #region Fields
        protected bool _sealable = true;
        protected string _pathfile;
        #endregion
    }
}
