using System;
using System.IO;

namespace ThumbDriveDuplicator
{
    public class DriveInfoData
    {
        private float _progress;

        public event EventHandler ProgressChanged;

        public string Volume { get; private set; }
        public string CopyFromPath { get; set; }
        public bool FormatDrive { get; set; }
        public FileSystem FileSystem { get; set; }
        public string VolumeLabel { get; set; }
        public float Progress { get { return _progress; } set { SetProgress(value); } }

        public DriveInfoData(DriveInfo driveInfo)
        {
            if (driveInfo == null)
                throw new ArgumentNullException("driveInfo");
            Volume = driveInfo.Name;
        }

        private void SetProgress(float value)
        {
            _progress = value;
            if (ProgressChanged != null)
                ProgressChanged(this, EventArgs.Empty);
        }
    }
}
