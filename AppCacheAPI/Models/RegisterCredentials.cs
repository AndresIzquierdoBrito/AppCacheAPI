using System.ComponentModel.DataAnnotations;

namespace AppCacheAPI.Models
{
    public class RegisterCredentials
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
