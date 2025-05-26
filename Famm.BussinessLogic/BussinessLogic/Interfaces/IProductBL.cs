using System.Collections.Generic;
using Famm.Domain.Models;

namespace Famm.BussinessLogic.BussinessLogic.Interfaces
{
    public interface IProductBL
    {
        List<Product> GetAllProducts();
        Product GetProductById(int productId);
        List<Product> GetProductsByCategory(int categoryId);
        bool AddProduct(Product product);
        bool UpdateProduct(Product product);
        bool DeleteProduct(int productId);
    }
} 