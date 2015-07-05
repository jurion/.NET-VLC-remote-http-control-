using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VlcLib.Media;

namespace VlcLib.ViewModels
{
    public class MediaElemntViewModel : BaseNotification
    {

        public MediaElemntViewModel() { }

        public MediaElemntViewModel(VlcMediaItem item)
            : this()
        {
            this.FileUri = item.FileName;
            this.LastDate = item.LastAccess;
            this.MediaType = item.MediaType;
            this.Name = item.Name;
        }

        private MediaType mediaType;
        private string fileUri;
        private DateTime lastDate;
        private string name;


        public MediaType MediaType
        {
            get { return mediaType; }
            set
            {
                if (value != mediaType)
                {
                    mediaType = value;
                    NotifyPropertyChanged("MediaType");
                }
            }
        }
        public string FileUri
        {
            get { return fileUri; }
            set
            {
                if (value != fileUri)
                {
                    fileUri = value;
                    NotifyPropertyChanged("FileUri");
                }
            }
        }
        public DateTime LastDate
        {
            get { return lastDate; }
            set
            {
                if (value != lastDate)
                {
                    lastDate = value;
                    NotifyPropertyChanged("LastDate");
                    NotifyPropertyChanged("LastDateForGrouping");
                }
            }
        }

        public DateTime LastDateForGrouping
        {
            get { return lastDate.Date; }
        }
        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    NotifyPropertyChanged("Name");
                    NotifyPropertyChanged("NameForGrouping");
                }
            }
        }
        public string NameForGrouping
        {
            get
            {
                return this.name[0].ToString().ToUpper();
            }
        }
    }
}
