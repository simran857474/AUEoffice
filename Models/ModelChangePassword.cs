using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Eoffice.Models
{
    public class ModelChangePassword
    {
            [Required]
            public int UserId { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string CurrentPassword { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
            [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).+$", ErrorMessage = "Password must include uppercase, lowercase, and number.")]
            public string NewPassword { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "Confirm password does not match.")]
            public string ConfirmPassword { get; set; }

        public string Password { get; set; }

        public DateTime ChangeDate { get; set; }
    }
}