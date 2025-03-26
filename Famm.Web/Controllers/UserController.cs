using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Famm.Database.Context;
using Famm.Domain.Models;
using System.Data.Entity;

namespace Famm.Web.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly FammDbContext _db;
        
        public UserController()
        {
            _db = new FammDbContext();
        }
        
        // GET: User/Profile
        public ActionResult Profile()
        {
            var userEmail = User.Identity.Name;
            var user = _db.Users.FirstOrDefault(u => u.Email == userEmail);
            
            if (user == null)
            {
                return RedirectToAction("Login", "AuthFlow");
            }
            
            return View(user);
        }
        
        // GET: User/Orders
        public ActionResult Orders()
        {
            var userEmail = User.Identity.Name;
            var user = _db.Users.FirstOrDefault(u => u.Email == userEmail);
            
            if (user == null)
            {
                return RedirectToAction("Login", "AuthFlow");
            }
            
            var orders = _db.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            
            return View(orders);
        }
        
        // GET: User/OrderDetails/5
        public ActionResult OrderDetails(int id)
        {
            var userEmail = User.Identity.Name;
            var user = _db.Users.FirstOrDefault(u => u.Email == userEmail);
            
            if (user == null)
            {
                return RedirectToAction("Login", "AuthFlow");
            }
            
            var order = _db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.ShippingAddress)
                .FirstOrDefault(o => o.Id == id && o.UserId == user.Id);
                
            if (order == null)
            {
                return HttpNotFound();
            }
            
            return View(order);
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 