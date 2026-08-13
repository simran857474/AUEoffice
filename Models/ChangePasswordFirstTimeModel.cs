using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eoffice.Models
{
    public class ChangePasswordFirstTimeModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string OTP { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string RandomOTP { get; set; }
    }
}
