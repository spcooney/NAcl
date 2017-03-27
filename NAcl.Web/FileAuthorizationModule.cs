using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Security;

namespace NAcl.Web
{
    class FileAuthorizationModule : IHttpModule
    {
        #region IHttpModule Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Init(HttpApplication context)
        {
            context.AuthorizeRequest += new EventHandler(context_AuthorizeRequest);
        }

        void context_AuthorizeRequest(object sender, EventArgs e)
        {
            HttpContext context = HttpContext.Current;
            if (!context.SkipAuthorization)
            {
                if (!AclManager.IsAllowed(context.Request.Url.AbsolutePath, context.Request.HttpMethod, Roles.GetRolesForUser()))
                {
                    context.Response.StatusCode = 401;
                    WriteErrorMessage(context);
                    ((HttpApplication)sender).CompleteRequest();
                }
            }
        }

        private void WriteErrorMessage(HttpContext context)
        {
            context.Response.Write("<html>");
            context.Response.Write("<head>");
            context.Response.Write("<title>");
            context.Response.Write("File access not allowed");
            context.Response.Write("</title>");
            context.Response.Write("</head>");
            context.Response.Write("<body>");
            context.Response.Write("<h1>You are not allowed to access to this file</h1>");
            context.Response.Write("</body>");
            context.Response.Write("</html>");
        }

        #endregion
    }
}
