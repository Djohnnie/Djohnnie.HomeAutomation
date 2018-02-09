using Microsoft.AspNetCore.Mvc;

namespace Djohnnie.HomeAutomation.Web.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            AddPageHeader("Dashboard", "");
            return View();
        }
    }
}