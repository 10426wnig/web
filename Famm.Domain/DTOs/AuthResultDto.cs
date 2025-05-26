using System.Web;

namespace Famm.Domain.DTOs
{
    /// <summary>
    /// DTO для результатов аутентификации
    /// </summary>
    public class AuthResultDto
    {
        /// <summary>
        /// Успешно ли прошла аутентификация
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// Сообщение об ошибке (если есть)
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// ID пользователя
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// Email пользователя
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Роль пользователя
        /// </summary>
        public string UserRole { get; set; }
        
        /// <summary>
        /// Cookie для сохранения состояния аутентификации
        /// </summary>
        public HttpCookie HttpCookie { get; set; }
    }
} 