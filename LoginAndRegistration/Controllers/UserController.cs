using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LoginAndRegistration.Models;
using System.Net;
using System.Net.Mail;
using System.Web.Security;

namespace LoginAndRegistration.Controllers
{
    public class UserController : Controller
    {
        // Registration Action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        // Registration Post Action
        [HttpPost]
        [ValidateAntiForgeryToken] // Helps verify if the form has been created/sent by the same user
        public ActionResult Registration([Bind(Exclude = "VerifiedEmail,ActivationCode")] User user)
        {
            bool Status = false;
            string message = "";
             
            // Model Validation
            if (ModelState.IsValid)
            {
                // Email Validation - Existance
                #region // Email Exists
                var IsExist = IsEmailExist(user.EmailID);
                // Condition for when a email already exists display error
                if (IsExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exists");
                    return View(user);
                }
                #endregion

                // Activation Code - Generate
                #region Generate Activation Code
                user.ActivationCode = Guid.NewGuid(); // Initializes new instance of Guid for user's activation code.
                #endregion

                // Password Hashing
                #region Password Validation
                user.Password = Crypto.Hash(user.Password); // Computers the SHA256 has for the inputted user password
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword); // Incase ConfirmPassword does not match the issue
                #endregion
                user.VerifiedEmail = false; // Equals false until user clicks activation link

                // Save Data - Database
                #region Save Database
                using (DatabaseEntities dc = new DatabaseEntities())
                {
                    dc.Users.Add(user); // Adds user details to database
                    dc.SaveChanges(); // Then saves the changes

                    // Email - Send to users
                    SendVerificationLinkEmail(user.EmailID, user.ActivationCode.ToString(), user.FirstName.ToString() + " " + user.LastName.ToString());
                    message = " Registration successfully created. Your account activiation link " +
                        " has been sent to your email id:" + user.EmailID;
                    Status = true;
                }
                #endregion
            }
            else
            {
                message = "Invalid Request";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }

        // Verify Account Action
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using (DatabaseEntities dc = new DatabaseEntities()) // Using our database
            {
                dc.Configuration.ValidateOnSaveEnabled = false; // Avoids ConfirmPassword not matching the issue on save changes
                var v = dc.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                
                // Checks if the v query for users ActivationCode is not equal to null 
                if (v != null)
                {
                    // Changes results for verified users
                    v.VerifiedEmail = true;
                    dc.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Invalid Request";
                }       
            }
            ViewBag.Status = Status;
            return View();
        }

        // Login Action
        [HttpGet] // For when users view the login page, that's a GET request
        public ActionResult Login()
        {
            return View();
        }

        // Login Post Action
        [HttpPost] // For when users submit/POSTS forms, that's a POST request
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin login, string returnUrl)
        {
            string message = "";
            using (DatabaseEntities dc = new DatabaseEntities())
            {
                var v = dc.Users.Where(a => a.EmailID == login.EmailID).FirstOrDefault(); // Database based on where users EmailID is equal to the login EmailID entered
                
                if (v != null)
                {
                    // Condition checks if  the compares hashed login password with the database password is not equal to 0
                    if (string.Compare(Crypto.Hash(login.Password),v.Password) == 0)
                    {
                        int timeout = login.RememberMe ? 525600 : 20; // 525600 is 1 year
                        var ticket = new FormsAuthenticationTicket(login.EmailID, login.RememberMe, timeout); // new instance of tickets used for authentiction to identify users
                        string encryption = FormsAuthentication.Encrypt(ticket); // Creates an encrypted string for the tickets authentication suitable for HTTP cookie
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryption); // Creates new cookie
                        cookie.Expires = DateTime.Now.AddMinutes(timeout); // Sets expirary session time for cookie
                        cookie.HttpOnly = true; // Cookie value accessible by client-side
                        Response.Cookies.Add(cookie); // Adds the cookie

                        if (Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home"); // Sends registered users to Home page
                        }
                    }
                    else
                    {
                        message = "Invalid credentials provided";
                    }
                }
                else
                {
                    message = "Invalid credentials provided";
                }
            }

            ViewBag.Message = message;
            return View();
        }

        // Logout Action
        [Authorize] // Restricts users who meet the authentication requirements.
        [HttpPost]
        public ActionResult Logout()
        {
            // Signs user out and redirects them to login page
            FormsAuthentication.SignOut();
            return RedirectToAction("Login","User");
        }

        // Method that checks database users to see if user EmailID exists
        [NonAction]
        public bool IsEmailExist(string emailId)
        {
            using (DatabaseEntities dc = new DatabaseEntities())
            {
                var v = dc.Users.Where(a => a.EmailID == emailId).FirstOrDefault();
                return v != null;
            }
        }

        // Method that sets up the verification email
        [NonAction]
        public void SendVerificationLinkEmail(string emailId, string activationCode, string fullName)
        {
            var urlVerify = "/User/VerifyAccount/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, urlVerify);

            // Emails Properties
            var fromEmail = new MailAddress("dalelambertemails@gmail.com", "Dale Lambert");
            var toEmail = new MailAddress(emailId);
            var fromEmailPassword = "Lildogg1"; // Enter you're actual password for your gmail here

            string subject = fullName + " your account has been successfully created!"; 
            string body = "<br/><br/>We are excited to tell you " + fullName +
                " that your account has been successfully created. Please click on the link below to verify your account" +
                "<br/><br/><a href='" + link + "'>" + link + "</a> ";

            // Makes sure we're using the correct protocols for the email
            var smtp = new SmtpClient
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword),
                Host = "smtp.gmail.com",
                Port = 587,  
                DeliveryMethod = SmtpDeliveryMethod.Network             
            };

            // Sends messges using Email Properties
            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })

            smtp.Send(message);
        }
    }

}