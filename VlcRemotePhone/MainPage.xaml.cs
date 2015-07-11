using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VlcLib.ViewModels;
using VlcRemotePhone.Common;
using VlcRemotePhone.Settings;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour en savoir plus sur le modèle d'élément Page vierge, consultez la page http://go.microsoft.com/fwlink/?LinkId=391641

namespace VlcRemotePhone
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        AppConfiguration configDialog = new AppConfiguration();
        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = App.ViewModel;
            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.CheckConfig();
            this.youtubeResultsList.ItemsSource = App.YoutubeSearchResults;
        }

        private async void SearchTube(string search)
        {
            await ProgressBarHelper.ShowProgress("Searching YouTube");
            var searchListRequest = App.YoutubeService.Search.List("snippet");
            searchListRequest.Q = search;
            searchListRequest.MaxResults = 50;
            var searchListResponse = await searchListRequest.ExecuteAsync();

            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.
            App.YoutubeSearchResults.Clear();
            foreach (var searchResult in searchListResponse.Items)
            {
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":
                        App.YoutubeSearchResults.Add(new YouTubeMedia()
                        {
                            Description = searchResult.Snippet.Description,
                            Name = searchResult.Snippet.Title,
                            ThumbUrl = searchResult.Snippet.Thumbnails.Default.Url,
                            VideoId = searchResult.Id.VideoId,
                            PostedBy = searchResult.Snippet.ChannelTitle
                        });
                        break;

                }
            }
            await ProgressBarHelper.HideProgress();
        }
        private async void CheckConfig()
        {
            if (!App.VlcSettings.IsLoadedFromStorage)
            {
                await configDialog.ShowAsync();
            }
            TryToConnect();
        }

        private async void TryToConnect()
        {
            await ProgressBarHelper.ShowProgress("Connecting to VLC…");
            var res = await App.ViewModel.ConnectAndLoad(App.VlcSettings.GetUrl(), App.VlcSettings.Password, @"");
            await ProgressBarHelper.HideProgress();
            if (!res)
            {
                var dialog = new Windows.UI.Popups.MessageDialog("Connection failed. Pleease check settings");
                await dialog.ShowAsync();
                return;
            }

        }

        /// <summary>
        /// Invoqué lorsque cette page est sur le point d'être affichée dans un frame.
        /// </summary>
        /// <param name="e">Données d’événement décrivant la manière dont l’utilisateur a accédé à cette page.
        /// Ce paramètre est généralement utilisé pour configurer la page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: préparer la page pour affichage ici.

            // TODO: si votre application comporte plusieurs pages, assurez-vous que vous
            // gérez le bouton Retour physique en vous inscrivant à l’événement
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed.
            // Si vous utilisez le NavigationHelper fourni par certains modèles,
            // cet événement est géré automatiquement.
        }

        private async void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            var res = await configDialog.ShowAsync();
            if (res == ContentDialogResult.Secondary)
            {
                this.TryToConnect();
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.ToggleFullScreen();
        }

        private async void youtubeResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                var elem = e.AddedItems[0] as YouTubeMedia;
                await ProgressBarHelper.ShowProgress("Luncing " + elem.Name);
                App.ViewModel.PlayItem(new MediaElemntViewModel()
                {
                    FileUri = elem.GetFullurl()
                });
                await ProgressBarHelper.HideProgress();
            }
        }

        private void AppBarButton_Click_2(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.YoutubeSearchText.Text))
            {
                this.SearchTube(this.YoutubeSearchText.Text.Trim());
            }
        }

        private void YoutubeSearchText_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                this.Focus(FocusState.Programmatic);
                this.AppBarButton_Click_2(null, null);
            }
        }
    }
}
