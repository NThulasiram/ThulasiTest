using EllieMae.Encompass.Client;
using EllieMae.Encompass.Collections;
using EllieMae.Encompass.Query;
using EllieMae.Encompass.Reporting;
using FGMC.Common.DataContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EllieMae.Encompass.BusinessObjects.Loans;
using EllieMae.Encompass.BusinessObjects.Loans.Logging;
using Log4NetLibrary;
using EllieMae.Encompass.BusinessObjects.Loans.Templates;

namespace EncompassLibrary
{


    public class CorrespondentQA
    {
        private readonly ILoggingService _logService;
        private  LogModel _logModel;

        public CorrespondentQA()
        {
            _logService = new FileLoggingService(typeof(CorrespondentQA));
            _logModel= new LogModel();
        }

        public LoanReportDataList GetLoanReportDataForQACorrespondent(List<string> loanFolders, DateTime closedLoanPackageRecDate, bool sweepComplete, List<string> sourceChannel, Session session, List<string> qaCanonicalNameList, string canonicalSweepCompleteFieldId,int applicationId=0)
        {
            LoanReportDataList loanReportDataList = null;
            string sweepCheck = sweepComplete == false ? "" : "X";
            try
            {
                if (loanFolders.Count <= 0 && sourceChannel.Count <= 0)
                {
                    LogHelper.Error(_logService, _logModel, applicationId, "loanFolders and sourceChannel are mandatory.");
                 
                    return loanReportDataList;
                }

                EllieMae.Encompass.Query.QueryCriterion qcForloanFolder = null;
                foreach (var loanFolder in loanFolders)
                {
                    if (qcForloanFolder == null)
                    {
                        qcForloanFolder = new StringFieldCriterion(EncompassLibraryConstants.CONANICAL_LOANFOLDER
                                                         , loanFolder
                                                         , StringFieldMatchType.Exact
                                                         , true);
                    }
                    else
                    {
                        qcForloanFolder =
                            qcForloanFolder.Or(new StringFieldCriterion(EncompassLibraryConstants.CONANICAL_LOANFOLDER
                                , loanFolder
                                , StringFieldMatchType.Exact
                                , true));
                    }
                }
                EllieMae.Encompass.Query.QueryCriterion qcForRecDate =
                                 new DateFieldCriterion(
                                                           EncompassLibraryConstants.CONANICAL_CLOSEDLOANPAKRECDATE
                                                             , closedLoanPackageRecDate
                                                         , OrdinalFieldMatchType.GreaterThanOrEquals
                                                         , DateFieldMatchPrecision.Exact
                                                         );
                EllieMae.Encompass.Query.QueryCriterion qcForSweepComplete =
                              new StringFieldCriterion(
                                                             canonicalSweepCompleteFieldId
                                                               , sweepCheck
                                                           , StringFieldMatchType.Exact
                                                           , true
                                                           );
                EllieMae.Encompass.Query.QueryCriterion qcForSourceLoan = null;

                foreach (var source in sourceChannel)
                {
                    if (qcForSourceLoan == null)
                    {
                        qcForSourceLoan = new StringFieldCriterion(EncompassLibraryConstants.CONANICAL_CHANNELSOURCE
                                                           , source
                                                           , StringFieldMatchType.Exact
                                                           , true);
                    }
                    else
                    {
                        qcForSourceLoan =
                            qcForSourceLoan.Or(new StringFieldCriterion(EncompassLibraryConstants.CONANICAL_CHANNELSOURCE
                                , source
                                , StringFieldMatchType.Exact
                                , true));

                    }
                }

                QueryCriterion queryCriterion = qcForloanFolder.And(qcForSourceLoan).And(qcForSweepComplete).And(qcForRecDate);
                StringList fieldlist = new StringList();
                foreach (var requiredField in qaCanonicalNameList)
                {
                    fieldlist.Add(requiredField);
                }

                EllieMae.Encompass.Reporting.LoanReportCursor loanReportCursor =
                    session.Reports.OpenReportCursor(fieldlist, queryCriterion);

                loanReportDataList = loanReportCursor.GetItems(0, loanReportCursor.Count);
                return loanReportDataList;
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logService,_logModel, applicationId,ex.Message,ex:ex);
                throw ex;
            }
        }

