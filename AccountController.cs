using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FGMC.RMS.Web.Constants;
using FGMC.RMS.Web.Models;
using FGMC.RMS.Web.RMSServiceReference;
using FGMC.SecurityLibrary;
using Log4NetLibrary;
using WebGrease.Css.Extensions;

namespace FGMC.RMS.Web.Controllers
{
    public class AccountController : Controller
    {
        ILoggingService _logger = new FileLoggingService(typeof (AccountController));
        StringCipher stringCipher = new StringCipher();
        LogModel _logModel = new LogModel();

        // GET: Account
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel loginModel)
        {
            if (ModelState.IsValid)
            {
                using (RMSServiceReference.FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    try
                    {
                        bool? isUserPresent = client.IfUserIsPresent(stringCipher.Encrypt(loginModel.Username));
                        if (isUserPresent.Equals(false))
                        {
                            ViewBag.ErrorMessage = RMSConstants.UNAUTHORIZED_USER;
                            return View(RMSConstants.LOGIN_VIEW);
                        }
                        if (isUserPresent.Equals(null))
                        {
                            ViewBag.ErrorMessage = RMSConstants.DB_NOTREACHABLE;
                            return View(RMSConstants.LOGIN_VIEW);
                        }

                        bool? result = client.ValidateADCredentials(stringCipher.Encrypt(loginModel.Username), stringCipher.Encrypt(loginModel.Password));
                        if (result.Equals(true))
                        {
                            var userDetails = client.GetLoggedUsersFullName(stringCipher.Encrypt(loginModel.Username), stringCipher.Encrypt(loginModel.Password));
                            Session[RMSConstants.USERNAME] = loginModel.Username;
                            Session[RMSConstants.ADUSERID] = $"{userDetails.ADUserID}";
                            Session[RMSConstants.USER_FIRSTNAME_LASTNAME] = $"{userDetails.ADUserFirstName} {userDetails.ADUserLastName}";
                            Session[RMSConstants.MANAGER] = userDetails.Manager;
                            Session[RMSConstants.ADUSER_EMAIL] = userDetails.ADUserEmailId;
                            Session[RMSConstants.ADUSER_FIRSTNAME] = userDetails.ADUserFirstName;
                            Session[RMSConstants.ADUSER_LASTNAME] = userDetails.ADUserLastName;
                            await GetDirectsReports(loginModel, client);
                        }
                        else if (result.Equals(null))
                        {
                            ViewBag.ErrorMessage = RMSConstants.LDAP_SERVER_UNAVAILABLE;
                            return View(RMSConstants.LOGIN_VIEW);
                        }
                        else if (result.Equals(false))
                        {
                            ViewBag.ErrorMessage = RMSConstants.INVALID_CREDENTIALS;
                            return View(RMSConstants.LOGIN_VIEW);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(_logger, _logModel, 0, "Login()" + ex.Message, "", ex: ex);
                        ViewBag.ErrorMessage = RMSConstants.TECHNICAL_ERROR;
                        return View(RMSConstants.LOGIN_VIEW);
                    }
                }
            }
           return  RedirectToAction(RMSConstants.DASHBOARD, RMSConstants.HOME);
        }

        private async Task GetDirectsReports(LoginModel loginModel, FgmcRMSServiceClient client)
        {
            var reportee = client.GetDirectReports(stringCipher.Encrypt(loginModel.Username));
            if (reportee.Count > 0)
            {
                Session[RMSConstants.IS_MANAGER] = RMSConstants.YES;
                await SetAdminSession(loginModel, client);
            }
            else
            {
                await SetAdminSession(loginModel, client);
            }
        }

        private async Task SetAdminSession(LoginModel loginModel, FgmcRMSServiceClient client)
        {
            
            string roleName = client.GetUserRoleByUserName(stringCipher.Encrypt(loginModel.Username));
            if (!string.IsNullOrEmpty(roleName))
            {
                if (roleName.Equals(RMSConstants.ADMIN, StringComparison.OrdinalIgnoreCase))
                     Session[RMSConstants.IS_ADMIN] = RMSConstants.YES;
            }
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
                this.Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
                this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                this.Response.Cache.SetNoStore();
                redirectUrl = new UrlHelper(Request.RequestContext).Action(RMSConstants.LOGIN_VIEW, RMSConstants.ACCOUNT_CONTROLLER);
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 0, "Logout()" + ex.Message, "", ex: ex);
            }
            return Json(new {Url = redirectUrl});
        }

    }
}