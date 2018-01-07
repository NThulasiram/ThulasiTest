using FGMC.LoanLogicsQC.Web.Constants;
//using FGMC.LoanDocExtractor.Respository;
using FGMC.LoanLogicsQC.Web.FGMCQCServiceReference;
using FGMC.LoanLogicsQC.Web.ViewModels;
using FGMC.SecurityLibrary;
using Log4NetLibrary;
using System;
using System.Collections.Generic;
using System.Web.Configuration;
using System.Web.Mvc;
using EncompassUser = FGMC.LoanLogicsQC.Web.Models.EncompassUser;

namespace FGMC.LoanLogicsQC.Web.Controllers
{
    public class ManageUserController : Controller
    {
        // GET: ManageUser
        StringCipher stringCipher = new StringCipher();
        EncompassUserViewModel userViewModel = new EncompassUserViewModel();
        ILoggingService _logger = new FileLoggingService(typeof(ManageUserController));
        LogModel _logModel = new LogModel();
        public ActionResult UserDetails()

        {
            var allUsers = new List<EncompassUser>();
            try
            {
                if (IsAdminUser() == null || IsAdminUser() == false)
                {
                    RedirectToLoginPage();
                    return RedirectToAction("Login", "Account");
                }

                int appId= (int)Session[LoanLogicsQCConstants.APPLICATIONID];
                 allUsers = GetAllUsers(appId);
            }
            catch (Exception ex)
            {
             
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, ex.Message,ex:ex);
            }
            return View(new EncompassUserViewModel(allUsers));
        }

