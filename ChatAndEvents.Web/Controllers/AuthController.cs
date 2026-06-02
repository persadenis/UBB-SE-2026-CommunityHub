using Microsoft.AspNetCore.Mvc;
using ChatAndEvents.Web.Models;
using ChatAndEvents.Data.ChatData.services;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace ChatAndEvents.Web.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly ChatAndEvents.Data.ChatData.services.IAuthenticationService _authService;

        // Serviciul este injectat automat prin Dependency Injection
        public AuthController(ChatAndEvents.Data.ChatData.services.IAuthenticationService authService)
        {
            _authService = authService;
        }

        // --- Rutele pentru LOGIN ---

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var user = await _authService.LoginAsync(model.Username, model.Password);

                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Username)
                    };
                    var identity = new ClaimsIdentity(claims, "Cookies");
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync("Cookies", principal);
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid credentials");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // --- Rutele pentru FORGOT PASSWORD ---

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _authService.ChangePasswordAsync(model.Email, model.NewPassword);

                // Transmitem mesajul de succes către View
                ViewBag.SuccessMessage = "Password updated successfully!";
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // --- Register routes ---

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _authService.RegisterAsync(
                    model.Username,
                    model.Email,
                    model.Password,
                    model.Phone,
                    model.Birthday,
                    avatarUrl: null);

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Login");
        }
    }
}