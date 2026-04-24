using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FastsFood.ViewModel.FastsFood.Models
{
    public class ItemVm
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Image URL is required")]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public double Price { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "SubCategory is required")]
        public int SubCategoryId { get; set; }

        // For image upload
        public IFormFile ImageFile { get; set; }
    }
}