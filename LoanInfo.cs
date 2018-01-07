namespace EncompassLibrary
{
    public class LoanInfo
    {
        public string Guid { get; set; }
        public string LoanNumber { get; set; }

        public LoanInfo(string guid, string loanNumber)
        {
            Guid = guid;
            LoanNumber = loanNumber;
        }
    }
}
