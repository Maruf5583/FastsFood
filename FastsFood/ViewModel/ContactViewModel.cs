using System.ComponentModel.DataAnnotations;

namespace FastsFood.ViewModel
{
    public class ContactViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        [Phone]
        public string Phone { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; }
    }

}
