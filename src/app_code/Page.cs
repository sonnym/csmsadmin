using System.Collections.Specialized;
using System.Web;
using System.Web.SessionState;

namespace CSMSAdmin {
	public abstract class Page {
		protected DBLayer dbl = new DBLayer();
		protected HttpRequest request;
		protected HttpResponse response;
		protected System.Web.UI.Page page;
		protected NameValueCollection post, qs;
		protected string body, db, tbl, url;
		protected HttpSessionState session;

		public Page() {
			request = HttpContext.Current.Request;
			response = HttpContext.Current.Response;
			qs = request.QueryString;
			post = request.Form;
			db = qs["db"];
			tbl = qs["tbl"];
			url = request.ServerVariables["URL"].ToLower();
			session = HttpContext.Current.Session;
			page = HttpContext.Current.Handler as System.Web.UI.Page;
		}

		public virtual string Render() {
			body = DisplayLayer.getLocation(session.SessionID, dbl.getServerName(), db, tbl) +
					DisplayLayer.getTopTabs(session.SessionID, LookupTables.pages(url), db, tbl) +
					body;
			return body;
		}
	}
}
