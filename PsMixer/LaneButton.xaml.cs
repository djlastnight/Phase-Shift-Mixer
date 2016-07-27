namespace PsMixer
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for LaneButton.xaml
    /// </summary>
    public partial class LaneButton : UserControl
    {
        public static readonly DependencyProperty CheckedSourceProperty =
            DependencyProperty.Register(
            "CheckedSource",
            typeof(ImageSource),
            typeof(LaneButton),
            new PropertyMetadata(null));

        public static readonly DependencyProperty UncheckedSourceProperty =
            DependencyProperty.Register(
            "UncheckedSource",
            typeof(ImageSource),
            typeof(LaneButton),
            new PropertyMetadata(null));

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
            "IsChecked",
            typeof(bool),
            typeof(LaneButton),
            new PropertyMetadata(false));

        public LaneButton()
        {
            this.InitializeComponent();
        }

        public event EventHandler CheckedChanged;

        public ImageSource CheckedSource
        {
            get { return (ImageSource)this.GetValue(CheckedSourceProperty); }
            set { this.SetValue(CheckedSourceProperty, value); }
        }

        public ImageSource UncheckedSource
        {
            get { return (ImageSource)this.GetValue(UncheckedSourceProperty); }
            set { this.SetValue(UncheckedSourceProperty, value); }
        }

        public bool IsChecked
        {
            get { return (bool)this.GetValue(IsCheckedProperty); }
            set { this.SetValue(IsCheckedProperty, value); }
        }

        private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.IsChecked = !this.IsChecked;
            if (this.CheckedChanged != null)
            {
                this.CheckedChanged(this, EventArgs.Empty);
            }
        }
    }
}