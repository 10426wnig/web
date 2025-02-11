using System;
using System.Collections.Generic;
using System.Linq;
using Famm.Domain.Models.Enums;

namespace Famm.Domain.Models
{
    /// <summary>
    /// Order model representing customer orders
    /// </summary>
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentTransactionId { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string ShippingMethod { get; set; }
        public decimal ShippingCost { get; set; }
        public DateTime? ShippingDate { get; set; }
        public string TrackingNumber { get; set; }
        public string Notes { get; set; }
        
        // Foreign keys
        public int UserId { get; set; }
        public int? BillingAddressId { get; set; }
        public int? ShippingAddressId { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; }
        public virtual Address BillingAddress { get; set; }
        public virtual Address ShippingAddress { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        
        public Order()
        {
            OrderItems = new List<OrderItem>();
            OrderDate = DateTime.UtcNow;
            Status = OrderStatus.Pending;
            IsPaid = false;
            OrderNumber = GenerateOrderNumber();
        }
        
        // Helper method to calculate total
        public void CalculateTotal()
        {
            TotalAmount = OrderItems.Sum(item => item.Quantity * item.UnitPrice) + ShippingCost;
        }
        
        // Generate unique order number
        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}
