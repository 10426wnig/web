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
        
        public override List<Category> GetAllCategories()
        {
            var baseResult = base.GetAllCategories();
            if (baseResult != null && baseResult.Any())
                return baseResult;
                
            return _dbContext.Categories.ToList();
        }
        
        public override Category GetCategoryById(int categoryId)
        {
            var baseResult = base.GetCategoryById(categoryId);
            if (baseResult != null)
                return baseResult;
                
            return _dbContext.Categories.FirstOrDefault(c => c.Id == categoryId);
        }
        
        public override List<Category> GetSubcategories(int parentCategoryId)
        {
            var baseResult = base.GetSubcategories(parentCategoryId);
            if (baseResult != null && baseResult.Any())
                return baseResult;
                
            return _dbContext.Categories.Where(c => c.ParentCategoryId == parentCategoryId).ToList();
        }
        
        public override bool AddCategory(Category category)
        {
            if (base.AddCategory(category))
                return true;
                
            _dbContext.Categories.Add(category);
            return _dbContext.SaveChanges() > 0;
        }
        
        public override bool UpdateCategory(Category category)
        {
            if (base.UpdateCategory(category))
                return true;
                
            var existingCategory = GetCategoryById(category.Id);
            
            if (existingCategory == null)
                return false;
            
            _dbContext.Entry(existingCategory).CurrentValues.SetValues(category);
            return _dbContext.SaveChanges() > 0;
        }
        
        public override bool DeleteCategory(int categoryId)
        {
            if (base.DeleteCategory(categoryId))
                return true;
                
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