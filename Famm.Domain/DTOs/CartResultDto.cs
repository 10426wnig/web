using System.Collections.Generic;

namespace Famm.Domain.DTOs
{
    /// <summary>
    /// DTO для результатов операций с корзиной
    /// </summary>
    public class CartResultDto
    {
        /// <summary>
        /// Успешно ли выполнена операция
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// Сообщение о результате операции
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// ID корзины
        /// </summary>
        public int CartId { get; set; }
        
        /// <summary>
        /// Общее количество товаров в корзине
        /// </summary>
        public int TotalItems { get; set; }
        
        /// <summary>
        /// Общая стоимость товаров в корзине
        /// </summary>
        public decimal TotalPrice { get; set; }
        
        /// <summary>
        /// Список элементов корзины
        /// </summary>
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }
    
    /// <summary>
    /// DTO для элемента корзины
    /// </summary>
    public class CartItemDto
    {
        /// <summary>
        /// ID элемента корзины
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// ID продукта
        /// </summary>
        public int ProductId { get; set; }
        
        /// <summary>
        /// Название продукта
        /// </summary>
        public string ProductName { get; set; }
        
        /// <summary>
        /// URL изображения продукта
        /// </summary>
        public string ProductImageUrl { get; set; }
        
        /// <summary>
        /// Цена за единицу продукта
        /// </summary>
        public decimal UnitPrice { get; set; }
        
        /// <summary>
        /// Количество продукта
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// Общая стоимость позиции
        /// </summary>
        public decimal TotalPrice => UnitPrice * Quantity;
    }
} 