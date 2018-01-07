using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGMC.SFTPLibrary
{
    public class FTPDownloadInfo
    {
        public FTPDownloadInfo()
        {
            CurrentMonthFTPDownloadStatusList = new List<FTPDownloadStatus>();
            PreviousMonthFTPDownloadStatusList = new List<FTPDownloadStatus>();

            //FilesToMovePrevMonth = new List<string>();
            //FilesToMoveCurrMonth = new List<string>();
            //ArchivedFolderPrevMonth = string.Empty;
            //ArchivedFolderCurrMonth = string.Empty;
        }
        public List<FTPDownloadStatus> CurrentMonthFTPDownloadStatusList { get; set; }
        public List<FTPDownloadStatus> PreviousMonthFTPDownloadStatusList { get; set; }

        //public List<string> FilesToMovePrevMonth { get; set; }
        //public List<string> FilesToMoveCurrMonth { get; set; }
        //public string ArchivedFolderPrevMonth { get; set; }
        //public string ArchivedFolderCurrMonth { get; set; }

    }
}
