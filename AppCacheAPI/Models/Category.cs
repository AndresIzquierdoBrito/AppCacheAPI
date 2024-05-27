using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppCacheAPI.Models
{
    [Table("category")]
    public class Category
    {
        [Key]
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Required]
        [Column("title")]
        [StringLength(255)]
        public string Title { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("color")]
        [StringLength(50)]
        public string? Color { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<CategoryIdea> CategoryIdeas { get; set; }
    }

}
