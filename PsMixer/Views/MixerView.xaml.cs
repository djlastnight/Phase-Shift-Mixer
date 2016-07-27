namespace PsMixer.Views
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using PsMixer.ViewModels;

    /// <summary>
    /// Interaction logic for MixerView.xaml
    /// </summary>
    public partial class MixerView : UserControl
    {
        public MixerView()
        {
            this.InitializeComponent();
            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Closing += this.OnParentWindowClosing;
            }
        }

        private void OnParentWindowClosing(object sender, CancelEventArgs e)
        {
            var mixerViewModel = this.DataContext as MixerViewModel;
            if (mixerViewModel != null &&
                mixerViewModel.Player != null)
            {
                mixerViewModel.Player.Stop();
            }
        }
    }
}