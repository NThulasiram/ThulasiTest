using System;
using System.Xml.Linq;
using System.Web.Mvc;
using FGMC.EncompassUtility.Security;
using FGMCPoolUpdater.Constants;
using FGMCPoolUpdater.Models;
using FGMCPoolUpdater.PoolUpdaterServiceReference;
using FGMCPoolUpdater.ViewModels;
using Log4NetLibrary;

namespace FGMCPoolUpdater.Controllers
{
    public class AccountController : Controller
    {

        ILogService _logger = new FileLogService(typeof(AccountController));
        StringCipher stringCipher = new StringCipher();


        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserViewModel user)
        {
            if (ModelState.IsValid)
            {
                var userData = IsValidUser(user);
                if (userData.IsValidUser)
                {
                    try
                    {
                        PoolUpdaterServiceReference.PoolUpdaterServiceClient client = new PoolUpdaterServiceClient();
                        var encompassuser =
                            client.GetEncompassUser(new EncompassUser()
                            {
                                UserId = stringCipher.Encrypt(user.UserId),
                                Server = stringCipher.Encrypt(user.Server),
                                Password = stringCipher.Encrypt(user.Password)
                            });
                        if (encompassuser.AccountLocked)
                        {
                            TempData[PoolUpdaterConstant.ERROR_MESSAGE] = PoolUpdaterConstant.ACCOUNT_LOCKED;
                            _logger.Error(TempData[PoolUpdaterConstant.ERROR_MESSAGE] + " " + user.UserId);
                            return RedirectToAction(PoolUpdaterConstant.INVALID_USER);
                        }

                        if (!encompassuser.AccountEnabled)
                        {
                            TempData[PoolUpdaterConstant.ERROR_MESSAGE] = PoolUpdaterConstant.USER_NOT_ENABLED;
                            _logger.Error(TempData[PoolUpdaterConstant.ERROR_MESSAGE] + " " + user.UserId);
                            return RedirectToAction(PoolUpdaterConstant.INVALID_USER);
                        }

                        if (encompassuser.UserId == user.UserId)
                        {
                            Session[PoolUpdaterConstant.CURRENTLY_LOGGEDIN_USER] = user.UserId;
                            Session[PoolUpdaterConstant.PASSWORD] = user.Password;
                            Session[PoolUpdaterConstant.SERVER] = user.Server;
                            Session[PoolUpdaterConstant.ISADMINUSER] = userData.IsAdminUser;

                            _logger.Info(PoolUpdaterConstant.USER_LOGGEDINTOAPP +
                                         Session[PoolUpdaterConstant.CURRENTLY_LOGGEDIN_USER]);
                            return RedirectToAction(PoolUpdaterConstant.IMPORTPOOLDATA, PoolUpdaterConstant.POOLUPDATER);
                        }
                    }

                    catch (Exception ex)
                    {
                        if (ex.Message.Contains(PoolUpdaterConstant.USERNOTFOUND))
                        {
                            TempData[PoolUpdaterConstant.ERROR_MESSAGE] = PoolUpdaterConstant.INCORRECT_USERID;
                        }
                        else if (ex.Message.Contains(PoolUpdaterConstant.INVALID_PASSWORD))
                        {
                            TempData[PoolUpdaterConstant.ERROR_MESSAGE] = PoolUpdaterConstant.INCORRECT_PASSWORD;
                        }
                        else
                        {
                            TempData[PoolUpdaterConstant.ERROR_MESSAGE] = ex.Message;
                        }

                        _logger.Error(ex.Message);
                        return RedirectToAction(PoolUpdaterConstant.INVALID_USER);
                    }
                }

                TempData[PoolUpdaterConstant.ERROR_MESSAGE] = PoolUpdaterConstant.USER_NOTAUTHORIZED;
                return RedirectToAction(PoolUpdaterConstant.INVALID_USER);
            }

            return View();
        }

        public ActionResult InvalidUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Logout()
        {
            _logger.Info(PoolUpdaterConstant.USER_LOGGEDOUT + Session[PoolUpdaterConstant.CURRENTLY_LOGGEDIN_USER]);
            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();
            var redirectUrl = new UrlHelper(Request.RequestContext).Action(PoolUpdaterConstant.LOGIN, PoolUpdaterConstant.ACCOUNT);
            return Json(new { Url = redirectUrl });
        }

        private UserData IsValidUser(UserViewModel user)
        {
            UserData userData = new UserData();
            PoolUpdaterServiceReference.PoolUpdaterServiceClient client = new PoolUpdaterServiceClient();
            var users = client.GetUsers();

            foreach (EncompassUser encompassUser in users)
            {
                if (encompassUser.UserId.Trim() == user.UserId)
                {
                    userData.IsValidUser = true;
                    userData.IsAdminUser = encompassUser.UserIsAdmin;
                    break;
                }
            }

            return userData;
        }
    }
}
