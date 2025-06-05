using System;
using System.Threading.Tasks;
using BCrypt.Net;
using EmployeeApi.Models;
using EmployeeApi.Repositories;
using EmployeeApi.Utils;

namespace EmployeeApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly SessionManager _sessionManager;

        public AuthService(IUserRepository userRepo, SessionManager sessionManager)
        {
            _userRepo = userRepo;
            _sessionManager = sessionManager;
        }

        public async Task<string> LoginAsync(LoginRequest request)
        {
            var user = await _userRepo.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                throw new ArgumentException("Неверный логин или пароль");
            }

            // Сравниваем bcrypt-хеш
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new ArgumentException("Неверный логин или пароль");
            }

            // Генерируем GUID-токен (сессионный)
            var token = Guid.NewGuid().ToString();

            // Сохраняем в Redis: ключ = token, значение = UserId + Role
            await _sessionManager.CreateSessionAsync(token, user.Id, user.Role);

            return token;
        }

        public async Task LogoutAsync(string token)
        {
            await _sessionManager.DeleteSessionAsync(token);
        }
    }
}
