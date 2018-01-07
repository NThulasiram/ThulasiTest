using EllieMae.Encompass.BusinessObjects.Loans;
using EllieMae.Encompass.Client;
using EllieMae.Encompass.Collections;
using EncompassLibrary.LoanDataExtractor;
using EncompassLibrary.Properties;
using EncompassLibrary.Utilities;
using FGMC.Common.DataContract;
using Log4NetLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using FGMC.SecurityLibrary;
using EllieMae.Encompass.BusinessObjects.Loans.Logging;

namespace EncompassLibrary.LoanManager
{
	public class EncompassLoanManager : IEncompassLoanManager
    {
        private LoanLogicsGenerator _loanLogicsGenerator;
		private readonly ILoggingService _logService;
	    private LogModel _logModel;
	    private ConfigManager _configManager;
	    private string userName = string.Empty;
        private string password = string.Empty;
        private string serverUrl = string.Empty;
        public EncompassLoanManager()
		{
			EllieMae.Encompass.Runtime.RuntimeServices runtimeServices = new EllieMae.Encompass.Runtime.RuntimeServices();
			runtimeServices.Initialize();
			_loanLogicsGenerator = new LoanLogicsGenerator();
			_logService = new FileLoggingService(typeof(EncompassLoanManager));
            _logModel= new LogModel();
            _configManager = new ConfigManager();
            userName = _configManager.GetConfigValue("UserId", 0);
            password = _configManager.GetConfigValue("Password", 0);
            serverUrl =_configManager.GetConfigValue("ServerUri", 0);
        }
        public IEnumerable<LoanLogicsReport> GetLoanAsLoanLogicsReport(IEnumerable<string> loanNumbers, string loanLogicsReportType, bool isPostCloseReport, Session session,int applicationId=0)
        {
		
			try
			{
				var reportName = (RawLoanLogicsReportName)Enum.Parse(typeof(RawLoanLogicsReportName), loanLogicsReportType);
                var reportData = getLoanDataList(loanNumbers,session, applicationId);
				if (reportData == null) return null;
				if (isPostCloseReport)
				{
                    var data = _loanLogicsGenerator.GeneratePostCloseLoanLogicsReportDataAsync(new PostCloseReport(), reportData);
                    return data.Result;
                }
				else
				{
                    var data = _loanLogicsGenerator.GenerateLoanLogicsReportDataAsync(getLoanLogicsReportObject(reportName), reportData);
				    return data.Result;
				}
			}
			catch (Exception ex)
			{
                LogHelper.Error(_logService,_logModel, applicationId,ex.Message,ex:ex);
			}
            return null;
        }        

        public string GetCurrentUserId()
        {
			var session = new Session();
			session.Start(serverUrl, userName, password);
			return session.GetCurrentUser().ID;
        }
        private LoanReportDataList getLoanDataList(IEnumerable<string> loanNumbers,Session session,int applicationId)
		{
			try
			{
				var stringGuidList = Common.SetStringGuidList(loanNumbers, session);
				if (stringGuidList == null || stringGuidList.Count == 0) return null;
				var stringFieldList = Common.SetStringFieldList(session);
				return session.Reports.SelectReportingFieldsForLoans(stringGuidList, stringFieldList);
			}
			catch (Exception ex)
			{

                LogHelper.Error(_logService, _logModel, applicationId, ex.Message, ex: ex);
            }
			return null;
        }
        private Loan getEncompassLoan(string loanNumber, Session encompassSession)
        {
            var loanGuid = Common.GetGuid(loanNumber, encompassSession);
            if (string.IsNullOrEmpty(loanGuid))
                throw new ArgumentException(string.Format(Resources.NoGuidMessage, loanNumber));
            return encompassSession.Loans.Open(loanGuid);
        }
        private LoanLogicsReport getLoanLogicsReportObject(RawLoanLogicsReportName reportType)
        {
            switch (reportType)
            {
                case RawLoanLogicsReportName.LOANLOGICSREPORTTYPES1:
                    return new PreCloseReport();
                case RawLoanLogicsReportName.LOANLOGICSREPORTTYPES3:
                    return new AdverseReport();
                default:
                    return null;
            }
        }
        private PostCloseLoanType getPostCloseReportType(string postClosedReportType)
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
       
        #region reading from configuration file
       

        private const string postCloseReport = "LoanHD Services PostClose";
        private const string adverseReport = "LOANLOGICSREPORTTYPES3";
        private const string precloseReport = "LoanHD Services PreClose";
        private const string preCloseSubReport = "LOANLOGICSREPORTTYPES1";
        private const string adverseSubReport = "LOANLOGICSREPORTTYPES3";
        private const string postCloseSubReport = "LOANLOGICSREPORTTYPES2";

        #endregion
    }
}
