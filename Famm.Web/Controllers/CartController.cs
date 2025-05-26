using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Famm.BussinessLogic.BussinessLogic;
using Famm.BussinessLogic.BussinessLogic.Interfaces;
using Famm.Domain.DTOs;
using Famm.Domain.Models;
using Newtonsoft.Json;

namespace Famm.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductBL _productBL;
        private readonly IUserBL _userBL;
        private readonly IOrderBL _orderBL;
        private readonly ICartBL _cartBL;
        
        public CartController()
        {
            var factory = BusinessLogicFactory.Instance;
            _productBL = factory.GetProductBL();
            _userBL = factory.GetUserBL();
            _orderBL = factory.GetOrderBL();
            _cartBL = factory.GetCartBL();
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
            var product = _productBL.GetProductById(productId);
            
            if (product == null)
            {
                return Json(new { success = false, message = "Товар не найден" });
            }
            
            int userId = 0;
            if (User.Identity.IsAuthenticated)
            {
                var user = _userBL.GetUserByEmail(User.Identity.Name);
                if (user != null)
                {
                    userId = user.Id;
                    
                    // Используем бизнес-логику для добавления в корзину
                    var cartAction = new CartActionDto
                    {
                        UserId = userId,
                        ProductId = productId,
                        Quantity = quantity,
                        ActionType = CartActionType.Add
                    };
                    
                    var result = _cartBL.AddToCart(cartAction);
                    
                    return Json(new { 
                        success = result.IsSuccess, 
                        message = result.IsSuccess ? "Товар добавлен в корзину" : result.Message, 
                        itemCount = result.TotalItems 
                    });
                }
            }
            
            // Если пользователь не авторизован или не найден, работаем с корзиной в сессии
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
            
            int userId = 0;
            if (User.Identity.IsAuthenticated)
            {
                var user = _userBL.GetUserByEmail(User.Identity.Name);
                if (user != null)
                {
                    userId = user.Id;
                    
                    // Используем бизнес-логику для обновления корзины
                    var cartAction = new CartActionDto
                    {
                        UserId = userId,
                        ProductId = productId,
                        Quantity = quantity,
                        ActionType = CartActionType.Update
                    };
                    
                    var result = _cartBL.UpdateCartItem(cartAction);
                    
                    return Json(new { 
                        success = result.IsSuccess, 
                        message = result.IsSuccess ? "Количество обновлено" : result.Message
                    });
                }
            }
            
            // Если пользователь не авторизован или не найден, работаем с корзиной в сессии
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
            int userId = 0;
            if (User.Identity.IsAuthenticated)
            {
                var user = _userBL.GetUserByEmail(User.Identity.Name);
                if (user != null)
                {
                    userId = user.Id;
                    
                    // Используем бизнес-логику для удаления из корзины
                    var cartAction = new CartActionDto
                    {
                        UserId = userId,
                        ProductId = productId,
                        ActionType = CartActionType.Remove
                    };
                    
                    var result = _cartBL.RemoveFromCart(cartAction);
                    
                    return Json(new { 
                        success = result.IsSuccess, 
                        message = result.IsSuccess ? "Товар удален из корзины" : result.Message
                    });
                }
            }
            
            // Если пользователь не авторизован или не найден, работаем с корзиной в сессии
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
                // Если пользователь авторизован, берем его из бизнес-логики
                user = _userBL.GetUserByEmail(User.Identity.Name);
            }
            else
            {
                // Если пользователь не авторизован, ищем его по email или создаем новый через бизнес-логику
                user = _userBL.GetUserByEmail(customerEmail);
                
                if (user == null)
                {
                    // В реальном приложении здесь должен быть метод для создания пользователя в бизнес-логике
                    // Для примера, используем прямое создание
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
                Status = Domain.Models.Enums.OrderStatus.Pending,
                ShippingMethod = "Стандартная доставка",
                ShippingCost = 0,
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
            
            // В реальном приложении здесь должен быть вызов метода бизнес-логики для создания заказа с товарами
            // Например, _orderBL.CreateOrderWithItems(order, orderItems, shippingAddress);
            
            // Для примера просто вызываем CreateOrder
            _orderBL.CreateOrder(order);
            
            // Очищаем корзину
            ClearCart();
            
            return Json(new { success = true, message = "Заказ успешно оформлен! Номер заказа: " + order.OrderNumber });
        }
        
        private List<CartItem> GetCartItems()
        {
            var cartJson = Session["Cart"] as string;
            
            if (string.IsNullOrEmpty(cartJson))
                return new List<CartItem>();
                
            return JsonConvert.DeserializeObject<List<CartItem>>(cartJson) ?? new List<CartItem>();
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
                (_productBL as IDisposable)?.Dispose();
                (_userBL as IDisposable)?.Dispose();
                (_orderBL as IDisposable)?.Dispose();
                (_cartBL as IDisposable)?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
    
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