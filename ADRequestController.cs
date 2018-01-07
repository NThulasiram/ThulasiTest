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

namespace FGMC.RMS.Web.Controllers
{
    public class ADRequestController : Controller
    {
        List<SelectListItem> AllBranches = new List<SelectListItem>();
        ConfigManager configManager = new ConfigManager();
        ILoggingService _logger = new DatabaseLoggingService(typeof(ADRequestController));
        LogModel _logModel = new LogModel();
        List<ADUserDTO> AllADUsers = new List<ADUserDTO>();

        // GET: ADRequest
        [CustomAuthorize]
        public ActionResult Index()
        {
            ADRequestVM viewModel = new ADRequestVM();
            try
            {
                viewModel.Branches = GetAllBranches();
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            return View(viewModel);
        }

        public List<SelectListItem> GetAllBranches()
        {
            List<string> branches = new List<string>();
            try
            {
                branches = FetchAllBranches();
                foreach (var ua in branches)
                {
                    SelectListItem item = new SelectListItem();
                    item.Text = ua;
                    item.Value = ua;
                    AllBranches.Add(item);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            return AllBranches;
        }

        [HttpPost]
        public JsonResult CheckADUserNameExists()
        {
            string firstName = Request.Form["firstname"].ToString();
            string lastName = Request.Form["lastname"].ToString();
            string msg = "User already exists in Active Directory with same name. It will create a new user with user name {0} in Active Directory.";
            string newUserName = string.Empty;
            try
            {
                bool exists = IsUserAlreadyExist(firstName, lastName);
                if (exists)
                {
                    newUserName = GetUserName(firstName, lastName);
                    if(!string.IsNullOrEmpty(newUserName))
                    {
                        msg = string.Format(msg, newUserName);
                    }
                    else
                    {
                        msg = "User name suggestion has been exhausted since multiple users exist with same name. User name will be provided by manage engine technician.";
                    }
                    return Json(new { msg = msg, returnurl = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { msg = "", returnurl = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            return Json(new { msg = "", returnurl = "" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult NewUserADRequest()
        {

            NewHireMETicketDTO newHireMETicketDTO = new NewHireMETicketDTO();
            bool isSuccess;
            try
            {
                var firstName = Request.Form["firstname"].ToString();
                var lastname = Request.Form["lastname"].ToString();
                var mimicfrom = Request.Form["mimicfrom"].ToString();
                var title = Request.Form["title"].ToString();
                var branch = Request.Form["branch"].ToString();
                var manager = Request.Form["manager"].ToString();
                var dept = Request.Form["dept"].ToString();
                var startDate = Request.Form["startDate"].ToString();
                var comment = Request.Form["comment"].ToString();
                var formDataValid = ValidateFormForAddRequest(firstName, lastname, title, branch, manager, dept, startDate);
                if (!formDataValid)
                {
                    return Json(new { msg = Constants.RMSConstants.AD_USER_MANDATORY_MSG, returnurl = "" }, JsonRequestBehavior.AllowGet);
                }
                newHireMETicketDTO.ADUserFirstName = firstName;
                newHireMETicketDTO.ADUserLastName = lastname;
                newHireMETicketDTO.MimicFrom = mimicfrom;
                newHireMETicketDTO.Title = title;
                newHireMETicketDTO.Branch = branch;
                newHireMETicketDTO.Manager = manager;
                newHireMETicketDTO.Department = dept;
                newHireMETicketDTO.StartDate = DateTime.Parse(startDate);
                newHireMETicketDTO.RequestedBy = Session[RMSConstants.USER_FIRSTNAME_LASTNAME].ToString();
                newHireMETicketDTO.RequestedByUserName = Session[RMSConstants.USERNAME].ToString();
                newHireMETicketDTO.Subject = configManager.GetConfigValue("NewHireSubject", 4);
                newHireMETicketDTO.RequestTemplate = configManager.GetConfigValue("NewHireTemplate", 4);
                newHireMETicketDTO.Group = configManager.GetConfigValue("NewHireGroup", 4);
                newHireMETicketDTO.Status = "Open";
                newHireMETicketDTO.ADUserName = GetUserName(firstName, lastname);
                newHireMETicketDTO.Comment = comment;
                newHireMETicketDTO.Description = CreateTicketDesc(newHireMETicketDTO);


                using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    isSuccess = client.CreateManageEngineTicketNewHire(newHireMETicketDTO);
                }
                //based on success display error message to user
                if (isSuccess)
                {
                    return Json(new { msg = Constants.RMSConstants.AD_REQUEST_ADD_SUCCESS, returnurl = "/ADRequest/Index" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { msg = Constants.RMSConstants.TECHNICAL_ERROR, returnurl = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            return Json(new { msg = Constants.RMSConstants.TECHNICAL_ERROR, returnurl = "" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EditUserADRequest()
        {
            try
            {
                var requestForUser = Request.Form["requestForUser"].ToString();
                var branch = Request.Form["branch"].ToString();
                var comment = Request.Form["comment"].ToString();
                var formDataValid = ValidateFormForEditRequest(requestForUser, branch);
                if (!formDataValid)
                {
                    return Json(new { msg = Constants.RMSConstants.AD_USER_MANDATORY_MSG, returnurl = "" }, JsonRequestBehavior.AllowGet);
                }

                bool isSuccess = false;
                NewHireMETicketDTO newHireMETicketDTO = new NewHireMETicketDTO();
                //Validate the UserName.
                bool isUserValid = ValidateUserNameFromFormatedUserName(requestForUser);
                if(!isUserValid)
                {
                    return Json(new { msg = Constants.RMSConstants.AD_USER_DOES_NOT_EXIST, returnurl = "" }, JsonRequestBehavior.AllowGet);
                }
                var userName = GetUserNameFromFormatedUserName(requestForUser);
                if(string.IsNullOrEmpty(userName))
                {
                    return Json(new { msg = Constants.RMSConstants.AD_USER_NOT_CREATED, returnurl = "" }, JsonRequestBehavior.AllowGet);
                }
                newHireMETicketDTO.ADUserName = userName;
                newHireMETicketDTO.Branch = branch;
                newHireMETicketDTO.RequestedBy = Session[RMSConstants.USER_FIRSTNAME_LASTNAME].ToString();
                newHireMETicketDTO.RequestedByUserName = Session[RMSConstants.USERNAME].ToString();
                newHireMETicketDTO.Subject = configManager.GetConfigValue("EditADUserSubject", 4);
                newHireMETicketDTO.Group = configManager.GetConfigValue("NewHireGroup", 4);
                newHireMETicketDTO.Status = "Open";
                newHireMETicketDTO.Comment = comment;
                newHireMETicketDTO.Description = CreateTicketDescEditUser(newHireMETicketDTO);
                using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    isSuccess = client.CreateManageEngineTicketEditADUser(newHireMETicketDTO);
                }
                //based on success display error message to user
                if(isSuccess)
                {
                    return Json(new { msg = Constants.RMSConstants.AD_REQUEST_EDIT_SUCCESS, returnurl = "/ADRequest/Index" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { msg = Constants.RMSConstants.TECHNICAL_ERROR, returnurl = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message,"", ex);
            }
            return Json(new { msg = Constants.RMSConstants.TECHNICAL_ERROR, returnurl = "" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteUserADRequest()
        {
            try
            {
                var requestForUser = Request.Form["requestForUser"].ToString();
                var comment = Request.Form["comment"].ToString();

                var formDataValid = ValidateFormForDeleteRequest(requestForUser);
                if (!formDataValid)
                {
                    return Json(new { msg = Constants.RMSConstants.AD_USER_MANDATORY_MSG, returnurl = "" }, JsonRequestBehavior.AllowGet);
                }
                bool isSuccess = false;
                //Validate the UserName.
                bool isUserValid = ValidateUserNameFromFormatedUserName(requestForUser);
                if (!isUserValid)
                {
                    return Json(new { msg = Constants.RMSConstants.AD_USER_DOES_NOT_EXIST, returnurl = "" }, JsonRequestBehavior.AllowGet);
                }
                NewHireMETicketDTO newHireMETicketDTO = new NewHireMETicketDTO();
                newHireMETicketDTO.ADUserName = GetUserNameFromFormatedUserName(requestForUser);
                newHireMETicketDTO.RequestedBy = Session[RMSConstants.USER_FIRSTNAME_LASTNAME].ToString();
                newHireMETicketDTO.RequestedByUserName = Session[RMSConstants.USERNAME].ToString();
                newHireMETicketDTO.Subject = configManager.GetConfigValue("EditADUserSubject", 4);
                newHireMETicketDTO.Group = configManager.GetConfigValue("NewHireGroup", 4);
                newHireMETicketDTO.Status = "Open";
                newHireMETicketDTO.Comment = comment;
                newHireMETicketDTO.Description = CreateTicketDescDeleteUser(newHireMETicketDTO);

                using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    isSuccess = client.CreateManageEngineTicketDeleteADUser(newHireMETicketDTO);
                }
                if (isSuccess)
                {
                    return Json(new { msg = Constants.RMSConstants.AD_REQUEST_DELETE_SUCCESS, returnurl = "/ADRequest/Index" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { msg = Constants.RMSConstants.TECHNICAL_ERROR, returnurl = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            return Json(new { msg = Constants.RMSConstants.TECHNICAL_ERROR, returnurl = "" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BindUserIdTextBox()
        {
            List<string> usersToBind = new List<string>();
            try
            {
                List<ADUserDTO> AllADUsers = GetAllRMSADUsers();
                if (AllADUsers.Count > 0)
                {
                    AllADUsers = AllADUsers.FindAll(user => user.ADUserIsActive == true && user.ADUserIsDelete == false);
                }
                foreach (var item in AllADUsers)
                {
                    usersToBind.Add(string.Format("{0} {1}[{2}]", item.ADUserFirstName, item.ADUserLastName, item.ADUserName));
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            return Json(new { usersToBind }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SearchUser()
        {

            try
            {
                string groupName = string.Empty;
                var requestForUser = Request.Form["requestForUser"].ToString();
                var formDataValid = ValidateFormForDeleteRequest(requestForUser);
                if (!formDataValid)
                {
                    return Json(new { msg = Constants.RMSConstants.AD_USER_MANDATORY_MSG, returnurl = "", groupname = "" }, JsonRequestBehavior.AllowGet);
                }
                bool isUserValid = ValidateUserNameFromFormatedUserName(requestForUser);
                if (!isUserValid)
                {
                    return Json(new { msg = Constants.RMSConstants.AD_USER_DOES_NOT_EXIST, returnurl = "", groupname = "" }, JsonRequestBehavior.AllowGet);
                }
                var userName = GetUserNameFromFormatedUserName(requestForUser);
                if (string.IsNullOrEmpty(userName))
                {
                    return Json(new { msg = Constants.RMSConstants.AD_USER_NOT_CREATED, returnurl = "" }, JsonRequestBehavior.AllowGet);
                }

                using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    groupName = client.GetUserAccountByUserName(userName);
                }
                if (string.IsNullOrEmpty(groupName))
                {
                    return Json(new { msg = "The user is not assigned any User Account yet.", returnurl = "", groupname = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { msg = "", returnurl = "", groupname = groupName }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            return Json(new { msg = Constants.RMSConstants.TECHNICAL_ERROR, returnurl = "", groupname = "" }, JsonRequestBehavior.AllowGet);
        }

        private List<ADUserDTO> GetAllRMSADUsers()
        {
            if (AllADUsers.Count == 0)
            {
                using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    var users = client.GetAllADUsersFromRMS();
                    AllADUsers = (from usr in users
                                  where usr.ADUserIsDelete == false
                                  select usr).ToList();
                }
            }
            return AllADUsers;
        }

        private string GetUserName(string firstName,string lastname)
        {
            //username format = firstName.lastname
            //if the user is there then the format will be : firstname initial.lastname
            //if that user is there then it will be firstname.lastinitial
            firstName = firstName.ToLower();
            lastname = lastname.ToLower();
            List<ADUserDTO> allUsers = GetAllRMSADUsers();
            var usrName = string.Format("{0}.{1}", firstName, lastname);
            
            if (allUsers.Count>0)
            {
                var found = (from usr in allUsers
                            where usr.ADUserName == usrName
                            select usr).FirstOrDefault();
                if(found==null)
                {
                    return usrName;
                }
                usrName = string.Format("{0}{1}", firstName[0], lastname);
                found = (from usr in allUsers
                             where usr.ADUserName == usrName
                             select usr).FirstOrDefault();
                if (found == null)
                {
                    return usrName;
                }
                usrName = string.Format("{0}.{1}", firstName[0], lastname);
                found = (from usr in allUsers
                         where usr.ADUserName == usrName
                         select usr).FirstOrDefault();
                if (found == null)
                {
                    return usrName;
                }
                return string.Empty;//Username will be generated by Desktop support Team.
            }

            return string.Format("{0}.{1}", firstName, lastname);
        }

        /// <summary>
        /// Returns true if user with same Fname and Lname exists
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastname"></param>
        /// <returns></returns>
        private bool IsUserAlreadyExist(string firstName, string lastname)
        {
            firstName = firstName.ToLower();
            lastname = lastname.ToLower();
            List<ADUserDTO> allUsers = GetAllRMSADUsers();
            if(allUsers.Count==0)
            {
                return false;
            }
            var usrName = string.Format("{0}.{1}", firstName, lastname);
            var found = (from usr in allUsers
                         where usr.ADUserName == usrName
                         select usr).FirstOrDefault();
            return found != null;
        }

        private string CreateTicketDesc(NewHireMETicketDTO newhire)
        {
            string path = HttpContext.Server.MapPath("~/App_Data/NewHireRequestDesc.txt");
            string desc = System.IO.File.ReadAllText(path);
            desc = desc.Replace("[FIRSTNAME]", newhire.ADUserFirstName)
                .Replace("[LASTNAME]", newhire.ADUserLastName)
                .Replace("[USERNAME]", newhire.ADUserName)
                .Replace("[MANAGER]", newhire.Manager)
                .Replace("[MIMICFORM]", newhire.MimicFrom)
                .Replace("[BRANCH]", newhire.Branch)
                .Replace("[DEPARTMENT]", newhire.Department)
                .Replace("[STARTDATE]", newhire.StartDate.ToShortDateString())
                .Replace("[APPROVER]", newhire.Approver)
                .Replace("[COMMENT]", newhire.Comment);
            return desc;
        }

        private string CreateTicketDescEditUser(NewHireMETicketDTO newhire)
        {
            string path = HttpContext.Server.MapPath("~/App_Data/EditUserAccount.txt");
            string desc = System.IO.File.ReadAllText(path);
            desc = desc.Replace("[USERNAME]", newhire.ADUserName)
                .Replace("[BRANCH]", newhire.Branch)
                .Replace("[COMMENT]", newhire.Comment);
            return desc;
        }

        private string CreateTicketDescDeleteUser(NewHireMETicketDTO newhire)
        {
            string path = HttpContext.Server.MapPath("~/App_Data/DeleteUserAccount.txt");
            string desc = System.IO.File.ReadAllText(path);
            desc = desc.Replace("[USERNAME]", newhire.ADUserName)
                .Replace("[COMMENT]", newhire.Comment);
            return desc;
        }

        private string GetUserNameFromFormatedUserName(string formattedText)
        {
            //formattedText -> FirstName LastName[UserName]
            var userName = formattedText.Substring(formattedText.IndexOf('[')).Replace("[", "").Replace("]", "");
            return userName;
        }

        private bool ValidateUserNameFromFormatedUserName(string formattedText)
        {
            List<ADUserDTO> allUsers = GetAllRMSADUsers();
            string userName = string.Empty;
            if (formattedText.Contains("[") && formattedText.Contains("]"))
            {
                //formattedText -> FirstName LastName[UserName]
                userName = formattedText.Substring(formattedText.IndexOf('[')).Replace("[", "").Replace("]", "");
            }
            else
            {
                userName = formattedText;
            }
            var found = (from usr in allUsers
                         where usr.ADUserIsActive == true && usr.ADUserName.ToLower() == userName.ToLower()
                         select usr).FirstOrDefault();
            return found != null;
        }
        private bool ValidateFormForAddRequest(string firstname, string lastname, string title, string branch, string manager, string dept,  string startdate)
        {
            if(string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(lastname) || string.IsNullOrEmpty(title) || string.IsNullOrEmpty(branch)
                || string.IsNullOrEmpty(manager) || string.IsNullOrEmpty(dept) || string.IsNullOrEmpty(startdate))
            {
                return false;
            }
            return true;
        }

        private bool ValidateFormForEditRequest(string username, string branch)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(branch))
            {
                return false;
            }
            return true;
        }

        private bool ValidateFormForDeleteRequest(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return false;
            }
            return true;
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
    }
}