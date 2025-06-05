using System.Threading.Tasks;
using EmployeeApi.Models;

namespace EmployeeApi.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(LoginRequest request);
        Task LogoutAsync(string token);
    }
}
