using System;
using System.Web.Mvc;
using FGMCPoolUpdater.Constants;
using FGMCPoolUpdater.PoolUpdaterServiceReference;
using FGMCPoolUpdater.Models;
using FGMCPoolUpdater.ViewModels;

namespace FGMCPoolUpdater.Controllers
{
    public class BaseController : Controller
    {
        protected void RedirectToLoginPage()
        {
            var loginUrl = PoolUpdaterConstant.LOGINURL;
            Session.RemoveAll();
            Session.Clear();
            Session.Abandon();
            HttpContext.Response.Redirect(loginUrl, true);
        }

        protected bool IsValidSession()
        {
            var user = Session[PoolUpdaterConstant.CURRENTLY_LOGGEDIN_USER];
            return !((user == null && !Session.IsNewSession) || Session.IsNewSession);
        }


        protected string ValidSessionWithAdminOnly()
        {
            string user = Convert.ToString(Session[PoolUpdaterConstant.ISADMINUSER]);
            return user;
        }
    }
}