using System;
using System.Data;

namespace CSMSAdmin {
	public class Configuration : CSMSAdmin.Page {
		public override string Render() {
			DataTable configuration = dbl.getConfiguration();

			string headers = String.Empty, tableBody = String.Empty;
			for (int i = 0, rows = configuration.Rows.Count; i < rows; i++) {
				DataRow r = configuration.Rows[i];

				tableBody += "<tr class=\"" + ((i % 2 == 0) ? "even" : "odd") + "\">";
				foreach (DataColumn c in configuration.Columns) {
					if (i == 0) headers += "<td>" + c.ColumnName + "</td>";
					tableBody += "<td>" + ((r[c.ColumnName].GetType().Name == "DBNull") ? "NULL" : r[c.ColumnName]) + "</td>";
				}
				tableBody += "</tr>";
			}
			body += "<table><tbody><tr class=\"title\">" + headers + "</tr>" + tableBody + "</tbody></table>";

			base.Render();
			return body;
		}
	}
}
