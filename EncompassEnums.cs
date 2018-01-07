namespace EncompassLibrary.Utilities
{
    /// <summary>
    /// Gets the eFolder type of the document
    /// </summary>
    public enum EncompassDocumentType
    {
        SettlementService=0,
        ClosingDocument=1,
        eDisclosure=2,
        Verification=3,
        Needed=4,
        StandaradForm=5,
        CustomForm=6
    }
    public enum SizeType
    {
        B=0,
        KB=1,
        MB=2
    }
    /// <summary>
    /// Used in Document Extraction 
    /// </summary>
    public enum FileExtension
    {
        pdf=0,
        Other=1
    }
    /// <summary>
    /// Type of Loan Logics Report
    /// </summary>
    public enum LogicsReportType
    {
        PreClose=0,
        PostClose=1,
        Adverse=2
    }
    public enum CriteriaFor
    {
        DocumentName=0,
        PlaceholderName=1
    }
    public enum CriteriaMatch
    {
        EqualTo=0,
        NotEqualTo=1
    }
    /// <summary>
    /// Post Close Report Type
    /// </summary>
    public enum PostCloseLoanType
    {
        EPD=0,
        Targeted=1,
        Discretionary=2,
        Random=3,
        None=4

    }
    public enum RawLoanLogicsReportName
    {
        LOANLOGICSREPORTTYPES1 = 0,
        LOANLOGICSREPORTTYPES2 = 1,
        LOANLOGICSREPORTTYPES3 = 2
    }
}
