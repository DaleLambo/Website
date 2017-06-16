using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LoginAndRegistration.Models
{
    public class UserLogin
    {
        [Display(Name = "Email")] // Display name for enities
        [Required(AllowEmptyStrings = false, ErrorMessage = "*Email ID is required")] // Validation for required inputs
        public string EmailID { get; set; }

        [Display(Name = "Password")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "*Password is required")]
        [DataType(DataType.Password)] // Specifies type for data field
        public string Password { get; set; }

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}