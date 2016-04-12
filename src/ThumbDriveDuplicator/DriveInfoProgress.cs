using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace ThumbDriveDuplicator
{
    public partial class DriveInfoProgress : UserControl
    {
        private MainForm _owner;
        private DriveInfoData _driveInfoData;
        private BackgroundWorker _bgw;

        public string Volume { get; private set; }
        public DriveInfoProgressStatus Status { get; private set; }
        public float Progress { get; private set; }

        public DriveInfoProgress(MainForm owner, DriveInfoData driveInfoData)
        {
            if (driveInfoData == null)
                throw new ArgumentNullException("driveInfoWrapper");
            InitializeComponent();
            _owner = owner;
            _driveInfoData = driveInfoData;
            _driveInfoData.ProgressChanged += _driveInfoWrapper_ProgressChanged;
            Volume = _driveInfoData.Volume;
            label1.Text = Volume;
            SetStatus(DriveInfoProgressStatus.Ready);
            _bgw = new BackgroundWorker() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            _bgw.DoWork += bgw_DoWork;
            _bgw.ProgressChanged += bgw_ProgressChanged;
            _bgw.RunWorkerCompleted += bgw_RunWorkerCompleted;
            StartCopy();
        }

        private void SetStatus(DriveInfoProgressStatus status, string formatString = "{0}")
        {
            Status = status;
            ThreadSafeWinControlHelper.SetText(label2, string.Format(formatString, status));
        }

        private void StartCopy()
        {
            if (_bgw == null || _bgw.IsBusy)
                return;
            _driveInfoData.CopyFromPath = _owner.CopyFromDir;
            _driveInfoData.FormatDrive = _owner.FormatDrive;
            _driveInfoData.FileSystem = _owner.FormatFileSystem;
            _driveInfoData.VolumeLabel = _owner.VolumeLabel;
            _bgw.RunWorkerAsync(_driveInfoData);
        }

        private void CancelCopy()
        {
            if (_bgw == null || !_bgw.IsBusy)
                return;
            _bgw.CancelAsync();
        }

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            var bgw = sender as BackgroundWorker;
            var data = e.Argument as DriveInfoData;
            var volume = data.Volume ?? string.Empty;
            var copyPath = data.CopyFromPath ?? string.Empty;
            if (!Directory.Exists(volume) || !Directory.Exists(copyPath))
            {
                e.Cancel = true;
                return;
            }
            if (data.FormatDrive)
            {
                SetStatus(DriveInfoProgressStatus.Formatting);
                DiskOperations.Format(volume, data.FileSystem, data.VolumeLabel ?? string.Empty);
            }
            var totalcopied = 0L;
            var totalsize = new DirectoryInfo(copyPath).EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
            foreach (var filePath in DiskOperations.GetFiles(copyPath).ToArray())
            {
                if (bgw.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                var relativePath = filePath.Replace(copyPath, string.Empty);
                if (relativePath.StartsWith(Path.DirectorySeparatorChar.ToString()))
                    relativePath = relativePath.Substring(1);
                var newPath = Path.Combine(volume, relativePath);
                if (!Directory.Exists(Path.GetDirectoryName(newPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                var size = new FileInfo(filePath).Length;
                bgw.ReportProgress((int)(data.Progress * 100), newPath);
                if (File.Exists(newPath))
                    File.Delete(newPath);
                var buffer = File.ReadAllBytes(filePath);
                File.WriteAllBytes(newPath, buffer);
                totalcopied += size;
                var percent = (float)totalcopied / (float)totalsize;
                data.Progress = percent;
                Thread.Sleep(100);
            }
        }

        private void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SetStatus(DriveInfoProgressStatus.Copying, string.Format("{0}% {{0}} '{1}'", e.ProgressPercentage, e.UserState));
        }

        private void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SetStatus(DriveInfoProgressStatus.Finished);
            if (e.Cancelled)
                SetStatus(DriveInfoProgressStatus.Canceled);
            else if (e.Error != null)
                SetStatus(DriveInfoProgressStatus.Failed, e.Error.Message);
        }

        private void _driveInfoWrapper_ProgressChanged(object sender, EventArgs e)
        {
            Progress = _driveInfoData.Progress;
            ThreadSafeWinControlHelper.SetProgress(progressBar1, (int)(Progress * 100));
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            var menu = sender as ContextMenuStrip;
            menu.Items.Clear();
            var menuItem = default(ToolStripMenuItem);
            if (_bgw != null && _bgw.IsBusy)
                menuItem = new ToolStripMenuItem("Cancel Copy");
            if (_bgw != null && !_bgw.IsBusy)
                menuItem = new ToolStripMenuItem("Start Copy");
            if (menuItem != null)
            {
                menuItem.Click += menuItem_Click;
                menu.Items.Add(menuItem);
            }
        }

        void menuItem_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            switch (menuItem.Text)
            {
                case "Cancel Copy":
                    CancelCopy();
                    break;
                case "Start Copy":
                    StartCopy();
                    break;
            }
        }
    }
}
