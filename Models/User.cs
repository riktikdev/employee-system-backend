using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeApi.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } // например, логин по email или произвольный

        [Required]
        public string PasswordHash { get; set; } // хеш пароля

        [Required]
        public string Role { get; set; } // "Administrator" или "Employee"

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
}
