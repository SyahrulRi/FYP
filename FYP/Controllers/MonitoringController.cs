using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;


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

            //sendEmail(name, 0);
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

            //sendEmail(name, 2);
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
            //sendEmail(name, 2);
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
        string subject = "TempHumid";
        string time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt");

        if (option == 0)
        {
            string template = "Dear " + User[0].FullName + " \n\r" +
                          "<p>Current Threshold: " + TH[0].BTT + TH[0].ATT + "C</p> \n\r" +
                          "<p>Sensor has detected the Temperature Threshold has been breach. </p> \n\r" +
                          "Temperatue Sensor \n\r";
            if (TH[0].SendEmailTH.AddHours(TH[0].TimeIntervalTH) >= DateTime.Now)
            {
                return;
            }
            else
            {
                DBUtl.ExecSQL("UPDATE SysUser SET SendEmailTH = '" + time + "' WHERE FullName = '" + name + "'");
                EmailUtl.SendEmail(User[0].Email, subject, template, out string result);
            }

        }
        else if (option == 1)
        {
            string template = "Dear " + User[0].FullName + " \n\r" +
                          "<p>Current Threshold:" + TH[0].BHT + TH[0].AHT + "%</p> \n\r" +
                          "<p>Sensor has detected the Humidity Threshold has been breach. </p> \n\r" +
                          "Humidity Sensor \n\r";
            if (TH[0].SendEmailTH.AddHours(TH[0].TimeIntervalTH) >= DateTime.Now)
            {
                return;
            }
            else
            {
                DBUtl.ExecSQL("UPDATE SysUser SET SendEmailTH = '" + time + "' WHERE FullName = '" + name + "'");
                EmailUtl.SendEmail(User[0].Email, subject, template, out string result);
            }
        }
        else if (option == 2)
        {
            string template = "Dear " + User[0].FullName + " \n\r" +
                         "<p>" + TH[0].BCOT + TH[0].ACOT + "</p> \n\r" +
                         "<p>Sensor has detected the CO2 Threshold has been breach. </p> \n\r" +
                         "CO2 Sensor \n\r";
            if (TH[0].SendEmailCO2.AddHours(TH[0].TimeIntervalTH) >= DateTime.Now)
            {
                return;
            }
            else
            {
                DBUtl.ExecSQL("UPDATE SysUser SET SendEmailCO2 = '" + time + "' WHERE FullName = '" + name + "'");
                EmailUtl.SendEmail(User[0].Email, subject, template, out string result);
            }
        }
    }

}

