using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FGMC.RMS.Web.Constants;
using FGMC.RMS.Web.RMSServiceReference;
using FGMC.RMS.Web.ViewModel;
using FGMC.SecurityLibrary;
using Log4NetLibrary;


namespace FGMC.RMS.Web.Controllers
{
    public class EncompassRequestController : Controller
    {
        ConfigManager configManager = new ConfigManager();
        ILoggingService _logger = new DatabaseLoggingService(typeof(EncompassRequestController));
        LogModel _logModel = new LogModel();

        // GET: EncompassRequest
        [CustomAuthorize]
        public ActionResult CreateNewRequest()
        {
            EncompassRequestVM viewModel = new EncompassRequestVM();
            try
            {
                viewModel.Environments = GetAllEnvironments();
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            return View(viewModel);
        }

        private List<SelectListItem> GetAllEnvironments()
        {
            List<SelectListItem> allEnvironments = new List<SelectListItem>();

            FgmcRMSServiceClient client = new FgmcRMSServiceClient();
            var environments = client.GetAllEnvironments();
            foreach (var environment in environments)
            {
                SelectListItem item = new SelectListItem();
                item.Text = environment.EnvironmentName;
                item.Value = Convert.ToString(environment.EnvironmentId);
                allEnvironments.Add(item);
            }
            return allEnvironments;
        }

        [HttpPost]
        public ActionResult CreateNewRequest(EncompassRequestDataVM reuqestData)
        {
            try
            {
                var operation = reuqestData.SelectedOperation;
                var encompassRequestDTO = CreateEncompassRequest(reuqestData, operation);
                ManageEngineResponseDTO encompassResponseDTO = null;


                using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    if (operation == "Add")
                    {
                        encompassResponseDTO = client.CreateEncompassAddUserTicket(encompassRequestDTO);
                        if (encompassResponseDTO.IsRequestSuccessful)
                        {
                            return Json(new { msg = Constants.RMSConstants.ENCOMPASS_REQUEST_ADD_SUCCESS, returnurl = "/EncompassRequest/CreateNewRequest" }, JsonRequestBehavior.AllowGet);
                        }
                        return Json(new { msg = Constants.RMSConstants.TECHNICAL_ERROR, returnurl = "" }, JsonRequestBehavior.AllowGet);
                    }

                    else if (operation == "Edit")
                    {
                        encompassResponseDTO = client.CreateEncompassEditUserTicket(encompassRequestDTO);
                        if (encompassResponseDTO.IsRequestSuccessful)
                        {
                            return Json(new { msg = Constants.RMSConstants.ENCOMPASS_REQUEST_EDIT_SUCCESS, returnurl = "/EncompassRequest/CreateNewRequest" }, JsonRequestBehavior.AllowGet);
                        }
                        return Json(new { msg = Constants.RMSConstants.TECHNICAL_ERROR, returnurl = "" }, JsonRequestBehavior.AllowGet);
                    }
                    else if (operation == "Remove")
                    {
                        encompassResponseDTO = client.CreateEncompassRemoveUserTicket(encompassRequestDTO);
                        if (encompassResponseDTO.IsRequestSuccessful)
                        {
                            return Json(new { msg = Constants.RMSConstants.ENCOMPASS_REQUEST_REMOVE_SUCCESS, returnurl = "/EncompassRequest/CreateNewRequest" }, JsonRequestBehavior.AllowGet);
                        }
                        return Json(new { msg = Constants.RMSConstants.TECHNICAL_ERROR, returnurl = "" }, JsonRequestBehavior.AllowGet);
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            return Json(new { msg = Constants.RMSConstants.TECHNICAL_ERROR, returnurl = "" }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public void UpdateStoredLists(EncompassListDataVM listStorage)
        {
            try
            {
                if (listStorage.RequestType == "Edit")
                {
                    UpdateStoredListOnEdit(listStorage);
                }
                else if (listStorage.RequestType == "Add")
                {
                    UpdateStoredListOnAdd(listStorage);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
        }

        private void UpdateStoredListOnEdit(EncompassListDataVM listStorage)
        {
            var editAddedList = TempData["EditAddedList"] as List<string>;
            var editRemovedList = TempData["EditRemovedList"] as List<string>;
            var editOriginalList = Session["EditOriginalList"] as List<string>;

            if (listStorage.StoredListOperationType == "Add")
            {
                if (editAddedList == null || editAddedList.Count == 0)
                {
                    editAddedList = listStorage.AddedList;
                    if (editRemovedList != null)
                    {
                        foreach (var item in editAddedList)
                        {
                            if (editRemovedList.Contains(item))
                                editRemovedList.Remove(item);
                        }
                    }
                }
                else
                {
                    foreach (var item in listStorage.AddedList)
                    {
                        if (!editAddedList.Contains(item))
                        {
                            editAddedList.Add(item);
                            if (editRemovedList != null)
                            {
                                if (editRemovedList.Contains(item))
                                    editRemovedList.Remove(item);
                            }
                        }
                    }
                }
            }

            else if (listStorage.StoredListOperationType == "Remove")
            {
                if (editRemovedList == null || editRemovedList.Count == 0)
                {
                    editRemovedList = listStorage.RemovedList;
                    if (editAddedList != null)
                    {
                        foreach (var item in editRemovedList)
                        {
                            if (editAddedList.Contains(item))
                                editAddedList.Remove(item);
                        }
                    }
                    if (editOriginalList != null && editOriginalList.Count > 0)
                        editRemovedList = editRemovedList.Intersect(editOriginalList).ToList();
                    }
                else
                {
                   
                    foreach (var item in listStorage.RemovedList)
                    {
                        if (!editRemovedList.Contains(item))
                        {
                            editRemovedList.Add(item);
                            if (editAddedList != null)
                            {
                                if (editAddedList.Contains(item))
                                    editAddedList.Remove(item);
                            }
                        }
                    }
                    if (editOriginalList != null && editOriginalList.Count > 0)
                        editRemovedList = editRemovedList.Intersect(editOriginalList).ToList();
                    }
            }
            TempData["EditAddedList"] = editAddedList;
            TempData["EditRemovedList"] = editRemovedList;
        }

        private void UpdateStoredListOnAdd(EncompassListDataVM listStorage)
        {
            var addedList = TempData["AddedList"] as List<string>;
            var removedList = TempData["RemovedList"] as List<string>;
            if (listStorage.StoredListOperationType == "Add")
            {
                if (addedList == null || addedList.Count == 0)
                {
                    addedList = listStorage.AddedList;
                    if (removedList != null)
                    {
                        foreach (var item in addedList)
                        {
                            if (removedList.Contains(item))
                                removedList.Remove(item);
                        }
                    }
                }
                else
                {
                    foreach (var item in listStorage.AddedList)
                    {
                        if (!addedList.Contains(item))
                        {
                            addedList.Add(item);
                            if (removedList != null)
                            {
                                if (removedList.Contains(item))
                                    removedList.Remove(item);
                            }
                        }
                    }
                }
            }

            else if (listStorage.StoredListOperationType == "Remove")
            {
                if (removedList == null || removedList.Count == 0)
                {
                    removedList = listStorage.RemovedList;
                    if (addedList != null)
                    {
                        foreach (var item in removedList)
                        {
                            if (addedList.Contains(item))
                                addedList.Remove(item);
                        }
                    }
                }
                else
                {
                    foreach (var item in listStorage.AddedList)
                    {
                        if (!removedList.Contains(item))
                        {
                            removedList.Add(item);
                            if (addedList != null)
                            {
                                if (addedList.Contains(item))
                                    addedList.Remove(item);
                            }
                        }
                    }
                }
            }
            TempData["AddedList"] = addedList;
            TempData["RemovedList"] = removedList;
        }

        [HttpPost]
        public void ClearStoredAddList()
        {
            TempData["AddedList"] = null;
            TempData["RemovedList"] = null;
        }

        [HttpPost]
        public void ClearStoredEditList()
        {
            TempData["EditAddedList"] = null;
            TempData["EditRemovedList"] = null;
        }

        [HttpPost]
        public void ClearStoredList()
        {
            TempData["AddedList"] = null;
            TempData["RemovedList"] = null;
            TempData["EditAddedList"] = null;
            TempData["EditRemovedList"] = null;
        }

        [HttpPost]
        public JsonResult GetAllPersonas()
        {
            try
            {
                var personas = Session[RMSConstants.ALLPERSONAS] as List<string>;
                if (personas != null)
                    return Json(personas, JsonRequestBehavior.AllowGet);
                FgmcRMSServiceClient client = new FgmcRMSServiceClient();
                var allPersonas = client.GetAllPersonas();
                if (allPersonas != null)
                {
                    Session[RMSConstants.ALLPERSONAS] = allPersonas;
                    return Json(allPersonas, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            return Json(new { msg = Constants.RMSConstants.TECHNICAL_ERROR, returnurl = "" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult FetchAllPersonaByUserId(string userName, string requestType)
        {
            var formattedUser = GetFormattedUser(userName);
            FgmcRMSServiceClient client = new FgmcRMSServiceClient();
            //Check if user exist in Encompass User table first, otherwise throw error
            try
            {
                bool result = client.ValidateEncompassUserId(formattedUser);

                if (result)
                {
                    var personas = client.GetAllPersonaByUserId(formattedUser);
                    if (personas != null)
                    {
                        if (requestType == "Add")
                        {
                            TempData["AddedList"] = personas;
                            TempData["RemovedList"] = null;
                        }

                        if (requestType == "Edit")
                        {
                            Session["EditOriginalList"] = personas;
                        }

                        return Json(personas, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { msg = RMSConstants.NO_PERSONA_FOUND, returnurl = "" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { msg = Constants.RMSConstants.ENCOMPASS_USER_DOESNOT_EXIST, returnurl = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
                return Json(new { msg = ex.Message, returnurl = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        private string GetFormattedUser(string userName)
        {
            string formattedUser = string.Empty;
            if (userName.Contains("[") && userName.Contains("]"))
            {
                //formattedText -> FirstName LastName[UserName]
                formattedUser = userName.Substring(userName.IndexOf('[')).Replace("[", "").Replace("]", "");
            }
            else
            {
                formattedUser = userName;
            }
            return formattedUser;
        }

        public JsonResult CheckEncompassUserNameExists(string firstName, string lastName, string selectedOpearation)
        {
            firstName = firstName.ToLower();
            lastName = lastName.ToLower();
            if(selectedOpearation != "Add")
            {
                return Json(new { msg = "", returnurl = "" }, JsonRequestBehavior.AllowGet);
            }
            string msg = "The user already exists in Encompass. It will create a new user with username {0} in Encompass.";

            string newUserName = string.Empty;
            try
            {
                using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    bool exists = client.CheckEncompassUserNameExists(firstName, lastName);
                    if (exists)
                    {
                        newUserName = client.CreateEncompassUserName(firstName, lastName);
                        msg = string.Format(msg, newUserName);
                        return Json(new { msg = msg, returnurl = "" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { msg = "", returnurl = "" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 4, ex.Message, "", ex);
            }
            return Json(new { msg = "", returnurl = "" }, JsonRequestBehavior.AllowGet);
        }

        private EncompassRequestDTO CreateEncompassRequest(EncompassRequestDataVM encompassRequestDataVM, string operation)
        {
            FGMC.RMS.Web.RMSServiceReference.EncompassRequestDTO encompassRequestDTO = new EncompassRequestDTO();
            var addedList = TempData["AddedList"] as List<string>;
            var editAddedList = TempData["EditAddedList"] as List<string>;
            var editremovedList = TempData["EditRemovedList"] as List<string>;
            //encompassRequestDTO.Environment = encompassRequestDataVM.SelectedEnvironment;
            encompassRequestDTO.Comments = encompassRequestDataVM.Comments;
            encompassRequestDTO.RequestedBy = Convert.ToString(Session[RMSConstants.USER_FIRSTNAME_LASTNAME]);
            encompassRequestDTO.Subject = configManager.GetConfigValue("EncompassSubject", 4);
            encompassRequestDTO.Priority = configManager.GetConfigValue("EncompassPriority", 4);
            encompassRequestDTO.RequestTemplate = configManager.GetConfigValue("EncompassChangeTemplate", 4);
            encompassRequestDTO.ServiceCategory = configManager.GetConfigValue("EncompassServiceCategory", 4);
            encompassRequestDTO.Category = configManager.GetConfigValue("EncompassCategory", 4);
            encompassRequestDTO.Project = configManager.GetConfigValue("EncompassProject", 4);
            if(!string.IsNullOrEmpty(encompassRequestDataVM.UserIdToEditOrRemove))
            encompassRequestDTO.UserId = GetFormattedUser(encompassRequestDataVM.UserIdToEditOrRemove);
            encompassRequestDTO.RequestedByUserName = Convert.ToString(Session[RMSConstants.USERNAME]);
            encompassRequestDTO.Operation = encompassRequestDataVM.SelectedOperation;

            if (operation == "Remove")
            {
                encompassRequestDTO.Description = GetEncompassRemoveReqDescription(encompassRequestDTO);
                return encompassRequestDTO;

            }

            if (operation == "Add")
            {
                encompassRequestDTO.FirstName = encompassRequestDataVM.FirstName;
                encompassRequestDTO.LastName = encompassRequestDataVM.LastName;
                encompassRequestDTO.MimicFrom = encompassRequestDataVM.MimicFrom;
                encompassRequestDTO.EncompassUserName = CreateEncompassUserId(encompassRequestDataVM.FirstName.ToLower(), encompassRequestDataVM.LastName.ToLower());
                encompassRequestDTO.PersonasToAdd = addedList == null ? string.Empty : string.Join(",", addedList);
                encompassRequestDTO.Description = GetEncompassAddReqDescription(encompassRequestDTO);
                encompassRequestDTO.SubCategory = configManager.GetConfigValue("EncompassSubCategoryNewReq", 4);
            }

            else if (operation == "Edit")
            {
                encompassRequestDTO.PersonasToAdd = editAddedList == null ? string.Empty : string.Join(",", editAddedList);
                encompassRequestDTO.PersonasToRemove = editremovedList == null ? string.Empty : string.Join(",", editremovedList);
                encompassRequestDTO.Description = GetEncompassEditReqDescription(encompassRequestDTO);
                encompassRequestDTO.SubCategory = configManager.GetConfigValue("EncompassSubCategoryEditReq", 4);
            }

            return encompassRequestDTO;
        }

        private string GetEncompassAddReqDescription(EncompassRequestDTO encompassRequestDTO)
        {
            string path = HttpContext.Server.MapPath("~/App_Data/EncompassAddRequest.txt");
            string desc = System.IO.File.ReadAllText(path);
            desc = desc.Replace("[FIRSTNAME]", encompassRequestDTO.FirstName)
                .Replace("[LASTNAME]", encompassRequestDTO.LastName)
                .Replace("[USERNAME]", encompassRequestDTO.EncompassUserName)
                .Replace("[MIMICFROM]", encompassRequestDTO.MimicFrom)
                .Replace("[PERSONASTOADD]", encompassRequestDTO.PersonasToAdd)
                .Replace("[COMMENTS]", encompassRequestDTO.Comments);
            return desc;
        }

        private string GetEnvironmentNameById(int environmentId)
        {
            string environmentName = string.Empty;
            using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
            {
                environmentName = client.GetEnvironmentNameById(environmentId);
            }
            return environmentName;
        }

        private string GetEncompassEditReqDescription(EncompassRequestDTO encompassRequestDTO)
        {
            string path = HttpContext.Server.MapPath("~/App_Data/EncompassEditRequest.txt");
            string desc = System.IO.File.ReadAllText(path);
            desc = desc.Replace("[USERID]", encompassRequestDTO.UserId)
                .Replace("[PERSONASTOADD]", encompassRequestDTO.PersonasToAdd)
                .Replace("[PERSONASTOREMOVE]", encompassRequestDTO.PersonasToRemove)
                .Replace("[COMMENTS]", encompassRequestDTO.Comments);
            return desc;
        }

        private string GetEncompassRemoveReqDescription(EncompassRequestDTO encompassRequestDTO)
        {
            string path = HttpContext.Server.MapPath("~/App_Data/EncompassRemoveRequest.txt");
            string desc = System.IO.File.ReadAllText(path);
            desc = desc.Replace("[USERID]", encompassRequestDTO.UserId)
                .Replace("[COMMENTS]", encompassRequestDTO.Comments);
            return desc;
        }

        private string CreateEncompassUserId(string firstName, string lastName)
        {
            string userId = string.Empty;
            using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
            {
                userId = client.CreateEncompassUserName(firstName, lastName);
            }
            return userId;
        }

        [HttpPost]
        public JsonResult BindUserIdTextBox()
        {
            List<EncompassUserDTO> allEncompassUsers = GetAllRMSEncompassUsers();
            if (allEncompassUsers.Count > 0)
            {
                allEncompassUsers = allEncompassUsers.FindAll(user => user.EncompassUserIsActive == true && user.EncompassUserIsDelete == false);
            }
            List<string> usersToBind = new List<string>();
            foreach (var item in allEncompassUsers)
            {
                usersToBind.Add(string.Format("{0} {1}[{2}]", item.EncompassUserFirstName, item.EncompassUserLastName, item.EncompassUserName));
            }
            return Json(new {usersToBind}, JsonRequestBehavior.AllowGet);
        }

        private List<EncompassUserDTO> GetAllRMSEncompassUsers()
            {
            List<EncompassUserDTO> allEncompassUsers = new List<EncompassUserDTO>();
            using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                var users = client.GetAllRMSEncompassUsers();
                allEncompassUsers = (from user in users
                                     where user.EncompassUserIsDelete == false
                                     select user).ToList();
                }
            return allEncompassUsers;
            }
        }
}