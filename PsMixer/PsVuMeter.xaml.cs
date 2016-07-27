namespace PsMixer
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for VuMeter.xaml
    /// </summary>
    public partial class PsVuMeter : UserControl
    {
        public static readonly DependencyProperty VuLevelProperty =
            DependencyProperty.Register(
            "VuLevel",
            typeof(float),
            typeof(PsVuMeter),
            new PropertyMetadata(0.0f));

        public PsVuMeter()
        {
            this.InitializeComponent();
        }

        public float VuLevel
        {
            get { return (float)this.GetValue(VuLevelProperty); }
            set { this.SetValue(VuLevelProperty, value); }
        }
    }
}