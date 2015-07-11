using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VlcLib.ViewModels
{
    public class YouTubeMedia : BaseNotification
    {
        private string videoId = "";
        private string description = "";
        private string name = "";
        private string thumbUrl = "";
        private string postedBy = "";

        public string VideoId
        {
            get
            {
                return videoId;
            }

            set
            {
                if (videoId != value)
                {
                    videoId = value;
                    NotifyPropertyChanged("VideoId");
                }
            }
        }

        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                if (description != value)
                {
                    description = value;
                    NotifyPropertyChanged("Description");
                }
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                if (name != value)
                {
                    name = value;
                    NotifyPropertyChanged("Name");
                }

            }
        }

        public string ThumbUrl
        {
            get
            {
                return thumbUrl;
            }

            set
            {
                if (thumbUrl != value)
                {
                    thumbUrl = value;
                    NotifyPropertyChanged("ThumbUrl");
                }
            }
        }

        public string PostedBy
        {
            get
            {
                return postedBy;
            }

            set
            {
                if (postedBy != value)
                {
                    postedBy = value;
                    NotifyPropertyChanged("PostedBy");
                }
            }
        }

        public string GetFullurl()
        {
            return string.Format("https://www.youtube.com/watch?v={0}", this.videoId);
        }
    }
}
