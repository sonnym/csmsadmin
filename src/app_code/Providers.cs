using System.Data;

namespace CSMSAdmin {
	public class Providers : CSMSAdmin.Page {
		public override string Render() {
			DataTable providers = dbl.getProviders();

			body += "<table><tbody><tr class=\"title\"><td>Name</td><td>Description</td></tr>";
			for (int i = 0, rows = providers.Rows.Count; i < rows; i++) {
				DataRow r = providers.Rows[i];
				body += "<tr class=\"" + ((i % 2 == 0) ? "even" : "odd") + "\"><td>" + r["Provider Name"] + "</td><td>" + r["Provider Description"] + "</td></tr>";
			}
			body += "</tbody></table>";

			base.Render();
			return body;
		}
	}
}
