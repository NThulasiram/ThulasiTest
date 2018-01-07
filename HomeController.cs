using System.Web.Mvc;
//using AccountLibrary;
//using AdminLibrary;
using AutoMapper;
using FGMCPoolUpdater.Logging;
using FGMCPoolUpdater.ViewModels;


namespace FGMCPoolUpdater.Controllers
{
    public class HomeController : Controller
    {
        //private readonly IAccountLibrary _accountLibrary;
        //private readonly IAdminLibrary _adminLibrary;   
        private readonly ILogger _logger;
        public HomeController(ILogger logger)//IAccountLibrary accountLibrary, IAdminLibrary adminLibrary, ILogger logger)
        {
            //_accountLibrary = accountLibrary;
            //_adminLibrary = adminLibrary;        
            _logger = logger;
        }
        public ActionResult Index()
        {
            _logger.Error("Index action called!");

            return View();
        }

        //public ActionResult Test()
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        var user = _accountLibrary.GetUser();
        //        Mapper.CreateMap<User, UserViewModel>();
        //        var userDto = Mapper.Map<User, UserViewModel>(user);
        //        return View(userDto);
        //    }
        //    return null;
        //}

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}