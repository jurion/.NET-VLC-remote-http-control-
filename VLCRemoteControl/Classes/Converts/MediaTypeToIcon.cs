using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VlcLib.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace VLCRemoteControl.Classes.Converts
{
    public class MediaTypeToIcon : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                MediaType mt = (MediaType)value;
                if (mt == MediaType.Audio)
                {
                    return new Uri("ms-appx:///Assets/Icons/Audio_white.png");
                }
                else if (mt == MediaType.Video)
                {
                    return new Uri("ms-appx:///Assets/Icons/Video_white.png");
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
