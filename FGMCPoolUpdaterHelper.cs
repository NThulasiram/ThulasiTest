using FGMC.EncompassUtility.Security;
using FGMCPoolUpdater.Constants;
using FGMCPoolUpdater.Models;
using Log4NetLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using FGMCPoolUpdater.PoolUpdaterServiceReference;

namespace FGMCPoolUpdater.Helper
{
    public class PoolUpdaterHelper
    {
        ILogService _logger = new FileLogService(typeof(PoolUpdaterHelper));
        List<SelectListItem> selectsheetlist = new List<SelectListItem>();
       

        public List<SelectListItem> LoadSheetsFromExcel(HttpPostedFileBase browsefile, string clientId)
        {
            _logger.Info("LoadSheetsFromExcel  Started.");
            string filename = Path.GetFileName(browsefile.FileName);
            filename = string.Format("{0}_{1}", clientId, filename);
            string[] xlsfiles = Directory.GetFiles(Path.GetTempPath(), string.Concat(clientId, PoolUpdaterConstant.STRXLS));
            string[] xlsxfiles = Directory.GetFiles(Path.GetTempPath(), string.Concat(clientId, PoolUpdaterConstant.STRXLSX));
            foreach (string file in xlsfiles)
            {
                System.IO.File.Delete(file);
            }
            foreach (string file in xlsxfiles)
            {
                System.IO.File.Delete(file);
            }
            _logger.Info(string.Concat(clientId, PoolUpdaterConstant.STR_SELECT_FILE, filename));
            if (filename != null)
            {
                string filepath = Path.Combine(Path.GetTempPath(), Path.GetFileName(filename));
                browsefile.SaveAs(filepath);
                _logger.Info(string.Concat(clientId, PoolUpdaterConstant.STR_SAVINGFILE, filepath));
                selectsheetlist = GetSheetNamesFromExcel(filepath);
            }
            return selectsheetlist;
        }

        public LoanRecordList ReadLoanDetails(string excelFilename, string sheetName, string excelColumnInfoPath,string clientId)
        {
            List<LoanRecord> loanRecords = new List<LoanRecord>();

            try
            {
                if (!string.IsNullOrEmpty(excelFilename))
                {
                    _logger.Info("Read LoanDetails from excel Started.");
                    // extract only the fielname
                    string serverfilepath = GetServerFilePath(excelFilename, clientId);
                    if (System.IO.File.Exists(serverfilepath))
                    {
                        var dtResult = ConvertExcelToDataTable(serverfilepath, sheetName);
                        if (dtResult != null)
                        {
                            int count = dtResult.Columns.Count;
                            DataColumnCollection xLcolumns = dtResult.Columns;
                            //string path = HttpContext.Server.MapPath(PoolUpdaterConstant.EXCELCOLUMNINFOPATH);
                            XElement xEle = XElement.Load(excelColumnInfoPath);
                            IEnumerable<XElement> xmlColumns = xEle.Descendants(PoolUpdaterConstant.STR_EXCEL_COLUMNS).Descendants(PoolUpdaterConstant.STR_EXCEL_NODE);
                            var appId = xEle.Descendants(PoolUpdaterConstant.STR_APPLICATION).Descendants(PoolUpdaterConstant.STR_EXCEL_NODE);
                            var applicationId = xEle.Element(PoolUpdaterConstant.STR_APPLICATIONID).Value;
                            foreach (XElement xmlColumn in xmlColumns)
                            {
                                var colHeaderNameFromXml = xmlColumn.Element(PoolUpdaterConstant.XML_FIELDNAME).Value;
                                if (!xLcolumns.Contains(colHeaderNameFromXml))
                                    return BindToLoanRecordList(loanRecords, string.Empty, string.Concat(colHeaderNameFromXml, PoolUpdaterConstant.COLUMNMISMATCH));

                            }
                            loanRecords = ConvertingDataTableToLoanRecordFormat(dtResult, clientId,excelColumnInfoPath);
                            return BindToLoanRecordList(loanRecords, PoolUpdaterConstant.LOAD_SUCCESS_MESSAGE, string.Empty);
                        }
                    }
                }
            }
           
            catch (Exception ex)
            {
                string errormsg = string.Empty;
                if (ex.Message.Equals("Not a legal OleAut date."))
                {
                    errormsg = "One or more date field columns contains invalid date format.";
                }
                else
                {
                    errormsg = PoolUpdaterConstant.TECHNICALERROR;
                }
                _logger.Error("ReadLoanDetails :" + ex.Message + " " + ex.StackTrace);
                return BindToLoanRecordList(loanRecords, string.Empty, errormsg);
            }
            return BindToLoanRecordList(loanRecords, string.Empty, PoolUpdaterConstant.TECHNICALERROR);
        }

