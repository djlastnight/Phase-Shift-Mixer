namespace PsMixer.Models
{
    using System;

    public class VolumeChangedEventArgs : EventArgs
    {
        private float oldVolume;
        private float newVolume;

        public VolumeChangedEventArgs(float oldVolume, float newVolume)
        {
            this.oldVolume = oldVolume;
            this.newVolume = newVolume;
        }

        public float OldVolume
        {
            get
            {
                return this.oldVolume;
            }
        }

        public float NewVolume
        {
            get
            {
                return this.newVolume;
            }
        }
    }
}