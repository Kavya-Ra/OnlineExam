using Newtonsoft.Json;
using OnlineExam.Authentication;
using OnlineExam.DbContext;
using OnlineExam.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace OnlineExam.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        readonly Exam_DBEntities dbContext = new Exam_DBEntities();

        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login(string ReturnUrl = "")
        {
            if (User.Identity.IsAuthenticated)
            {
                return LogOut();
            }
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel loginView, string ReturnUrl = "")
        {
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(loginView.UserName, loginView.Password))
                {
                    var user = (CustomMembershipUser)Membership.GetUser(loginView.UserName, false);
                    if(user.IsActive == 0)
                    {
                        ViewBag.Error = "Your account is Inactive";
                        return View(loginView);
                    }
                    else
                    {
                        if (user != null)
                        {
                            CustomSerializeModel userModel = new CustomSerializeModel()
                            {
                                UserId = user.UserId,
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                                RoleName = user.Roles.Select(r => r.RoleName).ToList()
                            };

                            string userData = JsonConvert.SerializeObject(userModel);
                            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, loginView.UserName, DateTime.Now, DateTime.Now.AddDays(1), false, userData);

                            string enTicket = FormsAuthentication.Encrypt(authTicket);
                            HttpCookie faCookie = new HttpCookie("Cookie1", enTicket);
                            Response.Cookies.Add(faCookie);
                            ReturnUrl = "~/" + user.Roles.FirstOrDefault().RoleName + "/Index";
                        }

                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }                    
                }
            }

            ViewBag.Error = "Invalid login attempt";
            return View(loginView);
        }

       /* [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registration(RegistrationViewModel registrationView)
        {
            bool statusRegistration = false;
            string messageRegistration;
            if (ModelState.IsValid)
            {
                // Email Verification
                string email = Membership.GetUserNameByEmail(registrationView.Email);
                if (!string.IsNullOrEmpty(email))
                {
                    ModelState.AddModelError("Warning Email", "Sorry: Email already Exists");
                    return View(registrationView);
                }


                *//*var userName = dbContext.Users.Where(x => x.UserName == registrationView.UserName).FirstOrDefault();
                if (userName.UserName != null)
                {
                    ModelState.AddModelError("Warning Username", "Sorry: Username already Exists");
                    return View(registrationView);
                }*//*


                string alpha;
                if (registrationView.RoleId == 1)
                {
                    alpha = "ECA";
                }
                else if (registrationView.RoleId == 2)
                {
                    alpha = "ECS";
                }
                else if (registrationView.RoleId == 3)
                {
                    alpha = "ECT";
                }
                else if (registrationView.RoleId == 4)
                {
                    alpha = "ECD";
                }
                else
                {
                    alpha = "ECC";
                }

                Random random = new Random();
                int unique = random.Next(10000, 99999);
                int y = DateTime.Now.Year;
                int m = DateTime.Now.Month;
                var uniqueID = alpha + y + m + unique;

                registrationView.ActivationCode = Guid.NewGuid().ToString();

                //Save User Data 
                using (Exam_DBEntities dbContext = new Exam_DBEntities())
                {
                    var user = new User()
                    {
                        UserName = registrationView.UserName,
                        Email = registrationView.Email,
                        MobileNo = registrationView.MobileNo,
                        FirstName = registrationView.FirstName,
                        LastName = registrationView.LastName,
                        Password = registrationView.Password,
                        RoleId = registrationView.RoleId,
                        UniqueID = uniqueID,
                        CreatedDate = DateTime.Now,
                        DeletedDate = DateTime.Now,
                        CreatedBy = 0,
                        ActivationCode = registrationView.ActivationCode,
                    };

                    dbContext.Users.Add(user);
                    dbContext.SaveChanges();
                }

                //Verification Email
                VerificationEmail(registrationView.Email, registrationView.ActivationCode);
                messageRegistration = "Your account has been created successfully. ^_^";
                statusRegistration = true;
            }
            else
            {
                messageRegistration = "Something Wrong!";
            }
            ViewBag.Message = messageRegistration;
            ViewBag.Status = statusRegistration;

            return View(registrationView);
        }*/

        [HttpGet]
        public ActionResult ActivationAccount(string id)
        {
            bool statusAccount = false;
            using (Exam_DBEntities dbContext = new Exam_DBEntities())
            {
                var userAccount = dbContext.Users.Where(u => u.ActivationCode.Equals(id)).FirstOrDefault();

                if (userAccount != null)
                {
                    userAccount.IsActive = 1;
                    dbContext.SaveChanges();
                    statusAccount = true;
                }
                else
                {
                    ViewBag.Message = "Something Wrong !!";
                }

            }
            ViewBag.Status = statusAccount;
            return View();
        }

        public ActionResult LogOut()
        {
            HttpCookie cookie = new HttpCookie("Cookie1", "")
            {
                Expires = DateTime.Now.AddYears(-1)
            };
            Response.Cookies.Add(cookie);

            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account", null);
        }

        [NonAction]
        public void VerificationEmail(string email, string activationCode)
        {
            var url = string.Format("/Account/ActivationAccount/{0}", activationCode);
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, url);

            var fromEmail = new MailAddress("microtechworkdeskakash@gmail.com", "Activation Account - entrancecoach.com");
            var toEmail = new MailAddress(email);

            var fromEmailPassword = "Akash@1994";
            string subject = "Activation Account !";

            string body = "<br/> Please click on the following link in order to activate your account" + "<br/><a href='" + link + "'> Activation Account ! </a>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = true,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

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