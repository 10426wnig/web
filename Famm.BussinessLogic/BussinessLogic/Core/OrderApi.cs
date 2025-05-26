using System;
using System.Collections.Generic;
using Famm.Domain.Models;
using Famm.Domain.Models.Enums;

namespace Famm.BussinessLogic.BussinessLogic.Core
{
    public abstract class OrderApi : BaseApi
    {
        // Общая функциональность для всех Order BL

        public virtual List<Order> GetAllOrders()
        {
            return new List<Order>();
        }
        
        public virtual Order GetOrderById(int orderId)
        {
            return null;
        }
        
        public virtual List<Order> GetOrdersByUserId(int userId)
        {
            return new List<Order>();
        }
        
        public virtual List<Order> GetOrdersByUserId(Guid userId)
        {
            return new List<Order>();
        }
        
        public virtual bool CreateOrder(Order order)
        {
            return false;
        }
        
        public virtual bool UpdateOrder(Order order)
        {
            return false;
        }
        
        public virtual bool UpdateOrderStatus(int orderId, OrderStatus status)
        {
            return false;
        }
        
        public virtual bool DeleteOrder(int orderId)
        {
            return false;
        }
    }
} 