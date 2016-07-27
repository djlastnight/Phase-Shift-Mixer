namespace PsMixer.Models
{
    using System;
    using System.IO;
    using System.Text;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using PsMixer.Helpers;

    public class PsSong
    {
        private const string DefaultArtPath = "/PsMixer;component/Images/DefaultAlbumArt/default_album_art.png";

        private readonly ImageSource defaultArt = new BitmapImage(
                new Uri(DefaultArtPath, UriKind.RelativeOrAbsolute));

        private string folder;

        private string genre;

        private string year;

        private string albumName;

        private ImageSource albumArt;

        public PsSong(string folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException("folder");
            }

            if (!Directory.Exists(folder))
            {
                throw new ArgumentException(
                    "folder does not exists");
            }

            this.Folder = folder;
        }

        public string ArtistName { get; private set; }

        public string SongName { get; private set; }

        public string Genre
        {
            get
            {
                if (this.genre == null)
                {
                    this.genre = this.GetSingleIniValue("genre");
                }

                return this.genre;
            }
        }

        public string Year
        {
            get
            {
                if (this.year == null)
                {
                    this.year = this.GetSingleIniValue("year");
                }

                return this.year;
            }
        }

        public string AlbumName
        {
            get
            {
                if (this.albumName == null)
                {
                    this.albumName = this.GetSingleIniValue("album");
                }

                return this.albumName;
            }
        }

        public ImageSource AlbumArt
        {
            get
            {
                if (this.albumArt == null)
                {
                    this.albumArt = this.RetrieveAlbumArt();
                }

                return this.albumArt;
            }
        }

        public string Folder
        {
            get
            {
                return this.folder;
            }

            private set
            {
                this.folder = value;

                this.RetrieveArtistAndName();
            }
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.ArtistName) &&
                !string.IsNullOrEmpty(this.SongName))
            {
                return string.Format(
                    "{0} - {1}", this.ArtistName, this.SongName);
            }

            return new DirectoryInfo(this.folder).Name;
        }

        private void RetrieveArtistAndName()
        {
            var values = IniReader.GetValues(
                this.folder + "\\song.ini",
                Encoding.ASCII,
                "artist",
                "name");

            this.ArtistName = values[0];
            this.SongName = values[1];
        }

        private ImageSource RetrieveAlbumArt()
        {
            string imageFileLocation = this.folder + "\\album.png";

            if (!File.Exists(imageFileLocation))
            {
                return this.defaultArt;
            }

            return new BitmapImage(new Uri(imageFileLocation));
        }

        private string GetSingleIniValue(string tag)
        {
            string value = IniReader.GetFirstValue(
                this.folder + "\\song.ini",
                Encoding.ASCII,
                tag);

            return value;
        }
    }
}