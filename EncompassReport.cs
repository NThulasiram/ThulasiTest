using System.Collections.Generic;
using System.Data;

namespace EncompassLibrary.Utilities
{
	public class EncompassReport
	{
		private string _reportName;
		public string ReportName
		{ get; set; }
		public string ReportDisplayName {
			get
			{
				if (ReportAsDataTable != null && !string.IsNullOrEmpty(ReportAsDataTable.TableName))
					_reportName = ReportAsDataTable.TableName;
				return _reportName;
			}
			set { _reportName = value; }
		}
		public string SubReportName { get; set; }
		public string SubReportDisplayName { get; set; }
		public string ErrorMessage { get; set; }
		public List<EncompassReportField> ReportFields { get; set; }
		public Dictionary<string, List<EncompassReportField>> KeyValue { get; set; }
		public string LoanNumber { get; set; }
		public EncompassReport()
		{
			ReportFields = new List<EncompassReportField>();
			KeyValue = new Dictionary<string, List<EncompassReportField>>();
		}
		public DataTable ReportAsDataTable { get; set; }

	}

	public class EncompassReportField
	{
		public string FieldName { get; set; }
		public string FieldValue { get; set; }
		public string DisplayName { get; set; }

	}

}
