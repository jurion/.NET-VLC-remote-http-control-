using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VlcLib.ViewModels
{
    public class BaseNotification : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }

        }

        protected void InvokeIfRequired(SynchronizationContext context, SendOrPostCallback action)
        {
            if (SynchronizationContext.Current == context)
            {
                action(new Object());
            }
            else
            {
                context.Post(action, new Object());
            }
        }
    }
}
