using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabiMarket.Application.DTOs.Requests
{
    // DTOs for the password reset flow
    public class ForgotPasswordDto
    {
        public string? PhoneNumber { get; set; }

        public string? EmailAddress { get; set; }       
    }

    public class VerifyOTPDto
    {
        public string? PhoneNumber { get; set; }
        
        public string? EmailAddress { get; set;}

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string OTP { get; set; }
    }

    public class ResetPasswordDto
    {
        public string? PhoneNumber { get; set; }
       
        public string? EmailAddress { get; set;}

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}
