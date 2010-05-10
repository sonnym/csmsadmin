using System;
using System.Data;
using System.Web;

namespace CSMSAdmin {
	public class Processes : CSMSAdmin.Page {
		public override string Render() {
			DataTable processes = dbl.getProcesses();

			string headers = String.Empty, tableBody = String.Empty;
			for (int i = 0, rows = processes.Rows.Count; i < rows; i++) {
				DataRow r = processes.Rows[i];

				tableBody += "<tr class=\"" + ((i % 2 == 0) ? "even" : "odd") + "\"><td><a href=\"query.aspx?a=" + session.SessionID + "&q=" + HttpUtility.UrlEncode("KILL " + r[0].ToString()) + "\">KILL</a></td>";
				foreach (DataColumn c in processes.Columns) {
					if (i == 0) headers += "<td>" + c.ColumnName + "</td>";
					tableBody += "<td>" + ((r[c.ColumnName].GetType().Name == "DBNull") ? "NULL" : r[c.ColumnName]) + "</td>";
				}
				tableBody += "</tr>";
			}
			body += "<table><tbody><tr class=\"title\"><td />" + headers + "</tr>" + tableBody + "</tbody></table>";

			base.Render();
			return body;
		}
	}
}
