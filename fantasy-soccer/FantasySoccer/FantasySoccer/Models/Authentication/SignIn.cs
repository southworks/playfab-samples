using System.ComponentModel.DataAnnotations;

namespace FantasySoccer.Models.Authentication
{
    public class SignIn
    {
        [Required(ErrorMessage = "Enter a valid Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(16, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 16 characters")]
        public string Password { get; set; }
    }
}
