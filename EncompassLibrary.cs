using System;
using System.Collections.Generic;
using System.Linq;
using EllieMae.Encompass.BusinessObjects.Contacts;
using EllieMae.Encompass.BusinessObjects.Loans;
using EllieMae.Encompass.Client;
using EllieMae.Encompass.Collections;
using EllieMae.Encompass.Query;
using EllieMae.Encompass.Reporting;
using System.Reflection;
using Log4NetLibrary;

namespace EncompassLibrary
{
    public static class EncompassLibrary
    {
        private static readonly ILoggingService _logService = new FileLoggingService(typeof(EncompassLibrary));

        private static LogModel _logModel = new LogModel();
        public static string GetLoanGuidFromLoanNumber(string loanNumber, EllieMae.Encompass.Client.Session session)
        {
            
            if (loanNumber == null || loanNumber.Trim().Length <= 0)
            {
                return null;
            }

            if (session == null) // || !(this.session.IsConnected))
            {
                return null;
            }

            QueryCriterion queryCriterion = GetQuerCriteria(loanNumber, "Loan.LoanNumber");

            StringList fields = new StringList(new string[] { "Loan.LoanNumber", "Loan.LoanFolder", "Loan.LoanFolder", "Loan.Guid", "Fields.65" });

            EllieMae.Encompass.Reporting.LoanReportCursor loanReportCursor =
                session.Reports.OpenReportCursor(fields, queryCriterion);          

            string loanGuid = "";

            if (loanReportCursor == null)
            {

                return loanGuid;
            }

            if (loanReportCursor.Count <= 0)
            {

                return loanGuid;
            }

            EllieMae.Encompass.Reporting.LoanReportData loanReportData = loanReportCursor.GetItem(0);          
            loanGuid = loanReportData.Guid;
            loanReportCursor.Close();
            return loanGuid; 

        }
        public static List<string> GetCorrespondentLoanGuidFromLoanNumber(string loanNumber, EllieMae.Encompass.Client.Session session, StringFieldMatchType stringFieldMatchType = StringFieldMatchType.Contains)
        {
            if (loanNumber == null || loanNumber.Trim().Length <= 0)
            {
                return null;
            }
            if (session == null)
            {
                return null;
            }
            QueryCriterion queryCriterion = GetQuerCriteria(loanNumber, "Loan.LoanNumber", stringFieldMatchType);
            StringList fields = new StringList(new string[] { "Loan.LoanNumber", "Loan.LoanFolder", "Loan.LoanFolder", "Loan.Guid", "Fields.65" });
            EllieMae.Encompass.Reporting.LoanReportCursor loanReportCursor = session.Reports.OpenReportCursor(fields, queryCriterion);
            List<string> loanGuidList = new List<string>();
            if (loanReportCursor == null)
            {
                return loanGuidList;
            }
            if (loanReportCursor.Count <= 0)
            {
                return loanGuidList;
            }
            foreach (LoanReportData data in loanReportCursor)
            {
                loanGuidList.Add(data.Guid);
            }
            loanReportCursor.Close();
            return loanGuidList;
        }

        public static List<LoanInfo> GetLoanInfoFromLoanNumber(string loanNumber, EllieMae.Encompass.Client.Session session, StringFieldMatchType stringFieldMatchType = StringFieldMatchType.Contains)
        {
            if (loanNumber == null || loanNumber.Trim().Length <= 0)
            {
                return null;
            }
            if (session == null)
            {
                return null;
            }
            QueryCriterion queryCriterion = GetQuerCriteria(loanNumber, "Loan.LoanNumber", stringFieldMatchType);
            StringList fields = new StringList(new string[] { "Loan.LoanNumber", "Loan.LoanFolder", "Loan.LoanFolder", "Loan.Guid", "Fields.65" });
            EllieMae.Encompass.Reporting.LoanReportCursor loanReportCursor = session.Reports.OpenReportCursor(fields, queryCriterion);
            List<LoanInfo> loanInfoList = new List<LoanInfo>();
            if (loanReportCursor == null)
            {
                return loanInfoList;
            }
            if (loanReportCursor.Count <= 0)
            {
                return loanInfoList;
            }
            foreach (LoanReportData data in loanReportCursor)
            {
                loanInfoList.Add(new LoanInfo(data.Guid, data["Loan.LoanNumber"].ToString()));
            }
            loanReportCursor.Close();
            return loanInfoList;
        }

