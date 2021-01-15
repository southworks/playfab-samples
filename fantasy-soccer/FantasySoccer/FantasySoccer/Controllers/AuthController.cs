using System.Threading.Tasks;
using FantasySoccer.Controllers.Attributes;
using FantasySoccer.Models.Authentication;
using FantasySoccer.Services;
using Microsoft.AspNetCore.Mvc;

namespace FantasySoccer.Controllers
{
    public class AuthController: Controller
    {
        private readonly IAuthenticationService authenticationService;
        public AuthController(IAuthenticationService authenticationService)
        {
            this.authenticationService = authenticationService;
        }

        [AnonymousOnly]
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [AnonymousOnly]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignIn signIn)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var response = await authenticationService.LoginWithEmailAsync(signIn.Email, signIn.Password);

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                ViewBag.ErrorMessage = response.ErrorMessage;
                return View();
            }

            return Redirect("/");
        }

        public async Task<IActionResult> SignOut()
        {
            await authenticationService.LogoutAsync();
            return Redirect("/");
        }

        [AnonymousOnly]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [AnonymousOnly]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUpAsync(SignUp signUp)
        {
            // Redirect to SignUp page in case there are form errors (NOTE: this shouldn't happen, as there is
            // a front end validation
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = $"There are {ModelState.ErrorCount} errors in this form.";
                return View("SignUp");
            }

            var result = await authenticationService.RegisterWithEmailAsync(signUp.Email, signUp.Password, signUp.DisplayName);

            // Redirect to SignUp page, showing a possible API error
            if (!string.IsNullOrWhiteSpace(result?.ErrorMessage ?? string.Empty))
            {
                ViewBag.ErrorMessage = $"An error ocurred: {result.ErrorMessage}";
                return View("SignUp");
            }

            return Redirect("/");
        }
    }
}
