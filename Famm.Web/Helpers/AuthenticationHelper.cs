using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using Famm.Domain.Models;

namespace Famm.Web.Helpers
{
    /// <summary>
    /// Вспомогательный класс для работы с аутентификацией на уровне веб-слоя
    /// </summary>
    public static class AuthenticationHelper
    {
        private const string AUTH_COOKIE_PREFIX = "Famm_Auth_";
        private const string USER_DATA_SEPARATOR = "|";
        private const int TICKET_VERSION = 1;

        public class AuthContextModel
        {
            public int UserId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
            public bool IsAuthenticated { get; set; }
            public bool IsPersistent { get; set; }
            public DateTime LoginTime { get; set; }
            public Dictionary<string, string> AdditionalData { get; set; } = new Dictionary<string, string>();
        }

        public class CookieManager
        {
            public static void SetCookie(string name, string value, DateTime? expiry = null)
            {
                var cookie = new HttpCookie(name, value);
                
                if (expiry.HasValue)
                    cookie.Expires = expiry.Value;
                
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            
            public static string GetCookie(string name)
            {
                var cookie = HttpContext.Current.Request.Cookies[name];
                return cookie?.Value;
            }
            
            public static void RemoveCookie(string name)
            {
                var cookie = new HttpCookie(name)
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        /// <summary>
        /// Создает аутентификационный тикет и сохраняет его в куках
        /// </summary>
        public static void SignIn(User user, bool rememberMe = false)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var authContext = new AuthContextModel
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role ?? "User",
                IsAuthenticated = true,
                IsPersistent = rememberMe,
                LoginTime = DateTime.UtcNow
            };
            
            var userData = SerializeUserData(user);
            
            var ticket = new FormsAuthenticationTicket(
                version: TICKET_VERSION,
                name: user.Email,
                issueDate: DateTime.Now,
                expiration: DateTime.Now.AddDays(rememberMe ? 30 : 1),
                isPersistent: rememberMe,
                userData: userData,
                cookiePath: FormsAuthentication.FormsCookiePath);

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);
            
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            
            if (rememberMe)
                cookie.Expires = ticket.Expiration;
            
            HttpContext.Current.Response.Cookies.Add(cookie);
            
            SaveAdditionalUserData(authContext);
            
            // Сохраняем роль пользователя в сессии для проверки авторизации
            HttpContext.Current.Session["UserRole"] = user.Role ?? "User";
        }

        /// <summary>
        /// Выход пользователя из системы
        /// </summary>
        public static void SignOut()
        {
            var currentUser = GetCurrentUser();
            
            FormsAuthentication.SignOut();
            
            if (currentUser != null)
            {
                ClearUserSessionData(currentUser.Id);
            }
            
            HttpContext.Current.Session.Clear();
        }

        /// <summary>
        /// Получает информацию о текущем пользователе из куки
        /// </summary>
        public static User GetCurrentUser()
        {
            try
            {
                if (HttpContext.Current.User.Identity is FormsIdentity identity)
                {
                    var ticket = identity.Ticket;
                    if (ticket != null && !string.IsNullOrEmpty(ticket.UserData))
                    {
                        return DeserializeUserData(ticket.UserData);
                    }
                }
            }
            catch (Exception)
            {
                // Ошибка при получении пользователя
            }
            
            return null;
        }
        
        /// <summary>
        /// Проверяет, принадлежит ли текущий пользователь указанной роли
        /// </summary>
        public static bool IsUserInRole(string role)
        {
            if (string.IsNullOrEmpty(role))
                return false;
                
            var currentUser = GetCurrentUser();
            if (currentUser == null)
                return false;
                
            return string.Equals(currentUser.Role, role, StringComparison.OrdinalIgnoreCase);
        }

        public static AuthContextModel GetAuthContext()
        {
            var user = GetCurrentUser();
            
            if (user == null)
                return new AuthContextModel { IsAuthenticated = false };
            
            var result = new AuthContextModel
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role ?? "User",
                IsAuthenticated = true,
                LoginTime = user.LastLoginDate ?? DateTime.UtcNow
            };
            
            LoadAdditionalUserData(result);
            
            return result;
        }
        
        private static string SerializeUserData(User user)
        {
            var parts = new[]
            {
                user.Id.ToString(),
                user.FirstName ?? string.Empty,
                user.LastName ?? string.Empty,
                user.Email ?? string.Empty,
                user.PhoneNumber ?? string.Empty,
                (user.RegistrationDate.ToString("o")),
                (user.LastLoginDate?.ToString("o") ?? string.Empty),
                user.IsActive.ToString(),
                user.Role ?? "User"
            };
            
            return string.Join(USER_DATA_SEPARATOR, parts);
        }
        
        private static User DeserializeUserData(string userData)
        {
            var parts = userData.Split(new[] { USER_DATA_SEPARATOR }, StringSplitOptions.None);
            
            if (parts.Length < 4)
                return null;
            
            var user = new User
            {
                Id = int.Parse(parts[0]),
                FirstName = parts[1],
                LastName = parts[2],
                Email = parts[3]
            };
            
            if (parts.Length > 4)
                user.PhoneNumber = parts[4];
            
            if (parts.Length > 5 && DateTime.TryParse(parts[5], out var regDate))
                user.RegistrationDate = regDate;
            
            if (parts.Length > 6 && !string.IsNullOrEmpty(parts[6]) && DateTime.TryParse(parts[6], out var loginDate))
                user.LastLoginDate = loginDate;
            
            if (parts.Length > 7 && bool.TryParse(parts[7], out var isActive))
                user.IsActive = isActive;
                
            if (parts.Length > 8)
                user.Role = parts[8];
            else
                user.Role = "User";
            
            return user;
        }
        
        private static void SaveAdditionalUserData(AuthContextModel context)
        {
            var sessionId = HttpContext.Current.Session.SessionID;
            CookieManager.SetCookie($"{AUTH_COOKIE_PREFIX}Session_{context.UserId}", sessionId);
            
            foreach (var kvp in context.AdditionalData)
            {
                HttpContext.Current.Session[$"{AUTH_COOKIE_PREFIX}{kvp.Key}"] = kvp.Value;
            }
        }
        
        private static void LoadAdditionalUserData(AuthContextModel context)
        {
            var sessionKeys = HttpContext.Current.Session.Keys.Cast<string>()
                .Where(k => k.StartsWith(AUTH_COOKIE_PREFIX))
                .ToList();
            
            foreach (var key in sessionKeys)
            {
                var dataKey = key.Substring(AUTH_COOKIE_PREFIX.Length);
                context.AdditionalData[dataKey] = HttpContext.Current.Session[key]?.ToString();
            }
        }
        
        private static void ClearUserSessionData(int userId)
        {
            CookieManager.RemoveCookie($"{AUTH_COOKIE_PREFIX}Session_{userId}");
            
            var sessionKeys = HttpContext.Current.Session.Keys.Cast<string>()
                .Where(k => k.StartsWith(AUTH_COOKIE_PREFIX))
                .ToList();
            
            foreach (var key in sessionKeys)
            {
                HttpContext.Current.Session.Remove(key);
            }
        }
    }
} 