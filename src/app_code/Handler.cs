using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.HtmlControls;

partial class Handler : IHttpHandler, IRequiresSessionState {
	private Page p;
	private HttpContext context = HttpContext.Current;

	public Handler() {
		p = new Page();
		p.AppRelativeVirtualPath = "~";
		p.PreInit += this.pre;
	}

	private void pre(object sender, EventArgs e) {
		if (context.Response.Cookies.Count > 0) context.Response.Cookies[0].HttpOnly = true; // only cookie is for session and is present at this point only if no session exists
		string url = context.Request.ServerVariables["URL"].ToLower();

		if (!String.IsNullOrEmpty(context.Request.Form["login"])) login();

		if (context.Session["cs"] == null || context.Session.SessionID != context.Request.QueryString["a"]) {
			if (Settings.DisableLoginPage) context.Session["cs"] = Settings.ConnectionString;
			else {
				p.MasterPageFile = "~/masters/login.master";
				context.Session.Abandon();
				return;
			}
		}

		if (context.Session["theme"] == null) context.Session.Add("theme", Settings.DefaultTheme);

		DBLayer dbl = new DBLayer();
		NameValueCollection qs = context.Request.QueryString;

		switch(url) {
			case "/":
				p.MasterPageFile = "~/masters/struct.master";
				break;
			case "/charsets.aspx":
				p.MasterPageFile = "~/masters/charsets.master";
				break;
			case "/configuration.aspx":
				p.MasterPageFile = "~/masters/configuration.master";
				break;
			case "/browse.aspx":
				p.MasterPageFile = "~/masters/browse.master";
				break;
			case "/browse_srv.aspx":
				p.MasterPageFile = "~/masters/browse_srv.master";
				return;
			case "/default.aspx":
				p.MasterPageFile = "~/masters/struct.master";
				break;
			case "/insert.aspx":
				p.MasterPageFile = "~/masters/insert.master";
				break;
			case "/operations.aspx":
				p.MasterPageFile = "~/masters/operations.master";
				break;
			case "/permissions.aspx":
				p.MasterPageFile = "~/masters/permissions.master";
				break;
			case "/processes.aspx":
				p.MasterPageFile = "~/masters/processes.master";
				break;
			case "/providers.aspx":
				p.MasterPageFile = "~/masters/providers.master";
				break;
			case "/query.aspx":
				p.MasterPageFile = "~/masters/query.master";
				break;
			case "/restore.aspx":
				p.MasterPageFile = "~/masters/restore.master";
				break;
			case "/select.aspx":
				p.MasterPageFile = "~/masters/select.master";
				break;
			case "/status.aspx":
				p.MasterPageFile = "~/masters/status.master";
				break;
			case "/struct.aspx":
				p.MasterPageFile = "~/masters/struct.master";
				if (!String.IsNullOrEmpty(qs["fn"]) && qs["fn"].Equals("create")) return;
				break;
			default:
				p.MasterPageFile = "~/masters/layout.master";
				((HtmlGenericControl)p.Master.FindControl("body")).InnerHtml = DisplayLayer.getLocation(dbl.getServerName(), qs["db"], qs["tbl"]) +
																			   DisplayLayer.getTopTabs(LookupTables.pages(url), qs["db"], qs["tbl"]) +
																			   "<br />Invalid URL";
				return;
		}

		((HtmlGenericControl)p.Master.Master.FindControl("body")).InnerHtml = DisplayLayer.getLocation(dbl.getServerName(), qs["db"], qs["tbl"]) +
																			  DisplayLayer.getTopTabs(LookupTables.pages(url), qs["db"], qs["tbl"]);
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
	private void login() {
		DBLayer dbl = new DBLayer();
		string cs = String.Format(Settings.LoginConnectionString, dbl.getServerDataSource(int.Parse(context.Request.Form["server"])), context.Request.Form["user"], context.Request.Form["password"]);
		context.Response.Write(dbl.testConnectionString(cs));
		if (!dbl.testConnectionString(cs)) context.Response.Write("Login faled, please try again.");
		else {
			context.Session["cs"] = cs;
			context.Response.Redirect("~/?a=" + context.Session.SessionID);
		}
	}
}
