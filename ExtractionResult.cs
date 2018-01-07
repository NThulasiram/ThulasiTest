using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncompassLibrary.LoanDataExtractor
{
    public class ExtractionResult
    {
        public ExtractionResult(string loanNumber, List<AttachmentStatus> attachmentStatus)
        {
            LoanNumber = loanNumber;
            AttachmentStatus = attachmentStatus;
        }
        public string LoanNumber { get; set; }
        public List<AttachmentStatus> AttachmentStatus { get; set; }
    }
}
