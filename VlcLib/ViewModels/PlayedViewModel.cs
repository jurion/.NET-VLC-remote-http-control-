using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VlcLib.Helpers;
using VlcLib.Status;

namespace VlcLib.ViewModels
{
    public class PlayedViewModel : BaseNotification
    {
        public PlayedViewModel()
        {
            this.MediaElements = new ObservableCollection<MediaElemntViewModel>();
            this.SubtitlesStreams = new ObservableCollection<VlcFlux>();
            this.AudioStreams = new ObservableCollection<VlcFlux>();
            this.VideoStreams = new ObservableCollection<VlcFlux>();
            this.MediaElements.CollectionChanged += MediaElements_CollectionChanged;
            this.IsConnected = false;
        }
        private SynchronizationContext uiContext = SynchronizationContext.Current;
        private object state = new Object();
        private string remoteDir;
        private bool isSortingByName = true;
        private bool isSortingAscendant = true;
        private string filter = "";
        private string vlcVersion;
        private string currentlyPlaying;
        private VlcState vlcState;
        private TimeSpan currentPosition;
        private TimeSpan videoLength;
        private decimal subtitlesDelay;

        private VlcWebControler com = null;
        private Timer timer = null;

        private bool isConnected;
        private SortOrder SortOrder = SortOrder.LastDateDescending;



        public ObservableCollection<MediaElemntViewModel> MediaElements { get; private set; }
        public ObservableCollection<VlcFlux> AudioStreams { get; private set; }
        public ObservableCollection<VlcFlux> VideoStreams { get; private set; }
        public ObservableCollection<VlcFlux> SubtitlesStreams { get; private set; }

