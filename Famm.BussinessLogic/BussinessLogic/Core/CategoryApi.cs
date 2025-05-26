using System.Collections.Generic;
using Famm.Domain.Models;

namespace Famm.BussinessLogic.BussinessLogic.Core
{
    public abstract class CategoryApi : BaseApi
    {
        // Общая функциональность для всех Category BL

        public virtual List<Category> GetAllCategories()
        {
            return new List<Category>();
        }
        
        public virtual Category GetCategoryById(int categoryId)
        {
            return null;
        }
        
        public virtual List<Category> GetSubcategories(int parentCategoryId)
        {
            return new List<Category>();
        }
        
        public virtual bool AddCategory(Category category)
        {
            return false;
        }
        
        public virtual bool UpdateCategory(Category category)
        {
            return false;
        }
        
        public virtual bool DeleteCategory(int categoryId)
        {
            return false;
        }
    }
} 