using System;

namespace Famm.Domain.DTOs
{
    /// <summary>
    /// DTO для информации о пользователе
    /// </summary>
    public class UserProfileDto
    {
        /// <summary>
        /// ID пользователя
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string FirstName { get; set; }
        
        /// <summary>
        /// Фамилия пользователя
        /// </summary>
        public string LastName { get; set; }
        
        /// <summary>
        /// Email пользователя
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Номер телефона пользователя
        /// </summary>
        public string PhoneNumber { get; set; }
        
        /// <summary>
        /// Дата регистрации
        /// </summary>
        public DateTime RegistrationDate { get; set; }
        
        /// <summary>
        /// Дата последнего входа
        /// </summary>
        public DateTime? LastLoginDate { get; set; }
        
        /// <summary>
        /// Роль пользователя
        /// </summary>
        public string Role { get; set; }
        
        /// <summary>
        /// Активен ли пользователь
        /// </summary>
        public bool IsActive { get; set; }
    }
} 