using System;
using System.Collections.Generic;
using System.Linq;

namespace Famm.Domain.Models
{
    /// <summary>
    /// Cart model representing a user's shopping cart
    /// </summary>
    public class Cart
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        
        // Foreign keys
        public int UserId { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
        
        public Cart()
        {
            CartItems = new List<CartItem>();
            CreatedDate = DateTime.UtcNow;
            LastModifiedDate = DateTime.UtcNow;
        }
        
        // Helper property to calculate total items in cart
        public int TotalItems => CartItems.Sum(i => i.Quantity);
        
        // Helper property to calculate total price of cart
        public decimal TotalPrice => CartItems.Sum(i => i.Quantity * i.Product.CurrentPrice);
    }
}
