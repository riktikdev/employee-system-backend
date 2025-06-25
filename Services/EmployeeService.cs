using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BCrypt.Net;
using EmployeeApi.Models;
using EmployeeApi.Repositories;

namespace EmployeeApi.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IUserRepository _userRepo;

        public EmployeeService(IEmployeeRepository employeeRepo, IUserRepository userRepo)
        {
            _employeeRepo = employeeRepo;
            _userRepo = userRepo;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _employeeRepo.GetAllAsync();
        }

        public async Task<Employee> GetByIdAsync(int id)
        {
            return await _employeeRepo.GetByIdAsync(id);
        }

        public async Task<Employee> CreateAsync(CreateEmployeeRequest request)
        {
            var employee = new Employee
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Position = request.Position,
                DateOfBirth = request.DateOfBirth,
                Email = request.Email,
                Phone = request.Phone
            };
            var createdEmployee = await _employeeRepo.CreateAsync(employee);

            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                EmployeeId = createdEmployee.Id
            };
            await _userRepo.CreateAsync(user);

            return createdEmployee;
        }

        public async Task UpdateAsync(int id, UpdateEmployeeRequest request, int requesterId, string requesterRole)
        {
            if (requesterRole != "Administrator" && requesterId != id)
            {
                throw new UnauthorizedAccessException("You do not have editing rights");
            }

            var existing = await _employeeRepo.GetByIdAsync(id);
            if (existing == null)
            {
                throw new ArgumentException("This employee was not found.");
            }

            existing.FirstName = request.FirstName;
            existing.LastName = request.LastName;
            existing.Position = request.Position;
            existing.DateOfBirth = request.DateOfBirth;
            existing.Email = request.Email;
            existing.Phone = request.Phone;

            await _employeeRepo.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _employeeRepo.GetByIdAsync(id);
            if (existing == null)
            {
                throw new ArgumentException("This employee was not found.");
            }
            await _employeeRepo.DeleteAsync(existing);
        }
    }
}
