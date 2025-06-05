using System.Threading.Tasks;
using EmployeeApi.Models;

namespace EmployeeApi.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByIdAsync(int id);
        Task<User> CreateAsync(User user);
    }
}
