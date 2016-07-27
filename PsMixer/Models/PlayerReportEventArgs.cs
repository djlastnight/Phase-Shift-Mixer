namespace PsMixer.Models
{
    using System;

    public class PlayerReportEventArgs : EventArgs
    {
        public PlayerReportEventArgs(double progress, TimeSpan currentTime, TimeSpan totalTime)
            : base()
        {
            if (currentTime == null)
            {
                throw new ArgumentNullException("currentTime");
            }

            if (totalTime == null)
            {
                throw new ArgumentNullException("totalTime");
            }

            this.Progress = progress;
            this.CurrentTime = currentTime;
            this.TotalTime = totalTime;
        }

        public double Progress { get; private set; }

        public TimeSpan CurrentTime { get; private set; }

        public TimeSpan RemainingTime
        {
            get
            {
                return this.TotalTime - this.CurrentTime;
            }
        }

        public TimeSpan TotalTime { get; private set; }
    }
}
