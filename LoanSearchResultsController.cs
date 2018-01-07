using FGMC.LoanLogicsQC.Web.Constants;
using FGMC.LoanLogicsQC.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using FGMC.SecurityLibrary;
using FGMC.Common.DataContract;
using FGMC.LoanLogicsQC.Web.Helper;
using System.IO;
using ClosedXML.Excel;
using Log4NetLibrary;
using System.Text;
using FGMC.LoanLogicsQC.Web.FGMCQCServiceReference;
using EncompassUser = FGMC.LoanLogicsQC.Web.FGMCQCServiceReference.EncompassUser;
using TrackDocumentInfo = FGMC.LoanLogicsQC.Web.FGMCQCServiceReference.TrackDocumentInfo;

namespace FGMC.LoanLogicsQC.Web.Controllers
{
    public class LoanSearchResultsController : Controller
    {

        ILoggingService _logger = new FileLoggingService(typeof(LoanSearchResultsController));
        LogModel _logModel = new LogModel();
        // GET: LoanSearchResults
        public ActionResult LoanSearchResults()
        {
            if (Session[LoanLogicsQCConstants.CURRENTLY_LOGGEDIN_USER] == null)
            {
                RedirectToLoginPage();
                return RedirectToAction("Login", "Account");
            }
            var vm = TempData[LoanLogicsQCConstants.STR_LOANSEARCHVIEWMODEL] as LoanSerchResultViewModel;
            if (vm == null) return RedirectToAction("SearchLoans", "SearchLoans");
            var appId = (int) Session[LoanLogicsQCConstants.APPLICATIONID];
            var client = new FgmcQCServiceClient();
            int totalInputLoansCount = 0;       
                 totalInputLoansCount = vm.TotalInputLoans;   
            try
            {
                var reportTypes = client.GetConfigurationsByConfigType((int)ConfiguartionTypes.LOANLOGICSREPORTTYPE, appId);
                var preClosedReports = client.GetConfigurationsByConfigType((int)ConfiguartionTypes.LOANLOGICSPOSTCLOSEREPORTTYPE, appId);
             
                vm.LoanLogicReportTypes = new SelectList(reportTypes,
                        LoanLogicsQCConstants.STR_CONFIGURATION_ID, LoanLogicsQCConstants.STR_CONFIGURATION_VALUE);
                    vm.PostClosedReportTypes = new SelectList(preClosedReports,
                        LoanLogicsQCConstants.STR_CONFIGURATION_ID, LoanLogicsQCConstants.STR_CONFIGURATION_VALUE);
                    vm.SuceessCountInfo = string.Format(LoanLogicsQCConstants.SUCEESSINFO_COUNT,
                        vm.LoanNumbers.Count, totalInputLoansCount, totalInputLoansCount - vm.LoanNumbers.Count);
            }
            catch (Exception ex)
            {
                client.Abort();
                TempData[LoanLogicsQCConstants.STR_ERROR_MESSAGE] = LoanLogicsQCConstants.TECHNICALERROR;
              
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, "LoanSearchResults : " + ex.Message ,ex:ex);
                return RedirectToAction("SearchLoans", "SearchLoans");
            }
            finally
            {
                //closing the client.abort in finally block instead of try,because client.abort can leave our service in faulty state ,hence can't close.
                client.Close();
            }
            return View(vm);
        }

        [HttpPost]
        public ActionResult ExtractAndGenerateReports(string loanNumbers, string reportType, string postClosedReportType,
            string monthid, string chkCopyFlieMergeFailed, string continueProcessOnConversionFailure)
        {
            var trackDocumentInfo = new TrackDocumentInfo();
            loanNumbers = loanNumbers.TrimEnd(',');

            if (Session[LoanLogicsQCConstants.CURRENTLY_LOGGEDIN_USER] == null || Session[LoanLogicsQCConstants.APPLICATIONID] == null)
            {
                
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, "ExtractAndGenerateReports : Session time out.");
                return Json(new {errormessage = LoanLogicsQCConstants.TECHNICALERROR, status = 401}, JsonRequestBehavior.AllowGet);
            }

