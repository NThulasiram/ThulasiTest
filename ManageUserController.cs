using FGMC.EncompassUtility.Security;
using FGMCPoolUpdater.Constants;
using FGMCPoolUpdater.PoolUpdaterServiceReference;
using Log4NetLibrary;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace FGMCPoolUpdater.Controllers
{
    public class ManageUserController : BaseController
    {
        StringCipher stringCipher = new StringCipher();
        ILogService _logger = new FileLogService(typeof (ManageUserController));

        [CustomAuthorize]
        public ActionResult UserDetails()
        {
            _logger.Info("UserDetails: Checking for AdminUser.");
            if (!IsAdminUser())
                RedirectToLoginPage();

            EncompassUser encompassUser = new EncompassUser();
            try
            {
                using (PoolUpdaterServiceReference.PoolUpdaterServiceClient client = new PoolUpdaterServiceClient())
                {
                    encompassUser.EncompassUsers = client.GetUsers();
                }

                encompassUser.AccountStatus = string.Empty;
                ViewBag.Userid = string.Empty;
            }
            catch (Exception ex)
            {
                _logger.Error("UserDetails: " + ex.Message + " " + ex.StackTrace);
            }

            return View(encompassUser );
        }

        protected bool IsAdminUser()
        {
            bool IsAdmin;
            string user = Convert.ToString(Session[PoolUpdaterConstant.ISADMINUSER]);
            return IsAdmin = (user == PoolUpdaterConstant.YES) ? true : false;
            //return user user 
        }

        [CustomAuthorize]
        [HttpPost]
        public ActionResult UserDetails(string userid, string command, EncompassUser userrDetails, string searchUserId,  bool IsAdmin = false)
        {
            EncompassUser encompassUser  = new EncompassUser();

            try
            {
                EncompassUser userObj;
             
                if (command == PoolUpdaterConstant.LOOKUP)
                {
                    _logger.Info("UserDetails: Getting Encompass User Details");
                    if (!string.IsNullOrEmpty(userid))
                    {
                        PoolUpdaterServiceClient client = new PoolUpdaterServiceClient();
                        userObj =
                            client.GetEncompassUser(new EncompassUser()
                            {
                                UserId = stringCipher.Encrypt(Session[PoolUpdaterConstant.CURRENTLY_LOGGEDIN_USER].ToString()),
                                Password = stringCipher.Encrypt(Session[PoolUpdaterConstant.PASSWORD].ToString()),
                                Server = stringCipher.Encrypt(Session[PoolUpdaterConstant.SERVER].ToString()),   
                                LookupUserId = userrDetails.UserId, 
                                IsLookupUser = true
                            });                        
                        _logger.Info(PoolUpdaterConstant.SEARCHEDUSER + userid);
                    }
                    else
                    {
                        using (PoolUpdaterServiceReference.PoolUpdaterServiceClient client = new PoolUpdaterServiceClient())
                        {
                            encompassUser .EncompassUsers = client.GetUsers();
                        }                   
                        return View(encompassUser );
                    }
                    ViewBag.Userid = userid;
                    if (userObj == null)
                    {
                        ViewBag.NoUsers = PoolUpdaterConstant.NOUSERS + userid;
                        _logger.Info(PoolUpdaterConstant.SEARCHEDUSER + userid);
                        var LoadEncompassUser = new EncompassUser();
                        using (PoolUpdaterServiceReference.PoolUpdaterServiceClient client = new PoolUpdaterServiceClient())
                        {
                            LoadEncompassUser.EncompassUsers = client.GetUsers();
                        }
                      
                        return View(LoadEncompassUser);
                    }
                    _logger.Info("UserDetails: Set Encompass date UserFields");
                    SetEncompassUserFields(encompassUser , userObj);
                }

                #region  Writing/Reading/Adding of users to XML and Binding to Grid

                if (command == PoolUpdaterConstant.ADDNEW)
                {
                    
                    return View(XmlWriteReadAndAddingUserToGrid(userid, userrDetails, IsAdmin));
                }

                #endregion

                #region  Remove Users from Grid (XML)

                // below piece of code is for removing of users from XML 
                if (!string.IsNullOrEmpty(command) && (command != PoolUpdaterConstant.ADDNEW && command != PoolUpdaterConstant.LOOKUP))
                {
                    _logger.Info("UserDetails: Remove User");
                    EncompassUser RemEncompassUser = RemoveAndReBindingUsers(command);
                    return View(RemEncompassUser);
                }
                #endregion
            }
            catch (Exception ex)
            {
                _logger.Error("UserDetails: " + ex.Message + " " + ex.StackTrace);
            }
            return View(encompassUser );
        }

        private EncompassUser RemoveAndReBindingUsers(string command)
        {
            var encompassUser = new EncompassUser();
            using (PoolUpdaterServiceReference.PoolUpdaterServiceClient client = new PoolUpdaterServiceClient())
            {
                List<EncompassUser> removeUserResponse = client.RemoveUser(stringCipher.Encrypt(command));         // added for Encryption
                encompassUser.EncompassUsers = client.GetUsers();
            }
        
            ViewBag.UserRemoved = PoolUpdaterConstant.USERINFO_REMOVED_MSG;
            _logger.Info(PoolUpdaterConstant.REMOVEDUSER + command);
            return encompassUser;
        }

        private EncompassUser XmlWriteReadAndAddingUserToGrid(string userid, EncompassUser userrDetails, bool IsAdmin)
        {
            List<EncompassUser> usersInXml = null;
            EncompassUser user = new EncompassUser();
            user.UserId = !string.IsNullOrEmpty(userid) ? stringCipher.Encrypt(userid) : string.Empty;           //  // added for encryption
            user.FirstName = !string.IsNullOrEmpty(userrDetails.FirstName) ? stringCipher.Encrypt(userrDetails.FirstName) : string.Empty;    // Enc added     
            user.LastName = !string.IsNullOrEmpty(userrDetails.LastName) ? stringCipher.Encrypt(userrDetails.LastName) : string.Empty;        // enc added  
            user.Email = !string.IsNullOrEmpty(userrDetails.Email) ? stringCipher.Encrypt(userrDetails.Email) : string.Empty;         // enc added
            bool isAdmin = IsAdmin;
            string isAdminstrator = string.Empty;
            user.UserIsAdmin = isAdmin ? stringCipher.Encrypt(PoolUpdaterConstant.YES) : stringCipher.Encrypt(PoolUpdaterConstant.NO);   // enc added 
            _logger.Info("XmlWriteReadAndAddingUserToGrid: Checking for existing User.");
            PoolUpdaterServiceReference.PoolUpdaterServiceClient client = new PoolUpdaterServiceClient();
            var existingUser = client.GetUser(stringCipher.Encrypt(userid)); // added for encryption

            bool userExist = existingUser != null;
            if(!userExist)
            {
                _logger.Info("XmlWriteReadAndAddingUserToGrid: Adding User.");
                usersInXml = client.AddUser(user);
            }
            else
            {
                ViewBag.UserAlreadyPresent = PoolUpdaterConstant.USERINFOTEXT;
                usersInXml = client.GetUsers();
            }

            _logger.Info(string.Format(PoolUpdaterConstant.USERDETAILS, stringCipher.Decrypt(user.UserId), stringCipher.Decrypt(user.FirstName), stringCipher.Decrypt(user.LastName), stringCipher.Decrypt(user.Email), stringCipher.Decrypt(user.UserIsAdmin)));


            var encompassUser = new EncompassUser();         
            encompassUser.EncompassUsers = usersInXml;
            if (!userExist)
                ViewBag.UserAdded = PoolUpdaterConstant.USERINFO_ADDED_MSG;

            return encompassUser;
        }

        private void SetEncompassUserFields(EncompassUser encompassUser, EncompassUser userObj)
        {
            encompassUser.FirstName = !string.IsNullOrEmpty(userObj.FirstName) ? userObj.FirstName : string.Empty;
            encompassUser.LastName = !string.IsNullOrEmpty(userObj.LastName) ? userObj.LastName : string.Empty;
            encompassUser.Email = !string.IsNullOrEmpty(userObj.Email) ? userObj.Email : string.Empty;
            encompassUser.AccountLocked = userObj.AccountLocked;
            encompassUser.AccountEnabled = userObj.AccountEnabled;
             AccountStatus(encompassUser);
            using (PoolUpdaterServiceReference.PoolUpdaterServiceClient client = new PoolUpdaterServiceClient())
            {
                encompassUser.EncompassUsers = client.GetUsers();

            }
        }

        private string AccountStatus(EncompassUser encompassUser)
        {
            if (encompassUser.AccountLocked || !encompassUser.AccountEnabled)
            {
                return encompassUser.AccountStatus = PoolUpdaterConstant.INACTIVE;
            }
            return encompassUser.AccountStatus = PoolUpdaterConstant.ACTIVE;
        }
    }
}