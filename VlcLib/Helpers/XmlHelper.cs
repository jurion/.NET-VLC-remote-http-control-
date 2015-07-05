using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VlcLib.Helpers
{
    internal static class XmlHelper
    {
        public static XElement SelectElement(XElement doc, string path)
        {
            XElement res = null;
            if (string.IsNullOrEmpty(path))
            {
                return res;
            }
            var tracks = path.Split('/');
            res = doc.Element(tracks[0]);
            for (int i = 1; (i < tracks.Length) && (res != null); i++)
            {
                res = res.Element(tracks[i]);
            }
            return res;
        }

        public static IEnumerable<XElement> SelectElements(XElement doc, string path)
        {
            IEnumerable<XElement> res = null;
            if (string.IsNullOrEmpty(path))
            {
                return res;
            }
            var tracks = path.Split('/');
            res = doc.Elements(tracks[0]);
            if (tracks.Length > 1)
            {
                List<XElement> temp = new List<XElement>();
                foreach (var elem in res)
                {
                    temp.AddRange(SelectElements(elem, string.Join("/", tracks, 1, tracks.Length - 1)));
                }
                res = temp;
            }
            return res;
        }

        public static XElement GetNodeByNameAttribute(XElement item, string NodeName, string AttrName, string AttrValue)
        {
            var res = (from e in item.Elements(NodeName)
                       where e.Attribute(AttrName).Value == AttrValue
                       select e).SingleOrDefault();
            return res;
        }

        public static IEnumerable<XElement> GetNodeArrayByNameAttribute(XElement item, string NodeName, string AttrName, string AttrValue)
        {
            var res = (from e in item.Elements(NodeName)
                       where e.Attribute(AttrName).Value == AttrValue
                       select e);
            return res;
        }
    }
}
