using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Teams.Archiver.Models;
using System.Diagnostics;

namespace Teams.Archiver.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly GraphServiceClient _graphClient;


        public HomeController(ILogger<HomeController> logger,
             GraphServiceClient graphClient)
        {
            _logger = logger;
            _graphClient = graphClient;
        }

        public async Task<IActionResult> Index()
        {
            var model = new List<Team>();

            var allGroups = await _graphClient.Groups
                .Request()
                .Filter("resourceProvisioningOptions/any(a:a%20eq%20'Team')")
                .Select("id")
                .GetAsync();

            foreach (var group in allGroups)
            {
                var team = await _graphClient.Teams[group.Id]
                    .Request()
                    .GetAsync();
                model.Add(team);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveTeam(string teamId,
                                                     bool isArchival)
        {
            if (!isArchival)
            {
                await _graphClient.Teams[teamId]
                    .Unarchive()
                    .Request()
                    .PostAsync();
            }
            else
            {
                await _graphClient.Teams[teamId]
                    .Archive()
                    .Request()
                    .PostAsync();
            }

            return RedirectToAction("Index");
        }


        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}