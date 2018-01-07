using EllieMae.Encompass.BusinessObjects.Loans;
using EllieMae.Encompass.Client;
using FGMC.Common.DataContract;
using System.Collections.Generic;

namespace EncompassLibrary.LoanManager
{
	public interface IEncompassLoanManager
	{
        IEnumerable<LoanLogicsReport> GetLoanAsLoanLogicsReport(IEnumerable<string> loanNumbers, string loanLogicsReportType, bool isPostCloseReport, Session session,int applicationId= 0);
		string GetCurrentUserId();
	}
}
