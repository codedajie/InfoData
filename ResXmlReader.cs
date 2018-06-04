using ChivaVR.Net.Toolkit;
using System.Collections.Generic;
using System.Xml;

namespace ChivaVR.Net.Core
{
    /// <summary>
    /// 资源xml 容器
    /// </summary>
    public class ResXmlItem
    {
        public string Id { get { return _xn.Attributes["id"].Value; } }
        public string Name { get { return _xn.ChildNodes[0].InnerText; } }
        public string Date { get { return _xn.ChildNodes[1].InnerText; } }
        public string Size { get { return _xn.ChildNodes[2].InnerText; } }
        public string Group { get { return _xn.ChildNodes[3].InnerText; } }
        public string Version { get { return _xn.ChildNodes[4].InnerText; } }
        public string Number { get { return _xn.ChildNodes[5].InnerText; } }
        public string Remark { get { return _xn.ChildNodes[6].InnerText; } }
        public string Pathname { get { return Kits.GetDashPathname(_xn.ChildNodes[7].InnerText, Name); } }

        private XmlNode _xn = null;

        public ResXmlItem(XmlNode xn)
        {
            _xn = xn;
        }
    }

    public class ResXmlReader : XmlFileWrapper
    {
        public List<ResXmlItem> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new List<ResXmlItem>();
                    foreach (XmlNode xn in Root.ChildNodes)
                    {
                        _items.Add(new ResXmlItem(xn));
                    }
                }
                return _items;
            }
        }

        List<ResXmlItem> _items = null;

        public ResXmlReader(string pathfile) : base(pathfile)
        {

        }
    }
}
