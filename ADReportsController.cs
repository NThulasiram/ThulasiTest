using AutoMapper;
using FGMC.RMS.Web.Constants;
using FGMC.RMS.Web.Models;
using FGMC.RMS.Web.RMSServiceReference;
using FGMC.SecurityLibrary;
using Log4NetLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace FGMC.RMS.Web.Controllers
{
    public class ADReportsController : Controller
    {
        ILoggingService _logger = new DatabaseLoggingService(typeof(ADReportsController));
        LogModel _logModel = new LogModel();
        StringCipher stringCipher = new StringCipher();
        // GET: ADReports
        [CustomAuthorize]
        public ActionResult Index()
        {
            ADReportsModel model = new Models.ADReportsModel();
            model.ADUsers = GetADUsersForDropDown();
            return View(model);
        }

        public ActionResult GenerateReport()
        {
            ADReportsModel model = new Models.ADReportsModel();
            try
            {
                var requestForUsers = Request.Form["requestForUser"].ToString();
                var selectUserID = Request.Form["selectUserID"].ToString();
                List<long> userIDs = new List<long>();

                if (requestForUsers.Contains(","))
                {
                    var allIDs = requestForUsers.Split(',').ToList();
                    foreach (var item in allIDs)
                    {
                        userIDs.Add(Convert.ToInt64(item));
                    }
                }
                else
                {
                    userIDs.Add(Convert.ToInt64(requestForUsers));
                }
                using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    List<ADReportDTO> report = client.GetADReport(userIDs);
                    Mapper.Initialize(cfg => cfg.CreateMap<ADReportDTO, ADReportData>());
                    foreach (var rep in report)
                    {
                        ADReportData reportEntry = Mapper.Map<ADReportData>(rep);
                        model.ReportData.Add(reportEntry);
                    }
                    if (model.ReportData.Count == 0)
                    {
                        model.ErrorMessage = "No matching records found.";
                    }
                }
                model.ADUsers = GetADUsersForDropDown();
                model.SelectedADUser = selectUserID == string.Empty ? 0 : Convert.ToInt64(selectUserID);
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            return PartialView("_ADReportGridPartialView", model);
        }

        private List<ADUserDTO> GetADUsersForDropDown()
        {
            List<ADUserDTO> ADUsers = new List<ADUserDTO>();
            try
            {
                bool isManager = (string)Session[RMSConstants.IS_MANAGER] == RMSConstants.YES;
                bool isAdmin = (string)Session[RMSConstants.IS_ADMIN] == RMSConstants.YES;
                if (isAdmin)
                {
                    ADUsers = GetAllRMSADUsers();
                }
                else
                {
                    using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                    {
                        string manager = Session[RMSConstants.USERNAME].ToString();
                        ADUsers = client.GetAllADUsersFromRMSByManager(stringCipher.Encrypt(manager));
                        ADUsers = ADUsers.OrderBy(usr => usr.ADUserName).ToList();
                    }
                }
                if (ADUsers.Count > 0)
                {
                    ADUsers = ADUsers.FindAll(user => user.ADUserIsActive == true && user.ADUserIsDelete == false);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            return ADUsers;
        }

        private List<ADUserDTO> GetAllRMSADUsers()
        {
            List<ADUserDTO> AllADUsers = new List<ADUserDTO>();
            using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
            {
                var users = client.GetAllADUsersFromRMS();
                AllADUsers = (from usr in users
                              orderby usr.ADUserName
                              select usr).ToList();
            }
            return AllADUsers;
        }
    }
}