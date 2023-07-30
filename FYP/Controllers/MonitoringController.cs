﻿using Microsoft.AspNetCore.Mvc;
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
    public IActionResult TempHumidChart()
    {
        List<DataPoint> TData = new List<DataPoint>();
        List<DataPoint> HData = new List<DataPoint>();

        // Create MySQL connection and define Query
        MySqlConnection conn = new MySqlConnection("server=localhost;uid=root;pwd=;database=esp32");
        MySqlCommand query = new MySqlCommand("SELECT * FROM dht22", conn);

        // Open connection and execute query
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
        //string x = "";
        for (int i = THData.Count - 30; i < THData.Count; i++)
        {
            //x = (1+i).ToString();
            TData.Add(new DataPoint(THData[i].DataInsert.ToString(), THData[i].Temperature));
            HData.Add(new DataPoint(THData[i].DataInsert.ToString(), THData[i].Humidity));
        }

        ViewBag.DataPoints1 = JsonConvert.SerializeObject(TData);
        ViewBag.DataPoints2 = JsonConvert.SerializeObject(HData);

        //-----------------------------------------

        //Second query to get Threshold from Sysuser
        List<SysUser> UserTH = DBUtl.GetList<SysUser>("SELECT * FROM SysUser where FullName = '" + User.Identity!.Name + "'");
        string name = "" + User.Identity!.Name;

        int count = THData.Count;
        if (THData[count - 1].Temperature > UserTH[0].ATT || THData[count - 1].Temperature < UserTH[0].BTT)
        {
            if (THData[count - 1].Humidity > UserTH[0].AHT || THData[count - 1].Humidity < UserTH[0].BHT)
            {
                TempData["Message"] = "Threshold Exceeded/Subceed. Temperature Threshold: " + UserTH[0].BTT + "-" + UserTH[0].ATT + "°C. Humidity Threshold: " + UserTH[0].BHT + "-" + UserTH[0].AHT + "%. Current Temperature/Humidity: " + THData[count - 1].Temperature + "°C : " + THData[count - 1].Humidity + "%";
                TempData["MsgType"] = "danger";

                sendEmail(name, 2);
            }
            else
            {
                TempData["Message"] = "Threshold Exceeded/Subceed. Temperature Threshold: " + UserTH[0].BTT + "-" + UserTH[0].ATT + "°C. Current Temperature: " + THData[count - 1].Temperature + "°C";
                TempData["MsgType"] = "danger";

                sendEmail(name, 0);
            }
        }
        else
        {
            if (THData[count - 1].Humidity > UserTH[0].AHT || THData[count - 1].Humidity < UserTH[0].BHT)
            {
                TempData["Message"] = "Threshold Exceeded/Subceed. Humidity Threshold: " + UserTH[0].BHT + "-" + UserTH[0].AHT + "%. Current Humidity: " + THData[count - 1].Humidity + "%";
                TempData["MsgType"] = "danger";

                sendEmail(name, 1);
            }
            else
            {
                TempData["Message"] = "Within Threshold. Temperature Threshold: " + UserTH[0].BTT + "-" + UserTH[0].ATT + "°C. Humidity Threshold: " + UserTH[0].BHT + "-" + UserTH[0].AHT + "%";
                TempData["MsgType"] = "success";
            }

        }
        // Pass data to view
        conn.Close();
        return View();
    }
    [Authorize(Roles = "admin, farmer")]
    public IActionResult CO2Chart()
    {
        List<DataPoint> CData = new List<DataPoint>();

        // Create MySQL connection and define Query
        MySqlConnection conn = new MySqlConnection("server=localhost;uid=root;pwd=;database=esp32");
        MySqlCommand query = new MySqlCommand("SELECT * FROM co2", conn);

        // Open connection and execute query
        conn.Open();
        MySqlDataReader execute = query.ExecuteReader();

        // Read data into list
        List<TempHumid> THData = new List<TempHumid>();

        while (execute.Read())
        {
            THData.Add(new TempHumid()
            {
                CO2 = execute.GetInt32("CO2"),
                DataInsert = execute.GetDateTime("datetime"),
            });
        }

        for (int i = THData.Count - 30; i < THData.Count; i++)
        {
            CData.Add(new DataPoint(THData[i].DataInsert.ToString(), THData[i].CO2));
        }

        ViewBag.DataPoints3 = JsonConvert.SerializeObject(CData);

        string name = "" + User.Identity!.Name;
        //Second query to get Threshold from Sysuser
        List<SysUser> UserTH = DBUtl.GetList<SysUser>("SELECT * FROM SysUser where FullName = '" + User.Identity!.Name + "'");
        int count = THData.Count;
        if (THData[count - 1].CO2 > UserTH[0].ACOT || THData[count - 1].CO2 < UserTH[0].BCOT)
        {
            TempData["Message"] = "Threshold Exceeded/Subceed. CO2 Levels Threshold: " + UserTH[0].BCOT + "ppm - " + UserTH[0].ACOT + "ppm. Current CO2 Levels: " + THData[count - 1].CO2 + "ppm";
            TempData["MsgType"] = "danger";
            sendEmail(name,3);
        }
        else
        {
            TempData["Message"] = "Within Threshold. Current Threshold: " + UserTH[0].BCOT + "ppm - " + UserTH[0].ACOT + "ppm. Current CO2 Levels: " + THData[count - 1].CO2 + "ppm";
            TempData["MsgType"] = "success";
        }

        // Pass data to view
        conn.Close();
        return View();
    }

    private static void sendEmail(string name, int option)
    {
        //query to get Threshold from Sysuser
        List<SysUser> UserTH = DBUtl.GetList<SysUser>("SELECT * FROM SysUser where FullName = '" + name + "'");

        // Template
        string subject = "TempHumid";
        string time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt");

        if (option == 0)
        {
            string template = "Dear " + UserTH[0].FullName + " \n\r" +
                          "<p>Current Threshold: " + UserTH[0].BTT + UserTH[0].ATT + "C</p> \n\r" +
                          "<p>Sensor has detected the Temperature Threshold has been breach. </p> \n\r" +
                          "Temperatue Sensor \n\r";
            if (UserTH[0].SendEmailTH.AddHours(UserTH[0].TimeIntervalTH) >= DateTime.Now)
            {
                return;
            }
            else
            {
                DBUtl.ExecSQL("UPDATE SysUser SET SendEmailTH = '" + time + "' WHERE FullName = '" + name + "'");
                EmailUtl.SendEmail(UserTH[0].Email, subject, template, out string result);
            }

        }
        else if (option == 1)
        {
            string template = "Dear " + UserTH[0].FullName + " \n\r" +
                          "<p>Current Threshold:" + UserTH[0].BHT + UserTH[0].AHT + "%</p> \n\r" +
                          "<p>Sensor has detected the Humidity Threshold has been breach. </p> \n\r" +
                          "Humidity Sensor \n\r";
            if (UserTH[0].SendEmailTH.AddHours(UserTH[0].TimeIntervalTH) >= DateTime.Now)
            {
                return;
            }
            else
            {
                DBUtl.ExecSQL("UPDATE SysUser SET SendEmailTH = '" + time + "' WHERE FullName = '" + name + "'");
                EmailUtl.SendEmail(UserTH[0].Email, subject, template, out string result);
            }
        }
        else if (option == 2)
        {
            string template = "Dear " + UserTH[0].FullName + " \n\r" +
                          "<p>" + UserTH[0].BTT + UserTH[0].ATT + "</p> \n\r" +
                          "<p>" + UserTH[0].BHT + UserTH[0].AHT + "</p> \n\r" +
                          "<p>Sensor has detected the both Temperature and Humidity Threshold has been breach.</p> \n\r" +
                          "Temperature & Humidity Sensor \n\r";
            if (UserTH[0].SendEmailTH.AddHours(UserTH[0].TimeIntervalTH) >= DateTime.Now)
            {
                return;
            }
            else
            {
                DBUtl.ExecSQL("UPDATE SysUser SET SendEmailTH = '" + time + "' WHERE FullName='" + name + "'");
                EmailUtl.SendEmail(UserTH[0].Email, subject, template, out string result);
            }
        }
        else if (option == 3)
        {
            string template = "Dear " + UserTH[0].FullName + " \n\r" +
                         "<p>" + UserTH[0].BCOT + UserTH[0].ACOT + "</p> \n\r" +
                         "<p>Sensor has detected the CO2 Threshold has been breach. </p> \n\r" +
                         "CO2 Sensor \n\r";
            if (UserTH[0].SendEmailCO2.AddHours(UserTH[0].TimeIntervalTH) >= DateTime.Now)
            {
                return;
            }
            else
            {
                DBUtl.ExecSQL("UPDATE SysUser SET SendEmailCO2 = '" + time + "' WHERE FullName = '" + name + "'");
                EmailUtl.SendEmail(UserTH[0].Email, subject, template, out string result);
            }
        }
    }

}
