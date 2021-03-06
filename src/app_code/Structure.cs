using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web;

namespace CSMSAdmin {
	public class Structure : CSMSAdmin.Page {

		public override string Render() {
			if (!String.IsNullOrEmpty(post["theme"])) session.Add("theme", post["theme"]);
			else if (!String.IsNullOrEmpty(post["create_tbl"])) tbl_create();
			else if (!String.IsNullOrEmpty(request.QueryString["fn"]) && request.QueryString["fn"].Equals("create")) col_struct();
			else if (String.IsNullOrEmpty(db)) srv_struct();
			else if (String.IsNullOrEmpty(tbl)) db_struct();
			else tbl_struct();

			if (!String.IsNullOrEmpty(qs["fn"]) && qs["fn"].Equals("create")) return body; // do not display navigation when creating a new table

			base.Render();
			return body;
		}

		private void srv_struct() {
			body += "MSSQL Version: " + dbl.getServerVersion() + "<br />SQL User: " + dbl.getUsername() + "<br /><br />" +
						"Web Server: " + request.ServerVariables["SERVER_SOFTWARE"] + "<br />" +
						"Identity: " + System.Security.Principal.WindowsIdentity.GetCurrent().Name + "<br /><br />" +
						"Theme: " + DisplayLayer.getThemeSelect();
		}

