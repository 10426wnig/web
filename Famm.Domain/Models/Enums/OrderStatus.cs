namespace Famm.Domain.Models.Enums
{
    /// <summary>
    /// Represents the possible statuses of an order
    /// </summary>
    public enum OrderStatus
    {
        Pending = 1,
        Processing = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5,
        Returned = 6,
        Refunded = 7
    }
}
