using System;
using System.Collections.Generic;

namespace Famm.Domain.Models
{
    /// <summary>
    /// User model representing a customer in the system
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; }
        
        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Address> Addresses { get; set; }
        public virtual Cart Cart { get; set; }
        
        public User()
        {
            Orders = new List<Order>();
            Addresses = new List<Address>();
            RegistrationDate = DateTime.UtcNow;
            IsActive = true;
            Role = "User"; 
        }
        
        // Helper property to display full name
        public string FullName => $"{FirstName} {LastName}";
    }
}
