using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using DocumentFormat.OpenXml.Spreadsheet;


namespace FYP.Controllers;

public class MonitoringController : Controller
{
    public string SQLName = "";

    [AllowAnonymous]
    public IActionResult About()
    {
        return View();
    }
    [Authorize(Roles = "admin, farmer")]
    public IActionResult Dashboard()
    {
        return View();
    }

    [Authorize(Roles = "admin, farmer")]
    public IActionResult TempChart()
    {
        List<DataPoint> TData = new List<DataPoint>();

        // Read data into list
        List<TempHumid> Temp = LoadData(1);

        for (int i = Temp.Count - 30; i < Temp.Count; i++)
        {
            TData.Add(new DataPoint(Temp[i].DataInsert.ToString(), Temp[i].Temperature));
        }

        ViewBag.DataPoints1 = JsonConvert.SerializeObject(TData);

        //-----------------------------------------

        //Second query to get Threshold from Sysuser
        List<Threshold> UserTH = DBUtl.GetList<Threshold>("SELECT * FROM SysUser where FullName = '" + User.Identity!.Name + "'");
        string name = "" + User.Identity!.Name;

        int count = Temp.Count;
        if (Temp[count - 1].Temperature > UserTH[0].ATT || Temp[count - 1].Temperature < UserTH[0].BTT)
        {

            TempData["Message"] = "Threshold Exceeded/Subceed. Temperature Threshold: " + UserTH[0].BTT + "-" + UserTH[0].ATT + "°C. Current Temperature: " + Temp[count - 1].Temperature + "°C";
            TempData["MsgType"] = "danger";

            sendEmail(name, 0);
        }
        else
        {
            TempData["Message"] = "Within Threshold. Temperature Threshold: " + UserTH[0].BTT + "-" + UserTH[0].ATT + "°C.";
            TempData["MsgType"] = "success";
        }

        // Pass data to view
        return View();
    }

    [Authorize(Roles = "admin, farmer")]
    public IActionResult HumidChart()
    {
        List<DataPoint> HData = new List<DataPoint>();

        // Read data into list
        List<TempHumid> Humid = LoadData(1);

        for (int i = Humid.Count - 30; i < Humid.Count; i++)
        {
            HData.Add(new DataPoint(Humid[i].DataInsert.ToString(), Humid[i].Humidity));
        }

        ViewBag.DataPoints2 = JsonConvert.SerializeObject(HData);

        List<Threshold> UserTH = DBUtl.GetList<Threshold>("SELECT * FROM SysUser where FullName = '" + User.Identity!.Name + "'");
        string name = "" + User.Identity!.Name;
        int count = Humid.Count;

        if (Humid[count - 1].Humidity > UserTH[0].AHT || Humid[count - 1].Humidity < UserTH[0].BHT)
        {
            TempData["Message"] = "Threshold Exceeded/Subceed. Humidity Threshold: " + UserTH[0].BHT + "-" + UserTH[0].AHT + "%. Current Humidity: " + Humid[count - 1].Humidity + "%";
            TempData["MsgType"] = "danger";

            sendEmail(name, 1);
        }
        else
        {
            TempData["Message"] = "Within Threshold. Humidity Threshold: " + UserTH[0].BHT + "-" + UserTH[0].AHT + "%";
            TempData["MsgType"] = "success";
        }

        // Pass data to view
        return View();
    }

    [Authorize(Roles = "admin, farmer")]
    public IActionResult CO2Chart()
    {
        List<DataPoint> CData = new List<DataPoint>();

        List<TempHumid> CO2 = LoadData(2);

        for (int i = CO2.Count - 30; i < CO2.Count; i++)
        {
            CData.Add(new DataPoint(CO2[i].DataInsert.ToString(), CO2[i].CO2));
        }

        ViewBag.DataPoints3 = JsonConvert.SerializeObject(CData);

        string name = "" + User.Identity!.Name;
        //Second query to get Threshold from Sysuser
        List<Threshold> UserTH = DBUtl.GetList<Threshold>("SELECT * FROM SysUser where FullName = '" + User.Identity!.Name + "'");
        int count = CO2.Count;
        if (CO2[count - 1].CO2 > UserTH[0].ACOT || CO2[count - 1].CO2 < UserTH[0].BCOT)
        {
            TempData["Message"] = "Threshold Exceeded/Subceed. CO2 Levels Threshold: " + UserTH[0].BCOT + "ppm - " + UserTH[0].ACOT + "ppm. Current CO2 Levels: " + CO2[count - 1].CO2 + "ppm";
            TempData["MsgType"] = "danger";
            sendEmail(name, 2);
        }
        else
        {
            TempData["Message"] = "Within Threshold. Current Threshold: " + UserTH[0].BCOT + "ppm - " + UserTH[0].ACOT + "ppm. Current CO2 Levels: " + CO2[count - 1].CO2 + "ppm";
            TempData["MsgType"] = "success";
        }

        // Pass data to view
        return View();
    }

