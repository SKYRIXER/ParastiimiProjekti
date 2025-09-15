using System.ComponentModel.DataAnnotations;

namespace db_testiä.Models
{
    public class UserCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Age { get; set; }

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
