using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeApi.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } // наприклад, логін по email або довільний

        [Required]
        public string PasswordHash { get; set; } // Хеш пароля

        [Required]
        public string Role { get; set; } // "Administrator" або "Employee"

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
}
