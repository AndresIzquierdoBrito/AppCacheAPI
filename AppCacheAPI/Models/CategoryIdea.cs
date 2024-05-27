using System.ComponentModel.DataAnnotations.Schema;

namespace AppCacheAPI.Models
{
    [Table("category_idea")]
    public class CategoryIdea
    {
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("idea_id")]
        public int IdeaId { get; set; }

        [Column("order")]
        public int Order { get; set; }

        public virtual Category Category { get; set; }

        public virtual Idea Idea { get; set; }
    }
}
