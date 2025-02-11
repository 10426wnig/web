using System;

namespace Famm.Domain.Models
{
    /// <summary>
    /// CartItem model representing individual items in a shopping cart
    /// </summary>
    public class CartItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public DateTime DateAdded { get; set; }
        
        // Foreign keys
        public int CartId { get; set; }
        public int ProductId { get; set; }
        
        // Navigation properties
        public virtual Cart Cart { get; set; }
        public virtual Product Product { get; set; }
        
        public CartItem()
        {
            DateAdded = DateTime.UtcNow;
            Quantity = 1;
        }
        
        // Helper property to calculate total price for this cart item
        public decimal TotalPrice => Quantity * Product.CurrentPrice;
    }
}
