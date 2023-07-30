using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Security.Claims;

namespace FYP.Controllers;

public class AccountController : Controller
{
    private const string LOGIN_SQL =
       @"SELECT * FROM SysUser 
            WHERE UserId = '{0}' 
              AND UserPw = HASHBYTES('SHA1', '{1}')";

    private const string ROLE_COL = "UserRole";
    private const string NAME_COL = "FullName";

    private const string REDIRECT_CNTR = "Monitoring";
    private const string REDIRECT_ACTN = "Dashboard";

    private const string LOGIN_VIEW = "UserLogin";

    [AllowAnonymous]
    public IActionResult Login(string returnUrl = null!)
    {
        TempData["ReturnUrl"] = returnUrl;
        return View(LOGIN_VIEW);
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Login(UserLogin user)
    {
        if (!AuthenticateUser(user.UserID, user.Password, out ClaimsPrincipal principal))
        {
            ViewData["Message"] = "Incorrect User ID or Password";
            ViewData["MsgType"] = "warning";
            return View(LOGIN_VIEW);
        }
        else
        {
            HttpContext.SignInAsync(
               CookieAuthenticationDefaults.AuthenticationScheme,
               principal,
           new AuthenticationProperties
           {
               IsPersistent = user.RememberMe
           });


            if (TempData["returnUrl"] != null)
            {
                string returnUrl = TempData["returnUrl"]!.ToString()!;
                if (Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
            }

            return RedirectToAction(REDIRECT_ACTN, REDIRECT_CNTR);
        }
    }

    [Authorize]
    public IActionResult Logoff(string returnUrl = null!)
    {
        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction(REDIRECT_ACTN, REDIRECT_CNTR);
    }

    [AllowAnonymous]
    public IActionResult Register()
    {
        return View("UserRegister");
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Register(SysUser usr)
    {
        // All new registrations will be initially farmer
        ModelState.Remove("UserRole");
        if (!ModelState.IsValid)
        {
            return View("UserRegister", usr);
        }
        else
        {
            string insert =
               @"INSERT INTO sysuser(UserId, UserPw, FullName, Email, UserRole, ATT, BTT, AHT, BHT, ACOT, BCOT, SendEmailTH, SendEmailCO2, TimeIntervalTH, TimeIntervalCO2) VALUES
                 ('{0}', HASHBYTES('SHA1', '{1}'), '{2}', '{3}', 'farmer', 20, 20, 20, 20, 20, 20 , NULL, NULL, 4, 3)";
            int res = DBUtl.ExecSQL(insert, usr.UserId, usr.UserPw, usr.FullName, usr.Email);
            if (res == 1)
            {
                TempData["Message"] = "Account Registered.";
                TempData["MsgType"] = "success";


                return View(LOGIN_VIEW);
            }
            else
            {
                ViewData["Message"] = DBUtl.DB_Message;
                ViewData["MsgType"] = "danger";
                return View("UserRegister");
            }
        }
    }
    [Authorize(Roles = "admin, farmer")]
    public IActionResult Profile()
    {
        string sql = "SELECT * FROM SysUser where FullName = '" + User.Identity!.Name + "'";
        DataTable dt = DBUtl.GetTable(sql);
        if (dt.Rows.Count == 1)
        {
            SysUser User = new()
            {
                ATT = (int)dt.Rows[0]["ATT"],
                BTT = (int)dt.Rows[0]["BTT"],
                AHT = (int)dt.Rows[0]["AHT"],
                BHT = (int)dt.Rows[0]["BHT"],
                ACOT = (int)dt.Rows[0]["ACOT"],
                BCOT = (int)dt.Rows[0]["BCOT"],
                TimeIntervalTH = (int)dt.Rows[0]["TimeIntervalTH"],
                TimeIntervalCO2 = (int)dt.Rows[0]["TimeIntervalCO2"],
            };
            return View(User);
        }
        else
        {
            TempData["Message"] = "Provider Not Found";
            TempData["MsgType"] = "warning";
            return RedirectToAction("Profile");
        }
    }
    public IActionResult ProfilePost()
    {
        IFormCollection form = HttpContext.Request.Form;
        string ATT1 = form["ATT"].ToString().Trim();
        string BTT1 = form["BTT"].ToString().Trim();
        string AHT1 = form["AHT"].ToString().Trim();
        string BHT1 = form["BHT"].ToString().Trim();
        string ACOT1 = form["ACOT"].ToString().Trim();
        string BCOT1 = form["BCOT"].ToString().Trim();
        string TimeInterval1 = form["TimeIntervalTH"].ToString().Trim();
        string TimeInterval2 = form["TimeIntervalCO2"].ToString().Trim();

        // Update Record in Database  
        string sql = @"UPDATE SysUser
                       SET ATT = {0}, BTT = {1}, AHT = {2}, BHT = {3}, ACOT = {4}, BCOT = {5}, TimeIntervalTH = {6}, TimeIntervalCO2 = {7}
                       WHERE FullName='" + User.Identity!.Name + "'";

        string update = string.Format(sql, ATT1, BTT1, AHT1, BHT1, ACOT1, BCOT1, TimeInterval1, TimeInterval2);

        int count = DBUtl.ExecSQL(update);
        if (count == 1)
        {
            TempData["Message"] = "Threshold Record Updated";
            TempData["MsgType"] = "success";
            return RedirectToAction("Profile");
        }
        else
        {
            ViewData["Message"] = DBUtl.DB_Message;
            ViewData["ExecSQL"] = DBUtl.DB_SQL;
            ViewData["MsgType"] = "danger";
            return RedirectToAction("Profile");
        }
    }

    [Authorize(Roles = "admin")]
    public IActionResult UserList()
    {
        List<SysUser> list = DBUtl.GetList<SysUser>("SELECT * FROM SysUser WHERE UserRole='farmer' ");
        return View(list);
    }

    [AllowAnonymous]
    public IActionResult Forbidden()
    {
        return View();
    }

    [AllowAnonymous]
    public IActionResult VerifyUserID(string userId)
    {
        string select = $"SELECT * FROM SysUser WHERE Userid='{userId}'";
        if (DBUtl.GetTable(select).Rows.Count > 0)
        {
            return Json($"[{userId}] already in use");
        }
        return Json(true);
    }

    private static bool AuthenticateUser(string uid, string pw, out ClaimsPrincipal principal)
    {
        principal = null!;

        DataTable ds = DBUtl.GetTable(LOGIN_SQL, uid, pw);
        if (ds.Rows.Count == 1)
        {
            principal =
               new ClaimsPrincipal(
                  new ClaimsIdentity(
                     new Claim[] {
                    new Claim(ClaimTypes.NameIdentifier, uid),
                    new Claim(ClaimTypes.Name, ds.Rows[0][NAME_COL]!.ToString()!),
                    new Claim(ClaimTypes.Role, ds.Rows[0][ROLE_COL]!.ToString()!)
                     }, "Basic"
                  )
               );
            return true;
        }
        return false;
    }

}