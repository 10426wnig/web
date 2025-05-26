using System.Collections.Generic;
using Famm.Domain.Models;

namespace Famm.BussinessLogic.BussinessLogic.Core
{
    public abstract class ProductApi : BaseApi
    {
        // Общая функциональность для всех Product BL

        public virtual List<Product> GetAllProducts()
        {
            return new List<Product>();
        }
        
        public virtual Product GetProductById(int productId)
        {
            return null;
        }
        
        public virtual List<Product> GetProductsByCategory(int categoryId)
        {
            return new List<Product>();
        }
        
        public virtual bool AddProduct(Product product)
        {
            return false;
        }
        
        public virtual bool UpdateProduct(Product product)
        {
            return false;
        }
        
        public virtual bool DeleteProduct(int productId)
        {
            return false;
        }
    }
} 