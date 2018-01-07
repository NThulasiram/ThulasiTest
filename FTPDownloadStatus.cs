using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGMC.SFTPLibrary
{
    public class FTPDownloadStatus
    {
        public FTPDownloadStatus(string remoteFilePath, bool status, string errorMessage,string downloadPath)
        {
            RemoteFilePath = remoteFilePath;
            Status = status;
            ErrorMessage = errorMessage;
            DownloadPath = downloadPath;
        }

        public string RemoteFilePath { get; set; }
        public bool Status { get; set; }
        public string ErrorMessage { get; set; }
        public string DownloadPath { get; set; }
    }
}
