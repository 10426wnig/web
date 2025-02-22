using System.Data.Entity.Migrations;
using Famm.Database.Context;
using Famm.Domain.Models;

namespace Famm.Database.Migrations
{
    /// <summary>
    /// Конфигурация миграций для базы данных Famm
    /// </summary>
    internal sealed class Configuration : DbMigrationsConfiguration<FammDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "Famm.Database.Context.FammDbContext";
        }

        protected override void Seed(FammDbContext context)
        {
            // Добавляем администратора
            context.Users.AddOrUpdate(
                u => u.Email,
                new User
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@famm.com",
                    PasswordHash = "Admin123", // В реальном приложении должен быть хеш
                    IsActive = true,
                    Role = "Admin",
                    PhoneNumber = "123-456-7890"
                }
            );
            
            // Здесь можно добавить начальные данные для БД
            // Например:
            
            /*
            context.Categories.AddOrUpdate(
                c => c.Name,
                new Category { Name = "Men's Clothing", Description = "Clothing items for men", IsActive = true },
                new Category { Name = "Women's Clothing", Description = "Clothing items for women", IsActive = true },
                new Category { Name = "Kids' Clothing", Description = "Clothing items for kids", IsActive = true },
                new Category { Name = "Accessories", Description = "Fashion accessories", IsActive = true }
            );
            
            context.SaveChanges();
            */
        }
    }
}
