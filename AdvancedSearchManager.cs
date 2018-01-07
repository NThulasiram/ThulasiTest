using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EllieMae.Encompass.Client;
using EllieMae.Encompass.Collections;
using EllieMae.Encompass.Query;
using EllieMae.Encompass.Reporting;
using FGMC.Common.DataContract;
using Log4NetLibrary;

namespace EncompassLibrary.AdvancedSearch
{
    public class AdvancedSearchManager
    {
        private Dictionary<string, QueryCriterion> _queries = null;
        private List<string> _operators = null;
        private List<QueryBuilderTemplate> _templates = null;
        private readonly ILogService _logger = null;

        public AdvancedSearchManager()
        {
            _logger = new FileLogService(typeof(AdvancedSearchManager));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputTemplates"></param>
        /// <param name="sExpression"></param>
        /// <returns></returns>
        public QueryCriterion GetFinalQueryFromExpression(List<QueryBuilderTemplate> inputTemplates,
            string sExpression)
        {
            QueryCriterion finalQuery = null;
            _templates = inputTemplates;

            try
            {
                var value = EvaluateExpression(sExpression);
                finalQuery = GetFinalQuery(value);
            }

            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }

            return finalQuery;
        }

        /// <summary>
        /// Gets loan number fromguid using report cursor
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public string GetLoanNumberFromGuid(string guid, Session session)
        {
            string loanNo = null;
            var criteria = new StringFieldCriterion(AdvancedSearchConstants.GUID_CANONICALNAME, guid, StringFieldMatchType.Exact, true);
            var fieldsToRetrieve = new StringList();
            fieldsToRetrieve.Add(AdvancedSearchConstants.LOANNO_CANONICALNAME);

            try
            {
                var loanReportCursor = session.Reports.OpenReportCursor(fieldsToRetrieve, criteria);
                if (loanReportCursor == null) return string.Empty;
                loanNo = Convert.ToString(loanReportCursor.Cast<LoanReportData>().FirstOrDefault()[AdvancedSearchConstants.LOANNO_CANONICALNAME]);
            }
            catch (Exception ex)
            {

                _logger.Error(ex.Message);
            }

            return loanNo;
        }

