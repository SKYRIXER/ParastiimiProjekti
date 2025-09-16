using System.ComponentModel.DataAnnotations;

namespace db_testiä.Models
{
    public class LoginRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
