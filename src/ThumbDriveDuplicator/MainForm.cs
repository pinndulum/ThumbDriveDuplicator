using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace ThumbDriveDuplicator
{
    public partial class MainForm : Form
    {
        public static IEqualityComparer<DriveInfo> DriveInfoComparer = new LambdaComparer<DriveInfo>((d1, d2) => d1.Name.Equals(d2.Name));
        private readonly ObservableCollection<DriveInfo> _drives = new ObservableCollection<DriveInfo>();

        public MainForm()
        {
            InitializeComponent();
            comboBox1.DataSource = DiskOperations.AvailableFormatFileSystems();
            _drives.CollectionChanged += _drives_CollectionChanged;
            var t = new Thread(DriveMonitorThread)
            {
                Name = "ThumbDriveDuplicatorDriveMonitor",
                IsBackground = true
            };
            t.Start();
        }

        public string CopyFromDir { get { return ThreadSafeWinControlHelper.GetText(textBox1); } }
        public bool FormatDrive { get { return ThreadSafeWinControlHelper.GetCheckBoxCheck(checkBox1); } }
        public FileSystem FormatFileSystem { get { return ThreadSafeWinControlHelper.GetComboBoxSelectedItem(comboBox1).ParseOrDefault<FileSystem>(FileSystem.FAT); } }
        public string VolumeLabel { get { return ThreadSafeWinControlHelper.GetText(textBox2); } }

        private void _drives_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var oldItems = e.OldItems != null ? e.OldItems.Cast<DriveInfo>() : Enumerable.Empty<DriveInfo>();
            foreach (var rmvItem in oldItems)
            {
                ThreadSafeWinControlHelper.RemoveDriveInfoWrapper(flowLayoutPanel1, rmvItem.Name);
            }
            var newItems = e.NewItems != null ? e.NewItems.Cast<DriveInfo>() : Enumerable.Empty<DriveInfo>();
            foreach (var addItem in newItems)
            {
                var t = new Thread((ThreadStart)delegate { DetectDriveDisconnectThread(addItem); })
                {
                    Name = string.Format("ThumbDriveDuplicatorDisconnect;{0}", addItem.RootDirectory.FullName),
                    IsBackground = true
                };
                t.Start();
                var driveWrapper = new DriveInfoData(addItem);
                ThreadSafeWinControlHelper.AddDriveInfoWrapper(this, flowLayoutPanel1, driveWrapper);
            }
        }

        private void DriveMonitorThread()
        {
            while (true)
            {
                var invalidDrive = Path.GetPathRoot(Application.StartupPath);
                var allDrives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType.Equals(DriveType.Removable) && !d.Name.Equals(invalidDrive, StringComparison.InvariantCultureIgnoreCase));
                var formatting = ThreadSafeWinControlHelper.GetDriveInfoProgress(flowLayoutPanel1, DriveInfoProgressStatus.Formatting).Select(c => c.Volume);
                var addDrives = allDrives.Where(d => !formatting.Contains(d.Name)).Except(_drives, DriveInfoComparer);
                foreach (var drive in addDrives)
                    _drives.Add(drive);
                Thread.Sleep(300);
            }
        }

        private void DetectDriveDisconnectThread(DriveInfo drive)
        {
            var control = ThreadSafeWinControlHelper.GetDriveInfoProgress(flowLayoutPanel1, drive.Name);
            while (drive.IsReady || control.Status == DriveInfoProgressStatus.Formatting)
            {
                if (control == null)
                    control = ThreadSafeWinControlHelper.GetDriveInfoProgress(flowLayoutPanel1, drive.Name);
                // Waiting for disconnect.
                Thread.Sleep(300);
            }
            lock (_drives)
            {
                _drives.Remove(drive);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
                textBox1.Text = folderBrowserDialog1.SelectedPath;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            groupBox1.Enabled = (sender as CheckBox).Checked;
        }

        private void flowLayoutPanel1_SizeChanged(object sender, EventArgs e)
        {
            var panel = sender as FlowLayoutPanel;
            foreach (var control in panel.Controls.OfType<DriveInfoProgress>())
                control.Width = panel.Width - 10;
        }
    }
}
