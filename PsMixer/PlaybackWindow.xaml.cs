namespace PsMixer
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for PlaybackWindow.xaml
    /// </summary>
    public partial class PlaybackWindow : Window
    {
        public PlaybackWindow()
        {
            this.InitializeComponent();
        }

        public static NAudio.Wave.IWavePlayer Player { get; set; }
    }
}