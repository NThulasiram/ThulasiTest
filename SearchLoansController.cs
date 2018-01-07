using FGMC.LoanLogicsQC.Web.Constants;
using FGMC.LoanLogicsQC.Web.Helper;
using FGMC.LoanLogicsQC.Web.FGMCQCServiceReference;
using FGMC.LoanLogicsQC.Web.Models;
using FGMC.SecurityLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FGMC.LoanLogicsQC.Web.ViewModels;
using Log4NetLibrary;
using System.Data;
using System.IO;
using ClosedXML.Excel;
using FGMC.Common.DataContract;

namespace FGMC.LoanLogicsQC.Web.Controllers 
{
    public class SearchLoansController : Controller
    {
        List<SelectListItem> selectsheetlist = new List<SelectListItem>();
        StringCipher stringCipher = new StringCipher();
        ILoggingService _logger = new FileLoggingService(typeof(SearchLoansController));
       
        LogModel _logModel = new LogModel();

        // GET: SearchLoans
        [HttpGet]
        public ActionResult SearchLoans()
        {
            if (Session[LoanLogicsQCConstants.CURRENTLY_LOGGEDIN_USER] == null)
            {
                RedirectToLoginPage();
                return RedirectToAction("Login", "Account");
            }
            int appId = (int)Session[LoanLogicsQCConstants.APPLICATIONID];
            LoanSearchViewModel modelView = new LoanSearchViewModel();
            ViewBag.ErrorMessage = TempData[LoanLogicsQCConstants.STR_ERROR_MESSAGE] as string;
            return View(modelView);
        }
        [HttpPost]
        public ActionResult SearchLoanResponse(LoanSearchViewModel viewModel)
        {
            try
            {
                int appId = (int)Session[LoanLogicsQCConstants.APPLICATIONID];
                List<FGMCQCServiceReference.LoanExtractionResponse> loanExtractionResponse = null;
                var loanSearchViewModel = new LoanSerchResultViewModel();
                if (!ModelState.IsValid)
                {
                    var Error = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault().ErrorMessage;
                    if (!string.IsNullOrEmpty(Error))
                    {
                        TempData[LoanLogicsQCConstants.STR_ERROR_MESSAGE] = Error;
                        LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, Convert.ToString(TempData[LoanLogicsQCConstants.STR_ERROR_MESSAGE]));
                        return RedirectToAction("SearchLoans", "SearchLoans");
                    }
                }
                List<string> loanNumbers = new List<string>();
                string loanFolder = viewModel.LoanFolder;
                string multilineloans = viewModel.LoanNumber;
                string[] stringSeparators = new string[] { "\r\n" };
                var loansLines = multilineloans.Split(stringSeparators, StringSplitOptions.None);
       
                foreach (var loansLine in loansLines)
                {
                    var loans = loansLine.Split(',');
                    foreach (var loan in loans)
                    {
                        if(!string.IsNullOrEmpty(loan))
                        {
                            loanNumbers.Add(loan.TrimEnd());
                            long numeric = 0;
                            bool isNumeric = long.TryParse(loan, out numeric);
                            if (isNumeric == false)
                            {
                                TempData[LoanLogicsQCConstants.STR_ERROR_MESSAGE] = LoanLogicsQCConstants.DATA_NOT_VALID;
                                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, Convert.ToString(TempData[LoanLogicsQCConstants.STR_ERROR_MESSAGE]));
                                return RedirectToAction("SearchLoans", "SearchLoans");
                            }
                        }
                    }
                }
                if(loanNumbers.Count>0)
                {
                    FgmcQCServiceClient client = new FgmcQCServiceClient();
                    try
                    {
                        loanExtractionResponse = client.GetQCLoanInfo(loanNumbers, loanFolder, GetLoggedInEncompassUser(), appId);
                        client.Close();

                    }
                    catch (Exception ex)
                    {
                        client.Abort();
                        TempData[LoanLogicsQCConstants.STR_ERROR_MESSAGE] = LoanLogicsQCConstants.TECHNICALERROR;
                        LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, "SearchLoanResponse: " + ex.Message ,ex:ex);
                        return RedirectToAction("SearchLoans", "SearchLoans");
                    }
                    List<string> loanNumberList = new List<string>();
                    var searchResults = new List<LoanSearchResult>();

