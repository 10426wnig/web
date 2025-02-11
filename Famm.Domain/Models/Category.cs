using System;
using System.Collections.Generic;

namespace Famm.Domain.Models
{
    /// <summary>
    /// Category model representing product categories
    /// </summary>
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int? ParentCategoryId { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // Navigation properties
        public virtual Category ParentCategory { get; set; }
        public virtual ICollection<Category> ChildCategories { get; set; }
        public virtual ICollection<Product> Products { get; set; }
        
        public Category()
        {
            ChildCategories = new List<Category>();
            Products = new List<Product>();
            CreatedDate = DateTime.UtcNow;
            IsActive = true;
            DisplayOrder = 0;
        }
    }
}
