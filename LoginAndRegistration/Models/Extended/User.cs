using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace LoginAndRegistration.Models
{
    [MetadataType(typeof(UserMetadata))] // Specifies the metadata class to the corresponding data model
    public partial class User
    {
        public string ConfirmPassword { get; set; } // Additional field not in model only displays in Registration page
    }

    public class UserMetadata
    {
        [Display(Name = "First Name")] // Display name for enities
        [Required(AllowEmptyStrings = false, ErrorMessage = "*First name is required")] // Validation for required inputs
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "*Last name is required")]
        public string LastName { get; set; }

        [Display(Name = "Email ID")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "*Email Id is required")]
        [DataType(DataType.EmailAddress)] // Specifies type for data field
        public string EmailID { get; set; }

        [Display(Name = "Date of birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")] // Provide specific format for data fields
        public DateTime DateOfBirth { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "*Password is required")]
        [DataType(DataType.Password)]
        [MinLength(6,ErrorMessage = "Minimum of 6 characters required")] // Sets minimum length for data field
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage = "*Confirm password does not match password")] // Compares data field with password
        public string ConfirmPassword { get; set; }
    }
}