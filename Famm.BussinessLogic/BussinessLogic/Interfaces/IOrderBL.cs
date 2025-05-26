using System;
using System.Collections.Generic;
using Famm.Domain.Models;
using Famm.Domain.Models.Enums;

namespace Famm.BussinessLogic.BussinessLogic.Interfaces
{
    public interface IOrderBL
    {
        List<Order> GetAllOrders();
        Order GetOrderById(int orderId);
        List<Order> GetOrdersByUserId(int userId);
        List<Order> GetOrdersByUserId(Guid userId);
        bool CreateOrder(Order order);
        bool UpdateOrder(Order order);
        bool UpdateOrderStatus(int orderId, OrderStatus status);
        bool DeleteOrder(int orderId);
    }
} 