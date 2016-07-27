namespace PsMixer.Models
{
    using System;
    using System.Collections.Generic;

    public class SongScannerEventArgs : EventArgs
    {
        public SongScannerEventArgs(string rootFolder, IEnumerable<string> songFolders, Exception exception, bool cancelled)
            : base()
        {
            this.RootFolder = rootFolder;
            this.SongFolders = songFolders;
            this.Exception = exception;
            this.Cancelled = cancelled;
        }

        public string RootFolder { get; private set; }

        public IEnumerable<string> SongFolders { get; private set; }

        public Exception Exception { get; private set; }

        public bool Cancelled { get; private set; }
    }
}