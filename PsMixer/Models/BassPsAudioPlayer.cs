namespace PsMixer.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Threading;
    using PsMixer.Enums;
    using Un4seen.Bass;
    using Un4seen.Bass.AddOn.Mix;
    using Un4seen.BassWasapi;

    public class BassPsAudioPlayer : IPsAudioPlayer
    {
        private int mixer;

        private DispatcherTimer timer;

        private List<BassChannel> channels;

        private bool isPeakLevelEnabled;

        private BassWasapiHandler wasapi;

        public BassPsAudioPlayer(AudioDriver driver = Enums.AudioDriver.WaveOut, bool isPeakLevelEnabled = true)
        {
            this.AudioDriver = driver;
            
            BassNet.Registration("cdg.vito@gmail.com", "2X292418152222");
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_OGG_PRESCAN, true);
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 32);
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, 17);

            var handle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            Bass.BASS_Init(-1, 48000, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);

            this.timer = new DispatcherTimer(DispatcherPriority.DataBind);
            this.timer.Interval = TimeSpan.FromMilliseconds(500);
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
            get { return this.channels.Select(x => x.ChannelFriendlyName); }
        }

        public TimeSpan CurrentTime
        {
            get
            {
                if (this.channels == null ||
                    this.channels.Count == 0)
                {
                    return TimeSpan.Zero;
                }

                int chan0 = this.channels[0].Channel;
                long position = Bass.BASS_ChannelGetPosition(chan0);
                return TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(chan0, position));
            }
        }

        public TimeSpan TotalTime
        {
            get
            {
                if (this.channels == null ||
                    this.channels.Count == 0)
                {
                    return TimeSpan.Zero;
                }

                int chan0 = this.channels[0].Channel;
                long totalLength = Bass.BASS_ChannelGetLength(chan0);
                return TimeSpan.FromSeconds(Bass.BASS_ChannelBytes2Seconds(chan0, totalLength));
            }
        }

        public double Progress
        {
            get
            {
                if (this.TotalTime == TimeSpan.Zero)
                {
                    return 0.0;
                }

                return this.CurrentTime.TotalMilliseconds / this.TotalTime.TotalMilliseconds;
            }

            set
            {
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

            if (song.Folder == null)
            {
                throw new InvalidDataException("song.Folder can not be null");
            }

            if (!Directory.Exists(song.Folder))
            {
                throw new DirectoryNotFoundException(song.Folder);
            }

            this.Stop();

            this.channels = this.CreateChannels(song.Folder);

            // creating mixer
            var channelInfo = Bass.BASS_ChannelGetInfo(this.channels[0].Channel);
            this.mixer = BassMix.BASS_Mixer_StreamCreate(
                channelInfo.freq, 2, BASSFlag.BASS_MIXER_END);

            foreach (var channel in this.channels)
            {
                BassMix.BASS_Mixer_StreamAddChannel(
                    this.mixer, channel.Channel, BASSFlag.BASS_MIXER_MATRIX);

                channel.PeakLevel += this.OnChannelPeakLevel;
            }

            switch (this.AudioDriver)
            {
                case AudioDriver.Wasapi:
                    {
                        BASS_WASAPI_DEVICEINFO defaultDeviceInfo = null;
                        var deviceInfos = Un4seen.BassWasapi.BassWasapi.BASS_WASAPI_GetDeviceInfos();

                        foreach (var deviceInfo in deviceInfos)
                        {
                            if (deviceInfo.IsDefault &&
                                deviceInfo.type == BASSWASAPIDeviceType.BASS_WASAPI_TYPE_SPEAKERS &&
                                deviceInfo.IsEnabled &&
                                !deviceInfo.IsInput)
                            {
                                defaultDeviceInfo = deviceInfo;
                                break;
                            }
                        }

                        if (defaultDeviceInfo == null)
                        {
                            throw new Exception("Failed to find the default WASAPI device!");
                        }

                        this.wasapi = new BassWasapiHandler(
                            -1, true, defaultDeviceInfo.mixfreq, 2, 0.5f, 0f);

                        this.wasapi.AddOutputSource(this.mixer, BASSFlag.BASS_DEFAULT);
                        this.wasapi.Init();
                        this.wasapi.Start();
                    }

                    break;
                case AudioDriver.Asio:
                    {
                        throw new InvalidOperationException(
                            "Bass ASIO is shareware! Use NAudio's ASIO instead!");
                    }

                case AudioDriver.WaveOut:
                    {
                        Bass.BASS_ChannelPlay(this.mixer, true);
                    }

                    break;
                default:
                    break;
            }

            this.timer.Start();
        }

        public void Pause()
        {
            Bass.BASS_ChannelPause(this.mixer);
        }

        public void Resume()
        {
            Bass.BASS_ChannelPlay(this.mixer, false);
        }

        public void Stop()
        {
            Bass.BASS_ChannelStop(this.mixer);
            this.timer.Stop();
        }

        public void Rewind(TimeSpan span)
        {
            if (this.channels == null)
            {
                return;
            }

            foreach (var channel in this.channels)
            {
                Bass.BASS_ChannelLock(channel.Channel, true);
                var currentPosition = Bass.BASS_ChannelGetPosition(channel.Channel);
                var currentSeconds = Bass.BASS_ChannelBytes2Seconds(channel.Channel, currentPosition);
                Bass.BASS_ChannelSetPosition(channel.Channel, currentSeconds + span.TotalSeconds);
                Bass.BASS_ChannelLock(channel.Channel, false);
            }
        }

        public void SetChannelVolume(Enums.ChannelFriendlyName name, float newVolume)
        {
            if (this.channels == null)
            {
                return;
            }

            var channel = this.channels.FirstOrDefault(
                x => x.ChannelFriendlyName == name);

            if (channel != null)
            {
                channel.Volume = newVolume;
            }
        }

        public void MuteChannel(Enums.ChannelFriendlyName channelFriendlyName, bool isMuted)
        {
            if (this.channels == null)
            {
                return;
            }

            var channel = this.channels.FirstOrDefault(
                x => x.ChannelFriendlyName == channelFriendlyName);

            if (channel != null)
            {
                channel.IsMuted = isMuted;
            }
        }

        private List<BassChannel> CreateChannels(string folder)
        {
            var channels = new List<BassChannel>();

            var stemInfos = Helpers.PsStemsRetriever.GetValidStems(folder);

            foreach (var stemInfo in stemInfos)
            {
                channels.Add(new BassChannel(stemInfo.ChannelName, stemInfo.FilePath, this.AudioDriver));
            }

            return channels;
        }

        private void OnChannelPeakLevel(object sender, float level)
        {
            if (this.ChannelPeak != null)
            {
                this.ChannelPeak(sender, level);
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (this.CurrentTime >= this.TotalTime)
            {
                this.OnSongFinished();
            }

            var reportArgs = new PlayerReportEventArgs(
                this.Progress,
                this.CurrentTime,
                this.TotalTime);

            this.OnReportProgress(reportArgs);
        }

        private void OnReportProgress(PlayerReportEventArgs e)
        {
            if (this.PlayerReport != null)
            {
                this.PlayerReport(this, e);
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
