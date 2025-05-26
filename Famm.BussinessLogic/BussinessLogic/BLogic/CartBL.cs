using System;
using System.Linq;
using Famm.BussinessLogic.BussinessLogic.Core;
using Famm.BussinessLogic.BussinessLogic.Interfaces;
using Famm.Database.Context;
using Famm.Domain.DTOs;
using Famm.Domain.Models;

namespace Famm.BussinessLogic.BussinessLogic.BLogic
{
    public class CartBL : CartApi, ICartBL
    {
        private readonly FammDbContext _dbContext;
        
        public CartBL()
        {
            _dbContext = new FammDbContext();
        }
        
        public CartResultDto GetCartByUserId(int userId)
        {
            var cart = _dbContext.Carts
                .FirstOrDefault(c => c.UserId == userId);
                
            if (cart == null)
            {
                return new CartResultDto
                {
                    IsSuccess = false,
                    Message = "Корзина не найдена"
                };
            }
            
            var cartItems = _dbContext.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .ToList();
                
            var result = new CartResultDto
            {
                IsSuccess = true,
                CartId = cart.Id,
                TotalItems = cartItems.Sum(ci => ci.Quantity),
                TotalPrice = cartItems.Sum(ci => ci.Quantity * ci.Product.CurrentPrice),
                Items = cartItems.Select(ci => new CartItemDto
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    ProductImageUrl = ci.Product.ImageUrl,
                    UnitPrice = ci.Product.CurrentPrice,
                    Quantity = ci.Quantity
                }).ToList()
            };
            
            return result;
        }
        
        public CartResultDto GetCartByUserId(Guid userId)
        {
            // В текущей модели используется int, но добавлена перегрузка для совместимости
            return new CartResultDto
            {
                IsSuccess = false,
                Message = "Неподдерживаемый тип ID пользователя"
            };
        }
        
        public CartResultDto AddToCart(CartActionDto cartAction)
        {
            // Проверяем, есть ли у пользователя корзина
            var cart = _dbContext.Carts
                .FirstOrDefault(c => c.UserId == cartAction.UserId);
                
            if (cart == null)
            {
                // Создаем новую корзину
                cart = new Cart
                {
                    UserId = cartAction.UserId,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };
                
                _dbContext.Carts.Add(cart);
                _dbContext.SaveChanges();
            }
            
            // Проверяем, есть ли товар в корзине
            var cartItem = _dbContext.CartItems
                .FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == cartAction.ProductId);
                
            if (cartItem != null)
            {
                // Обновляем количество
                cartItem.Quantity += cartAction.Quantity;
                _dbContext.SaveChanges();
            }
            else
            {
                // Получаем информацию о продукте
                var product = _dbContext.Products.Find(cartAction.ProductId);
                
                if (product == null)
                {
                    return new CartResultDto
                    {
                        IsSuccess = false,
                        Message = "Товар не найден"
                    };
                }
                
                // Добавляем новый товар в корзину
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Quantity = cartAction.Quantity,
                    DateAdded = DateTime.UtcNow
                };
                
                _dbContext.CartItems.Add(cartItem);
                _dbContext.SaveChanges();
            }
            
            // Обновляем дату изменения корзины
            cart.LastModifiedDate = DateTime.UtcNow;
            _dbContext.SaveChanges();
            
            return GetCartByUserId(cartAction.UserId);
        }
        
        public CartResultDto RemoveFromCart(CartActionDto cartAction)
        {
            var cart = _dbContext.Carts
                .FirstOrDefault(c => c.UserId == cartAction.UserId);
                
            if (cart == null)
            {
                return new CartResultDto
                {
                    IsSuccess = false,
                    Message = "Корзина не найдена"
                };
            }
            
            var cartItem = _dbContext.CartItems
                .FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == cartAction.ProductId);
                
            if (cartItem == null)
            {
                return new CartResultDto
                {
                    IsSuccess = false,
                    Message = "Товар не найден в корзине"
                };
            }
            
            _dbContext.CartItems.Remove(cartItem);
            
            // Обновляем дату изменения корзины
            cart.LastModifiedDate = DateTime.UtcNow;
            _dbContext.SaveChanges();
            
            return GetCartByUserId(cartAction.UserId);
        }
        
        public CartResultDto UpdateCartItem(CartActionDto cartAction)
        {
            var cart = _dbContext.Carts
                .FirstOrDefault(c => c.UserId == cartAction.UserId);
                
            if (cart == null)
            {
                return new CartResultDto
                {
                    IsSuccess = false,
                    Message = "Корзина не найдена"
                };
            }
            
            var cartItem = _dbContext.CartItems
                .FirstOrDefault(ci => ci.CartId == cart.Id && ci.ProductId == cartAction.ProductId);
                
            if (cartItem == null)
            {
                return new CartResultDto
                {
                    IsSuccess = false,
                    Message = "Товар не найден в корзине"
                };
            }
            
            if (cartAction.Quantity <= 0)
            {
                _dbContext.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = cartAction.Quantity;
            }
            
            // Обновляем дату изменения корзины
            cart.LastModifiedDate = DateTime.UtcNow;
            _dbContext.SaveChanges();
            
            return GetCartByUserId(cartAction.UserId);
        }
        
        public CartResultDto ClearCart(int userId)
        {
            var cart = _dbContext.Carts
                .FirstOrDefault(c => c.UserId == userId);
                
            if (cart == null)
            {
                return new CartResultDto
                {
                    IsSuccess = false,
                    Message = "Корзина не найдена"
                };
            }
            
            var cartItems = _dbContext.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .ToList();
                
            foreach (var item in cartItems)
            {
                _dbContext.CartItems.Remove(item);
            }
            
            // Обновляем дату изменения корзины
            cart.LastModifiedDate = DateTime.UtcNow;
            _dbContext.SaveChanges();
            
            return new CartResultDto
            {
                IsSuccess = true,
                CartId = cart.Id,
                TotalItems = 0,
                TotalPrice = 0,
                Message = "Корзина очищена"
            };
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 