namespace PsMixer.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using PsMixer.Enums;
    using PsMixer.Models;

    public static class PsStemsRetriever
    {
        public static IEnumerable<PsStemInfo> GetValidStems(string phaseShiftSongFolder)
        {
            List<PsStemInfo> stemInfos = new List<PsStemInfo>();

            var existingOggFiles = Directory.EnumerateFiles(
                phaseShiftSongFolder, "*.ogg", SearchOption.TopDirectoryOnly);

            var allowedOggFiles = Enum.GetNames(typeof(OggFileName));

            foreach (var existingOggFile in existingOggFiles)
            {
                var cleanFileName = Path.GetFileNameWithoutExtension(existingOggFile);

                OggFileName parsedFileName = 0;
                bool parseOk = Enum.TryParse(cleanFileName.ToLower(), out parsedFileName);
                if (parseOk)
                {
                    int underlayingOggFileName = (int)parsedFileName;
                    var friendlyName = (ChannelFriendlyName)underlayingOggFileName;

                    var stemInfo = new PsStemInfo(existingOggFile, friendlyName);
                    stemInfos.Add(stemInfo);
                }
            }

            return stemInfos;
        }
    }
}