        private LoanRecordList BindToLoanRecordList(List<LoanRecord> loanRecords, string successMessage, string errorMessage)
        {
            LoanRecordList loanRecordList = new LoanRecordList();
            loanRecordList.UpdateRecords = loanRecords;
            loanRecordList.SuccessMessage = successMessage;
            loanRecordList.ErrorMessage = errorMessage;
            return loanRecordList;
        }

        public string GetServerFilePath(string excelFilename,string clientId)
        {
            string fileName = Path.GetFileName(excelFilename);
            //string clientId = Convert.ToString(Session[PoolUpdaterConstant.CURRENTLYLOGGEDINUSER]);
            fileName = string.Format("{0}_{1}", clientId, fileName);
            string serverfilepath = Path.Combine(Path.GetTempPath(), fileName);
            return serverfilepath;
        }

        //ImportData grid Binding.
        public List<LoanRecord> ConvertingDataTableToLoanRecordFormat(DataTable FiledLoanResult,string clientId,string excelColumnInfoPath)
        {
            List<LoanRecord> loanRecords = new List<LoanRecord>();
            //string path = HttpContext.Server.MapPath(PoolUpdaterConstant.EXCELCOLUMNINFOPATH);
            XElement xEle = XElement.Load(excelColumnInfoPath);
            foreach (DataRow row in FiledLoanResult.Rows)
            {
                LoanRecord updateRow = new LoanRecord();
                foreach (XElement elm in xEle.Descendants().Elements(PoolUpdaterConstant.STR_EXCEL_NODE))
                {
                    var value = row[elm.Element(PoolUpdaterConstant.XML_FIELDNAME).Value].ToString();
                    if (elm.Element(PoolUpdaterConstant.XML_FIELDTYPE).Value.ToString().ToUpper() == PoolUpdaterConstant.FIELDTYPE_DATE.ToUpper())
                    {
                        try
                        {
                            value = Convert.ToDateTime(row[elm.Element(PoolUpdaterConstant.XML_FIELDNAME).Value].ToString()).ToString(PoolUpdaterConstant.DATEFORMAT, CultureInfo.CurrentCulture);
                        }

                        catch (Exception ex)
                        {
                            updateRow.FailedReason = ex.Message;
                            
                        }
                    }
                    SetColumn(elm.Element(PoolUpdaterConstant.XML_FIELDORDER).Value, value, updateRow);
                }
                loanRecords.Add(updateRow);
            }
            return loanRecords;
        }

