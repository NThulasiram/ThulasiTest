using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGMC.SFTPLibrary
{
    public class FTPUploadStatus
    {
        public FTPUploadStatus(string localFilePath, bool status, string errorMessage)
        {
            LocalFilePath = localFilePath;
            Status = status;
            ErrorMessage = errorMessage;
        }
        public string LocalFilePath { get; set; }
        public bool Status { get; set; }
        public string ErrorMessage { get; set; }
    }
}
