using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VlcLib.Status;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace VLCRemoteControl.Classes.Converts
{
    class SubsToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var items = (ObservableCollection<VlcFlux>)value;
            if (items.Count > 0)
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
