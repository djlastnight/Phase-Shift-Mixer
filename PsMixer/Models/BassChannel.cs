namespace PsMixer.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using PsMixer.Enums;
    using Un4seen.Bass;

    public class BassChannel : IPsChannel, IDisposable
    {
        private const float PeakLevelNormalizationValue = 0.81f;

        private AudioDriver audioDriver;

        private int channel;

        private GCHandle pinnedHandle;

        private DSPPROC dpsCallback;

        private float volume;

        private bool isMuted;

        private List<float> peakBuffer;

        private PeakLevelUpdateSpeed peakLevelUpdateSpeed;

        public BassChannel(ChannelFriendlyName name, string pathToFile, AudioDriver driver)
        {
            this.audioDriver = driver;
            this.peakBuffer = new List<float>();

            var bytes = File.ReadAllBytes(pathToFile);
            this.pinnedHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            this.channel = Bass.BASS_StreamCreateFile(
                pathToFile,
                0,
                bytes.LongLength,
                BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT);

            if (this.channel == 0)
            {
                throw new InvalidOperationException(
                    "Failed to create bass channel from the following file: " + pathToFile);
            }

            this.dpsCallback = new DSPPROC(this.OnChannelDSP);

            Bass.BASS_ChannelSetDSP(this.channel, this.dpsCallback, IntPtr.Zero, 0);

            this.IsPeakLevelEnabled = true;

            this.PeakLevelUpdateSpeed = Enums.PeakLevelUpdateSpeed.Double;

            this.ChannelFriendlyName = name;
        }

        public event PeakLevelHandler PeakLevel;

        public int Channel
        {
            get
            {
                return this.channel;
            }
        }

        public ChannelFriendlyName ChannelFriendlyName
        {
            get;
            private set;
        }

        public bool IsPeakLevelEnabled
        {
            get;
            set;
        }

        public PeakLevelUpdateSpeed PeakLevelUpdateSpeed
        {
            get
            {
                return this.peakLevelUpdateSpeed;
            }

            set
            {
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

                Bass.BASS_ChannelSetAttribute(
                    this.channel,
                    BASSAttribute.BASS_ATTRIB_VOL,
                    this.volume);
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
                    Bass.BASS_ChannelSetAttribute(
                        this.channel,
                        BASSAttribute.BASS_ATTRIB_VOL,
                        0.0f);
                }
                else
                {
                    Bass.BASS_ChannelSetAttribute(
                        this.channel,
                        BASSAttribute.BASS_ATTRIB_VOL,
                        this.volume);
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.pinnedHandle != null &&
                    this.pinnedHandle.IsAllocated)
                {
                    this.pinnedHandle.Free();
                }

                Bass.BASS_StreamFree(this.channel);

                GC.SuppressFinalize(this);
            }
        }

        private void OnChannelDSP(int handle, int channel, IntPtr buffer, int length, IntPtr user)
        {
            if (!this.IsPeakLevelEnabled)
            {
                return;
            }

            int floatLength = length / 4;
            float[] data = new float[floatLength];
            Marshal.Copy(buffer, data, 0, floatLength);

            float maxLevel = data.Max();

            this.peakBuffer.Add(maxLevel);

            if (this.peakBuffer.Count >= (int)this.peakLevelUpdateSpeed)
            {
                var vol = this.isMuted ? 0.0f : this.volume;
                var level = this.peakBuffer.Max() * PeakLevelNormalizationValue * vol;
                level = level < 0.0f ? 0.0f : level;
                level = level > 1.00f ? 1.00f : level;

                this.OnPeakLevel(level);
                this.peakBuffer.Clear();
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