		private void db_struct() {
			ArrayList tbls = dbl.getTableNames(db);
			DataTable procedures = dbl.getStoredProcedures(db);
			DataTable views = dbl.getViews(db);

			bool are_tbls = tbls != null && tbls.Count > 0;
			bool are_procedures = procedures != null && procedures.Rows.Count > 0;
			bool are_views = views != null && views.Rows.Count > 0;
			bool db_empty = !(are_tbls || are_procedures || are_views);

			if (db_empty) {
				body += "Database is empty";
				return;
			}

			body += "<a name=\"top\" />";
			if (!db_empty) body += "Go To: ";
			if (are_tbls) body += "<a href=\"#tables\">Tables</a>";
			if (are_procedures) body += "&nbsp;<a href=\"#procedures\">Stored Procedures</a>";
			if (are_views) body += "&nbsp;<a href=\"#views\">Views</a>";

			if (tbls != null) {
				body += "<a name=\"tables\" /><table><tbody><tr class=\"title\"><td>&nbsp;</td><td>Name</td><td colspan=\"6\">Actions</td><td>Rows</td><td>Size</td></tr>";
				for (int i = 0, l = tbls.Count; i < l; i++) {
					string tbl = tbls[i].ToString();
					Dictionary<string, string> size = dbl.getTableSize(db, tbl);
					body += "<tr class=\"" + ((i % 2 == 0) ? "even" : "odd")  + "\">" +
								 "<td><input type=\"checkbox\" id=\"" + db + "_" + tbl + "\"</td>" +
								 "<td><a href=\"struct.aspx?a=" + session.SessionID + "&db=" + db + "&tbl=" + tbl + "\">" + tbl + "</a></td>" +
								 "<td><a href=\"browse.aspx?a=" + session.SessionID + "&db=" + db + "&tbl=" + tbl + "\"><img src=\"themes/" + HttpUtility.UrlEncode(HttpContext.Current.Session["theme"].ToString()) + "/img/browse.png\" alt=\"Browse \" /></a></td>" +
								 "<td><a href=\"struct.aspx?a=" + session.SessionID + "&db=" + db + "&tbl=" + tbl + "\"><img src=\"themes/" + HttpUtility.UrlEncode(HttpContext.Current.Session["theme"].ToString()) + "/img/struct.png\" alt=\"Structure\" /></a></td>" +
								 "<td><a href=\"select.aspx?a=" + session.SessionID + "&db=" + db + "&tbl=" + tbl + "\"><img src=\"themes/" + HttpUtility.UrlEncode(HttpContext.Current.Session["theme"].ToString()) + "/img/search.png\" alt=\"Search\" /></td>" +
								 "<td><a href=\"insert.aspx?a=" + session.SessionID + "&db=" + db + "&tbl=" + tbl + "\"><img src=\"themes/" + HttpUtility.UrlEncode(HttpContext.Current.Session["theme"].ToString()) + "/img/insert.png\" /></a></td>" +
									 "<td><a href=\"query.aspx?a=" + session.SessionID + "&db=" + db + "&tbl=" + tbl + "&q=" + HttpUtility.UrlEncode("TRUNCATE TABLE " + tbl) + "\"><img src=\"themes/" + HttpUtility.UrlEncode(HttpContext.Current.Session["theme"].ToString()) + "/img/empty.png\" alt=\"Empty\" /></td>" +
									 "<td><a href=\"query.aspx?a=" + session.SessionID + "&db=" + db + "&q=" + HttpUtility.UrlEncode("DROP TABLE " + tbl) + "\"><img src=\"themes/" + HttpUtility.UrlEncode(HttpContext.Current.Session["theme"].ToString()) + "/img/drop.png\" alt=\"Drop\" /></td>" +
									 "<td class=\"right\">" + size["rows"] + "</td><td class=\"right\">" + size["reserved"] + "</td></tr>";
				}
				body += "<tbody></table>";
			}

			if (procedures.Rows.Count > 0) {
				body += "<br /><a href=\"#top\">Top</a><a name=\"procedures\" /><table><tbody><tr class=\"title\"><td /><td>Name</td><td>Type</td><td>Return Type</td><td colspan=\"2\" /></tr>";
				for (int i = 0, len = procedures.Rows.Count; i < len; i++)
					body += "<tr class=\"" + ((i % 2 == 0) ? "even" : "odd") + "\">" +
										"<td><input type=\"checkbox\" id=\"" + db + "_" + HttpUtility.HtmlEncode(procedures.Rows[i][0].ToString()) + "\"</td>" +
										"<td>" + procedures.Rows[i][0] + "</td>" +
										"<td>" + procedures.Rows[i][1] + "</td>" +
										"<td class=\"right\">" + DisplayLayer.getDataType(procedures.Rows[i]["DATA_TYPE"], procedures.Rows[i]["CHARACTER_MAXIMUM_LENGTH"], procedures.Rows[i]["NUMERIC_PRECISION"],
																							procedures.Rows[i]["NUMERIC_SCALE"], procedures.Rows[i]["DATETIME_PRECISION"]) + "</td>" +
										"<td><img src=\"themes/" + HttpUtility.UrlEncode(session["theme"].ToString()) + "/img/edit.png\" /></td>" +
										"<td><a href=\"query.aspx?a=" + session.SessionID + "&db=" + db + "&q=" + HttpUtility.UrlEncode("DROP PROCEDURE " + procedures.Rows[i][0]) + "\"<img src=\"themes/" + HttpUtility.UrlEncode(HttpContext.Current.Session["theme"].ToString()) + "/img/drop.png\" /></td></tr>";
				body += "</tbody></table>";
			}

			if (views.Rows.Count > 0) {
				body += "<br /><a href=\"#top\">Top</a><a name=\"views\" /><table><tbody><tr class=\"title\"><td /><td>Name</td><td colspan=\"2\" /></tr>";
				for (int i = 0, len = views.Rows.Count; i < len; i++)
					body += "<tr class=\"" + ((i % 2 == 0) ? "even" : "odd") + "\">" +
										"<td><input type=\"checkbox\" id=\"" + db + "_" + HttpUtility.HtmlEncode(views.Rows[i][0].ToString()) + "\"</td>" +
										"<td>" + views.Rows[i][1] + "</td>" +
										"<td><img src=\"themes/" + HttpUtility.UrlEncode(session["theme"].ToString()) + "/img/edit.png\" /></td>" +
										"<td><a href=\"query.aspx?a=" + session.SessionID + "&db=" + db + "&q=" + HttpUtility.UrlEncode("DROP VIEW " + views.Rows[i][0]) + "\"<img src=\"themes/" + HttpUtility.UrlEncode(HttpContext.Current.Session["theme"].ToString()) + "/img/drop.png\" /></td></tr>";
				body += "</tbody></table>";
			}
		}

