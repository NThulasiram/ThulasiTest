using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncompassLibrary.LoanDataExtractor
{
    public interface IAttachmentExtractor
    {
        List<ExtractionResult> Extract(List<string> loans);
    }
}
