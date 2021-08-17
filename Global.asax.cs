using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using Newtonsoft.Json;
using OnlineExam.Authentication;
using OnlineExam.Models;

namespace OnlineExam
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);            
        }

        protected FormsAuthenticationTicket GetAuthTicket()
        {
            HttpCookie authCookie = Request.Cookies["Cookie1"];
            if (authCookie == null) return null;
            try
            {
                return FormsAuthentication.Decrypt(authCookie.Value);
            }
            catch (Exception exception)
            {
                throw new Exception("Can't decrypt cookie! {0}", exception);
            }
        }
        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            var authCookie = GetAuthTicket();
            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = authCookie;

                var serializeModel = JsonConvert.DeserializeObject<CustomSerializeModel>(authTicket.UserData);

                CustomPrincipal principal = new CustomPrincipal(authTicket.Name)
                {
                    UserId = serializeModel.UserId,
                    FirstName = serializeModel.FirstName,
                    LastName = serializeModel.LastName,
                    Roles = serializeModel.RoleName.ToArray<string>()
                };

                HttpContext.Current.User = principal;
            }

        }
    }
}