using System;
using System.Collections.Generic;
using System.Web;
using Famm.Domain.DTOs;
using Famm.Domain.Models;

namespace Famm.BussinessLogic.BussinessLogic.Core
{
    public abstract class UserApi : BaseApi
    {
        // Общая функциональность для всех User BL

        public virtual User GetUserById(int userId)
        {
            return null;
        }
        
        public virtual User GetUserById(Guid userId)
        {
            return null;
        }
        
        public virtual User GetUserByEmail(string email)
        {
            return null;
        }
        
        public virtual AuthResultDto Authenticate(string email, string password, bool rememberMe = false)
        {
            return new AuthResultDto { IsSuccess = false };
        }
        
        public virtual SignOutResultDto SignOut()
        {
            return new SignOutResultDto { IsSuccess = false };
        }
        
        public virtual UserProfileDto GetUserProfile(int userId)
        {
            return null;
        }
        
        public virtual UserProfileDto GetUserProfile(Guid userId)
        {
            return null;
        }
        
        public virtual bool UpdateUserProfile(UserProfileDto userProfile)
        {
            return false;
        }
        
        public virtual bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            return false;
        }
        
        public virtual List<User> GetAllUsers()
        {
            return new List<User>();
        }
    }
} 