using System;
using System.Linq;
using System.Web.Mvc;
using FGMC.LoanLogicsQC.Web.ViewModels;
using FGMC.SecurityLibrary;
using FGMC.LoanLogicsQC.Web.Constants;
using FGMC.LoanLogicsQC.Web.FGMCQCServiceReference;

using System.Web.Configuration;
using Log4NetLibrary;

namespace FGMC.LoanLogicsQC.Web.Controllers
{
    public class AccountController : Controller
    {
        ILoggingService _logger = new FileLoggingService(typeof(AccountController));
        LogModel _logModel = new LogModel();
        StringCipher stringCipher = new StringCipher();
        ConfigManager configManager = new ConfigManager();
        // GET: Account
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserViewModel user)
        {
            int applicationID = 0;
            if (ModelState.IsValid)
            {
                using (
                    FGMCQCServiceReference.FgmcQCServiceClient client =
                        new FgmcQCServiceClient())
                {
                    DevOpsUserDto appUser = null;
                    try
                    {
                        string appName = configManager.GetConfigValue("ApplicationName",
                            LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID);
                        int? appID = client.GetApplicationIDByAppName(appName);
                        if (appID == null || appID ==0)
                        {
                            ViewBag.ErrorMessage = LoanLogicsQCConstants.CONFIGURED_APPNAME_ERROR;

                            LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, LoanLogicsQCConstants.CONFIGURED_APPNAME_ERROR + " " + user.UserId);
                     
                            return View(LoanLogicsQCConstants.LOGIN);
                        }
                        applicationID = Convert.ToInt32(appID);
                        appUser = client.GetUserById(user.UserId, applicationID);
                      
                    }
                    catch (Exception ex)
                    {
                    
                        LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, "Login:" + ex.Message + " " + ex.StackTrace,ex:ex);

                    }

                    if (appUser != null)
                    {
                        try
                        {
                            var encompassuser =
                                client.GetEncompassUser(new EncompassUser()
                                {
                                    UserId = stringCipher.Encrypt(user.UserId),
                                    Server = stringCipher.Encrypt(user.Server),
                                    Password = stringCipher.Encrypt(user.Password)
                                });
                            if (encompassuser.AccountLocked)
                            {
                                ViewBag.ErrorMessage = LoanLogicsQCConstants.ACCOUNT_LOCKED;
                     
                                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, TempData[LoanLogicsQCConstants.ERROR_MESSAGE] + " " + user.UserId);
                                return View(LoanLogicsQCConstants.LOGIN);
                            }

                            if (!encompassuser.AccountEnabled)
                            {
                                ViewBag.ErrorMessage = LoanLogicsQCConstants.USER_NOT_ENABLED;
                           
                                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, TempData[LoanLogicsQCConstants.ERROR_MESSAGE] + " " + user.UserId);
                                return View(LoanLogicsQCConstants.LOGIN);
                            }
                          
                            if (encompassuser.UserId == user.UserId)
                            {
                                Session[LoanLogicsQCConstants.CURRENTLY_LOGGEDIN_USER] = user.UserId;
                                Session[LoanLogicsQCConstants.PASSWORD] = user.Password;
                                Session[LoanLogicsQCConstants.SERVER] = user.Server;
                                Session[LoanLogicsQCConstants.FIRSTNAME] = encompassuser.FirstName;
                                Session[LoanLogicsQCConstants.LASTNAME] = encompassuser.LastName;
                                Session[LoanLogicsQCConstants.APPLICATIONID] = applicationID;
                             
                                // below piece to get userid in DB based on username

                                var users = client.GetAllUsers(applicationID);
                                var dbUserId = (from User in users
                                    where
                                        User.DevOpsUserName ==
                                        Convert.ToString(Session[LoanLogicsQCConstants.CURRENTLY_LOGGEDIN_USER])
                                    select User.DevOpsUserId).FirstOrDefault();

                                var isAdmin = (from dbUser in users
                                    where
                                        dbUser.DevOpsUserName ==
                                        Convert.ToString(Session[LoanLogicsQCConstants.CURRENTLY_LOGGEDIN_USER])
                                    select dbUser.IsAdmin).FirstOrDefault();
                                Session[LoanLogicsQCConstants.ID_ADMIN] = isAdmin;
                                Session[LoanLogicsQCConstants.CREATED_BY_USERID] = dbUserId;
                                return RedirectToAction(LoanLogicsQCConstants.SEARCH_LOANS, LoanLogicsQCConstants.SEARCH_LOANS);
                            }
                        }

                        catch (Exception ex)
                        {
                           
                            LogHelper.Info(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, "Login:" + ex.Message + " " + ex.StackTrace,ex:ex);
                            if (ex.Message.Contains(LoanLogicsQCConstants.USERNOTFOUND))
                            {
                                ViewBag.ErrorMessage =
                                    LoanLogicsQCConstants.INCORRECT_USERID;
                            }
                            else if (ex.Message.Contains(LoanLogicsQCConstants.INVALID_PASSWORD))
                            {
                                ViewBag.ErrorMessage =
                                    LoanLogicsQCConstants.INCORRECT_PASSWORD;
                            }
                            else if (ex.Message.Contains(LoanLogicsQCConstants.USER_DISABLED))
                            {
                                ViewBag.ErrorMessage =
                                    LoanLogicsQCConstants.USER_DISABLED_MESSAGE;
                            }
                            else
                            {
                                ViewBag.ErrorMessage = ex.Message;
                            }

                          
                            LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, ex.Message,ex:ex);

                            return View(LoanLogicsQCConstants.LOGIN);
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = LoanLogicsQCConstants.USER_NOT_FOUND_IN_DB;
                        return View(LoanLogicsQCConstants.LOGIN);
                    }
                }
            }

            return View(LoanLogicsQCConstants.LOGIN);
        }

        [HttpPost]
        public ActionResult Logout()
        {
            var redirectUrl = string.Empty;
            try
            {
                Session.Clear();
                Session.RemoveAll();
                Session.Abandon();
                 redirectUrl = new UrlHelper(Request.RequestContext).Action(LoanLogicsQCConstants.LOGIN,
                    LoanLogicsQCConstants.ACCOUNT);
            }
            catch (Exception ex)
            {
                LogHelper.Info(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, ex.Message,ex:ex);

            }
            return Json(new { Url = redirectUrl });
        }

    }
}