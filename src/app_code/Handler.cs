using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;

namespace CSMSAdmin {
	partial class Handler : IHttpHandler, IRequiresSessionState {
		private System.Web.UI.Page p;
		private HttpContext context = HttpContext.Current;
		private CSMSAdmin.Page renderer;

		public Handler() {
			p = new System.Web.UI.Page();
			p.AppRelativeVirtualPath = "~";
			p.MasterPageFile = "~/masters/layout.master";
			p.PreInit += this.pre;
		}

		private void pre(object sender, EventArgs e) {
			if (context.Response.Cookies.Count > 0) context.Response.Cookies[0].HttpOnly = true; // only cookie is for session and is present at this point only if no session exists

			NameValueCollection qs = context.Request.QueryString;

			// ensure user is logged in
			if (Settings.DisableLoginPage && context.Session["cs"] == null) {
				login(false);
			} else if (!String.IsNullOrEmpty(context.Request.Form["login"])) {
				login(true);
				return;
			} else if (!String.IsNullOrEmpty(context.Request.Form["loginacs"])) {
				login(context.Request.Form["cs"]);
			} else if (!Settings.DisableLoginPage && (context.Session["cs"] == null || context.Session.SessionID != qs["a"])) {
				context.Session.Abandon();
				p.MasterPageFile = "~/masters/login.master";
				return;
			}

			renderer = new CSMSAdmin.Page(ref p);
			renderer.Render();
		}

		public bool IsReusable {
			get { return false; }
		}
		public void ProcessRequest(HttpContext c) {
			try {
				p.ProcessRequest(c);
			} catch(Exception ex) { // System.Web.HttpUnhandledException is predominant
				if (ex.InnerException != null && ex.InnerException.GetType().ToString().Equals("System.Data.SqlClient.SqlException")) {
					context.Response.Write("SQL Error Occurred: " + ex.InnerException.Message);
				} else context.Response.Write(ex.ToString().Replace(Environment.NewLine, "<br />") + "<br />");
			}
		}

		  ///////////////////////
		 // private functions //
		///////////////////////
		private void login(string cs) {
			context.Session.Clear();
			context.Session.Add("cs", cs);
			context.Session.Add("theme", Settings.DefaultTheme);

			context.Response.Redirect("~/default.aspx?a=" + context.Session.SessionID);
		}
		private void login(bool useform) {
			DBLayer dbl = new DBLayer();
			string cs = useform ?
						 String.Format(Settings.LoginConnectionString, dbl.getServerDataSource(int.Parse(context.Request.Form["server"])), context.Request.Form["user"], context.Request.Form["password"]) :
						 Settings.ConnectionString;
			if (!useform || dbl.testConnectionString(cs)) {
				context.Session.Clear();
				context.Session.Add("cs", cs);
				context.Session.Add("theme", Settings.DefaultTheme);

				context.Response.Redirect("~/default.aspx?a=" + context.Session.SessionID);
			} 
			else {
				context.Response.Write("Login faled, please try again.");
				p.MasterPageFile = "~/masters/login.master";
			}
		}
	}
}
