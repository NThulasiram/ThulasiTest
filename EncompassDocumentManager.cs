using EllieMae.Encompass.BusinessObjects.Loans;
using EllieMae.Encompass.BusinessObjects.Loans.Logging;
using EllieMae.Encompass.Client;
using EllieMae.Encompass.Collections;
using EncompassLibrary.Properties;
using EncompassLibrary.Utilities;
using FGMC.Common.DataContract;
using Log4NetLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EllieMae.Encompass.BusinessObjects.Loans.Templates;

namespace EncompassLibrary.DocumentManager
{
	public interface IEncompassDocumentManager
	{
	    IEnumerable<LoanLogicsFile> GetFilteredLoanDocuments(CorrespondentLoanConfigInfo correspondentLoanConfigInfo, NonCorrespondentLoanConfigInfo nonCorrespondentLoanConfigInfo, string loanNumber, long batchId, long batchDetailId, int configurationId, Session session, int applicationId = 0);
	}
    public class EncompassDocumentManager: IEncompassDocumentManager
    {
		private readonly ILoggingService _logService = null;
        private  LogModel _logModel = null;

        public EncompassDocumentManager()
        {
            _logService = new FileLoggingService(typeof(EncompassDocumentManager));
            _logModel= new LogModel();
        }

        public IEnumerable<LoanLogicsFile> GetFilteredLoanDocuments(CorrespondentLoanConfigInfo correspondentLoanConfigInfo,NonCorrespondentLoanConfigInfo nonCorrespondentLoanConfigInfo,string loanNumber, long batchId, long batchDetailId,  int configurationId, Session session,int applicationId = 0)
        {
            var loanGuid = Common.GetGuid(loanNumber, session);
            if (string.IsNullOrEmpty(loanGuid))
                throw new LoanNotFoundException($"loan# {loanNumber} does not exist in the system.");
            var loan = session.Loans.Open(loanGuid);
            if (loan.Fields["2626"].Value.ToString() == "Correspondent")
            {
                return GetAttachmentsForCorrespondentLoans(loan, correspondentLoanConfigInfo, batchId, batchDetailId, configurationId, applicationId);
            }
            return GetAttachmentsForNonCorrespondentLoans(loan, nonCorrespondentLoanConfigInfo, batchId, batchDetailId, configurationId, applicationId);
        }

        private IEnumerable<LoanLogicsFile> GetAttachmentsForNonCorrespondentLoans(Loan loan, NonCorrespondentLoanConfigInfo nonCorrespondentLoanConfigInfo, long batchId, long batchDetailId, int configurationId,int applicationId = 0)
        {
            var fileDetails = new List<LoanLogicsFile>();
            var documents = loan.Log.TrackedDocuments;
            if (documents == null)
                return null;
           
            var placeholders = documents.Cast<TrackedDocument>();
            //remove excluded placeholders
			var filteredPlaceholders = placeholders.Where(o => !nonCorrespondentLoanConfigInfo.ExcludedPlaceholders.Contains(o.Title.RemoveWhitespaceAndToLower(), StringComparer.OrdinalIgnoreCase));

            //below scenario not there in FSD
            List<TrackedDocument> placeholdersToConsider = new List<TrackedDocument>();
            foreach (var placeholder in filteredPlaceholders)
            {
                //remove excludedStartsWithPlaceHolders
                var validPlaceholder = (from excludedStartsWithPlaceHolder in nonCorrespondentLoanConfigInfo.ExcludeStartsWithPlaceholders
                                        where !placeholder.Title.RemoveWhitespaceAndToLower().StartsWith(excludedStartsWithPlaceHolder.RemoveWhitespaceAndToLower())
                                        select placeholder);
                if (validPlaceholder != null && validPlaceholder.Any())
                {
                    placeholdersToConsider.Add(placeholder);
                }
            }

            foreach (var placeholder in filteredPlaceholders)
            {
				var attachments = GetDocsFromNonCorrespondentLoanPlaceholders(nonCorrespondentLoanConfigInfo,loan.LoanNumber, batchId, batchDetailId, placeholder, configurationId, applicationId);
				if(attachments != null&& attachments.Any())
				{
				    fileDetails.AddRange(attachments);
				}
			}
            return fileDetails;
        }

        private IEnumerable<LoanLogicsFile> GetAttachmentsForCorrespondentLoans(Loan loan, CorrespondentLoanConfigInfo correspondentLoanConfigInfo, long batchId, long batchDetailId, int configurationId, int applicationId = 0)
        {
            if (IsDelegatedLoan(loan, correspondentLoanConfigInfo))
            {
              return  GetAttachmentsForDelegatedLoans(loan, correspondentLoanConfigInfo, batchId, batchDetailId, configurationId, applicationId);
            }
            if (IsNonDelegatedLoan(loan, correspondentLoanConfigInfo))
            {
                return GetAttachmentsForNonDelegatedLoans(loan, correspondentLoanConfigInfo, batchId, batchDetailId, configurationId, applicationId);
            }
            //temporary: default correspondent loan type delegated loans
            return GetAttachmentsForDelegatedLoans(loan, correspondentLoanConfigInfo, batchId, batchDetailId, configurationId, applicationId);
        }

