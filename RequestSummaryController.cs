using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using FGMC.RMS.Web.Models;
using FGMC.RMS.Web.RMSServiceReference;
using FGMC.RMS.Web.Constants;
using Log4NetLibrary;
using System.ServiceModel;
using FGMC.RMS.Web.ViewModel;

namespace FGMC.RMS.Web.Controllers
{
    public class RequestSummaryController : Controller
    {
        ILoggingService _logger = new FileLoggingService(typeof(HomeController));
        LogModel _logModel = new LogModel();
        // GET: RequestSummary
        [CustomAuthorize]
        public ActionResult RequestSummary()
        {
            RequestDashbordModel requestDashbordModel = new RequestDashbordModel { SelectedRequestType = "Requests" };
            try
            {
                using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    var requestReponse = client.GetRequetsReponse(Convert.ToInt64(Session[RMSConstants.ADUSERID]));
                    requestDashbordModel.RequestTypes = new SelectList(requestReponse.RequestTypes, "RequestTypeId", "RequestTypeName");
                    requestDashbordModel.MeRequestStatus = new SelectList(requestReponse.MeRequestStatus, "MeRequestStatus", "MeRequestStatus");
                    requestDashbordModel.SelectedMeStatus = "Open";
                    foreach (var request in requestReponse.Requests)
                    {
                        var requestDetail = MapRequestDTORequestDeatil(request);
                        requestDashbordModel.RequestDetail.Add(requestDetail);
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = RMSConstants.TECHNICAL_ERROR;
                LogHelper.Error(_logger, _logModel, 0, string.Format("{0} {1}", "RequestSummary()", ex.Message), ex: ex);
                return View(requestDashbordModel);
            }
            return View(requestDashbordModel);
        }

        [HttpPost]
        public ActionResult GetSelectedTypeRequest(RequestVM requestVm)
        {
            RequestDashbordModel requestDashbordModel = new RequestDashbordModel { SelectedRequestType = "Requests" };
            try
            {
                using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    var requestReponse = client.GetRequestsForSataus(Convert.ToInt64(Session[RMSConstants.ADUSERID]), requestVm.MeStatus, requestVm.RequestType);
                    foreach (var request in requestReponse)
                    {
                        var requestDetail = MapRequestDTORequestDeatil(request);
                        requestDashbordModel.RequestDetail.Add(requestDetail);
                    }
                }
            }
            catch (Exception ex)
            {
                requestDashbordModel.ErrorMessage = RMSConstants.TECHNICAL_ERROR;
                LogHelper.Error(_logger, _logModel, 0, string.Format("{0} {1}", "GetSelectedTypeRequest()", ex.Message), ex: ex);
            }

            return PartialView("RequestSummaryGridPartial", requestDashbordModel);
        }

        public ActionResult OpenRequestDetail(long id)
        {
            try
            {
                using (FgmcRMSServiceClient client = new FgmcRMSServiceClient())
                {
                    RequestDTO request = client.GetRequestByRequestId(id);
                    var requestDetail = MapRequestDTORequestDeatil(request);
                    if (String.Compare(requestDetail.RequestType, RMSConstants.NEW_USER_IT_SETUP_REQUEST, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        requestDetail.RequestSummary = requestDetail.RequestSummary.Replace("#", "");
                    }
                    return PartialView("RequestDetailPartial", requestDetail);
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = RMSConstants.TECHNICAL_ERROR;
                LogHelper.Error(_logger, _logModel, 0, string.Format("{0} {1}", "OpenRequestDetail()", ex.Message), ex: ex);
                return PartialView("ErrorDailogView");
            }

        }

        private RequestDetail MapRequestDTORequestDeatil(RequestDTO requestDTO)
        {
            RequestDetail requestDetail = new RequestDetail();
            {
                requestDetail.UserId = requestDTO.UserId;
                requestDetail.StatusDate = requestDTO.StatusDate;
                requestDetail.RequestType = requestDTO.RequestType;
                requestDetail.RequestSummary = requestDTO.RequestSummary;
                requestDetail.RequestStatus = requestDTO.RequestStatus;
                requestDetail.RequestId = requestDTO.RequestId;
                requestDetail.RequestDate = requestDTO.RequestDate;
                requestDetail.RaisedBy = requestDTO.RaisedBy;
                requestDetail.MimicFrom = requestDTO.MimicFrom;
                requestDetail.MEStatus = requestDTO.MEStatus;
                requestDetail.MERequestId = requestDTO.MERequestId;
                requestDetail.IsBulkUpload = requestDTO.IsBulkUpload;
                requestDetail.Environment = requestDTO.Environment;
                requestDetail.Description = requestDTO.Description;
                requestDetail.DeniedDate = requestDTO.DeniedDate;
                requestDetail.DeniedBy = requestDTO.DeniedBy;
                requestDetail.Comments = requestDTO.Comments;
                requestDetail.ClosedDate = requestDTO.ClosedDate;
                requestDetail.ClosedBy_ = requestDTO.ClosedBy_;
                requestDetail.ApprovedDate = requestDTO.ApprovedDate;
                requestDetail.ApprovedBy = requestDTO.ApprovedBy;
                requestDetail.PendingWith = requestDTO.PendingWith;
            }

            return requestDetail;
        }
    }
}