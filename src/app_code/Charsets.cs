using System.Data;

namespace CSMSAdmin {
	public class Charsets : CSMSAdmin.Page {
		public override string Render() {
			DataSet result = dbl.getCharsets();
			DataTable collations = dbl.getCollations();

			DataTable charsets = result.Tables[0];
			DataTable sortorders = result.Tables[1];
			int i = 0, j = 0, curcs = -1, solen = sortorders.Rows.Count;

			body += "Charsets:<br /><table><tbody><tr class=\"title\"><td>Name</td><td>Description</td></tr>";
			for (int len = charsets.Rows.Count; i < len; i++) {
				curcs = int.Parse(charsets.Rows[i][0].ToString());
				body += "<tr class=\"bold " + (((i + j) % 2 == 0) ? "even" : "odd") + "\"><td>" + charsets.Rows[i][1].ToString() + "</td><td>" + charsets.Rows[i][2].ToString() + "</td></tr>";
				while (j < solen && int.Parse(sortorders.Rows[j][0].ToString()) == curcs) {
					body += "<tr class=\"" + (((i + j + 1) % 2 == 0) ? "even" : "odd") + "\"><td>" + sortorders.Rows[j][1].ToString() + "</td><td>" + sortorders.Rows[j][2] + "</td></tr>";
					j++;
				}
			}
			body += "</tbody></table><br /><br />Collations:<br /><table><tbody><tr class=\"title\"><td>Name</td><td>Description</td></tr>";
			for (int k = 0, len = collations.Rows.Count; k < len; k++) {
				body += "<tr class=\"" + ((k % 2 == 0) ? "even" : "odd") + "\"><td>" + collations.Rows[k][0].ToString() + "</td><td>" +  collations.Rows[k][1].ToString() + "</td></tr>";
			}
			body += "</tbody></table>";

			base.Render();
			return body;
		}
	}
}
