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
        
        public override List<Order> GetAllOrders()
        {
            var baseResult = base.GetAllOrders();
            if (baseResult != null && baseResult.Any())
                return baseResult;
                
            return _dbContext.Orders.ToList();
        }
        
        public override Order GetOrderById(int orderId)
        {
            var baseResult = base.GetOrderById(orderId);
            if (baseResult != null)
                return baseResult;
                
            return _dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
        }
        
        public override List<Order> GetOrdersByUserId(int userId)
        {
            var baseResult = base.GetOrdersByUserId(userId);
            if (baseResult != null && baseResult.Any())
                return baseResult;
                
            return _dbContext.Orders.Where(o => o.UserId == userId).ToList();
        }
        
        public override List<Order> GetOrdersByUserId(Guid userId)
        {
            var baseResult = base.GetOrdersByUserId(userId);
            if (baseResult != null && baseResult.Any())
                return baseResult;
                
            // В текущей модели используется int, но добавлена перегрузка для совместимости
            return new List<Order>();
        }
        
        public override bool CreateOrder(Order order)
        {
            if (base.CreateOrder(order))
                return true;
                
            _dbContext.Orders.Add(order);
            return _dbContext.SaveChanges() > 0;
        }
        
        public override bool UpdateOrder(Order order)
        {
            if (base.UpdateOrder(order))
                return true;
                
            var existingOrder = GetOrderById(order.Id);
            
            if (existingOrder == null)
                return false;
            
            _dbContext.Entry(existingOrder).CurrentValues.SetValues(order);
            return _dbContext.SaveChanges() > 0;
        }
        
        public override bool UpdateOrderStatus(int orderId, OrderStatus status)
        {
            if (base.UpdateOrderStatus(orderId, status))
                return true;
                
            var order = GetOrderById(orderId);
            
            if (order == null)
                return false;
            
            order.Status = status;
            return _dbContext.SaveChanges() > 0;
        }
        
        public override bool DeleteOrder(int orderId)
        {
            if (base.DeleteOrder(orderId))
                return true;
                
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