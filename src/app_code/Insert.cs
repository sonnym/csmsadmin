using System;
using System.Data;
using System.Text;
using System.Web;

namespace CSMSAdmin {
	public class Insert : CSMSAdmin.Page {
		private DataSet records;
		
		public override string Render() {
			if (post.Count == 0 && (String.IsNullOrEmpty(db) || String.IsNullOrEmpty(tbl))) return String.Empty;

			records = dbl.getColumnInformation(db, tbl);

			if (post.Count > 0) generateQuery();
			else showInsertForm();

			base.Render();
			return body;
		}

		private void showInsertForm() {
			if (records != null) {
				DataTable t = records.Tables[0];
				body += "<form method=\"post\"><table><tbody><tr class=\"title\"><td>Name</td><td>Type</td><td>Collation</td><td>Null</td><td>&nbsp;</td></tr>";
				for(int i = 0, l = t.Rows.Count; i < l; i++) {
					body += "<tr class=\"" + ((i % 2 == 0) ? "even" : "odd")  + "\">" +
								 "<td>" +t.Rows[i]["name"] + "</td>" +
								 "<td class=\"right\">" + DisplayLayer.getDataType(t.Rows[i]["type"], t.Rows[i]["max_length"], t.Rows[i]["precision"], t.Rows[i]["scale"]) + "</td>" +
								 "<td class=\"right\">" + t.Rows[i]["collation_name"].ToString().ToLower() + "</td>" +
								 "<td class=\"right\">" + ((t.Rows[i]["is_nullable"].ToString().ToLower().Trim().Equals("true")) ?
															"<input type=\"checkbox\" name=\"null_" + t.Rows[i]["name"] + "\" value=\"null\" />" : "") + "</td>" +
								 "<td>" +
									((int.Parse(t.Rows[i]["max_length"].ToString()) != 0 && (int.Parse(t.Rows[i]["max_length"].ToString()) == -1 || int.Parse(t.Rows[i]["max_length"].ToString()) > 256)) ?
										"<textarea name=\"ex_" + t.Rows[i]["name"] + "\" cols=\"41\" rows=\"5\">" + DisplayLayer.stripDefaults(t.Rows[i]["default"].ToString()) + "</textarea>" :
										"<input name=\"ex_" + t.Rows[i]["name"] + "\" type=\"text\" value=\"" + DisplayLayer.stripDefaults(t.Rows[i]["default"].ToString())  + "\" size=\"40\" maxlength=\"" + t.Rows[i]["max_length"] + "\" />")  + "</td>" +
								"</tr>";
				}
				body += "<tr><td class=\"right\" colspan=\"6\"><input type=\"submit\" value=\"Submit\" /></td></tr></tbody></table></form>";
			}
		}

		private void generateQuery() {
			DataTable t = records.Tables[0];
			StringBuilder cols = new StringBuilder(), vals = new StringBuilder();

			for (int i = 0, l = t.Rows.Count; i < l; i++) {
				if (bool.Parse(t.Rows[i]["is_identity"].ToString())) continue; // Simple way of handling identities, but unlikely to fit all cases
				if (post["ex_" + t.Rows[i]["name"]].Equals(DisplayLayer.stripDefaults(t.Rows[i]["default"].ToString()))) continue; // Simple way of handling defaults values wrapped in parentheses but also likely incomplete

				cols.Append(((cols.Length == 0) ? "" : ",") + t.Rows[i]["name"]);
				vals.Append(((vals.Length == 0) ? "" : ",") + ((String.IsNullOrEmpty(post["null_" + t.Rows[i]["name"]])) ?  "'" + post["ex_" + t.Rows[i]["name"]] + "'" : "NULL"));
			}

			response.Redirect("query.aspx?a=" + session.SessionID + "&db=" + db + "&tbl=" + tbl + "&q=" + HttpUtility.UrlEncode("INSERT INTO " + tbl + "(" + cols.ToString() + ") VALUES(" + vals.ToString() + ")"));
		}
	}
}
