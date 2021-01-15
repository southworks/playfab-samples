using System.Threading.Tasks;
using FantasySoccer.Authorization;
using FantasySoccer.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace FantasySoccer.Controllers
{
    public class UserController: Controller
    {
        private readonly IFantasySoccerService fantasySoccerService;

        public UserController(IFantasySoccerService fantasySoccerService)
        {
            this.fantasySoccerService = fantasySoccerService;
        }

        [CustomAuthorize]
        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "User Transactions";

            var viewModel = await fantasySoccerService.GetUserTransactionsAsync();

            return View(viewModel);
        }
    }
}
