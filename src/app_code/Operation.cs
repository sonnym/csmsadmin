using System;
using System.Web;

namespace CSMSAdmin {
	public class Operations : CSMSAdmin.Page {

		public override string Render() {
			if (post["create_db"] != null) _createDB();
			else if (post["create_tbl"] != null) _createTable();

			if (String.IsNullOrEmpty(db)) body += DisplayLayer.getCreateNewDatabase();
			else body += DisplayLayer.getCreateNewTable(db);

			base.Render();
			return body;
		}

		private void _createDB() {
			if (post["name"].Equals("")) {
				body += "<span class=\"error\">You must supply a name for the new database.</span><br />" + DisplayLayer.getCreateNewDatabase();
				return;
			}
			bool created = dbl.createDatabase(post["name"]);
			if (created) response.Redirect("struct.aspx?a=" + session.SessionID + "&db=" + HttpUtility.UrlEncode(post["name"]) + "&tbl=");
			else body += "<span class=\"error\">Something failed</span>";
		}

		private void _createTable() {
			if (post["name"].Equals("")) {
				body += "<span class=\"error\">You must supply a name for the new table.</span><br />" + DisplayLayer.getCreateNewTable(post["db"]);
				return;
			}
			response.Redirect("struct.aspx?a=" + session.SessionID + "&db=" + post["db"] + "&tbl=" + HttpUtility.UrlEncode(post["name"]) + "&fn=create&fields=" + post["fields"]);
		}
	}
}