                    if (loanExtractionResponse.Count <= 0)
                    {
                        TempData[LoanLogicsQCConstants.STR_ERROR_MESSAGE] = LoanLogicsQCConstants.NOLOANS_FOUND;
                        return RedirectToAction("SearchLoans", "SearchLoans");
                    }
                    foreach (FGMCQCServiceReference.LoanExtractionResponse loan in loanExtractionResponse)
                    {
                        loanNumberList.Add(loan.LoanNumber);
                        searchResults.Add(new LoanSearchResult { LoanNumber = loan.LoanNumber, LoanFolder = loan.LoanFolder, BorrowerFirstName = loan.BorrowerFirstName, BorrowerLastName = loan.BorrowerLastName });
                        loanSearchViewModel.LoanSearchResults = searchResults;
                        loanSearchViewModel.LoanNumbers = loanNumberList;
                        loanSearchViewModel.TotalInputLoans = loanNumbers.Count;    
                    }
                    TempData[LoanLogicsQCConstants.STR_LOANSEARCHVIEWMODEL] = loanSearchViewModel;
            }
            }
            catch (Exception ex)
            {
                TempData[LoanLogicsQCConstants.STR_ERROR_MESSAGE] = LoanLogicsQCConstants.TECHNICALERROR;
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, ex.Message,ex:ex);
                return RedirectToAction("SearchLoans", "SearchLoans");     
            }
            return RedirectToAction("LoanSearchResults", "LoanSearchResults");
        }

        public ActionResult SearchImportLoans()
        {
            try
            {
                List<FGMCQCServiceReference.LoanExtractionResponse> loanExtractionResponse = null;
                string ImportLoans = Session[LoanLogicsQCConstants.SESSION_IMPORTLOANS] as string;
                List<string> ImportLoanslist = ImportLoans.Split(',').ToList();
                foreach (string loan in ImportLoanslist)
                {
                    long numeric = 0;
                    bool isNumeric = long.TryParse(loan, out numeric);
                    if (isNumeric == false)
                    {
                        TempData[LoanLogicsQCConstants.STR_ERROR_MESSAGE] = LoanLogicsQCConstants.DATA_NOT_VALID;
                        LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, Convert.ToString(TempData[LoanLogicsQCConstants.STR_ERROR_MESSAGE]));
                        return RedirectToAction("SearchLoans", "SearchLoans");
                    }
                }
               
                int appId = (int)Session[LoanLogicsQCConstants.APPLICATIONID];
                FgmcQCServiceClient client = new FgmcQCServiceClient();
                try
                {
                    loanExtractionResponse  = client.GetQCLoanInfo(ImportLoanslist, string.Empty, GetLoggedInEncompassUser(), appId);
                }
                catch(Exception ex)
                {
                    TempData[LoanLogicsQCConstants.STR_ERROR_MESSAGE] = LoanLogicsQCConstants.TECHNICALERROR;
                    LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, "SearchImportLoans: " + ex.Message, ex: ex);
                    return RedirectToAction("SearchLoans", "SearchLoans");
                }
                var loanSearchViewModel = new LoanSerchResultViewModel();

                List<string> loanNumberList = new List<string>();
                if (loanExtractionResponse.Count <= 0)
                {
                    TempData[LoanLogicsQCConstants.STR_ERROR_MESSAGE] = LoanLogicsQCConstants.NOLOANS_FOUND;
                    return RedirectToAction("SearchLoans", "SearchLoans");
                }
                var searchResults=new List<LoanSearchResult>();
                foreach (FGMCQCServiceReference.LoanExtractionResponse loan in loanExtractionResponse)
                {
                    loanNumberList.Add(loan.LoanNumber);
                    searchResults.Add(new LoanSearchResult { LoanNumber = loan.LoanNumber, LoanFolder = loan.LoanFolder, BorrowerFirstName = loan.BorrowerFirstName, BorrowerLastName = loan.BorrowerLastName });
                    loanSearchViewModel.LoanSearchResults = searchResults;
                    loanSearchViewModel.LoanNumbers = loanNumberList;
                    loanSearchViewModel.TotalInputLoans = ImportLoanslist.Count;
                }
                TempData[LoanLogicsQCConstants.STR_LOANSEARCHVIEWMODEL] = loanSearchViewModel;
            }
            catch (Exception ex)
            {
                TempData[LoanLogicsQCConstants.STR_ERROR_MESSAGE] = LoanLogicsQCConstants.TECHNICALERROR;
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, "SearchImportLoans: " + ex.Message, ex: ex);
                return RedirectToAction("SearchLoans", "SearchLoans");
            }
            return RedirectToAction("LoanSearchResults", "LoanSearchResults");
        }
        public JsonResult GetSheetFromExcel(HttpPostedFileBase browsefile)
        {
            try
            {
                string clientId = Session[LoanLogicsQCConstants.CURRENTLY_LOGGEDIN_USER] as string;
                selectsheetlist = LoanDocExtractionHelper.LoadSheetsFromExcel(browsefile, clientId);
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, "GetSheetFromExcel: " + ex.Message ,ex:ex);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            return Json(new { selectsheetlist }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult OpenExcelSheet(string ExcelFilePath, string SeletedSheetName)
        {
            try
            {
                string clientId = Session[LoanLogicsQCConstants.CURRENTLY_LOGGEDIN_USER] as string;
                string excelServerPath = LoanDocExtractionHelper.GetServerFilePath(ExcelFilePath, clientId);
                //var loaninfo = LoanDocExtractionHelper.ExcelDataToComaSepertedString(excelServerPath, SeletedSheetName);
              
                var dtExcelData = LoanDocExtractionHelper.ExcelData(excelServerPath, SeletedSheetName);
                if (dtExcelData != null)
                {
                    string loanInfo = string.Empty;
                    foreach (DataRow row in dtExcelData.Rows)
                    {
                        loanInfo += row["Loan Number"].ToString() + ",";
                    }
                    if (!string.IsNullOrEmpty(loanInfo))
                    {
                        loanInfo = loanInfo.TrimEnd(',');
                    }
                    Session[LoanLogicsQCConstants.SESSION_IMPORTLOANS] = loanInfo;
                    return Json(new { data = loanInfo, sucessmessage = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogHelper.Info(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, "OpenExcelSheet: " + LoanLogicsQCConstants.NO_LOANCOLUMN_FOUND);
                    return Json(new { data = LoanLogicsQCConstants.NO_LOANCOLUMN_FOUND, errormessage = LoanLogicsQCConstants.NO_LOANCOLUMN_FOUND }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, "OpenExcelSheet: " + ex.Message ,ex:ex);
                return Json(new { data = LoanLogicsQCConstants.NO_LOANCOLUMN_FOUND, errormessage = LoanLogicsQCConstants.TECHNICALERROR }, JsonRequestBehavior.AllowGet);
            }
        }

        private FGMCQCServiceReference.EncompassUser GetLoggedInEncompassUser()
        {
            FGMCQCServiceReference.EncompassUser user =
                    new FGMCQCServiceReference.EncompassUser();
            try
            {
                user.UserId =
                    stringCipher.Encrypt(Convert.ToString(Session[LoanLogicsQCConstants.CURRENTLY_LOGGEDIN_USER]));
                user.Password = stringCipher.Encrypt(Convert.ToString(Session[LoanLogicsQCConstants.PASSWORD]));
                user.Server = stringCipher.Encrypt(Convert.ToString(Session[LoanLogicsQCConstants.SERVER]));
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, "GetLoggedInEncompassUser: " + ex.Message ,ex:ex);
            }
            return user;
        }

        public ActionResult TemplateDownload()
        {
            try
            {
                System.Data.DataTable TempDataTable = new System.Data.DataTable("Loan Logic QC Template");
                TempDataTable.Columns.Add("Loan Number");
                MemoryStream stream = new MemoryStream();
                var workbook = new XLWorkbook();
                workbook.Worksheets.Add(TempDataTable, TempDataTable.TableName);
                workbook.SaveAs(stream);
                stream.Position = 0;
                return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = TempDataTable.TableName + ".xlsx" };
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, "TemplateDownload:" + ex.Message ,ex:ex);
                return Json(new { downloaderrormessage = LoanLogicsQCConstants.FILEDOWNLOADFAILED }, JsonRequestBehavior.AllowGet);
            }
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