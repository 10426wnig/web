using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Famm.Database.Context;
using Famm.Domain.Models;
using Famm.Web.Helpers;
using Famm.Domain.Models.Enums;

namespace Famm.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly FammDbContext _db;
        
        public AdminController()
        {
            _db = new FammDbContext();
        }
        
        // Dashboard
        public ActionResult Index()
        {
            var dashboardData = new AdminDashboardViewModel
            {
                TotalProducts = _db.Products.Count(),
                TotalOrders = _db.Orders.Count(),
                TotalUsers = _db.Users.Count(),
                RecentOrders = _db.Orders.OrderByDescending(o => o.OrderDate).Take(5).ToList(),
                TopSellingProducts = _db.Products.OrderByDescending(p => p.OrderItems.Sum(oi => oi.Quantity)).Take(5).ToList()
            };
            
            return View(dashboardData);
        }
        
        #region Products

        // GET: /Admin/Products
        public ActionResult Products(string search = "", int page = 1, int pageSize = 10)
        {
            ViewBag.Search = search;
            
            var query = _db.Products.Include(p => p.Category).AsQueryable();
            
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || 
                                         p.Description.Contains(search) || 
                                         p.SKU.Contains(search));
            }
            
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;
            
            var products = query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            var model = new ProductListViewModel
            {
                Products = products,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                Search = search
            };
            
            return View(model);
        }
        
        // GET: /Admin/CreateProduct
        public ActionResult CreateProduct()
        {
            ViewBag.Categories = GetCategoriesSelectList();
            return View(new ProductViewModel());
        }
        
        // POST: /Admin/CreateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProduct(ProductViewModel model, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    DiscountPrice = model.DiscountPrice,
                    SKU = model.SKU,
                    StockQuantity = model.StockQuantity,
                    IsAvailable = model.IsAvailable,
                    CategoryId = model.CategoryId,
                    CreatedDate = DateTime.UtcNow
                };
                
                // Handle image upload
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(imageFile.FileName);
                    var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                    var uploadPath = Path.Combine(Server.MapPath("~/Content/images/products"), uniqueFileName);
                    
                    // Ensure directory exists
                    var directory = Path.GetDirectoryName(uploadPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    
                    imageFile.SaveAs(uploadPath);
                    product.ImageUrl = $"/Content/images/products/{uniqueFileName}";
                }
                
                _db.Products.Add(product);
                _db.SaveChanges();
                
                TempData["SuccessMessage"] = "Товар успешно создан!";
                return RedirectToAction("Products");
            }
            
            ViewBag.Categories = GetCategoriesSelectList(model.CategoryId);
            return View(model);
        }
        
        // GET: /Admin/EditProduct/5
        public ActionResult EditProduct(int id)
        {
            var product = _db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            
            var model = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                SKU = product.SKU,
                StockQuantity = product.StockQuantity,
                IsAvailable = product.IsAvailable,
                CategoryId = product.CategoryId,
                ImageUrl = product.ImageUrl
            };
            
            ViewBag.Categories = GetCategoriesSelectList(product.CategoryId);
            return View(model);
        }
        
        // POST: /Admin/EditProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct(ProductViewModel model, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                var product = _db.Products.Find(model.Id);
                if (product == null)
                {
                    return HttpNotFound();
                }
                
                product.Name = model.Name;
                product.Description = model.Description;
                product.Price = model.Price;
                product.DiscountPrice = model.DiscountPrice;
                product.SKU = model.SKU;
                product.StockQuantity = model.StockQuantity;
                product.IsAvailable = model.IsAvailable;
                product.CategoryId = model.CategoryId;
                product.UpdatedDate = DateTime.UtcNow;
                
                // Handle image upload
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(imageFile.FileName);
                    var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                    var uploadPath = Path.Combine(Server.MapPath("~/Content/images/products"), uniqueFileName);
                    
                    // Ensure directory exists
                    var directory = Path.GetDirectoryName(uploadPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        var oldImagePath = Server.MapPath($"~{product.ImageUrl}");
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                            catch
                            {
                                // Log error or handle exception
                            }
                        }
                    }
                    
                    imageFile.SaveAs(uploadPath);
                    product.ImageUrl = $"/Content/images/products/{uniqueFileName}";
                }
                
                _db.Entry(product).State = EntityState.Modified;
                _db.SaveChanges();
                
                TempData["SuccessMessage"] = "Товар успешно обновлен!";
                return RedirectToAction("Products");
            }
            
            ViewBag.Categories = GetCategoriesSelectList(model.CategoryId);
            return View(model);
        }
        
        // GET: /Admin/DeleteProduct/5
        public ActionResult DeleteProduct(int id)
        {
            var product = _db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            
            return View(product);
        }
        
        // POST: /Admin/DeleteProduct/5
        [HttpPost, ActionName("DeleteProduct")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProductConfirmed(int id)
        {
            var product = _db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            
            // Delete image if exists
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Server.MapPath($"~{product.ImageUrl}");
                if (System.IO.File.Exists(imagePath))
                {
                    try
                    {
                        System.IO.File.Delete(imagePath);
                    }
                    catch
                    {
                        // Log error or handle exception
                    }
                }
            }
            
            // Check for related order items
            var hasOrderItems = _db.OrderItems.Any(oi => oi.ProductId == id);
            
            if (hasOrderItems)
            {
                // Set product as unavailable instead of deleting
                product.IsAvailable = false;
                product.UpdatedDate = DateTime.UtcNow;
                _db.Entry(product).State = EntityState.Modified;
                
                TempData["WarningMessage"] = "Товар имеет связанные заказы. Он был отмечен как недоступный вместо удаления.";
            }
            else
            {
                _db.Products.Remove(product);
                TempData["SuccessMessage"] = "Товар успешно удален!";
            }
            
            _db.SaveChanges();
            return RedirectToAction("Products");
        }
        
        #endregion
        
        #region Categories
        
        // GET: /Admin/Categories
        public ActionResult Categories()
        {
            var categories = _db.Categories
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToList();
            
            return View(categories);
        }
        
        // GET: /Admin/CreateCategory
        public ActionResult CreateCategory()
        {
            ViewBag.ParentCategories = GetParentCategoriesSelectList();
            return View(new CategoryViewModel());
        }
        
        // POST: /Admin/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCategory(CategoryViewModel model, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                var category = new Category
                {
                    Name = model.Name,
                    Description = model.Description,
                    ParentCategoryId = model.ParentCategoryId,
                    DisplayOrder = model.DisplayOrder,
                    IsActive = model.IsActive,
                    CreatedDate = DateTime.UtcNow
                };
                
                // Handle image upload
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(imageFile.FileName);
                    var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                    var uploadPath = Path.Combine(Server.MapPath("~/Content/images/categories"), uniqueFileName);
                    
                    // Ensure directory exists
                    var directory = Path.GetDirectoryName(uploadPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    
                    imageFile.SaveAs(uploadPath);
                    category.ImageUrl = $"/Content/images/categories/{uniqueFileName}";
                }
                
                _db.Categories.Add(category);
                _db.SaveChanges();
                
                TempData["SuccessMessage"] = "Категория успешно создана!";
                return RedirectToAction("Categories");
            }
            
            ViewBag.ParentCategories = GetParentCategoriesSelectList(model.ParentCategoryId);
            return View(model);
        }
        
        // GET: /Admin/EditCategory/5
        public ActionResult EditCategory(int id)
        {
            var category = _db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            
            var model = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive,
                ImageUrl = category.ImageUrl
            };
            
            // Exclude self from parent categories list
            ViewBag.ParentCategories = GetParentCategoriesSelectList(category.ParentCategoryId, category.Id);
            return View(model);
        }
        
        // POST: /Admin/EditCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCategory(CategoryViewModel model, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                var category = _db.Categories.Find(model.Id);
                if (category == null)
                {
                    return HttpNotFound();
                }
                
                category.Name = model.Name;
                category.Description = model.Description;
                category.ParentCategoryId = model.ParentCategoryId;
                category.DisplayOrder = model.DisplayOrder;
                category.IsActive = model.IsActive;
                
                // Handle image upload
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(imageFile.FileName);
                    var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                    var uploadPath = Path.Combine(Server.MapPath("~/Content/images/categories"), uniqueFileName);
                    
                    // Ensure directory exists
                    var directory = Path.GetDirectoryName(uploadPath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(category.ImageUrl))
                    {
                        var oldImagePath = Server.MapPath($"~{category.ImageUrl}");
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                            catch
                            {
                                // Log error or handle exception
                            }
                        }
                    }
                    
                    imageFile.SaveAs(uploadPath);
                    category.ImageUrl = $"/Content/images/categories/{uniqueFileName}";
                }
                
                _db.Entry(category).State = EntityState.Modified;
                _db.SaveChanges();
                
                TempData["SuccessMessage"] = "Категория успешно обновлена!";
                return RedirectToAction("Categories");
            }
            
            ViewBag.ParentCategories = GetParentCategoriesSelectList(model.ParentCategoryId, model.Id);
            return View(model);
        }
        
        // GET: /Admin/DeleteCategory/5
        public ActionResult DeleteCategory(int id)
        {
            var category = _db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            
            return View(category);
        }
        
        // POST: /Admin/DeleteCategory/5
        [HttpPost, ActionName("DeleteCategory")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCategoryConfirmed(int id)
        {
            var category = _db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            
            // Check for child categories
            var hasChildCategories = _db.Categories.Any(c => c.ParentCategoryId == id);
            
            // Check for products in this category
            var hasProducts = _db.Products.Any(p => p.CategoryId == id);
            
            if (hasChildCategories || hasProducts)
            {
                TempData["ErrorMessage"] = "Нельзя удалить категорию, которая содержит товары или подкатегории.";
                return RedirectToAction("Categories");
            }
            
            // Delete image if exists
            if (!string.IsNullOrEmpty(category.ImageUrl))
            {
                var imagePath = Server.MapPath($"~{category.ImageUrl}");
                if (System.IO.File.Exists(imagePath))
                {
                    try
                    {
                        System.IO.File.Delete(imagePath);
                    }
                    catch
                    {
                        // Log error or handle exception
                    }
                }
            }
            
            _db.Categories.Remove(category);
            _db.SaveChanges();
            
            TempData["SuccessMessage"] = "Категория успешно удалена!";
            return RedirectToAction("Categories");
        }
        
        #endregion
        
        #region Orders
        
        // GET: /Admin/Orders
        public ActionResult Orders(string status = "", int page = 1, int pageSize = 10)
        {
            var query = _db.Orders.Include(o => o.User).AsQueryable();
            
            if (!string.IsNullOrEmpty(status))
            {
                OrderStatus orderStatus;
                if (Enum.TryParse(status, out orderStatus))
                {
                    query = query.Where(o => o.Status == orderStatus);
                }
            }
            
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;
            
            var orders = query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            var model = new OrderListViewModel
            {
                Orders = orders,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                Status = status
            };
            
            return View(model);
        }
        
        // GET: /Admin/OrderDetails/5
        public ActionResult OrderDetails(int id)
        {
            var order = _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Include(o => o.OrderItems.Select(oi => oi.Product))
                .Include(o => o.BillingAddress)
                .Include(o => o.ShippingAddress)
                .FirstOrDefault(o => o.Id == id);
                
            if (order == null)
            {
                return HttpNotFound();
            }
            
            return View(order);
        }
        
        // POST: /Admin/UpdateOrderStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var order = _db.Orders.Find(orderId);
            if (order == null)
            {
                return HttpNotFound();
            }
            
            order.Status = status;
            order.ShippingDate = DateTime.UtcNow;
            _db.SaveChanges();
            
            TempData["SuccessMessage"] = "Статус заказа успешно обновлен!";
            return RedirectToAction("OrderDetails", new { id = orderId });
        }
        
        #endregion
        
        #region Users
        
        // GET: /Admin/Users
        public ActionResult Users(string search = "", int page = 1, int pageSize = 10)
        {
            var query = _db.Users.AsQueryable();
            
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.Email.Contains(search) || 
                                         u.FirstName.Contains(search) || 
                                         u.LastName.Contains(search));
            }
            
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;
            
            var users = query
                .OrderByDescending(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            var model = new UserListViewModel
            {
                Users = users,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                Search = search
            };
            
            return View(model);
        }
        
        // GET: /Admin/UserDetails/5
        public ActionResult UserDetails(int id)
        {
            var user = _db.Users
                .Include(u => u.Orders)
                .Include(u => u.Addresses)
                .FirstOrDefault(u => u.Id == id);
                
            if (user == null)
            {
                return HttpNotFound();
            }
            
            return View(user);
        }
        
        // POST: /Admin/UpdateUserStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateUserStatus(int userId, string isActive)
        {
            var user = _db.Users.Find(userId);
            if (user == null)
            {
                return HttpNotFound();
            }
            
            // Преобразование строки в bool
            bool isActiveValue;
            if (bool.TryParse(isActive, out isActiveValue))
            {
                user.IsActive = isActiveValue;
                _db.SaveChanges();
                
                TempData["SuccessMessage"] = isActiveValue ? "Пользователь успешно активирован!" : "Пользователь успешно заблокирован!";
            }
            else
            {
                TempData["ErrorMessage"] = "Ошибка при изменении статуса пользователя. Неверный формат данных.";
            }
            
            return RedirectToAction("UserDetails", new { id = userId });
        }
        
        #endregion
        
        #region Helper Methods
        
        private SelectList GetCategoriesSelectList(int? selectedCategoryId = null)
        {
            var categories = _db.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToList();
            
            return new SelectList(categories, "Id", "Name", selectedCategoryId);
        }
        
        private SelectList GetParentCategoriesSelectList(int? selectedCategoryId = null, int? excludeCategoryId = null)
        {
            var categories = _db.Categories
                .Where(c => c.IsActive && (excludeCategoryId == null || c.Id != excludeCategoryId.Value))
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToList();
            
            // Add empty option for root categories
            var selectList = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Корневая категория --" }
            };
            
            selectList.AddRange(categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = selectedCategoryId.HasValue && c.Id == selectedCategoryId.Value
            }));
            
            return new SelectList(selectList, "Value", "Text", selectedCategoryId);
        }
        
        #endregion
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
    
    public class AdminDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public List<Order> RecentOrders { get; set; }
        public List<Product> TopSellingProducts { get; set; }
    }
    
    public class ProductListViewModel
    {
        public List<Product> Products { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }
    }
    
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public string SKU { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public int CategoryId { get; set; }
        public string ImageUrl { get; set; }
        
        public ProductViewModel()
        {
            IsAvailable = true;
            StockQuantity = 0;
        }
    }
    
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public string ImageUrl { get; set; }
        
        public CategoryViewModel()
        {
            IsActive = true;
            DisplayOrder = 0;
        }
    }
    
    public class OrderListViewModel
    {
        public List<Order> Orders { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public string Status { get; set; }
        
        public OrderListViewModel()
        {
            Orders = new List<Order>();
        }
    }
    
    public class UserListViewModel
    {
        public List<User> Users { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }
        
        public UserListViewModel()
        {
            Users = new List<User>();
        }
    }
} 