using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Famm.Database.Context;
using Famm.Domain.Models;
using Famm.Domain.Models.Enums;

namespace Famm.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly FammDbContext _db;
        
        public HomeController()
        {
            _db = new FammDbContext();
        }
        
        public ActionResult Index()
        {
            ViewBag.ShowSlider = true;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
        
        public ActionResult Product()
        {
            var products = _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsAvailable)
                .OrderBy(p => p.Category.Name)
                .ThenBy(p => p.Name)
                .ToList();
                
            return View(products);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateOrder(int productId, int quantity, string customerName, string customerEmail, string customerPhone, string address)
        {
            var product = _db.Products.Find(productId);
            
            if (product == null || quantity <= 0)
            {
                return Json(new { success = false, message = "Неверные данные заказа" });
            }
            
            // Проверяем, авторизован ли пользователь
            User user = null;
            
            if (User.Identity.IsAuthenticated)
            {
                // Если пользователь авторизован, берем его из базы данных
                var userEmail = User.Identity.Name;
                user = _db.Users.FirstOrDefault(u => u.Email == userEmail);
            }
            else
            {
                // Если пользователь не авторизован, ищем его по email или создаем новый
                user = _db.Users.FirstOrDefault(u => u.Email == customerEmail);
                
                if (user == null)
                {
                    // Создаем нового пользователя
                    user = new User
                    {
                        Email = customerEmail,
                        FirstName = customerName.Split(' ').FirstOrDefault() ?? customerName,
                        LastName = customerName.Split(' ').Skip(1).FirstOrDefault() ?? "",
                        PhoneNumber = customerPhone,
                        PasswordHash = "tempPassword123", // Временный пароль для валидации
                        RegistrationDate = DateTime.UtcNow,
                        IsActive = true,
                        Role = "Customer"
                    };
                    
                    _db.Users.Add(user);
                    _db.SaveChanges(); // Сохраняем пользователя, чтобы получить его Id
                }
            }
            
            // Создаем адрес
            var shippingAddress = new Address
            {
                UserId = user.Id,
                AddressLine1 = address,
                IsDefault = true,
                City = "Не указан",
                State = "Не указан",
                PostalCode = "Не указан",
                Country = "Молдова"
            };
            
            _db.Addresses.Add(shippingAddress);
            _db.SaveChanges(); // Сохраняем адрес, чтобы получить его Id
            
            // Создаем заказ
            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                ShippingAddressId = shippingAddress.Id,
                BillingAddressId = shippingAddress.Id,
                ShippingMethod = "Стандартная доставка",
                ShippingCost = 0, // Бесплатная доставка
                PaymentMethod = "Оплата при получении"
            };
            
            _db.Orders.Add(order);
            _db.SaveChanges(); // Сохраняем заказ, чтобы получить его Id
            
            // Добавляем товар в заказ
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = product.Id,
                Quantity = quantity,
                UnitPrice = product.CurrentPrice,
                Discount = 0,
                ProductName = product.Name,
                ProductSKU = product.SKU,
                ProductImageUrl = product.ImageUrl
            };
            
            _db.OrderItems.Add(orderItem);
            
            // Рассчитываем сумму заказа
            order.CalculateTotal();
            
            _db.SaveChanges();
            
            return Json(new { success = true, message = "Заказ успешно создан! Номер заказа: " + order.OrderNumber });
        }
        
        public ActionResult Blog()
        {
            return View();
        }
        
        public ActionResult Testimonial()
        {
            return View();
        }

        // GET: Home/AccessDenied
        public ActionResult AccessDenied()
        {
            return View();
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