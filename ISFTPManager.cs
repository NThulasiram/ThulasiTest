using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGMC.SFTPLibrary
{
    public interface ISFTPManager
    {
        void Initialize(string sftpUrl, string sftpUserId, string sftpPassword, string sftpProtocol, string sftpKey, string sftpPortNumber, string transferResumeSupport, bool isTestMode);
        bool Upload(string localFilePath, string remoteFilePath,int applicationId);
        List<FTPUploadStatus> Upload(List<string> localFiles, string remoteDirPath,int applicationId);
        bool CreateRemoteFolder(string remoteBasePath, string remoteFolderName,int applicationId);
        bool CreateRemoteFolder(string remoteBasePath, string remoteFolderName, int applicationId,out string errorMessage);
        FTPDownloadInfo DownloadLoansBasedOnRDY(string localDirPath, string remoteBasePath, string localDirPathTemp, int? fileDownloadLimit,int applicationId);
        bool RemoteDirExists(string remoteDirPath,int applicationId);
        bool RemoteDirExists(string remoteDirPath, int applicationId, out string errormessage);
        bool MoveRemoteFiles(List<string> remoteFiles, string remoteDirPath, string localDirPathTemp,int applicationId);
        /// <summary>
        /// Call this method at the end of the FTP operation.
        /// </summary>
        void Close();
    }
}
