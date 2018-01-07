using EllieMae.Encompass.Client;
using EncompassLibrary.Utilities;
using FGMC.Common.DataContract;
using System.Collections.Generic;

namespace EncompassLibrary.LoanDataExtractor
{
    public class Extractor
    {



        #region private fields
        private LogicsReportType _logicsReportType;
        private PostCloseLoanType? _postCloseLoanType;
        private string _sourcePath;
        private string _destinationSavePath;
        private string _loanNumber;
        private IEnumerable<string> _loanNumbers;
        #endregion
        /// <summary>
        /// Overloaded Constructor
        /// </summary>
        /// <param name="reportType"></param>
        /// <param name="loanNumbers"></param>
        /// <param name="sourcePath"></param>
        /// <param name="destinationSavePath"></param>
        public Extractor(IEnumerable<string> loanNumbers)
        {
            _loanNumbers = new List<string>();
            _loanNumbers = loanNumbers;
        }
        /// <summary>
        /// Overloaded Constructor 2nd
        /// </summary>
        /// <param name="reportType"></param>
        /// <param name="postCloseType"></param>
        /// <param name="loanNumbers"></param>
        /// <param name="sourcePath"></param>
        /// <param name="destinationSavePath"></param>
        public Extractor(LogicsReportType reportType, PostCloseLoanType postCloseType, IEnumerable<string> loanNumbers, string sourcePath, string destinationSavePath)
        {
            _logicsReportType = reportType;
            _postCloseLoanType = postCloseType;
            _sourcePath = sourcePath;
            _destinationSavePath = destinationSavePath;
            loanNumbers = new List<string>();
            _loanNumbers = loanNumbers;
        }
        /// <summary>
        /// Overloaded Constructor 3rd
        /// </summary>
        /// <param name="reportType"></param>
        /// <param name="postCloseType"></param>
        /// <param name="loanNumber"></param>
        /// <param name="sourcePath"></param>
        /// <param name="destinationSavePath"></param>
        public Extractor(LogicsReportType reportType, PostCloseLoanType postCloseType,string loanNumber, string sourcePath, string destinationSavePath)
        {
            _logicsReportType = reportType;
            _postCloseLoanType = postCloseType;
            _sourcePath = sourcePath;
            _destinationSavePath = destinationSavePath;
            _loanNumber = loanNumber;
        }
        public IEnumerable<LoanLogicsReport> GetReport(string reportType, string postClosedReportType, Session session)
        {
            var reportObjectToCreate = GetReportObject(reportType);
            var generator = new LoanLogicsGenerator();
            return generator.GetReportData(reportObjectToCreate, _loanNumbers, session, GetPostCloseReportType(postClosedReportType));
        }

        public IEnumerable<PostCloseReport> GetPostCloseReport(string reportType, string postClosedReportType, Session session)
        {
            var reportObjectToCreate = GetReportObject(reportType);
            var generator = new LoanLogicsGenerator();
            return generator.GetPostCloseReportData(reportObjectToCreate, _loanNumbers, session, GetPostCloseReportType(postClosedReportType));
        }

        public IEnumerable<LoanLogicsReport> GetReportForWinService(string reportType, string postClosedReportType, Session session)
        {
            var reportObjectToCreate = GetReportObjectForWinService(reportType);
            var generator = new LoanLogicsGenerator();
            return generator.GetReportData(reportObjectToCreate, _loanNumbers, session, GetPostCloseReportTypeForWinService(postClosedReportType));
        }
        public IEnumerable<LoanLogicsReport> CreatePostCloseReportForWinService(string reportType,Session session)
        {
            var reportObjectToCreate = GetReportObjectForWinService(reportType);
            var generator = new LoanLogicsGenerator();
            return generator.CreatePostCloseReportData(reportObjectToCreate, _loanNumbers, session);
        }

        public IEnumerable<LoanLogicsReport> GetPostCloseReportForWinService(string reportType, string postClosedReportType, Session session)
        {
            var reportObjectToCreate = GetReportObjectForWinService(reportType);
            var generator = new LoanLogicsGenerator();
            return generator.GetPostCloseReportData(reportObjectToCreate, _loanNumbers, session, GetPostCloseReportTypeForWinService(postClosedReportType));
        }

        public IEnumerable<LoanLogicsReport> GenerateCopyReports(IEnumerable<string> loanNumbers, Session session)
        {
            var generator = new LoanLogicsGenerator();
            return generator.CopyAndGenerateReport(loanNumbers,session);
        }

        #region Private Method(s)

        private LoanLogicsReport GetReportObject(LogicsReportType reportType, PostCloseLoanType? postCloseType)
        {
            switch (reportType)
            {
                case LogicsReportType.PreClose:
                    return new PreCloseReport();
                case LogicsReportType.PostClose:
                    return new PostCloseReport();
                case LogicsReportType.Adverse:
                    return new AdverseReport();
                default:
                    return null;
            }
        }
        /// <summary>
        /// Instead of Again Making DB Call we know the report Type
        /// </summary>
        /// <param name="reportType"></param>
        /// <returns></returns>
        private LoanLogicsReport GetReportObject(string reportType)
        {
                switch (reportType)
                {
                    case EncompassLibraryConstants.PRECLOSEREPORT:
                        return new PreCloseReport();
                    case EncompassLibraryConstants.POSTCLOSEREPORT:
                        return new PostCloseReport();
                    case EncompassLibraryConstants.ADVERSEREPORT:
                        return new AdverseReport();
                    default:
                        return null;
            }
        }
        private LoanLogicsReport GetReportObjectForWinService(string reportType)
        {
            switch (reportType)
            {
                case "LOANLOGICSREPORTTYPES1":
                    return new PreCloseReport();
                case "LOANLOGICSREPORTTYPES2":
                    return new PostCloseReport();
                case "LOANLOGICSREPORTTYPES3":
                    return new AdverseReport();
                default:
                    return null;
            }
        }
        private PostCloseLoanType GetPostCloseReportType(string postClosedReportType)
        {
                switch (postClosedReportType)
                {
                    case EncompassLibraryConstants.EARLY_PAYMENT_DEFAULT:
                        return PostCloseLoanType.EPD;
                    case EncompassLibraryConstants.TARGETED_REPORT:
                        return PostCloseLoanType.Targeted;
                    case EncompassLibraryConstants.DISCRETIONARY_REPORT:
                        return PostCloseLoanType.Discretionary;
                    case EncompassLibraryConstants.RANDOM_REPORT:
                        return PostCloseLoanType.Random;
                    default:
                        return PostCloseLoanType.None;
            }
        }
        private PostCloseLoanType GetPostCloseReportTypeForWinService(string postClosedReportType)
        {
            switch (postClosedReportType)
            {
                case "Early Payment Default":
                    return PostCloseLoanType.EPD;
                case "Targeted":
                    return PostCloseLoanType.Targeted;
                case "Discretionary":
                    return PostCloseLoanType.Discretionary;
                case "Random":
                    return PostCloseLoanType.Random;
                default:
                    return PostCloseLoanType.None;
            }
        }

        #endregion


    }

   
}
