using System;
using System.Collections.Generic;

namespace Famm.Domain.Models
{
    /// <summary>
    /// Product model representing items for sale in the store
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public string SKU { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        
        // Foreign keys
        public int CategoryId { get; set; }
        
        // Navigation properties
        public virtual Category Category { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        
        public Product()
        {
            OrderItems = new List<OrderItem>();
            CreatedDate = DateTime.UtcNow;
            IsAvailable = true;
        }
        
        // Helper method to check if product is on sale
        public bool IsOnSale => DiscountPrice.HasValue && DiscountPrice < Price;
        
        // Helper method to get the current price (either regular or discount)
        public decimal CurrentPrice => IsOnSale ? DiscountPrice.Value : Price;
    }
}
