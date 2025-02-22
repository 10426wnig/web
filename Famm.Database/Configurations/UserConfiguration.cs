using System.Data.Entity.ModelConfiguration;
using Famm.Domain.Models;

namespace Famm.Database.Configurations
{
    /// <summary>
    /// Конфигурация для сущности User
    /// </summary>
    public class UserConfiguration : EntityTypeConfiguration<User>
    {
        public UserConfiguration()
        {
            // Первичный ключ
            HasKey(u => u.Id);
            
            // Свойства
            Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);
                
            Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(50);
                
            Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(50);
                
            Property(u => u.PasswordHash)
                .IsRequired();
                
            Property(u => u.PhoneNumber)
                .HasMaxLength(20);
                
            // Индексы
            HasIndex(u => u.Email)
                .IsUnique();
                
            // Связи
            HasMany(u => u.Orders)
                .WithRequired(o => o.User)
                .HasForeignKey(o => o.UserId)
                .WillCascadeOnDelete(false);
                
            HasMany(u => u.Addresses)
                .WithRequired(a => a.User)
                .HasForeignKey(a => a.UserId)
                .WillCascadeOnDelete(true);
                
            HasOptional(u => u.Cart)
                .WithRequired(c => c.User);
                
            // Имя таблицы
            ToTable("Users");
        }
    }
}
