using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncompassLibrary.LoanDataExtractor
{
    public class AttachmentStatus
    {
        public AttachmentStatus(string attachmentName, string placeholderName, bool downloadStatus, string error)
        {
            AttachmentName = attachmentName;
            PlaceholderName = placeholderName;
            DownloadStatus = downloadStatus;
            Error = error;
        }
        public string AttachmentName { get; set; }
        public string PlaceholderName { get; set; }
        public bool DownloadStatus { get; set; }
        public string Error { get; set; }
    }
}
