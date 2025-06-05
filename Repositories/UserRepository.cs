using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EmployeeApi.Data;
using EmployeeApi.Models;

namespace EmployeeApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _db.Users
                            .Include(u => u.Employee)
                            .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _db.Users
                            .Include(u => u.Employee)
                            .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> CreateAsync(User user)
        {
            var entry = await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
            return entry.Entity;
        }
    }
}
