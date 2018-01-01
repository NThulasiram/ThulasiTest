using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Services;
using System.Web.UI;
using System.Xml;
using EllieMae.EMLite.DataEngine;
using EllieMae.Encompass.Automation;
using EllieMae.Encompass.BusinessObjects.Loans;
using FGMCPoolUpdater.ViewModels;
using Excel = Microsoft.Office.Interop.Excel;
using EllieMae.Encompass.Client;
using EllieMae.Encompass.Collections;
using EllieMae.Encompass.Query;
using EllieMae.Encompass.BusinessObjects.Loans.Logging;
using System.Xml.Linq;
using FGMC.PoolUpdater.DataContract;
using FGMCPoolUpdater.Hubs;
using Microsoft.AspNet.SignalR;
using FGMCPoolUpdater.Logging;
using PoolUpdaterLibrary;
using Log4NetLibrary;
using FGMCPoolUpdater.Constants;

namespace FGMCPoolUpdater.Controllers
{
    public class PoolUpdaterController : BaseController
    {
        //private readonly ILogger _logger;
        private readonly IPoolUpdater _poolUpdater;
        ILogService _logger = new FileLogService(typeof(PoolUpdaterController));
        public PoolUpdaterController(ILogger logger, IPoolUpdater poolUpdater)
        {
            //_logger = logger;
            _poolUpdater = poolUpdater;
        }

        List<SelectListItem> selectsheetlist = new List<SelectListItem>();
        List<Microsoft.Office.Interop.Excel.Worksheet> WorksheetsList = new List<Excel.Worksheet>();
        private Microsoft.Office.Interop.Excel.Application _app = null;
        LoanTemplateViewModel PoolData = new LoanTemplateViewModel();
        

        public ActionResult ImportPoolData()
        {
            if (!IsValidSession())
                RedirectToLoginPage();

            ViewBag.Worksheets = selectsheetlist;
            return View(PoolData);
        }

        [HttpPost]
        public JsonResult ImportPoolData(HttpPostedFileBase browsefile)
        {
            selectsheetlist = LoadSheetsFromExcel(browsefile);
           

            return Json(new { selectsheetlist }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult LoadPoolData(string excelFilename, string sheetName)
        {
            List<PoolUpdateLoanTemplate> poolDataList = new List<PoolUpdateLoanTemplate>();
            _logger.Info("Binding Data from sheet " + sheetName + "to the Grid");
            LoanDeail LoanDeail= _poolUpdater.ReadLoanDetails(excelFilename, sheetName);

            if (LoanDeail != null)
            {
                poolDataList = LoanDeail.PoolUpdateLoanTemplate;
                Session["LoanList"] = LoanDeail.PoolUpdateLoanTemplate;
                if (!string.IsNullOrEmpty(LoanDeail.SuccessMessage))
                {
                    return Json(new { data = poolDataList, sucessmessage = LoanDeail.SuccessMessage }, JsonRequestBehavior.AllowGet);
                }

                else
                {
                    return Json(new { data = poolDataList, errormessage = LoanDeail.ErrorMessage }, JsonRequestBehavior.AllowGet);
                }
            }
           
            return Json(new { data = poolDataList, errormessage = PoolUpdaterConstants.TechnicalError }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult UploadPoolData(string vals, string connectionId)
        {


            List<PoolUpdateLoanTemplate> FailedLoanList = new List<PoolUpdateLoanTemplate>();
            string[] loans = vals.Split(',');

            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

            List<PoolUpdateLoanTemplate> loanTemplateViewModels = Session["LoanList"] as List<PoolUpdateLoanTemplate>;
            EllieMae.Encompass.Client.Session session = Session["Session"] as EllieMae.Encompass.Client.Session;
         

            LoanDeail loanDetail=  _poolUpdater.UpdateLoanDetails(loans, loanTemplateViewModels, connectionId, session, context);
            if (loanDetail != null)
            {
                FailedLoanList = loanDetail.PoolUpdateLoanTemplate;
                if (!string.IsNullOrEmpty(loanDetail.SuccessMessage))
                {
                    return Json(new { data = FailedLoanList, sucessmessage = loanDetail.SuccessMessage }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { data = FailedLoanList, errormessage = loanDetail.ErrorMessage }, JsonRequestBehavior.AllowGet);
                }
              
            }

            return Json(new { data = FailedLoanList, errormessage = PoolUpdaterConstants.TechnicalError }, JsonRequestBehavior.AllowGet);

        }


        public List<SelectListItem> LoadSheetsFromExcel(HttpPostedFileBase browsefile)
        {
            string filename = Path.GetFileName(browsefile.FileName);
            _logger.Info(Session["CurrentlyLoggedInUser"] + " User Selected the file " + filename);
            if (filename != null && System.IO.File.Exists((Path.Combine(Path.GetTempPath(), filename))))
            {
                System.IO.File.Delete((Path.Combine(Path.GetTempPath(), filename)));
            }

            if (filename != null)
            {
                string filepath = Path.Combine(Path.GetTempPath(), Path.GetFileName(filename));
                browsefile.SaveAs(filepath);
                _logger.Info(Session["CurrentlyLoggedInUser"] + " Saving the file to the location " + filepath);

                if (_app == null)
                {
                    _app = new Microsoft.Office.Interop.Excel.Application();
                }
                Microsoft.Office.Interop.Excel.Workbooks wbs = _app.Workbooks;
                Microsoft.Office.Interop.Excel.Workbook wb = wbs.Open(filepath, Type.Missing, Type.Missing, Type.Missing,
                    Type.Missing,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                    Type.Missing, Type.Missing);

                if (browsefile.ContentLength > 0)
                {
                    try
                    {
                        foreach (Microsoft.Office.Interop.Excel.Worksheet worksheet in wb.Worksheets)
                        {
                            SelectListItem item = new SelectListItem();
                            item.Text = worksheet.Name;
                            item.Value = worksheet.CodeName;
                            selectsheetlist.Add(item);

                            WorksheetsList.Add(worksheet);

                        }
                        _logger.Info("Worksheets in the file " + WorksheetsList);
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = "ERROR:" + ex.Message.ToString();
                        _logger.Error(ViewBag.Message);
                    }
                }

                else
                {
                    ViewBag.Message = "You have not specified a file.";
                }

                ViewBag.Worksheets = selectsheetlist;

               

                wb.Close();
                wbs.Close();
                _app.Quit();

                Marshal.ReleaseComObject(wb);
                Marshal.ReleaseComObject(wbs);
                Marshal.ReleaseComObject(_app);
                _app = null;
               
            }

            return selectsheetlist;
        }
    }
}