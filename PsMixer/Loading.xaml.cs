namespace PsMixer
{
    using System;
    using System.Windows;

    /// <summary>
    /// Interaction logic for Loading.xaml
    /// </summary>
    public partial class Loading : Window
    {
        public Loading()
        {
            this.InitializeComponent();
        }

        public event EventHandler Cancelled;

        private void OnCancelButtonClicked(object sender, RoutedEventArgs e)
        {
            if (this.Cancelled != null)
            {
                this.Cancelled(this, EventArgs.Empty);
            }
        }
    }
}