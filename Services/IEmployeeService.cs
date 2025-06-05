using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeApi.Models;

namespace EmployeeApi.Services
{
    public interface IEmployeeService
    {
        Task<Employee> GetByIdAsync(int id);
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee> CreateAsync(CreateEmployeeRequest request);
        Task UpdateAsync(int id, UpdateEmployeeRequest request, int requesterId, string requesterRole);
        Task DeleteAsync(int id);
    }
}
