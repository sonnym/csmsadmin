using System;
using System.Data;
using System.Web;

namespace CSMSAdmin {
	delegate string LookupTable(string s);

	public class Permissions : CSMSAdmin.Page {

		private string submitValue = "Submit";

		public override string Render() {
			if (!String.IsNullOrEmpty(qs["pid"]) && String.IsNullOrEmpty(post["submit"])) principal_edit(int.Parse(qs["pid"]));
			else if (!String.IsNullOrEmpty(db) && String.IsNullOrEmpty(post["submit"])) {
				if (String.IsNullOrEmpty(qs["t"]) && String.IsNullOrEmpty(qs["l"])) db_principals_summary();
				else db_principals_drilldown();
			}
			else if (!String.IsNullOrEmpty(qs["t"]) || !String.IsNullOrEmpty(qs["l"])) srv_principals_drilldown();
			else if (!String.IsNullOrEmpty(post["submit"])) updatePermissions(int.Parse(qs["pid"]));
			else srv_principals_summary();

			base.Render();
			return body;
		}

		// server level
		private void srv_principals_summary() {
			DataTable types = dbl.getServerPrincipalTypes();
			DataTable principals = dbl.getServerPrincipals();

			int k = 0; 

			for (int i = 0, len = types.Rows.Count; i < len; i++) {
				body += "<b>" + LookupTables.principalType(types.Rows[i]["type"].ToString()) + "s:</b> (<a href=\"permissions.aspx?a=" + session.SessionID + "&t=" + types.Rows[i]["type"].ToString() + "&l=\">all</a>) <br />";
				for (int j = 97; j <= 122; j++) {
					if (k < principals.Rows.Count && principals.Rows[k]["type"].Equals(types.Rows[i]["type"]) && principals.Rows[k]["first"].ToString().ToLower().ToCharArray()[0] == (char)j) {
						body += "<a href=\"permissions.aspx?a=" + session.SessionID + "&t=" + types.Rows[i]["type"].ToString() + "&l=" + (char)j + "\">" + (char)j + "</a>&nbsp;&nbsp;";
						k++;
					} else body += ((char)j).ToString() + "&nbsp;&nbsp;";
				}
				body += "<br /><br />";
			}
		}

		private void srv_principals_drilldown() {
			DataTable principals = dbl.getServerPrincipals(qs["t"], qs["l"]);

			if (principals.Rows.Count > 0) {
				body += "<table><tbody><tr class=\"title\">";
				foreach (DataColumn c in principals.Columns) body += "<td>" + c.ColumnName + "</td>";
				body += "<td />";
				body += "</tr>";

				foreach (DataRow r in principals.Rows) {
					body += "<tr>";
					foreach (DataColumn c in principals.Columns) body += "<td>" + r[c.ColumnName] + "</td>";
					body += "<td><a href=\"permissions.aspx?a=" + session.SessionID + "&pid=" + r["principal_id"].ToString() + "\"><img src=\"themes/" + HttpUtility.UrlEncode(HttpContext.Current.Session["theme"].ToString()) + "/img/edit.png\" /></a></td>";
					body += "</tr>";
				}
				body += "</tbody></table>";
			} else body += "<span>No principals found for this query.</span>";
		}

		// database level
		private void db_principals_summary() {
			string db = qs["db"];
			DataTable types = dbl.getDatabasePrincipalTypes(db);
			DataTable principals = dbl.getDatabasePrincipals(db);

			int k = 0; 

			for (int i = 0, len = types.Rows.Count; i < len; i++) {
				body += "<b>" + LookupTables.principalType(types.Rows[i]["type"].ToString()) + "s:</b> (<a href=\"permissions.aspx?a=" + session.SessionID + "&db=" + db + "&t=" + types.Rows[i]["type"].ToString() + "&l=\">all</a>) <br />";
				for (int j = 97; j <= 122; j++) {
					if (k < principals.Rows.Count && principals.Rows[k]["type"].Equals(types.Rows[i]["type"]) && principals.Rows[k]["first"].ToString().ToLower().ToCharArray()[0] == (char)j) {
						body += "<a href=\"permissions.aspx?a=" + session.SessionID + "&db=" + db + "&t=" + types.Rows[i]["type"].ToString() + "&l=" + (char)j + "\">" + (char)j + "</a>&nbsp;&nbsp;";
						k++;
					} else body += ((char)j).ToString() + "&nbsp;&nbsp;";
				}
				body += "<br /><br />";
			}
		}

