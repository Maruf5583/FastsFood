using System.ComponentModel.DataAnnotations;

namespace FastsFood.ViewModel
{
    public class CategoryVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category Title is required")]
        [Display(Name = "Category Title")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 100 characters")]
        public string Title { get; set; }

       
    }
}
