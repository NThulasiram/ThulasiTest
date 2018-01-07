using System.Collections.Generic;

namespace FGMC.MergeDocuments
{
    public interface IMergeDocumentManager
    {
        IEnumerable<string> ProcessDirectory(string targetDirectory);
        IEnumerable<IndividualFileInfo> SortFiles(List<IndividualFileInfo> inputFileList, FileOrder fileorder, int applicationId = 0);
        IEnumerable<MergeOperationInfo> ExtractAndMergeDocuments(MergeDocumentsInputInfo mergeDocumentsInputInfo, int applicationId = 0);
        IEnumerable<IndividualFileInfo> ConvertDocuments(IEnumerable<IndividualFileInfo> loanLogicsFiles,FileExtensionType convertFileExtension, bool isAllowProcessConvertionFail, int applicationId = 0);
    }
}