        private IEnumerable<LoanLogicsFile>  GetAttachmentsForDelegatedLoans(Loan loan, CorrespondentLoanConfigInfo correspondentLoanConfigInfo, long batchId, long batchDetailId, int configurationId, int applicationId)
        {
            List<LoanLogicsFile> allAttachments = new List<LoanLogicsFile>();
            var fileSaveLocation = GetTempPath(loan.LoanNumber);
            var currentVersionAttachmets = GetCurrentVersionAttachments(loan, correspondentLoanConfigInfo.DelegateCurrentVersionPlaceholder, batchId, batchDetailId, configurationId, fileSaveLocation, applicationId);
            if (currentVersionAttachmets != null && currentVersionAttachmets.Any())
            {
                allAttachments.AddRange(currentVersionAttachmets);
            }
            var loanNumberTitleAttachmets = GetloanNumberTitleAttachmentsFromCurrentVersion(loan, correspondentLoanConfigInfo.DelegateLoannumberTitlePlaceholder, batchId, batchDetailId, configurationId, fileSaveLocation, applicationId).ToList();
            if (loanNumberTitleAttachmets != null && loanNumberTitleAttachmets.Any())
            {
                allAttachments.AddRange(loanNumberTitleAttachmets);
            }

            return allAttachments;
        }

        private IEnumerable<LoanLogicsFile> GetAttachmentsForNonDelegatedLoans(Loan loan, CorrespondentLoanConfigInfo correspondentLoanConfigInfo, long batchId, long batchDetailId, int configurationId,  int applicationId)
        {
            List<LoanLogicsFile> allAttachments = new List<LoanLogicsFile>();
            var fileSaveLocation = GetTempPath(loan.LoanNumber);
            var currentVersionAttachmets = GetCurrentVersionAttachments(loan, correspondentLoanConfigInfo.NonDelegateCurrentVersionPlaceholder, batchId, batchDetailId, configurationId, fileSaveLocation, applicationId);
            if (currentVersionAttachmets != null && currentVersionAttachmets.Any())
            {
                allAttachments.AddRange(currentVersionAttachmets);
            }
            var loanNumberTitleAttachmets = GetloanNumberTitleAttachmentsFromCurrentVersion(loan, correspondentLoanConfigInfo.NonDelegateLoannumberTitlePlaceholder, batchId, batchDetailId, configurationId, fileSaveLocation, applicationId).ToList();
            if (loanNumberTitleAttachmets != null && loanNumberTitleAttachmets.Any())
            {
                allAttachments.AddRange(loanNumberTitleAttachmets);
            }

            return allAttachments;
        }

        private IEnumerable<LoanLogicsFile> GetloanNumberTitleAttachmentsFromCurrentVersion(Loan loan,List<string> placeholderNames, long batchId, long batchDetailId, int configurationId, string fileSaveLocation, int applicationId)
        {
            List<LoanLogicsFile> downloadedAttachments = new List<LoanLogicsFile>();
            foreach (var placeholderName in placeholderNames)
            {
              var trackedDocuments=  loan.Log.TrackedDocuments.GetDocumentsByTitle(placeholderName);
                List<Attachment> filterAttachments= new List<Attachment>();
                foreach (TrackedDocument trackedDocument in trackedDocuments)
                {
                   var attachments=  trackedDocument.GetAttachments();
                    foreach (Attachment attachment in attachments)
                    {
                        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(attachment.Title);
                        var fileExtension = Path.GetExtension(attachment.Name);
                        fileExtension = string.IsNullOrEmpty(fileExtension) ? Path.GetExtension(attachment.Title) : fileExtension;

                        if (fileNameWithoutExtension == loan.LoanNumber && fileExtension.ToLower()==".pdf")
                        {
                            filterAttachments.Add(attachment);
                        }
                    }
                    downloadedAttachments.AddRange(SaveAttachments(filterAttachments, trackedDocument, loan.LoanNumber, batchId, batchDetailId, configurationId, fileSaveLocation, applicationId));
                }
            }
            return downloadedAttachments;
        }

        private IEnumerable<LoanLogicsFile> GetCurrentVersionAttachments(Loan loan, List<string> placeholderNames, long batchId, long batchDetailId, int configurationId, string fileSaveLocation, int applicationId)
        {
            List<LoanLogicsFile> downloadedAttachments = new List<LoanLogicsFile>();
            foreach (var placeholderName in placeholderNames)
            {
                var trackedDocuments = loan.Log.TrackedDocuments.GetDocumentsByTitle(placeholderName);
                List<Attachment> filterAttachments = new List<Attachment>();
                foreach (TrackedDocument trackedDocument in trackedDocuments)
                {
                    var attachments = trackedDocument.GetAttachments();
                    filterAttachments.AddRange(attachments.Cast<Attachment>());
                    downloadedAttachments.AddRange(SaveAttachments(filterAttachments, trackedDocument, loan.LoanNumber, batchId, batchDetailId, configurationId, fileSaveLocation, applicationId));
                }
            }
            return downloadedAttachments;
        }

