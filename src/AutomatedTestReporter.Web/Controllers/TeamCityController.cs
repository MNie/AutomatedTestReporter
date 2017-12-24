using System.Web.Mvc;
using AutomatedTestReporter.Teamcity.Provider;

namespace AutomatedTestReporter.Controllers
{
    [Route("TeamCity")]
    public class TeamCityController : Controller
    {
        [HttpGet]
        [Route("full")]
        public JsonResult Index()
        {
            var result = Reporter.GetTestResultsFor("project name");
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}