using System;
using System.Collections.Generic;
using System.IO;

namespace FGMC.MergeDocuments
{
    public class MergeOperationInfo
    {
        public MergeOperationInfo()
        {
            Files = new List<IndividualFileInfo>();
            MergeFailedFiles = new List<IndividualFileInfo>();
            ConversionFailedFiles = new List<IndividualFileInfo>();
        }

        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public List<IndividualFileInfo> Files { get; set; }
        public List<IndividualFileInfo> MergeFailedFiles { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public bool IsMergeSuccess { get; set; } = false;
        public List<IndividualFileInfo> ConversionFailedFiles { get; set; }
    }

    public class IndividualFileInfo:IComparable<IndividualFileInfo>
    {
        public IndividualFileInfo()
        {

        }
        public IndividualFileInfo(string filePath)
        {
            FilePath = filePath;
            FileInfo = new FileInfo(filePath);
        }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public bool IsMerged { get; set; } = false;
        public string ErrorMessage { get; set; }
        public bool IsConversionSuccessful { get; set; } = false;
        public long FileId { get; set; } = 0;
        public FileInfo FileInfo { get; set; }
        public int CompareTo(IndividualFileInfo obj)
        {
            IndividualFileInfo individualFileInfo = obj;
            return String.Compare(this.FileInfo.Name, individualFileInfo.FileInfo.Name);
        }
    }

    public  class SortFileBySizeAsc : IComparer<IndividualFileInfo>
    {
        public int Compare(IndividualFileInfo a, IndividualFileInfo b)
        {
            IndividualFileInfo f1 = a;
            IndividualFileInfo f2 = b;

            if (f1.FileInfo.Length > f2.FileInfo.Length)
                return 1;

            if (f1.FileInfo.Length < f2.FileInfo.Length)
                return -1;
            else
                return 0;
        }
    }

    public class SortFileBySizeDesc : IComparer<IndividualFileInfo>
    {
        public int Compare(IndividualFileInfo a, IndividualFileInfo b)
        {
            IndividualFileInfo f1 = a;
            IndividualFileInfo f2 = b;

            if (f1.FileInfo.Length < f2.FileInfo.Length)
                return 1;

            if (f1.FileInfo.Length > f2.FileInfo.Length)
                return -1;
            else
                return 0;
        }
    }

    public class SortFileByNameDesc : IComparer<IndividualFileInfo>
    {
        public int Compare(IndividualFileInfo a, IndividualFileInfo b)
        {
            IndividualFileInfo f1 = a;
            IndividualFileInfo f2 = b;
            return String.Compare(f1.FileInfo.Name, f2.FileInfo.Name);
        }
    }

    public class SortFileByDateAsc : IComparer<IndividualFileInfo>
    {
        public int Compare(IndividualFileInfo a, IndividualFileInfo b)
        {
            IndividualFileInfo f1 = (IndividualFileInfo)a;
            IndividualFileInfo f2 = (IndividualFileInfo)b;

            if (f1.FileInfo.CreationTime > f2.FileInfo.CreationTime)
                return 1;

            if (f1.FileInfo.CreationTime < f2.FileInfo.CreationTime)
                return -1;
            else
                return 0;
        }
    }
    public class SortFileByDateDesc : IComparer<IndividualFileInfo>
    {
        public int Compare(IndividualFileInfo a, IndividualFileInfo b)
        {
            IndividualFileInfo f1 = a;
            IndividualFileInfo f2 = b;

            if (f1.FileInfo.CreationTime < f2.FileInfo.CreationTime)
                return 1;

            if (f1.FileInfo.CreationTime > f2.FileInfo.CreationTime)
                return -1;
            else
                return 0;
        }
    }

    public class MergeDocumentsInputInfo
    {
        public IEnumerable<IndividualFileInfo> InputFiles { get; set; }
        public int MergeFileSizeLimit { get; set; } = 200;
        public string DestinationDirectory { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Filemerge");
        public string MergedFileTitle { get; set; } = "MergeFile";
        public string FileCountSeperator { get; set; } = "_";
        public bool ContinueProcessOnConvertionFail { get; set; } = true;

    }

    public enum  FileOrder
    {
        SIZEASCENDING,
        SIZEDESCENDING,
        NAMEASCENDING,
        NAMEDESCENDING,
        CREATEDDATEASCENDING,
        CREATEDDATEDESCENDING,
    }

    public enum FileExtensionType
    {
        PDF=1,
        TXT=2,
        JPG=3,
        HTML=4,
        JPEG=5,
        JPE=6,
        GIF=7,
        BMP=8,
        TIF=9,
        DOCX=10,
        DOC=11
    }

}
