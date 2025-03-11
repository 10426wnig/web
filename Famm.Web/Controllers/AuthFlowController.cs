using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Famm.Application.Services;
using Famm.Web.Helpers;

namespace Famm.Web.Controllers
{
    public class LoginViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string ReturnUrl { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class RegisterViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class LoginService
    {
        private readonly AuthenticationService _authService;

        public LoginService(AuthenticationService authService)
        {
            _authService = authService;
        }

        public async Task<UserDTO> ProcessLoginAsync(string email, string password, bool rememberMe)
        {
            var request = new LoginRequest
            {
                Email = email,
                Password = password,
                RememberMe = rememberMe
            };

            var result = await _authService.LoginAsync(request);

            if (!result.Success)
            {
                return null;
            }

            return result.User;
        }
    }

    public class RegistrationService
    {
        private readonly AuthenticationService _authService;

        public RegistrationService(AuthenticationService authService)
        {
            _authService = authService;
        }

        public async Task<UserDTO> ProcessRegistrationAsync(string firstName, string lastName, string email, string password)
        {
            var request = new RegisterRequest
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = password
            };

            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
            {
                return null;
            }

            return result.User;
        }
    }

    public class AuthFlowController : Controller
    {
        private readonly AuthenticationService _authService;
        private readonly LoginService _loginService;
        private readonly RegistrationService _registrationService;

        public AuthFlowController()
        {
            _authService = new AuthenticationService();
            _loginService = new LoginService(_authService);
            _registrationService = new RegistrationService(_authService);
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                model.ErrorMessage = "Email and password are required";
                return View(model);
            }

            try
            {
                var user = await _loginService.ProcessLoginAsync(
                    model.Email, 
                    model.Password, 
                    model.RememberMe);

                if (user != null)
                {
                    var domainUser = _authService.AuthenticateUser(model.Email, model.Password);
                    AuthenticationHelper.SignIn(domainUser, model.RememberMe);

                    TempData["UserRole"] = domainUser.Role ?? "User";
                    TempData["UserName"] = user.FirstName;
                    TempData["UserEmail"] = user.Email;

                    if (domainUser.Role == "Admin" && string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return RedirectToAction("Index", "Admin");
                    }

                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                        return Redirect(model.ReturnUrl);
                    else
                        return RedirectToAction("Index", "Home");
                }
                else
                {
                    model.ErrorMessage = "Invalid email or password";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                model.ErrorMessage = "An error occurred during login: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password) ||
                string.IsNullOrEmpty(model.FirstName) || string.IsNullOrEmpty(model.LastName))
            {
                model.ErrorMessage = "All fields are required";
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                model.ErrorMessage = "Passwords do not match";
                return View(model);
            }

            try
            {
                var user = await _registrationService.ProcessRegistrationAsync(
                    model.FirstName,
                    model.LastName,
                    model.Email,
                    model.Password);

                if (user != null)
                {
                    var domainUser = _authService.AuthenticateUser(model.Email, model.Password);
                    AuthenticationHelper.SignIn(domainUser);

                    TempData["SuccessMessage"] = "Registration successful! Welcome to Famm Store.";
                    TempData["UserRole"] = "User";
                    TempData["UserName"] = user.FirstName;
                    
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    model.ErrorMessage = "A user with this email already exists";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                model.ErrorMessage = "An error occurred during registration: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            try
            {
                AuthenticationHelper.SignOut();
                TempData.Clear();
                Session.Clear();
                
                return RedirectToAction("Login");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _authService.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
