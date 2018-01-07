using FGMC.LoanLogicsQC.Web.Constants;
using FGMC.LoanLogicsQC.Web.Models;
using FGMC.LoanLogicsQC.Web.ViewModels;
using Log4NetLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using FGMC.LoanLogicsQC.Web.FGMCQCServiceReference;

namespace FGMC.LoanLogicsQC.Web.Controllers
{
    public class ApplicationConfigurationController : Controller
    {
        ILoggingService _logger = new FileLoggingService(typeof(ApplicationConfigurationController));
        LogModel _logModel = new LogModel();
        List<string> sortOrderconfigurationList = new List<string>() { "SizeAscending ", "NameAscending", "CreatedDateAscending", "SizeDescending", "NameDescending", "CreatedDateDescending" };

        // GET: ApplicationConfiguration
        public ActionResult Configurator()
        {
            var allConfigs = new List<ConfigurationModel>();
            int appId = (int)Session[LoanLogicsQCConstants.APPLICATIONID];
            try
            {
                if (IsAdmin() == null || IsAdmin() == false)
                {
                    RedirectToLoginPage();
                    return RedirectToAction("Login", "Account");
                }

                using (FGMCQCServiceReference.FgmcQCServiceClient client =
                    new FgmcQCServiceClient())
                {

                    var allConfigTypes = client.GetAllConfigurationsTypes();
                    ViewBag.GetConfigurationsTypeName = new SelectList(allConfigTypes,
                        LoanLogicsQCConstants.CONFIGURATION_TYPEID,
                        LoanLogicsQCConstants.CONFIGURATION_TYPENAME);
                }
                allConfigs = GetAllConfigs(appId);
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, ex.Message,ex:ex);
            }
            return View(new ConfigurationViewModel(allConfigs));
        }

        public ActionResult EditConfigurator(int? id)
        {
            ConfigurationViewModel vm = new ConfigurationViewModel();
            try
            {
                ModelState.Clear();
                int appId = (int)Session[LoanLogicsQCConstants.APPLICATIONID];
                using (
                    FGMCQCServiceReference.FgmcQCServiceClient client =
                        new FgmcQCServiceClient())
                {
                    var configs = client.GetAppConfigurations(appId);
                    var configToUpdate = configs.FirstOrDefault(o => o.ConfigurationId == id);
                    if (configToUpdate == null) return new EmptyResult();

                    var configuration = new FGMC.LoanLogicsQC.Web.Models.ConfigurationModel
                    {
                        ConfigurationTypeId = configToUpdate.ConfigurationTypeId,
                        ConfigurationId = (id.HasValue) ? id.Value : 0,
                        ConfigurationName = configToUpdate.ConfigurationName,
                        CreatedByUserId = configToUpdate.CreatedByUserId,
                        ConfigurationValue = configToUpdate.ConfigurationValue
                    };

                    vm = new ConfigurationViewModel {CurrentConfiguration = configuration};
                    var allConfigTypes = client.GetAllConfigurationsTypes();
                    ViewBag.SelectedConfigurationType =
                        new SelectList(
                            allConfigTypes.Where(
                                o => o.ConfigurationTypeId == vm.CurrentConfiguration.ConfigurationTypeId),
                            LoanLogicsQCConstants.CONFIGURATION_TYPEID, LoanLogicsQCConstants.CONFIGURATION_TYPENAME);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, ex.Message,ex:ex);
            }
            return PartialView(LoanLogicsQCConstants._UPDATED_CONFIG, vm.CurrentConfiguration);
        }
        private List<ConfigurationModel> GetAllConfigs(int appId)
        {
            List<ConfigurationModel> allConfigs = new List<ConfigurationModel>();

            try
            {
                ConfigurationModel configurationModel = null;

                using (FGMCQCServiceReference.FgmcQCServiceClient client =
                    new FgmcQCServiceClient())
                {
                    var configDto = client.GetAppConfigurations(appId);

                    foreach (var item in configDto)
                    {
                        configurationModel = new ConfigurationModel();
                        configurationModel.ConfigurationId = item.ConfigurationId;
                        configurationModel.ConfigurationTypeName = item.ConfigurationType.ConfigurationTypeName;
                        configurationModel.ConfigurationName = item.ConfigurationName;
                        configurationModel.ConfigurationValue = item.ConfigurationValue;
                        allConfigs.Add(configurationModel);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, ex.Message,ex:ex);
            }
            return allConfigs;
        }

        [HttpPost]
        public ActionResult AddConfiguration()
        {
            try
            {
                int appId = (int)Session[LoanLogicsQCConstants.APPLICATIONID];
                string configKey = Request.Form[LoanLogicsQCConstants.CONFIGURATION_NAME];
                var keyExsists = GetAllConfigs(appId).Any(o => o.ConfigurationName == configKey);
                if (keyExsists)
                {
                    TempData[LoanLogicsQCConstants.CONFIG_KEY_IS_PRESENT] = LoanLogicsQCConstants.IS_CONFIG_PRESENT;
                    return RedirectToAction(LoanLogicsQCConstants.CONFIGURATOR);
                }
                if (Request.Form[LoanLogicsQCConstants.CONFIGURATION_NAME].Equals(LoanLogicsQCConstants.MERGESORTORDER_CONFIGNAME) && !sortOrderconfigurationList.Contains(Request.Form[LoanLogicsQCConstants.CONFIGURATION_VALUE]))
                {
                    TempData[LoanLogicsQCConstants.MERGESORTORDER_CHECK] = LoanLogicsQCConstants.MERGESORTORDER_MISMATCH;
                    return RedirectToAction(LoanLogicsQCConstants.CONFIGURATOR);
                }
                var newConfigDto = new ConfigurationDto();
                newConfigDto.ConfigurationTypeId = Convert.ToInt32(Request.Form[LoanLogicsQCConstants.CONFIGURATION_TYPENAME]);
                newConfigDto.ConfigurationName = Request.Form[LoanLogicsQCConstants.CONFIGURATION_NAME];
                newConfigDto.ConfigurationValue = Request.Form[LoanLogicsQCConstants.CONFIGURATION_VALUE];
                newConfigDto.CreatedDate = DateTime.Today;
                newConfigDto.CreatedByUserName =
                    Convert.ToString(Session[LoanLogicsQCConstants.CURRENTLY_LOGGEDIN_USER]);
                newConfigDto.CreatedByUserId = Convert.ToInt32(Session[LoanLogicsQCConstants.CREATED_BY_USERID]);
                newConfigDto.ApplicationId = appId;
                newConfigDto.Description = String.Empty;

                using (
                    FgmcQCServiceClient client =
                        new FgmcQCServiceClient())
                {
                    client.AddConfiguration(newConfigDto);

                    TempData[LoanLogicsQCConstants.CONFIG_ADDED] = string.Format(LoanLogicsQCConstants.ADDED_MSG_CONFIG,
                        newConfigDto.ConfigurationName);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, ex.Message,ex:ex);
            }
            return RedirectToAction(LoanLogicsQCConstants.CONFIGURATOR);
        }

        [HttpPost]
        //public ActionResult UpdateConfiguration(ConfigurationModel configurationModel)
        public ActionResult UpdateConfiguration(ConfigurationModel configurationModel)
        {

            MapConfigModelToConfigDto();
            var configurationDto = Mapper.Map<ConfigurationModel, ConfigurationDto>(configurationModel);

            try
            {
                if (configurationDto.ConfigurationName.Equals(LoanLogicsQCConstants.MERGESORTORDER_CONFIGNAME) && !sortOrderconfigurationList.Contains(configurationDto.ConfigurationValue))
                {
                    TempData[LoanLogicsQCConstants.MERGESORTORDER_CHECK] = LoanLogicsQCConstants.MERGESORTORDER_MISMATCH;
                    return RedirectToAction(LoanLogicsQCConstants.CONFIGURATOR);
                }
               // var updateConfigurationValues = new Respository.Configuration();
                configurationDto.ConfigurationId = configurationDto.ConfigurationId;
                //configurationDto.ConfigurationTypeId = Convert.ToInt32(configurationDto.ConfigurationType.ConfigurationTypeName);
                configurationDto.ConfigurationName = configurationDto.ConfigurationName;
                configurationDto.ConfigurationValue = configurationDto.ConfigurationValue;
                configurationDto.ConfigurationTypeName = configurationDto.ConfigurationTypeName;
                configurationDto.CreatedDate = DateTime.Today;
                configurationDto.CreatedByUserName =
                    Convert.ToString(Session[LoanLogicsQCConstants.CURRENTLY_LOGGEDIN_USER]);
                configurationDto.CreatedByUserId = Convert.ToInt32(Session[LoanLogicsQCConstants.CREATED_BY_USERID]);
             
                using (
                    FGMCQCServiceReference.FgmcQCServiceClient client =
                        new FgmcQCServiceClient())
                {
                    client.UpdateConfig(configurationDto);
                }
                //configurationRepository.UpdateConfig(updateConfigurationValues);
                TempData[LoanLogicsQCConstants.CONFIG_UPDATED] = string.Format(LoanLogicsQCConstants.UPDATED_MSG_CONFIG, configurationDto.ConfigurationName);
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, ex.Message,ex:ex);
            }
            return RedirectToAction(LoanLogicsQCConstants.CONFIGURATOR);
        }

