using Log4NetLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WinSCP;

namespace FGMC.SFTPLibrary
{
    /// <summary>
    /// This class opens a WinScp session for every request. After the FTP operation it closes the session.
    /// </summary>
    public class SFTPManagerMultiSession : ISFTPManager
    {
        // private readonly ILogService _logService;
        private readonly ILoggingService _logService;
        private SessionOptions _sessionOption;
        private string _resumeSupport = string.Empty;
        private bool _isTestMode = false;
        private LogModel _logModel;
        public SFTPManagerMultiSession()
        {
            _logService = new FileLoggingService(typeof(SFTPManagerMultiSession));
            _logModel = new LogModel();
        }

        public void Initialize(string sftpUrl, string sftpUserId, string sftpPassword, string sftpProtocol, string sftpKey, string sftpPortNumber, string transferResumeSupport, bool isTestMode)
        {
            _resumeSupport = transferResumeSupport;
            _isTestMode = isTestMode;
            SetSessionOption(sftpUrl, sftpUserId, sftpPassword, sftpProtocol, sftpKey, sftpPortNumber);
        }
        public bool RemoteDirExists(string remoteDirPath, int applicationId)
        {
            try
            {
                using (var session = new Session())
                {
                    session.Open(_sessionOption);
                    remoteDirPath = remoteDirPath.Replace('\\', '/');
                    return session.FileExists(remoteDirPath);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logService, _logModel, applicationId, ex.Message, ex: ex);
            }
            return false;
        }
        public bool RemoteDirExists(string remoteDirPath, int applicationId,out string errorMessage)
        {
            try
            {
                errorMessage = string.Empty;
                using (var session = new Session())
                {
                    session.Open(_sessionOption);
                    remoteDirPath = remoteDirPath.Replace('\\', '/');
                    return session.FileExists(remoteDirPath);
                }
            }
            catch (Exception ex)
            {
                errorMessage = GetCustomErrorMessage(ex);
                LogHelper.Error(_logService, _logModel, applicationId, ex.Message, ex: ex);
            }
            return false;
        }


