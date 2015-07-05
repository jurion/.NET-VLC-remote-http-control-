using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace VlcRemotePhone.Common
{
    public static class ProgressBarHelper
    {
        public static IAsyncAction ShowProgress(string text)
        {
            StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            statusBar.ProgressIndicator.Text = text;
            return statusBar.ProgressIndicator.ShowAsync();
        }

        public static IAsyncAction HideProgress()
        {
            StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            return statusBar.ProgressIndicator.HideAsync();
        }

    }
}
