using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Famm.BussinessLogic.BussinessLogic;
using Famm.BussinessLogic.BussinessLogic.Interfaces;
using Famm.Domain.DTOs;
using Famm.Domain.Models;

namespace Famm.Web.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserBL _userBL;
        private readonly IOrderBL _orderBL;
        
        public UserController()
        {
            var factory = BusinessLogicFactory.Instance;
            _userBL = factory.GetUserBL();
            _orderBL = factory.GetOrderBL();
        }
        
        // GET: User/Profile
        public ActionResult Profile()
        {
            var userEmail = User.Identity.Name;
            var user = _userBL.GetUserByEmail(userEmail);
            
            if (user == null)
            {
                return RedirectToAction("Login", "AuthFlow");
            }
            
            // Можно использовать DTO вместо модели напрямую
            var userProfile = _userBL.GetUserProfile(user.Id);
            
            return View(user); // Или return View(userProfile); если представление обновлено для работы с DTO
        }
        
        // GET: User/Orders
        public ActionResult Orders()
        {
            var userEmail = User.Identity.Name;
            var user = _userBL.GetUserByEmail(userEmail);
            
            if (user == null)
            {
                return RedirectToAction("Login", "AuthFlow");
            }
            
            var orders = _orderBL.GetOrdersByUserId(user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            
            return View(orders);
        }
        
        // GET: User/OrderDetails/5
        public ActionResult OrderDetails(int id)
        {
            var userEmail = User.Identity.Name;
            var user = _userBL.GetUserByEmail(userEmail);
            
            if (user == null)
            {
                return RedirectToAction("Login", "AuthFlow");
            }
            
            var order = _orderBL.GetOrderById(id);
                
            if (order == null || order.UserId != user.Id)
            {
                return HttpNotFound();
            }
            
            return View(order);
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                (_userBL as IDisposable)?.Dispose();
                (_orderBL as IDisposable)?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 