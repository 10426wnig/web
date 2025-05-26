using System.Collections.Generic;
using System.Linq;
using Famm.BussinessLogic.BussinessLogic.Core;
using Famm.BussinessLogic.BussinessLogic.Interfaces;
using Famm.Database.Context;
using Famm.Domain.Models;

namespace Famm.BussinessLogic.BussinessLogic.BLogic
{
    public class CategoryBL : CategoryApi, ICategoryBL
    {
        private readonly FammDbContext _dbContext;
        
        public CategoryBL()
        {
            _dbContext = new FammDbContext();
        }
        
        public List<Category> GetAllCategories()
        {
            return _dbContext.Categories.ToList();
        }
        
        public Category GetCategoryById(int categoryId)
        {
            return _dbContext.Categories.FirstOrDefault(c => c.Id == categoryId);
        }
        
        public List<Category> GetSubcategories(int parentCategoryId)
        {
            return _dbContext.Categories.Where(c => c.ParentCategoryId == parentCategoryId).ToList();
        }
        
        public bool AddCategory(Category category)
        {
            _dbContext.Categories.Add(category);
            return _dbContext.SaveChanges() > 0;
        }
        
        public bool UpdateCategory(Category category)
        {
            var existingCategory = GetCategoryById(category.Id);
            
            if (existingCategory == null)
                return false;
            
            _dbContext.Entry(existingCategory).CurrentValues.SetValues(category);
            return _dbContext.SaveChanges() > 0;
        }
        
        public bool DeleteCategory(int categoryId)
        {
            var category = GetCategoryById(categoryId);
            
            if (category == null)
                return false;
            
            _dbContext.Categories.Remove(category);
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