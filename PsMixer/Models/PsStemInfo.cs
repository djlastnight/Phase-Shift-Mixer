namespace PsMixer.Models
{
    using PsMixer.Enums;

    public class PsStemInfo
    {
        public PsStemInfo(string filePath, ChannelFriendlyName channelName)
        {
            this.FilePath = filePath;
            this.ChannelName = channelName;
        }

        public string FilePath { get; private set; }

        public ChannelFriendlyName ChannelName { get; private set; }
    }
}