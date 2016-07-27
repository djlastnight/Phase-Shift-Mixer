namespace PsMixer.Enums
{
    /// <summary>
    /// Enumeration used for channel (stem) friendly name.
    /// Its values are related to the OggFileName enumeration's values.
    /// This allows easy conversion from channel's file name to its friendly name.
    /// </summary>
    public enum ChannelFriendlyName : int
    {
        None = 0,
        DrumMix = 1,
        Bass = 2,
        Guitar = 3,
        Keys = 4,
        VocalMix = 5,
        Vocals1 = 6,
        Vocals2 = 7,
        Drums1 = 8,
        Drums2 = 9,
        Drums3 = 10,
        Drums4 = 11,
        Song = 12,
        Crowd = 13
    }
}