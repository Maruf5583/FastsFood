using FastsFood.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FastsFood.ViewModel.FastsFood.Models
{
    public class CuponVm
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Cupon type is required")]
        public CuponType Type { get; set; }

        [Required(ErrorMessage = "Discount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Discount must be greater than 0")]
        public double Discount { get; set; }

        [Required(ErrorMessage = "Minimum amount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Minimum amount must be 0 or greater")]
        public double MinimumAmount { get; set; }

        // For image upload
        public IFormFile CuponImageFile { get; set; }

        // Display existing image
        public string ExistingImageBase64 { get; set; }

        public bool IsActive { get; set; } = true;
    }
}