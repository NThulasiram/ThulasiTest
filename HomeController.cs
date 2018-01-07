using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FGMC.RMS.Web.Constants;
using FGMC.RMS.Web.Models;
using FGMC.RMS.Web.RMSServiceReference;
using Log4NetLibrary;
using System.ServiceModel;

namespace FGMC.RMS.Web.Controllers
{
    public class HomeController : Controller
    {
        ILoggingService _logger = new FileLoggingService(typeof(HomeController));
        LogModel _logModel = new LogModel();
        // GET: Home
        [CustomAuthorize]
        public ActionResult Dashboard()
        {
            DashboardModel dashboardModel = new DashboardModel();
            try
            {
                using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    var aduser = GetADUserDetail();
                    if (aduser == null)
                    {
                        ViewBag.ErrorMessage = RMSConstants.TECHNICAL_ERROR;
                        return RedirectToAction(RMSConstants.LOGIN_VIEW, RMSConstants.ACCOUNT_CONTROLLER);
                    }
                    var requestSummaryInfo = client.GetRequestSummary(aduser);
                    Web.Models.UserProfile userProfile = new Web.Models.UserProfile
                    {
                        Name = Convert.ToString(Session[RMSConstants.USER_FIRSTNAME_LASTNAME]),
                        Manage = Convert.ToString(Session[RMSConstants.MANAGER]),
                        Location = requestSummaryInfo.UserProfile.Location,
                        EncompassPriviledge = requestSummaryInfo.UserProfile.EncompassPrivieges

                    };

                    Web.Models.RequestSummary requestSummary = new Web.Models.RequestSummary
                    {
                        ApprovedRequests = requestSummaryInfo.RequestSummary.ApprovedRequests,
                        AllRequests = requestSummaryInfo.RequestSummary.AllRequests,
                        PendingRequests = requestSummaryInfo.RequestSummary.PendingRequests,
                        DeniedRequests = requestSummaryInfo.RequestSummary.DeniedRequests
                    };
                    dashboardModel.UserProfile = userProfile;
                    dashboardModel.RequestSummary = requestSummary;
                    return View(dashboardModel);
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = RMSConstants.TECHNICAL_ERROR;
                LogHelper.Error(_logger, _logModel, 0, string.Empty, ex: ex);
                return View(dashboardModel);
            }
        }

     
        private ADUserDTO GetADUserDetail()
        {
            ADUserDTO aduserDTO = new ADUserDTO();
            try
            {
                aduserDTO.ADUserID = Convert.ToInt64(Session[RMSConstants.ADUSERID]);
                aduserDTO.ADUserName = Convert.ToString(Session[RMSConstants.USERNAME]);
                aduserDTO.Manager = Convert.ToString(Session[RMSConstants.MANAGER]);
                aduserDTO.ADUserEmailId = Convert.ToString(Session[RMSConstants.ADUSER_EMAIL]);
                aduserDTO.ADUserFirstName = Convert.ToString(Session[RMSConstants.ADUSER_FIRSTNAME]);
                aduserDTO.ADUserLastName = Convert.ToString(Session[RMSConstants.ADUSER_LASTNAME]);
            }
            catch(Exception ex)
            {
                LogHelper.Error(_logger, _logModel, 0, ex.Message, ex: ex);
                return null;
            }
           
            return aduserDTO;
        }
    }
}