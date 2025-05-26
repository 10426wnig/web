using System;
using System.Collections.Generic;
using System.Linq;
using Famm.BussinessLogic.BussinessLogic.Core;
using Famm.BussinessLogic.BussinessLogic.Interfaces;
using Famm.Database.Context;
using Famm.Domain.Models;
using Famm.Domain.Models.Enums;

namespace Famm.BussinessLogic.BussinessLogic.BLogic
{
    public class OrderBL : OrderApi, IOrderBL
    {
        private readonly FammDbContext _dbContext;
        
        public OrderBL()
        {
            _dbContext = new FammDbContext();
        }
        
        public List<Order> GetAllOrders()
        {
            return _dbContext.Orders.ToList();
        }
        
        public Order GetOrderById(int orderId)
        {
            return _dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
        }
        
        public List<Order> GetOrdersByUserId(int userId)
        {
            return _dbContext.Orders.Where(o => o.UserId == userId).ToList();
        }
        
        public List<Order> GetOrdersByUserId(Guid userId)
        {
            // В текущей модели используется int, но добавлена перегрузка для совместимости
            return new List<Order>();
        }
        
        public bool CreateOrder(Order order)
        {
            _dbContext.Orders.Add(order);
            return _dbContext.SaveChanges() > 0;
        }
        
        public bool UpdateOrder(Order order)
        {
            var existingOrder = GetOrderById(order.Id);
            
            if (existingOrder == null)
                return false;
            
            _dbContext.Entry(existingOrder).CurrentValues.SetValues(order);
            return _dbContext.SaveChanges() > 0;
        }
        
        public bool UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var order = GetOrderById(orderId);
            
            if (order == null)
                return false;
            
            order.Status = status;
            return _dbContext.SaveChanges() > 0;
        }
        
        public bool DeleteOrder(int orderId)
        {
            var order = GetOrderById(orderId);
            
            if (order == null)
                return false;
            
            _dbContext.Orders.Remove(order);
            return _dbContext.SaveChanges() > 0;
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