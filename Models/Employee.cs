using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeApi.Models
{
    public class Employee
    {
        public int Id { get; set; }

        // Имя, фамилия, должность, дата рождения, контактные данные
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Position { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        // Связь к User (для аутентификации)
        public User? User { get; set; }
    }
}
