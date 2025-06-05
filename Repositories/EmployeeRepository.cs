using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EmployeeApi.Data;
using EmployeeApi.Models;

namespace EmployeeApi.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _db;

        public EmployeeRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Employee> GetByIdAsync(int id)
        {
            return await _db.Employees
                            .AsNoTracking()
                            .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _db.Employees
                            .AsNoTracking()
                            .ToListAsync();
        }

        public async Task<Employee> CreateAsync(Employee employee)
        {
            var entry = await _db.Employees.AddAsync(employee);
            await _db.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task UpdateAsync(Employee employee)
        {
            _db.Employees.Update(employee);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Employee employee)
        {
            _db.Employees.Remove(employee);
            await _db.SaveChangesAsync();
        }
    }
}
