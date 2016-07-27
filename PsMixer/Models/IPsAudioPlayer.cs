namespace PsMixer.Models
{
    using System;
    using System.Collections.Generic;
    using PsMixer.Enums;

    public interface IPsAudioPlayer
    {
        event EventHandler<PlayerReportEventArgs> PlayerReport;

        event EventHandler SongFinished;

        event PeakLevelHandler ChannelPeak;

        AudioDriver AudioDriver { get; set; }

        IEnumerable<ChannelFriendlyName> ChannelNames { get; }

        TimeSpan CurrentTime { get; }

        TimeSpan TotalTime { get; }

        double Progress { get; set; }

        bool IsPeakLevelEnabled { get; set; }

        void Play(PsSong song);
        
        void Pause();
        
        void Resume();

        void Stop();

        void Rewind(TimeSpan span);

        void SetChannelVolume(ChannelFriendlyName name, float newVolume);

        void MuteChannel(ChannelFriendlyName channelFriendlyName, bool isMuted);
    }
}
