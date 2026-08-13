using Eoffice.BAL;
using Eoffice.DAL;
using Eoffice.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace Eoffice.Controllers
{
    public class HomeController : Controller
    {
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       UserBAL bal = new UserBAL();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CreateDocument()
        {
            ViewBag.Dropdown1Items = new List<SelectListItem>
    {
        new SelectListItem { Text = "Option 1", Value = "1" },
        new SelectListItem { Text = "Option 2", Value = "2" }
    };

            ViewBag.Dropdown2Items = new List<SelectListItem>
    {
        new SelectListItem { Text = "Choice A", Value = "A" },
        new SelectListItem { Text = "Choice B", Value = "B" }
    };

            return View();
        }
        public ActionResult CreateFile()
        {
            return View();
        }
        public ActionResult HandOverFile()
        {
            return View();
        }
        public ActionResult Login()
        {
            string id = cryptography.EncryptText(RandomString(10, false));
            TempData["lid"] = id;
            Session["lid"] = id;
            ViewBag.Salt = id;
            return View();
        }

        [HttpPost]
        public ActionResult Login(ModelClass obj)
        {

            if (ModelState.IsValid)
            {
                string plainpwd = ""; //
                string hash = "";
                Session["utypeForCheck"] = obj.RoleType;
                string userAgent1 = Request.UserAgent;
                Session["Browser_Name"] = GetBrowserName(userAgent1);
                Session["Ip_Address"] = GetClientIpAddress();
                Session["Mac_Address"] = GetMacAddress();
                Session["Host_Name"] = Dns.GetHostName();
                Session["Client_PC_Name"] = GetComputerName(Request.UserHostAddress);
                string sessionId = Session.SessionID;
                Session["Session_Id"] = sessionId.ToString();

                bal.CheckLastAccess(obj.User_Name);

                DataSet ds1 = bal.GetPassword(obj);

                if (ds1 != null && ds1.Tables[0].Rows.Count > 0)
                {
                    plainpwd = cryptography.DecryptText(ds1.Tables[0].Rows[0]["s2"].ToString());
                    string finaltext = obj.User_Name.Trim().Replace("'", "").Replace("''", "") + "855S4K7850A" + TempData["lid"].ToString() + plainpwd;
                    hash = cryptography.CreateHash256(finaltext);
                    Session["uid"] = ds1.Tables[0].Rows[0]["UserID"].ToString();
                    Session["utype"] = ds1.Tables[0].Rows[0]["User_Type"].ToString();
                    Session["utypeForCheck"] = ds1.Tables[0].Rows[0]["User_Type"].ToString();
                }
                else
                {
                    Session["lid"] = TempData["lid"];
                    TempData["ErrorMsg"] = "Invalid credentials!";
                    return RedirectToAction("Login");
                }

                if (obj.User_Password == hash)
                {
                    if (obj.RoleType == "1")
                    {
                        //string str = " select * from Utility_MUser WHERE loginname='" + obj.User_Name.Trim() + "' and password='" + cryptography.EncryptText(plainpwd) + "' and CategoryCode='" + obj.RoleType.Trim() + "'";
                        DataTable dt = bal.GetUtilityUser(obj, plainpwd);

                        if (dt.Rows.Count > 0)
                        {
                            Session["UserID"] = dt.Rows[0]["TableID"].ToString();
                            Session["UserName"] = obj.User_Name.Trim();
                            Session["Section"] = null;
                            Session["EmpID"] = null;
                            Session["IsHR"] = null;
                            Session["UserCatCode"] = dt.Rows[0]["CategoryCode"].ToString();
                            //Session["FINYEAR"] = ddlFinYear.SelectedValue;
                            Session["Name"] = dt.Rows[0]["Name"].ToString();
                            Session["CENTERCODE"] = "Admin";
                            Session["Role"] = "Admin";
                            Session["lid"] = TempData["lid"];

                            DataSet dsLoginTrack = new DataSet();
                            dsLoginTrack = bal.FN_ExecuteQuerySingle("ADM_PROC_TRACKING_LOGIN '" + Session["UserID"].ToString() + "','" + Session["Ip_Address"].ToString() + "','" + Session["Role"].ToString() + "','" + Session["UserCatCode"] + "','" + null + "','" + Session["Mac_Address"].ToString() + "','" + null + "','" + Session["Browser_Name"].ToString() + "','" + Session["Client_PC_Name"].ToString() + "','Y','" + Session["Session_Id"].ToString() + "'");
                            if (dsLoginTrack.Tables[0].Rows.Count > 0)
                            {
                                Session["PreLoginTime"] = dsLoginTrack.Tables[0].Rows[0]["LoginTime"].ToString();
                            }

                            string guid = Guid.NewGuid().ToString();
                            Session["AuthToken"] = guid;
                            Session["AppSessionId"] = guid;
                            // now create a new cookie with this guid value
                            Response.Cookies.Add(new HttpCookie("AuthToken", guid));

                            return RedirectToAction("Index", "Admin");
                        }
                        else
                        {
                            Session["lid"] = TempData["lid"];
                            TempData["ErrorMsg"] = "Invalid credentials!";
                            return RedirectToAction("Login");
                        }
                    }
                    else
                    {
                        DataSet dsuser = new DataSet();
                        dsuser = bal.FN_ExecuteQuerySingle("proc_user_details '" + obj.User_Name.Trim() + "','" + cryptography.EncryptText(plainpwd) + "' ");
                        if (dsuser.Tables[0].Rows.Count > 0)
                        {
                            Session["E_Mail"] = dsuser.Tables[0].Rows[0]["E_Mail"].ToString();
                            Session["Contact_No"] = dsuser.Tables[0].Rows[0]["Contact_No"].ToString();
                            Session["UserID"] = dsuser.Tables[0].Rows[0]["TableID"].ToString();
                            Session["UserName"] = obj.User_Name.Trim();
                            Session["EmpID"] = dsuser.Tables[0].Rows[0]["EmpID"].ToString();
                            Session["IsHR"] = dsuser.Tables[0].Rows[0]["HRFlag"].ToString();
                            Session["UserCatCode"] = dsuser.Tables[0].Rows[0]["CategoryCode"].ToString();
                            Session["Role"] = dsuser.Tables[0].Rows[0]["RoleName"].ToString();
                            Session["utypeForCheck"] = dsuser.Tables[0].Rows[0]["RoleName"].ToString();
                            Session["RoleCode"] = dsuser.Tables[0].Rows[0]["RoleCode"].ToString();
                            Session["Emp_Name"] = dsuser.Tables[0].Rows[0]["Emp_Name"].ToString();
                            Session["ED_RowID"] = dsuser.Tables[0].Rows[0]["ED_RowID"].ToString();
                            Session["Est_TypeName"] = dsuser.Tables[0].Rows[0]["Est_TypeName"].ToString();
                            Session["dept_sec"] = dsuser.Tables[0].Rows[0]["dept_sec"].ToString();

                            //Session["Dep_Code"] = dsuser.Tables[0].Rows[0]["Dep_Code"].ToString();
                            //Session["Dep_Name"] = dsuser.Tables[0].Rows[0]["Dep_Name"].ToString();
                            //Session["Est_typeCode"] = dsuser.Tables[0].Rows[0]["Est_typeCode"].ToString();
                            //Session["Des_Code"] = dsuser.Tables[0].Rows[0]["Des_Code"].ToString();
                            //Session["Des_Name"] = dsuser.Tables[0].Rows[0]["Des_Name"].ToString();
                            //Session["Scode"] = dsuser.Tables[0].Rows[0]["Sec_Code"].ToString();
                            //Session["Section"] = dsuser.Tables[0].Rows[0]["Sec_Name"].ToString();

                            DataSet dsLoginTrack = new DataSet();
                            dsLoginTrack = bal.FN_ExecuteQuerySingle("ADM_PROC_TRACKING_LOGIN '" + Session["UserID"].ToString() + "','" + Session["Ip_Address"].ToString() + "','" + Session["Role"].ToString() + "','" + Session["UserCatCode"] + "','" + null + "','" + Session["Mac_Address"].ToString() + "','" + null + "','" + Session["Browser_Name"].ToString() + "','" + Session["Client_PC_Name"].ToString() + "','Y','" + Session["Session_Id"].ToString() + "'");
                            if (dsLoginTrack.Tables[0].Rows.Count > 0)
                            {
                                Session["PreLoginTime"] = dsLoginTrack.Tables[0].Rows[0]["LoginTime"].ToString();
                            }

                            Session["lid"] = TempData["lid"];
                            string guid = Guid.NewGuid().ToString();
                            Session["AuthToken"] = guid;
                            Session["AppSessionId"] = guid;
                            // now create a new cookie with this guid value  
                            Response.Cookies.Add(new HttpCookie("AuthToken", guid));
                            if (dsuser.Tables[0].Rows[0]["Is_Pwd_Change"].ToString() == "N" || dsuser.Tables[0].Rows[0]["Is_Pwd_Change"].ToString() == "")
                            {
                                Session["lid"] = TempData["lid"];
                                return RedirectToAction("ChangePasswordFirstTime");
                            }
                            else
                            {
                                Session["lid"] = TempData["lid"];
                                //Response.Redirect("~/Home.aspx",false);
                                // Response.Redirect("~/frm_User_Dashboard.aspx", false);

                                if (dsuser.Tables[0].Rows.Count <= 1)
                                {
                                    DataSet dsED = new DataSet();
                                    dsED = bal.FN_ExecuteQuerySingle("proc_employerdepartment_details '" + Session["ED_RowID"].ToString() + "','" + Session["EmpID"].ToString() + "' ");
                                    if (dsED.Tables[0].Rows.Count > 0)
                                    {
                                        Session["ED_Row_ID"] = dsED.Tables[0].Rows[0]["id"].ToString();
                                        Session["Est_typeCode"] = dsED.Tables[0].Rows[0]["Est_typeCode"].ToString();
                                        Session["Est_TypeName"] = dsED.Tables[0].Rows[0]["Est_TypeName"].ToString();
                                        Session["Dep_Code"] = dsED.Tables[0].Rows[0]["Est_deptCode"].ToString();
                                        Session["Dep_Name"] = dsED.Tables[0].Rows[0]["Est_deptName"].ToString();
                                        Session["Des_Code"] = dsED.Tables[0].Rows[0]["Est_desigCode"].ToString();
                                        Session["Des_Name"] = dsED.Tables[0].Rows[0]["Est_desigName"].ToString();
                                        Session["Scode"] = dsED.Tables[0].Rows[0]["Est_secCode"].ToString();
                                        Session["Section"] = dsED.Tables[0].Rows[0]["Est_secName"].ToString();

                                        return RedirectToAction("Index", "User");
                                    }

                                    Session["lid"] = TempData["lid"];
                                    TempData["ErrorMsg"] = "Invalid credentials!";

                                    return RedirectToAction("Login");
                                }

                                else
                                {
                                    // Pass DataTable to ViewBag
                                    ViewBag.DepartmentData = dsuser.Tables[0];

                                    // Flag to open modal
                                    ViewBag.ShowModal = true;
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      return View("Login");
                                }

                                    

                                //grdSection.DataSource = dsuser;
                                //grdSection.DataBind();
                                //divDepartment.Visible = true;
                                //login.Visible = false;
                                //ScriptManager.RegisterStartupScript(this, this.GetType(), "script", "open_Section_modal();", true);
                            }
                        }
                        else
                        {
                            Session["lid"] = TempData["lid"];
                            TempData["ErrorMsg"] = "Invalid credentials!";
                            return RedirectToAction("Login");
                        }
                    }
                }
                else
                {
                    Session["lid"] = TempData["lid"];
                    TempData["ErrorMsg"] = "Invalid credentials!";
                    return RedirectToAction("Login");
                }



            }

            return RedirectToAction("Login");

        }


        [HttpPost]
        public ActionResult SelectDepartment(string ED_RowID)
        {
            DataSet dsED = new DataSet();
            dsED = bal.FN_ExecuteQuerySingle("proc_employerdepartment_details '" + ED_RowID + "','" + Session["EmpID"].ToString() + "' ");

            if (dsED != null && dsED.Tables.Count > 0 && dsED.Tables[0].Rows.Count > 0)
            {
                var row = dsED.Tables[0].Rows[0];

                Session["ED_Row_ID"] = dsED.Tables[0].Rows[0]["id"].ToString();
                Session["Est_typeCode"] = dsED.Tables[0].Rows[0]["Est_typeCode"].ToString();
                Session["Est_TypeName"] = dsED.Tables[0].Rows[0]["Est_TypeName"].ToString();
                Session["Dep_Code"] = dsED.Tables[0].Rows[0]["Est_deptCode"].ToString();
                Session["Dep_Name"] = dsED.Tables[0].Rows[0]["Est_deptName"].ToString();
                Session["Des_Code"] = dsED.Tables[0].Rows[0]["Est_desigCode"].ToString();
                Session["Des_Name"] = dsED.Tables[0].Rows[0]["Est_desigName"].ToString();
                Session["Scode"] = dsED.Tables[0].Rows[0]["Est_secCode"].ToString();
                Session["Section"] = dsED.Tables[0].Rows[0]["Est_secName"].ToString();

                
            }

            return RedirectToAction("Index", "User");
        }


        private string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }

        static string GetMacAddress()
        {
            string macAddress = string.Empty;
            try
            {
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface networkInterface in networkInterfaces)
                {
                    // Check if the network interface is not a loopback or virtual interface
                    if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        networkInterface.NetworkInterfaceType != NetworkInterfaceType.Ppp &&
                        !networkInterface.Description.ToLowerInvariant().Contains("virtual"))
                    {
                        PhysicalAddress physicalAddress = networkInterface.GetPhysicalAddress();
                        byte[] bytes = physicalAddress.GetAddressBytes();
                        // Format the MAC address as a string
                        macAddress = string.Join(":", bytes.Select(b => b.ToString("X2")));
                        // Exit the loop after the first valid MAC address is found
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return macAddress;
        }

        private string GetClientIpAddress()
        {
            string ipAddress = string.Empty;

            try
            {
                // Check if the request is from a proxy or has a forwarded-for header
                if (HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
                {
                    ipAddress = HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                }
                else if (HttpContext.Request.UserHostAddress.Length != 0)
                {
                    ipAddress = HttpContext.Request.UserHostAddress;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return ipAddress;
        }

        public string GetWebBrowserName()
        {
            string WebBrowserName = string.Empty;
            try
            {
                WebBrowserName = HttpContext.Request.Browser.Browser;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return WebBrowserName;
        }

        public string GetComputerName(string clientIP)
        {
            try
            {
                var hostEntry = Dns.GetHostEntry(clientIP);
                return hostEntry.HostName;
            }
            catch (Exception ex )
            {
                return string.Empty;
            }
        }

        private string GetBrowserName(string userAgent)
        {
            if (userAgent.Contains("MSIE") || userAgent.Contains("Trident"))
            {
                return "Internet Explorer";
            }
            else if (userAgent.Contains("Firefox"))
            {
                return "Mozilla Firefox";
            }
            else if (userAgent.Contains("Chrome"))
            {
                return "Google Chrome";
            }
            else if (userAgent.Contains("Safari"))
            {
                return "Apple Safari";
            }
            else if (userAgent.Contains("Opera") || userAgent.Contains("OPR"))
            {
                return "Opera";
            }
            else if (userAgent.Contains("Edge"))
            {
                return "Microsoft Edge";
            }
            else
            {
                return "Unknown";
            }
         

        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear(); // Clear session variables
            Session.Abandon(); // End session
            Response.Cookies.Clear(); // Clear cookies
            return RedirectToAction("Login");
        }


        //public ActionResult Logout()
        //{
        //    // 1) Clear session
        //    Session.Clear();
        //    Session.Abandon();

        //    // 2) Remove auth cookie(s)
        //    var authCookie = Request.Cookies[".ASPXAUTH"] ?? Request.Cookies[".AspNet.ApplicationCookie"];
        //    if (authCookie != null)
        //    {
        //        authCookie.Expires = DateTime.UtcNow.AddDays(-1);
        //        authCookie.Value = null;
        //        authCookie.Path = "/";
        //        Response.Cookies.Add(authCookie);
        //    }

        //    // 3) Remove all cookies (optional)
        //    foreach (var key in Request.Cookies.AllKeys)
        //    {
        //        var c = new HttpCookie(key, "");
        //        c.Expires = DateTime.UtcNow.AddDays(-1);
        //        c.Path = "/";
        //        Response.Cookies.Add(c);
        //    }

        //    // 4) Prevent caching
        //    Response.Cache.SetCacheability(HttpCacheability.NoCache);
        //    Response.Cache.SetNoStore();
        //    Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));

        //    return RedirectToAction("Login");
        //   // return RedirectToAction("Login", "Home");
        //}



        #region forgot password and verify otp 
        public ActionResult ForgotPassword()
        {
            //System.Diagnostics.Debug.WriteLine("Inside ForgotPassword");
            return View();
          
        }

        [HttpPost]
        public JsonResult ForgotPassword(string email, string mobile) //It returns a JSON response (for AJAX) instead of loading a new page.
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(mobile))
            {
                return Json(new { success = false, message = "Please enter email and mobile both." });
            }

            DataTable dt = bal.GetUser(email, mobile);//It checks if a user with this email and mobile exists.//Returns user data in a DataTable (like an Excel sheet).

            if (dt.Rows.Count > 0)  //If at least one row was returned, that means the user exists.
            {
                string[] AllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                string RandomOTP = GenerateRandomOTP(4, AllowedCharacters);
                Session["ForgotPasswordOTP"] = RandomOTP;// Stores OTP, login ID, decrypted password, and email into session. //This memory is saved only for this user until they close the browser or finish the process

                Session["LoginID"] = dt.Rows[0]["LoginName"].ToString();
                Session["LoginPassword"] = cryptography.DecryptText(dt.Rows[0]["Password"].ToString());
                Session["ToEmail"] = email.ToString().Trim();

                SendForgotPasswordMail(email, RandomOTP, dt.Rows[0]["LoginName"].ToString());

                return Json(new { success = true, message = "OTP has been sent to your email/mobile." }); //This tells the AJAX that OTP was sent successfully.

            }

            return Json(new { success = false, message = "Email ID and Mobile No not registered in AU-Eoffice Portal. Please contact to ICT Cell for registration!" }); //If no match in the database, show an error.

        }


        [HttpGet]
        public ActionResult VerifyForgotPasswordOtp()
        {
            return View();
        }

        [HttpPost]
        public JsonResult VerifyForgotPasswordOtp(string otp)
        {

            if (otp.ToString().Trim() == Session["ForgotPasswordOTP"]?.ToString())
            {
                SendIdPassMail(Session["ToEmail"]?.ToString(), Session["LoginPassword"]?.ToString(), Session["LoginID"]?.ToString());
                return Json(new { success = true, message = "OTP verified. Login ID and Password sent to your registered E-mail Successfully." });
            }
            return Json(new { success = false, message = "Invalid or expired OTP." });
        }

        private void SendForgotPasswordMail(string toEmail, string otp, string username) //Builds an email body with the OTP and sends it using SmtpClient.
        {
            string subject = "AU-Eoffice Login credentials verification code";
            string body = $"Hi {username},<br/>Use this verification code for verification: <b>{otp}</b><br/>This OTP is required to forget password in AU-Eoffice portal.<br/>If you didn’t request this, contact admin.";

            MailMessage message = new MailMessage("software@acmedigitek.in", toEmail);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient("mail.acmedigitek.in", 587);
            smtp.Credentials = new NetworkCredential("software@acmedigitek.in", "Softwareacme@#987");
            smtp.EnableSsl = false;
            smtp.Send(message);
        }


        private void SendIdPassMail(string toEmail, string password, string username) //After OTP is verified, this method emails the user’s login ID and password.
        {
            string subject = "AU-Eoffice Login credentials";
            string body = $"Hi {username},<br/>Your Login ID is: <b>{username}</b> and password is: <b>{password}</b> for AU-Eoffice portal.<br/>If you didn’t request this, contact admin.";

            MailMessage message = new MailMessage("software@acmedigitek.in", toEmail);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient("mail.acmedigitek.in", 587);
            smtp.Credentials = new NetworkCredential("software@acmedigitek.in", "Softwareacme@#987");
            smtp.EnableSsl = false;
            smtp.Send(message); 

        }
#endregion

        #region Changepassword first time 

        private string GenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)
        {
            string sOTP = String.Empty;
            string sTempChars = String.Empty;
            Random rand = new Random();
            for (int i = 0; i < iOTPLength; i++)
            {
                int p = rand.Next(0, saAllowedCharacters.Length);
                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
                sOTP += sTempChars;
            }
            return sOTP;
        }



        [HttpGet]
        public ActionResult ChangePasswordFirstTime()
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Home");

            var model = new ChangePasswordFirstTimeModel
            {
                Username = Session["Emp_Name"].ToString() + " (" + Session["UserName"].ToString() + ")"
            };

            Session["FirstUserName"] = model.Username;

            return View(model);
        }

        [HttpPost]
        public ActionResult ChangePasswordFirstTime(ChangePasswordFirstTimeModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Password.ToString().Trim()) || string.IsNullOrWhiteSpace(model.ConfirmPassword.ToString().Trim()))
                {
                    ViewBag.Error = "Please enter Password and Confirm Password both.";
                    return View(model);
                }

                if (model.Password.ToString().Trim() != model.ConfirmPassword.ToString().Trim())
                {
                    ViewBag.Error = "Passwords do not match.";
                    return View(model);
                }



                var userDetails = bal.GetUserDetails(Session["UserName"]?.ToString().Trim());
                if (userDetails.Rows.Count == 0)
                {
                    ViewBag.Error = "User not found.";
                    return View(model);
                }

                model.Email = userDetails.Rows[0]["E_Mail"].ToString();
                model.Mobile = userDetails.Rows[0]["Contact_No"].ToString();
                //model.RandomOTP = GenerateOTP();

                //TempData["RandomOTP"] = model.RandomOTP;
                Session["RandomOTP"] = "";
                Session["plain_pwd"] = model.Password;


                if (!string.IsNullOrEmpty(model.Email.ToString().Trim()) && !string.IsNullOrEmpty(model.Mobile.ToString().Trim()))
                {
                    string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
                    string sRandomOTP = GenerateRandomOTP(4, saAllowedCharacters);
                    Session["RandomOTP"] = sRandomOTP;
                    string Tomail = model.Email.ToString().Trim();
                    SendMail(Tomail, sRandomOTP, Session["UserName"]?.ToString());
                }
                else
                {
                    ViewBag.Error = "Email-ID and Mobile Number Not Found. Please Contact Admin.";
                    return View(model);

                }


                //SendMail(model.Email, model.RandomOTP, model.Username);

                return RedirectToAction("VerifyOTP");
            }
            catch(Exception ex)
            {
                ViewBag.Error = "Error: "+ ex.Message +"";
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult VerifyOTP()
        {
            return View();
        }

        [HttpPost]
        public ActionResult VerifyOTP(string otp)
        {
            try
            {
                if (Session["RandomOTP"]?.ToString() == otp.ToString().Trim())
                {
                    string username = Session["UserName"]?.ToString();
                    string plainPwd = Session["plain_pwd"]?.ToString();

                    bool isUpdated = bal.UpdatePassword(username, plainPwd);

                    if (isUpdated)
                        return RedirectToAction("Login", "Home");

                    ViewBag.Error = "Password update failed.";
                }
                else
                {
                    ViewBag.Error = "Invalid OTP.";
                }

                return View();
            }
            catch (Exception ex) 
            {
                ViewBag.Error = "Error: "+ ex.Message +"";
                return View();
            }
        }

        private string GenerateOTP()
        {
            var rand = new Random();
            return string.Concat(Enumerable.Range(0, 4).Select(_ => rand.Next(0, 10)));
        }

        private void SendMail(string toEmail, string otp, string username)
        {
            string subject = "AU-Eoffice Login credentials verification code";
            string body = $"Hi {username},<br/>Use this verification code to update your password: <b>{otp}</b><br/>If you didn’t request this, contact admin.";

            MailMessage message = new MailMessage("software@acmedigitek.in", toEmail);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient("mail.acmedigitek.in", 587);
            smtp.Credentials = new NetworkCredential("software@acmedigitek.in", "Softwareacme@#987");
            smtp.EnableSsl = false;
            smtp.Send(message);
        }


        
        #endregion
    }
}

















