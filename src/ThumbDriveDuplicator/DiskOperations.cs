using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ThumbDriveDuplicator
{
    public static class DiskOperations
    {
        public static void Format(string volume, FileSystem fileSytem = FileSystem.FAT, string volumeLabel = "MyVolume")
        {
            volume = volume.Replace(Path.VolumeSeparatorChar.ToString(), string.Empty);
            volume = volume.Replace(Path.DirectorySeparatorChar.ToString(), string.Empty);
            var args = string.Format("{0}: /fs:{1} /v:{2} /q", volume, fileSytem, volumeLabel.Replace(" ", string.Empty));
            var startInfo = new ProcessStartInfo { FileName = "format.com", Arguments = args, UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardOutput = true };
            var formatComplete = false;
            using (var p = Process.Start(startInfo))
            {
                p.StandardInput.Write("\r\n");
                formatComplete = p.StandardOutput.ReadToEnd().Contains("Format complete.");
                p.WaitForExit();
            }
            if (!formatComplete)
                throw new Exception(string.Format("Failed to format drive {0}:", volume));
            if (!string.IsNullOrWhiteSpace(volumeLabel))
                Label(volume, volumeLabel);
        }

        public static string[] AvailableFormatFileSystems()
        {
            var fileSystems = new List<string>();
            var startInfo = new ProcessStartInfo { FileName = "format.com", Arguments = "/?", UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardOutput = true };
            var fileSystemValues = Enum.GetNames(typeof(FileSystem));
            using (var p = Process.Start(startInfo))
            {
                p.OutputDataReceived += (sender, e) =>
                {
                    var line = e.Data ?? string.Empty;
                    foreach (var fileSystem in fileSystemValues)
                    {
                        if (line.Contains(fileSystem) && !fileSystems.Contains(fileSystem))
                            fileSystems.Add(fileSystem);
                    }
                };
                p.BeginOutputReadLine();
                p.WaitForExit();
            }
            return fileSystems.ToArray();
        }

        public static void Label(string volume, string volumeLabel = "MyVolume")
        {
            volume = volume.Replace(Path.VolumeSeparatorChar.ToString(), string.Empty);
            volume = volume.Replace(Path.DirectorySeparatorChar.ToString(), string.Empty);
            var args = string.Format("{0}: {1}", volume, volumeLabel);
            var startInfo = new ProcessStartInfo { FileName = "label.exe", Arguments = args, UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardOutput = true };
            var p = Process.Start(startInfo);
            p.WaitForExit();
            var resp = p.StandardOutput.ReadToEnd();
            p.Close();
            if (!string.IsNullOrWhiteSpace(resp))
                throw new Exception(string.Format("Failed to set drive {0}: volume label", volume), new Exception(resp));
        }

        public static IEnumerable<string> GetFiles(string root, string searchPattern = "*.*", bool searchSubDirectories = true)
        {
            var pending = new Stack<string>();
            pending.Push(root);
            while (pending.Count > 0)
            {
                var path = pending.Pop();
                var next = Enumerable.Empty<string>();
                try
                {
                    next = Directory.GetFiles(path, searchPattern);
                }
                catch { }
                foreach (var file in next)
                    yield return file;
                if (!searchSubDirectories)
                    break;
                try
                {
                    next = Directory.GetDirectories(path);
                    foreach (var subdir in next)
                        pending.Push(subdir);
                }
                catch { }
            }
        }
    }
}