       private List<EncompassUser> GetAllUsers(int applicationID)
        {
            List<EncompassUser> allUsers = new List<EncompassUser>();
            try
            {
                EncompassUser encompassUser = null;
                using (FGMCQCServiceReference.FgmcQCServiceClient client = new FgmcQCServiceClient())
                {
                    var refEncompassUsersDto = client.GetUsers(applicationID);
                    foreach (var item in refEncompassUsersDto)
                    {
                        encompassUser = new EncompassUser();
                        encompassUser.UserId = item.UserId;
                        encompassUser.FirstName = item.FirstName;
                        encompassUser.LastName = item.LastName;
                        encompassUser.Email = item.Email;
                        encompassUser.IsAdmin = item.IsAdmin;
                        allUsers.Add(encompassUser);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, ex.Message,ex:ex);
            }
            return allUsers;
        }

        [HttpPost]
        public ActionResult UserDetails(string userId,string command, EncompassUser userDetail, bool IsAdmin = false)
        {
          // EncompassUser encompassUser = new EncompassUser();

            EncompassUserViewModel encompassUserVM = new EncompassUserViewModel();
            try
            {
                if (command == LoanLogicsQCConstants.LOOKUP)
                {
                    using (
                        FGMCQCServiceReference.FgmcQCServiceClient client =
                            new FgmcQCServiceClient())
                    {

                        var userObj = client.GetEncompassUser(new FGMCQCServiceReference.EncompassUser()
                        {
                            UserId =
                                stringCipher.Encrypt(
                                    Session[LoanLogicsQCConstants.CURRENTLY_LOGGEDIN_USER].ToString()),
                            Password = stringCipher.Encrypt(Session[LoanLogicsQCConstants.PASSWORD].ToString()),
                            Server = stringCipher.Encrypt(Session[LoanLogicsQCConstants.SERVER].ToString()),
                            LookupUserId = userId,
                            IsLookupUser = true,
                            ApplicationID = Convert.ToInt32(Session[LoanLogicsQCConstants.APPLICATIONID])
                        });
                        if (userObj == null)
                        {
                            //  ViewBag.NoUsers = String.Format(LoanDocExtractorConstants.NO_USER_FOUND, userId);
                            ViewBag.Failure = String.Format(LoanLogicsQCConstants.NO_USER_FOUND, userId);
                            return UserDetails();
                        }
                        SetEncompassUserFields(encompassUserVM, userObj);
                    }
                }

                if (command == LoanLogicsQCConstants.ADDNEW)
                {

                    using(
                        FGMCQCServiceReference.FgmcQCServiceClient client =
                            new FgmcQCServiceClient())
                    {
                        var existingUser = client.GetUser(Convert.ToString(TempData[LoanLogicsQCConstants.LOOKUP_USERID]),Convert.ToInt32(Session[LoanLogicsQCConstants.APPLICATIONID]));

                        bool userExist = existingUser != null;
                        if (!userExist)
                        {
                            FGMCQCServiceReference.EncompassUser userDetailToAdd =
                                new FGMCQCServiceReference.EncompassUser();
                            userDetailToAdd.UserId = userDetail.UserId.Trim();
                            userDetailToAdd.Password = userDetail.Password;
                            userDetailToAdd.Server = userDetail.Server;
                            userDetailToAdd.Email = userDetail.Email;
                            userDetailToAdd.FirstName = userDetail.FirstName;
                            userDetailToAdd.LastName = userDetail.LastName;
                            userDetailToAdd.IsAdmin = IsAdmin;
                            userDetailToAdd.AccountLocked = (bool) TempData[LoanLogicsQCConstants.IS_ACCOUNT_LOCKED];
                            userDetailToAdd.AccountEnabled = (bool) TempData[LoanLogicsQCConstants.IS_ACCOUNT_ENABLED];
                            userDetailToAdd.AccountStatus = Convert.ToString(TempData[LoanLogicsQCConstants.ACCOUNT_STATUS]);
                            userDetailToAdd.ApplicationID = Convert.ToInt32(Session[LoanLogicsQCConstants.APPLICATIONID]);
                            // Corresponds to Application Id {1 is application id Loan Doc Extractor}
                            client.AddUser(userDetailToAdd, (bool) TempData[LoanLogicsQCConstants.IS_ACCOUNT_LOCKED],
                                (bool) TempData[LoanLogicsQCConstants.IS_ACCOUNT_ENABLED],
                                Convert.ToString(Session[LoanLogicsQCConstants.CURRENTLY_LOGGEDIN_USER]),
                                Convert.ToInt32(Session[LoanLogicsQCConstants.CREATED_BY_USERID]));
                            //  ViewBag.UserAdded = LoanDocExtractorConstants.USERINFO_ADDED_MSG;
                            ViewBag.Success = LoanLogicsQCConstants.USERINFO_ADDED_MSG;


                            return UserDetails();

                        }

                        else
                        {
                            // ViewBag.UserAlreadyPresent = LoanDocExtractorConstants.USERINFOTEXT;
                            ViewBag.Failure = LoanLogicsQCConstants.USERINFOTEXT;

                            return UserDetails();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(command) &&
                    (command != LoanLogicsQCConstants.ADDNEW && command != LoanLogicsQCConstants.LOOKUP))
                {
                    using (
                        FGMCQCServiceReference.FgmcQCServiceClient client =
                            new FgmcQCServiceClient())
                    {
                        client.RemoveUser(command, Convert.ToInt32(Session[LoanLogicsQCConstants.APPLICATIONID]));
                    }
                    // ViewBag.UserRemoved = LoanDocExtractorConstants.USERINFO_REMOVED_MSG;
                    ViewBag.Success = LoanLogicsQCConstants.USERINFO_REMOVED_MSG;
                    return UserDetails();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, ex.Message,ex:ex);
            }

             return View(encompassUserVM);
          //  return GetAllUsers();
        }

        private void SetEncompassUserFields(EncompassUserViewModel encompassUserVM, FGMCQCServiceReference.EncompassUser userObj)
        {
            TempData[LoanLogicsQCConstants.LOOKUP_USERID] = encompassUserVM.CurrentEncompassUser.UserId = userObj.UserId;
            encompassUserVM.CurrentEncompassUser.FirstName = !string.IsNullOrEmpty(userObj.FirstName)
                ? userObj.FirstName
                : string.Empty;
            encompassUserVM.CurrentEncompassUser.LastName = !string.IsNullOrEmpty(userObj.LastName)
                ? userObj.LastName
                : string.Empty;
            encompassUserVM.CurrentEncompassUser.Email = !string.IsNullOrEmpty(userObj.Email)
                ? userObj.Email
                : string.Empty;
            TempData[LoanLogicsQCConstants.IS_ACCOUNT_LOCKED] = encompassUserVM.CurrentEncompassUser.AccountLocked = userObj.AccountLocked;
            TempData[LoanLogicsQCConstants.IS_ACCOUNT_ENABLED] = encompassUserVM.CurrentEncompassUser.AccountEnabled = userObj.AccountEnabled;
            TempData[LoanLogicsQCConstants.ACCOUNT_STATUS] = SetAccountStatus(encompassUserVM);
            encompassUserVM.EncompassUsers = GetAllUsers(Convert.ToInt32(Session[LoanLogicsQCConstants.APPLICATIONID]));
            encompassUserVM.CurrentEncompassUser = encompassUserVM.CurrentEncompassUser;
        }
        private string SetAccountStatus(EncompassUserViewModel encompassUser)
        {
            if (encompassUser.CurrentEncompassUser.AccountLocked || !encompassUser.CurrentEncompassUser.AccountEnabled)
            {
                return encompassUser.CurrentEncompassUser.AccountStatus = LoanLogicsQCConstants.INACTIVE;
            }
            return encompassUser.CurrentEncompassUser.AccountStatus = LoanLogicsQCConstants.ACTIVE;
        }

        protected bool? IsAdminUser()
        {
            bool? user = (bool?) Session[LoanLogicsQCConstants.ID_ADMIN];
            return user;
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