using System;
using Famm.BussinessLogic.BussinessLogic.BLogic;
using Famm.BussinessLogic.BussinessLogic.Interfaces;

namespace Famm.BussinessLogic.BussinessLogic
{
    public class BusinessLogicFactory
    {
        private static readonly Lazy<BusinessLogicFactory> _instance = 
            new Lazy<BusinessLogicFactory>(() => new BusinessLogicFactory());
            
        public static BusinessLogicFactory Instance => _instance.Value;
        
        private BusinessLogicFactory()
        {
        }
        
        public IUserBL GetUserBL()
        {
            return new UserBL();
        }
        
        public IRoleBL GetRoleBL()
        {
            return new RoleBL();
        }
        
        public IProductBL GetProductBL()
        {
            return new ProductBL();
        }
        
        public IOrderBL GetOrderBL()
        {
            return new OrderBL();
        }
        
        public ICartBL GetCartBL()
        {
            return new CartBL();
        }
        
        public ICategoryBL GetCategoryBL()
        {
            return new CategoryBL();
        }
    }
} 