    private static List<TempHumid> LoadData(int chart)
    {
        if (chart == 1)
        {
            MySqlConnection conn = new MySqlConnection("server=localhost;uid=root;pwd=;database=esp32");
            MySqlCommand query = new MySqlCommand("SELECT * FROM dht22", conn);

            conn.Open();
            MySqlDataReader execute = query.ExecuteReader();

            // Read data into list
            List<TempHumid> THData = new List<TempHumid>();

            while (execute.Read())
            {
                THData.Add(new TempHumid()
                {
                    Temperature = execute.GetInt32("temperature"),
                    Humidity = execute.GetInt32("humidity"),
                    DataInsert = execute.GetDateTime("datetime"),
                });
            }
            conn.Close();
            return (THData);
        }
        else
        {
            MySqlConnection conn = new MySqlConnection("server=localhost;uid=root;pwd=;database=esp32");
            MySqlCommand query = new MySqlCommand("SELECT * FROM co2", conn);

            conn.Open();
            MySqlDataReader execute = query.ExecuteReader();

            // Read data into list
            List<TempHumid> CData = new List<TempHumid>();

            while (execute.Read())
            {
                CData.Add(new TempHumid()
                {
                    CO2 = execute.GetInt32("CO2"),
                    DataInsert = execute.GetDateTime("datetime"),
                });
            }
            conn.Close();
            return (CData);
        }
    }
    private static void sendEmail(string name, int option)
    {
        //query to get Threshold from Sysuser
        List<Threshold> TH = DBUtl.GetList<Threshold>("SELECT * FROM SysUser where FullName = '" + name + "'");
        List<SysUser> User = DBUtl.GetList<SysUser>("SELECT * FROM SysUser where FullName = '" + name + "'");
        // Template
        string subject = "Warning";
        string time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt");

        string template = "Dear {0} \n\r" +
                          "<p>Sensor has detected the {1} Threshold has been breach. </p> \n\r" +
                          "<p>Current Threshold: {2}-{3}°C</p> \n\r" +
                          "<p>EnviRo</p>";

        if (option == 0)
        {
            if (TH[0].EmailTemp.AddHours(TH[0].TimeIntervalTemp) >= DateTime.Now)
            {
                return;
            }
            else
            {
                string body = String.Format(template, name, "Temperature", TH[0].BTT, TH[0].ATT);
                DBUtl.ExecSQL("UPDATE SysUser SET EmailTemp = '" + time + "' WHERE FullName = '" + name + "'");
                EmailUtl.SendEmail(User[0].Email, subject, body, out string result);
            }
        }
        else if (option == 1)
        {
            if (TH[0].EmailHumid.AddHours(TH[0].TimeIntervalHumid) >= DateTime.Now)
            {
                return;
            }
            else
            {
                string body = String.Format(template, name, "Humidity", TH[0].BHT, TH[0].AHT);
                DBUtl.ExecSQL("UPDATE SysUser SET EmailHumid = '" + time + "' WHERE FullName = '" + name + "'");
                EmailUtl.SendEmail(User[0].Email, subject, body, out string result);
            }
        }
        else if (option == 2)
        {
            if (TH[0].EmailCO2.AddHours(TH[0].TimeIntervalCO2) >= DateTime.Now)
            {
                return;
            }
            else
            {
                string body = String.Format(template, name, "CO2", TH[0].BCOT, TH[0].ACOT);
                DBUtl.ExecSQL("UPDATE SysUser SET EmailCO2 = '" + time + "' WHERE FullName = '" + name + "'");
                EmailUtl.SendEmail(User[0].Email, subject, body, out string result);
            }
        }
    }
}

