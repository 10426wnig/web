using System.Collections.Generic;
using Famm.Domain.Models;

namespace Famm.BussinessLogic.BussinessLogic.Interfaces
{
    public interface ICategoryBL
    {
        List<Category> GetAllCategories();
        Category GetCategoryById(int categoryId);
        List<Category> GetSubcategories(int parentCategoryId);
        bool AddCategory(Category category);
        bool UpdateCategory(Category category);
        bool DeleteCategory(int categoryId);
    }
} 