using System;
using System.Data;

namespace CSMSAdmin {
	public class Status : CSMSAdmin.Page {
		public override string Render() {
			DataTable optimizations = dbl.getOptimizations();

			string headers = String.Empty, tableBody = String.Empty;
			for (int i = 0, rows = optimizations.Rows.Count; i < rows; i++) {
				DataRow r = optimizations.Rows[i];

				tableBody += "<tr class=\"" + ((i % 2 == 0) ? "even" : "odd") + "\">";
				foreach (DataColumn c in optimizations.Columns) {
					if (i == 0) headers += "<td>" + c.ColumnName + "</td>";
					tableBody += "<td>" + ((r[c.ColumnName].GetType().Name == "DBNull") ? "NULL" : r[c.ColumnName]) + "</td>";
				}
				tableBody += "</tr>";
			}
			body += "Optimizations:<br /><table><tbody><tr class=\"title\">" + headers + "</tr>" + tableBody + "</tbody></table>";

			base.Render();
			return body;
		}
	}
}
