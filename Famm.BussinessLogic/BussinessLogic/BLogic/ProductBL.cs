using System.Collections.Generic;
using System.Linq;
using Famm.BussinessLogic.BussinessLogic.Core;
using Famm.BussinessLogic.BussinessLogic.Interfaces;
using Famm.Database.Context;
using Famm.Domain.Models;

namespace Famm.BussinessLogic.BussinessLogic.BLogic
{
    public class ProductBL : ProductApi, IProductBL
    {
        private readonly FammDbContext _dbContext;
        
        public ProductBL()
        {
            _dbContext = new FammDbContext();
        }
        
        public List<Product> GetAllProducts()
        {
            return _dbContext.Products.ToList();
        }
        
        public Product GetProductById(int productId)
        {
            return _dbContext.Products.FirstOrDefault(p => p.Id == productId);
        }
        
        public List<Product> GetProductsByCategory(int categoryId)
        {
            return _dbContext.Products.Where(p => p.CategoryId == categoryId).ToList();
        }
        
        public bool AddProduct(Product product)
        {
            _dbContext.Products.Add(product);
            return _dbContext.SaveChanges() > 0;
        }
        
        public bool UpdateProduct(Product product)
        {
            var existingProduct = GetProductById(product.Id);
            
            if (existingProduct == null)
                return false;
            
            _dbContext.Entry(existingProduct).CurrentValues.SetValues(product);
            return _dbContext.SaveChanges() > 0;
        }
        
        public bool DeleteProduct(int productId)
        {
            var product = GetProductById(productId);
            
            if (product == null)
                return false;
            
            _dbContext.Products.Remove(product);
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