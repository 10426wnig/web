using System.Web;

namespace Famm.Domain.DTOs
{
    /// <summary>
    /// DTO для хранения результата выхода из системы
    /// </summary>
    public class SignOutResultDto
    {
        /// <summary>
        /// Успешно ли выполнен выход из системы
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// Cookie для удаления состояния аутентификации
        /// </summary>
        public HttpCookie HttpCookie { get; set; }
        
        /// <summary>
        /// Сообщение о выходе из системы
        /// </summary>
        public string Message { get; set; }
    }
} 