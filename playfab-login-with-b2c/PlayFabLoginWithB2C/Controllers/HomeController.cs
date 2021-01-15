using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlayFabLoginWithB2C.Models;
using PlayFabLoginWithB2C.Services;

namespace PlayFabLoginWithB2C.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPlayFabService playFabService;

        public HomeController(ILogger<HomeController> logger, IPlayFabService playFabService)
        {
            this.playFabService = playFabService;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userData = new UserData
                {
                    PlayFabId = User.Claims.FirstOrDefault(c => c.Type == PlayFabClaims.PlayFabId)?.Value
                };
                return View(userData);
            }
            else
            {
                return View();
            }
        }

        [Authorize]
        public async Task<IActionResult> GetUserData()
        {
            var userData = await playFabService.GetUserDataWithClaims(User.Claims.FirstOrDefault(c => c.Type == PlayFabClaims.PlayFabId)?.Value);

            return View(userData);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
