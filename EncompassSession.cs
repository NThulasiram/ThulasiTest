using EllieMae.Encompass.Client;
using EllieMae.Encompass.Collections;
using EncompassLibrary.LoanDataExtractor;
using EncompassLibrary.Utilities;
using FGMC.Common.DataContract;
using Log4NetLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using FGMC.SecurityLibrary;

namespace EncompassLibrary
{
	/// <summary>
	/// The Entry Point for Encompass,Session will be created Here and same will be passed on for other operations
	/// </summary>
	public sealed class EncompassSession
    {
		private readonly ILoggingService _logService;
        private static int  _applicationId=0;
	    private LogModel _logModel;
	    private ConfigManager _configManager;

        public string UserName { get; set; }
        public string Password { get; set; }
        public string ServerUrl { get; set; }


	    public EncompassSession(string userName, string password, string serverUrl, int applicationId)
	    {
	        _logService = new FileLoggingService(typeof (EncompassSession));
	        _logModel = new LogModel();
	        UserName = userName;
	        Password = password;
	        ServerUrl = serverUrl;
	        _applicationId = applicationId;
	        _configManager = new ConfigManager();
	    }

	    public EncompassSession(int applicationId)
        {
			_logService = new FileLoggingService(typeof(EncompassSession));
            _logModel = new LogModel();
            _configManager = new ConfigManager();
            UserName = _configManager.GetConfigValue("UserId", _applicationId);
            Password = _configManager.GetConfigValue("Password", _applicationId);
            ServerUrl = _configManager.GetConfigValue("ServerUri", _applicationId);
            _applicationId = applicationId;

        }
        #region Private Field Declaration
        

        #endregion
        public IEnumerable<LoanLogicsReport> GetLoanLogicsReport(string report, IEnumerable<string> loanNumbers,string postClosedReportType)
        {
            if (loanNumbers==null)
                throw new ArgumentNullException("loanNumbers");
			var session = new Session();
			try
			{
				session.Start(ServerUrl, UserName, Password);
				var extractor = new Extractor(loanNumbers);
				return extractor.GetReport(report, postClosedReportType, session);
			}
			catch (Exception ex)
			{
                LogHelper.Error(_logService,_logModel, _applicationId,ex.Message,ex:ex);
		
			}
			finally
			{
				session.End();
			}
			
			return null;

        }

        public IEnumerable<PostCloseReport> GetLoanLogicsPostCloseReport(string report, IEnumerable<string> loanNumbers, string postClosedReportType)
        {
            if (loanNumbers == null)
                throw new ArgumentNullException("loanNumbers");
			var session = new Session();
			try
			{
				session.Start(ServerUrl, UserName, Password);
				var extractor = new Extractor(loanNumbers);
				return extractor.GetPostCloseReport(report, postClosedReportType, session);
			}
			catch (Exception ex)
			{
                LogHelper.Error(_logService, _logModel, _applicationId, ex.Message, ex: ex);
            }
			finally
			{
				session.End();
			}
			return null;
        }

        public LoanReportDataList GetReportFiedValuesByLoanNumbers(IEnumerable<string> loanNumbers, IEnumerable<string> fieldIDs)
        {
			var session = new Session();
			try
			{
				session.Start(ServerUrl, UserName, Password);
				return Common.GetReportFiedValuesByLoanNumbers(loanNumbers, fieldIDs, session);
			}
			catch (Exception ex)
			{
                LogHelper.Error(_logService, _logModel, _applicationId, ex.Message, ex: ex);
            }
			finally
			{
				session.End();
			}
			return null;
        }
		public bool UnMarkLoansSelectedForQC(out List<string> loansUnmarkedForQCOut)
		{
			var session = new Session();
			session.Start(ServerUrl, UserName, Password);
			LoanLogicQC loanLogicQC = new LoanLogicQC();
			var result= loanLogicQC.UnMarkLoansSelectedForQC(session, out loansUnmarkedForQCOut);
			session.End();
			return result;
		}

        public bool MarkLoansForQC(int randomPercent, List<string> loanFolders, out List<string> allLoansFromPreFundingReport, out List<string> loansMarkedForQCOut)
        {
			var session = new Session();
			session.Start(ServerUrl, UserName, Password);
			LoanLogicQC loanLogicQC = new LoanLogicQC();
			var result = loanLogicQC.MarkLoansForQC(session, randomPercent, loanFolders, out allLoansFromPreFundingReport, out loansMarkedForQCOut);
			session.End();
			return result;
        }
        public CorrespondentQAResponse GetLoanInfoForQaCorrespondent(IEnumerable<string> loanFolders, DateTime ClosedLoanPackageRecDate, bool SweepComplete, IEnumerable<string> sourceChannel, List<string> qaCanonicalNameList, string canonicalSweepCompleteFieldId)
        {
            CorrespondentQAResponse correspondentQaResponse = null;
            try
            {
                var session = new Session();
                session.Start(ServerUrl, UserName, Password);
                CorrespondentQA correspondentQa = new CorrespondentQA();
                LoanReportDataList loanReportDataList = correspondentQa.GetLoanReportDataForQACorrespondent(loanFolders.ToList(), ClosedLoanPackageRecDate, SweepComplete,
                    sourceChannel.ToList(), session, qaCanonicalNameList, canonicalSweepCompleteFieldId);
                if (loanReportDataList != null)
                {
                    correspondentQaResponse = correspondentQa.FillResponseWithReportData(loanReportDataList, canonicalSweepCompleteFieldId);
                }
                session.End();
                return correspondentQaResponse;
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logService, _logModel, _applicationId, ex.Message, ex: ex);
                throw ex;
            }
           
        }

        public IEnumerable<CorrespondentQAPlaceHoder> GetAttachmentsFromPlaceHoder(ExtractAttachmentDetails extractAttachmentDetails)
        {
	        try
	        {
                var session = new Session();
                session.Start(ServerUrl, UserName, Password);
                CorrespondentQA correspondentQa = new CorrespondentQA();
                var correspondentQaPlaceHoderlist = correspondentQa.GetAttachmentsFromPlaceHoder(extractAttachmentDetails, session);
                session.End();

                return correspondentQaPlaceHoderlist;
	        }
	        catch (Exception ex)
	        {
                LogHelper.Error(_logService, _logModel, _applicationId, ex.Message, ex: ex);
                throw;
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
    }
}