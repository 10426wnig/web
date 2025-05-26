using System;
using System.Collections.Generic;
using System.Linq;
using Famm.BussinessLogic.BussinessLogic.Core;
using Famm.BussinessLogic.BussinessLogic.Interfaces;
using Famm.Database.Context;

namespace Famm.BussinessLogic.BussinessLogic.BLogic
{
    public class RoleBL : RoleApi, IRoleBL
    {
        private readonly FammDbContext _dbContext;
        
        private readonly string[] _availableRoles = { "Admin", "Manager", "User", "Customer", "Guest" };
        
        public RoleBL()
        {
            _dbContext = new FammDbContext();
        }
        
        public override string[] GetAllRoles()
        {
            var baseResult = base.GetAllRoles();
            if (baseResult != null && baseResult.Length > 0)
                return baseResult;
                
            return _availableRoles;
        }
        
        public override string[] GetRolesForUser(string username)
        {
            var baseResult = base.GetRolesForUser(username);
            if (baseResult != null && baseResult.Length > 0)
                return baseResult;
                
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == username);
            
            if (user == null || string.IsNullOrEmpty(user.Role))
                return new[] { "Guest" };
            
            return new[] { user.Role };
        }
        
        public override string[] GetUsersInRole(string roleName)
        {
            var baseResult = base.GetUsersInRole(roleName);
            if (baseResult != null && baseResult.Length > 0)
                return baseResult;
                
            if (!RoleExists(roleName))
                return new string[0];
            
            return _dbContext.Users
                .Where(u => u.Role == roleName)
                .Select(u => u.Email)
                .ToArray();
        }
        
        public override bool IsUserInRole(string username, string roleName)
        {
            if (base.IsUserInRole(username, roleName))
                return true;
                
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == username);
            
            if (user == null || string.IsNullOrEmpty(user.Role))
                return false;
            
            return string.Equals(user.Role, roleName, StringComparison.OrdinalIgnoreCase);
        }
        
        public override bool RoleExists(string roleName)
        {
            if (base.RoleExists(roleName))
                return true;
                
            return _availableRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
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