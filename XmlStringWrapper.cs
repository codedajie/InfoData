using System;
using System.Data;
using System.IO;
using System.Xml;

namespace ChivaVR.Net.Toolkit
{

    /// <summary>
    /// XML文本剖析
    /// </summary>
    public class XmlStringWrapper
    {
        protected XmlDocument _xd = new XmlDocument();
        protected XmlNode Root { get { return Nodes; } }

        public XmlStringWrapper(string text)
        {
            try
            {
                _xd.LoadXml(text);
            }
            catch(Exception ex)
            {
                throw new Exception("XML字符串包含非法字符", ex);
            }
        }

        public XmlNode Nodes
        {
            get
            {
                //没有：<?xml version="1.0" encoding="utf-8"?>
                return _xd.FirstChild;
            }
        }

        public DataSet DataSet
        {
            get
            {
                StringReader sr = null;
                XmlTextReader xtr = null;
                try
                {
                    DataSet ds = new DataSet();
                    sr = new StringReader(_xd.InnerXml);
                    
                    xtr = new XmlTextReader(sr);
                    ds.ReadXml(xtr);
                    return ds;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (xtr != null)
                    {
                        xtr.Close();
                    }
                }
            }
        }

        public void Save(string pathfile)
        {
            _xd.Save(pathfile);
        }

        public static void ToFile(string pathfile, string text)
        {
            File.WriteAllText(pathfile, text);
        }
    }
}
