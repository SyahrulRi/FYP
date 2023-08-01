using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FYP.Models;

public class SysUser

{
    [Required(ErrorMessage = "Please enter User ID")]
    [Remote(action: "VerifyUserID", controller: "Account")]
    public string UserId { get; set; } = null!;

    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Please enter Password")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "Password must be 5 characters or more")]
    public string UserPw { get; set; } = null!;

    [DataType(DataType.Password)]
    [Compare("UserPw", ErrorMessage = "Passwords do not match")]
    public string UserPw2 { get; set; } = null!;

    [Required(ErrorMessage = "Please enter Full Name")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Please enter Email")]
    [EmailAddress(ErrorMessage = "Invalid Email")]
    public string Email { get; set; } = null!;

    public string UserRole { get; set; } = null!;

    public bool RememberMe { get; set; }

}
