using System.Collections.Generic;
using System.Linq;

namespace FGMC.MergeDocuments
{
    public class FileSort
    {
        public IEnumerable<IndividualFileInfo> SortPdfFileList(List<IndividualFileInfo> inputFileList, FileOrder fileorder)
        {
            IEnumerable<IndividualFileInfo> pdfFileinfoList = new List<IndividualFileInfo>();

            if (inputFileList.Count() > 0)
            {

                switch (fileorder)
                {
                    case FileOrder.SIZEASCENDING:
                        {
                            pdfFileinfoList = SortFilesUsingSize(inputFileList);
                            break;
                        }
                    case FileOrder.SIZEDESCENDING:
                        {
                            pdfFileinfoList = SortFilesUsingSizeDesc(inputFileList);
                            break;
                        }
                    case FileOrder.NAMEASCENDING:
                        {
                            pdfFileinfoList = SortFilesUsingName(inputFileList);
                            break;
                        }
                    case FileOrder.NAMEDESCENDING:
                        {
                            pdfFileinfoList = SortFilesUsingNameDesc(inputFileList).Reverse();
                            break;
                        }
                    case FileOrder.CREATEDDATEASCENDING:
                        {
                            pdfFileinfoList = SortFilesUsingDate(inputFileList);
                            break;
                        }
                    case FileOrder.CREATEDDATEDESCENDING:
                        {
                            pdfFileinfoList = SortFilesUsingDateDesc(inputFileList);
                            break;
                        }
                    default:
                        {
                            pdfFileinfoList = SetFileorderNone(inputFileList);
                            break;
                        }
                }
                return pdfFileinfoList;
            }
            return inputFileList;
        }

        private  IEnumerable<IndividualFileInfo> SortFilesUsingSize(IEnumerable<IndividualFileInfo> inputFileInfoList)
        {
            List<IndividualFileInfo> fileInfoList = inputFileInfoList.ToList();
            IComparer<IndividualFileInfo> sortFileComparer = new SortFileBySizeAsc();
            fileInfoList.Sort(sortFileComparer);
            return fileInfoList;
        }

        private IEnumerable<IndividualFileInfo> SortFilesUsingSizeDesc(IEnumerable<IndividualFileInfo> inputFileInfoList)
        {
            List<IndividualFileInfo> fileInfoList = inputFileInfoList.ToList();
            IComparer<IndividualFileInfo> sortFileComparer = new SortFileBySizeDesc();
            fileInfoList.Sort(sortFileComparer);
            return fileInfoList;
        }

        private IEnumerable<IndividualFileInfo> SortFilesUsingName(IEnumerable<IndividualFileInfo> inputFileInfoList)
        {
            List<IndividualFileInfo> fileInfoList = inputFileInfoList.ToList();
            fileInfoList.Sort();
            return fileInfoList;
        }

        private IEnumerable<IndividualFileInfo> SortFilesUsingNameDesc(IEnumerable<IndividualFileInfo> inputFileInfoList)
        {
            List<IndividualFileInfo> fileInfoList = inputFileInfoList.ToList();
            IComparer<IndividualFileInfo> sortFileComparer = new SortFileByNameDesc();
            fileInfoList.Sort(sortFileComparer);
            return fileInfoList;
        }

        private IEnumerable<IndividualFileInfo> SetFileorderNone(IEnumerable<IndividualFileInfo> inputFileInfoList)
        {
            return inputFileInfoList;
        }

        private IEnumerable<IndividualFileInfo> SortFilesUsingDate(IEnumerable<IndividualFileInfo> inputFileInfoList)
        {
            List<IndividualFileInfo> fileInfoList = inputFileInfoList.ToList();
            IComparer<IndividualFileInfo> sortFileComparer = new SortFileByDateAsc();
            fileInfoList.Sort(sortFileComparer);
            return fileInfoList;
        }

        private IEnumerable<IndividualFileInfo> SortFilesUsingDateDesc(IEnumerable<IndividualFileInfo> inputFileInfoList)
        {
            List<IndividualFileInfo> fileInfoList = inputFileInfoList.ToList();
            IComparer<IndividualFileInfo> sortFileComparer = new SortFileByDateDesc();
            fileInfoList.Sort(sortFileComparer);
            return fileInfoList;
        }
    }
}
