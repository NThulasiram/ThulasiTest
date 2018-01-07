using AutoMapper;
using FGMC.RMS.Web.Constants;
using FGMC.RMS.Web.Models;
using FGMC.RMS.Web.RMSServiceReference;
using Log4NetLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace FGMC.RMS.Web.Controllers
{
    public class EncompassReportController : Controller
    {
        ILoggingService _logger = new DatabaseLoggingService(typeof(EncompassReportController));
        LogModel _logModel = new LogModel();

        // GET: EncompassReport
        [CustomAuthorize]
        public ActionResult Index()
        {
            EncompassReportsModel model = new Models.EncompassReportsModel();
            model.EncompassUsers = GetEncompassUsersByEnvironment();
            return View(model);
        }

        public ActionResult PopulateUsers()
        {
            EncompassReportsModel model = new EncompassReportsModel();
            try
            {
                model.EncompassUsers = GetEncompassUsersByEnvironment();
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            return PartialView("_EncompassReportGridPartialView", model);
        }

        public ActionResult GenerateReport()
        {
            EncompassReportsModel model = new Models.EncompassReportsModel();
            try
            {
                var requestForUsers = Request.Form["requestForUser"].ToString();
                var selectUserID = Request.Form["selectUserID"].ToString();
                List<long> userIDs = new List<long>();
                model.EncompassUsers = GetEncompassUsersByEnvironment();

                if(string.IsNullOrEmpty(requestForUsers) && string.IsNullOrEmpty(requestForUsers))
                {
                    model.SelectedEncompassUser = selectUserID == string.Empty ? 0 : Convert.ToInt64(selectUserID);
                    return PartialView("_EncompassReportGridPartialView", model);
                }

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

                //get the data from service
                using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    List<PersonaReportDTO> report = client.GetPersonaReport(userIDs);
                    Mapper.Initialize(cfg => cfg.CreateMap<PersonaReportDTO, EncompassReportData>());
                    foreach (var rep in report)
                    {
                        EncompassReportData reportEntry = Mapper.Map<EncompassReportData>(rep);
                        model.ReportData.Add(reportEntry);
                    }
                    if (model.ReportData.Count == 0)
                    {
                        model.ErrorMessage = "No matching records found.";
                    }
                }
                model.SelectedEncompassUser = selectUserID == string.Empty ? 0 : Convert.ToInt64(selectUserID);
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            return PartialView("_EncompassReportGridPartialView", model);
        }



        private List<EncompassUserDTO> GetEncompassUsersByEnvironment()
        {
            List<EncompassUserDTO> encompassUsers = new List<RMSServiceReference.EncompassUserDTO>();
            bool isManager = (string)Session[RMSConstants.IS_MANAGER] == RMSConstants.YES;
            bool isAdmin = (string)Session[RMSConstants.IS_ADMIN] == RMSConstants.YES;
            if (isManager && !isAdmin)
            {
                using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    string manager = Session[RMSConstants.USERNAME].ToString();
                    encompassUsers = client.GetEncompassUsersByManager(manager);
                    encompassUsers = encompassUsers.FindAll(eu=>eu.EncompassUserIsActive==true && eu.EncompassUserIsDelete==false).OrderBy(usr => usr.EncompassUserName).ToList();
                }
            }
            else
            {
                encompassUsers = GetAllRMSEncompassUsers();
            }
            return encompassUsers;
        }

        private List<EncompassUserDTO> GetAllRMSEncompassUsers()
        {
            List<EncompassUserDTO> AllEncompassUsers = new List<EncompassUserDTO>();
            using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
            {
                var users = client.GetAllRMSEncompassUsers();
                AllEncompassUsers = (from usr in users
                                     where usr.EncompassUserIsDelete == false && usr.EncompassUserIsActive == true
                                     orderby usr.EncompassUserName
                                     select usr).ToList();
            }
            return AllEncompassUsers;
        }
    }
}