        public static Loan OpenLoan(string guid, EllieMae.Encompass.Client.Session session, int applicationId = 0)
        {


            if (guid == null || "".Equals(guid.Trim())) { return null; }

            if (session == null || !(session.IsConnected))
            {

                return null;
            }

            if (guid == null || guid.Trim().Length <= 0)
            {

                return null;
            }

            EllieMae.Encompass.BusinessObjects.Loans.Loan loan = null;

            try
            {

                loan = session.Loans.Open(guid);                 
                if (loan == null) { throw new System.Exception("session.Loans.Open(" + guid + ") returned null"); }

            }
            catch (System.Exception e)
            {
                LogHelper.Error(_logService, _logModel, applicationId, e.Message, "0", e);
            }

            return loan;
        }

        private static QueryCriterion GetQuerCriteria(string value, string fieldName, StringFieldMatchType stringFieldMatchType = StringFieldMatchType.Exact)
        {

            //// Similar to the method that opens by loanFolder & loanName - Very lightweight, but we don't have to use Canonical Names
            //// with the Query method... It still uses the Reporting Database as long as the fields in the criteria are in the Reporting Database...


            EllieMae.Encompass.Query.QueryCriterion queryCriterion =
                                                    new StringFieldCriterion(fieldName //field_id "364" is the  LoanNumber
                                                      , value
                                                      , stringFieldMatchType
                                                      , true // include (true)/exclude (false)                                                                                                                  
                                                     );

            return queryCriterion;
        }

        public static IEnumerable<string> GetCompanyNameByCategory(Session session, string category)
        {

            var bizContacts = session.Contacts.GetAll(ContactType.Biz);
            return (from BizContact bizContact in bizContacts where bizContact.Category != null where bizContact.Category.Name.Equals(category, StringComparison.OrdinalIgnoreCase) select bizContact.CompanyName).ToList();

        }

        public static ReportingFieldDescriptorList GetReportingDatabaseFields(Session session)
        {
            var instantiatedType = Activator.CreateInstance(typeof(Reports),
                                                           BindingFlags.NonPublic | BindingFlags.Instance,
                                                           null, new object[] { session }, null);
            Type classinfo = instantiatedType.GetType();
            MethodInfo mi = classinfo.GetMethod("GetReportingDatabaseFields", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { }, null);
            ReportingFieldDescriptorList reportingFieldDescriptorList = (ReportingFieldDescriptorList)mi.Invoke(instantiatedType, null);

            return reportingFieldDescriptorList;
        }

        public static QueryCriterion GetQueryCriteria(string fieldName, string value)
        {
            return new StringFieldCriterion(fieldName, value, StringFieldMatchType.Exact, true);
        }

        public static  bool UpdateFormCheckBoxField(string loanNumber, Session session, string value, string fieldId,int applicationId =0)
        {
            bool isUpdated = true;
            StringFieldCriterion loanCriterion = new StringFieldCriterion();
            loanCriterion.FieldName = "Loan.LoanNumber";
            loanCriterion.Value = loanNumber;
            BatchUpdate batch = new BatchUpdate(loanCriterion);
            batch.Fields.Add(fieldId, value);
            try
            {
                session.Loans.SubmitBatchUpdate(batch);
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logService,_logModel, applicationId, ex.Message,loanNumber,ex);
                isUpdated = false;

            }
            return isUpdated;
        }
        public static bool UpdateFieldValuesByLoanNumber(string loanNumber, Session session, Dictionary<string,string> fielddata, int applicationId = 0)
        {
            bool isUpdated = true;
            StringFieldCriterion loanCriterion = new StringFieldCriterion
            {
                FieldName = "Loan.LoanNumber",
                Value = loanNumber
            };
            BatchUpdate batch = new BatchUpdate(loanCriterion);
            try
            {
                foreach (var data in fielddata)
                {
                    batch.Fields.Add(data.Key, data.Value);
                }
                session.Loans.SubmitBatchUpdate(batch);
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logService, _logModel, applicationId, ex.Message, loanNumber, ex);
                 isUpdated = false;
            }
            return isUpdated;
        }


    }
}
