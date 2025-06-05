using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeApi.Models;

namespace EmployeeApi.Repositories
{
    public interface IEmployeeRepository
    {
        Task<Employee> GetByIdAsync(int id);
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee> CreateAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task DeleteAsync(Employee employee);
    }
}
