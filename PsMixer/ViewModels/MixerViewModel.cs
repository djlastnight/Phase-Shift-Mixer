namespace PsMixer.ViewModels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media.Animation;
    using PsMixer.Commands;
    using PsMixer.Enums;
    using PsMixer.Models;

    public class MixerViewModel : INotifyPropertyChanged
    {
        private readonly TimeSpan rewindSpan;

        private IPsAudioPlayer player;

        private string songFilter;

        private IEnumerable<MixerLane> lanes;

        private IList<PsSong> songs;

        private PsSong selectedSong;

        private PsSong playingSong;

        private PsPlayerState playState;

        private double playerProgress;

        private TimeSpan playerCurrentTime;

        private TimeSpan playerRemainingTime;

        private ICommand playCommand;

        private ICommand pauseCommand;

        private ICommand resumeCommand;

        private ICommand stopCommand;

        private ICommand previousTrackCommand;

        private ICommand nextTrackCommand;

        private ICommand rewindBackCommand;

        private ICommand rewindForwardCommand;

        private ICommand addFolderCommand;

        private ICommand resetSlidersCommand;

        private ICommand deleteSelectedSongsCommand;

        public MixerViewModel()
        {
            this.Songs = new List<PsSong>();
            this.Lanes = this.CreateDefaultLanes();
            this.player = new PsAudioPlayer();
            this.player.ChannelPeak += this.OnChannelPeak;
            this.player.PlayerReport += this.OnPlayerReport;
            this.player.SongFinished += this.OnSongFinished;
            this.rewindSpan = TimeSpan.FromSeconds(5);
            this.songFilter = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<MixerLane> Lanes
        {
            get
            {
                return this.lanes;
            }

            set
            {
                this.lanes = value;
                this.OnPropertyChanged("Lanes");
            }
        }

        public PsSong SelectedSong
        {
            get
            {
                return this.selectedSong;
            }

            set
            {
                this.selectedSong = value;
                this.OnPropertyChanged("SelectedSong");
            }
        }

        public PsSong PlayingSong
        {
            get
            {
                return this.playingSong;
            }

            set
            {
                this.playingSong = value;
                this.OnPropertyChanged("PlayingSong");
            }
        }

        public IList<PsSong> Songs
        {
            get
            {
                return this.songs;
            }

            set
            {
                this.songs = value;
                this.OnPropertyChanged("Songs");
            }
        }

        public PsPlayerState PlayState
        {
            get
            {
                return this.playState;
            }

            set
            {
                this.playState = value;
                this.OnPropertyChanged("PlayState");
            }
        }

        public double PlayerProgress
        {
            get
            {
                return this.playerProgress;
            }

            set
            {
                this.playerProgress = value;
                this.player.Progress = value;
                this.OnPropertyChanged("PlayerProgress");
            }
        }

        public TimeSpan PlayerCurrentTime
        {
            get
            {
                return this.playerCurrentTime;
            }

            set
            {
                this.playerCurrentTime = value;
                this.OnPropertyChanged("PlayerCurrentTime");
            }
        }

        public TimeSpan PlayerRemainingTime
        {
            get
            {
                return this.playerRemainingTime;
            }

            set
            {
                this.playerRemainingTime = value;
                this.OnPropertyChanged("PlayerRemainingTime");
            }
        }

        public string SongFilter
        {
            get
            {
                return this.songFilter;
            }

            set
            {
                this.songFilter = value;
                this.OnPropertyChanged("SongFilter");
                this.ApplySongFilter();
            }
        }

        public IPsAudioPlayer Player
        {
            get
            {
                return this.player;
            }
        }

        public ICommand PlayCommand
        {
            get
            {
                if (this.playCommand == null)
                {
                    this.playCommand = new RelayCommand(
                        this.HandlePlayCommand);
                }

                return this.playCommand;
            }
        }

        public ICommand PauseCommand
        {
            get
            {
                if (this.pauseCommand == null)
                {
                    this.pauseCommand = new RelayCommand(
                        this.HandlePauseCommand);
                }

                return this.pauseCommand;
            }
        }

        public ICommand ResumeCommand
        {
            get
            {
                if (this.resumeCommand == null)
                {
                    this.resumeCommand = new RelayCommand(
                        this.HandleResumeCommand);
                }

                return this.resumeCommand;
            }
        }

        public ICommand StopCommand
        {
            get
            {
                if (this.stopCommand == null)
                {
                    this.stopCommand = new RelayCommand(
                        this.HandleStopCommand);
                }

                return this.stopCommand;
            }
        }

        public ICommand PreviousTrackCommand
        {
            get
            {
                if (this.previousTrackCommand == null)
                {
                    this.previousTrackCommand = new RelayCommand(
                        this.HandlePreviousTrackCommand);
                }

                return this.previousTrackCommand;
            }
        }

        public ICommand NextTrackCommand
        {
            get
            {
                if (this.nextTrackCommand == null)
                {
                    this.nextTrackCommand = new RelayCommand(
                        this.HandleNextTrackCommand);
                }

                return this.nextTrackCommand;
            }
        }

        public ICommand RewindBackCommand
        {
            get
            {
                if (this.rewindBackCommand == null)
                {
                    this.rewindBackCommand = new RelayCommand(
                        this.HandleRewindBackCommand);
                }

                return this.rewindBackCommand;
            }
        }

        public ICommand RewindForwardCommand
        {
            get
            {
                if (this.rewindForwardCommand == null)
                {
                    this.rewindForwardCommand = new RelayCommand(
                        this.HandleRewindForwardCommand);
                }

                return this.rewindForwardCommand;
            }
        }

        public ICommand AddFolderCommand
        {
            get
            {
                if (this.addFolderCommand == null)
                {
                    this.addFolderCommand = new RelayCommand(
                        this.HandleAddFolderCommand);
                }

                return this.addFolderCommand;
            }
        }

        public ICommand ResetSlidersCommand
        {
            get
            {
                if (this.resetSlidersCommand == null)
                {
                    this.resetSlidersCommand = new RelayCommand(
                        this.HandleResetSlidersCommand);
                }

                return this.resetSlidersCommand;
            }
        }

        public ICommand DeleteSelectedSongsCommand
        {
            get
            {
                if (this.deleteSelectedSongsCommand == null)
                {
                    this.deleteSelectedSongsCommand = new RelayCommand(
                        this.HandleDeleteSongsCommand);
                }

                return this.deleteSelectedSongsCommand;
            }
        }

        public void AddFolder(string folder)
        {
            var scanner = new SongScanner(folder);
            scanner.ScanCompleted += this.OnSongScanCompleted;
            scanner.ScanAsync();
        }

        private void OnSongScanCompleted(object sender, SongScannerEventArgs e)
        {
            if (e.Cancelled)
            {
                return;
            }

            if (e.Exception != null)
            {
                MessageBox.Show(
                    e.Exception.Message,
                    "Phase Shift Mixer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            if (e.SongFolders == null || e.SongFolders.Count() == 0)
            {
                MessageBox.Show(
                    string.Format("No Phase Shift songs found at '{0}'", e.RootFolder),
                    "Phase Shift Mixer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var newSongs = new HashSet<PsSong>();

            foreach (var songFolder in e.SongFolders)
            {
                var newSong = new PsSong(songFolder);
                this.Songs.Add(newSong);
                newSongs.Add(newSong);
            }

            this.OnPropertyChanged("Songs");
            var collectionView = CollectionViewSource.GetDefaultView(this.Songs);
            collectionView.Refresh();

            // caching songs
            Thread cacheThread = new Thread(
                new ParameterizedThreadStart(this.OnSongCaching));

            cacheThread.IsBackground = true;
            cacheThread.Priority = ThreadPriority.Lowest;
            cacheThread.Start(newSongs);

            MessageBox.Show(
                string.Format("{0} songs added to playlist!", newSongs.Count),
                "Phase Shift Mixer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void OnSongCaching(object obj)
        {
            var songs = (HashSet<PsSong>)obj;

            foreach (var song in songs)
            {
                var album = song.AlbumName;
                var genre = song.Genre;
                var year = song.Year;
                Thread.Sleep(1);
            }
        }

        private void ApplySongFilter()
        {
            var songCollection = CollectionViewSource.GetDefaultView(this.Songs) as ListCollectionView;

            if (songCollection == null)
            {
                return;
            }

            songCollection.Filter = this.FilterCallback;
        }

        private bool FilterCallback(object obj)
        {
            var song = (PsSong)obj;

            bool matchArtist = song.ArtistName.IndexOf(this.songFilter, StringComparison.OrdinalIgnoreCase) != -1;
            if (matchArtist)
            {
                return true;
            }

            bool matchSong = song.SongName.IndexOf(this.songFilter, StringComparison.OrdinalIgnoreCase) != -1;

            if (matchSong)
            {
                return true;
            }

            bool matchFolder = song.Folder.IndexOf(this.songFilter, StringComparison.OrdinalIgnoreCase) != -1;

            if (matchFolder)
            {
                return true;
            }

            bool matchAlbum = song.AlbumName.IndexOf(this.songFilter, StringComparison.OrdinalIgnoreCase) != -1;

            if (matchAlbum)
            {
                return true;
            }

            bool matchGenre = song.Genre.IndexOf(this.songFilter, StringComparison.OrdinalIgnoreCase) != -1;

            if (matchGenre)
            {
                return true;
            }

            bool matchYear = song.Year.IndexOf(this.songFilter, StringComparison.OrdinalIgnoreCase) != -1;

            if (matchYear)
            {
                return true;
            }

            return false;
        }

        private IEnumerable<MixerLane> CreateDefaultLanes()
        {
            var lanes = new List<MixerLane>();
            var channelNames = Enum.GetValues(typeof(Enums.ChannelFriendlyName));

            foreach (ChannelFriendlyName channelName in channelNames)
            {
                if (channelName == ChannelFriendlyName.None)
                {
                    continue;
                }

                var lane = new MixerLane() { ChannelName = channelName };
                lane.VolumeChanged += this.OnVolumeChanged;
                lane.MuteChanged += this.OnMuteChanged;

                lanes.Add(lane);
            }

            return lanes;
        }

        private void OnVolumeChanged(object sender, EventArgs e)
        {
            var lane = sender as MixerLane;
            var newVolume = (e as VolumeChangedEventArgs).NewVolume;
            this.player.SetChannelVolume(lane.ChannelName, newVolume);
        }

        private void OnMuteChanged(object sender, EventArgs e)
        {
            var lane = sender as MixerLane;
            this.player.MuteChannel(lane.ChannelName, lane.IsMuted);
        }

        private void OnChannelPeak(object sender, float level)
        {
            this.lanes.First().Dispatcher.BeginInvoke((Action)delegate
            {
                var channel = sender as IPsChannel;

                if (channel != null)
                {
                    var lane = this.lanes.First(
                        x => x.ChannelName == channel.ChannelFriendlyName);

                    lane.Level = level;
                }
            });
        }

        private void OnPlayerReport(object sender, PlayerReportEventArgs e)
        {
            this.PlayerProgress = e.Progress;
            this.PlayerCurrentTime = e.CurrentTime;
            this.PlayerRemainingTime = e.RemainingTime;
        }

        private void OnSongFinished(object sender, EventArgs e)
        {
            this.HandleStopCommand(null);
        }

        private void HandleAddFolderCommand(object parameter)
        {
            var directoryPicker = new System.Windows.Forms.FolderBrowserDialog();
            directoryPicker.ShowNewFolderButton = false;

            directoryPicker.Description = "Please choose single or root Phase Shift song(s) directory.\r\n" +
                "Download songs from: http://www.tinyurl.com/ghseries" +
                "\r\n" +
                "http://www.tinyurl.com/rbseries" +
                " or http://www.pksage.com/songlist/?sc=C3";

            directoryPicker.RootFolder = Environment.SpecialFolder.MyComputer;
            directoryPicker.SelectedPath = "C:";
            var result = directoryPicker.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.AddFolder(directoryPicker.SelectedPath);
            }
        }

        private void HandlePlayCommand(object parameter)
        {
            if (this.SelectedSong == null)
            {
                if (this.Songs == null || this.Songs.Count == 0)
                {
                    this.HandleAddFolderCommand(null);
                    return;
                }
                else
                {
                    this.SelectedSong = this.Songs[0];
                }
            }

            try
            {
                this.player.Play(this.SelectedSong);
                this.PlayState = PsPlayerState.Playing;
                this.PlayingSong = this.SelectedSong;

                foreach (var lane in this.Lanes)
                {
                    lane.IsActive = false;
                }

                foreach (var channelName in this.player.ChannelNames)
                {
                    var lane = this.Lanes.First(x => x.ChannelName == channelName);
                    this.player.SetChannelVolume(lane.ChannelName, lane.Volume);
                    this.player.MuteChannel(lane.ChannelName, lane.IsMuted);
                    lane.IsActive = true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void HandlePauseCommand(object parameter)
        {
            this.player.Pause();
            this.PlayState = PsPlayerState.Paused;
        }

        private void HandleResumeCommand(object parameter)
        {
            this.player.Resume();
            this.PlayState = PsPlayerState.Playing;
        }

        private void HandleStopCommand(object parameter)
        {
            this.player.Stop();
            this.PlayState = PsPlayerState.Stopped;
            this.PlayingSong = null;
        }

        private void HandlePreviousTrackCommand(object parameter)
        {
            var collection = CollectionViewSource.GetDefaultView(this.Songs);
            if (collection != null)
            {
                var success = collection.MoveCurrentToPrevious();
                if (success)
                {
                    this.HandlePlayCommand(null);
                }
            }
        }

        private void HandleNextTrackCommand(object parameter)
        {
            var collection = CollectionViewSource.GetDefaultView(this.Songs);
            if (collection != null)
            {
                var success = collection.MoveCurrentToNext();
                if (success)
                {
                    this.HandlePlayCommand(null);
                }
            }
        }

        private void HandleRewindBackCommand(object parameter)
        {
            if (this.player != null)
            {
                this.player.Rewind(-this.rewindSpan);
            }
        }

        private void HandleRewindForwardCommand(object parameter)
        {
            if (this.player != null)
            {
                this.player.Rewind(this.rewindSpan);
            }
        }

        private void HandleResetSlidersCommand(object parameter)
        {
            var storyBoard = new Storyboard();

            foreach (var lane in this.Lanes)
            {
                if (lane.Volume == PsAudioPlayer.DefaultVolume)
                {
                    continue;
                }

                var animation = new SingleAnimation();
                animation.Duration = TimeSpan.FromSeconds(1);
                animation.To = (float)PsAudioPlayer.DefaultVolume;
                storyBoard.Children.Add(animation);
                Storyboard.SetTarget(animation, lane);
                Storyboard.SetTargetProperty(
                    animation,
                    new PropertyPath(MixerLane.VolumeProperty));
            }

            storyBoard.Completed += (sender, e) =>
            {
                storyBoard.Remove();
                foreach (var lane in this.Lanes)
                {
                    if (lane.Volume != PsAudioPlayer.DefaultVolume)
                    {
                        lane.Volume = (float)PsAudioPlayer.DefaultVolume;
                    }
                }
            };

            storyBoard.Begin();
        }

        private void HandleDeleteSongsCommand(object parameter)
        {
            var passedSongs = parameter as IList;

            if (passedSongs != null)
            {
                var songsToDelete = new PsSong[passedSongs.Count];
                passedSongs.CopyTo(songsToDelete, 0);

                foreach (var songToDelete in songsToDelete)
                {
                    this.Songs.Remove(songToDelete);
                }

                this.OnPropertyChanged("Songs");
                var viewCollection = CollectionViewSource.GetDefaultView(this.Songs);
                viewCollection.Refresh();
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(
                    this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}