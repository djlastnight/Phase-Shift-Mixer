namespace PsMixer.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    public class SongScanner
    {
        private const double MaxFoldersCount = 10000;

        private readonly string rootFolder;

        private BackgroundWorker worker;

        private HashSet<string> scannedDirectories;

        private long scannedDirectoriesCount;

        private Exception exception;

        private Loading loading;

        private bool cancelled;

        public SongScanner(string rootFolder)
        {
            this.rootFolder = rootFolder;
        }

        public event EventHandler<SongScannerEventArgs> ScanCompleted;

        public Exception Exception
        {
            get
            {
                return this.exception;
            }
        }

        public void ScanAsync()
        {
            if (this.ScanCompleted == null)
            {
                throw new InvalidAsynchronousStateException(
                    "You must subscribe to ScanCompleted event first!");
            }

            this.scannedDirectoriesCount = 0;
            this.scannedDirectories = new HashSet<string>();
            this.worker = new BackgroundWorker();
            this.worker.WorkerSupportsCancellation = true;
            this.worker.DoWork += this.WorkerDoWork;
            this.worker.RunWorkerCompleted += this.WorkerCompleted;
            this.loading = new Loading();
            if (Application.Current != null &&
                Application.Current.MainWindow != null &&
                Application.Current.MainWindow.IsActive)
            {
                this.loading.Owner = Application.Current.MainWindow;
                this.loading.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                this.loading.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            this.loading.Cancelled += (ss, ee) =>
                {
                    this.worker.CancelAsync();
                    this.cancelled = true;
                };

            this.loading.Show();

            this.worker.RunWorkerAsync();
        }

        private void ScanFolders(DirectoryInfo dirInfo)
        {
            if (this.worker.CancellationPending)
            {
                return;
            }

            try
            {
                var directories = dirInfo.EnumerateDirectories();
                if (directories.Count<DirectoryInfo>() > 0)
                {
                    Parallel.ForEach<DirectoryInfo>(directories, this.ScanFolder);
                }
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (IOException)
            {
            }
        }

        private void ScanFolder(DirectoryInfo currentFolder, ParallelLoopState arg2, long arg3)
        {
            if (this.worker.CancellationPending)
            {
                return;
            }

            if (this.scannedDirectoriesCount > MaxFoldersCount)
            {
                string message = string.Format(
                    "The choosen folder '{0}' exceeds the allowed subfolders count: {1}.\r\nPlease scan smaller folders!",
                    this.rootFolder,
                    SongScanner.MaxFoldersCount);

                this.worker.CancelAsync();
                this.exception = new InvalidOperationException(message);
                return;
            }

            this.scannedDirectoriesCount++;

            if (this.IsValidSongFolder(currentFolder.FullName))
            {
                lock (this.scannedDirectories)
                {
                    this.scannedDirectories.Add(currentFolder.FullName);
                }

                return;
            }

            this.ScanFolders(currentFolder);
        }

        private bool IsValidSongFolder(string folder)
        {
            if (!File.Exists(folder + "\\song.ini"))
            {
                return false;
            }

            if (File.Exists(folder + "\\guitar.ogg") ||
                File.Exists(folder + "\\song.ogg"))
            {
                return true;
            }

            return false;
        }

        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            this.ScanFolders(new DirectoryInfo(this.rootFolder));
        }

        private void WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.loading.Close();

            if (this.ScanCompleted != null)
            {
                var args = new SongScannerEventArgs(
                    this.rootFolder,
                    this.scannedDirectories,
                    this.exception,
                    this.cancelled);

                this.ScanCompleted(this, args);
            }
        }
    }
}