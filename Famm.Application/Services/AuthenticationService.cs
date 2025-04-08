using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Famm.Database.Context;
using Famm.Domain.Models;
using AutoMapper;

namespace Famm.Application.Services
{
    /// <summary>
    /// Сервис для аутентификации и регистрации пользователей
    /// </summary>
    public class UserDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool RememberMe { get; set; }
        public bool IsActiveUser { get; set; }
        public DateTime LastLogin { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class RegisterRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class AuthResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UserDTO User { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class AuthValidator
    {
        public bool ValidateEmail(string email)
        {
            return !string.IsNullOrEmpty(email) && email.Contains("@");
        }

        public bool ValidatePassword(string password)
        {
            return !string.IsNullOrEmpty(password) && password.Length >= 6;
        }
    }

    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.IsActiveUser, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.LastLogin, opt => opt.MapFrom(src => src.LastLoginDate ?? DateTime.UtcNow))
                .ForMember(dest => dest.RememberMe, opt => opt.Ignore());

            CreateMap<RegisterRequest, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.RegistrationDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Orders, opt => opt.Ignore())
                .ForMember(dest => dest.Addresses, opt => opt.Ignore())
                .ForMember(dest => dest.Cart, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginDate, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore());
        }
    }

    public class AuthenticationService : IDisposable
    {
        private readonly FammDbContext _dbContext;
        private readonly AuthValidator _validator;
        private readonly IMapper _mapper;

        public AuthenticationService()
        {
            _dbContext = new FammDbContext();
            _validator = new AuthValidator();
            
            var config = new MapperConfiguration(cfg => {
                cfg.AddProfile<AutoMapperProfile>();
            });
            _mapper = config.CreateMapper();
        }

        /// <summary>
        /// Аутентифицирует пользователя по email и паролю
        /// </summary>
        /// <returns>User если аутентификация успешна, null если нет</returns>
        public User AuthenticateUser(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;

            // В реальном приложении пароль должен быть хеширован
            var user = _dbContext.Users.FirstOrDefault(u => 
                u.Email.ToLower() == email.ToLower() && 
                u.PasswordHash == password);  // В реальности здесь должна быть проверка хеша

            return user;
        }

        /// <summary>
        /// Регистрирует нового пользователя
        /// </summary>
        /// <returns>Созданный пользователь при успешной регистрации, null при ошибке</returns>
        public User RegisterUser(string firstName, string lastName, string email, string password)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || 
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;

            // Проверяем, существует ли уже пользователь с таким email
            if (_dbContext.Users.Any(u => u.Email.ToLower() == email.ToLower()))
                return null;

            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = password, // В реальности здесь должен быть хеш пароля
                RegistrationDate = DateTime.UtcNow,
                IsActive = true
            };

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            return user;
        }

        public async Task<AuthResultDTO> LoginAsync(LoginRequest request)
        {
            var result = new AuthResultDTO();

            try
            {
                if (!_validator.ValidateEmail(request.Email))
                {
                    result.Errors.Add("Invalid email format");
                    return result;
                }

                if (!_validator.ValidatePassword(request.Password))
                {
                    result.Errors.Add("Password must be at least 6 characters");
                    return result;
                }

                var user = await Task.FromResult(_dbContext.Users.FirstOrDefault(u =>
                    u.Email.ToLower() == request.Email.ToLower() &&
                    u.PasswordHash == request.Password));

                if (user != null)
                {
                    user.LastLoginDate = DateTime.UtcNow;
                    _dbContext.SaveChanges();

                    result.Success = true;
                    result.User = _mapper.Map<UserDTO>(user);
                    result.User.RememberMe = request.RememberMe;
                    result.Message = "Login successful";
                }
                else
                {
                    result.Success = false;
                    result.Message = "Invalid credentials";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "An error occurred during login";
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public async Task<AuthResultDTO> RegisterAsync(RegisterRequest request)
        {
            var result = new AuthResultDTO();

            try
            {
                if (!_validator.ValidateEmail(request.Email))
                {
                    result.Errors.Add("Invalid email format");
                    return result;
                }

                if (!_validator.ValidatePassword(request.Password))
                {
                    result.Errors.Add("Password must be at least 6 characters");
                    return result;
                }

                var existingUser = await Task.FromResult(_dbContext.Users.Any(u => 
                    u.Email.ToLower() == request.Email.ToLower()));
                
                if (existingUser)
                {
                    result.Success = false;
                    result.Message = "A user with this email already exists";
                    return result;
                }

                var newUser = _mapper.Map<User>(request);
                _dbContext.Users.Add(newUser);
                await Task.FromResult(_dbContext.SaveChanges());

                result.Success = true;
                result.User = _mapper.Map<UserDTO>(newUser);
                result.Message = "Registration successful";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "An error occurred during registration";
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Освобождает ресурсы
        /// </summary>
        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
} 