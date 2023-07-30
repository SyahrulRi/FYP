using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FYP.Models;

public class TempHumid
{
    public double Temperature { get; set; }
    public double Humidity { get; set; }

    public double CO2 { get; set; }

    public string ID { get; set; } = null!;

    public DateTime DataInsert { get; set; }
}