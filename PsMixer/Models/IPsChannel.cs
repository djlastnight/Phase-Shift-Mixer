namespace PsMixer.Models
{
    using PsMixer.Enums;

    public interface IPsChannel
    {
        event PeakLevelHandler PeakLevel;

        ChannelFriendlyName ChannelFriendlyName { get; }

        bool IsPeakLevelEnabled { get; set; }

        PeakLevelUpdateSpeed PeakLevelUpdateSpeed { get; set; }

        float Volume { get; set; }

        bool IsMuted { get; set; }
    }
}