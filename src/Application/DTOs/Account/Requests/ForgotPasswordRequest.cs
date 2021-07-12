using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Account.Requests
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
