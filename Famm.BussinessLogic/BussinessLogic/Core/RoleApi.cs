using System.Collections.Generic;

namespace Famm.BussinessLogic.BussinessLogic.Core
{
    public abstract class RoleApi : BaseApi
    {
        // Общая функциональность для всех Role BL

        public virtual string[] GetAllRoles()
        {
            return new string[0];
        }
        
        public virtual string[] GetRolesForUser(string username)
        {
            return new string[0];
        }
        
        public virtual string[] GetUsersInRole(string roleName)
        {
            return new string[0];
        }
        
        public virtual bool IsUserInRole(string username, string roleName)
        {
            return false;
        }
        
        public virtual bool RoleExists(string roleName)
        {
            return false;
        }
    }
} 