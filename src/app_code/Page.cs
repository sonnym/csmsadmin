using System.Collections.Specialized;
using System.Web;
using System.Web.SessionState;
using System.Web.UI.HtmlControls;

namespace CSMSAdmin {
	public class Page {
		protected DBLayer dbl = new DBLayer();
		protected HttpRequest request;
		protected HttpResponse response;
		protected System.Web.UI.Page page;
		protected NameValueCollection post, qs;
		protected string body, db, tbl, url;
		protected HttpSessionState session;

		protected bool isAJAX = false;

		public Page() {
			request = HttpContext.Current.Request;
			response = HttpContext.Current.Response;
			qs = request.QueryString;
			post = request.Form;
			db = qs["db"];
			tbl = qs["tbl"];
			session = HttpContext.Current.Session;
			page = HttpContext.Current.Handler as System.Web.UI.Page;

			url = request.ServerVariables["URL"];
			url = url.Substring(url.LastIndexOf('/'), url.Length - url.LastIndexOf('/')).ToLower();
		}

		public Page(ref System.Web.UI.Page p) : this() {
			CSMSAdmin.Page renderer;

			switch(url) {
				case "/":
				case "/default.aspx":
				case "/struct.aspx":
					renderer = new CSMSAdmin.Structure();
					break;
				case "/backup.aspx":
					renderer = new CSMSAdmin.Backup();
					break;
				case "/browse.aspx":
					renderer = new CSMSAdmin.Browse();
					break;
				case "/browse_srv.aspx":
					renderer = new CSMSAdmin.ServerBrowser();
					break;
				case "/charsets.aspx":
					renderer = new CSMSAdmin.Charsets();
					break;
				case "/configuration.aspx":
					renderer = new CSMSAdmin.Configuration();
					break;
				case "/insert.aspx":
					renderer = new CSMSAdmin.Insert();
					break;
				case "/operations.aspx":
					renderer = new CSMSAdmin.Operations();
					break;
				case "/permissions.aspx":
					renderer = new CSMSAdmin.Permissions();
					break;
				case "/processes.aspx":
					renderer = new CSMSAdmin.Processes();
					break;
				case "/providers.aspx":
					renderer = new CSMSAdmin.Providers();
					break;
				case "/query.aspx":
					renderer = new CSMSAdmin.Query();
					break;
				case "/restore.aspx":
					renderer = new CSMSAdmin.Restore();
					break;
				case "/select.aspx":
					renderer = new CSMSAdmin.Select();
					break;
				case "/status.aspx":
					renderer = new CSMSAdmin.Status();
					break;
				default:
					((HtmlGenericControl)p.Master.FindControl("body")).InnerHtml = DisplayLayer.getLocation(session.SessionID, dbl.getServerName(), db, tbl) +
																				   DisplayLayer.getTopTabs(session.SessionID, LookupTables.pages(url), db, tbl) +
																				   "<br />Invalid URL";
					return;
			}

			((HtmlGenericControl)p.Master.FindControl("body")).InnerHtml = renderer.Render();

			if (renderer.isAJAX) {
				p.Visible = false;
			}
		}

		public virtual string Render() {
			body = DisplayLayer.getLocation(session.SessionID, dbl.getServerName(), db, tbl) +
					DisplayLayer.getTopTabs(session.SessionID, LookupTables.pages(url), db, tbl) +
					body;
			return body;
		}
	}
}
