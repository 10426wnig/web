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
        
        public string[] GetAllRoles()
        {
            return _availableRoles;
        }
        
        public string[] GetRolesForUser(string username)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == username);
            
            if (user == null || string.IsNullOrEmpty(user.Role))
                return new[] { "Guest" };
            
            return new[] { user.Role };
        }
        
        public string[] GetUsersInRole(string roleName)
        {
            if (!RoleExists(roleName))
                return new string[0];
            
            return _dbContext.Users
                .Where(u => u.Role == roleName)
                .Select(u => u.Email)
                .ToArray();
        }
        
        public bool IsUserInRole(string username, string roleName)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == username);
            
            if (user == null || string.IsNullOrEmpty(user.Role))
                return false;
            
            return string.Equals(user.Role, roleName, StringComparison.OrdinalIgnoreCase);
        }
        
        public bool RoleExists(string roleName)
        {
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