        public List<FTPUploadStatus> Upload(List<string> localFiles, string remoteDirPath, int applicationId)
        {
            List<FTPUploadStatus> statusList = new List<FTPUploadStatus>();
            try
            {
                using (var session = new Session())
                {
                    session.Open(_sessionOption);
                    remoteDirPath = remoteDirPath.Replace('\\', '/');
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.ResumeSupport.State = _resumeSupport.ToLower() == "off" ? TransferResumeSupportState.Off : TransferResumeSupportState.On;

                    foreach (var localFilePath in localFiles)
                    {
                        try
                        {
                            var fileName = Path.GetFileName(localFilePath);
                            var remoteFilePath = string.Format("{0}/{1}", remoteDirPath, fileName);
                            session.PutFiles(localFilePath, remoteFilePath, false, transferOptions).Check();
                            statusList.Add(new FTPUploadStatus(localFilePath, true, string.Empty));
                        }
                        catch (Exception ex)
                        {
                            statusList.Add(new FTPUploadStatus(localFilePath, false, GetCustomErrorMessage(ex)));
                            LogHelper.Error(_logService, _logModel, applicationId, ex.Message, ex: ex);
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                statusList.Add(new FTPUploadStatus(string.Empty, false, GetCustomErrorMessage(ex)));
                LogHelper.Error(_logService, _logModel, applicationId, ex.Message, ex: ex);
            }
            return statusList;
        }

        public bool Upload(string localFilePath, string remoteFilePath, int applicationId)
        {
            try
            {
                using (var session = new Session())
                {
                    session.Open(_sessionOption);
                    remoteFilePath = remoteFilePath.Replace('\\', '/');
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.ResumeSupport.State = _resumeSupport.ToLower() == "off" ? TransferResumeSupportState.Off : TransferResumeSupportState.On;
                    var fileName = Path.GetFileName(localFilePath);
                    remoteFilePath = string.Format("{0}/{1}", remoteFilePath, fileName);
                    session.PutFiles(localFilePath, remoteFilePath, false, transferOptions).Check();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logService, _logModel, applicationId, ex.Message, ex: ex);
                return false;
            }
            return true;
        }

        public bool CreateRemoteFolder(string remoteBasePath, string remoteFolderName, int applicationId)
        {
            try
            {
                using (var session = new Session())
                {
                    session.Open(_sessionOption);
                    remoteBasePath = remoteBasePath.Replace('\\', '/');
                    var remoteDirectoryToCreate = string.Format("{0}/{1}", remoteBasePath, remoteFolderName);
                    session.CreateDirectory(remoteDirectoryToCreate);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logService, _logModel, applicationId, ex.Message, ex: ex);
                return false;
            }
            return true;
        }

        public bool CreateRemoteFolder(string remoteBasePath, string remoteFolderName, int applicationId,out string errorMessage)
        {
            try
            {
                errorMessage = string.Empty;
                using (var session = new Session())
                {
                    session.Open(_sessionOption);
                    remoteBasePath = remoteBasePath.Replace('\\', '/');
                    var remoteDirectoryToCreate = string.Format("{0}/{1}", remoteBasePath, remoteFolderName);
                    session.CreateDirectory(remoteDirectoryToCreate);
                }
            }
            catch (Exception ex)
            {
                errorMessage = GetCustomErrorMessage(ex);
                LogHelper.Error(_logService, _logModel, applicationId, ex.Message, ex: ex);
                return false;
            }
            return true;
        }


        /// <summary>
        /// Downloads files from SFTP
        /// </summary>
        /// <param name="localDirPath">where the filed will be downloaded</param>
        /// <param name="remoteBasePath">pass till 1.None – 42.LoanHD Services PreFund – 22.InvestorQxS</param>
        /// <param name="fileDownloadLimit">Maximum no of the remote file to download. Pass null if wish to download all available files</param>
        /// <returns></returns>
        public FTPDownloadInfo DownloadLoansBasedOnRDY(string localDirPath, string remoteBasePath, string localDirPathTemp, int? fileDownloadLimit, int applicationId)
        {
            List<FTPDownloadStatus> downloadStatusList = new List<FTPDownloadStatus>();
            FTPDownloadInfo ftpDownloadInfo = new FTPDownloadInfo();
            try
            {
                string currentMonth = GetMonthFolder(DateTime.Now.Month);
                string prevMonth = DateTime.Now.Month == 1 ? GetMonthFolder(12) : GetMonthFolder(DateTime.Now.Month - 1);
                remoteBasePath = remoteBasePath.Replace('\\', '/');
                var prevMonthRemotePath = string.Format("{0}/{1}/Outbound", remoteBasePath, prevMonth);
                var currentMonthRemotePath = string.Format("{0}/{1}/Outbound", remoteBasePath, currentMonth);

                //1.Download
                var downloadStatusListPrevMonth = DownLoadFromFtp(localDirPath, prevMonthRemotePath, fileDownloadLimit, applicationId);
                var downloadStatusListCurrentMonth = DownLoadFromFtp(localDirPath, currentMonthRemotePath, fileDownloadLimit, applicationId);

                downloadStatusList.AddRange(downloadStatusListPrevMonth);
                downloadStatusList.AddRange(downloadStatusListCurrentMonth);

                ftpDownloadInfo.PreviousMonthFTPDownloadStatusList = downloadStatusListPrevMonth;//Set the download status list
                ftpDownloadInfo.CurrentMonthFTPDownloadStatusList = downloadStatusListCurrentMonth;//Set the download status list

                //2.Create Archived Folder

                string archivedFolderPrevMonth = string.Empty;
                string archivedFolderCurrMonth = string.Empty;
                if (downloadStatusListPrevMonth.Count != 0)
                {
                    LogHelper.Info(_logService, _logModel, applicationId, "Creating Archived folder in " + prevMonthRemotePath);
                    archivedFolderPrevMonth = CreateArchiveFolder("Archived", prevMonthRemotePath, applicationId);
                }
                if (downloadStatusListCurrentMonth.Count != 0)
                {
                    LogHelper.Info(_logService, _logModel, applicationId, "Creating Archived folder in " + prevMonthRemotePath);
                    archivedFolderCurrMonth = CreateArchiveFolder("Archived", currentMonthRemotePath, applicationId);
                }
            }
            catch (Exception ex)
            {

                LogHelper.Error(_logService, _logModel, applicationId, ex.Message, ex: ex);
            }
            return ftpDownloadInfo;
        }

        private void SetSessionOption(string sftpUrl, string sftpUserId, string sftpPassword, string sftpProtocol, string sftpKey, string sftpPortNumber)
        {
            var protocol = sftpProtocol == "FTP" ? Protocol.Ftp : Protocol.Sftp;
            var sessionOptions = new SessionOptions
            {
                Protocol = protocol,
                HostName = sftpUrl,
                UserName = sftpUserId,
                Password = sftpPassword,
                PortNumber = Convert.ToInt32(sftpPortNumber)
            };
            if (protocol == Protocol.Sftp)
            {
                sessionOptions.SshHostKeyFingerprint = sftpKey;
            }
            else
            {
                sessionOptions.FtpSecure = FtpSecure.Implicit;
            }
            _sessionOption = sessionOptions;
        }

        private string GetMonthFolder(int monthId)
        {
            switch (monthId)
            {
                case 1:
                    return "1.Jan";
                case 2:
                    return "2.Feb";
                case 3:
                    return "3.Mar";
                case 4:
                    return "4.Apr";
                case 5:
                    return "5.May";
                case 6:
                    return "6.Jun";
                case 7:
                    return "7.Jul";
                case 8:
                    return "8.Aug";
                case 9:
                    return "9.Sep";
                case 10:
                    return "10.Oct";
                case 11:
                    return "11.Nov";
                case 12:
                    return "12.Dec";
                default:
                    break;
            }
            return string.Empty;
        }

        private string CreateArchiveFolder(string archiveFolderName, string remoteBasePath, int applicationId)
        {
            string folderToCreate = string.Empty;
            try
            {
                using (var session = new Session())
                {
                    session.Open(_sessionOption);
                    remoteBasePath = remoteBasePath.Replace('\\', '/');
                    folderToCreate = string.Format("{0}/{1}", remoteBasePath, archiveFolderName);
                    if (!session.FileExists(folderToCreate))
                    {
                        session.CreateDirectory(folderToCreate);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logService, _logModel, applicationId, ex.Message, ex: ex);
            }
            return folderToCreate;
        }

        public bool MoveRemoteFiles(List<string> remoteFiles, string remoteDirPath, string localDirPathTemp, int applicationId)
        {
            if (_isTestMode)
            {
                return MoveRemoteFilesByDelete(remoteFiles, remoteDirPath, localDirPathTemp, applicationId);
            }
            else
            {
                return MoveRemoteFilesByMoveCommand(remoteFiles, remoteDirPath, localDirPathTemp, applicationId);
            }
        }

        private bool MoveRemoteFilesByMoveCommand(List<string> remoteFiles, string remoteDirPath, string localDirPathTemp, int applicationId)
        {
            if (remoteFiles.Count == 0)
                return true;
            List<string> moveFailedFiles = new List<string>();
            try
            {
                using (var session = new Session())
                {
                    session.Open(_sessionOption);
                    foreach (var remoteFile in remoteFiles)
                    {
                        try
                        {
                            var pdfSourcePath = remoteFile;
                            var sourceBaseDir = Path.GetDirectoryName(remoteFile);
                            sourceBaseDir = sourceBaseDir.Replace('\\', '/');

                            var rdySourcePath = string.Format("{0}/{1}.rdy", sourceBaseDir, Path.GetFileNameWithoutExtension(remoteFile));
                            var xmlSourcePath = string.Format("{0}/{1}.xml", sourceBaseDir, Path.GetFileNameWithoutExtension(remoteFile));

                            var pdfTargetPath = string.Format("{0}/{1}", remoteDirPath, Path.GetFileName(pdfSourcePath));
                            var rdyTargetPath = string.Format("{0}/{1}", remoteDirPath, Path.GetFileName(rdySourcePath));
                            var xmlTargetpath = string.Format("{0}/{1}", remoteDirPath, Path.GetFileName(xmlSourcePath));

                            if (session.FileExists(pdfSourcePath))
                            {
                                session.MoveFile(pdfSourcePath, pdfTargetPath);

                            }
                            if (session.FileExists(rdySourcePath))
                            {
                                session.MoveFile(rdySourcePath, rdyTargetPath);

                            }
                            if (session.FileExists(xmlSourcePath))
                            {
                                session.MoveFile(xmlSourcePath, xmlTargetpath);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(_logService, _logModel, applicationId, "File Archiving failed due to following exception.");
                            LogHelper.Error(_logService, _logModel, applicationId, ex.Message, ex: ex);
                            moveFailedFiles.Add(remoteFile);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logService, _logModel, applicationId, ex.Message, ex: ex);
            }
            //if any file was not moved by Move file command try to move by downloading/deleting/uploading the file
            if (moveFailedFiles.Any())
            {
                LogHelper.Info(_logService, _logModel, applicationId, "Some files could not be moved by Move file command.Trying to Move files by locally downloading them.");
                MoveRemoteFilesByDelete(moveFailedFiles, remoteDirPath, localDirPathTemp, applicationId);
            }
            return true;
        }

        private bool MoveRemoteFilesByDelete(List<string> remoteFiles, string remoteDirPath, string localDirPathTemp, int applicationId)
        {
            if (remoteFiles.Count == 0)
                return true;
            try
            {
                using (var session = new Session())
                {
                    session.Open(_sessionOption);

                    foreach (var remoteFile in remoteFiles)
                    {
                        var pdfSourcePath = remoteFile;
                        var sourceBaseDir = Path.GetDirectoryName(remoteFile);
                        sourceBaseDir = sourceBaseDir.Replace('\\', '/');

                        var rdySourcePath = string.Format("{0}/{1}.rdy", sourceBaseDir, Path.GetFileNameWithoutExtension(remoteFile));
                        var xmlSourcePath = string.Format("{0}/{1}.xml", sourceBaseDir, Path.GetFileNameWithoutExtension(remoteFile));

                        var pdfLocalPath = string.Format("{0}\\{1}", localDirPathTemp, Path.GetFileName(pdfSourcePath));
                        var rdyLocalPath = string.Format("{0}\\{1}", localDirPathTemp, Path.GetFileName(rdySourcePath));
                        var xmlLocalpath = string.Format("{0}\\{1}", localDirPathTemp, Path.GetFileName(xmlSourcePath));

                        var pdfTargetPath = string.Format("{0}/{1}", remoteDirPath, Path.GetFileName(pdfSourcePath));
                        var rdyTargetPath = string.Format("{0}/{1}", remoteDirPath, Path.GetFileName(rdySourcePath));
                        var xmlTargetpath = string.Format("{0}/{1}", remoteDirPath, Path.GetFileName(xmlSourcePath));

                        TransferOptions transferOptions = new TransferOptions();
                        transferOptions.ResumeSupport.State = _resumeSupport.ToLower() == "off" ? TransferResumeSupportState.Off : TransferResumeSupportState.On;

                        try
                        {
                            if (session.FileExists(pdfSourcePath))
                            {
                                session.GetFiles(pdfSourcePath, pdfLocalPath, true).Check(); // True signifies that file will be deleted from source after move
                                session.PutFiles(pdfLocalPath, pdfTargetPath, true, transferOptions).Check();
                            }
                            if (session.FileExists(rdySourcePath))
                            {
                                session.GetFiles(rdySourcePath, rdyLocalPath, true).Check();
                                session.PutFiles(rdyLocalPath, rdyTargetPath, true, transferOptions).Check();
                            }
                            if (session.FileExists(xmlSourcePath))
                            {
                                session.GetFiles(xmlSourcePath, xmlLocalpath, true).Check();
                                session.PutFiles(xmlLocalpath, xmlTargetpath, true, transferOptions).Check();
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(_logService, _logModel, applicationId, "File Archiving failed due to following exception.");
                            LogHelper.Error(_logService, _logModel, applicationId, ex.Message, ex: ex);
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logService, _logModel, applicationId, ex.Message, ex: ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Downloads a remote file
        /// </summary>
        /// <param name="localDirPath">Local path</param>
        /// <param name="remotePath">remote file path</param>
        /// <param name="fileDownloadLimit">Maximum no of the remote file to download</param>
        /// <returns></returns>
        private List<FTPDownloadStatus> DownLoadFromFtp(string localDirPath, string remotePath, int? fileDownloadLimit, int applicationId)
        {
            List<string> remotePdfFileNames = new List<string>();
            List<FTPDownloadStatus> downloadStatusList = new List<FTPDownloadStatus>();

            //If download limit is set to 0, then no need to continue
            if (fileDownloadLimit.HasValue && fileDownloadLimit.Value == 0)
            {
                return downloadStatusList;
            }

            try
            {
                using (var session = new Session())
                {
                    session.Open(_sessionOption);
                    if (!session.FileExists(remotePath))
                    {
                        LogHelper.Error(_logService, _logModel, applicationId, "No files found in remote SFTP path");
                        return downloadStatusList;
                    }

                    //Find all the RDY files in the remote previous month
                    IEnumerable<RemoteFileInfo> fileInfos = session.ListDirectory(remotePath).Files;

                    if (fileInfos.Count<RemoteFileInfo>() == 0)
                    {
                        return downloadStatusList;
                    }

                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    transferOptions.ResumeSupport.State = _resumeSupport.ToLower() == "off" ? TransferResumeSupportState.Off : TransferResumeSupportState.On;

                    //get a list of all pdf files based on rdy files
                    TransferOperationResult transferResult;
                    foreach (RemoteFileInfo fileInfo in fileInfos)
                    {
                        if (Path.GetExtension(fileInfo.Name).ToLower() == ".rdy")
                        {
                            if (fileDownloadLimit.HasValue && remotePdfFileNames.Count == fileDownloadLimit.Value)
                            {
                                break;
                            }
                            remotePdfFileNames.Add(string.Format("{0}.pdf", Path.GetFileNameWithoutExtension(fileInfo.Name)));
                        }
                    }

                    //if pdf files are there in remote then download
                    foreach (var remotePdfFileName in remotePdfFileNames)
                    {
                        var remotePdfFilePath = string.Format("{0}/{1}", remotePath, remotePdfFileName);
                        var localPdfpath = string.Format("{0}\\{1}", localDirPath, remotePdfFileName);
                        if (session.FileExists(remotePdfFilePath))
                        {
                            try
                            {
                                transferResult = session.GetFiles(remotePdfFilePath, localPdfpath, false, transferOptions);
                                transferResult.Check();
                                downloadStatusList.Add(new FTPDownloadStatus(remotePdfFilePath, true, string.Empty, localPdfpath));
                            }
                            catch (Exception ex)
                            {
                                downloadStatusList.Add(new FTPDownloadStatus(remotePdfFilePath, false, ex.Message, string.Empty));
                                //Log the error but continue downloading with other files

                                LogHelper.Error(_logService, _logModel, applicationId, ex.Message, ex: ex);
                                LogHelper.Error(_logService, _logModel, applicationId, "SFTP Download Failed: " + remotePdfFilePath);
                                continue;
                            }
                        }
                        else
                        {
                            downloadStatusList.Add(new FTPDownloadStatus(remotePdfFilePath, false, "rdy file exists but corresponding pdf does not exist.", string.Empty));
                            LogHelper.Info(_logService, _logModel, applicationId, "SFTP Download Failed. rdy file exists but corresponding pdf does not exist. " + remotePdfFilePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                LogHelper.Error(_logService, _logModel, applicationId, ex.Message, ex: ex);
            }
            return downloadStatusList;
        }

        public void Close()
        {
            //Intentionally left blank
        }

        public string GetCustomErrorMessage(Exception ex)
        {
            if (ex is SessionException)
            {
                if (ex.Message.Contains("Software caused connection abort"))
                {
                    return SFTPLibraryConstant.NetworkIssueMsg;
                }
                if (ex.Message.Contains("Access denied"))
                {
                    return SFTPLibraryConstant.AceessDeniedMsg;
                }
            }
            return ex.Message;
        }
    }
}
