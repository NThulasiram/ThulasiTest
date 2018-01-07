using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using FGMC.LoanLogicsQC.Web.Constants;
using FGMC.LoanLogicsQC.Web.FGMCQCServiceReference;
using Log4NetLibrary;

namespace FGMC.LoanLogicsQC.Web.Controllers
{
    public class GenerateReportController : Controller
    {       
        ILoggingService _logger = new FileLoggingService(typeof(GenerateReportController));
        LogModel _logModel = new LogModel();
        // GET: GenerateReport
        public ActionResult Report()
        {
            try
            {
                if (Session[LoanLogicsQCConstants.CURRENTLY_LOGGEDIN_USER] == null)
                    RedirectToLoginPage();
                using (FGMCQCServiceReference.FgmcQCServiceClient client =
                    new FgmcQCServiceClient())
                {

                    var allStatus = client.GetExtractionStatus();

                    ViewBag.DocumentExtractStatus = new SelectList(allStatus,
                        LoanLogicsQCConstants.DOCUMENT_EXTRACT_STATUSID,
                        LoanLogicsQCConstants.DOCUMENT_EXTRACT_STATUSNAME);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, ex.Message,ex:ex);
            }
            return View();
        }

        public JsonResult ReportGenerator(string statusValue, string dateFrom, string dateTo)
        {
            IEnumerable<FGMCQCServiceReference.ReportGenerator> resultSet = null;
            try
            {
                int actualValue;
                var r = int.TryParse(statusValue, out actualValue);
                DateTime fromDate;
                DateTime toDate;
                DateTime.TryParse(dateFrom, out fromDate);
                DateTime.TryParse(dateFrom, out toDate);
                var applicationId = (int)Session[LoanLogicsQCConstants.APPLICATIONID];

                using (FGMCQCServiceReference.FgmcQCServiceClient client =
                    new FgmcQCServiceClient())
                {
                    resultSet = client.GetColumnsForReport(Convert.ToInt32(actualValue), fromDate, toDate, applicationId);
                    if (resultSet == null)
                    {
                        return Json(new {data = new EmptyResult(), errormessage = LoanLogicsQCConstants.QUERY_NORESULTS },
                            JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (SqlException sqe)
            {
          
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, sqe.Message,ex:sqe);
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, ex.Message,ex:ex);
 
            }
            return Json(new {data = resultSet, sucessmessage = LoanLogicsQCConstants.SEARCH_RESULTS }, JsonRequestBehavior.AllowGet);
        }

        protected void RedirectToLoginPage()
        {
            var loginUrl = LoanLogicsQCConstants.LOGINURL;
            Session.RemoveAll();
            Session.Clear();
            Session.Abandon();
            HttpContext.Response.Redirect(loginUrl, true);
        }

    }
}