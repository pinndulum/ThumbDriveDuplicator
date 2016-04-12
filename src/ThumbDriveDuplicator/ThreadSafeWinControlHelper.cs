using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ThumbDriveDuplicator
{
    public static class ThreadSafeWinControlHelper
    {
        private delegate string GetTextDelegate(Control control);
        private delegate void SetTextDelegate(Control control, string text);
        private delegate bool GetCheckBoxCheckDelegate(CheckBox checkBox);
        private delegate object GetComboBoxSelectedItemDelegate(ComboBox comboBox);
        private delegate void AddDriveInfoWrapperDelegate(MainForm owner, FlowLayoutPanel panel, DriveInfoData driveInfo);
        private delegate DriveInfoProgress GetDriveInfoProgressDelegate(FlowLayoutPanel panel, string volume);
        private delegate IEnumerable<DriveInfoProgress> GetDriveInfoProgressControlsDelegate(FlowLayoutPanel panel, DriveInfoProgressStatus status);
        private delegate void RemoveDriveInfoWrapperDelegate(FlowLayoutPanel panel, string volume);
        private delegate void SetPrgressDelegate(ProgressBar progressBar, int value);

        public static string GetText(Control control)
        {
            if (control.InvokeRequired)
                return (string)control.Invoke(new GetTextDelegate(GetText), control);
            return control.Text;
        }

        public static void SetText(Control control, string text)
        {
            if (control.InvokeRequired)
                control.Invoke(new SetTextDelegate(SetText), control, text);
            else
                control.Text = text;
        }

        public static bool GetCheckBoxCheck(CheckBox checkBox)
        {
            if (checkBox.InvokeRequired)
                return (bool)checkBox.Invoke(new GetCheckBoxCheckDelegate(GetCheckBoxCheck), checkBox);
            return checkBox.Checked;
        }

        public static object GetComboBoxSelectedItem(ComboBox comboBox)
        {
            if (comboBox.InvokeRequired)
                return comboBox.Invoke(new GetComboBoxSelectedItemDelegate(GetComboBoxSelectedItem), comboBox);
            return comboBox.SelectedItem;
        }

        public static void AddDriveInfoWrapper(MainForm owner, FlowLayoutPanel panel, DriveInfoData driveInfo)
        {
            if (panel.InvokeRequired)
            {
                panel.Invoke(new AddDriveInfoWrapperDelegate(AddDriveInfoWrapper), owner, panel, driveInfo);
            }
            else
            {
                var control = new DriveInfoProgress(owner, driveInfo);
                //control.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right)));
                control.Width = panel.Width - 10;
                control.Left = 5;
                control.Top = (panel.Controls.Count * control.Height) + 4;
                panel.Controls.Add(control);
                panel.PerformLayout();
            }
        }

        public static DriveInfoProgress GetDriveInfoProgress(FlowLayoutPanel panel, string volume)
        {
            if (panel.InvokeRequired)
                return (DriveInfoProgress)panel.Invoke(new GetDriveInfoProgressDelegate(GetDriveInfoProgress), panel, volume);
            return panel.Controls.Cast<DriveInfoProgress>().FirstOrDefault(item => item.Volume.Equals(volume));
        }

        public static IEnumerable<DriveInfoProgress> GetDriveInfoProgress(FlowLayoutPanel panel, DriveInfoProgressStatus status)
        {
            if (panel.InvokeRequired)
                return (IEnumerable<DriveInfoProgress>)panel.Invoke(new GetDriveInfoProgressControlsDelegate(GetDriveInfoProgress), panel, status);
            return panel.Controls.Cast<DriveInfoProgress>().Where(item => item.Status.Equals(status));
        }

        public static void RemoveDriveInfoWrapper(FlowLayoutPanel panel, string volume)
        {
            if (panel.InvokeRequired)
            {
                panel.Invoke(new RemoveDriveInfoWrapperDelegate(RemoveDriveInfoWrapper), panel, volume);
            }
            else
            {
                var control = GetDriveInfoProgress(panel, volume);
                if (control != null)
                    panel.Controls.Remove(control);
            }
        }

        public static void SetProgress(ProgressBar progressBar, int value)
        {
            if (progressBar.InvokeRequired)
                progressBar.Invoke(new SetPrgressDelegate(SetProgress), progressBar, value);
            else
                progressBar.Value = value;
        }
    }
}
