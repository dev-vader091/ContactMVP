using System.ComponentModel.DataAnnotations;

namespace ContactMVP.Models
{
    public class Category
    {
        // Primary Key
        public int Id { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        public string? Name { get; set; }

        // Foreign Key 
        [Required]
        public string? AppUserId { get; set; }

        // Navigation Properties
        public virtual AppUser? AppUser { get; set; }

        public virtual ICollection<Contact> Contacts { get;set; } = new HashSet<Contact>();    

    }
}
