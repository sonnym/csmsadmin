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

		DBLayer dbl = new DBLayer();

		string url = context.Request.ServerVariables["URL"].ToLower();
		switch(url) {
			case "/":
				p.MasterPageFile = "~/masters/struct.master";
				break;
			case "/backup.aspx":
				p.MasterPageFile = "~/masters/backup.master";
				break;
			case "/browse.aspx":
				p.MasterPageFile = "~/masters/browse.master";
				break;
			case "/browse_srv.aspx":
				p.MasterPageFile = "~/masters/browse_srv.master";
				return;
			case "/charsets.aspx":
				p.MasterPageFile = "~/masters/charsets.master";
				break;
			case "/configuration.aspx":
				p.MasterPageFile = "~/masters/configuration.master";
				break;
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
				((HtmlGenericControl)p.Master.FindControl("body")).InnerHtml = DisplayLayer.getLocation(context.Session.SessionID, dbl.getServerName(), qs["db"], qs["tbl"]) +
																			   DisplayLayer.getTopTabs(context.Session.SessionID, LookupTables.pages(url), qs["db"], qs["tbl"]) +
																			   "<br />Invalid URL";
				return;
		}

		((HtmlGenericControl)p.Master.Master.FindControl("body")).InnerHtml = DisplayLayer.getLocation(context.Session.SessionID, dbl.getServerName(), qs["db"], qs["tbl"]) +
																			  DisplayLayer.getTopTabs(context.Session.SessionID, LookupTables.pages(url), qs["db"], qs["tbl"]);
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

		context.Response.Redirect("~/?a=" + context.Session.SessionID);
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

			context.Response.Redirect("~/?a=" + context.Session.SessionID);
		} 
		else {
			context.Response.Write("Login faled, please try again.");
			p.MasterPageFile = "~/masters/login.master";
		}
	}
}
