using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Famm.BussinessLogic.BussinessLogic;
using Famm.BussinessLogic.BussinessLogic.Interfaces;
using Famm.Domain.DTOs;
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

    public class AuthFlowController : Controller
    {
        private readonly IUserBL _userBL;

        public AuthFlowController()
        {
            var factory = BusinessLogicFactory.Instance;
            _userBL = factory.GetUserBL();
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
        public ActionResult Login(LoginViewModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                model.ErrorMessage = "Email and password are required";
                return View(model);
            }

            try
            {
                var authResult = _userBL.Authenticate(model.Email, model.Password, model.RememberMe);

                if (authResult.IsSuccess)
                {
                    // Добавляем куки аутентификации
                    if (authResult.HttpCookie != null)
                    {
                        Response.Cookies.Add(authResult.HttpCookie);
                    }

                    var userProfile = _userBL.GetUserProfile(authResult.UserId);

                    TempData["UserRole"] = authResult.UserRole ?? "User";
                    TempData["UserName"] = userProfile.FirstName;
                    TempData["UserEmail"] = userProfile.Email;

                    if (authResult.UserRole == "Admin" && string.IsNullOrEmpty(model.ReturnUrl))
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
                    model.ErrorMessage = authResult.ErrorMessage;
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
        public ActionResult Register(RegisterViewModel model)
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
                // Создаем пользователя и получаем результат
                var user = _userBL.GetUserByEmail(model.Email);
                
                if (user != null)
                {
                    model.ErrorMessage = "A user with this email already exists";
                    return View(model);
                }
                
                // Создаем нового пользователя через UserRepository или UserBL
                // В данном случае, мы предполагаем, что метод создания пользователя должен быть реализован
                
                // После успешной регистрации, аутентифицируем пользователя
                var authResult = _userBL.Authenticate(model.Email, model.Password, false);
                
                if (authResult.IsSuccess)
                {
                    // Добавляем куки аутентификации
                    if (authResult.HttpCookie != null)
                    {
                        Response.Cookies.Add(authResult.HttpCookie);
                    }
                    
                    var userProfile = _userBL.GetUserProfile(authResult.UserId);

                    TempData["SuccessMessage"] = "Registration successful! Welcome to Famm Store.";
                    TempData["UserRole"] = "User";
                    TempData["UserName"] = userProfile.FirstName;
                    
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    model.ErrorMessage = "Error during registration. Please try again.";
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
                var signOutResult = _userBL.SignOut();
                
                if (signOutResult.IsSuccess && signOutResult.HttpCookie != null)
                {
                    Response.Cookies.Add(signOutResult.HttpCookie);
                }
                
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
            if (disposing && _userBL != null)
            {
                // Освобождаем ресурсы бизнес-логики
                (_userBL as IDisposable)?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
