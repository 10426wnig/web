using System;
using Famm.Domain.DTOs;
using Famm.Domain.Models;

namespace Famm.BussinessLogic.BussinessLogic.Core
{
    public abstract class CartApi : BaseApi
    {
        // Общая функциональность для всех Cart BL

        public virtual CartResultDto GetCartByUserId(int userId)
        {
            return new CartResultDto { IsSuccess = false };
        }
        
        public virtual CartResultDto GetCartByUserId(Guid userId)
        {
            return new CartResultDto { IsSuccess = false };
        }
        
        public virtual CartResultDto AddToCart(CartActionDto cartAction)
        {
            return new CartResultDto { IsSuccess = false };
        }
        
        public virtual CartResultDto RemoveFromCart(CartActionDto cartAction)
        {
            return new CartResultDto { IsSuccess = false };
        }
        
        public virtual CartResultDto UpdateCartItem(CartActionDto cartAction)
        {
            return new CartResultDto { IsSuccess = false };
        }
        
        public virtual CartResultDto ClearCart(int userId)
        {
            return new CartResultDto { IsSuccess = false };
        }
    }
} 