		private void db_principals_drilldown() {
			DataTable principals = dbl.getDatabasePrincipals(qs["db"], qs["t"], qs["l"]);

			if (principals.Rows.Count > 0) {
				body += "<table><tbody><tr class=\"title\">";
				foreach (DataColumn c in principals.Columns) body += "<td>" + c.ColumnName + "</td>";
				body += "<td /></tr>";

				foreach (DataRow r in principals.Rows) {
					body += "<tr>";
					foreach (DataColumn c in principals.Columns) body += "<td>" + r[c.ColumnName] + "</td>";
					body += "<td><a href=\"permissions.aspx?a=" + session.SessionID + "&db=" + HttpUtility.UrlEncode(qs["db"]) + "&pid=" + r["principal_id"].ToString() + "\"><img src=\"themes/" + HttpUtility.UrlEncode(HttpContext.Current.Session["theme"].ToString()) + "/img/edit.png\" /></a></td>";
					body += "</tr>";
				}
				body += "</tbody></table>";
			} else body += "<span>No principals found for this query.</span>";
		}

		// principals
		private void principal_edit(int pid) {
			string principal = String.Empty;
			string[] permissionTypesAbbr, permissionTypesFull;
			DataTable permissions;

			if (String.IsNullOrEmpty(db)) {
				principal = dbl.getPrincipalName(pid);
				permissions = dbl.getServerPermissions(pid);
				permissionTypesAbbr = Constants.serverPermissionTypeAbbr;
				permissionTypesFull = Constants.serverPermissionType;
			} else {
				principal = dbl.getPrincipalName(db, pid);
				permissions = dbl.getDatabasePermissions(db, pid);
				permissionTypesAbbr = Constants.databasePermissionTypeAbbr;
				permissionTypesFull = Constants.databasePermissionType;
			}

			string[] states = Constants.permissionStates;
			string[] stateAbbrs = Constants.permissionStateAbbrs;
			int stateCount = states.Length;

			body += "<span class=\"bold\">Editing " + principal + "</span><form method=\"post\"><table><tbody><tr class=\"title\">";
			for (int i = 0, l = stateCount; i < l; i++) body += "<td>" + states[i] + "</td>";
			body += "<td>Type</td></tr>";

			for (int i = 0, l = permissionTypesAbbr.Length; i < l; i++) {
				string state = String.Empty;
				DataRow[] permission = permissions.Select("type = '" + permissionTypesAbbr[i] + "'");

				for (int j = 0, s = stateCount; j < s; j++) {
					state += "<td><input type=\"radio\" name=\"" + permissionTypesAbbr[i] + "\" value=\"" + stateAbbrs[j] + "\" " +
								((permission.Length > 0 && String.Compare(permission[0]["state"].ToString().Trim(), stateAbbrs[j]) == 0) ? "checked=\"checked\"" : "") +
								"/></td>";
				}

				body += "<tr>" +
							state +
							"<td>" + permissionTypesFull[i] + "</td>" +
						 "</tr>";
			}
			body += "<tr><td colspan=\"5\" class=\"right\"><input type=\"submit\" name=\"submit\" value=\"" + submitValue + "\" /></td></tr></tbody></form></form>";
		}

		// updating
		private void updatePermissions(int pid) {
			string principal = String.Empty, grant = String.Empty, deny = String.Empty, revoke = String.Empty, grantwgrant = String.Empty;
			LookupTable permissionLT;

			if (String.IsNullOrEmpty(db)) {
				principal = dbl.getPrincipalName(pid);
				permissionLT = new LookupTable(LookupTables.serverPermissionType);
			} else {
				principal = dbl.getPrincipalName(db, pid);
				permissionLT = new LookupTable(LookupTables.databasePermissionType);
			}

			for (int i = 0; i < post.Count; i++) {
				if (String.Compare(post[i], submitValue) == 0) continue;

				switch(post[i]) {
					case "G":
						grant += ((grant.Length > 0) ? ", " : "") + permissionLT(post.GetKey(i));
						break;
					case "D":
						deny += ((deny.Length > 0) ? ", " : "") + permissionLT(post.GetKey(i));
						break;
					case "R":
						revoke += ((revoke.Length > 0) ? ", " : "") + permissionLT(post.GetKey(i));
						break;
					case "W":
						grantwgrant += ((grantwgrant.Length > 0) ? ", " : "") + permissionLT(post.GetKey(i));
						break;
				}
			}

			if (grant.Length > 0) dbl.executeQuery(db, "GRANT " + grant + " TO " + principal);
			if (deny.Length > 0) dbl.executeQuery(db, "DENY " + deny + " TO " + principal); // there is no way this will work
			if (revoke.Length > 0) dbl.executeQuery(db, "REVOKE " + revoke + " FROM " + principal);
			if (grantwgrant.Length > 0) dbl.executeQuery(db, "GRANT " + grantwgrant + " TO " + principal + " WITH GRANT OPTION");

			principal_edit(pid);
		}
	}
}
