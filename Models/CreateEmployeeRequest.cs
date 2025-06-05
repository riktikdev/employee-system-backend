namespace EmployeeApi.Models
{
    public class CreateEmployeeRequest
    {
        // Поля Employee
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Position { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        // Поля для User
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
