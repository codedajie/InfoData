using System;
using System.Xml;

namespace ChivaVR.Net.Toolkit
{
    public class ResXmlNodeParser
    {
        //objectid、pathfile、name、folder、version、versionname、lastwrite、remark、crc
        public string Id { get { return _xn.ChildNodes[0].InnerText; } }
        public string Pathfile { get { return _xn.ChildNodes[1].InnerText; } }
        public string Name { get { return _xn.ChildNodes[2].InnerText; } }
        public string Group { get { return _xn.ChildNodes[3].InnerText; } }
        public string Version { get { return _xn.ChildNodes[4].InnerText; } }
        public string Versioname { get { return _xn.ChildNodes[5].InnerText; } }
        public DateTime Lastwrite { get { return DateTime.Parse(_xn.ChildNodes[6].InnerText); } }
        public string Remark { get { return _xn.ChildNodes[7].InnerText; } }
        public string Crc { get { return _xn.ChildNodes[8].InnerText; } }

        private XmlNode _xn = null;

        public ResXmlNodeParser(XmlNode xn)
        {
            _xn = xn;
        }
    }
}
