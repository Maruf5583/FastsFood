using System.ComponentModel.DataAnnotations;

namespace FastsFood.ViewModel
{
    public class ResetPasswordVm
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, ErrorMessage = "Password must be at least 6 characters long", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmNewPassword { get; set; }
    }
}
