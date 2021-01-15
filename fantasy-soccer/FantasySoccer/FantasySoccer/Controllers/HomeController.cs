using System.Diagnostics;
using System.Threading.Tasks;
using FantasySoccer.Authorization;
using FantasySoccer.Core.Services;
using FantasySoccer.Models;
using Microsoft.AspNetCore.Mvc;

namespace FantasySoccer.Controllers
{
    public class HomeController: Controller
    {
        private readonly IFantasySoccerService fantasySoccerService;

        public HomeController(IFantasySoccerService fantasySoccerService)
        {
            this.fantasySoccerService = fantasySoccerService;
        }

        [CustomAuthorize]
        public async Task<IActionResult> IndexAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    return View(await fantasySoccerService.GetUserTeamStatisticAsync());
                }
                catch
                {
                    return View();
                }
            }

            return Redirect("/Auth/SignIn");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
