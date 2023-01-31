using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactMVP.Models
{
    public class AppUser : IdentityUser 
    {
        // [Required --] allows for code to continue running w/null values
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long", MinimumLength = 2)]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long", MinimumLength = 2)]
        public string? LastName { get; set; }

        // [NotMapped] -- makes it so property not tracked in DB
        [NotMapped]
        public string? FullName { get { return $"{FirstName} {LastName}"; } }

        // Navigation Properties
        public virtual ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();
        public virtual ICollection<Category> Categories { get; set; } = new HashSet<Category>();



    }
}
