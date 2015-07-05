using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace VlcLib.Media
{
    public enum MediaType
    {
        None,
        Audio,
        Video
    }

    public class VlcMediaItem
    {

        public string FileName { get; set; }
        public string Name { get; set; }
        public MediaType MediaType { get; set; }
        public DateTime LastAccess { get; set; }

        public VlcMediaItem() { }

        public VlcMediaItem(XElement elem)
        {
            this.FileName = Uri.UnescapeDataString(elem.Attribute("path").Value);
            this.Name = Uri.UnescapeDataString(elem.Attribute("name").Value);
            var ticks = Convert.ToInt64(elem.Attribute("modification_time").Value);
            this.LastAccess = Helper.UnixTimeStampToDateTime(ticks);
            this.MediaType = Helper.GetMediaType(this.FileName);
        }

    }
}
