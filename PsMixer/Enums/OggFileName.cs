namespace PsMixer.Enums
{
    /// <summary>
    /// Enumerates the possible ogg file names without extension.
    /// All names written here are in lower case and their value
    /// must corespond to the respective ChannelName's enumeration value.
    /// </summary>
    public enum OggFileName : int
    {
        none = 0,
        drums = 1,
        rhythm = 2,
        guitar = 3,
        keys = 4,
        vocals = 5,
        vocals_1 = 6,
        vocals_2 = 7,
        drums_1 = 8,
        drums_2 = 9,
        drums_3 = 10,
        drums_4 = 11,
        song = 12,
        crowd = 13,
    }
}