        private void MapConfigModelToConfigDto()
        {
           // MapperConfiguration.
            Mapper.CreateMap<ConfigurationModel, ConfigurationDto>()
                .ForMember(dest => dest.ConfigurationId, opt => opt.MapFrom(src => src.ConfigurationId))
                .ForMember(dest => dest.ConfigurationName, opt => opt.MapFrom(src => src.ConfigurationName))
                .ForMember(dest => dest.ConfigurationTypeId, opt => opt.MapFrom(src => src.ConfigurationTypeId))
                .ForMember(dest => dest.ConfigurationValue, opt => opt.MapFrom(src => src.ConfigurationValue))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUserName))
                .ForMember(dest => dest.CreatedByUserId, opt => opt.MapFrom(src => src.CreatedByUserId));
        }

        public ActionResult RemoveConfiguration(string command)
        {
            string message = String.Empty;
            try
            {
                int configId = Convert.ToInt32(command);
                int appId = (int)Session[LoanLogicsQCConstants.APPLICATIONID];
                //var configDto = configurationRepository.GetAppConfigurations(appId);
                using (
                    FGMCQCServiceReference.FgmcQCServiceClient client =
                        new FgmcQCServiceClient())
                {
                    var configDto = client.GetAllConfigurations();
                    var configToDelete = configDto.FirstOrDefault(o => o.ConfigurationId == configId);

                    if (configToDelete != null)
                        client.RemoveConfiguration(configId);

                    message = LoanLogicsQCConstants.USERINFO_REMOVED_MSG;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logger, _logModel, LoanLogicsQCConstants.LOANLOGICQC_APPLICATION_ID, ex.Message,ex:ex);
            }
            return Json(new { message }, JsonRequestBehavior.AllowGet);
        }
        protected bool? IsAdmin()
        {
            bool? user = (bool?)Session[LoanLogicsQCConstants.ID_ADMIN];
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