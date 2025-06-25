using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeApi.Models
{
    public class Employee
    {
        public int Id { get; set; }

        // Ім'я, прізвище, посада, дата народження, контактні дані
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Position { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        // Зв'язок до User (для аутентифікації)
        public User? User { get; set; }
    }
}
