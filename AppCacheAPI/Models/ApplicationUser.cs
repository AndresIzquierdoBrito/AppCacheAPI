using Microsoft.AspNetCore.Identity;

namespace AppCacheAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsGoogleUser { get; set; }

        public virtual ICollection<Idea>? Ideas { get; set; }

    }
}
