using System.Collections.Generic;

namespace FGMC.SMTPLibrary
{
	public class EmailInfo
	{
		public List<string> LoanNumbers { get; set; }
		public long BatchId { get; set; }
		public string MergedFileName { get; set; }
		public string FileName { get; set; }
		public string FolderName { get; set; }
		public string CopyStatus { get; set; } 
		public string LoanNumber { get; set; }
		public string StatusDescription { get; set; }
        public string Reason { get; set; }
        public EmailInfo()
		{
			LoanNumbers = new List<string>();
		}
	}
}