        private bool IsDelegatedLoan(Loan loan, CorrespondentLoanConfigInfo correspondentLoanConfigInfo)
        {
            var channelSource = loan.Fields["CUST03FV"].Value;
            return correspondentLoanConfigInfo.DelegateLoanChannelSource.Count(o => o.Equals(channelSource))>0;
        }
        private bool IsNonDelegatedLoan(Loan loan, CorrespondentLoanConfigInfo correspondentLoanConfigInfo)
        {
            var channelSource = loan.Fields["CUST03FV"].Value;
            return correspondentLoanConfigInfo.NonDelegateLoanChannelSource.Count(o => o.Equals(channelSource)) > 0;
        }

        private IEnumerable<LoanLogicsFile> GetDocsFromNonCorrespondentLoanPlaceholders(NonCorrespondentLoanConfigInfo nonCorrespondentLoanConfigInfo, string loanNumber, long batchId, long batchDetailId, TrackedDocument placeholder, int configurationId,int applicationId)
		{
			var allAttachments = placeholder.GetAttachments();
			AttachmentList attachments = (allAttachments != null && allAttachments.Count > 0) ? allAttachments : null;
			if (attachments == null) return null;
			var fileSaveLocation = GetTempPath(loanNumber);
            List<Attachment> filterAttachments = new List<Attachment>();

            foreach (Attachment attachment in attachments)
			{
				if (attachment != null)
				{
                    if (placeholder.Title.RemoveWhitespaceAndToLower().Equals(nonCorrespondentLoanConfigInfo.StackedPlaceholder))
					{
						if (attachment.Title.RemoveWhitespaceAndToLower().Contains(nonCorrespondentLoanConfigInfo.StakcedPlaceholderAttachmentName))
						{
                            filterAttachments.Add(attachment);
                        }
					}
					else
					{
                        filterAttachments.Add(attachment);
                    }
				}
			}
            return SaveAttachments(filterAttachments, placeholder, loanNumber, batchId, batchDetailId, configurationId, fileSaveLocation, applicationId);
		}

        private List<LoanLogicsFile> SaveAttachments(List<Attachment> attachmentList,TrackedDocument placeholder, string loanNumber, long batchId, long batchDetailId, int configurationId,string saveFileLocation, int applicationId)
        {
            List<LoanLogicsFile> loanLogicsFileList = new List<LoanLogicsFile>();

            foreach (var attachment in attachmentList)
            {
                var fileDetail = new LoanLogicsFile();
                fileDetail.PlaceholderName = placeholder.Title;
                fileDetail.Title = attachment.Title;
                fileDetail.AttchmentName = attachment.Name;
                fileDetail.LoanNumber = loanNumber;
                fileDetail.OriginalFileName = attachment.Title;
                fileDetail.DocumentExtractBatchDetailId = batchDetailId;
                fileDetail.RevisedFileName = attachment.Name;
                fileDetail.ConfigurationId = configurationId;
                fileDetail.DocumentExtractBatchDetailId = batchDetailId;
                fileDetail.DocumentExtractBatchId = batchId;
                fileDetail.IsActive = true;
                fileDetail.FullFilePath = saveFileLocation + "\\" + FormatFileName(attachment, placeholder.Title, applicationId);
                fileDetail.FileSize = ConvertBytesToKiloBytes(attachment.Size);

                try
                {

                    attachment.SaveToDisk(fileDetail.FullFilePath);
                    fileDetail.IsSuccess = true;
                }
                catch (Exception ex)
                {
                    fileDetail.IsSuccess = false;
                    fileDetail.Reason = ex.Message;
                    LogHelper.Error(_logService, _logModel, applicationId, ex.Message, loanNumber, ex);
                }
                loanLogicsFileList.Add(fileDetail);
            }

            return loanLogicsFileList;
        }
        private string GetTempPath(string loanNumber)
        {
			var localPathOfFilesStored = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location.Replace("\\bin\\Debug", "")) + "\\Export" + "\\" + loanNumber;
            if (!Directory.Exists(localPathOfFilesStored))
                Directory.CreateDirectory(localPathOfFilesStored);
            return localPathOfFilesStored;
        }
        private string FormatFileName(Attachment attachment,string placeHolderName,int applicationId)
        {
            var attFileNameWithoutExtension = Path.GetFileNameWithoutExtension(attachment.Name);
            var attExtension = Path.GetExtension(attachment.Name);
            attExtension = string.IsNullOrEmpty(attExtension) ? Path.GetExtension(attachment.Title) : attExtension;
            if(string.IsNullOrEmpty(attExtension))
            {
                LogHelper.Error(_logService,_logModel, applicationId, $"No Attachment Extension. Attachment Name: {attachment.Name} :: Attachment Title: {attachment.Title} :: Placeholder Name: {placeHolderName}");
            }
            var fileName = attFileNameWithoutExtension + attExtension;
            return fileName;
        }

        private long ConvertBytesToKiloBytes(long bytes)
        {
            return (bytes / 1000);
        }

    }
}
