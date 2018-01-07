using EllieMae.Encompass.Client;
using EllieMae.Encompass.Collections;
using EllieMae.Encompass.Reporting;
using Log4NetLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace EncompassLibrary.Utilities
{

	/// <summary>
	/// Generic Class For Report Generation
	/// Usage : var reporter = new Reporter();
	/// var result = reporter.GetReport<TYPE>(loanNumbers,Encompass session);
	/// var asDataTable = result.ReportAsDataTable;
	/// var file = CreateExcelFromDataTable(asDataTable, result.ReportName);
	/// CreateExcelFromDataTable uses closedxml to generate Excel File
	/// For TYPE see Fields class in CanonicalFields.cs
	/// </summary>
	public class Reporter
	{
		private readonly ILoggingService _logService;
        private  LogModel _logModel;

        public string ReportName { get; set; }

		public Reporter()
		{
			_logService = new FileLoggingService(typeof(Reporter));
		}
		public Reporter(string reportName)
		{
			ReportName = reportName;
			_logService = new FileLoggingService(typeof(Reporter));
		}
		/// <summary>
		/// Create Report
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reportName"></param>
		/// <param name="loanNumbers"></param>
		/// <param name="session"></param>
		/// <returns></returns>
		public EncompassReport GetReport<T> (IEnumerable<string> loanNumbers, Session session, string reportName=null,int applicationId=0) where T:class
		{
			var report = new EncompassReport();
			try
			{
				var stringGuidList = Common.SetStringGuidList(loanNumbers, session);
				var stringFieldList = GetFieldList(typeof(T), session);
				var dataList = session.Reports.SelectReportingFieldsForLoans(stringGuidList, stringFieldList);
				var tableName = string.IsNullOrEmpty(reportName) ? typeof(T).Name : reportName;
				report.ReportName = tableName;
				List<EncompassReportField> ReportFields;
				report.ReportAsDataTable= GetReportAsDataTable(typeof(T), dataList, tableName, applicationId,out ReportFields );
			}
			catch (Exception ex)
			{
				report.ErrorMessage = string.IsNullOrEmpty(ex.Message) ? ex.StackTrace : ex.Message;
                LogHelper.Error(_logService,_logModel, applicationId,ex.Message,ex:ex);
				return report;
			}
			return report;
		}
		private StringList GetFieldList(Type reportType, Session session)
		{
			var fields = reportType.GetFields();
			var list = new StringList();
			foreach (FieldInfo field in fields)
			{
				if (!string.IsNullOrEmpty(field.GetValue(reportType).ToString()))
					list.Add(field.GetValue(reportType).ToString());
			}
			return list;
		}
		private DataTable GetReportAsDataTable(Type reportType,LoanReportDataList dataList,string reportName, int applicationId,out List<EncompassReportField> ReportFields)
		{
			ReportFields = new List<EncompassReportField>();
			   var table = new DataTable(reportName);
			Func<object, string> checkNullFunc = CheckForNull;
			try
			{
				FieldInfo[] fields = reportType.GetFields();
				SetColumns(fields, ref table);
				foreach (LoanReportData data in dataList)
				{
					var row = table.NewRow();
					var result = data.GetFieldNames();
					for (var i = 0; i < result.Count - 1; i++)
					{
						var field = new EncompassReportField
						{
							FieldName = Convert.ToString(result[i]),
							FieldValue = FormatAndCast(data[Convert.ToString(result[i])]),
							DisplayName = checkNullFunc(fields[i].Name)
						};
						ReportFields.Add(field);
						row[reportType.GetFields()[i].Name] = FormatAndCast(data[Convert.ToString(result[i])]);
					}
					table.Rows.Add(row);
				}
			}
			catch (Exception ex)
			{
                LogHelper.Error(_logService, _logModel, applicationId, ex.Message, ex: ex);
            }
			return table;
		}
		private string FormatAndCast(object valueToCheck)
		{
			if (valueToCheck == null)
				return string.Empty;
			if (valueToCheck is string)
				return valueToCheck.ToString();
			if (valueToCheck is decimal)
				return Convert.ToDecimal(valueToCheck).ToString("0.0");
			if (valueToCheck is DateTime)
				return Convert.ToDateTime(valueToCheck.ToString()).ToShortDateString();
			return valueToCheck.ToString();
		}
		private string CheckForNull(object valueToCheck)
		{
			return valueToCheck != null && !string.IsNullOrEmpty(valueToCheck.ToString()) ? valueToCheck.ToString() : string.Empty;
		}
		private void SetColumns(FieldInfo[] columns, ref DataTable table)
		{
			for (var i = 0; i < columns.Count(); i++)
			{
				table.Columns.Add(columns[i].Name);
			}
		}
	}
}
