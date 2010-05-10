using System;
using System.Data;
using System.Web;

namespace CSMSAdmin {
	public class Browse : CSMSAdmin.Page {

		public override string Render() {
			if (db == null || tbl == null) return String.Empty;

			int pg = (qs["p"] != null) ? int.Parse(qs["p"]) : 0;
			int cnt = (qs["c"] != null) ? int.Parse(qs["c"]) : 30;

			dbl.SaveLastQuery = true;
			DataSet records = dbl.getRecordsFromTable(db, tbl, pg * cnt, cnt);
			body += "Returned " + Convert.ToString(records.Tables[0].Rows.Count) + " rows<br />";

			body += DisplayLayer.getQueryBox(session.SessionID, db, tbl, dbl.LastQuery) +
					  DisplayLayer.getBrowseTableNavigation(session.SessionID, db, tbl, dbl.getRowCount(db, tbl), pg, cnt);

			string headers = String.Empty, tableBody = String.Empty;

			int rows = records.Tables[0].Rows.Count;
			for (int i = 0; i < rows; i++) {
				DataRow r = records.Tables[0].Rows[i];
				string rowBody = String.Empty, args = String.Empty;

				for (int j = 0, cols = records.Tables[0].Columns.Count; j < cols; j++) {
					DataColumn c = records.Tables[0].Columns[j];

					bool dbnull = r[c.ColumnName].GetType().Name == "DBNull";
					if (i == 0) headers += "<td><a href=\"query.aspx?a=" + session.SessionID + "&db=" + db + "&tbl=" + tbl + "&q=" + HttpUtility.UrlEncode("SELECT TOP 30 * FROM " + tbl + " ORDER BY " + c.ColumnName) + "\">" + c.ColumnName + "</a></td>";
					rowBody += "<td>" + ((dbnull) ? "NULL" : HttpUtility.HtmlEncode(r[c.ColumnName].ToString())) + "</td>";
					args += ((j > 0) ? " AND " : "") + c.ColumnName + ((dbnull) ? " IS NULL" : " = '" + r[c.ColumnName] + "'");
				}
				tableBody += "<tr class=\"" + ((i % 2 == 0) ? "even" : "odd")  + "\"><td><input type=\"checkbox\" /></td><td><img src=\"themes/" + HttpUtility.UrlEncode(HttpContext.Current.Session["theme"].ToString()) + "/img/edit.png\" /></td>" +
							 "<td><a href=\"query.aspx?a=" + session.SessionID + "&db=" + db + "&tbl=" + tbl + "&q=" + HttpUtility.UrlEncode("DELETE FROM " + tbl + " WHERE " + args) + "\"><img src=\"themes/" + HttpUtility.UrlEncode(HttpContext.Current.Session["theme"].ToString()) + "/img/drop.png\" /></a></td>" +
							 rowBody + "</tr>";
			}

			body += "<table><tbody><tr class=\"title\"><td colspan=\"3\">&nbsp;</td>" + headers + "</tr>" + tableBody + "</tbody></table>";

			base.Render();
			return body;
		}
	}
}