        /// <summary>
        /// Gets the loans from a resultant query built using an expression
        /// </summary>
        /// <param name="session">Encompass session</param>
        /// <param name="finalQuery">The resultant query built based on an expression </param>
        /// <returns></returns>
        public List<string> GetLoansByQueryCriteria(QueryCriterion finalQuery, EncompassUser user)
        {
            List<string> loans = null;
            List<string> loanGuidList = null;
            try
            {
                //Get guid list
                AccountLibrary.AccountLibrary accountLibrary = new AccountLibrary.AccountLibrary();
                var session = accountLibrary.GetUserSession(user);
                var loanIdentityList = session.Loans.Query(finalQuery);
                if (loanIdentityList != null)
                {
                    loanGuidList = new List<string>();
                    loans = new List<string>();

                    for (int i = 0; i < loanIdentityList.Count; ++i)
                    {
                        var loanIdentity = loanIdentityList[i];
                        if (loanIdentity == null)
                        {
                            continue;
                        }

                        loanGuidList.Add(loanIdentity.Guid);
                    }
                }

                //Get loan numbers from guid's
                string loanNumber = null;
                foreach (var guid in loanGuidList)
                {
                    loanNumber = GetLoanNumberFromGuid(guid, session);
                    if (loanNumber != null)
                        loans.Add(loanNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
            return loans;
        }

        private QueryCriterion GetFinalQuery(string value)
        {
            QueryCriterion finalQuery = null;
            if (value.Contains(AdvancedSearchConstants.AND))
            {
                var result = value.Split(new string[] { AdvancedSearchConstants.AND }, StringSplitOptions.None);
                finalQuery = GetCurrentCriterionFromExpression(result);
            }
            else
            {
                // Split with OR and get the resultant OR condition
                if (value.Contains(AdvancedSearchConstants.OR))
                {
                    finalQuery = GetFinalOrCriterion(value);
                }

                else
                {
                    // //Check whether it is a already evaluated expression stored in dictionary.   
                    if (_queries.ContainsKey(value))
                    {
                        finalQuery = _queries[value];
                    }
                    else
                    {
                        // Check whether it contains any _operators. If so get the corrosponding template from template collection and build the query.
                        finalQuery = GetQueryCriterionByOperators(value);
                    }
                }
            }
            return finalQuery;
        }

        private QueryCriterion GetQueryCriterionByOperators(string value)
        {
            QueryCriterion finalQuery = null;
            string[] output;
            foreach (var op in _operators)
            {
                if (value.Contains(op))
                {
                    output = value.Split(new string[] { op }, StringSplitOptions.None);
                    var template = GetTemplate(output, GetOperationFromSymbol(op));
                    if (template != null)
                    {
                        finalQuery = GetQueryCriterionByType(template);
                        break;
                    }
                }
            }

            return finalQuery;
        }

        /// <summary>
        /// Matches the innermost parentheses and replaces the evaluated value with the original expression till a flat expression is received.
        /// </summary>
        /// <param name="sExpression"></param>
        /// <returns></returns>
        private string EvaluateExpression(string sExpression)
        {

            EllieMae.Encompass.Query.QueryCriterion currentQuery = null;
            bool isExpressionMatched;
            string[] result = null;
            int i = 1;
            _operators = GetAllOperators();
            _queries = new Dictionary<string, QueryCriterion>();

            do
            {
                isExpressionMatched = false;

                //Matches the innermost parentheses expression and replaces with the string output.
                sExpression = Regex.Replace(
                    sExpression,
                    @"\(([^()]*)\)",
                    new MatchEvaluator(delegate (Match m)
                    {
                        isExpressionMatched = true;

                        // // This expression is a subset of original expression and will not contain intermediate parentheses. Split with AND operation and handle if each split item contains OR operation.
                        result = m.Groups[0].Value.Split(new string[] { AdvancedSearchConstants.AND }, StringSplitOptions.None);

                        return GetSubstringToReplace(result, ref currentQuery, ref i);
                    })
                    );

            } while (isExpressionMatched);

            return sExpression;
        }

        private string GetSubstringToReplace(string[] result, ref QueryCriterion currentQuery, ref int i)
        {
            if (result != null)
            {
                // Get the resultant query for the current match expression                        
                currentQuery = GetCurrentCriterionFromExpression(result);
            }

            if (currentQuery != null)
            {
                var resultantQuery = currentQuery;

                //Define a dynamic key for the dictionary
                var currentKey = AdvancedSearchConstants.KEY + i;

                //Store intermediate result with a dynamically generated key in a dictionary for the later part of the evaluation
                _queries.Add(currentKey, resultantQuery);
                i++;

                // Reset current query to evaluate next match expression
                currentQuery = null;
                return currentKey;
            }

            _logger.Warn(AdvancedSearchConstants.CURRENT_QUER_NULL_ERROR);
            return string.Empty;
        }

        /// <summary>
        /// Gets the resultant query criterion out of multiple conditions
        /// </summary>
        /// <param name="result">Array of conditions passed</param>
        /// <returns></returns>
        private QueryCriterion GetCurrentCriterionFromExpression(string[] result)
        {
            QueryCriterion query = null;
            List<QueryCriterion> andList = new List<QueryCriterion>();

            if (result.Length >= 2)
            {
                foreach (var item in result)
                {
                    if (item.Contains(AdvancedSearchConstants.OR))
                    {
                        //Handle if item contains one or more OR operation and get resultant OR operation among them.
                        var orQriterion = GetFinalOrCriterion(item);

                        //Add it to ANDList for the later part of evaluation
                        andList.Add(orQriterion);
                    }
                    else
                    {
                        // Check _operators are there in item
                        if (!_operators.Any(item.Contains))
                        {
                            //Get the matching template and add it AND list
                            var key = item.Replace(AdvancedSearchConstants.LEFT_PARENTHESES, string.Empty)
                      .Replace(AdvancedSearchConstants.RIGHT_PARANTHESES, string.Empty)
                      .Trim();

                            //Check dictionary has the key or not
                            if (_queries.ContainsKey(key))
                            {
                                andList.Add(
                                    _queries[key]);
                            }
                            else
                            {
                                _logger.Warn(AdvancedSearchConstants.INVALID_EXPRESSION);
                            }

                            continue;
                        }

                        //Get the matching template and build query 
                        andList.Add(GetQueryCriterionByOperators(item));
                    }
                }

                //Get resultant AND condition out of the items in the ANDList
                QueryCriterion andQriterion = GetResultantAndCriterion(andList);
                query = andQriterion;
            }

            else
            {
                //Check for existence of OR. IF exsist get the resultant OR condition out of it
                var value = result[0];
                query = GetQueryWithoutAnd(value);
            }
            return query;
        }

        private QueryCriterion GetQueryWithoutAnd(string value)
        {
            QueryCriterion query = null;
            if (value.Contains(AdvancedSearchConstants.OR))
            {
                var orQriterion = GetFinalOrCriterion(value);
                query = orQriterion;
            }
            else
            {
                //Check dictionary has a value
                if (!_operators.Any(value.Contains))
                {
                    var key = value.Replace(AdvancedSearchConstants.LEFT_PARENTHESES, string.Empty)
                        .Replace(AdvancedSearchConstants.RIGHT_PARANTHESES, string.Empty)
                        .Trim();

                    //Check dictionary has the key or not
                    if (_queries.ContainsKey(key))
                        query = _queries[key];
                    else
                    {
                        _logger.Warn(AdvancedSearchConstants.INVALID_EXPRESSION);
                    }
                }
                else
                {
                    //Get the matching template and build query
                    query = GetQueryCriterionByOperators(value);
                }
            }
            return query;
        }

        /// <summary>
        /// All Encompass supported _operators
        /// </summary>
        /// <returns></returns>
        private List<string> GetAllOperators()
        {
            return new List<string>()
            {
                AdvancedSearchConstants.EQUALS_OPERATOR,
                AdvancedSearchConstants.GREATER_THAN_OPERATOR,
                AdvancedSearchConstants.LESS_THAN_OPERATOR,
                AdvancedSearchConstants.BETWEEN,
                AdvancedSearchConstants.NOT_BETWEEN,
                AdvancedSearchConstants.CONTAINS,
                AdvancedSearchConstants.IS_NOT_OPERATOR,
                AdvancedSearchConstants.NOT_GREATER_THAN_OPERATOR,
                AdvancedSearchConstants.NOT_LESS_THAN_OPERATOR,
                AdvancedSearchConstants.AFTER_OPERATION,
                AdvancedSearchConstants.BEFORE_OPERATION,
                AdvancedSearchConstants.ON_OR_BEFORE,
                AdvancedSearchConstants.AFTER_OPERATION,
                AdvancedSearchConstants.STARTS_WITH, 
                AdvancedSearchConstants.IS_NOT,
                AdvancedSearchConstants.DOESNT_STARTS_WITH,
                AdvancedSearchConstants.DOESNT_CONTAIN,
                AdvancedSearchConstants.TODAY
            };
        }

        /// <summary>
        /// Builds the query list among the split items by OR
        /// </summary>
        /// <param name="item">Condition containing one or more OR operation </param>
        /// <returns></returns>
        private QueryCriterion GetFinalOrCriterion(string item)
        {
            var orResult = item.Split(new string[] { AdvancedSearchConstants.OR }, StringSplitOptions.None);
            List<QueryCriterion> orList = new List<QueryCriterion>();

            foreach (var entry in orResult)
            {
                //If the entry does not have any operator
                if (!_operators.Any(entry.Contains))
                {
                    // If the entry is a already evaluated expression stored in a dictionary, get the value from dictionary.

                    var key = entry.Replace(AdvancedSearchConstants.LEFT_PARENTHESES, string.Empty)
                        .Replace(AdvancedSearchConstants.RIGHT_PARANTHESES, string.Empty)
                        .Trim();

                    //Check dictionary has the key or not
                    if (_queries.ContainsKey(key))
                        orList.Add(_queries[key]);
                    else
                    {
                        _logger.Warn(AdvancedSearchConstants.INVALID_EXPRESSION);
                    }

                    continue;
                }

                //Get the matching template and build query
                foreach (var op in _operators)
                {
                    if (entry.Contains(op))
                    {
                        var output = entry.Split(new string[] { op }, StringSplitOptions.None);
                        var template = GetTemplate(output, GetOperationFromSymbol(op));
                        orList.Add(GetQueryCriterionByType(template));
                        break;
                    }
                }
            }

            var orQriterion = GetResultantORCriterion(orList);
            return orQriterion;
        }

        /// <summary>
        /// Get's the appropriate template from _templates collection
        /// </summary>
        /// <param name="output"> string array containing description and input value required for filtering the template from _templates collection</param>
        /// <param name="operation">operation required for filtering the template from _templates collection</param>
        /// <returns></returns>
        private QueryBuilderTemplate GetTemplate(string[] output, string operation)
        {
            string[] operations = null;
            operations = operation.Split(',');

            var template =
                _templates.Find(
                    p =>
                        p.Desciption.Trim() ==
                        output[0].Replace(AdvancedSearchConstants.LEFT_PARENTHESES, string.Empty)
                            .Replace(AdvancedSearchConstants.RIGHT_PARANTHESES, string.Empty)
                            .Trim() &&
                        p.InputValue.Trim() ==
                        output[1].Replace(AdvancedSearchConstants.LEFT_PARENTHESES, string.Empty)
                            .Replace(AdvancedSearchConstants.RIGHT_PARANTHESES, string.Empty)
                            .Trim() &&
                        operations.Any(p.OperatorName.Trim().Contains)
              );

            return template;
        }

        /// <summary>
        /// Gets the resultant query by repeated OR operation among the list of conditions
        /// </summary>
        /// <param name="ORList"></param>
        /// <returns></returns>
        private QueryCriterion GetResultantORCriterion(List<QueryCriterion> ORList)
        {
            QueryCriterion orQriterion = null;
            foreach (var orListItem in ORList)
            {
                if (orQriterion == null)
                    orQriterion = orListItem;
                else
                {
                    orQriterion = orQriterion.Or(orListItem);
                }
            }
            return orQriterion;
        }

        /// <summary>
        /// Gets the resultant query by repeated AND operation among the list of conditions
        /// </summary>
        /// <param name="ANDList"></param>
        /// <returns></returns>
        private QueryCriterion GetResultantAndCriterion(List<QueryCriterion> ANDList)
        {
            QueryCriterion andQriterion = null;
            foreach (var andListItem in ANDList)
            {
                andQriterion = andQriterion == null ? andListItem : andQriterion.And(andListItem);
            }
            return andQriterion;
        }

        /// <summary>
        /// Returns the opearation name for a given symbol required for template comparison.
        /// </summary>
        /// <param name="Operator">Input symbol</param>
        /// <returns></returns>
        public string GetOperationFromSymbol(string Operator)
        {
            switch (Operator)
            {
                case AdvancedSearchConstants.IS_NOT_OPERATOR:
                    return AdvancedSearchConstants.IS_NOT;
                case AdvancedSearchConstants.GREATER_THAN_OPERATOR:
                    return AdvancedSearchConstants.GREATER_THAN;
                case AdvancedSearchConstants.NOT_GREATER_THAN_OPERATOR:
                    return AdvancedSearchConstants.NOT_GREATER_THAN;
                case AdvancedSearchConstants.LESS_THAN_OPERATOR:
                    return AdvancedSearchConstants.LESS_THAN;
                case AdvancedSearchConstants.NOT_LESS_THAN_OPERATOR:
                    return AdvancedSearchConstants.NOT_LESS_THAN;
                case AdvancedSearchConstants.EQUALS_OPERATOR:
                    return AdvancedSearchConstants.IS_OR_ISEXACT;
                case AdvancedSearchConstants.BETWEEN:
                    return AdvancedSearchConstants.BETWEEN;
                case AdvancedSearchConstants.NOT_BETWEEN:
                    return AdvancedSearchConstants.NOT_BETWEEN;
                case AdvancedSearchConstants.CONTAINS:
                    return AdvancedSearchConstants.CONTAINS;
                default:
                    return Operator;
            }
        }

        private StringFieldMatchType GetStringFieldMatchType(QueryBuilderTemplate template)
        {
            StringFieldMatchType stringFieldMatchType = 0;
            var result = template.OperatorName;

            switch (result)
            {
                case AdvancedSearchConstants.CONTAINS:
                    stringFieldMatchType = StringFieldMatchType.Contains;
                    break;
                case AdvancedSearchConstants.IS_EXACT:
                    stringFieldMatchType = StringFieldMatchType.Exact;
                    break;
                case AdvancedSearchConstants.STARTS_WITH:
                    stringFieldMatchType = StringFieldMatchType.StartsWith;
                    break;
            }
            return stringFieldMatchType;
        }

        public EllieMae.Encompass.Query.QueryCriterion GetQueryCriteriaForString(string fieldName, string value, StringFieldMatchType stringFieldMatchType, bool include)
        {
            return new StringFieldCriterion(fieldName, value, stringFieldMatchType, include);
        }

        private OrdinalFieldType GetOrdinalFieldType(QueryBuilderTemplate template)
        {
            OrdinalFieldType ordinalFieldType = new OrdinalFieldType();
            var result = template.OperatorName;

            switch (result)
            {
                case AdvancedSearchConstants.IS_OPERATION:
                    ordinalFieldType.OrdinalFieldMatchType = OrdinalFieldMatchType.Equals;
                    break;
                case AdvancedSearchConstants.AFTER_OPERATION:
                case AdvancedSearchConstants.GREATER_THAN:
                    ordinalFieldType.OrdinalFieldMatchType = OrdinalFieldMatchType.GreaterThan;
                    break;
                case AdvancedSearchConstants.BEFORE_OPERATION:
                case AdvancedSearchConstants.LESS_THAN:
                    ordinalFieldType.OrdinalFieldMatchType = OrdinalFieldMatchType.LessThan;
                    break;
                case AdvancedSearchConstants.IS_NOT:
                case AdvancedSearchConstants.NOT_EQUALS_OPERATION:
                    ordinalFieldType.OrdinalFieldMatchType = OrdinalFieldMatchType.NotEquals;
                    break;
                case AdvancedSearchConstants.ON_OR_AFTER_OPERATION:
                case AdvancedSearchConstants.NOT_LESS_THAN:
                    ordinalFieldType.OrdinalFieldMatchType = OrdinalFieldMatchType.GreaterThanOrEquals;
                    break;
                case AdvancedSearchConstants.ON_OR_BEFORE:
                case AdvancedSearchConstants.NOT_GREATER_THAN:
                    ordinalFieldType.OrdinalFieldMatchType = OrdinalFieldMatchType.LessThanOrEquals;
                    break;
                case AdvancedSearchConstants.BETWEEN:
                    ordinalFieldType.OrdinalFieldMatchOtherType = OrdinalFieldMatchOtherType.BETWEEN;
                    break;
                case AdvancedSearchConstants.NOT_BETWEEN:
                    ordinalFieldType.OrdinalFieldMatchOtherType = OrdinalFieldMatchOtherType.NOTBETWEEN;
                    break;
            }
            return ordinalFieldType;
        }

        /// <summary>
        /// Gets the query criterion based on the input field type
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        private QueryCriterion GetQueryCriterionByType(QueryBuilderTemplate template)
        {
            QueryCriterion currentQuery = null;
            switch (template.FieldType)
            {
                case AdvancedSearchConstants.STRING_FIELD_TYPE:
                    var stringFieldMatchType = GetStringFieldMatchType(template);
                    currentQuery = GetQueryCriteriaForString(template.CanonicalName, template.InputValue,
                        stringFieldMatchType, true);
                    break;
                case AdvancedSearchConstants.NUMERIC_FIELD_TYPE:
                    var numericType = GetOrdinalFieldType(template);
                    currentQuery = GetQueryCriteriaForNumeric(template.CanonicalName, template.InputValue, numericType);
                    break;
                case AdvancedSearchConstants.DATE_FIELD_TYPE:
                    var dateType = GetOrdinalFieldType(template);
                    currentQuery = GetQueryCriteriaForDate(template.CanonicalName, template.InputValue, dateType);
                    break;
            }
            return currentQuery;
        }

        public QueryCriterion GetQueryCriteriaForNumeric(string fieldName, string value, OrdinalFieldType numericType)
        {
            QueryCriterion queryCriterion = null;

            if (Enum.IsDefined(typeof(OrdinalFieldMatchOtherType), numericType.OrdinalFieldMatchOtherType))
            {
                string[] values = value.Split(' ');
                if (numericType.OrdinalFieldMatchOtherType == OrdinalFieldMatchOtherType.BETWEEN)
                {
                    //Build the logic here 
                    //Between 5 and 10 
                    // Greater than or euqal to 5 AND Less than or equal to 10

                    queryCriterion = new NumericFieldCriterion(fieldName, Convert.ToDouble(values[0]), OrdinalFieldMatchType.GreaterThanOrEquals);
                    queryCriterion = queryCriterion.And(new NumericFieldCriterion(fieldName, Convert.ToDouble(values[2]), OrdinalFieldMatchType.LessThanOrEquals));
                }

                else if (numericType.OrdinalFieldMatchOtherType == OrdinalFieldMatchOtherType.NOTBETWEEN)
                {
                    //Build the logic here 
                    //Not Between 5 and 10 
                    // Less than or euqal to 5 AND Greater than or equal to 10

                    queryCriterion = new NumericFieldCriterion(fieldName, Convert.ToDouble(values[0]), OrdinalFieldMatchType.LessThanOrEquals);
                    queryCriterion = queryCriterion.And(new NumericFieldCriterion(fieldName, Convert.ToDouble(values[2]), OrdinalFieldMatchType.GreaterThanOrEquals));
                }
            }

            else if (Enum.IsDefined(typeof(OrdinalFieldMatchType), numericType.OrdinalFieldMatchType))
            {
                var result = Convert.ToDouble(value);
                queryCriterion = new NumericFieldCriterion(fieldName, result, numericType.OrdinalFieldMatchType);
            }

            return queryCriterion;
        }

        private QueryCriterion GetQueryCriteriaForDate(string fieldName, string value, OrdinalFieldType dateType)
        {
            QueryCriterion queryCriterion = null;

            if (Enum.IsDefined(typeof(OrdinalFieldMatchOtherType), dateType.OrdinalFieldMatchOtherType))
            {
                DateTime lowEndValue;
                DateTime highEndValue;
                string[] values = value.Split(' ');
                if (dateType.OrdinalFieldMatchOtherType == OrdinalFieldMatchOtherType.BETWEEN)
                {
                    //Build the logic here 
                    // Between 04/01/2015 and 04/11/2015 

                    // On or after 04/01/2015 AND On or before 04/11/2015

                    // First Date field
                    if (DateTime.TryParse(values[0], out lowEndValue))
                        queryCriterion = new DateFieldCriterion(fieldName, Convert.ToDateTime(lowEndValue), OrdinalFieldMatchType.GreaterThanOrEquals, DateFieldMatchPrecision.Day);

                    //AND opearation with second date field
                    if (DateTime.TryParse(values[2], out highEndValue))
                        queryCriterion = queryCriterion.And(new DateFieldCriterion(fieldName, Convert.ToDateTime(highEndValue), OrdinalFieldMatchType.LessThanOrEquals, DateFieldMatchPrecision.Day));
                    else
                        // If any one of the date value is incorrect we should not proceed. Since for range we need both the upper and lower limit, 
                        //the absense of one of the value will mis interprete the requirement. So the value should be reset to null.                  
                        queryCriterion = null;                                        
                }

                else if (dateType.OrdinalFieldMatchOtherType == OrdinalFieldMatchOtherType.NOTBETWEEN)
                {
                    //Build the logic here 
                    // Not between 04/01/2015 and 04/11/2015 

                    // On or before  04/01/2015 AND On or after 04/11/2015

                    // First Date field
                    if (DateTime.TryParse(values[0], out lowEndValue))
                        queryCriterion = new DateFieldCriterion(fieldName, Convert.ToDateTime(lowEndValue), OrdinalFieldMatchType.LessThanOrEquals, DateFieldMatchPrecision.Day);

                    //AND opearation with second date field
                    if (DateTime.TryParse(values[2], out highEndValue))
                        queryCriterion = queryCriterion.And(new DateFieldCriterion(fieldName, Convert.ToDateTime(highEndValue), OrdinalFieldMatchType.GreaterThanOrEquals, DateFieldMatchPrecision.Day));
                    else
                        // If any one of the date value is incorrect we should not proceed. Since for range we need both the upper and lower limit, 
                        //the absense of one of the value will mis interprete the requirement. So the value should be reset to null.               
                        queryCriterion = null;                
                }
            }

            else if (Enum.IsDefined(typeof(OrdinalFieldMatchType), dateType.OrdinalFieldMatchType))
            {
                DateTime output;
                if (DateTime.TryParse(value, out output))
                    queryCriterion = new DateFieldCriterion(fieldName, output, dateType.OrdinalFieldMatchType, DateFieldMatchPrecision.Day);
            }

            return queryCriterion;
        }    
    }
}
