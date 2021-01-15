using System;
using System.Threading.Tasks;
using FantasySoccer.Authorization;
using FantasySoccer.Core.Services;
using FantasySoccer.Models.Responses;
using FantasySoccer.Schema.Models.PlayFab;
using Microsoft.AspNetCore.Mvc;

namespace FantasySoccer.Controllers
{
    public class UserTeamManagementController: Controller
    {
        private readonly IFantasySoccerService fantasySoccerService;

        public UserTeamManagementController(IFantasySoccerService fantasySoccerService)
        {
            this.fantasySoccerService = fantasySoccerService;
        }

        [CustomAuthorize]
        public async Task<IActionResult> Index()
        {
            return View(await fantasySoccerService.GetUserTeamAsync());            
        }

        [HttpPost]
        [CustomAuthorize]
        public async Task<BaseResponseWrapper> UpdateUserTeam(UserTeam userTeam)
        {
            try
            {
                await fantasySoccerService.UpdateUserTeamAsync(userTeam);

                var response = new BaseResponseWrapper
                {
                    StatusCode = Models.Responses.StatusCode.OK,
                    Message = "The user team has been updated successful"
                };

                return response;
            }
            catch (Exception exception)
            {
                var response = new BaseResponseWrapper
                {
                    StatusCode = Models.Responses.StatusCode.BadRequest,
                    Message = exception.Message
                };

                return response;
            }
        }
    }
}