        public object GroupedItems
        {
            get
            {
                if (isSortingByName)
                {
                    if (IsSortingAscendant)
                    {
                        return from e in this.MediaElements where e.Name.ToUpper().Contains(Filter.Trim().ToUpper()) group e by e.NameForGrouping into grp orderby grp.Key select grp;
                    }
                    else
                    {
                        return from e in this.MediaElements where e.Name.ToUpper().Contains(Filter.Trim().ToUpper()) group e by e.NameForGrouping into grp orderby grp.Key descending select grp;
                    }
                }
                else
                {
                    if (IsSortingAscendant)
                    {
                        return from e in this.MediaElements where e.Name.ToUpper().Contains(Filter.Trim().ToUpper()) group e by e.LastDateForGrouping into grp orderby grp.Key select grp;
                    }
                    else
                    {
                        return from e in this.MediaElements where e.Name.ToUpper().Contains(Filter.Trim().ToUpper()) group e by e.LastDateForGrouping into grp orderby grp.Key descending select grp;
                    }
                }
            }
        }


        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                if (value != isConnected)
                {
                    isConnected = value;
                    NotifyPropertyChanged("IsConnected");
                    NotifyPropertyChanged("Status");
                }
            }
        }

        public bool IsSortingByName
        {
            get { return isSortingByName; }
            set
            {
                if (value != isSortingByName)
                {
                    isSortingByName = value;
                    NotifyPropertyChanged("IsSortingByName");
                    this.NotifyPropertyChanged("GroupedItems");
                }
            }
        }
        public decimal SubtitlesDelay
        {
            get { return subtitlesDelay; }
            set
            {
                if (value != subtitlesDelay)
                {
                    subtitlesDelay = value;
                    NotifyPropertyChanged("SubtitlesDelay");
                }
            }
        }
        public bool IsSortingAscendant
        {
            get { return isSortingAscendant; }
            set
            {
                if (value != isSortingAscendant)
                {
                    isSortingAscendant = value;
                    NotifyPropertyChanged("IsSortingAscendant");
                    this.NotifyPropertyChanged("GroupedItems");
                }
            }
        }

        public string VlcVersion
        {
            get { return vlcVersion; }
            set
            {
                if (value != vlcVersion)
                {
                    vlcVersion = value;
                    NotifyPropertyChanged("VlcVersion");
                    NotifyPropertyChanged("Status");
                }
            }
        }
        public string CurrentlyPlaying
        {
            get { return currentlyPlaying; }
            set
            {
                if (value != currentlyPlaying)
                {
                    currentlyPlaying = value;
                    NotifyPropertyChanged("CurrentlyPlaying");
                    NotifyPropertyChanged("Status");
                }
            }
        }


        public string Status
        {
            get
            {
                if (this.isConnected)
                {
                    if (this.CurrentlyPlaying != "")
                    {
                        return this.CurrentlyPlaying + " on " + this.VlcVersion;
                    }
                    else
                    {
                        return "Connected to " + this.VlcVersion;
                    }
                }
                else
                {
                    return "Not connected";
                }
            }
        }

        public string Filter
        {
            get { return filter; }
            set
            {
                if (value != filter)
                {
                    filter = value;
                    NotifyPropertyChanged("Filter");
                    this.NotifyPropertyChanged("GroupedItems");
                }
            }
        }

        public VlcState VlcState
        {
            get { return vlcState; }
            set
            {
                if (value != vlcState)
                {
                    vlcState = value;
                    NotifyPropertyChanged("VlcState");
                }
            }
        }

        public TimeSpan CurrentPosition
        {
            get { return currentPosition; }
            set
            {
                if (value != currentPosition)
                {
                    currentPosition = value;
                    NotifyPropertyChanged("CurrentPosition");
                    NotifyPropertyChanged("Progress");
                }
            }
        }
        public TimeSpan VideoLength
        {
            get { return videoLength; }
            set
            {
                if (value != videoLength)
                {
                    videoLength = value;
                    NotifyPropertyChanged("VideoLength");
                    NotifyPropertyChanged("Progress");
                }
            }
        }

        public int Progress
        {
            get
            {
                if (VideoLength.TotalSeconds == 0)
                {
                    return 0;
                }
                return (int)(this.CurrentPosition.TotalSeconds * 100 / this.VideoLength.TotalSeconds);
            }
        }

        

        public async Task<bool> ConnectAndLoad(string url, string password, string remoteDir)
        {
            if (timer != null)
            {
                timer.Cancel();
            }
            com = new VlcWebControler(url, password);

            try
            {
                this.IsConnected = await com.TestConnexion();
                if (this.IsConnected)
                {
                    this.timer = new Timer(this.TimerCallBack, state, 250, 250);
                    this.remoteDir = remoteDir;
                }
            }
            catch
            {
                this.IsConnected = false;

            }
            return this.IsConnected;

        }

        public async Task<bool> RefreshList()
        {
            var res = false;
            try
            {
                var list = await com.GetMediaList(this.remoteDir, this.SortOrder);
                //this.MediaElements.Clear();
                foreach (var item in list)
                {
                    this.MediaElements.Add(new MediaElemntViewModel(item));
                }
                res = true;
            }
            catch (Exception)
            {
                res = false;
                this.IsConnected = false;
            }
            return res;
        }

        private async void TimerCallBack(object state)
        {
            try
            {
                var status = await com.GetStatus();
                InvokeIfRequired(uiContext, new SendOrPostCallback((s) =>
                {
                    this.IsConnected = true;
                    this.CurrentlyPlaying = status.CurrentllyPlaying;
                    this.VlcVersion = status.VlcVersion;
                    this.VlcState = status.State;
                    this.VideoLength = status.Duration;
                    this.CurrentPosition = status.CurrentPossiton;
                    this.SubtitlesDelay = (decimal)status.SubsDelay;
                    this.AudioStreams.Clear();
                    this.VideoStreams.Clear();
                    this.SubtitlesStreams.Clear();
                    for (int i = 0; i < status.Fluxs.Count; i++)
                    {
                        if (status.Fluxs[i].FluxType == FluxType.Audio)
                        {
                            this.AudioStreams.Add(status.Fluxs[i]);
                        }
                        else if (status.Fluxs[i].FluxType == FluxType.Video)
                        {
                            this.VideoStreams.Add(status.Fluxs[i]);
                        } if (status.Fluxs[i].FluxType == FluxType.SubTitles)
                        {
                            this.SubtitlesStreams.Add(status.Fluxs[i]);
                        }
                    }
                    this.NotifyPropertyChanged("SubtitlesStreams");
                }));
            }
            catch
            {
                InvokeIfRequired(uiContext, new SendOrPostCallback((s) =>
                {
                    this.IsConnected = false;
                    this.VlcState = VlcLib.Status.VlcState.Stopped;
                }));
            }
        }

        private void MediaElements_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.NotifyPropertyChanged("GroupedItems");
        }

        public async void PlayItem(MediaElemntViewModel item)
        {
            await com.PlayFile(new Media.VlcMediaItem()
            {
                FileName = item.FileUri
            });
        }

        public void Pause()
        {
            com.Pause();
        }

        public void PlayResume()
        {
            if (vlcState == VlcLib.Status.VlcState.Stopped)
            {
                com.PlatLast();
            }
            else
            {
                com.PlayResume();
            }
        }

        public void Stop()
        {
            com.Stop();
        }


        public void ToggleFullScreen()
        {
            com.ToggleFullScreen();
        }

        public void AdvanceSeconds(int p)
        {
            com.AdvanceSeconds(p);
        }

        public void AdjustSubtitles(int ms)
        {
            com.SetSubsDelay(this.subtitlesDelay + ((decimal) (ms / 1000.0)));
        }
    }
}
