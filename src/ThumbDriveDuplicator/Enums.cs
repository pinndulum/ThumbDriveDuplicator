using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThumbDriveDuplicator
{
    public enum FileSystem { FAT, FAT32, exFAT, NTFS, UDF }

    public enum DriveInfoProgressStatus { Ready, Formatting, Copying, Failed, Canceled, Finished }
}
