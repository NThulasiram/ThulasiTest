using FGMC.RMS.Web.Constants;
using FGMC.RMS.Web.RMSServiceReference;
using FGMC.RMS.Web.ViewModel;
using FGMC.SecurityLibrary;
using Log4NetLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using static FGMC.RMS.Web.Constants.RMSConstants;
namespace FGMC.RMS.Web.Controllers
{
    public class NewUserITSetupController : Controller
    {
        ConfigManager configManager = new ConfigManager();
        ILoggingService _logger = new DatabaseLoggingService(typeof(NewUserITSetupController));
        LogModel _logModel = new LogModel();
        FgmcRMSServiceClient serviceLient = null;
        List<ListOfValueDTO> _allLOVs = new List<ListOfValueDTO>();

        [CustomAuthorize]
        public ActionResult NewUserITSetup()
        {
            var viewModel = setupPageLoad();
            return View(viewModel);
        }

        [CustomAuthorize]
        [HttpPost]
        public ActionResult CreateNewRequest(NewUserITSetupFormVM vm)
        {
            try
            {
                if (!ValidateMandatoryFields())
                {
                    return Json(new { msg = NEW_USER_IT_REQUEST_MANDATORY_MSG, returnurl = "" }, JsonRequestBehavior.AllowGet);
                }

                //General Information 
                var firstName = Convert.ToString(Request.Form["firstname"]);
                var lastname = Request.Form["lastname"]?.ToString();
                var title = Request.Form["title"]?.ToString();
                var branch = Request.Form["branch"]?.ToString();
                var manager = Request.Form["manager"]?.ToString();
                var dept = Request.Form["dept"]?.ToString();
                var startDate = Request.Form["startDate"].ToString();
                var requester = Request.Form["requester"].ToString();
                var requesteremail = Request.Form["requesteremail"].ToString();
                var isTrainingReqd = Convert.ToBoolean(Request.Form["isTrainingReqd"].ToString());
                var traininglocation = Request.Form["traininglocation"].ToString();
                var trainingdate = Request.Form["trainingdate"].ToString();

                DateTime startDate_Dt = default(DateTime);
                bool res_startDate = DateTime.TryParse(startDate, out startDate_Dt);

                DateTime trainingDate_Dt = default(DateTime);
                bool res_trainingDate = DateTime.TryParse(trainingdate, out trainingDate_Dt);

                //Applications/External Systems
                var selectedExternalSystemsIdList = Request.Form["selectedExternalSystems"].ToString();
                var selectedBanksIdList = Request.Form["selectedBanks"].ToString();
                var selectedstatus = Request.Form["selectedstatus"].ToString();
                var extenalSystemsOther = Request.Form["extenalSystemsOther"].ToString();
                var emailGroups = Request.Form["emailGroups"].ToString();
                var modelE360after = Request.Form["modelE360after"].ToString();

                //Automated Underwriting Systems
                var automatedUnderwritingSystemsIdList = Request.Form["automatedUnderwritingSystems"].ToString();
                var automatedunderwritingsystemsNotes = Request.Form["automatedunderwritingsystemsNotes"].ToString();

                //Computer/Network Resources 
                var compOrNetworkResourcesIdList = Request.Form["compOrNetworkResources"].ToString();
                var printerName = Request.Form["printerName"].ToString();
                var securedSharedDrives = Request.Form["securedSharedDrives"].ToString();
                var isRemoteAccessReqd = Convert.ToBoolean(Request.Form["isRemoteAccessReqd"].ToString());

                //Phone Resources 
                var phoneResourcesIdList = Request.Form["phoneResources"].ToString();

                //Misc. Requirements/Notes 
                var isOffSiteEmp = Convert.ToBoolean(Request.Form["isOffSiteEmp"].ToString());
                var streetAddress = Request.Form["streetAddress"].ToString();
                var addressLine2 = Request.Form["addressLine2"].ToString();
                var city = Request.Form["city"].ToString();
                var state = Request.Form["state"].ToString();
                var zipcode = Request.Form["zipcode"].ToString();
                var country = Request.Form["country"].ToString();
                var miscRequirementsNotes = Request.Form["miscRequirementsNotes"].ToString();

                // TODO: Validation goes here

                CacheAllLOV();
                ConfigManager configManager = new ConfigManager();
                var generalInfo = new GeneralInformation { FirstName = firstName, LastName = lastname, Title = title, Branch = branch, Department = dept, Manager = manager, StartDate = res_startDate ? startDate_Dt : (DateTime?)null, Requester = requester, RequesterEmail = requesteremail, WillReceiveFgmcTraining = isTrainingReqd, TrainingLocation = traininglocation, TrainingDate = res_trainingDate ? trainingDate_Dt : (DateTime?)null };
                var externalSystem = new ExternalSystems { ExternalSystemList = GetListOfValuesFromControlID(selectedExternalSystemsIdList), InvestorPortals = GetListOfValuesFromControlID(selectedBanksIdList), Milestone = GetValueFromControlID(selectedstatus), EmailGroups = emailGroups, OtherApplicationsExternalSystems = extenalSystemsOther, ModelE360After = modelE360after };
                var automatedUnderwritingSystem = new AutomatedUnderwritingSystems { Applications = GetListOfValuesFromControlID(automatedUnderwritingSystemsIdList), Notes = automatedunderwritingsystemsNotes };
                var computerOrNetworkResources = new ComputerOrNetworkResources { NetworkResources = GetListOfValuesFromControlID(compOrNetworkResourcesIdList), PrinterName = printerName, SecuredSharedDrives = securedSharedDrives, RemoteAccess = isRemoteAccessReqd };
                var phoneResources = new PhoneResources { PhoneResourceList = GetListOfValuesFromControlID(phoneResourcesIdList) };
                var shippingAddress = new ShippingAddress { StreetAddress = streetAddress, AddressLine2 = addressLine2, City = city, StateProvinceRegion = state, Country = country, PostalOrZipCode = zipcode };
                var miscRequirement = new MiscRequirementAndNote { OffSiteEmployee = isOffSiteEmp, MiscNote = miscRequirementsNotes, ShippingAddress = shippingAddress };

                var newUserDTO = new NewUserITSetupDetailDTO
                {
                    GeneralInformation = generalInfo,
                    ApplicationsOrExtenalSystems = externalSystem,
                    AutomatedUnderwritingSystems = automatedUnderwritingSystem,
                    ComputerOrNetworkResources = computerOrNetworkResources,
                    PhoneResources = phoneResources,
                    MiscRequirementsNotes = miscRequirement,
                    ManageEngineStatus = "Open",
                    Priority = "Medium",
                    RequestedBy = Convert.ToString(Session[USER_FIRSTNAME_LASTNAME]),
                    RequestedByUserName = Convert.ToString(Session[USERNAME]),
                    Subject = configManager.GetConfigValue("NewUserITSetupSubject", 4),
                    Group = configManager.GetConfigValue("NewUserITSetupGroup", 4),
                    RequestTemplate = configManager.GetConfigValue("NewUserITSetupTemplate", 4),
                    ActivityType = 1,
                    RequestType = 4,
                    RequestDate = DateTime.Now,
                    ADUserEmail = Convert.ToString(Session[ADUSER_EMAIL])
                };


                using (serviceLient = new FgmcRMSServiceClient())
                {
                    bool isCreated = serviceLient.CreateNewUserITSetupTicket(newUserDTO);
                    if (isCreated)
                        return Json(new { msg = NEW_USER_IT_REQUEST_SUCCESS, returnurl = "/NewUserITSetup/NewUserITSetup" }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { msg = TECHNICAL_ERROR, returnurl = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            return Json(new { msg = TECHNICAL_ERROR, returnurl = "" }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult BindManagerTextBox()
        {
            List<string> managersToBind = new List<string>();
            try
            {
                using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    var adManagers = client.GetManagersInfo();

                    foreach (var item in adManagers)
                    {
                        managersToBind.Add(string.Format("{0} {1} [{2}]", item.ADUserFirstName, item.ADUserLastName, item.ADUserName));
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            return Json(new { managersToBind }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult BindRequesterTextBox()
        {
            List<string> requestersToBind = new List<string>();
            try
            {
                using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    var users = client.GetAllADUsersFromRMS();

                    foreach (var item in users)
                    {
                        if (item.ADUserIsActive == true && item.ADUserIsDelete == false)
                        {
                            requestersToBind.Add(string.Format("{0} {1} [{2}]", item.ADUserFirstName, item.ADUserLastName, item.ADUserName));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            return Json(new { requestersToBind }, JsonRequestBehavior.AllowGet);
        }

        private List<SelectListItem> GetAllBranches()
        {
            List<SelectListItem> allBranches = new List<SelectListItem>();
            try
            {
                foreach (var ua in FetchAllBranches())
                {
                    SelectListItem item = new SelectListItem();
                    item.Text = ua;
                    item.Value = ua;
                    allBranches.Add(item);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            return allBranches;
        }
        private List<SelectListItem> GetAllCountries()
        {
            List<SelectListItem> allCountries = new List<SelectListItem>();
            try
            {
                foreach (var ua in FetchAllCountries())
                {
                    SelectListItem item = new SelectListItem();
                    item.Text = ua;
                    item.Value = ua;
                    allCountries.Add(item);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            allCountries.First().Selected = true;
            return allCountries;
        }

        private List<string> FetchAllBranches()
        {
            List<string> branches = new List<string>();
            string path = HttpContext.Server.MapPath("~/App_Data/FGMCBranches.xml");
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlNodeList allBranchesNodes = doc.SelectNodes("/Branches/Branch/@name");
            foreach (XmlNode item in allBranchesNodes)
            {
                branches.Add(item.InnerText);
            }

            return branches;
        }

        private List<string> FetchAllCountries()
        {
            List<string> countries = new List<string>();
            string path = HttpContext.Server.MapPath("~/App_Data/countries.txt");
            countries = System.IO.File.ReadAllLines(path).ToList();
            return countries;
        }

        private NewUserITSetupFormVM setupPageLoad()
        {
            NewUserITSetupFormVM viewModel = new NewUserITSetupFormVM();
            using (serviceLient = new FgmcRMSServiceClient())
            {
                viewModel.ApplicationsOrExtenalSystems.AllExternalSystems = serviceLient.GetLOVsByTypeId(EXTERNAL_SYSTEM_ID);
                viewModel.ApplicationsOrExtenalSystems.AllInvestorPortals = serviceLient.GetLOVsByTypeId(INVESTER_PORTALS_ID);
                viewModel.ApplicationsOrExtenalSystems.AllMilestones = serviceLient.GetLOVsByTypeId(MILESTONES_ID);
                viewModel.AutomatedUnderwritingSystems.AllUnderwritingSystems = serviceLient.GetLOVsByTypeId(UNDERWRITING_SYSTEMS_ID);
                viewModel.ComputerOrNetworkResources.AllCompOrNetworkResources = serviceLient.GetLOVsByTypeId(COMPUTER_RESOURCES_ID);
                viewModel.PhoneResources.AllPhoneResources = serviceLient.GetLOVsByTypeId(PHONE_RESOURCES_ID);
            }
            viewModel.Branches = GetAllBranches();
            viewModel.Countries = GetAllCountries();
            viewModel.GeneralInformation.Requester = Convert.ToString($"{Convert.ToString(Session[ADUSER_FIRSTNAME])} {Convert.ToString(Session[ADUSER_LASTNAME])} [{Convert.ToString(Session[USERNAME])}]");
            viewModel.GeneralInformation.RequesterEmail = Convert.ToString(Session[ADUSER_EMAIL]);
            return viewModel;
        }

        private void CacheAllLOV()
        {
            if (_allLOVs.Count == 0)
            {
                using (serviceLient = new FgmcRMSServiceClient())
                {
                    _allLOVs = serviceLient.GetAllLOVs();
                }
            }
        }

        private List<string> GetListOfValuesFromControlID(string controlIDs)
        {
            if (string.IsNullOrEmpty(controlIDs))
                return new List<string>();
            var idList = controlIDs.Split(',').ToList().Select(ctrlId => Convert.ToInt32(ctrlId.Replace("dynamic_", "")));

            var nameList = from val in _allLOVs
                           where idList.Contains(val.Id)
                           select val.Name;
            return nameList.ToList();
        }

        private string GetValueFromControlID(string controlID)
        {
            if (string.IsNullOrEmpty(controlID))
                return string.Empty;
            var id = Convert.ToInt32(controlID.Replace("dynamic_", ""));
            var nameList = from val in _allLOVs
                           where val.Id == id
                           select val.Name;
            return nameList.First();
        }

        /// <summary>
        /// Validates the Form
        /// </summary>
        /// <returns>True-Form is valid, False-Form is invalid</returns>
        private bool ValidateMandatoryFields()
        {
            //General Information 
            var firstName = Request.Form["firstname"].ToString();
            var lastname = Request.Form["lastname"].ToString();
            var title = Request.Form["title"].ToString();
            var branch = Request.Form["branch"].ToString();
            var manager = Request.Form["manager"].ToString();
            var dept = Request.Form["dept"].ToString();
            var startDate = Request.Form["startDate"].ToString();
            var requester = Request.Form["requester"].ToString();
            var requesteremail = Request.Form["requesteremail"].ToString();

            bool isValid = false;
            isValid = !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastname) && !string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(branch) &&
                !string.IsNullOrEmpty(manager) && !string.IsNullOrEmpty(dept) && !string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(requester) &&
                !string.IsNullOrEmpty(requesteremail);

            if (isValid == false)
                return isValid;

            //Training Location is required if Yes
            var isTrainingReqd = Convert.ToBoolean(Request.Form["isTrainingReqd"].ToString());
            var traininglocation = Request.Form["traininglocation"].ToString();
            var trainingdate = Request.Form["trainingdate"].ToString();
            if (isTrainingReqd)
            {
                isValid = !string.IsNullOrEmpty(traininglocation) && !string.IsNullOrEmpty(trainingdate);
                if (isValid == false)
                    return isValid;
            }

            //Applications / External Systems
            var emailGroups = Request.Form["emailGroups"].ToString();
            isValid = !string.IsNullOrEmpty(emailGroups);
            if (isValid == false)
                return isValid;



            return isValid;
        }
    }
}