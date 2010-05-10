using System;
using System.Data;
using System.Text;
using System.Web;

namespace CSMSAdmin {
	public class Select : CSMSAdmin.Page {

		private DataSet records;
		
		public override string Render() {
			if (post.Count == 0 && (String.IsNullOrEmpty(db) || String.IsNullOrEmpty(tbl))) return String.Empty;
			records = dbl.getColumnInformation(db, tbl);

			if (post.Count > 0) formatQuery();
			else showSearchForm();

			base.Render();
			return body;
		}

		private void showSearchForm() {
			if (records != null) {
				DataTable t = records.Tables[0];
				body += "<form method=\"post\"><table><tbody><tr class=\"title\"><td /><td>Name</td><td>Type</td><td>Collation</td><td>Operator</td><td>&nbsp;</td></tr>";
				for(int i = 0, l = t.Rows.Count; i < l; i++) {
					body += "<tr class=\"" + ((i % 2 == 0) ? "even" : "odd")  + "\">" +
								 "<td><input type=\"checkbox\" name=\"use_" + t.Rows[i]["name"] + "\" id=\"use_" + t.Rows[i]["name"] + "\" value=\"1\" /></td>" +
								 "<td>" +t.Rows[i]["name"] + "</td>" +
								 "<td class=\"right\">" + DisplayLayer.getDataType(t.Rows[i]["type"], t.Rows[i]["max_length"], t.Rows[i]["precision"], t.Rows[i]["scale"]) + "</td>" +
								 "<td class=\"right\">" + t.Rows[i]["collation_name"].ToString().ToLower() + "</td>" +
								 "<td class=\"right\"><select style=\"width: 110px\" name=\"op_" + t.Rows[i]["name"] + "\" id=\"op_" + t.Rows[i]["name"] + "\" onchange=\"updateSelectForm(this)\"><option value=\"\"></option>" +
									"<option value=\"eq\">=</option>" +
									"<option value=\"neq\">&lt;&gt;</option>" +
									"<option value=\"neq\">!=</option>" +
									((!t.Rows[i].IsNull("precision")) ?
										"<option value=\"lt\">&lt;</option>" +
										"<option value=\"lte\">&lt;=</option>" +
										"<option value=\"lte\">!&gt;</option>" +
										"<option value=\"gt\">&gt;</option>" +
										"<option value=\"gte\">&gt;=</option>" +
										"<option value=\"gte\">!&lt;</option>" : "") +
										"<option value=\"bet\">BETWEEN</option>" +
									((!t.Rows[i].IsNull("max_length")) ? 
										"<option value=\"l\">LIKE</option>" +
										"<option value=\"nl\">NOT LIKE</option>" : "") +
									"<option value=\"all\">ALL</option>" +
									"<option value=\"any\">ANY</option>" +
									"<option value=\"exi\">EXISTS</option>" +
									"<option value=\"i\">IN</option>" +
									"<option id=\"som\" value\"SOME\">SOME</option>" +
									((t.Rows[i]["is_nullable"].ToString().ToLower().Trim().Equals("yes")) ? 
										"<option value=\"isn\">IS NULL</option>" +
										"<option value=\"inn\">IS NOT NULL</option>" : "") +
									"</select></td>" +
								 "<td>" + ((!t.Rows[i].IsNull("max_length") &&
									(t.Rows[i]["max_length"].ToString().Trim().ToLower().Equals("max") ||
									 int.Parse(t.Rows[i]["max_length"].ToString()) > 256)) ?
										"<textarea name=\"ex_" + t.Rows[i]["name"] + "\" cols=\"41\" rows=\"5\" onkeyup=\"updateSelectForm(this)\"></textarea>" :  
										"<input name=\"ex_" + t.Rows[i]["name"] + "\" type=\"text\" size=\"40\" maxlength=\"" + t.Rows[i]["max_length"] + "\" onkeyup=\"updateSelectForm(this)\" />") + "</td></tr>";
				}
				body += "<tr><td colspan=\"6\">Edit WHERE clause:<br /><input type=\"text\" name=\"where\" style=\"width: 100%\" /></td></tr>" +
							 "<tr><td class=\"right\" colspan=\"6\"><input type=\"submit\" value=\"Submit\" /></td></tr></tbody></table></form>";
			}
		}

		private void formatQuery() {
			DataTable t = records.Tables[0];
			StringBuilder qsb = new StringBuilder();

			for (int i = 0, l = t.Rows.Count; i < l; i++) {
				if (post["use_" + t.Rows[i]["name"]] == null) continue;

				if (qsb.Length == 0) qsb.Append("SELECT * FROM " + tbl + " WHERE " + ((!String.IsNullOrEmpty(post["where"])) ? post["where"] + " AND" : ""));
				else qsb.Append(" AND");

				qsb.Append(" "  + t.Rows[i]["name"] + " " + LookupTables.comparisonOperators(post["op_" + t.Rows[i]["name"]]) + " '" + post["ex_" + t.Rows[i]["name"]] + "'");
			}

			response.Redirect("query.aspx?a=" + session.SessionID + "&db=" + db + "&tbl=" + tbl + "&q=" + HttpUtility.UrlEncode(qsb.ToString()));
		}
	}
}
