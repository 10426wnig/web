using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Famm.Database.Context;
using Famm.Domain.Models;
using System.Data.Entity;
using Newtonsoft.Json;

namespace Famm.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly FammDbContext _db;
        
        public CartController()
        {
            _db = new FammDbContext();
        }
        
        // GET: Cart
        public ActionResult Index()
        {
            var cartItems = GetCartItems();
            return View(cartItems);
        }
        
        // POST: Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int productId, int quantity = 1)
        {
            var product = _db.Products.Find(productId);
            
            if (product == null)
            {
                return Json(new { success = false, message = "Товар не найден" });
            }
            
            var cartItems = GetCartItems();
            
            // Проверяем, есть ли уже этот товар в корзине
            var existingItem = cartItems.FirstOrDefault(c => c.ProductId == productId);
            
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cartItems.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.CurrentPrice,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                });
            }
            
            SaveCartItems(cartItems);
            
            return Json(new { success = true, message = "Товар добавлен в корзину", itemCount = cartItems.Count });
        }
        
        // POST: Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                return RemoveFromCart(productId);
            }
            
            var cartItems = GetCartItems();
            var cartItem = cartItems.FirstOrDefault(c => c.ProductId == productId);
            
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                SaveCartItems(cartItems);
                
                return Json(new { success = true, message = "Количество обновлено" });
            }
            
            return Json(new { success = false, message = "Товар не найден в корзине" });
        }
        
        // POST: Cart/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(int productId)
        {
            var cartItems = GetCartItems();
            var itemToRemove = cartItems.FirstOrDefault(c => c.ProductId == productId);
            
            if (itemToRemove != null)
            {
                cartItems.Remove(itemToRemove);
                SaveCartItems(cartItems);
                
                return Json(new { success = true, message = "Товар удален из корзины" });
            }
            
            return Json(new { success = false, message = "Товар не найден в корзине" });
        }
        
        // POST: Cart/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(string customerName, string customerEmail, string customerPhone, string address)
        {
            var cartItems = GetCartItems();
            
            if (cartItems.Count == 0)
            {
                return Json(new { success = false, message = "Корзина пуста" });
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
                Status = Domain.Models.Enums.OrderStatus.Pending,
                ShippingAddressId = shippingAddress.Id,
                BillingAddressId = shippingAddress.Id,
                ShippingMethod = "Стандартная доставка",
                ShippingCost = 0, // Бесплатная доставка
                PaymentMethod = "Оплата при получении"
            };
            
            _db.Orders.Add(order);
            _db.SaveChanges(); // Сохраняем заказ, чтобы получить его Id
            
            // Добавляем товары в заказ
            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price,
                    Discount = 0,
                    ProductName = item.ProductName,
                    ProductSKU = _db.Products.Find(item.ProductId)?.SKU ?? "",
                    ProductImageUrl = item.ImageUrl
                };
                
                _db.OrderItems.Add(orderItem);
            }
            
            // Рассчитываем сумму заказа
            order.CalculateTotal();
            
            _db.SaveChanges();
            
            // Очищаем корзину
            ClearCart();
            
            return Json(new { success = true, message = "Заказ успешно создан! Номер заказа: " + order.OrderNumber });
        }
        
        // Вспомогательные методы для работы с корзиной в сессии
        private List<CartItem> GetCartItems()
        {
            var cartJson = Session["Cart"] as string;
            
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItem>();
            }
            
            return JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
        }
        
        private void SaveCartItems(List<CartItem> cartItems)
        {
            Session["Cart"] = JsonConvert.SerializeObject(cartItems);
        }
        
        private void ClearCart()
        {
            Session["Cart"] = null;
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
    
    // Класс для представления элемента корзины
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        
        public decimal TotalPrice => Price * Quantity;
    }
} 