            if (!string.IsNullOrEmpty(loanNumbers))
                trackDocumentInfo.LoanNumbers = loanNumbers.Split(',').ToList();
            trackDocumentInfo.CreatedByUserName = Session[LoanLogicsQCConstants.CURRENTLY_LOGGEDIN_USER] as string;
            trackDocumentInfo.DestinationTypePID = Convert.ToInt32(reportType);
            if (!string.IsNullOrEmpty(postClosedReportType))
                trackDocumentInfo.DestinationSubTypeId = Convert.ToInt32(postClosedReportType);
            trackDocumentInfo.MonthId = Convert.ToInt32(monthid);
            trackDocumentInfo.CopyMergedFileFail = Convert.ToBoolean(chkCopyFlieMergeFailed);
            trackDocumentInfo.ContinueProcessOnConversionFailure = Convert.ToBoolean(continueProcessOnConversionFailure);
            trackDocumentInfo.ApplicationID = (int) Session[LoanLogicsQCConstants.APPLICATIONID];

            if (trackDocumentInfo.LoanNumbers != null && trackDocumentInfo.LoanNumbers.Count > 0)
            {
                try
                {
                    var client = new FgmcQCServiceClient();
                    var result = client.ExtractAndGenerateReports(trackDocumentInfo);
                    if (result)
                    {
                    
                        return Json(new {sucessmessage = LoanLogicsQCConstants.SUCESSMESSAGE}, JsonRequestBehavior.AllowGet);
                    }
                    LogHelper.Info(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, "ExtractAndGenerateReports : Server return false result.");
                    return Json(new {errormessage = LoanLogicsQCConstants.TECHNICALERROR}, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, "ExtractAndGenerateReports : " + ex.Message ,ex:ex);
                    return Json(new {errormessage = LoanLogicsQCConstants.TECHNICALERROR}, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new {errormessage = LoanLogicsQCConstants.lOANNUMBERMANDATORY}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DownloadExcel(string loanNumbers, string reportType, string postClosedReportType)
        {
            var stream = new MemoryStream();
            try
            {
                var client = new FgmcQCServiceClient();
                loanNumbers = loanNumbers.TrimEnd(',');
                var numbers = loanNumbers.Split(',').ToList();
                DataTable dataTable=new DataTable();
                if (reportType == LoanLogicsQCConstants.POSTCLOSEREPORT)
                {
                  
                    var result1 = client.GenerateLoanLogicsPostCloseReport(numbers, reportType, postClosedReportType,LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID);
                    dataTable = DataGenerator.FillDataTableForPostClose(result1, reportType);
                }
                else
                {
                    var result = client.GenerateLoanLogicsReport(numbers, reportType, postClosedReportType, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID);
                    dataTable = DataGenerator.FillDataTable(result, reportType);
                }

                var workbook = new XLWorkbook();
                workbook.Worksheets.Add(dataTable, dataTable.TableName);
                workbook.SaveAs(stream);
                stream.Position = 0;

                return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = dataTable.TableName + ".xlsx"
                };
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, "DownloadExcel :" + ex.Message, ex: ex);
                var info = ex.Message;
                var bytedata = Encoding.UTF8.GetBytes(info);
                return File(bytedata, "text/plain", "DownloadFailed.txt");
            }
        }

        private EncompassUser GetLoggedInEncompassUser()
        {
            var stringCipher = new StringCipher();
            var user = new EncompassUser();
            user.UserId = stringCipher.Encrypt(Convert.ToString(Session[LoanLogicsQCConstants.CURRENTLY_LOGGEDIN_USER]));
            user.Password = stringCipher.Encrypt(Convert.ToString(Session[LoanLogicsQCConstants.PASSWORD]));
            user.Server = stringCipher.Encrypt(Convert.ToString(Session[LoanLogicsQCConstants.SERVER]));
            return user;
        }

        protected void RedirectToLoginPage()
        {
            Session.RemoveAll();
            Session.Clear();
            Session.Abandon();
            HttpContext.Response.Redirect(LoanLogicsQCConstants.LOGINURL, true);
        }
    }
}