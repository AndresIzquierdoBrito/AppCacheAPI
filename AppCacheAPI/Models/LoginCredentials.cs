using System.ComponentModel.DataAnnotations;

namespace AppCacheAPI.Models
{
    public class LoginCredentials
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
