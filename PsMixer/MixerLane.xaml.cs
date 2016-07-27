namespace PsMixer
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using PsMixer.Enums;
    using PsMixer.Models;

    /// <summary>
    /// Interaction logic for MixerControl.xaml
    /// </summary>
    public partial class MixerLane : UserControl
    {
        public static readonly DependencyProperty VolumeProperty =
            DependencyProperty.Register(
            "Volume",
            typeof(float),
            typeof(MixerLane),
            new PropertyMetadata(1.0f));

        public static readonly DependencyProperty LevelProperty =
            DependencyProperty.Register(
            "Level",
            typeof(float),
            typeof(MixerLane),
            new PropertyMetadata(0.0f));

        public static readonly DependencyProperty IsMutedProperty =
            DependencyProperty.Register(
            "IsMuted",
            typeof(bool),
            typeof(MixerLane),
            new PropertyMetadata(false));

        public static readonly DependencyProperty ChannelNameProperty =
            DependencyProperty.Register(
            "ChannelName",
            typeof(ChannelFriendlyName),
            typeof(MixerLane),
            new PropertyMetadata(ChannelFriendlyName.None));

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(
            "IsActive",
            typeof(bool),
            typeof(MixerLane),
            new PropertyMetadata(true));

        public MixerLane()
        {
            this.InitializeComponent();
        }

        public event EventHandler VolumeChanged;

        public event EventHandler MuteChanged;

        public float Volume
        {
            get { return (float)this.GetValue(VolumeProperty); }
            set { this.SetValue(VolumeProperty, value); }
        }

        public float Level
        {
            get { return (float)this.GetValue(LevelProperty); }
            set { this.SetValue(LevelProperty, value); }
        }

        public bool IsMuted
        {
            get { return (bool)this.GetValue(IsMutedProperty); }
            set { this.SetValue(IsMutedProperty, value); }
        }

        public ChannelFriendlyName ChannelName
        {
            get { return (ChannelFriendlyName)this.GetValue(ChannelNameProperty); }
            set { this.SetValue(ChannelNameProperty, value); }
        }

        public bool IsActive
        {
            get { return (bool)this.GetValue(IsActiveProperty); }
            set { this.SetValue(IsActiveProperty, value); }
        }

        private void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.VolumeChanged != null)
            {
                this.VolumeChanged(
                    this,
                    new VolumeChangedEventArgs((float)e.OldValue, (float)e.NewValue));
            }
        }

        private void OnMuteButtonValueChanged(object sender, EventArgs e)
        {
            var button = sender as LaneButton;
            this.IsMuted = button.IsChecked;

            if (this.MuteChanged != null)
            {
                this.MuteChanged(this, EventArgs.Empty);
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double currentVolume = Math.Round(this.Volume, 2);

            if (e.Delta > 0 && currentVolume < PsAudioPlayer.VolumeMaxValue)
            {
                this.Volume += (float)PsAudioPlayer.VolumeStep;
            }
            else if (e.Delta < 0 && currentVolume > PsAudioPlayer.VolumeMinValue)
            {
                this.Volume -= (float)PsAudioPlayer.VolumeStep;
            }
        }
    }
}