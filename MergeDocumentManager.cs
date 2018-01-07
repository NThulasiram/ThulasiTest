using Log4NetLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebSupergoo.ABCpdf10;

namespace FGMC.MergeDocuments
{
    public class MergeDocumentManager : IMergeDocumentManager
    {
        readonly ILoggingService _logger = new FileLoggingService(typeof (MergeDocumentManager));
        private readonly LogModel _logModel = new LogModel();
        private readonly FileSort _fileSort = new FileSort();


        /// <summary>
        /// It's sort given list of files based on file order.
        /// </summary>
        /// <param name="pdfFilePathList">List of file paths.</param>
        /// <param name="fileorder">Files sort order.</param>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        public IEnumerable<IndividualFileInfo> SortFiles(List<IndividualFileInfo> inputFileList, FileOrder fileorder, int applicationId)
        {
            IEnumerable<IndividualFileInfo> sortedFileinfoList;
            try
            {
                sortedFileinfoList = _fileSort.SortPdfFileList(inputFileList, fileorder);
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, applicationId, ex.Message, ex: ex);
                throw;
            }

            return sortedFileinfoList;
        }
        private IEnumerable<MergeOperationInfo> PrepareMergeFileList(IEnumerable<IndividualFileInfo> inputFileInfoList, int mergeFileSizeLimit,int applicationId)
        {
            ABCPdfFilelimitCheck(mergeFileSizeLimit);
            List<MergeOperationInfo> mergedfileList = new List<MergeOperationInfo>();
            MergeOperationInfo mergedfile = new MergeOperationInfo();
            double mergedFileSize = 0;
            try
            {
                if (inputFileInfoList.Count() > 0)
                {
                    mergedfileList.Add(mergedfile);
                    int fileCount = 0;
                    foreach (var inputfile in inputFileInfoList)
                    {
                        double length = ConvertBytesToMegabytes(inputfile.FileInfo.Length);
                        mergedFileSize = CalculateFileSize(length, mergedFileSize);

                        if (fileCount != 0 && mergedFileSize >= Convert.ToInt32(mergeFileSizeLimit))
                        {
                            mergedFileSize = length;
                            mergedfile = new MergeOperationInfo();
                            IndividualFileInfo file = new IndividualFileInfo
                            {
                                FileName = inputfile.FileInfo.Name,
                                FilePath = inputfile.FilePath,
                                FileId = inputfile.FileId
                            };
                            mergedfile.Files.Add(file);
                            mergedfileList.Add(mergedfile);
                        }
                        else
                        {
                            IndividualFileInfo file = new IndividualFileInfo();
                            file.FileName = inputfile.FileInfo.Name;
                            file.FilePath = inputfile.FilePath;
                            file.FileId = inputfile.FileId;
                            mergedfile.Files.Add(file);
                        }
                        fileCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, applicationId, ex.Message, ex: ex);
                throw;
            }

            return mergedfileList;
        }
        /// <summary>
        /// It's merge given list of files.
        /// </summary>
        /// <param name="pdfFileinfoList">List of what files need to be merge.</param>
        /// <param name="mergeFileSizeLimit">Merge file max size(In MB).</param>
        /// <param name="destinationDirectory">Directory path for to copy merged file.</param>
        /// <param name="mergedFileTitle">Merged file name (without extension)</param>
        /// <param name="fileCountSeperator">file count more then one then it separate with this symbol</param>
        /// <returns></returns>
        public IEnumerable<MergeOperationInfo> ExtractAndMergeDocuments(MergeDocumentsInputInfo mergeDocumentsInputInfo,int applicationId)
        {
            int i = 1;
            IEnumerable<MergeOperationInfo> mergeFileList = new List<MergeOperationInfo>();
            if (mergeDocumentsInputInfo.InputFiles.Any())
            {
                mergeFileList = PrepareMergeFileList(mergeDocumentsInputInfo.InputFiles, mergeDocumentsInputInfo.MergeFileSizeLimit, applicationId);
                if (!Directory.Exists(mergeDocumentsInputInfo.DestinationDirectory))
                {
                    Directory.CreateDirectory(mergeDocumentsInputInfo.DestinationDirectory);
                }
                foreach (MergeOperationInfo mergefile in mergeFileList)
                {

                    try
                    {
                        var mergedFilePath = Path.Combine(mergeDocumentsInputInfo.DestinationDirectory, string.Format("{0}" + mergeDocumentsInputInfo.FileCountSeperator + "{1}{2}", mergeDocumentsInputInfo.MergedFileTitle, i,MergeDocumentConstant.EXTENSION_PDF));
                        ConvertFilesIntoPdf(mergefile, mergeDocumentsInputInfo.ContinueProcessOnConvertionFail, applicationId);
                        if (!mergeDocumentsInputInfo.ContinueProcessOnConvertionFail && mergefile.ConversionFailedFiles.Count > 0)
                        {
                            return mergeFileList;
                        }
                        Merge(mergefile, mergedFilePath, mergeDocumentsInputInfo.MergedFileTitle, applicationId);
                        i++;
                    }
                    catch (Exception ex)
                    {
                        mergefile.IsMergeSuccess = false;
                        mergefile.ErrorMessage = ex.Message;
                        LogHelper.Error(_logger, _logModel, applicationId, ex.Message, mergeDocumentsInputInfo.MergedFileTitle, ex);
                    }
                }
            }
            return mergeFileList;
        }
        public IEnumerable<IndividualFileInfo> ConvertDocuments(IEnumerable<IndividualFileInfo> loanLogicsFiles, FileExtensionType convertFileExtension,bool isAllowProcessConvertionFail,int applicationId)
        {
            if (convertFileExtension == FileExtensionType.PDF)
            {
                foreach (var loanLogicsFile in loanLogicsFiles)
                {
                  var convertedfile=  ConvertFileIntoPdf(loanLogicsFile, applicationId);
                    if (!convertedfile.IsConversionSuccessful && !isAllowProcessConvertionFail)
                    {
                        foreach (var file in loanLogicsFiles)
                        {
                            if(convertedfile.FileId!= file.FileId)
                            file.ErrorMessage = "Conversion Skipped due to one of the file conversion failed.";
                        }
                        break;
                    }
                }
            }
            else
            {
                foreach (var file in loanLogicsFiles)
                {
                    file.ErrorMessage = "File conversion not supported other than pdf extension.";
                }
            }
            return loanLogicsFiles;
        }
        /// <summary>
        /// Get all files paths from the Directory.
        /// </summary>
        /// <param name="targetDirectory">TargetDirectory Name.</param>
        /// <returns></returns>
        public IEnumerable<string> ProcessDirectory(string targetDirectory)
        {
            var fileEntries = Directory.EnumerateFiles(targetDirectory, "*.*", SearchOption.AllDirectories);

            return fileEntries;
        }

        private void Merge(MergeOperationInfo mergeOperationInfo, string mergedFilePath, string loanNumber,int applicationId)
        {
            Doc finalPdf = new Doc();
            try
            {
                foreach (var file in mergeOperationInfo.Files)
                {
                    try
                    {
                        var filePath = file.FilePath;
                        if (filePath.EndsWith(".pdf", StringComparison.CurrentCultureIgnoreCase))
                        {
                            var pdfDoc = new Doc();
                            pdfDoc.Read(filePath);
                            finalPdf.Append(pdfDoc);
                            file.IsMerged = true;
                            pdfDoc.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        IndividualFileInfo failedFile = new IndividualFileInfo
                        {
                            FileId = file.FileId,
                            FileName = file.FileName,
                            FilePath = file.FilePath,
                            IsMerged = false,
                            ErrorMessage = ex.Message,
                            IsConversionSuccessful = file.IsConversionSuccessful
                        };
                        file.ErrorMessage = ex.Message;
                        mergeOperationInfo.MergeFailedFiles.Add(failedFile);
                        LogHelper.Error(_logger, _logModel, applicationId, ex.Message, loanNumber, ex: ex);

                    }
                }
                //at least one file should successfully merge to save merge file.
                if (mergeOperationInfo.Files.Count(o => o.IsMerged) > 0)
                {
                    finalPdf.Save(mergedFilePath);
                    mergeOperationInfo.IsMergeSuccess = true;
                    mergeOperationInfo.FileName = Path.GetFileName(mergedFilePath);
                    mergeOperationInfo.FilePath = mergedFilePath;
                }
                else
                {
                    mergeOperationInfo.IsMergeSuccess = false;
                    mergeOperationInfo.FileName = string.Empty;
                    mergeOperationInfo.FilePath = string.Empty;
                    mergeOperationInfo.ErrorMessage = "All files failed to Merge";
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, applicationId, ex.Message, loanNumber, ex);
                throw;
            }
            finally
            {
                finalPdf.Clear();
            }
        }
        private void ConvertFilesIntoPdf(MergeOperationInfo mergeOperationInfo, bool isAllowProcessConvertionFail,int applicationId)
        {
            foreach (var file in mergeOperationInfo.Files)
            {
                var convertedfile = ConvertFileIntoPdf(file, applicationId);
                if (!convertedfile.IsConversionSuccessful)
                {
                    mergeOperationInfo.ConversionFailedFiles.Add(convertedfile);
                    if (!isAllowProcessConvertionFail)
                        break;
                }
            }
        }
        private IndividualFileInfo ConvertFileIntoPdf(IndividualFileInfo file,int applicationId)
        {
            Doc pdfDoc = null;
            try
            {
                if (!file.FilePath.EndsWith(".pdf", StringComparison.CurrentCultureIgnoreCase))
                {
                    var filePath = file.FilePath;
                    pdfDoc = new Doc();
                    if (filePath.EndsWith(".html", StringComparison.CurrentCultureIgnoreCase)
                        || filePath.EndsWith(".htm", StringComparison.CurrentCultureIgnoreCase)
                        || filePath.EndsWith(".FINDINGSHTML", StringComparison.CurrentCultureIgnoreCase)
                        || Path.GetExtension(filePath).ToUpper().Contains("FINDINGSHTML"))
                    {
                        pdfDoc.Rect.Inset(70, 70);
                        pdfDoc.HtmlOptions.UseScript = false;

                        pdfDoc.Page = pdfDoc.AddPage();
                        int theID;
                        theID = pdfDoc.AddImageHtml(File.ReadAllText(filePath));
                        while (true)
                        {
                            if (!pdfDoc.Chainable(theID))
                                break;
                            pdfDoc.Page = pdfDoc.AddPage();
                            theID = pdfDoc.AddImageToChain(theID);
                        }

                        for (int i = 1; i <= pdfDoc.PageCount; i++)
                        {
                            pdfDoc.PageNumber = i;
                            pdfDoc.Flatten();
                        }

                    }
                    else if (filePath.EndsWith(".png", StringComparison.CurrentCultureIgnoreCase)
                             || filePath.EndsWith(".jpg", StringComparison.CurrentCultureIgnoreCase)
                             || filePath.EndsWith(".jpeg", StringComparison.CurrentCultureIgnoreCase)
                             || filePath.EndsWith(".jpe", StringComparison.CurrentCultureIgnoreCase)
                             || filePath.EndsWith(".gif", StringComparison.CurrentCultureIgnoreCase)
                             || filePath.EndsWith(".bmp", StringComparison.CurrentCultureIgnoreCase)
                             || filePath.EndsWith(".tif", StringComparison.CurrentCultureIgnoreCase)
                        )
                    {
                        pdfDoc.Rect.Inset(70, 70);
                        XImage theImg = new XImage();
                        theImg.SetFile(filePath);
                        pdfDoc.AddImageObject(theImg, false);
                        file.IsConversionSuccessful = true;
                    }
                    else if (filePath.EndsWith(".txt", StringComparison.CurrentCultureIgnoreCase)|| 
                        Path.GetExtension(filePath).ToUpper().Contains("CREDITPRINTFILE"))
                    {
                        pdfDoc.Rect.Inset(70, 70);
                        pdfDoc.Page = pdfDoc.AddPage();
                        int theID;
                        theID = pdfDoc.AddText(File.ReadAllText(filePath));

                        while (true)
                        {
                            if (!pdfDoc.Chainable(theID))
                                break;
                            pdfDoc.Page = pdfDoc.AddPage();
                            theID = pdfDoc.AddHtml(File.ReadAllText(filePath), theID);
                        }
                        for (int i = 1; i <= pdfDoc.PageCount; i++)
                        {
                            pdfDoc.PageNumber = i;
                            pdfDoc.Flatten();
                        }

                    }
                    else if (filePath.EndsWith(".xps", StringComparison.CurrentCultureIgnoreCase))
                    {
                        pdfDoc.Read(filePath);
                    }
                    else if (filePath.EndsWith(".docx", StringComparison.CurrentCultureIgnoreCase) || filePath.EndsWith(".doc", StringComparison.CurrentCultureIgnoreCase))
                    {
                        XReadOptions xr = new XReadOptions();
                        xr.ReadModule = ReadModuleType.MSOffice;
                        pdfDoc.Read(filePath, xr);

                    }
                    else
                    {
                        pdfDoc.Read(filePath);
                    }
                    var pdfpath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + ".pdf";
                    pdfDoc.Save(pdfpath);
                    file.IsConversionSuccessful = true;
                    file.FilePath = pdfpath;
                    pdfDoc.Clear();
                }
                else
                {
                    file.IsConversionSuccessful = true;
                }
            }
            catch (Exception ex)
            {
                file.IsConversionSuccessful = false;
                file.ErrorMessage = ex.Message;
                pdfDoc?.Clear();
                LogHelper.Error(_logger, _logModel, applicationId, string.Format("Pdf conversion process failed for the file {0} ", file.FileName), null, ex);
            }
            return file;
        }
        private static double ConvertBytesToMegabytes(long bytes)
        {
            float vlaue = (bytes / 1024f) / 1024f;

            return Math.Round(vlaue, 2);
        }
        private double CalculateFileSize(double mBytes, double existingFileSize)
        {
            return Math.Round(existingFileSize += mBytes, 2);
        }
        private void ABCPdfFilelimitCheck(int filelimit)
        {
            if (Convert.ToInt32((MergeDocumentConstant.ABC_MERGE_FILELIMIT)) < filelimit)
            {
                throw new Exception(string.Format("{0}{1}{2}", MergeDocumentConstant.ABC_FILELIMIT_EXCEED,
                    MergeDocumentConstant.ABC_MERGE_FILELIMIT, "MB"));
            }
        }
    }
}
