using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EmployeeApi.Models;
using EmployeeApi.Services;
using EmployeeApi.Utils;
using Serilog;

namespace EmployeeApi.Controllers
{
    [ApiController]
    [Route("employees")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _empService;
        private readonly RedisCache _cache;

        // Ключи кеширования
        private const string ALL_EMPLOYEES_CACHE_KEY = "employees:all";
        private const string EMPLOYEE_CACHE_PREFIX = "employee:";

        public EmployeeController(IEmployeeService empService, RedisCache cache)
        {
            _empService = empService;
            _cache = cache;
        }

        /// <summary>
        /// Получить всех сотрудников (только для Administrator)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var role = HttpContext.Items["UserRole"] as string;
            if (role != "Administrator")
                return Forbid("Только администратор может получить список всех сотрудников");

            // Пытаемся взять из кеша
            var cached = await _cache.GetAsync<IEnumerable<EmployeeDto>>(ALL_EMPLOYEES_CACHE_KEY);
            if (cached != null)
            {
                return Ok(cached);
            }

            var employees = await _empService.GetAllAsync();
            var dtos = employees.Select(e => new EmployeeDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Position = e.Position,
                DateOfBirth = e.DateOfBirth,
                Email = e.Email,
                Phone = e.Phone
            }).ToList();

            // Сохраняем в кеш на 5 минут
            await _cache.SetAsync(ALL_EMPLOYEES_CACHE_KEY, dtos, TimeSpan.FromMinutes(5));

            return Ok(dtos);
        }

        /// <summary>
        /// Получить конкретного сотрудника (доступен и для Employee — только о себе, и для Admin)
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = HttpContext.Items["UserRole"] as string;
            var userId = (int)HttpContext.Items["UserId"];

            if (role != "Administrator" && userId != id)
                return Forbid("Нет прав доступа к данным этого пользователя");

            // Пытаемся взять из кеша
            var cacheKey = $"{EMPLOYEE_CACHE_PREFIX}{id}";
            var cached = await _cache.GetAsync<EmployeeDto>(cacheKey);
            if (cached != null)
            {
                return Ok(cached);
            }

            var employee = await _empService.GetByIdAsync(id);
            if (employee == null)
            {
                return NotFound("Сотрудник не найден");
            }

            var dto = new EmployeeDto
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Position = employee.Position,
                DateOfBirth = employee.DateOfBirth,
                Email = employee.Email,
                Phone = employee.Phone
            };

            // Кешируем на 5 минут
            await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));

            return Ok(dto);
        }

        /// <summary>
        /// Добавить нового сотрудника (только для Administrator)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request)
        {
            var role = HttpContext.Items["UserRole"] as string;
            if (role != "Administrator")
                return Forbid("Only administrator can add employees");

            try
            {
                var newEmployee = await _empService.CreateAsync(request);

                // Сбрасываем кеш списка
                await _cache.RemoveAsync(ALL_EMPLOYEES_CACHE_KEY);

                var dto = new EmployeeDto
                {
                    Id = newEmployee.Id,
                    FirstName = newEmployee.FirstName,
                    LastName = newEmployee.LastName,
                    Position = newEmployee.Position,
                    DateOfBirth = newEmployee.DateOfBirth,
                    Email = newEmployee.Email,
                    Phone = newEmployee.Phone
                };
                return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при создании сотрудника");
                return StatusCode(500, "Внутренняя ошибка при создании сотрудника");
            }
        }

        /// <summary>
        /// Оновити дані співробітника (Admin або сам співробітник)
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeRequest request)
        {
            var role = HttpContext.Items["UserRole"] as string;
            var userId = (int)HttpContext.Items["UserId"];

            try
            {
                await _empService.UpdateAsync(id, request, userId, role);

                // Сбрасываем кеш
                await _cache.RemoveAsync(ALL_EMPLOYEES_CACHE_KEY);
                await _cache.RemoveAsync($"{EMPLOYEE_CACHE_PREFIX}{id}");

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("Недостаточно прав");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating employee");
                return StatusCode(500, "Internal error updating employee");
            }
        }

        /// <summary>
        /// Видалити співробітника (тільки Administrator)
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = HttpContext.Items["UserRole"] as string;
            if (role != "Administrator")
                return Forbid("Only the administrator can delete employees");

            try
            {
                await _empService.DeleteAsync(id);

                // Скидаємо кеш
                await _cache.RemoveAsync(ALL_EMPLOYEES_CACHE_KEY);
                await _cache.RemoveAsync($"{EMPLOYEE_CACHE_PREFIX}{id}");

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while deleting employee");
                return StatusCode(500, "Internal error deleting employee");
            }
        }
    }
}