        private void SetColumn(string fieldOrder, string value, LoanRecord loanRecord)
        {
            switch (fieldOrder)
            {
                case "1":
                    loanRecord.Column1 = value;
                    break;
                case "2":
                    loanRecord.Column2 = value;
                    break;
                case "3":
                    loanRecord.Column3 = value;
                    break;
                case "4":
                    loanRecord.Column4 = value;
                    break;
                case "5":
                    loanRecord.Column5 = value;
                    break;
                case "6":
                    loanRecord.Column6 = value;
                    break;
                case "7":
                    loanRecord.Column7 = value;
                    break;
                case "8":
                    loanRecord.Column8 = value;
                    break;
                case "9":
                    loanRecord.Column9 = value;
                    break;
                case "10":
                    loanRecord.Column10 = value;
                    break;
                case "11":
                    loanRecord.Column11 = value;
                    break;
                case "12":
                    loanRecord.Column12 = value;
                    break;
                case "13":
                    loanRecord.Column13 = value;
                    break;
                case "14":
                    loanRecord.Column14 = value;
                    break;
                case "15":
                    loanRecord.Column15 = value;
                    break;
                case "16":
                    loanRecord.Column16 = value;
                    break;
                case "17":
                    loanRecord.Column17 = value;
                    break;
                case "18":
                    loanRecord.Column18 = value;
                    break;
                case "19":
                    loanRecord.Column19 = value;
                    break;
                case "20":
                    loanRecord.Column20 = value;
                    break;
                case "21":
                    loanRecord.Column21 = value;
                    break;
                case "22":
                    loanRecord.Column22 = value;
                    break;
                case "23":
                    loanRecord.Column23 = value;
                    break;
                case "24":
                    loanRecord.Column24 = value;
                    break;
                case "25":
                    loanRecord.Column25 = value;
                    break;
                case "26":
                    loanRecord.Column26 = value;
                    break;
                case "27":
                    loanRecord.Column27 = value;
                    break;
                case "28":
                    loanRecord.Column28 = value;
                    break;
                case "29":
                    loanRecord.Column29 = value;
                    break;
                case "30":
                    loanRecord.Column30 = value;
                    break;
                case "31":
                    loanRecord.Column31 = value;
                    break;
                case "32":
                    loanRecord.Column32 = value;
                    break;
                case "33":
                    loanRecord.Column33 = value;
                    break;
                case "34":
                    loanRecord.Column34 = value;
                    break;
                case "35":
                    loanRecord.Column35 = value;
                    break;
                case "36":
                    loanRecord.Column36 = value;
                    break;
                case "37":
                    loanRecord.Column37 = value;
                    break;
                case "38":
                    loanRecord.Column38 = value;
                    break;
                case "39":
                    loanRecord.Column39 = value;
                    break;
                case "40":
                    loanRecord.Column40 = value;
                    break;
                case "41":
                    loanRecord.Column41 = value;
                    break;
                case "42":
                    loanRecord.Column42 = value;
                    break;
                case "43":
                    loanRecord.Column43 = value;
                    break;
                case "44":
                    loanRecord.Column44 = value;
                    break;
                

            }
        }

        public DataTable ConvertExcelToDataTable(string fileName, string selectedsheet)
        {
            using (
                System.Data.OleDb.OleDbConnection objConn =
                    new OleDbConnection(string.Format(PoolUpdaterConstant.STR_OLEDB_CONNECTION, fileName)))
            {
                OleDbDataAdapter da = new OleDbDataAdapter(string.Format(PoolUpdaterConstant.STR_OLEDB_SELECTQUERY, selectedsheet), objConn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                DataTable dataTable = RemoveEmptyRowsFromDataTable(dt);
                return dataTable;
            }
        }

        public DataTable RemoveEmptyRowsFromDataTable(DataTable dt)
        {
            int columnCount = dt.Columns.Count;
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                bool allNull = true;
                for (int j = 0; j < columnCount; j++)
                {
                    if (dt.Rows[i][j] != DBNull.Value && !string.IsNullOrWhiteSpace(dt.Rows[i][j].ToString()))
                    {
                        allNull = false;
                        break;
                    }
                }
                if (allNull)
                {
                    dt.Rows[i].Delete();
                }
            }
            dt.AcceptChanges();
            return dt;
        }

        private List<SelectListItem> GetSheetNamesFromExcel(string fileName)
        {
            _logger.Info("Get SheetNames From Excel Started.");
            List<SelectListItem> Sheetnames = new List<SelectListItem>();
            using (OleDbConnection connection = new OleDbConnection(string.Format(PoolUpdaterConstant.STR_OLEDB_CONNECTION, fileName)))
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
                connection.Open();
                DataTable Sheets = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                foreach (DataRow Sheet in Sheets.Rows)
                {
                    string sheetName = Sheet[2].ToString().Replace("'", "");
                    if (!sheetName.EndsWith("$"))
                        continue;
                    sheetName = sheetName.TrimEnd('$');
                    SelectListItem item = new SelectListItem();
                    item.Text = sheetName;
                    item.Value = sheetName;
                    selectsheetlist.Add(item);
                }
                connection.Close();
            }
            return selectsheetlist;
        }

