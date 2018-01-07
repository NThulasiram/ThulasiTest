using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncompassLibrary
{
    public class EncompassLibraryConstants
    {
        public const string PRECLOSEREPORT = "2001";
        public const string POSTCLOSEREPORT = "2002";
        public const string ADVERSEREPORT = "2003";

        public const string EARLY_PAYMENT_DEFAULT = "2004";
        public const string TARGETED_REPORT= "2005";
        public const string DISCRETIONARY_REPORT = "2006";
        public const string RANDOM_REPORT = "2007";
        public const string MERGEDFILESIZELIMIT = "MERGEDFILESIZELIMIT_INMB";
        public const string ALLOW_PARTIALFTP_UPLOAD = "ALLOWPARTIALFTPUPLOAD";

        #region ConanicalNames
        public const string CONANICAL_LOANNUMBER = "Loan.LoanNumber";
        public const string CONANICAL_LOANFOLDER = "Loan.LoanFolder";
        public const string CONANICAL_CLOSEDLOANPAKRECDATE = "Fields.CX.CORR.CLPKGRECDATE";
        //public const string CONANICAL_SWEEPCOMPLETE = "Fields.CX.CORR.SWEEPCOMPLETE";
        public const string CONANICAL_CHANNELSOURCE = "Fields.CUST03FV";
        public const string CONANICAL_BORROWER_FIRSTNAME = "Fields.4000";
        public const string CONANICAL_BORROWER_LASTNAME = "Fields.4002";
        public const string CONANICAL_GUID = "Loan.Guid";
        #endregion
        public const int CORRESPONDENTQA_APPLICATION_ID = 3;
        public const int LOANLOGICSQC_APPLICATION_ID = 2;
    }
}
