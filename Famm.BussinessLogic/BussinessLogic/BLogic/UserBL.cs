using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Famm.BussinessLogic.BussinessLogic.Core;
using Famm.BussinessLogic.BussinessLogic.Interfaces;
using Famm.Database.Context;
using Famm.Domain.DTOs;
using Famm.Domain.Models;

namespace Famm.BussinessLogic.BussinessLogic.BLogic
{
    public class UserBL : UserApi, IUserBL
    {
        private readonly FammDbContext _dbContext;
        
        public UserBL()
        {
            _dbContext = new FammDbContext();
        }
        
        public override User GetUserById(int userId)
        {
            return base.GetUserById(userId) ?? _dbContext.Users.FirstOrDefault(u => u.Id == userId);
        }
        
        public override User GetUserById(Guid userId)
        {
            return base.GetUserById(userId); // В текущей модели используется int, но добавлена перегрузка для совместимости
        }
        
        public override User GetUserByEmail(string email)
        {
            return base.GetUserByEmail(email) ?? _dbContext.Users.FirstOrDefault(u => u.Email == email);
        }
        
        public override AuthResultDto Authenticate(string email, string password, bool rememberMe = false)
        {
            var baseResult = base.Authenticate(email, password, rememberMe);
            if (baseResult != null && baseResult.IsSuccess)
                return baseResult;
                
            var user = GetUserByEmail(email);
            
            if (user == null)
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Пользователь не найден"
                };
            }
            
            // Проверка пароля (хеширование опущено для простоты)
            if (user.PasswordHash != password)
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Неверный пароль"
                };
            }
            
            if (!user.IsActive)
            {
                return new AuthResultDto
                {
                    IsSuccess = false,
                    ErrorMessage = "Учетная запись отключена"
                };
            }
            
            // Обновляем дату последнего входа
            user.LastLoginDate = DateTime.UtcNow;
            _dbContext.SaveChanges();
            
            // Создаем аутентификационный тикет
            var ticket = new FormsAuthenticationTicket(
                version: 1,
                name: user.Email,
                issueDate: DateTime.Now,
                expiration: DateTime.Now.AddDays(rememberMe ? 30 : 1),
                isPersistent: rememberMe,
                userData: SerializeUserData(user),
                cookiePath: FormsAuthentication.FormsCookiePath);

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);
            
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
            {
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL
            };
            
            if (rememberMe)
                cookie.Expires = ticket.Expiration;
            
            return new AuthResultDto
            {
                IsSuccess = true,
                UserId = user.Id,
                Email = user.Email,
                UserRole = user.Role,
                HttpCookie = cookie
            };
        }
        
        public override SignOutResultDto SignOut()
        {
            var baseResult = base.SignOut();
            if (baseResult != null && baseResult.IsSuccess)
                return baseResult;
                
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName)
            {
                Expires = DateTime.Now.AddDays(-1),
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL
            };
            
            return new SignOutResultDto
            {
                IsSuccess = true,
                HttpCookie = cookie,
                Message = "Выход выполнен успешно"
            };
        }
        
        public override UserProfileDto GetUserProfile(int userId)
        {
            var baseResult = base.GetUserProfile(userId);
            if (baseResult != null)
                return baseResult;
                
            var user = GetUserById(userId);
            
            if (user == null)
                return null;
            
            return new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RegistrationDate = user.RegistrationDate,
                LastLoginDate = user.LastLoginDate,
                Role = user.Role,
                IsActive = user.IsActive
            };
        }
        
        public override UserProfileDto GetUserProfile(Guid userId)
        {
            return base.GetUserProfile(userId); // В текущей модели используется int, но добавлена перегрузка для совместимости
        }
        
        public override bool UpdateUserProfile(UserProfileDto userProfile)
        {
            if (base.UpdateUserProfile(userProfile))
                return true;
                
            var user = GetUserById(userProfile.Id);
            
            if (user == null)
                return false;
            
            user.FirstName = userProfile.FirstName;
            user.LastName = userProfile.LastName;
            user.PhoneNumber = userProfile.PhoneNumber;
            user.IsActive = userProfile.IsActive;
            user.Role = userProfile.Role;
            
            // Email обычно не меняем, так как он используется для входа
            
            _dbContext.SaveChanges();
            return true;
        }
        
        public override bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            if (base.ChangePassword(userId, oldPassword, newPassword))
                return true;
                
            var user = GetUserById(userId);
            
            if (user == null)
                return false;
            
            if (user.PasswordHash != oldPassword)
                return false;
            
            user.PasswordHash = newPassword;
            _dbContext.SaveChanges();
            return true;
        }
        
        public override List<User> GetAllUsers()
        {
            var baseResult = base.GetAllUsers();
            if (baseResult != null && baseResult.Any())
                return baseResult;
                
            return _dbContext.Users.ToList();
        }
        
        private string SerializeUserData(User user)
        {
            var parts = new[]
            {
                user.Id.ToString(),
                user.FirstName ?? string.Empty,
                user.LastName ?? string.Empty,
                user.Email ?? string.Empty,
                user.PhoneNumber ?? string.Empty,
                user.RegistrationDate.ToString("o"),
                user.LastLoginDate?.ToString("o") ?? string.Empty,
                user.IsActive.ToString(),
                user.Role ?? "User"
            };
            
            return string.Join("|", parts);
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