        //Export grid Binding.
        public LoanRecordList BindLoanTemplatesToExportFailedLoanGrid(List<LoanTemplateRequest> processedLoans, List<LoanRecord> excludedLoans)
        {
            LoanRecordList loanRecordList = new LoanRecordList();
            List<LoanRecord> loanRecords = new List<LoanRecord>();
            LoanRecord updateRow = null;
            if (excludedLoans != null && excludedLoans.Count > 0)
            {
                loanRecords.AddRange(excludedLoans);
            }
            if (processedLoans.Count == 0)
            {
                loanRecordList.UpdateRecords = loanRecords;
                return loanRecordList;
            }
            foreach (LoanTemplateRequest loanTemplate in processedLoans)
            {
                if (!string.IsNullOrEmpty(loanTemplate.ErrorMessage))
                {
                    updateRow = new LoanRecord();
                    updateRow.FailedReason = loanTemplate.ErrorMessage;

                    foreach (Field field in loanTemplate.Fields)
                    {
                        //Set back X and Empty values to Yes and No.
                        if (field.FieldType == FieldTypes.X)
                        {
                            string fieldvalue = string.Empty;
                            if (field.FieldValue == PoolUpdaterConstant.STRX)
                            {
                                field.FieldValue = field.FieldValue.Replace(PoolUpdaterConstant.STRX, PoolUpdaterConstant.STRYES);
                            }
                            else if (field.FieldValue == string.Empty)
                            {
                                field.FieldValue = PoolUpdaterConstant.STRNO;
                            }
                        }

                        //if (field.FieldName == PoolUpdaterConstant.STR_INVESTOR_TYPE)
                        //{
                        //    if (field.FieldValue == PoolUpdaterConstant.STR_GNMA_VALUE)
                        //    {
                        //        field.FieldValue = PoolUpdaterConstant.STR_GNMA; 
                        //    }
                        //    else if (field.FieldValue == PoolUpdaterConstant.STR_FNMA_VALUE) 
                        //    {
                        //        field.FieldValue = PoolUpdaterConstant.STR_FNMA;
                        //    }
                        //}

                        SetColumn(field.FieldOrder, field.FieldValue, updateRow);
                    }
                    loanRecords.Add(updateRow);
                }
            }
            loanRecordList.UpdateRecords = loanRecords;
            return loanRecordList;
        }

        public List<Field> EncryptFilelds(List<Field> fields)
        {
            List<Field> fieldsEncrypted = new List<Field>();
            Field encryptedField = null;
            StringCipher stringCipher = new StringCipher();
            foreach (var field in fields)
            {
                encryptedField = new Field();
                encryptedField.FieldId = stringCipher.Encrypt(field.FieldId);
                encryptedField.FieldName = stringCipher.Encrypt(field.FieldName);
                encryptedField.FieldValue = stringCipher.Encrypt(field.FieldValue);
                encryptedField.FieldType = field.FieldType;
                encryptedField.FieldOrder = field.FieldOrder;
                encryptedField.IsMandatory = field.IsMandatory;
                encryptedField.IsValidationRequired = field.IsValidationRequired;
                encryptedField.AcceptData = field.AcceptData;
                encryptedField.UseLockRequest = field.UseLockRequest;
                encryptedField.AllowEmptyUpdate = field.AllowEmptyUpdate;
                fieldsEncrypted.Add(encryptedField);
            }
            return fieldsEncrypted;
        }
        public static FieldTypes GetFieldTypes(string fieldType)
        {
            FieldTypes result = FieldTypes.NOMATCH;
            Enum.TryParse<FieldTypes>(fieldType, out result);
            return result;
        }
    }
}