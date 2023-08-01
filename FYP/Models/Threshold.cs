using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FYP.Models;

public class Threshold

{

    //Define Threshold for Temp, Humid, CO2 accordingly
    public int ATT { get; set; }
    public int BTT { get; set; }
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

}
