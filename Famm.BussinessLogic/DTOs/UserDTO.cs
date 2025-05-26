using System;

namespace Famm.BussinessLogic.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActiveUser { get; set; }
        public bool RememberMe { get; set; }
        public string Role { get; set; }
    }
} 