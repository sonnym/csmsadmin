using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

partial class Handler : System.Web.IHttpHandler {
	private Page p;

	public Handler() {
		p = new Page();
		p.AppRelativeVirtualPath = "~";
		p.PreInit += this.pre;
	}

	private void pre(object sender, EventArgs e) {
		string url = HttpContext.Current.Request.ServerVariables["URL"];
		DBLayer dbl = new DBLayer();
		NameValueCollection qs = HttpContext.Current.Request.QueryString;

		switch(url) {
			case "/":
				p.MasterPageFile = "~/masters/layout.master";	
				((HtmlGenericControl)p.Master.FindControl("body")).Visible = false;
				((HtmlGenericControl)p.Master.FindControl("frameset")).Visible = true;
				return;
			case "/charsets.aspx":
				p.MasterPageFile = "~/masters/charsets.master";
				break;
			case "/configuration.aspx":
				p.MasterPageFile = "~/masters/configuration.master";
				break;
			case "/browse.aspx":
				p.MasterPageFile = "~/masters/browse.master";
				break;
			case "/default.aspx":
				p.MasterPageFile = "~/masters/layout.master";
				((HtmlGenericControl)p.Master.FindControl("body")).Visible = false;
				((HtmlGenericControl)p.Master.FindControl("frameset")).Visible = true;
				return;
			case "/home.aspx":
				p.MasterPageFile = "~/masters/home.master";
				break;
			case "/insert.aspx":
				p.MasterPageFile = "~/masters/insert.master";
				break;
			case "/navigation.aspx":
				p.MasterPageFile = "~/masters/navigation.master";
				return;
			case "/operations.aspx":
				p.MasterPageFile = "~/masters/operations.master";
				break;
			case "/processes.aspx":
				p.MasterPageFile = "~/masters/processes.master";
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
			case "/struct.aspx":
				p.MasterPageFile = "~/masters/struct.master";
				if (!String.IsNullOrEmpty(qs["fn"]) && qs["fn"].Equals("create")) return;
				break;
			default:
				p.MasterPageFile = "~/masters/layout.master";
				((HtmlGenericControl)p.Master.FindControl("body")).InnerHtml = DisplayLayer.GetLocation(dbl.getServerName(), qs["db"], qs["tbl"]) +
																			   DisplayLayer.GetTopTabs(LookupTables.pages(url), qs["db"], qs["tbl"]) +
																			   "<br />Invalid URL";
				return;
		}

		((HtmlGenericControl)p.Master.Master.FindControl("body")).InnerHtml = DisplayLayer.GetLocation(dbl.getServerName(), qs["db"], qs["tbl"]) +
																			  DisplayLayer.GetTopTabs(LookupTables.pages(url), qs["db"], qs["tbl"]);
	}

	public bool IsReusable {
		get { return false; }
	}
	public void ProcessRequest(HttpContext c) {
		try {
			p.ProcessRequest(c);
		} catch(Exception ex) { // System.Web.HttpUnhandledException is predominant
			if (ex.InnerException != null && ex.InnerException.GetType().ToString().Equals("System.Data.SqlClient.SqlException")) {
				HttpContext.Current.Response.Write("SQL Error Occurred: " + ex.InnerException.Message);
			} else HttpContext.Current.Response.Write(ex.ToString().Replace(Environment.NewLine, "<br />") + "<br />");
		}
	}
}
