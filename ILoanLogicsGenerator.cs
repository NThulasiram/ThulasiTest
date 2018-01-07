using EllieMae.Encompass.Collections;
using FGMC.Common.DataContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncompassLibrary.LoanManager
{
    public interface ILoanLogicsGenerator
    {
        IEnumerable<LoanLogicsReport> GenerateLoanLogicsReportDataAsync(LoanLogicsReport loanLogicsReport, LoanReportDataList dataList);
        IEnumerable<LoanLogicsReport> GeneratePostCloseLoanLogicsReportDataAsync(LoanLogicsReport loanLogicsReport, LoanReportDataList dataList);


    }
}
