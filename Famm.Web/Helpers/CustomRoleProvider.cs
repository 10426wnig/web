using System;
using System.Web.Security;
using Famm.Application.BussinessLogic;
using Famm.Application.BussinessLogic.BLogic;

namespace Famm.Web.Helpers
{
    public class CustomRoleProvider : RoleProvider
    {
        private readonly UserBL _userBL;
        private readonly RoleBL _roleBL;

        public CustomRoleProvider()
        {
            var factory = BusinessLogicFactory.Instance;
            _userBL = factory.GetUserBL() as UserBL;
            _roleBL = factory.GetRoleBL() as RoleBL;
        }
        
        public override string ApplicationName { get; set; }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException("Метод не поддерживается в текущей реализации");
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException("Метод не поддерживается в текущей реализации");
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException("Метод не поддерживается в текущей реализации");
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            return _roleBL.GetUsersInRole(roleName);
        }

        public override string[] GetAllRoles()
        {
            return _roleBL.GetAllRoles();
        }

        public override string[] GetRolesForUser(string username)
        {
            return _roleBL.GetRolesForUser(username);
        }

        public override string[] GetUsersInRole(string roleName)
        {
            return _roleBL.GetUsersInRole(roleName);
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            return _roleBL.IsUserInRole(username, roleName);
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException("Метод не поддерживается в текущей реализации");
        }

        public override bool RoleExists(string roleName)
        {
            return _roleBL.RoleExists(roleName);
        }
    }
} 