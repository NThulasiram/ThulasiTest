using EllieMae.Encompass.BusinessObjects.Loans;
using EllieMae.Encompass.BusinessObjects.Loans.Logging;
using EncompassLibrary.Properties;
using EncompassLibrary.Utilities;
using FGMC.Common.DataContract;
using Log4NetLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EllieMae.Encompass.Client;
using EllieMae.Encompass.Collections;

namespace EncompassLibrary.DocumentManager
{
    public class DocumentExtractor
    {
       
        public static ILoggingService _logService = new FileLoggingService(typeof(DocumentExtractor));
        public static  LogModel _logModel =new LogModel();
        public async static Task<IEnumerable<LoanLogicsFile>> ExtractAndDownloadLoanDocuments(Loan loan, string saveLocation,int applicationId=0)
        {
            if (loan == null) return null;
            return await Task.Run(() =>
            {
                var loanAttachments = loan.Attachments.Cast<Attachment>();
                var loanHasAttachments = loanAttachments.Any();
                if (loanHasAttachments)
                {
                    var downloadedAttachments = DownloadLoanAttachments(loan.LoanNumber, loanAttachments, saveLocation, applicationId);
                    var downloadedDisclosures = DownloadLoanDisclosures(loan.LoanNumber, loan.Log.DocumentOrders.Cast<DocumentOrder>(), saveLocation, applicationId);
                    var result = new List<LoanLogicsFile>();
                    result.AddRange(downloadedAttachments);
                    result.AddRange(downloadedDisclosures);
                    return result;
                }
                return null;
            });
        }

       
        #region Private
        private static IEnumerable<LoanLogicsFile> DownloadLoanAttachments(string loanNumber, IEnumerable<Attachment> attachments, string saveLocation,int applicationId)
        {
            var loanLogicsFiles = new List<LoanLogicsFile>();
            try
            {
                LoanLogicsFile loanLogicsFileObject = null;
                attachments.ToList().ForEach(attachment =>
                {
                    try
                    {
                        LogHelper.Info(_logService, _logModel,applicationId, string.Format(Resources.DocumentDownloadStart, attachment.Name, loanNumber),loanNumber);
                     
                        var hasPdfExtension = FileHasExtension(attachment.Name, Resources.PdfExtension) || FileHasExtension(attachment.Title, Resources.PdfExtension);
                        if (hasPdfExtension)
                        {
                            var directoryPath = string.Format("{0}\\{1}", saveLocation, loanNumber);
                            CreatePathIfNotExists(directoryPath);
                            var trackedDocument = attachment.GetDocument();
                            var placeholderName = trackedDocument == null ? Resources.UnAssignedPlaceHolder : trackedDocument.Title;
                            var modifiedFileName = CreateAndCleanFileName(placeholderName, attachment, counter);
                            var folderName = trackedDocument.DocumentType.Equals(Resources.DisclosureTitleName) ? Resources.DisclosureTitleName : Resources.EFolderTitleName;
                            var fileSavePath = string.Format("{0}\\{1}", directoryPath, modifiedFileName);
                            LogHelper.Info(_logService, _logModel, applicationId, string.Format(Resources.SavingToDisk, fileSavePath), loanNumber);
                            attachment.SaveToDisk(fileSavePath);
                            LogHelper.Info(_logService, _logModel, applicationId, string.Format(Resources.DocumentDownloadEnd, attachment.Name, loanNumber), loanNumber);
                            loanLogicsFileObject = SetLoanLogicsFileDetails(attachment.Name, modifiedFileName, folderName,loanNumber);
                        }
                    }
                    catch (Exception ex)
                    {
                        loanLogicsFileObject.Reason=!string.IsNullOrEmpty(ex.Message) ? ex.Message : string.Empty;
                        loanLogicsFileObject.Status = Resources.FileFailCopy;
                        LogHelper.Error(_logService, _logModel, applicationId, ex.Message, loanNumber,ex);
                    }
                    loanLogicsFiles.Add(loanLogicsFileObject);
                });
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logService, _logModel, applicationId, ex.Message, loanNumber, ex);
            }
            return loanLogicsFiles;
        }
        private static IEnumerable<LoanLogicsFile> DownloadLoanDisclosures(string loanNumber,IEnumerable<DocumentOrder> orderedDocuments, string saveLocation,int applicationId)
        {
            var disclosureDocuments = new List<LoanLogicsFile>();
            try
            {
                foreach (DocumentOrder orderDocument in orderedDocuments)
                {
                    if (orderDocument.Documents != null && !orderDocument.Documents.Cast<OrderedDocument>().Any(od => od.DocumentType.Equals(Resources.ClosingDocumentType, StringComparison.OrdinalIgnoreCase)))
                    {
                        orderDocument.Documents.Cast<OrderedDocument>().ToList().ForEach(doc =>
                        {
                            var disclosureDocument = new LoanLogicsFile();
                            var documentAsDataObject = doc.Retrieve();
                            try
                            {
                                if (documentAsDataObject != null)
                                {
                                    var placeholderName = doc == null ? Resources.UnAssignedPlaceHolder : doc.Title;
                                    disclosureDocument.FolderName = doc.Title;
                                    disclosureDocument.LoanNumber = loanNumber;
                                    disclosureDocument.Status = Resources.FileSuccessCopy;
                                    var directoryPath = string.Format("{0}{1}\\", saveLocation, loanNumber);
                                    CreatePathIfNotExists(directoryPath);
                  var fileName = string.Format(Resources.FileNameThreePlacesFormat, placeholderName, doc.Title, string.Format("MMddyyyyhhmm", orderDocument.Date.ToString()));
                                    disclosureDocument.DocumentName = placeholderName + "_" + Path.GetFileNameWithoutExtension(doc.Title) + "_" + string.Format("MMddyyyyhhmm", orderDocument.Date.ToString()) + Resources.PdfExtension;
                                    if (fileName.Length > int.Parse(Resources.FileNameMaxLength))
                                    {
                                        fileName = string.Format(Resources.FileNameThreePlacesFormat, doc.Title, DateTime.Now.ToShortDateString(), DateTime.Now.Ticks);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                disclosureDocument.Reason = !string.IsNullOrEmpty(ex.Message) ? ex.Message : string.Empty;
                                disclosureDocument.Status = Resources.FileFailCopy;
                                LogHelper.Error(_logService, _logModel, applicationId, ex.Message, loanNumber, ex);
                            }
                            disclosureDocuments.Add(disclosureDocument);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logService, _logModel, applicationId, ex.Message, loanNumber, ex);
            }
            return disclosureDocuments;
        }
        private static string CreateAndCleanFileName(string  placeHolderName,Attachment attachment,int counter)
        {
            var fileName = placeHolderName + "_" + Path.GetFileNameWithoutExtension(attachment.Title) + "_" + attachment.Date.ToString("MMddyyyyhhmm") + Path.GetExtension(attachment.Title);
            if (fileName.Length > int.Parse(Resources.FileNameMaxLength))
                fileName = string.Format(Resources.FileNameThreePlacesFormat, attachment.Title, DateTime.Now.ToShortDateString(), DateTime.Now.Ticks);
            Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
            return fileName.RemoveWhitespace();
        }
        private static bool FileHasExtension(string fileToCheck,string extensionToCheck)
        {
            return Path.GetExtension(fileToCheck).Equals(extensionToCheck, StringComparison.OrdinalIgnoreCase);
        }
        private static void CreatePathIfNotExists(string pathToCheck)
        {
            if (!Directory.Exists(pathToCheck))
            {
                Directory.CreateDirectory(pathToCheck);
            }
        }
        private static LoanLogicsFile SetLoanLogicsFileDetails(string attachmentName,string modifiedFileName,string folderName,string loanNumber)
        {
            return new LoanLogicsFile
            {
                FolderName = folderName,
                LoanNumber = loanNumber,
                OriginalFileName = attachmentName,
                RevisedFileName = modifiedFileName,
                Status=Resources.FileSuccessCopy               
            };
        }
        private static int counter = 1;
        #endregion
    }

}
