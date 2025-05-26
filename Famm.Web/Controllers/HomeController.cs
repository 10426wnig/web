using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Famm.BussinessLogic.BussinessLogic;
using Famm.BussinessLogic.BussinessLogic.Interfaces;
using Famm.Domain.Models;
using Famm.Domain.Models.Enums;

namespace Famm.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductBL _productBL;
        private readonly IUserBL _userBL;
        private readonly IOrderBL _orderBL;
        
        public HomeController()
        {
            var factory = BusinessLogicFactory.Instance;
            _productBL = factory.GetProductBL();
            _userBL = factory.GetUserBL();
            _orderBL = factory.GetOrderBL();
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
            var products = _productBL.GetAllProducts()
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
            var product = _productBL.GetProductById(productId);
            
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
                user = _userBL.GetUserByEmail(userEmail);
            }
            else
            {
                // Если пользователь не авторизован, ищем его по email или создаем новый
                user = _userBL.GetUserByEmail(customerEmail);
                
                if (user == null)
                {
                    // В реальном приложении здесь должен быть метод для создания пользователя в бизнес-логике
                    // Для примера, мы продолжим использовать прямое создание
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
                    
                    // В реальном приложении здесь должен быть вызов метода бизнес-логики для добавления пользователя
                }
            }
            
            // Создаем заказ
            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                ShippingMethod = "Стандартная доставка",
                ShippingCost = 0, // Бесплатная доставка
                PaymentMethod = "Оплата при получении"
            };
            
            // Создаем адрес - в реальном приложении здесь должен быть метод бизнес-логики
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
            
            // В реальном приложении этот код должен быть в бизнес-логике
            order.ShippingAddressId = shippingAddress.Id;
            order.BillingAddressId = shippingAddress.Id;
            
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
            
            // В реальном приложении здесь должен быть вызов метода бизнес-логики для создания заказа с товарами
            // Например, _orderBL.CreateOrderWithItems(order, new List<OrderItem> { orderItem }, shippingAddress);
            
            // Для примера просто вызываем CreateOrder
            _orderBL.CreateOrder(order);
            
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
                (_productBL as IDisposable)?.Dispose();
                (_userBL as IDisposable)?.Dispose();
                (_orderBL as IDisposable)?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}