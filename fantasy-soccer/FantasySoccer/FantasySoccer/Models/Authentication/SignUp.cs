using System.ComponentModel.DataAnnotations;

namespace FantasySoccer.Models.Authentication
{
    public class SignUp
    {
        [Required(ErrorMessage = "Enter a valid Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Display name is required")]
        [Display(Name = "Display Name")]
        [StringLength(25, ErrorMessage = "Display name must have lest than 25 characters")]
        public string DisplayName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(16, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 16 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Password confirmation is required")]
        [Display(Name = "Password Confirmation")]
        [DataType(DataType.Password)]
        [StringLength(16, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 16 characters")]
        [Compare("Password", ErrorMessage = "Passwords should be the same")]
        public string PasswordConfirmation { get; set; }
    }
}
