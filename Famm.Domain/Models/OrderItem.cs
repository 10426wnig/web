using System;

namespace Famm.Domain.Models
{
    /// <summary>
    /// OrderItem model representing individual items in an order
    /// </summary>
    public class OrderItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public string ProductName { get; set; }  // Storing the name in case the product changes later
        public string ProductSKU { get; set; }
        public string ProductImageUrl { get; set; }
        
        // Foreign keys
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        
        // Navigation properties
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
        
        // Helper property to calculate total for this line item
        public decimal TotalPrice => Quantity * UnitPrice - Discount;
    }
}
