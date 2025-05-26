namespace Famm.Domain.DTOs
{
    /// <summary>
    /// DTO для действий с корзиной покупок
    /// </summary>
    public class CartActionDto
    {
        /// <summary>
        /// ID пользователя
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// ID продукта
        /// </summary>
        public int ProductId { get; set; }
        
        /// <summary>
        /// Количество товара
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// Тип действия (добавить, удалить, обновить)
        /// </summary>
        public CartActionType ActionType { get; set; }
    }
    
    /// <summary>
    /// Типы действий с корзиной
    /// </summary>
    public enum CartActionType
    {
        /// <summary>
        /// Добавить товар в корзину
        /// </summary>
        Add = 1,
        
        /// <summary>
        /// Удалить товар из корзины
        /// </summary>
        Remove = 2,
        
        /// <summary>
        /// Обновить количество товара в корзине
        /// </summary>
        Update = 3,
        
        /// <summary>
        /// Очистить корзину
        /// </summary>
        Clear = 4
    }
} 