        public CorrespondentQAResponse FillResponseWithReportData(LoanReportDataList loanReportDataList, string canonicalSweepCompleteFieldId)
        {
            CorrespondentQAResponse correspondentQaResponse = new CorrespondentQAResponse();
            try
            {
                foreach (LoanReportData loaninfo in loanReportDataList)
                {
                    CorrespondentQALoanInfo correspondentQaLoanInfo = new CorrespondentQALoanInfo();

                    if (loaninfo[EncompassLibraryConstants.CONANICAL_LOANNUMBER] != null)
                        correspondentQaLoanInfo.LoanNumber = Convert.ToString(loaninfo[EncompassLibraryConstants.CONANICAL_LOANNUMBER]);
                    if (loaninfo[EncompassLibraryConstants.CONANICAL_LOANFOLDER] != null)
                        correspondentQaLoanInfo.Loanfolder = Convert.ToString(loaninfo[EncompassLibraryConstants.CONANICAL_LOANFOLDER]);
                    if (loaninfo[EncompassLibraryConstants.CONANICAL_BORROWER_FIRSTNAME] != null)
                        correspondentQaLoanInfo.BorrowerFirstName = Convert.ToString(loaninfo[EncompassLibraryConstants.CONANICAL_BORROWER_FIRSTNAME]);
                    if (loaninfo[EncompassLibraryConstants.CONANICAL_BORROWER_LASTNAME] != null)
                        correspondentQaLoanInfo.BorrowerLastName = Convert.ToString(loaninfo[EncompassLibraryConstants.CONANICAL_BORROWER_LASTNAME]);
                    if (loaninfo[EncompassLibraryConstants.CONANICAL_CHANNELSOURCE] != null)
                        correspondentQaLoanInfo.SourceChannel = Convert.ToString(loaninfo[EncompassLibraryConstants.CONANICAL_CHANNELSOURCE]);
                    if (loaninfo[canonicalSweepCompleteFieldId] != null)
                        correspondentQaLoanInfo.SweepComplete = Convert.ToString(loaninfo[canonicalSweepCompleteFieldId]) != string.Empty;

                    if (loaninfo[EncompassLibraryConstants.CONANICAL_GUID] != null)
                        correspondentQaLoanInfo.LoanGuid = Convert.ToString(loaninfo[EncompassLibraryConstants.CONANICAL_GUID]);

                    if (loaninfo[EncompassLibraryConstants.CONANICAL_CLOSEDLOANPAKRECDATE] != null)
                        correspondentQaLoanInfo.PackageReceivedDate = loaninfo[EncompassLibraryConstants.CONANICAL_CLOSEDLOANPAKRECDATE] == string.Empty
                        ? DateTime.MinValue
                        : Convert.ToDateTime(loaninfo[EncompassLibraryConstants.CONANICAL_CLOSEDLOANPAKRECDATE]);
                    correspondentQaLoanInfo.ExtractStatusId = Convert.ToInt32(DocumentExtractStatus.SUBMIT);
                    correspondentQaResponse.Loansinfo.Add(correspondentQaLoanInfo);
                }

                return correspondentQaResponse;
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logService, _logModel, EncompassLibraryConstants.CORRESPONDENTQA_APPLICATION_ID, ex.Message, ex:ex);
             
                throw ex;
            }
        }

        public EncompassFileUploadStatus UploadFileToEncompassEFolder(string filepath, string placeholderName, Session session, int applicationId)
        {
            const string loanNotExistMsg = "Loan # doesn't exists in the system.";
            const string multipleLoanExistMsg = "Multiple loans exist in the system for the same Loan #";

            EncompassFileUploadStatus uploadStatus = new EncompassFileUploadStatus();
            uploadStatus.ChangedLoanNumber = string.Empty;
            uploadStatus.ErrorMessage = string.Empty;
            uploadStatus.EncompassUploadStatus = CorrespondentLoanEncompassUploadStatus.FAIL;

            Loan loan = null;

            try
            {
                var file = Path.GetFileNameWithoutExtension(filepath);
                var loanNumber = Path.GetFileName(file);
                string guid = EncompassLibrary.GetLoanGuidFromLoanNumber(loanNumber, session);
                //This Change is required because sometimes loan # can be changed in the E360. System may have uploaded a loan file 200012345678.pdf to FTP but
                //latter Loan # 200012345678 gets changed to 400012345678. In that case take the last 8 digits and search the loan.
                if (string.IsNullOrEmpty(guid.Trim()))
                {
                    //take the last 8 digits and search the loan.
                    if(loanNumber.Length < 12)
                    {
                        uploadStatus.ErrorMessage = loanNumber + " is not a valid Loan #. Loan # has to be 12 digits at least.";
                        uploadStatus.EncompassUploadStatus = CorrespondentLoanEncompassUploadStatus.LOAN_NOT_FOUND;
                        LogHelper.Error(_logService, _logModel, applicationId, uploadStatus.ErrorMessage);
                        return uploadStatus;
                    }
                    var newLoanNumber = loanNumber.Substring(4, 8);
                    List<string> loanGuids = EncompassLibrary.GetCorrespondentLoanGuidFromLoanNumber(newLoanNumber, session);
                    if(loanGuids.Count == 0)
                    {
                        LogHelper.Error(_logService, _logModel, applicationId, loanNumber + " " + loanNotExistMsg);
                        uploadStatus.ErrorMessage = loanNotExistMsg;
                        uploadStatus.EncompassUploadStatus = CorrespondentLoanEncompassUploadStatus.LOAN_NOT_FOUND;
                        return uploadStatus;
                    }
                    else if (loanGuids.Count > 1)
                    {
                        uploadStatus.ErrorMessage = multipleLoanExistMsg + " " + newLoanNumber;
                        LogHelper.Error(_logService, _logModel, applicationId, uploadStatus.ErrorMessage);
                        uploadStatus.EncompassUploadStatus = CorrespondentLoanEncompassUploadStatus.MULTIPLE_LOANS_FOUND;
                        return uploadStatus;
                    }
                    else
                    {
                        loan = EncompassLibrary.OpenLoan(loanGuids[0], session);
                        uploadStatus.ChangedLoanNumber = loan.LoanNumber;
                        LogHelper.Info(_logService, _logModel, applicationId, string.Format("Loan # is changed in the system. Old Loan #: {0} New Loan #: {1}", loanNumber, uploadStatus.ChangedLoanNumber));
                    }
                }
                else
                {
                    loan = EncompassLibrary.OpenLoan(guid, session);
                }
                if (loan != null)
                {
                    LoanLock lockInfo = loan.GetCurrentLock();
                    //Check for lock lock
                    if (lockInfo == null)
                    {
                        LogEntryList documentList = loan.Log.TrackedDocuments.GetDocumentsByTitle(placeholderName);
                        Attachment att = loan.Attachments.Add(filepath);
                        // Now attach the new Attachment to the Appraisal on the loan
                        loan.Lock();
                        att.Title = loanNumber + ".pdf";//Keep the old loan number file name, no need to change the file name to New loan #, confirmed with Manoj
                        if (documentList.Count > 0)
                        {
                            TrackedDocument appraisal = (TrackedDocument)documentList[0];
                            appraisal.Attach(att);
                        }
                        else
                        {
                            DocumentTemplate docTemplate = session.Loans.Templates.Documents.GetTemplateByTitle(placeholderName);
                            TrackedDocument appraisal = loan.Log.TrackedDocuments.AddFromTemplate(docTemplate, session.Loans.Milestones.Processing.Name);
                            appraisal.Attach(att);
                        }
                        loan.Commit();
                        uploadStatus.EncompassUploadStatus = CorrespondentLoanEncompassUploadStatus.COMPLETE;
                    }
                    else
                    {
                        string lockMessage = string.Format("Loan # {0} is locked by {1}", loan.LoanNumber, lockInfo.LockedBy);
                        LogHelper.Error(_logService, _logModel, applicationId, lockMessage);
                        uploadStatus.ErrorMessage = lockMessage;
                        uploadStatus.EncompassUploadStatus = CorrespondentLoanEncompassUploadStatus.LOAN_LOCKED;
                        return uploadStatus;
                    }
                }
                else
                {
                    LogHelper.Error(_logService, _logModel, applicationId, loanNumber + " " + loanNotExistMsg);
                    uploadStatus.ErrorMessage = loanNotExistMsg;
                    uploadStatus.EncompassUploadStatus = CorrespondentLoanEncompassUploadStatus.LOAN_NOT_FOUND;
                    return uploadStatus;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logService, _logModel, applicationId, ex.Message,ex:ex);
                uploadStatus.ErrorMessage = ex.Message;
                uploadStatus.EncompassUploadStatus = CorrespondentLoanEncompassUploadStatus.FAIL;
            }
            finally
            {
                if (loan != null)
                {
                    loan.Close();
                }
            }
            return uploadStatus;       
        }


        private void AddBatchDetailToList(ref List<CorrespondentQALoanInfo> loansInfoList, string loanNumber,
            bool sweepComplete, int extractStatusId)
        {
            CorrespondentQALoanInfo loansinfo = new CorrespondentQALoanInfo();
            loansinfo.LoanNumber = loanNumber;
            loansinfo.SweepComplete = sweepComplete;
            var data = GetLoanDataFromReportDB(new List<string>() { loanNumber });
            loansinfo.BorrowerFirstName = data.BorrowerFirstName;
            loansinfo.BorrowerLastName = data.BorrowerLastName;
            loansinfo.Loanfolder = data.Loanfolder;
            loansinfo.PackageReceivedDate = data.PackageReceivedDate;
            loansinfo.UploadDate = DateTime.Now;
            loansinfo.ExtractStatusId = extractStatusId;
            loansInfoList.Add(loansinfo);
        }

        public static CorrespondentQALoanInfo GetLoanDataFromReportDB(List<string> loanNumbers)
        {
            CorrespondentQALoanInfo correspondentQALoanInfo = new CorrespondentQALoanInfo();
            List<string> fieldsList = new List<string>();
            fieldsList.Add("Fields.4000");//Borrower First Name
            fieldsList.Add("Fields.4002");//Borrower Last Name
            fieldsList.Add("Fields.LOANFOLDER");//Loan folder
            fieldsList.Add("Fields.CX.CORR.CLPKGRECDATE");//Package received date            

            EncompassSession EncomSession = new EncompassSession(EncompassLibraryConstants.CORRESPONDENTQA_APPLICATION_ID);
            var LoanReportDataList = EncomSession.GetReportFiedValuesByLoanNumbers(loanNumbers, fieldsList);
            foreach (LoanReportData data in LoanReportDataList)
            {
                correspondentQALoanInfo.BorrowerFirstName = data["Fields.4000"].ToString();
                correspondentQALoanInfo.BorrowerLastName = data["Fields.4002"].ToString();
                correspondentQALoanInfo.Loanfolder = data["Fields.LOANFOLDER"].ToString();

                if (data["Fields.CX.CORR.CLPKGRECDATE"] != null)
                {
                    correspondentQALoanInfo.PackageReceivedDate = correspondentQALoanInfo.PackageReceivedDate = Convert.ToDateTime(data["Fields.CX.CORR.CLPKGRECDATE"]);
                }
                else
                {
                    correspondentQALoanInfo.PackageReceivedDate = DateTime.Now;
                }
            }
            return correspondentQALoanInfo;
        }

        public IEnumerable<CorrespondentQAPlaceHoder> GetAttachmentsFromPlaceHoder(ExtractAttachmentDetails extractAttachmentDetails, Session session)
        {
            if (extractAttachmentDetails.IsCurrentVersion)
            {
                return GetCurrentVersionAttachments(extractAttachmentDetails, session);
            }
            else
            {
                return GetAllVersionsAttachments(extractAttachmentDetails, session);
            }
        }

        private IEnumerable<CorrespondentQAPlaceHoder> GetCurrentVersionAttachments(ExtractAttachmentDetails extractAttachmentDetails, Session session)
        {
            List<CorrespondentQAPlaceHoder> correspondentQAPlaceHoderList = new List<CorrespondentQAPlaceHoder>();
            try
            {
                if (!string.IsNullOrEmpty(extractAttachmentDetails.LoanNumber))
                {
                    var guid = extractAttachmentDetails.Guid;
                    if (!string.IsNullOrEmpty(guid))
                    {
                        var loan = EncompassLibrary.OpenLoan(guid, session);

                        foreach (var documentTitle in extractAttachmentDetails.Placeholders)
                        {
                            LogEntryList trackedDocuments = loan.Log.TrackedDocuments.GetDocumentsByTitle(documentTitle);
                            if (trackedDocuments != null)
                            {
                                foreach (TrackedDocument trackedDocument in trackedDocuments)
                                {
                                    CorrespondentQAPlaceHoder qAPlaceHoder = new CorrespondentQAPlaceHoder();
                                    qAPlaceHoder.PlaceholderName = documentTitle;
                                    qAPlaceHoder.IsActive = true;
                                    AttachmentList attachmentList = trackedDocument.GetAttachments();
                                    foreach (Attachment attachment in attachmentList)
                                    {
                                        correspondentQAAttachment qAAttachment = new correspondentQAAttachment();
                                        try
                                        {
                                            var attachmentExtension = Path.GetExtension(attachment.Name);
                                            var attachmentFileNameWithoutExtension = Path.GetFileNameWithoutExtension(attachment.Name);

                                            if (string.IsNullOrEmpty(attachmentExtension))
                                            {
                                                attachmentExtension = Path.GetExtension(attachment.Title);
                                            }
                                            // var validFileName = Path.GetInvalidFileNameChars().Aggregate(attachmentFileNameWithoutExtension + attachmentExtension, (current, c) => current.Replace(c.ToString(), string.Empty));
                                            var validFileName = attachmentFileNameWithoutExtension + attachmentExtension;
                                            var filepath = Path.Combine(extractAttachmentDetails.SavePath, validFileName);
                                            qAAttachment.AttachmentFullPath = filepath;
                                            qAAttachment.AttachmentTitle = attachment.Title;
                                            qAAttachment.FileSize = attachment.Size;
                                            qAAttachment.IsActive = true;
                                            qAAttachment.PlaceholderName = trackedDocument.Title;
                                            qAAttachment.AttachmentName = attachment.Name;
                                            attachment.SaveToDisk(filepath);
                                            qAAttachment.IsSuccess = true;
                                        }
                                        catch (Exception ex)
                                        {
                                            qAAttachment.IsSuccess = false;
                                            qAAttachment.FailureReason = ex.Message;
                                            LogHelper.Error(_logService,_logModel,EncompassLibraryConstants.CORRESPONDENTQA_APPLICATION_ID, attachment.Title + " Attachment download failed :" + ex.Message + " " + ex.StackTrace);
                                        }
                                        qAPlaceHoder.Attachments.Add(qAAttachment);
                                    }
                                    if (qAPlaceHoder.Attachments.Count > 0)
                                    {
                                        correspondentQAPlaceHoderList.Add(qAPlaceHoder);
                                    }
                                }
                            }
                        }
                        return correspondentQAPlaceHoderList;
                    }
                    else
                    {
                        throw new Exception(extractAttachmentDetails.LoanNumber + " not exists in system.");
                    }
                }
                else
                {
                    throw new Exception("LoanNumber is mandatory.");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logService, _logModel, EncompassLibraryConstants.CORRESPONDENTQA_APPLICATION_ID,  ex.Message , ex:ex);
                throw ex;
            }
        }

        private IEnumerable<CorrespondentQAPlaceHoder> GetAllVersionsAttachments(ExtractAttachmentDetails extractAttachmentDetails, Session session)
        {
            List<CorrespondentQAPlaceHoder> correspondentQAPlaceHoderList = new List<CorrespondentQAPlaceHoder>();
            try
            {
                if (!string.IsNullOrEmpty(extractAttachmentDetails.LoanNumber))
                {
                    var guid = extractAttachmentDetails.Guid;
                    if (!string.IsNullOrEmpty(guid))
                    {
                        var loan = EncompassLibrary.OpenLoan(guid, session, EncompassLibraryConstants.CORRESPONDENTQA_APPLICATION_ID);

                        foreach (var documentTitle in extractAttachmentDetails.Placeholders)
                        {
                            LogEntryList trackedDocuments = loan.Log.TrackedDocuments.GetDocumentsByTitle(documentTitle);
                            if (trackedDocuments != null)
                            {
                                foreach (TrackedDocument trackedDocument in trackedDocuments)
                                {
                                    CorrespondentQAPlaceHoder qAPlaceHoder = new CorrespondentQAPlaceHoder();
                                    qAPlaceHoder.PlaceholderName = documentTitle;
                                    qAPlaceHoder.IsActive = true;

                                    //loop through attachments

                                    foreach (Attachment attachment in loan.Attachments)
                                    {
                                        TrackedDocument doc = attachment.GetDocument();
                                        if (doc == null || doc.Title != documentTitle)
                                            continue;

                                        //qAPlaceHoder.PlaceholderName = trackedDocument.Title;

                                        correspondentQAAttachment qAAttachment = new correspondentQAAttachment();
                                        try
                                        {
                                            var attachmentExtension = Path.GetExtension(attachment.Name);
                                            var attachmentFileNameWithoutExtension = Path.GetFileNameWithoutExtension(attachment.Name);

                                            if (string.IsNullOrEmpty(attachmentExtension))
                                            {
                                                attachmentExtension = Path.GetExtension(attachment.Title);
                                            }
                                            //change on 14/2/17, now we saving file with GUID
                                            // var validFileName = Path.GetInvalidFileNameChars().Aggregate(attachmentFileNameWithoutExtension + attachmentExtension, (current, c) => current.Replace(c.ToString(), string.Empty));
                                            var validFileName = attachmentFileNameWithoutExtension + attachmentExtension;
                                            var filepath = Path.Combine(extractAttachmentDetails.SavePath, validFileName);
                                            qAAttachment.AttachmentFullPath = filepath;
                                            qAAttachment.AttachmentTitle = attachment.Title;
                                            qAAttachment.FileSize = attachment.Size;
                                            qAAttachment.IsActive = true;
                                            qAAttachment.PlaceholderName = trackedDocument.Title;
                                            qAAttachment.AttachmentName = attachment.Name;
                                            attachment.SaveToDisk(filepath);
                                            qAAttachment.IsSuccess = true;
                                        }
                                        catch (Exception ex)
                                        {
                                            qAAttachment.IsSuccess = false;
                                            qAAttachment.FailureReason = ex.Message;
                                            LogHelper.Error(_logService, _logModel, EncompassLibraryConstants.CORRESPONDENTQA_APPLICATION_ID, attachment.Title + " Attachment download failed :" + ex.Message + " " + ex.StackTrace);
                                        }
                                        qAPlaceHoder.Attachments.Add(qAAttachment);
                                    }

                                    correspondentQAPlaceHoderList.Add(qAPlaceHoder);
                                }
                            }
                        }
                        return correspondentQAPlaceHoderList;
                    }
                    else
                    {
                        throw new Exception(extractAttachmentDetails.LoanNumber + " not exists in system.");
                    }
                }
                else
                {                    
                    throw new Exception("LoanNumber is mandatory.");
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
    }
}
