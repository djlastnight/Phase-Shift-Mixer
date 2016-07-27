namespace PsMixer.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows.Threading;
    using NAudio.CoreAudioApi;
    using NAudio.Wave;
    using PsMixer.Enums;
    using PsMixer.Helpers;

    public class PsAudioPlayer : IPsAudioPlayer
    {
        public const double VolumeMinValue = 0.0;

        public const double VolumeMaxValue = 2.0;

        public const double DefaultVolume = 1.0;

        public const double VolumeStep = 0.1;

        private const int Latency = 50;

        private const double TimerIntervalInMilliseconds = 333;

        private List<NaudioChannel> channels;

        private DispatcherTimer timer;

        private TimeSpan totalTime;

        private bool isPeakLevelEnabled;

        public PsAudioPlayer(AudioDriver driver = Enums.AudioDriver.Wasapi, bool isPeakLevelEnabled = true)
        {
            this.AudioDriver = driver;
            this.isPeakLevelEnabled = isPeakLevelEnabled;
            this.timer = new DispatcherTimer(DispatcherPriority.DataBind);
            this.timer.Interval = TimeSpan.FromMilliseconds(TimerIntervalInMilliseconds);
            this.timer.Tick += this.OnTimerTick;
        }

        public event EventHandler<PlayerReportEventArgs> PlayerReport;

        public event EventHandler SongFinished;

        public event PeakLevelHandler ChannelPeak;

        public AudioDriver AudioDriver
        {
            get;
            set;
        }

        public IEnumerable<ChannelFriendlyName> ChannelNames
        {
            get
            {
                return this.channels.Select(x => x.ChannelFriendlyName);
            }
        }

        public TimeSpan CurrentTime
        {
            get
            {
                if (PlaybackWindow.Player == null)
                {
                    return TimeSpan.Zero;
                }
                else if (PlaybackWindow.Player.PlaybackState == PlaybackState.Stopped)
                {
                    return TimeSpan.Zero;
                }

                return this.channels[0].WaveChannel.CurrentTime;
            }
        }

        public TimeSpan TotalTime
        {
            get
            {
                if (PlaybackWindow.Player == null)
                {
                    return TimeSpan.Zero;
                }
                else if (PlaybackWindow.Player.PlaybackState == PlaybackState.Stopped)
                {
                    return TimeSpan.Zero;
                }

                return this.totalTime;
            }
        }

        public double Progress
        {
            get
            {
                if (PlaybackWindow.Player == null)
                {
                    return 0.0f;
                }

                if (PlaybackWindow.Player.PlaybackState == PlaybackState.Stopped)
                {
                    return 0.0f;
                }

                double progress =
                    this.CurrentTime.TotalMilliseconds / this.TotalTime.TotalMilliseconds;

                return progress;
            }

            set
            {
                if (PlaybackWindow.Player == null || PlaybackWindow.Player.PlaybackState == PlaybackState.Stopped)
                {
                    return;
                }

                var jumpTime = TimeSpan.FromMilliseconds(value * this.TotalTime.TotalMilliseconds);
                var deltaTime = jumpTime - this.CurrentTime;
                this.Rewind(deltaTime);
            }
        }

        public bool IsPeakLevelEnabled
        {
            get
            {
                return this.isPeakLevelEnabled;
            }

            set
            {
                this.isPeakLevelEnabled = value;
                if (this.channels != null)
                {
                    foreach (var channel in this.channels)
                    {
                        channel.IsPeakLevelEnabled = value;
                    }
                }
            }
        }

        public void Play(PsSong song)
        {
            if (song == null)
            {
                throw new ArgumentNullException("song");
            }

            if (!Directory.Exists(song.Folder))
            {
                throw new DirectoryNotFoundException(
                    song.Folder + " folder does not exists!");
            }

            this.Stop();

            PlaybackWindow.Player = this.CreatePlayer(this.AudioDriver);

            var mixer = this.CreateMixer(song);

            PlaybackWindow.Player.Init(mixer);

            var playThread = new Thread(
                new ParameterizedThreadStart(this.PlayCallback));

            playThread.IsBackground = true;
            playThread.Priority = ThreadPriority.Highest;
            playThread.Start();

            this.totalTime = this.channels.Max(x => x.WaveChannel.TotalTime);

            this.timer.Start();

            foreach (var channel in this.channels)
            {
                if (this.isPeakLevelEnabled)
                {
                    channel.PeakLevel += this.OnChannelPeakLevel;
                }
                else
                {
                    channel.IsPeakLevelEnabled = false;
                }
            }
        }

        public void Pause()
        {
            if (PlaybackWindow.Player == null)
            {
                return;
            }

            if (PlaybackWindow.Player.PlaybackState != PlaybackState.Paused)
            {
                PlaybackWindow.Player.Pause();
            }
        }

        public void Resume()
        {
            if (PlaybackWindow.Player != null &&
                PlaybackWindow.Player.PlaybackState == PlaybackState.Paused)
            {
                PlaybackWindow.Player.Play();
            }
        }

        public void Stop()
        {
            this.timer.Stop();

            if (PlaybackWindow.Player != null)
            {
                PlaybackWindow.Player.Stop();
                PlaybackWindow.Player.Dispose();
            }

            if (this.channels != null)
            {
                foreach (var channel in this.channels)
                {
                    channel.Dispose();
                }
            }
        }

        public void Rewind(TimeSpan span)
        {
            if (PlaybackWindow.Player != null &&
                PlaybackWindow.Player.PlaybackState != PlaybackState.Stopped)
            {
                var jumpTime = this.CurrentTime + span;

                if (jumpTime > this.TotalTime ||
                    jumpTime < TimeSpan.Zero)
                {
                    return;
                }

                foreach (var channel in this.channels)
                {
                    channel.WaveChannel.CurrentTime = jumpTime;
                }
            }
        }

        public void SetChannelVolume(ChannelFriendlyName name, float newVolume)
        {
            if (newVolume < VolumeMinValue)
            {
                newVolume = (float)VolumeMinValue;
            }

            if (newVolume > VolumeMaxValue)
            {
                newVolume = (float)VolumeMaxValue;
            }

            if (this.channels != null)
            {
                var targetChannel = this.channels.Find(x => x.ChannelFriendlyName == name);
                if (targetChannel != null)
                {
                    targetChannel.Volume = newVolume;
                }
            }
        }

        public void MuteChannel(ChannelFriendlyName channelFriendlyName, bool isMuted)
        {
            if (this.channels != null)
            {
                var targetChannel = this.channels.Find(
                    x => x.ChannelFriendlyName == channelFriendlyName);

                if (targetChannel != null)
                {
                    targetChannel.IsMuted = isMuted;
                }
            }
        }

        private IWavePlayer CreatePlayer(AudioDriver driver)
        {
            switch (driver)
            {
                case AudioDriver.Wasapi:
                    return new WasapiOut(
                        AudioClientShareMode.Shared, PsAudioPlayer.Latency);
                case AudioDriver.Asio:
                    return new AsioOut();
                case AudioDriver.WaveOut:
                    return new WaveOut();
                default:
                    throw new NotImplementedException(
                        "Not implemened audio driver: " + driver);
            }
        }

        private MixingWaveProvider32 CreateMixer(PsSong song)
        {
            this.channels = new List<NaudioChannel>();

            var mixer = new MixingWaveProvider32();

            var stemInfos = PsStemsRetriever.GetValidStems(song.Folder);

            foreach (var stemInfo in stemInfos)
            {
                var channel = new NaudioChannel(
                    stemInfo.ChannelName,
                    stemInfo.FilePath,
                    this.AudioDriver);

                this.channels.Add(channel);
                mixer.AddInputStream(channel.WaveChannel);
            }

            return mixer;
        }

        private void PlayCallback(object obj)
        {
            PlaybackWindow.Player.Play();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (PlaybackWindow.Player == null)
            {
                return;
            }

            if (PlaybackWindow.Player.PlaybackState == PlaybackState.Playing)
            {
                if (this.CurrentTime >= this.TotalTime)
                {
                    this.OnSongFinished();
                    this.Stop();
                }
            }

            var reportArgs = new PlayerReportEventArgs(
                this.Progress,
                this.CurrentTime,
                this.TotalTime);

            this.OnPlayerReport(reportArgs);
        }

        private void OnPlayerReport(PlayerReportEventArgs e)
        {
            if (this.PlayerReport != null)
            {
                this.PlayerReport(this, e);
            }
        }

        private void OnChannelPeakLevel(object sender, float level)
        {
            if (this.ChannelPeak != null)
            {
                this.ChannelPeak(sender, level);
            }
        }

        private void OnSongFinished()
        {
            if (this.SongFinished != null)
            {
                this.SongFinished(this, EventArgs.Empty);
            }
        }
    }
}