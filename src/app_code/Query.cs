using System;
using System.Data;
using System.Web;

namespace CSMSAdmin {
	public class Query : CSMSAdmin.Page {
		public override string Render() {
			bool show_plan = qs["sp"] != null && String.Compare(qs["sp"], "1") == 0;
			string q = HttpUtility.UrlDecode(qs["q"]);

			body += DisplayLayer.getQueryInput(session.SessionID, db, tbl, q);
			   
			if (qs["q"] == null || qs["q"].Length == 0) {
				base.Render();
				return body;
			}

			body += DisplayLayer.getQueryBox(session.SessionID, db, tbl, q);

			DataSet result = dbl.executeQuery(db, q);

			foreach (DataTable t in result.Tables) {
				body += "<table><tbody><tr class=\"title\">";
				foreach (DataColumn c in t.Columns) body += "<td><a href=\"query.aspx?a=" + session.SessionID + "&db=" + db + "&tbl=" + tbl + "&q=" + HttpUtility.UrlEncode("SELECT TOP 30 * FROM " + tbl + " ORDER BY " + c.ColumnName) + "\">" + c.ColumnName + "</a></td>";
				body += "</tr>";

				foreach (DataRow r in t.Rows) {
					body += "<tr>";
					foreach(DataColumn c in t.Columns) body += "<td>" + ((r[c.ColumnName].GetType().Name == "DBNull") ? "NULL" : HttpUtility.HtmlEncode(r[c.ColumnName].ToString()).Replace(Environment.NewLine, "<br />")) + "</td>";
					body += "</tr>";
				}
				body += "</tbody></table><br />";
			}

			base.Render();
			return body;
		}
	}
}

