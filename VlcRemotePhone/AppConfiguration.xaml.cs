using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VlcLib;
using VlcRemotePhone.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers.Provider;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour en savoir plus sur le modèle d'élément Boîte de dialogue de contenu, consultez la page http://go.microsoft.com/fwlink/?LinkID=390556

namespace VlcRemotePhone
{
    public sealed partial class AppConfiguration : ContentDialog
    {
        public AppConfiguration()
        {
            this.InitializeComponent();
            this.DataContext = App.VlcSettings;
            this.connectionResult.Visibility = Visibility.Collapsed;
        }

        private async void TestConnection()
        {
            await ProgressBarHelper.ShowProgress("Connecting to VLC…");
            var com = new VlcWebControler(this.GetUrl(), this.password.Password);
            var isOk = await com.TestConnexion();
            if (isOk)
            {
                this.connectionResult.Text = "Connection TEST: OK!";
             /*   await com.PlayFile(new VlcLib.Media.VlcMediaItem()
                {
                    FileName = "https://www.youtube.com/watch?v=OCy5461BtTg?vq=hd1080"
                });*/
            }
            else
            {
                this.connectionResult.Text = "Connection TEST: Failed";
            }
            await ProgressBarHelper.HideProgress();
        }
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            /*this.connectionResult.Text = "Trying to connect…";
            this.TestConnection();*/
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            App.VlcSettings.IpAdress = this.IP_Adress.Text.Trim();
            App.VlcSettings.Password = this.password.Password;
            App.VlcSettings.Port = int.Parse(this.Port_number.Text.Trim());
        }

        private string GetUrl()
        {
            return "http://" + this.IP_Adress.Text.Trim() + ":" + this.Port_number.Text.Trim() + "/";
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.connectionResult.Visibility = Visibility.Visible;
            this.connectionResult.Text = "Trying to connect…";
            this.TestConnection();
        }
    }
}
