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
        
        public override List<Product> GetAllProducts()
        {
            var baseResult = base.GetAllProducts();
            if (baseResult != null && baseResult.Any())
                return baseResult;
                
            return _dbContext.Products.ToList();
        }
        
        public override Product GetProductById(int productId)
        {
            var baseResult = base.GetProductById(productId);
            if (baseResult != null)
                return baseResult;
                
            return _dbContext.Products.FirstOrDefault(p => p.Id == productId);
        }
        
        public override List<Product> GetProductsByCategory(int categoryId)
        {
            var baseResult = base.GetProductsByCategory(categoryId);
            if (baseResult != null && baseResult.Any())
                return baseResult;
                
            return _dbContext.Products.Where(p => p.CategoryId == categoryId).ToList();
        }
        
        public override bool AddProduct(Product product)
        {
            if (base.AddProduct(product))
                return true;
                
            _dbContext.Products.Add(product);
            return _dbContext.SaveChanges() > 0;
        }
        
        public override bool UpdateProduct(Product product)
        {
            if (base.UpdateProduct(product))
                return true;
                
            var existingProduct = GetProductById(product.Id);
            
            if (existingProduct == null)
                return false;
            
            _dbContext.Entry(existingProduct).CurrentValues.SetValues(product);
            return _dbContext.SaveChanges() > 0;
        }
        
        public override bool DeleteProduct(int productId)
        {
            if (base.DeleteProduct(productId))
                return true;
                
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