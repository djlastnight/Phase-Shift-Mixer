namespace PsMixer.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NAudio.Wave;
    using NVorbis.NAudioSupport;
    using PsMixer.Enums;

    public class NaudioChannel : IPsChannel, IDisposable
    {
        private const float PeakLevelNormalizationValue = 0.81f;

        private readonly AudioDriver audioDriver;

        private List<float> peakBuffer = new List<float>();

        private int peakBufferMaxCount;

        private PeakLevelUpdateSpeed peakLevelUpdateSpeed;

        private float volume = 1.0f;

        private bool isMuted;

        private VorbisWaveReader vorbisStream;

        public NaudioChannel(ChannelFriendlyName name, string pathToFile, AudioDriver driver)
        {
            this.ChannelFriendlyName = name;
            this.vorbisStream = new VorbisWaveReader(pathToFile);
            this.WaveChannel = new WaveChannel32(this.vorbisStream);
            this.WaveChannel.PadWithZeroes = true;
            this.audioDriver = driver;
            this.PeakLevelUpdateSpeed = Enums.PeakLevelUpdateSpeed.Double;
            this.IsPeakLevelEnabled = true;
            this.WaveChannel.Sample += this.WaveChannel_Sample;
        }

        public event PeakLevelHandler PeakLevel;

        public ChannelFriendlyName ChannelFriendlyName { get; private set; }

        public bool IsPeakLevelEnabled { get; set; }

        public PeakLevelUpdateSpeed PeakLevelUpdateSpeed
        {
            get
            {
                return this.peakLevelUpdateSpeed;
            }

            set
            {
                if (this.audioDriver != AudioDriver.WaveOut)
                {
                    this.peakBufferMaxCount = this.WaveChannel.WaveFormat.SampleRate / (12 * (int)value);
                }
                else
                {
                    this.peakBufferMaxCount = this.WaveChannel.WaveFormat.SampleRate / 6;
                }

                this.peakLevelUpdateSpeed = value;
            }
        }

        public float Volume
        {
            get
            {
                return this.volume;
            }

            set
            {
                this.volume = value;
                if (this.isMuted)
                {
                    return;
                }

                this.WaveChannel.Volume = value;
            }
        }

        public bool IsMuted
        {
            get
            {
                return this.isMuted;
            }

            set
            {
                this.isMuted = value;

                if (this.isMuted)
                {
                    this.WaveChannel.Volume = 0.0f;
                }
                else
                {
                    this.WaveChannel.Volume = this.volume;
                }
            }
        }

        public WaveChannel32 WaveChannel
        {
            get;
            private set;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.IsPeakLevelEnabled = false;
                this.PeakLevel = null;
                this.vorbisStream.Dispose();
                this.WaveChannel.Sample -= this.WaveChannel_Sample;
                this.WaveChannel.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        private void WaveChannel_Sample(object sender, SampleEventArgs e)
        {
            if (this.IsPeakLevelEnabled)
            {
                this.peakBuffer.Add(Math.Max(e.Left, e.Right));

                if (this.peakBuffer.Count >= this.peakBufferMaxCount)
                {
                    var level = this.peakBuffer.Max() * PeakLevelNormalizationValue;
                    this.OnPeakLevel(level);
                    this.peakBuffer.Clear();
                }
            }
        }

        private void OnPeakLevel(float level)
        {
            if (this.PeakLevel != null)
            {
                this.PeakLevel(this, level);
            }
        }
    }
}