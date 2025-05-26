using System;
using System.Collections.Generic;
using Famm.Domain.DTOs;
using Famm.Domain.Models;

namespace Famm.BussinessLogic.BussinessLogic.Interfaces
{
    public interface IUserBL
    {
        User GetUserById(int userId);
        User GetUserById(Guid userId);
        User GetUserByEmail(string email);
        
        AuthResultDto Authenticate(string email, string password, bool rememberMe = false);
        SignOutResultDto SignOut();
        
        UserProfileDto GetUserProfile(int userId);
        UserProfileDto GetUserProfile(Guid userId);
        
        bool UpdateUserProfile(UserProfileDto userProfile);
        bool ChangePassword(int userId, string oldPassword, string newPassword);
        
        List<User> GetAllUsers();
    }
} 