		private void tbl_struct() {
			DataSet cols = dbl.getColumnInformation(db, tbl);
			if (cols != null) {
				body += "<table><tbody><tr class=\"title\"><td /><td>Name</td><td>Default</td><td>Nullable</td><td>Type</td><td>Identity</td><td>Collation</td><td colspan=\"3\" /></tr>";
				foreach (DataTable t in cols.Tables) for (int i = 0; i < t.Rows.Count; i++) {
					body += "<tr class=\"" + ((i % 2 == 0) ? "even" : "odd")  + "\">" +
								 "<td><input type=\"checkbox\" id=\"" + db + "_" + tbl + "_" + t.Rows[i]["name"] + "\"</td>" +
								 "<td>" +t.Rows[i]["name"] + "</td>" +
								 "<td class=\"right\">" + DisplayLayer.stripDefaults(t.Rows[i]["default"].ToString()) + "</td>" +
								 "<td class=\"right\">" + t.Rows[i]["is_nullable"].ToString().ToLower() + "</td>" +
								 "<td class=\"right\">" + DisplayLayer.getDataType(t.Rows[i]["type"], t.Rows[i]["max_length"], t.Rows[i]["precision"],
																					t.Rows[i]["scale"]) + "</td>" +
								 "<td>" + t.Rows[i]["is_identity"].ToString().ToLower() + "</td>" +
								 "<td>" + t.Rows[i]["collation_name"].ToString().ToLower() + "</td>" +
								 "<td><a href=\"query.aspx?a=" + session.SessionID + "&db=" + db + "&tbl=" + tbl + "&q=" + HttpUtility.UrlEncode("SELECT COUNT(*) AS Cardinality, " + t.Rows[i]["name"] + 
									" FROM " + tbl + " GROUP BY " + t.Rows[i]["name"] + " ORDER BY " + t.Rows[i]["name"]) + "\"><img src=\"themes/" + HttpUtility.UrlEncode(session["theme"].ToString()) + "/img/browse.png\" /></a></td>" +
								 "<td><img src=\"themes/" + HttpUtility.UrlEncode(session["theme"].ToString()) + "/img/edit.png\" /></td>" +
								 "<td><a href=\"query.aspx?a=" + session.SessionID + "&db=" + db + "&tbl=" + tbl + "&q=" + HttpUtility.UrlEncode("ALTER TABLE " + tbl + " DROP COLUMN " + t.Rows[i]["name"]) + "\"><img src=\"themes/" + HttpUtility.UrlEncode(HttpContext.Current.Session["theme"].ToString()) + "/img/drop.png\" /></a></td>" +
								 "</tr>";
				}
				body += "</tbody></table>";
			}
		}

		private void col_struct() {
			int fields = 1;
			if (!String.IsNullOrEmpty(request.QueryString["fields"])) int.TryParse(request.QueryString["fields"], out fields);

			DataRowCollection types = dbl.getTypes();
			DataRowCollection collations = dbl.getCollations().Rows;

			body += "Creating table: " + request.QueryString["tbl"] + "<br /><form method=\"post\">";
			if (fields < 4) {  // vertical arrangement
				string fieldsrow = String.Empty, typesrow = String.Empty, lengthsrow = String.Empty, defaultsrow = String.Empty, collationsrow = String.Empty, nullsrow = String.Empty, identityrow = String.Empty;
				for (int i = 0; i < fields; i++) {
					string n = Convert.ToString(i);
					fieldsrow += "<td><input type=\"text\" name=\"" + n + "_name\" /></td>";
					typesrow += "<td>" + DisplayLayer.getTypeSelect(n + "_type", types) + "</td>";
					lengthsrow += "<td><input type=\"text\" size=\"5\" name=\"" + n + "_length\" /></td>";
					defaultsrow += "<td><select name=\"" + n + "_default\">" +
											"<option value=\"none\">None</option>" +
											"<option value=\"null\">NULL</option>" +
											"<option value=\"getdate\">getdate()</option>" +
											"<option value=\"defined\">As Defined:</option>" +
										"</select><br /><input type=\"text\" size=\"15\" name=\"" + n + "_default_value\" /></td>";
					collationsrow += "<td>" + DisplayLayer.getCollationSelect(n + "_collation", collations) + "</td>";
					nullsrow += "<td><input type=\"checkbox\" name=\"" + n + "_null\" value=\"1\" /></td>";
					identityrow += "<td><input type=\"checkbox\" name=\"" + n + "_identity\" value=\"1\" /><br />" +
										"Seed: <input type=\"text\" size=\"2\" name=\"" + n + "_identity_seed\" /> Inc: <input type=\"text\" size=\"2\" name=\"" + n + "_identity_inc\" /><br />" +
										"Not for replication: <input type=\"checkbox\" name=\"" + n + "_identity_nfr\" value=\"1\" /></td>";
				}
				body += "<table><tbody>" +
									"<tr class=\"even\"><td class=\"title\">Field</td>" + fieldsrow + "</tr>" +
									"<tr class=\"odd\"><td class=\"title\">Type</td>" + typesrow + "</tr>" +
									"<tr class=\"even\"><td class=\"title\">Length(, Precision)</td>" + lengthsrow + "</tr>" +
									"<tr class=\"odd\"><td class=\"title\">Default</td>" + defaultsrow + "</tr>" +
									"<tr class=\"even\"><td class=\"title\">Collation</td>" + collationsrow + "</tr>" +
									"<tr class=\"odd\"><td class=\"title\">Null</td>" + nullsrow + "</tr>" +
									"<tr class=\"even\"><td class=\"title\">Identity</td>" + identityrow + "</tr>" +
								  "</tbody></table>";
			} else { // horizontal arrangement
				body += "<table><tbody><tr class=\"title\"><td>Field</td><td>Type</td><td>Length(, Precision)</td><td>Default</td><td>Collation</td><td>Null</td><td>Identity</td></tr>";
				for (int i = 0; i < fields; i++) {
					string n = Convert.ToString(i);
					body += "<tr class=\"" + ((i % 2 == 0) ? "even" : "odd") + "\"><td><input type=\"text\" name=\"" + n + "_name\" /></td>" +
										"<td>" + DisplayLayer.getTypeSelect(n + "_type", types) + "</td>" +
										"<td><input type=\"text\" size=\"5\" name=\"" + n + "_length\" /></td>" +
										"<td><select name=\"" + n + "_default\">" +
												"<option value=\"none\">None</option>" +
												"<option value=\"null\">NULL</option>" +
												"<option value=\"defined\">As Defined:</option>" +
											"</select><br /><input type=\"text\" size=\"15\" name=\"" + n + "_default_value\" /></td>" +
										"<td>" + DisplayLayer.getCollationSelect(n + "_collation", collations) + "</td>" +
										"<td><input type=\"checkbox\" name=\"" + n + "_null\" value=\"1\" /></td>" +
										"<td class=\"center\"><input type=\"checkbox\" name=\"" + n + "_identity\" value=\"1\"/><br />" +
											"Seed: <input type=\"text\" size=\"2\" name=\"" + n + "_identity_seed\" /> Inc: <input type=\"text\" size=\"2\" name=\"" + n + "_identity_inc\" /><br />" +
											"Not for replication: <input type=\"checkbox\" name=\"" + n + "_identity_nfr\" value=\"1\"/></td>" +
									  "</tr>";
				}
				body += "</tbody></table>";
			}
			body += "<input type=\"submit\" name=\"create_tbl\" value=\"Submit\" /></form>";
		}

