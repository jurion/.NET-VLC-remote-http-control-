using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VlcLib.Status;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace VLCRemoteControl.Classes.Converts
{
    public class PlayButtonVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            VlcState obj = (VlcState)value;
            if ((obj == VlcState.Stopped) || (obj == VlcState.Paused))
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
