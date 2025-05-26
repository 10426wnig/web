using System;
using Famm.Domain.DTOs;
using Famm.Domain.Models;

namespace Famm.BussinessLogic.BussinessLogic.Interfaces
{
    public interface ICartBL
    {
        CartResultDto GetCartByUserId(int userId);
        CartResultDto GetCartByUserId(Guid userId);
        
        CartResultDto AddToCart(CartActionDto cartAction);
        CartResultDto RemoveFromCart(CartActionDto cartAction);
        CartResultDto UpdateCartItem(CartActionDto cartAction);
        CartResultDto ClearCart(int userId);
    }
} 