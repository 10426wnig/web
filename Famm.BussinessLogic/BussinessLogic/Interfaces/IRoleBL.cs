using System.Collections.Generic;

namespace Famm.BussinessLogic.BussinessLogic.Interfaces
{
    public interface IRoleBL
    {
        string[] GetAllRoles();
        string[] GetRolesForUser(string username);
        string[] GetUsersInRole(string roleName);
        bool IsUserInRole(string username, string roleName);
        bool RoleExists(string roleName);
    }
} 