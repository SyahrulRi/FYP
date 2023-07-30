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

    //Define Threshold for Temp, Humid, CO2 accordingly
    public int ATT { get; set; }
    public int BTT { get; set; }
    [Range(0, 100, ErrorMessage = "0-100% Humidity")]
    public int AHT { get; set; }
    [Range(0, 100, ErrorMessage = "0-100% Humidity")]
    public int BHT { get; set; }
    [Range(0, 5000, ErrorMessage = "0-5000 CO2 ppm")]
    public int ACOT { get; set; }
    [Range(0, 5000, ErrorMessage = "0-5000 CO2 ppm")]
    public int BCOT { get; set; }
    [Range(1, 24, ErrorMessage = "1-24 Time Interval (Hr)")]
    public int TimeIntervalTH { get; set; }
    [Range(1, 24, ErrorMessage = "1-24 Time Interval (Hr)")]
    public int TimeIntervalCO2 { get; set; }
    public DateTime SendEmailTH { get; set; }
    public DateTime SendEmailCO2 { get; set; }
    public bool RememberMe { get; set; }
}
