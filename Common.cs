using EllieMae.Encompass.Client;
using EllieMae.Encompass.Collections;
using EllieMae.Encompass.Query;
using EllieMae.Encompass.Reporting;
using EncompassLibrary.CustomException;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EncompassLibrary.Utilities
{
    /// <summary>
    /// Helper Methods
    /// </summary>
    public class Common
    {
        /// <summary>
        /// Gets LoanGuid from LoanNumber
        /// </summary>
        /// <param name="loanNumber"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static string GetGuid(string loanNumber, Session session)
        {
            return GetLoanGuid(loanNumber, session);
        }
        /// <summary>
        /// Gets LoanGuids from LoanNumbers
        /// </summary>
        /// <param name="loanNumbers"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetGuid(IEnumerable<string> loanNumbers, Session session)
        {
            if (loanNumbers == null) throw new ArgumentNullException("loanNumbers");
            var guids = new List<string>();
            loanNumbers.ToList().ForEach(o => guids.Add(GetGuid(o, session)));
            return guids;
        }
        public static double ConvertSize(double bytes, SizeType sizeType)
        {
            try
            {
                const int CONVERSION_VALUE = 1024;
                switch (sizeType)
                {
                    case SizeType.B:
                        return bytes;
                    case SizeType.KB:
                        return (bytes / CONVERSION_VALUE);
                    case SizeType.MB:
                        return (bytes / CalculateSize(CONVERSION_VALUE));
                    default:
                        return bytes;
                }
            }
            catch 
            {
                return 0;
            }
        }

       
        private static double CalculateSize(Int32 number)
        {
            return Math.Pow(number, 2);
        }
        /// <summary>
        /// Gets All EFolder Names
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetEFolderNames()
        {
            return Enum.GetNames(typeof(EncompassDocumentType));           
        }
        /// <summary>
        /// Use it when checking multiple string parameters
        /// </summary>
        /// <param name="valuesToCheck"></param>
        /// <returns></returns>
        public static bool AnyValueNull(IEnumerable<string> valuesToCheck)
        {
            return valuesToCheck.Any(o => string.IsNullOrEmpty(o.ToString()));
        }
        /// <summary>
        /// Converts LoanNumber to LoanGuid and adds to StringList
        /// </summary>
        /// <param name="loanNumbers"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static StringList SetStringGuidList(IEnumerable<string> loanNumbers, Session session)
        {
            if (loanNumbers == null || loanNumbers.Count() == 0) return null;

            if (session == null) throw new SessionConnectionException("Session Is Not Connected");
            var loanGuids = new List<string>();
            foreach (var loanNumber in loanNumbers)

                {
                var criteria = new StringFieldCriterion(CanonicalFields.LoanNumber, loanNumber.Trim(), StringFieldMatchType.Exact, true);
                var loanQuery = session.Loans.Query(criteria);
                if (loanQuery == null || loanQuery.Count == 0)
                    continue;
                loanGuids.Add(loanQuery[0].Guid);
            }
            return new StringList(loanGuids);
        }
        public static StringList SetStringList()
        {
            var type = typeof(Fields);
            var fields = type.GetFields();
            var list = new StringList();
            foreach (FieldInfo field in fields)
            {
                list.Add(field.GetValue(typeof(Fields)).ToString());

            }
            return list;
        }
        public static StringList SetStringFieldList( Session session)
        {
            if (session == null) throw new SessionConnectionException("Session Is Not Connected");
            var type =typeof(Fields);
            return GetStringList(type);
            }

        public static StringList SetStringFieldListForMonthlyReport(Session session)
            {
            if (session == null) throw new SessionConnectionException("Session Is Not Connected");
            var type = typeof(MonthlyReportFields);
            return GetStringList(type);
            }

        private static StringList GetStringList(Type type)
        {
            var fields = type.GetFields();
            var list = new StringList();
            foreach (FieldInfo field in fields)
            {
                list.Add(field.GetValue(typeof (Fields)).ToString());
            }
            return list;
        }

        public static LoanReportDataList GetReportFiedValuesByLoanNumbers(IEnumerable<string> loanNumbers, IEnumerable<string> fieldIDs,Session session)
        {
            var stringGuidList = SetStringGuidList(loanNumbers, session);
            LoanReportDataList dataList = null;
            StringList fieldlist = new StringList();
           
            if (stringGuidList.Count > 0)
            {
                foreach (string field in fieldIDs)
                {
                    fieldlist.Add(field);
                }
                dataList = session.Reports.SelectReportingFieldsForLoans(stringGuidList, fieldlist);
            }
           

            return dataList;
        }

        #region Private
        private static string GetLoanGuid(string loanNumber, Session session)
        {
            var criteria = new StringFieldCriterion(CanonicalFields.LoanNumber,loanNumber,StringFieldMatchType.Exact,true);
            var fieldsToRetrieve = new StringList();
            fieldsToRetrieve.Add(CanonicalFields.LoanGuid);
            var loanReportCursor = session.Reports.OpenReportCursor(fieldsToRetrieve, criteria);
            if (loanReportCursor == null  || loanReportCursor.Count==0) return string.Empty;
            return loanReportCursor.Cast<LoanReportData>().FirstOrDefault()[CanonicalFields.LoanGuid].ToString();
        }



        #endregion
    }
}
