namespace EncompassLibrary.Utilities
{
    /// <summary>
    /// CanonicalField name in Encompass
    /// </summary>
    public static class CanonicalFields
    {
        public static string LoanNumber = "Loan.LoanNumber";
        public static string Amortization = "Loan.Amortization";
        public static string DateCreated = "Loan.DateCreated";
        public static string AppraisedValue = "Loan.AppraisedValue";
        public static string BorrowerFirstName = "Loan.BorrowerFirstName";
        public static string BorrowerLastName = "Loan.BorrowerLastName";
        public static string LoanGuid = "Loan.Guid";

    }



    public static class Fields
    {
        public static string Amortization = "Fields.608";
        public static string AnnualPercentageRate = "Fields.799";
        public static string ApplicationDate = "Fields.CX.ECOA.APPLICATIONDATE";
        public static string AppraisedValue = "Fields.356";
        public static string Borrower1FirstName = "Fields.4000";
        public static string Borrower1LastName = "Fields.4002";
        public static string Borrower1SSN = "Fields.65";
        public static string Borrower2FirstName = "Fields.4004";
        public static string Borrower2LastName = "Fields.4006";
        public static string Borrower2SSN = "Fields.97";
        public static string Borrower3FirstName = "Fields.2880";
        public static string Borrower3LastName = "Fields.2881";
        public static string Borrower3SSN = "Fields.2882";
        public static string Borrower4FirstName = "Fields.2886";
        public static string Borrower4LastName = "Fields.2887";
        public static string Borrower4SSN = "Fields.2888";
        public static string Borrower1MiddleInitial = "Fields.4001";
        public static string Borrower2MiddleInitial = "Fields.4005";
        public static string InterestRate = "Fields.3";
        public static string LoanOfficerName = "Fields.1612";
        public static string LoanNumber = "Fields.364";
        public static string LTV = "Fields.353";
        public static string MortgageAppliedFor = "Fields.1172";
        public static string PropertyUnitNo = "Fields.16";
        public static string PropertyCity = "Fields.12";
        public static string PropertyState = "Fields.14";
        public static string PropertyAddressline1 = "Fields.11";
        public static string PropertyType = "Fields.1041";
        public static string PropertyWillBe = "Fields.1811";
        public static string PropertyZipcode = "Fields.15";
        public static string PurchasePrice = "Fields.136";
        public static string PurposeofLoan = "Fields.19";
        public static string TotalLoanAmount = "Fields.2";
        public static string UnderwriterName = "Fields.LoanTeamMember.Name.Underwriter";
        public static string UnpaidPrincipalBalance = "Fields.CX.RPT.UPB";
        public static string ClosingDate = "Fields.748";
        public static string MINNumber = "Fields.1051";
        public static string CaseNumber = "Fields.1040";
        public static string PaymentAmount = "Fields.912";
        public static string FirstPaymentDate = "Fields.682";
        public static string MaturityDate = "Fields.78";
        public static string PMI = "Fields.471";
        public static string DisbursementDate = "Fields.2553";
        public static string StreamlineType = "Fields.MORNET.X40";
        public static string LoanTerm = "Fields.4";
        public static string CLTV = "Loan.CLTV";
        public static string PrimaryFICO = "Fields.VASUMM.X23";
        public static string UnderwritingStandard = "Fields.2312";
        public static string BranchName = "Fields.CUST02FV";
        public static string CloserName = "Fields.VEND.X317";
        public static string BackRatio = "Fields.742";
        public static string ProcessorName = "Fields.LoanTeamMember.Name.LP/AM";
        public static string OriginationCompany = "Fields.315";
        public static string Name1EthnicityBox = "Fields.1523";
        public static string Name2EthnicityBox = "Fields.1531";
        public static string Name1RaceBox = "Fields.CX.RPT.LL.BORRRACE";
        public static string Name2RaceBox = "Fields.CX.RPT.LL.COBORRRACE";
        public static string Name1SexBox = "Fields.471";
        public static string Name2SexBox = "Fields.478";
        public static string FrontRatio = "Fields.740";
        public static string Investor = "Fields.VEND.X263";
        public static string TPO = "Fields.VEND.X293";
        public static string Channel = "Fields.2626";
        public static string Borrower1SelfEmployed = "Fields.fe0115";
        public static string Borrower2SelfEmployed = "Fields.3517";
        public static string LoanSeletedForQC = "Fields.CX.QC.LOANSELECTEDFORQC";
        public static string ExpensesProposedMtgIns = "Fields.232";
        public static string FeesMtgInsPremiumBorr = "Fields.337";

        //Newly added fields as per change request of correspondent QA
        public static string TPOCompanyName = "Fields.TPO.X14";
        public static string TPOCompanyId = "Fields.TPO.X15";

        //Newly added fields as per change request of correspondent QA - 03/11/2016
        public static string ClosedLoanPackageReceivedDate = "Fields.CX.CORR.CLPKGRECDATE";
        }

    public static class MonthlyReportFields
    {
        public static string LoanNumber = "Fields.364";
        public static string BorrowerLastName = "Fields.4002";
        public static string TPOCompanyName = "Fields.TPO.X14";
        public static string TPOCompanyId = "Fields.TPO.X15";
       
        public static string DataFileUploadTime = "Fields.CX.CORR.DATESWPCOMP";
        public static string LoanProgram = "Fields.1401";
        public static string PurchasePriceMonthlyReport = "Fields.3038";
        public static string PropertyStateMonthlyReport = "Fields.2945";
    }
}
