using AutoMapper;
using FGMC.RMS.Web.Constants;
using FGMC.RMS.Web.Models;
using FGMC.RMS.Web.RMSServiceReference;
using FGMC.SecurityLibrary;
using Log4NetLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FGMC.RMS.Web.ViewModel;

namespace FGMC.RMS.Web.Controllers
{
    public class ReportsController : Controller
    {
        ILoggingService _logger = new DatabaseLoggingService(typeof(ReportsController));
        LogModel _logModel = new LogModel();
        StringCipher stringCipher = new StringCipher();

        // GET: Reports
        public ActionResult Index()
        {
            Reports model = new Models.Reports();
            return View(model);
        }

        private List<ADUserDTO> GetADUsersForDropDown()
        {
            List<ADUserDTO> ADUsers = new List<ADUserDTO>();
            try
            {
                bool isManager = (string) Session[RMSConstants.IS_MANAGER] == RMSConstants.YES;
                bool isAdmin = (string) Session[RMSConstants.IS_ADMIN] == RMSConstants.YES;
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
                LogHelper.Error(_logger, _logModel, 4, ex.Message, string.Empty, ex);
            }
            return ADUsers;
        }


        private List<string> GetADManagersForDropDown()
        {
            List<string> ADManagers = new List<string>();
            try
            {
                bool isManager = (string) Session[RMSConstants.IS_MANAGER] == RMSConstants.YES;
                bool isAdmin = (string) Session[RMSConstants.IS_ADMIN] == RMSConstants.YES;
                if (isAdmin)
                {
                    using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                    {
                        ADManagers = client.GetAllADManagers();
                    }
                }
                else
                {
                    using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                    {
                        string manager = Session[RMSConstants.USERNAME].ToString();
                        ADManagers = client.GetAllADManagers();

                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, string.Empty, ex);
            }
            return ADManagers;
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

        public ActionResult PopulateUserDropdown()
        {
            List<ReportUser> users = new List<ReportUser>();
            try
            {
                string reportType = Request.Form[RMSConstants.SELECT_REPORT_TYPE].ToString();

                if (reportType == RMSConstants.REPORTTYPE_ACTIVEDIRECTORY)
                {
                    var ADUsers = GetADUsersForDropDown();
                    foreach (var item in ADUsers)
                    {
                        users.Add(new ReportUser(item.ADUserID, item.ADUserName));
                    }
                }
                else if (reportType == RMSConstants.REPORTTYPE_ENCOMPASS)
                {
                    var encompassUsers = GetEncompassUsers();
                    foreach (var item in encompassUsers)
                    {
                        users.Add(new ReportUser(item.EncompassRMSUserID, item.EncompassUserName));
                    }
                }
                var json = JsonConvert.SerializeObject(users);
                return Json(new {msg = string.Empty, users = json, returnurl = string.Empty}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, string.Empty, ex);
            }
            return Json(new {msg = Constants.RMSConstants.TECHNICAL_ERROR, users = string.Empty, returnurl = string.Empty}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PopulateUserDropdownForManager()
        {
            List<ReportUser> users = new List<ReportUser>();
            try
            {
                string managerreportType = Request.Form[RMSConstants.MANAGER_REPORTTYPE].ToString();

                if (managerreportType == RMSConstants.REPORTTYPE_ACTIVEDIRECTORY)
                {
                    var ADUsers = GetADManagersForDropDown();
                    foreach (var item in ADUsers)
                    {
                        users.Add(new ReportUser(item.ToString(), item.ToString()));
                    }
                }

                else if (managerreportType == RMSConstants.REPORTTYPE_ENCOMPASS)
                {
                    var encompassUsers = GetADManagersForDropDown();
                    foreach (var item in encompassUsers)
                    {
                        users.Add(new ReportUser(item.ToString(), item.ToString()));
                    }
                }

                var json = JsonConvert.SerializeObject(users);
                return Json(new {msg = string.Empty, users = json, returnurl = string.Empty}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, string.Empty, ex);
            }
            return Json(new {msg = Constants.RMSConstants.TECHNICAL_ERROR, users = string.Empty, returnurl = string.Empty}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GenerateReport(ReportSelectionVM reportSelectionVm)
        {
            Reports model = new Reports();

            try
            {
                if (reportSelectionVm.ReportType == "Active Directory - Manager" || reportSelectionVm.ReportType == "Encompass - Manager")
                {
                    if (!string.IsNullOrEmpty(reportSelectionVm.SelectedManager))
                    {
                        if (!CheckExistingManager(reportSelectionVm.SelectedManager))
                        {
                            return Json(new {ErrorMessage = RMSConstants.USER_NOT_EXISTS}, JsonRequestBehavior.AllowGet);
                        }
                    }
                    if (reportSelectionVm.ReportType == RMSConstants.REPORTTYPE_ACTIVEDIRECTORY_MANAGER)
                    {
                        ADManagerReportVM adManagerReportVm = GenerateAdManagerReport(reportSelectionVm);
                        return PartialView(RMSConstants.ADMANAGER_PARTIALGRID, adManagerReportVm);
                    }
                    else if (reportSelectionVm.ReportType == RMSConstants.REPORTTYPE_ENCOMPASS_MANAGER)
                    {
                        var result = GenerateEncompassReportForManager(reportSelectionVm);
                        return PartialView(RMSConstants.ENCOMPASSMANAGER_PARTIALGRID, result);
                    }
                }
                else
                {
                    List<long> userIDs = new List<long>();

                    if (reportSelectionVm.RequestForUsers!=null && reportSelectionVm.RequestForUsers.Length>0)
                    {
                        var allIDs = reportSelectionVm.RequestForUsers.ToList();
                        foreach (var item in allIDs)
                        {
                            userIDs.Add(Convert.ToInt64(item));
                        }
                    }
                    if (reportSelectionVm.ReportType == RMSConstants.REPORTTYPE_ACTIVEDIRECTORY)
                    {
                        using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                        {
                            if (userIDs.Count > 0)
                            {
                                List<ADReportDTO> report = client.GetADReport(userIDs);
                                Mapper.Initialize(cfg => cfg.CreateMap<ADReportDTO, ADReportData>());
                                foreach (var rep in report)
                                {
                                    ADReportData reportEntry = Mapper.Map<ADReportData>(rep);
                                    model.ADReportData.Add(reportEntry);
                                }
                            }
                            if (model.ADReportData.Count == 0)
                            {
                                model.ErrorMessage = RMSConstants.NO_MATCHING_RECORDS;
                            }
                        }
                        model.SelectedReportType = reportSelectionVm.ReportType;
                        model.SelectedUser = reportSelectionVm.SelectUserId == string.Empty ? 0 : Convert.ToInt64(reportSelectionVm.SelectUserId);
                    }
                    else
                    {
                        using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                        {
                            if(userIDs.Count>0)
                            {
                                List<PersonaReportDTO> report = client.GetPersonaReport(userIDs);
                                Mapper.Initialize(cfg => cfg.CreateMap<PersonaReportDTO, EncompassReportData>());
                                foreach (var rep in report)
                                {
                                    EncompassReportData reportEntry = Mapper.Map<EncompassReportData>(rep);
                                    model.EncompassReportData.Add(reportEntry);
                                }
                            }
                            if (model.EncompassReportData.Count == 0)
                            {
                                model.ErrorMessage = RMSConstants.NO_MATCHING_RECORDS;
                            }
                        }
                        model.SelectedReportType = reportSelectionVm.ReportType;
                        model.SelectedUser = reportSelectionVm.SelectUserId == string.Empty ? 0 : Convert.ToInt64(reportSelectionVm.SelectUserId);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, string.Empty, ex);
            }
            return PartialView(RMSConstants.REPORTS_PARTIALGRID, model);
        }

        private EncompassManagerReportDetails GenerateEncompassReportForManager(ReportSelectionVM reportSelectionVm)
        {
            if (reportSelectionVm.SelectedManager != null && reportSelectionVm.SelectedManager.Contains("["))
            {
                reportSelectionVm.SelectedManager = GetUserIdFromFormattedUserName(reportSelectionVm.SelectedManager, "[", "]");
            }
            var result = GenerateEncompassManagerReport(reportSelectionVm);
            result.SelectedUser = reportSelectionVm.SelectedManager;
            if (result.ReportData.Count == 0)
            {
                result.ErrorMessage = RMSConstants.NO_MATCHING_RECORDS;
            }
            return result;
        }


        private List<EncompassUserDTO> GetEncompassUsers()
        {
            List<EncompassUserDTO> encompassUsers = new List<RMSServiceReference.EncompassUserDTO>();
            bool isManager = (string) Session[RMSConstants.IS_MANAGER] == RMSConstants.YES;
            bool isAdmin = (string) Session[RMSConstants.IS_ADMIN] == RMSConstants.YES;
            if (isManager && !isAdmin)
            {
                using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    string manager = Session[RMSConstants.USERNAME].ToString();
                    encompassUsers = client.GetEncompassUsersByManager(manager);
                    encompassUsers = encompassUsers.FindAll(eu => eu.EncompassUserIsActive == true && eu.EncompassUserIsDelete == false).OrderBy(usr => usr.EncompassUserName).ToList();
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

        public ADManagerReportVM GenerateAdManagerReport(ReportSelectionVM reportSelectionVm)
        {
            ADManagerReportVM adManagerReport = new ADManagerReportVM();
            try
            {
                using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    var managerReport = client.GetAdManagerReport(MapReportSelectionVmToReportRequestDto(reportSelectionVm));
                    if (managerReport == null || !managerReport.IsProcessSuccess)
                    {
                        adManagerReport.ErrorMessage = RMSConstants.TECHNICAL_ERROR;
                        return adManagerReport;
                    }
                    if (managerReport.ADManagerReport.Count == 0)
                    {
                        adManagerReport.ErrorMessage = RMSConstants.NO_MATCHING_RECORDS;
                    }
                    adManagerReport.AdManagerReport = managerReport.ADManagerReport;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, string.Empty, ex);
                adManagerReport.ErrorMessage = RMSConstants.TECHNICAL_ERROR;
            }
            return adManagerReport;
        }

        public EncompassManagerReportDetails GenerateEncompassManagerReport(ReportSelectionVM reportSelectionVm)
        {
            EncompassManagerReportDetails encompassManagerReportDetails = new EncompassManagerReportDetails();
            try
            {
                using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    ReportRequestDTO reportRequestDTO = new ReportRequestDTO();
                    reportRequestDTO.RepoteesType = reportSelectionVm.ReporteesType;
                    reportRequestDTO.SeletedUser = reportSelectionVm.SelectedManager;
                    reportRequestDTO.RequestType = reportSelectionVm.ReportType;
                    reportRequestDTO.UserSatus = reportSelectionVm.SelectedStatus;
                    reportRequestDTO.IncludeGenericAccounts = (reportSelectionVm.IncludeGenericAccounts != null && reportSelectionVm.IncludeGenericAccounts.ToUpper() == "TRUE") ? "TRUE" : "FALSE";
                    var result = client.GetEncompassReportForManagerOrAll(reportRequestDTO);
                    if (result == null)
                    {
                        encompassManagerReportDetails.ErrorMessage = RMSConstants.TECHNICAL_ERROR;
                        return encompassManagerReportDetails;
                    }
                    encompassManagerReportDetails.ReportData = ConvertToEncompassReportData(result);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, string.Empty, ex);
                encompassManagerReportDetails.ErrorMessage = RMSConstants.TECHNICAL_ERROR;
            }
          
            return encompassManagerReportDetails;
        }

        public JsonResult BindManagerIdTextBox()
        {
            List<string> managersToBind = new List<string>();
            try
            {
                using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    var adManagers = client.GetManagersInfo();

                    foreach (var item in adManagers)
                    {
                        managersToBind.Add(string.Format("{0} {1}[{2}]", item.ADUserFirstName, item.ADUserLastName, item.ADUserName));
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, string.Empty, ex);
            }
            return Json(new {managersToBind}, JsonRequestBehavior.AllowGet);
        }

        private ReportRequestDTO MapReportSelectionVmToReportRequestDto(ReportSelectionVM reportSelectionVm)
        {
            ReportRequestDTO reportRequestDto = new ReportRequestDTO();
            reportRequestDto.RequestType = reportSelectionVm.ReportType ?? string.Empty;
            reportRequestDto.RepoteesType = reportSelectionVm.ReporteesType ?? string.Empty;
            reportRequestDto.SeletedUser = GetUserIdFromFormattedUserName(reportSelectionVm.SelectedManager, RMSConstants.OPEN_SQUARE_BRACKET, RMSConstants.CLOSE_SQUARE_BRACKET);
            reportRequestDto.UserSatus = reportSelectionVm.SelectedStatus ?? string.Empty;
            return reportRequestDto;
        }

        private string GetUserIdFromFormattedUserName(string formattedUserName, string splitFirstString, string splitSecondString)
        {
            string userId = string.Empty;
            if (!string.IsNullOrEmpty(formattedUserName))
            {
                if (!formattedUserName.Contains(splitFirstString) || !formattedUserName.Contains(splitSecondString))
                {
                    userId = formattedUserName;
                }
                else
                {
                    int pos1 = formattedUserName.IndexOf(splitFirstString, StringComparison.Ordinal) + splitFirstString.Length;
                    int pos2 = formattedUserName.IndexOf(splitSecondString, StringComparison.Ordinal);
                    userId = formattedUserName.Substring(pos1, pos2 - pos1);
                }
                
            }
            return userId;
        }

        private List<EncompassManagerReportData> ConvertToEncompassReportData(List<EncompassReportResponseDTO> result)
        {
            List<EncompassManagerReportData> encompassReportDataList = new List<EncompassManagerReportData>();
            EncompassManagerReportData encompassReportData;
            foreach (var report in result)
            {
                encompassReportData = new EncompassManagerReportData();
                encompassReportData.Account = report.IsUserEnabledInEncompass? "Enabled":"Disabled";
                encompassReportData.FirstName = report.FirstName;
                encompassReportData.LastName = report.LastName;
                encompassReportData.EmployeeId = report.EmployeeId;
                encompassReportData.Login = report.IsAccountLocked == false?RMSConstants.IS_ENABLED:RMSConstants.IS_DISABLED; //Here comparision is with false because it maps to ISAccountLocked property which should be false to say whether Login is Enabled.
                encompassReportData.ManagerName = report.ManagerName;
                encompassReportData.OrganizationGroups = report.OrganizationGroups;
                encompassReportData.Personas = report.Personas;
                encompassReportData.UserEmail = report.Email;
                encompassReportData.UserName = report.EncompassUserName;
                encompassReportDataList.Add(encompassReportData);
            }

            return encompassReportDataList;
        }

        public bool CheckExistingManager(string selectedManager)
        {
            bool isexits = false;
            try
            {
                var managerId = GetUserIdFromFormattedUserName(selectedManager, "[", "]");
                if (!string.IsNullOrEmpty(managerId))
                {
                    using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                    {
                        var adManagers = client.GetManagersInfo();
                        if(adManagers!=null)
                        isexits = adManagers.Exists(o => o.ADUserName.ToLower() == managerId.ToLower());
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            return isexits;
        }
    }
}