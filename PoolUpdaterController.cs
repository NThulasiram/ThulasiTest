using FGMC.EncompassUtility.Security;
using FGMCPoolUpdater.Constants;
using FGMCPoolUpdater.Helper;
using FGMCPoolUpdater.Hubs;
using FGMCPoolUpdater.Models;
using FGMCPoolUpdater.PoolUpdaterServiceReference;
using FGMCPoolUpdater.ViewModels;
using Log4NetLibrary;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace FGMCPoolUpdater.Controllers
{
    public class PoolUpdaterController : BaseController
    {
        ILogService _logger = new FileLogService(typeof(PoolUpdaterController));
        List<SelectListItem> selectsheetlist = new List<SelectListItem>();
        LoanTemplateViewModel PoolData = new LoanTemplateViewModel();
        PoolUpdaterHelper poolUpdaterHelper = new PoolUpdaterHelper();
        StringCipher stringCipher = new StringCipher();

        public ActionResult ImportPoolData()
        {
            try
            {
                if (!IsValidSession())
                    RedirectToLoginPage();

                ViewBag.Worksheets = selectsheetlist;
            }
            catch (Exception ex)
            {
                _logger.Error("ImportPoolData : " + ex.Message + " " + ex.StackTrace);
            }

            return View(PoolData);
        }

        [HttpPost]
        public JsonResult ImportPoolData(HttpPostedFileBase browsefile)
        {
            try
            {
                string clientId = Convert.ToString(Session[PoolUpdaterConstant.CURRENTLY_LOGGEDIN_USER]);

                selectsheetlist = poolUpdaterHelper.LoadSheetsFromExcel(browsefile, clientId);
            }
            catch (Exception ex)
            {
                _logger.Info("ImportPoolData : " + ex.Message + " " + ex.StackTrace);
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            return Json(new { selectsheetlist }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadPoolData(string excelFilename, string sheetName)
        {
            List<LoanRecord> warehouseUpdateColumns = new List<LoanRecord>();
            try
            {
                _logger.Info(string.Format(PoolUpdaterConstant.MSGBINDDATATOSHEET, sheetName));
                string path = HttpContext.Server.MapPath(PoolUpdaterConstant.EXCEL_COLUMNINFO_PATH);
                string clientId = Convert.ToString(Session[PoolUpdaterConstant.CURRENTLY_LOGGEDIN_USER]);
                var loanDetail = poolUpdaterHelper.ReadLoanDetails(excelFilename, sheetName, path, clientId);
                if (loanDetail != null)
                {
                    warehouseUpdateColumns = loanDetail.UpdateRecords;
                    if (!string.IsNullOrEmpty(loanDetail.SuccessMessage))
                    {
                        return Json(new { data = warehouseUpdateColumns, sucessmessage = loanDetail.SuccessMessage }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { data = warehouseUpdateColumns, errormessage = loanDetail.ErrorMessage }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("LoadPoolData " + ex.Message + " " + ex.StackTrace);
            }
            return Json(new { data = warehouseUpdateColumns, errormessage = PoolUpdaterConstant.TECHNICALERROR }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UploadPoolData(string excludedLoans, string connectionId, string selectSheetName, string excelFilename)
        {
            List<LoanRecord> failedLoanList = null;
            try
            {
                string clientId = Convert.ToString(Session[PoolUpdaterConstant.CURRENTLY_LOGGEDIN_USER]);
                string path = HttpContext.Server.MapPath(PoolUpdaterConstant.EXCEL_COLUMNINFO_PATH);
                string serverfilepath = poolUpdaterHelper.GetServerFilePath(excelFilename, clientId);
                _logger.Info("UploadPoolData: Checking for server file path.");
                if (System.IO.File.Exists(serverfilepath))
                {
                    DataTable dtProcessedLoans = poolUpdaterHelper.ConvertExcelToDataTable(serverfilepath, selectSheetName);
                    int totalProcessingLoans = dtProcessedLoans.Rows.Count;
                    DataTable failedLoans = dtProcessedLoans.Clone();
                    if (!string.IsNullOrEmpty(excludedLoans))
                    {
                        string[] loans = excludedLoans.TrimEnd(',').Split(',');

                        for (int i = dtProcessedLoans.Rows.Count - 1; i >= 0; i--)
                        {
                            DataRow dr = dtProcessedLoans.Rows[i];
                            if (loans.ToList().Any(p => p.Equals(dr[PoolUpdaterConstant.FIELD_LOANNUMBER].ToString())))
                            {
                                failedLoans.Rows.Add(dr.ItemArray);
                                dr.Delete();
                            }
                            dtProcessedLoans.AcceptChanges();
                        }
                    }
                    List<LoanRecord> excludedLoanList = new List<LoanRecord>();
                    if (failedLoans.Rows.Count > 0)
                    {
                        _logger.Info("UploadPoolData: Converting DataTable To LoanRecordFormat.");
                        excludedLoanList = poolUpdaterHelper.ConvertingDataTableToLoanRecordFormat(failedLoans, clientId, path);
                        foreach (LoanRecord excludedLoan in excludedLoanList)
                        {
                            excludedLoan.FailedReason = PoolUpdaterConstant.MSG_EXCLUDEDLOAN;
                        }
                    }
                    LoanTemplateResponse loansUpdateResponse = null;
                    List<LoanTemplateRequest> processedLoans=new List<LoanTemplateRequest>();
                    if (dtProcessedLoans.Rows.Count > 0)
                    {

                        var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                        XElement xEle = XElement.Load(path);
                        IEnumerable<XElement> xmlColumns = xEle.Descendants(PoolUpdaterConstant.STR_EXCEL_COLUMNS).Descendants(PoolUpdaterConstant.STR_EXCEL_NODE);
                        Field field = null;
                        List<Field> fields = null;
                        LoanTemplateRequest loanTemplate = null;
                        List<LoanTemplateRequest> loanTemplateRequest;


                        #region [ Temporary Logic For pool updater issue ]

                        int limit = Convert.ToInt32(ConfigurationManager.AppSettings["LoanLimit"]);

                        for (int i = 0; i < dtProcessedLoans.Rows.Count; i++)
                        {

                            if (dtProcessedLoans.Rows.Count < i * limit)
                            {
                                continue;
                            }

                            var selectedRows = dtProcessedLoans.Select().Skip(i * limit).Take(limit);
                            loanTemplateRequest = new List<LoanTemplateRequest>();

                            foreach (DataRow row in selectedRows)
                            {
                                loanTemplate = new LoanTemplateRequest();
                                fields = new List<Field>();
                                foreach (XElement xmlColumn in xmlColumns)
                                {
                                    field = new Field();
                                    loanTemplate.LoanNumber = stringCipher.Encrypt(Convert.ToString(row[PoolUpdaterConstant.EXCEL_LOANNUMBER]));
                                    field.FieldId = xmlColumn.Element(PoolUpdaterConstant.XML_FIELDID).Value;
                                    field.FieldName = xmlColumn.Element(PoolUpdaterConstant.FIELDNAME).Value;
                                    field.FieldValue = Convert.ToString(row[xmlColumn.Element(PoolUpdaterConstant.FIELDNAME).Value]);
                                    field.FieldType = PoolUpdaterHelper.GetFieldTypes(xmlColumn.Element(PoolUpdaterConstant.XML_FIELDTYPE).Value.ToUpper());
                                    field.FieldOrder = xmlColumn.Element(PoolUpdaterConstant.XML_FIELDORDER).Value;
                                    field.IsMandatory = xmlColumn.Element(PoolUpdaterConstant.XML_ISMANDATORY).Value.ToLower() == PoolUpdaterConstant.STRYES.ToLower() ? true : false;
                                    field.IsValidationRequired = xmlColumn.Element(PoolUpdaterConstant.XML_ISVALIDATION_REQUIRED).Value.ToLower() == PoolUpdaterConstant.STRYES.ToLower() ? true : false;
                                    field.AcceptData = xmlColumn.Element(PoolUpdaterConstant.XML_ACCEPTDATA).Value;
                                    field.UseLockRequest = xmlColumn.Element(PoolUpdaterConstant.XML_UseLockRequest).Value.ToLower() == PoolUpdaterConstant.STRYES.ToLower() ? true : false;
                                    field.AllowEmptyUpdate = xmlColumn.Element(PoolUpdaterConstant.XML_ALLOWEMPTY_UPDATE).Value.ToLower() == PoolUpdaterConstant.STRYES.ToLower() ? true : false;
                                    field = UpdateField(field);
                                    fields.Add(field);
                                }

                                var encryptedFields = poolUpdaterHelper.EncryptFilelds(fields);
                                loanTemplate.Fields = encryptedFields;
                                loanTemplateRequest.Add(loanTemplate);
                            }

                            PoolUpdaterServiceClient client = new PoolUpdaterServiceClient();
                            try
                            {
                                _logger.Info(PoolUpdaterConstant.LOG_UPLOADPOOLDATA_METHOD);
                                loansUpdateResponse = client.UpdateLoanFields(loanTemplateRequest, GetLoggedInEncompassUser(), connectionId);
                                client.Close();
                                if (loansUpdateResponse != null)
                                {
                                    if (string.IsNullOrEmpty(loansUpdateResponse.ErrorMessage))
                                    {
                                        processedLoans.AddRange(loansUpdateResponse.LoanTemplates);
                                    }
                                    else
                                    {
                                        _logger.Error(" UploadPoolData process for chunk of loans skipped due to below reason: " + loansUpdateResponse.ErrorMessage);
                                        //Hot Fix for Error: FGMCEN-5769
                                        return Json(new { data = failedLoanList = new List<LoanRecord>(), errormessage = loansUpdateResponse.ErrorMessage }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                            catch (TimeoutException ex)
                            {
                                client.Abort();
                                _logger.Error("UploadPoolData: " + ex.Message + " " + ex.StackTrace);
                                return Json(new { data = failedLoanList = new List<LoanRecord>(), errormessage = PoolUpdaterConstant.TECHNICALERROR }, JsonRequestBehavior.AllowGet);
                            }
                            catch (CommunicationException ex)
                            {
                                client.Abort();
                                _logger.Error("UploadPoolData: " + ex.Message + " " + ex.StackTrace);
                                return Json(new { data = failedLoanList = new List<LoanRecord>(), errormessage = PoolUpdaterConstant.TECHNICALERROR }, JsonRequestBehavior.AllowGet);
                            }
                            catch (Exception ex)
                            {
                                client.Close();
                                _logger.Error("UploadPoolData: " + ex.Message + " " + ex.StackTrace);
                                return Json(new { data = failedLoanList = new List<LoanRecord>(), errormessage = PoolUpdaterConstant.TECHNICALERROR }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        #endregion [ Temporary Logic For pool updater issue ]
                    }

                    if (processedLoans.Count > 0 || excludedLoanList.Count>0)
                    {
                        LoanRecordList loanRecordList = poolUpdaterHelper.BindLoanTemplatesToExportFailedLoanGrid(processedLoans, excludedLoanList);
                        failedLoanList = loanRecordList.UpdateRecords;
                        int sucessCount = totalProcessingLoans - failedLoanList.Count;

                        if (failedLoanList.Count == 0)
                        {
                            loanRecordList.SuccessMessage = string.Format(PoolUpdaterConstant.IMPORT_ALL_SUCCESS, sucessCount, totalProcessingLoans);
                        }
                        else
                        {
                            loanRecordList.SuccessMessage = string.Format(PoolUpdaterConstant.IMPORT_SUCCESS, sucessCount, totalProcessingLoans);
                        }
                        return Json(new {data = failedLoanList, sucessmessage = loanRecordList.SuccessMessage}, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { data = failedLoanList = new List<LoanRecord>(), errormessage = PoolUpdaterConstant.TECHNICALERROR }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("UploadPoolData " + ex.Message + " " + ex.StackTrace);
            }
            return Json(new { data = failedLoanList = new List<LoanRecord>(), errormessage = PoolUpdaterConstant.TECHNICALERROR }, JsonRequestBehavior.AllowGet);
        }

        private EncompassUser GetLoggedInEncompassUser()
        {
            EncompassUser user = new EncompassUser();
            user.UserId = stringCipher.Encrypt(Convert.ToString(Session[PoolUpdaterConstant.CURRENTLY_LOGGEDIN_USER]));
            user.Password = stringCipher.Encrypt(Convert.ToString(Session[PoolUpdaterConstant.PASSWORD]));
            user.Server = stringCipher.Encrypt(Convert.ToString(Session[PoolUpdaterConstant.SERVER]));
            user.ApplicationName = stringCipher.Encrypt(PoolUpdaterConstant.POOLUPDATER_DEPENDENCY_RESOLVE);
            return user;
        }
        public Field UpdateField(Field field)
        {
            if (field.FieldType == FieldTypes.DATE)
            {
                try
                {
                    field.FieldValue = Convert.ToDateTime(field.FieldValue).ToString(PoolUpdaterConstant.DATEFORMAT);
                }
                catch (Exception ex)
                {
                    field.ErrorMessage = ex.Message;
                    return field;
                }
            }
            if (field.FieldType == FieldTypes.X)
            {
                string fieldvalue = string.Empty;
                if (field.FieldValue == PoolUpdaterConstant.STRYES)
                {
                    field.FieldValue = field.FieldValue.Replace(PoolUpdaterConstant.STRYES, PoolUpdaterConstant.STRX);
                }
                else if (field.FieldValue == PoolUpdaterConstant.STRNO)
                {
                    field.FieldValue = field.FieldValue.Replace(PoolUpdaterConstant.STRNO, string.Empty);
                }
                else
                    field.ErrorMessage = PoolUpdaterConstant.MSG_ALLOWEDVALUES_YESNO;
            }

            return field;
        }
    }
}