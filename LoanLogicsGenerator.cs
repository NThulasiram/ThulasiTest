using EllieMae.Encompass.Client;
using EllieMae.Encompass.Collections;
using EllieMae.Encompass.Reporting;
using EncompassLibrary.Utilities;
using FGMC.Common.DataContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Log4NetLibrary;

namespace EncompassLibrary.LoanDataExtractor
    {
    public class LoanLogicsGenerator
        {
        public IEnumerable<LoanLogicsReport> CopyAndGenerateReport(IEnumerable<string> loanNumbers, Session session)
            {
            Func<object, string> checkNullFunc = CheckForNull;
            if (loanNumbers == null) throw new ArgumentNullException("loanNumbers");
            var stringGuidList = Common.SetStringGuidList(loanNumbers, session);
            var stringFieldList = Common.SetStringList();
            var dataList = session.Reports.SelectReportingFieldsForLoans(stringGuidList, stringFieldList);
            var reportData = new List<LoanLogicsReport>();

            foreach (LoanReportData data in dataList)
                {
                var report = new AdverseReport();
                report.Amortization = checkNullFunc(data[Fields.Amortization]);
                report.AnnualPercentageRate = checkNullFunc(data[Fields.AnnualPercentageRate]);
                report.ApplicationDate = checkNullFunc(data[Fields.ApplicationDate]);
                report.AppraisedValue = checkNullFunc(data[Fields.AppraisedValue]);
                report.Borrower1FirstName = checkNullFunc(data[Fields.Borrower1FirstName]);
                report.Borrower1LastName = checkNullFunc(data[Fields.Borrower1LastName]);
                report.Borrower1SSN = checkNullFunc(data[Fields.Borrower1SSN]);
                report.Borrower2FirstName = checkNullFunc(data[Fields.Borrower2FirstName]);
                report.Borrower2LastName = checkNullFunc(data[Fields.Borrower2LastName]);
                report.Borrower2SSN = checkNullFunc(data[Fields.Borrower2SSN]);
                report.InterestRate = checkNullFunc(data[Fields.InterestRate]);
                report.LoanOfficerName = checkNullFunc(data[Fields.LoanOfficerName]);
                report.LoanNumber = checkNullFunc(data[Fields.LoanNumber]);
                report.LTV = checkNullFunc(data[Fields.LTV]);
                report.MortgageAppliedFor = checkNullFunc(data[Fields.MortgageAppliedFor]);
                report.PropertyUnitNo = checkNullFunc(data[Fields.PropertyUnitNo]);
                report.PropertyCity = checkNullFunc(data[Fields.PropertyCity]);
                report.PropertyState = checkNullFunc(data[Fields.PropertyState]);
                report.PropertyStreet1 = checkNullFunc(data[Fields.PropertyAddressline1]);
                report.PropertyType = checkNullFunc(data[Fields.PropertyType]);
                report.PropertyWillBe = checkNullFunc(data[Fields.PropertyWillBe]);
                report.PropertyZipcode = checkNullFunc(data[Fields.PropertyZipcode]);
                report.PurchasePrice = checkNullFunc(data[Fields.PurchasePrice]);
                report.PurposeofLoan = checkNullFunc(data[Fields.PurposeofLoan]);
                report.TotalLoanAmount = checkNullFunc(data[Fields.TotalLoanAmount]);
                report.UnderwriterName = checkNullFunc(data[Fields.UnderwriterName]);
                reportData.Add(report);
                }
            return reportData;
            }
        public IEnumerable<LoanLogicsReport> GetReportData(LoanLogicsReport loanLogicsReport, IEnumerable<string> loanNumbers, Session session, PostCloseLoanType postCloseReportType)
            {
            if (loanNumbers == null) throw new ArgumentNullException("loanNumbers");
            var stringGuidList = Common.SetStringGuidList(loanNumbers, session);
            if (stringGuidList == null || stringGuidList.Count == 0) return null;

            var stringFieldList = Common.SetStringFieldList(session);
            var dataList = session.Reports.SelectReportingFieldsForLoans(stringGuidList, stringFieldList);
            var reportList = FillReportData(loanLogicsReport, dataList);
            return reportList;
            }
        public IEnumerable<PostCloseReport> CreatePostCloseReportData(LoanLogicsReport loanLogicsReport, IEnumerable<string> loanNumbers, Session session)
            {
            if (loanNumbers == null) throw new ArgumentNullException("loanNumbers");
            var stringGuidList = Common.SetStringGuidList(loanNumbers, session);
            if (stringGuidList == null || stringGuidList.Count == 0) return null;

            var stringFieldList = Common.SetStringFieldList(session);

            var dataList = session.Reports.SelectReportingFieldsForLoans(stringGuidList, stringFieldList);
            var reportList = FillPostCloseReportData(loanLogicsReport, dataList);

            return reportList;
            }
        public IEnumerable<PostCloseReport> GetPostCloseReportData(LoanLogicsReport loanLogicsReport, IEnumerable<string> loanNumbers, Session session, PostCloseLoanType postCloseReportType)
            {
            if (loanNumbers == null) throw new ArgumentNullException("loanNumbers");
            var stringGuidList = Common.SetStringGuidList(loanNumbers, session);
            if (stringGuidList == null || stringGuidList.Count == 0) return null;

            var stringFieldList = Common.SetStringFieldList(session);

            var dataList = session.Reports.SelectReportingFieldsForLoans(stringGuidList, stringFieldList);
            var reportList = FillPostCloseReportData(loanLogicsReport, dataList);

            return reportList;
            }
        public async Task<List<LoanLogicsReport>> GenerateLoanLogicsReportDataAsync(LoanLogicsReport loanLogicsReport, LoanReportDataList dataList)
            {
            Task<List<LoanLogicsReport>> fillDataTask = Task.Run(() => FillReportData(loanLogicsReport, dataList));
            var waitForCompletion = await fillDataTask;
            return waitForCompletion;
            }
        public async Task<List<PostCloseReport>> GeneratePostCloseLoanLogicsReportDataAsync(LoanLogicsReport loanLogicsReport, LoanReportDataList dataList)
            {
            Task<List<PostCloseReport>> fillDataTask = Task.Run(() => FillPostCloseReportData(loanLogicsReport, dataList));
            var waitForCompletion = await fillDataTask;
            return waitForCompletion;
            }

        private List<LoanLogicsReport> FillReportData(LoanLogicsReport loanLogicsReport, LoanReportDataList dataList)
            {
            LoanLogicsReport report = null;

            var reports = new List<LoanLogicsReport>();
            Func<object, string> checkNullFunc = CheckForNull;
            foreach (LoanReportData data in dataList)
                {
                try
                    {
                    if (loanLogicsReport is PreCloseReport)
                        report = new PreCloseReport();
                    if (loanLogicsReport is AdverseReport)
                        report = new AdverseReport();
                    report.Amortization = checkNullFunc(data[Fields.Amortization]);
                    report.AnnualPercentageRate = CheckDecimalForNull(data[Fields.AnnualPercentageRate], 3);
                    report.ApplicationDate = CheckDateForNull(data[Fields.ApplicationDate]);
                    report.AppraisedValue = CheckDecimalForNull(data[Fields.AppraisedValue], 0);
                    report.Borrower1FirstName = checkNullFunc(data[Fields.Borrower1FirstName]);
                    report.Borrower1LastName = checkNullFunc(data[Fields.Borrower1LastName]);
                    report.Borrower1SSN = checkNullFunc(data[Fields.Borrower1SSN]);
                    report.Borrower2FirstName = checkNullFunc(data[Fields.Borrower2FirstName]);
                    report.Borrower2LastName = checkNullFunc(data[Fields.Borrower2LastName]);
                    report.Borrower2SSN = checkNullFunc(data[Fields.Borrower2SSN]);
                    report.Borrower3FirstName = checkNullFunc(data[Fields.Borrower3FirstName]);
                    report.Borrower3LastName = checkNullFunc(data[Fields.Borrower3LastName]);
                    report.Borrower3SSN = checkNullFunc(data[Fields.Borrower3SSN]);
                    report.Borrower4FirstName = checkNullFunc(data[Fields.Borrower4FirstName]);
                    report.Borrower4LastName = checkNullFunc(data[Fields.Borrower4LastName]);
                    report.Borrower4SSN = checkNullFunc(data[Fields.Borrower4SSN]);
                    report.Borrower1MiddleInitial = checkNullFunc(data[Fields.Borrower1MiddleInitial]) == string.Empty ? string.Empty : data[Fields.Borrower1MiddleInitial].ToString()[0].ToString();
                    report.Borrower2MiddleInitial = checkNullFunc(data[Fields.Borrower2MiddleInitial]) == string.Empty ? string.Empty : data[Fields.Borrower2MiddleInitial].ToString()[0].ToString();
                    report.InterestRate = CheckDecimalForNull(data[Fields.InterestRate], 3);
                    report.LoanOfficerName = checkNullFunc(data[Fields.LoanOfficerName]);
                    report.LoanNumber = checkNullFunc(data[Fields.LoanNumber]);
                    report.LTV = CheckDecimalForNull(data[Fields.LTV], 3);
                    report.MortgageAppliedFor = checkNullFunc(data[Fields.MortgageAppliedFor]);
                    report.PropertyUnitNo = CheckDecimalForNull(data[Fields.PropertyUnitNo], 0);
                    report.PropertyCity = checkNullFunc(data[Fields.PropertyCity]);
                    report.PropertyState = checkNullFunc(data[Fields.PropertyState]);
                    report.PropertyStreet1 = checkNullFunc(data[Fields.PropertyAddressline1]);
                    report.PropertyType = checkNullFunc(data[Fields.PropertyType]);
                    report.PropertyWillBe = checkNullFunc(data[Fields.PropertyWillBe]);
                    report.PropertyZipcode = checkNullFunc(data[Fields.PropertyZipcode]);
                    report.PurchasePrice = CheckDecimalForNull(data[Fields.PurchasePrice], 2);
                    report.PurposeofLoan = checkNullFunc(data[Fields.PurposeofLoan]);
                    report.TotalLoanAmount = CheckDecimalForNull(data[Fields.TotalLoanAmount], 2);
                    report.UnderwriterName = checkNullFunc(data[Fields.UnderwriterName]);
                    report.UnpaidPrincipalBalance = CheckDecimalForNull(data[Fields.UnpaidPrincipalBalance], 2);
                    reports.Add(report);
                    }
                catch (Exception ex)
                    {
                    reports.Add(report);
                    continue;
                    }

                }
            return reports;
            }
        private List<PostCloseReport> FillPostCloseReportData(LoanLogicsReport loanLogicsReport, LoanReportDataList dataList)
            {

            PostCloseReport report = null;

            var reports = new List<PostCloseReport>();
            Func<object, string> checkNullFunc = CheckForNull;

            foreach (LoanReportData data in dataList)
                {
                try
                    {
                    report = new PostCloseReport();
                    report.Amortization = checkNullFunc(data[Fields.Amortization]);
                    report.AnnualPercentageRate = CheckDecimalForNull(data[Fields.AnnualPercentageRate], 3);
                    report.ApplicationDate = CheckDateForNull(data[Fields.ApplicationDate]);
                    report.AppraisedValue = CheckDecimalForNull(data[Fields.AppraisedValue], 0);
                    report.Borrower1FirstName = checkNullFunc(data[Fields.Borrower1FirstName]);
                    report.Borrower1LastName = checkNullFunc(data[Fields.Borrower1LastName]);
                    report.Borrower1SSN = checkNullFunc(data[Fields.Borrower1SSN]);
                    report.Borrower2FirstName = checkNullFunc(data[Fields.Borrower2FirstName]);
                    report.Borrower2LastName = checkNullFunc(data[Fields.Borrower2LastName]);
                    report.Borrower2SSN = checkNullFunc(data[Fields.Borrower2SSN]);
                    report.Borrower3FirstName = checkNullFunc(data[Fields.Borrower3FirstName]);
                    report.Borrower3LastName = checkNullFunc(data[Fields.Borrower3LastName]);
                    report.Borrower3SSN = checkNullFunc(data[Fields.Borrower3SSN]);
                    report.Borrower4FirstName = checkNullFunc(data[Fields.Borrower4FirstName]);
                    report.Borrower4LastName = checkNullFunc(data[Fields.Borrower4LastName]);
                    report.Borrower4SSN = checkNullFunc(data[Fields.Borrower4SSN]);
                    report.Borrower1MiddleInitial = checkNullFunc(data[Fields.Borrower1MiddleInitial]) == string.Empty ? string.Empty : data[Fields.Borrower1MiddleInitial].ToString()[0].ToString();
                    report.Borrower2MiddleInitial = checkNullFunc(data[Fields.Borrower2MiddleInitial]) == string.Empty ? string.Empty : data[Fields.Borrower2MiddleInitial].ToString()[0].ToString();

                    report.InterestRate = CheckDecimalForNull(data[Fields.InterestRate], 3);
                    report.LoanOfficerName = checkNullFunc(data[Fields.LoanOfficerName]);
                    report.LoanNumber = checkNullFunc(data[Fields.LoanNumber]);
                    report.LTV = CheckDecimalForNull(data[Fields.LTV], 3);
                    report.MortgageAppliedFor = checkNullFunc(data[Fields.MortgageAppliedFor]);
                    report.PropertyUnitNo = CheckDecimalForNull(data[Fields.PropertyUnitNo], 0);
                    report.PropertyCity = checkNullFunc(data[Fields.PropertyCity]);
                    report.PropertyState = checkNullFunc(data[Fields.PropertyState]);
                    report.PropertyStreet1 = checkNullFunc(data[Fields.PropertyAddressline1]);
                    report.PropertyType = checkNullFunc(data[Fields.PropertyType]);
                    report.PropertyWillBe = checkNullFunc(data[Fields.PropertyWillBe]);
                    report.PropertyZipcode = checkNullFunc(data[Fields.PropertyZipcode]);
                    report.PurchasePrice = CheckDecimalForNull(data[Fields.PurchasePrice], 2);
                    report.PurposeofLoan = checkNullFunc(data[Fields.PurposeofLoan]);
                    report.TotalLoanAmount = CheckDecimalForNull(data[Fields.TotalLoanAmount], 2);
                    report.UnderwriterName = checkNullFunc(data[Fields.UnderwriterName]);
                    report.UnpaidPrincipalBalance = CheckDecimalForNull(data[Fields.UnpaidPrincipalBalance], 2);
                    //Post Closed Fields
                    report.ClosingDate = CheckDateForNull(data[Fields.ClosingDate]);
                    report.MINNumber = checkNullFunc(data[Fields.MINNumber]);
                    report.CaseNumber = checkNullFunc(data[Fields.CaseNumber]);
                    report.PaymentAmount = CheckDecimalForNull(data[Fields.PaymentAmount], 2);
                    report.FirstPaymentDate = CheckDateForNull(data[Fields.FirstPaymentDate]);
                    report.MaturityDate = CheckDateForNull(data[Fields.MaturityDate]);
                    report.DisbursementDate = CheckDateForNull(data[Fields.DisbursementDate]);
                    report.StreamlineType = checkNullFunc(data[Fields.StreamlineType]);
                    report.LoanTerm = CheckDecimalForNull(data[Fields.LoanTerm], 0);
                    report.CLTV = CheckDecimalForNull(data[Fields.CLTV], 3);
                    report.BackRatio = CheckDecimalForNull(data[Fields.BackRatio], 3);
                    report.PrimaryFICO = checkNullFunc(data[Fields.PrimaryFICO]);
                    report.UnderwritingStandard = checkNullFunc(data[Fields.UnderwritingStandard]);
                    report.BranchName = checkNullFunc(data[Fields.BranchName]);
                    report.CloserName = checkNullFunc(data[Fields.CloserName]);
                    report.ProcessorName = checkNullFunc(data[Fields.ProcessorName]);
                    report.OriginationCompany = checkNullFunc(data[Fields.OriginationCompany]);
                    report.Name1EthnicityBox = checkNullFunc(data[Fields.Name1EthnicityBox]);
                    report.Name2EthnicityBox = checkNullFunc(data[Fields.Name2EthnicityBox]);
                    report.Name1RaceBox = checkNullFunc(data[Fields.Name1RaceBox]);
                    report.Name2RaceBox = checkNullFunc(data[Fields.Name2RaceBox]);
                    report.Name1SexBox = checkNullFunc(data[Fields.Name1SexBox]);
                    report.Name2SexBox = checkNullFunc(data[Fields.Name2SexBox]);
                    report.FrontRatio = CheckDecimalForNull(data[Fields.FrontRatio], 3);
                    report.Investor = checkNullFunc(data[Fields.Investor]);
                    report.TPO = checkNullFunc(data[Fields.TPO]);
                    report.Channel = checkNullFunc(data[Fields.Channel]);
                    report.Borrower1SelfEmployed = checkNullFunc(data[Fields.Borrower1SelfEmployed]) == string.Empty ? "No" : "Yes";
                    report.Borrower2SelfEmployed = checkNullFunc(data[Fields.Borrower2SelfEmployed]) == string.Empty ? "No" : "Yes";
                    report.Borrower3MiddleInitial = string.Empty;
                    report.Borrower4MiddleInitial = string.Empty;


                    if (data[Fields.MortgageAppliedFor] != null && data[Fields.ExpensesProposedMtgIns] != null && data[Fields.FeesMtgInsPremiumBorr] != null)
                        {
                        if (data[Fields.MortgageAppliedFor].ToString() == "Conventional" && ((!string.IsNullOrEmpty(data[Fields.ExpensesProposedMtgIns].ToString()) && Convert.ToDecimal(data[Fields.ExpensesProposedMtgIns].ToString()) > 0) || (!string.IsNullOrEmpty(data[Fields.FeesMtgInsPremiumBorr].ToString()) && Convert.ToDecimal(data[Fields.FeesMtgInsPremiumBorr].ToString()) > 0)))
                            {
                            report.PMI = "Yes";
                            }
                        else
                            {
                            report.PMI = "No";
                            }

                        }
                    reports.Add(report);
                    }
                catch (Exception ex)
                    {
                    reports.Add(report);
                    continue;
                    }

                }

            return reports;
            }

        public List<CorrespondentQAPostCloseReport> FillQAPostCloseReportData(LoanLogicsReport loanLogicsReport, LoanReportDataList dataList)
            {

            CorrespondentQAPostCloseReport report = null;

            var reports = new List<CorrespondentQAPostCloseReport>();
            Func<object, string> checkNullFunc = CheckForNull;

            foreach (LoanReportData data in dataList)
                {
                try
                    {
                    report = new CorrespondentQAPostCloseReport();
                    report.Amortization = checkNullFunc(data[Fields.Amortization]);
                    report.AnnualPercentageRate = CheckDecimalForNull(data[Fields.AnnualPercentageRate], 3);
                    report.ApplicationDate = CheckDateForNull(data[Fields.ApplicationDate]);
                    report.AppraisedValue = CheckDecimalForNull(data[Fields.AppraisedValue], 0);
                    report.Borrower1FirstName = checkNullFunc(data[Fields.Borrower1FirstName]);
                    report.Borrower1LastName = checkNullFunc(data[Fields.Borrower1LastName]);
                    report.Borrower1SSN = checkNullFunc(data[Fields.Borrower1SSN]);
                    report.Borrower2FirstName = checkNullFunc(data[Fields.Borrower2FirstName]);
                    report.Borrower2LastName = checkNullFunc(data[Fields.Borrower2LastName]);
                    report.Borrower2SSN = checkNullFunc(data[Fields.Borrower2SSN]);
                    report.Borrower3FirstName = checkNullFunc(data[Fields.Borrower3FirstName]);
                    report.Borrower3LastName = checkNullFunc(data[Fields.Borrower3LastName]);
                    report.Borrower3SSN = checkNullFunc(data[Fields.Borrower3SSN]);
                    report.Borrower4FirstName = checkNullFunc(data[Fields.Borrower4FirstName]);
                    report.Borrower4LastName = checkNullFunc(data[Fields.Borrower4LastName]);
                    report.Borrower4SSN = checkNullFunc(data[Fields.Borrower4SSN]);
                    report.Borrower1MiddleInitial = checkNullFunc(data[Fields.Borrower1MiddleInitial]) == string.Empty ? string.Empty : data[Fields.Borrower1MiddleInitial].ToString()[0].ToString();
                    report.Borrower2MiddleInitial = checkNullFunc(data[Fields.Borrower2MiddleInitial]) == string.Empty ? string.Empty : data[Fields.Borrower2MiddleInitial].ToString()[0].ToString();

                    report.InterestRate = CheckDecimalForNull(data[Fields.InterestRate], 3);
                    report.LoanOfficerName = checkNullFunc(data[Fields.LoanOfficerName]);
                    report.LoanNumber = checkNullFunc(data[Fields.LoanNumber]);
                    report.LTV = CheckDecimalForNull(data[Fields.LTV], 3);
                    report.MortgageAppliedFor = checkNullFunc(data[Fields.MortgageAppliedFor]);
                    report.PropertyUnitNo = CheckDecimalForNull(data[Fields.PropertyUnitNo], 0);
                    report.PropertyCity = checkNullFunc(data[Fields.PropertyCity]);
                    report.PropertyState = checkNullFunc(data[Fields.PropertyState]);
                    report.PropertyStreet1 = checkNullFunc(data[Fields.PropertyAddressline1]);
                    report.PropertyType = checkNullFunc(data[Fields.PropertyType]);
                    report.PropertyWillBe = checkNullFunc(data[Fields.PropertyWillBe]);
                    report.PropertyZipcode = checkNullFunc(data[Fields.PropertyZipcode]);
                    report.PurchasePrice = CheckDecimalForNull(data[Fields.PurchasePrice], 2);
                    report.PurposeofLoan = checkNullFunc(data[Fields.PurposeofLoan]);
                    report.TotalLoanAmount = CheckDecimalForNull(data[Fields.TotalLoanAmount], 2);
                    report.UnderwriterName = checkNullFunc(data[Fields.UnderwriterName]);
                    report.UnpaidPrincipalBalance = CheckDecimalForNull(data[Fields.UnpaidPrincipalBalance], 2);
                    //Post Closed Fields
                    report.ClosingDate = CheckDateForNull(data[Fields.ClosingDate]);
                    report.MINNumber = checkNullFunc(data[Fields.MINNumber]);
                    report.CaseNumber = checkNullFunc(data[Fields.CaseNumber]);
                    report.PaymentAmount = CheckDecimalForNull(data[Fields.PaymentAmount], 2);
                    report.FirstPaymentDate = CheckDateForNull(data[Fields.FirstPaymentDate]);
                    report.MaturityDate = CheckDateForNull(data[Fields.MaturityDate]);
                    report.DisbursementDate = CheckDateForNull(data[Fields.DisbursementDate]);
                    report.StreamlineType = checkNullFunc(data[Fields.StreamlineType]);
                    report.LoanTerm = CheckDecimalForNull(data[Fields.LoanTerm], 0);
                    report.CLTV = CheckDecimalForNull(data[Fields.CLTV], 3);
                    report.BackRatio = CheckDecimalForNull(data[Fields.BackRatio], 3);
                    report.PrimaryFICO = checkNullFunc(data[Fields.PrimaryFICO]);
                    report.UnderwritingStandard = checkNullFunc(data[Fields.UnderwritingStandard]);
                    report.BranchName = checkNullFunc(data[Fields.BranchName]);
                    report.CloserName = checkNullFunc(data[Fields.CloserName]);
                    report.ProcessorName = checkNullFunc(data[Fields.ProcessorName]);
                    report.OriginationCompany = checkNullFunc(data[Fields.OriginationCompany]);
                    report.Name1EthnicityBox = checkNullFunc(data[Fields.Name1EthnicityBox]);
                    report.Name2EthnicityBox = checkNullFunc(data[Fields.Name2EthnicityBox]);
                    report.Name1RaceBox = checkNullFunc(data[Fields.Name1RaceBox]);
                    report.Name2RaceBox = checkNullFunc(data[Fields.Name2RaceBox]);
                    report.Name1SexBox = checkNullFunc(data[Fields.Name1SexBox]);
                    report.Name2SexBox = checkNullFunc(data[Fields.Name2SexBox]);
                    report.FrontRatio = CheckDecimalForNull(data[Fields.FrontRatio], 3);
                    report.Investor = checkNullFunc(data[Fields.Investor]);
                    report.TPO = checkNullFunc(data[Fields.TPO]);
                    report.Channel = checkNullFunc(data[Fields.Channel]);
                    report.Borrower1SelfEmployed = checkNullFunc(data[Fields.Borrower1SelfEmployed]) == string.Empty ? "No" : "Yes";
                    report.Borrower2SelfEmployed = checkNullFunc(data[Fields.Borrower2SelfEmployed]) == string.Empty ? "No" : "Yes";
                    report.Borrower3MiddleInitial = string.Empty;
                    report.Borrower4MiddleInitial = string.Empty;


                    if (data[Fields.MortgageAppliedFor] != null && data[Fields.ExpensesProposedMtgIns] != null && data[Fields.FeesMtgInsPremiumBorr] != null)
                        {
                        if (data[Fields.MortgageAppliedFor].ToString() == "Conventional" && ((!string.IsNullOrEmpty(data[Fields.ExpensesProposedMtgIns].ToString()) && Convert.ToDecimal(data[Fields.ExpensesProposedMtgIns].ToString()) > 0) || (!string.IsNullOrEmpty(data[Fields.FeesMtgInsPremiumBorr].ToString()) && Convert.ToDecimal(data[Fields.FeesMtgInsPremiumBorr].ToString()) > 0)))
                            {
                            report.PMI = "Yes";
                            }
                        else
                            {
                            report.PMI = "No";
                            }

                        }

                    ////Newly added fields as per change request
                    report.TPOCompanyName = checkNullFunc(data[Fields.TPOCompanyName]);
                    report.TPOCompanyID = checkNullFunc(data[Fields.TPOCompanyId]);

                    ////Newly added fields as per change request 27-10-2016
                    report.ClosedLoanPackageReceivedDate = CheckDateForNull(data[Fields.ClosedLoanPackageReceivedDate]);

                    reports.Add(report);
                    }
                catch (Exception ex)
                    {
                    reports.Add(report);
                    continue;
                    }

                }

            return reports;
            }

        public List<QAMonthlyReport> FillQAMonthlyReportData(LoanReportDataList dataList, LoanData loanData)
            {
            QAMonthlyReport report = null;
            var reports = new List<QAMonthlyReport>();
            Func<object, string> checkNullFunc = CheckForNull;

            foreach (LoanReportData data in dataList)
                {
                try
                    {
                    report = new QAMonthlyReport();
                    report.LoanNumber = GetPreviousLoanNumber(data, loanData);
                    report.CurrentLoanNumber = checkNullFunc(data[MonthlyReportFields.LoanNumber]);
                    report.BorrowerLastName = checkNullFunc(data[MonthlyReportFields.BorrowerLastName]);
                    report.DataFileUploadTime = CheckDateForNull(data[MonthlyReportFields.DataFileUploadTime]);
                    report.LoanProgram = checkNullFunc(data[MonthlyReportFields.LoanProgram]);
                    report.TPOCompanyId = checkNullFunc(data[MonthlyReportFields.TPOCompanyId]);
                    report.TPOCompanyName = checkNullFunc(data[MonthlyReportFields.TPOCompanyName]);
                    report.PropertyState = checkNullFunc(data[MonthlyReportFields.PropertyStateMonthlyReport]);
                    report.PurchasePrice = CheckDecimalForNull(data[MonthlyReportFields.PurchasePriceMonthlyReport], 2);
                    reports.Add(report);
                    }
                catch (Exception ex)
                    {
                    reports.Add(report);
                    }
                }

            return reports;
            }

        private string GetPreviousLoanNumber(LoanReportData data, LoanData loanData)
        {
           // var loanNumber = loanData.LoanDetail.Where(p => p.Key == data.Guid).FirstOrDefault().Value;
            var loanNumber = loanData.LoanDetail.FirstOrDefault(p => p.Key == data.Guid).Value;
            return string.IsNullOrEmpty(loanNumber) ? string.Empty : loanNumber;
        }

        private string CheckForNull(object valueToCheck)
            {
            return valueToCheck != null && !string.IsNullOrEmpty(valueToCheck.ToString()) ? valueToCheck.ToString() : string.Empty;
            }
        private string CheckDecimalForNull(object valueToCheck, int Precision)
            {
            try
                {
                if (valueToCheck != null && !string.IsNullOrEmpty(valueToCheck.ToString()))
                    {
                    switch (Precision)
                        {
                        case 0:
                            return Convert.ToDecimal(valueToCheck).ToString("0");
                        case 2:
                            return Convert.ToDecimal(valueToCheck).ToString("0.00");
                        case 3:
                            return Convert.ToDecimal(valueToCheck).ToString("0.000");

                        default:
                            return valueToCheck.ToString();
                        }
                    }
                return string.Empty;
                }
            catch (Exception)
                {
                return string.Empty;
                }

            }
        private string CheckDateForNull(object valueToCheck)
            {
            try
                {
                if (valueToCheck != null && !string.IsNullOrEmpty(valueToCheck.ToString()))
                    {
                    return Convert.ToDateTime(valueToCheck.ToString()).ToShortDateString();
                    }
                else
                    {
                    return string.Empty;
                    }

                }
            catch (Exception)
                {
                return string.Empty;
                }

            }
        }
    }