		private void tbl_create() {
			int fields = 1;
			if (!String.IsNullOrEmpty(request.QueryString["fields"])) int.TryParse(request.QueryString["fields"], out fields);
			
			ArrayList definitions = new ArrayList();

			for (int i = 0; i < fields; i++) {
				string n = Convert.ToString(i);
				if (String.IsNullOrEmpty(post[n + "_name"]) && !post[n + "_type"].Equals("timestamp")) continue; // timestamp datatype can be entered without a name

				string definition = post[n + "_name"] + " " + post[n + "_type"] + ((!String.IsNullOrEmpty(post[n + "_length"])) ? "(" + post[n + "_length"] + ")" : "");
				if (!String.IsNullOrEmpty(post[n + "_collation"])) definition += " COLLATE " + post[n + "_collation"];
				if (!String.IsNullOrEmpty(post[n + "_default"])) {
					if (post[n + "_default"].Equals("null")) definition += " DEFAULT NULL";
					if (post[n + "_default"].Equals("defined")) definition += " DEFAULT " + post[n + "_default_value"];
				}
				if (!String.IsNullOrEmpty(post[n + "_identity"]) && post[n + "_identity"].Equals("1")) {
					definition += " IDENTITY " + ((!String.IsNullOrEmpty(post[n + "_identity_seed"]) && !String.IsNullOrEmpty(post[n + "_identity_inc"])) ?
													"(" + post[n + "_identity_seed"] + "," + post[n + "_identity_inc"] + ")" : "") +
												 ((!String.IsNullOrEmpty(post[n + "_identity_nfr"]) && post[n + "_identity_nfr"].Equals("1")) ? " NOT FOR REPLICATION" : "");
				}
				if (!String.IsNullOrEmpty(post[n + "_null"]) && post[n + "_null"].Equals("1")) definition += " NULL";
				else definition += " NOT NULL";

				definitions.Add(definition);
			}

			if (dbl.createTable(db, tbl, string.Join(",", definitions.ToArray(typeof(string)) as string[]))) HttpContext.Current.Response.Redirect("struct.aspx?a=" + session.SessionID + "&db=" + db + "&tbl=" + tbl);
			//else Response.Redirect("query.aspx?db=" + db + "&q=" + HttpUtility.UrlEncode(dbl.LastQuery));
		}
	}
}
