using System;

namespace Famm.Domain.Models
{
    /// <summary>
    /// Address model representing user addresses for shipping and billing
    /// </summary>
    public class Address
    {
        public int Id { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public bool IsDefault { get; set; }
        public string PhoneNumber { get; set; }
        public string RecipientName { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // Foreign keys
        public int UserId { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; }
        
        public Address()
        {
            CreatedDate = DateTime.UtcNow;
            IsDefault = false;
        }
        
        // Helper method to get a formatted address
        public string FormattedAddress
        {
            get
            {
                var address = $"{AddressLine1}";
                
                if (!string.IsNullOrEmpty(AddressLine2))
                {
                    address += $", {AddressLine2}";
                }
                
                address += $", {City}, {State} {PostalCode}, {Country}";
                
                return address;
            }
        }
    }
}
