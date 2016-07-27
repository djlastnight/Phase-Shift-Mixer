namespace PsMixer
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for MediaButtons.xaml
    /// </summary>
    public partial class MediaButton : UserControl
    {
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(
            "ImageSource",
            typeof(ImageSource),
            typeof(MediaButton),
            new PropertyMetadata(null));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(MediaButton),
            new PropertyMetadata(null));

        public MediaButton()
        {
            this.InitializeComponent();
        }

        public ImageSource ImageSource
        {
            get { return (ImageSource)this.GetValue(ImageSourceProperty); }
            set { this.SetValue(ImageSourceProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)this.GetValue(CommandProperty); }
            set { this.SetValue(CommandProperty, value); }
        }

        private void OnLeftClick(object sender, MouseButtonEventArgs e)
        {
            if (this.Command != null)
            {
                this.Command.Execute(null);
            }
        }
    }
}