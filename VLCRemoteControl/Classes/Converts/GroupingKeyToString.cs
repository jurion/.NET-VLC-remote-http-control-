using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace VLCRemoteControl.Classes.Converts
{
    public class GroupingKeyToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                if (value is DateTime)
                {
                    return ((DateTime)value).ToString("dd/MM/yyyy");
                }
                else
                {
                    return value;
                }
            }
            else
            {
                return "";
            }
                 
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
