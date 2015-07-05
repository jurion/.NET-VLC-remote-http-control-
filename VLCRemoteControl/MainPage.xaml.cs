using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VlcLib;
using VlcLib.Status;
using VlcLib.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace VLCRemoteControl
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = App.ViewModel;
            App.ViewModel.AudioStreams.CollectionChanged += AudioStreams_CollectionChanged;
            App.ViewModel.VideoStreams.CollectionChanged += VideoStreams_CollectionChanged;
            App.ViewModel.SubtitlesStreams.CollectionChanged += SubtitlesStreams_CollectionChanged;
            testRun();
        }

        void SubtitlesStreams_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            subsStreams.Items.Clear();
            foreach (var item in App.ViewModel.SubtitlesStreams)
            {
                MenuFlyoutItem i = new MenuFlyoutItem()
                {
                    Text = item.Name,
                    Tag = "subs " + item.Id,
                };
                i.Click += selectStream_Click;
                subsStreams.Items.Add(i);
            }
        }

        void VideoStreams_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            videoStreams.Items.Clear();
            foreach (var item in App.ViewModel.VideoStreams)
            {
                MenuFlyoutItem i = new MenuFlyoutItem()
                {
                    Text = item.Name,
                    Tag = "video " + item.Id,
                };
                i.Click += selectStream_Click;
                videoStreams.Items.Add(i);
            }
        }

        void selectStream_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = (MenuFlyoutItem)sender;
            var id = Convert.ToInt32(item.Tag.ToString().Split(' ')[1]);
            
        }

        void AudioStreams_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            audioStreams.Items.Clear();
            foreach (var item in App.ViewModel.AudioStreams)
            {
                MenuFlyoutItem i = new MenuFlyoutItem()
                {
                    Text = item.Name,
                    Tag = "audio " + item.Id,
                };
                i.Click += selectStream_Click;
                audioStreams.Items.Add(i);
            }
        }


        public async void testRun()
        {
            var res = await App.ViewModel.ConnectAndLoad("http://delta:8080/", "jurik", @"e:\\downloads");
            if (!res)
            {
                var dialog = new Windows.UI.Popups.MessageDialog("Connection failed. Pleease check settings");
                await dialog.ShowAsync();
                return;
            }
            res = await App.ViewModel.RefreshList();
            if (!res)
            {
                var dialog = new Windows.UI.Popups.MessageDialog("Error loading media list");
                await dialog.ShowAsync();
            }
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            App.ViewModel.IsSortingByName = (sender as ToggleSwitch).IsOn;

        }

        private void ToggleSwitch_Toggled_1(object sender, RoutedEventArgs e)
        {
            App.ViewModel.IsSortingAscendant = (sender as ToggleSwitch).IsOn;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            App.ViewModel.Filter = (sender as TextBox).Text.Trim();
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await App.ViewModel.RefreshList();
        }

        private void StackPanel_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var selected = (MediaElemntViewModel)itemsView.SelectedItem;
            App.ViewModel.PlayItem(selected);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.PlayResume();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.Pause();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.Stop();
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.ToggleFullScreen();
        }

        private void PreviousButton20_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.AdvanceSeconds(-20);
        }

        private void PreviousButton60_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.AdvanceSeconds(-60);
        }

        private void ForwardButton20_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.AdvanceSeconds(20);
        }

        private void ForwardButton60_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.AdvanceSeconds(60);
        }

        private void SubsAdd_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.AdjustSubtitles(100);
        }

        private void SubsRemove_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.AdjustSubtitles(-100);
        }
    }
}
