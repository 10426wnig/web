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
        
        public User GetUserById(int userId)
        {
            return _dbContext.Users.FirstOrDefault(u => u.Id == userId);
        }
        
        public User GetUserById(Guid userId)
        {
            return null; // В текущей модели используется int, но добавлена перегрузка для совместимости
        }
        
        public User GetUserByEmail(string email)
        {
            return _dbContext.Users.FirstOrDefault(u => u.Email == email);
        }
        
        public AuthResultDto Authenticate(string email, string password, bool rememberMe = false)
        {
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
        
        public SignOutResultDto SignOut()
        {
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
        
        public UserProfileDto GetUserProfile(int userId)
        {
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
        
        public UserProfileDto GetUserProfile(Guid userId)
        {
            return null; // В текущей модели используется int, но добавлена перегрузка для совместимости
        }
        
        public bool UpdateUserProfile(UserProfileDto userProfile)
        {
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
        
        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            var user = GetUserById(userId);
            
            if (user == null)
                return false;
            
            if (user.PasswordHash != oldPassword)
                return false;
            
            user.PasswordHash = newPassword;
            _dbContext.SaveChanges();
            return true;
        }
        
        public List<User> GetAllUsers()
        {
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