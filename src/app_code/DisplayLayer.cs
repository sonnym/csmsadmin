using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Web;
using System.Web.SessionState;

public class DisplayLayer {
	private static StringBuilder sb;

	public DisplayLayer() { }

	  /////////////////////
	 // layout sections //
	/////////////////////

	public static string getLocation(string auth, string srv, string db, string tbl) {
		sb = new StringBuilder("<div class=\"loc\">Server: <a href=\"default.aspx?a=" + auth + "\" target=\"_top\">" + srv + "</a>");
		if (db != null && db.Length > 0) sb.Append(" &gt; Database: <a href=\"default.aspx?a=" + auth + "&db=" + db + "\" target=\"_top\">" + db + "</a>");
		if (tbl != null && tbl.Length > 0) sb.Append(" &gt; Table: <a href=\"default.aspx?a=" + auth + "&db=" + db + "&tbl=" + tbl + "\" target=\"_top\">" + tbl + "</a>");
		sb.Append("</div>");
		return sb.ToString();
	}
	
	public static string getTopTabs(string auth, string selected, string db, string tbl) {
		bool nodb = String.IsNullOrEmpty(db), notbl = String.IsNullOrEmpty(tbl);
		sb = new StringBuilder();
		sb.Append("<div class=\"toptabs\"><ul>");
		sb.Append("<li class=\"" + ((String.Compare("Structure", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"struct.aspx?a=" + auth + "&db=" + db + "&tbl=" + tbl + "\">Structure</a></li>");
		if (!notbl) sb.Append("<li class=\"" + ((String.Compare("Browse", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"browse.aspx?a=" + auth + "&db=" + db + "&tbl=" + tbl + "\">Browse</a></li>");
		sb.Append("<li class=\"" + ((String.Compare("SQL", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"query.aspx?a=" + auth + "&db=" + db + "&tbl=" + tbl + "\">SQL</a></li>");
		if (!nodb) sb.Append("<li class=\"" + ((String.Compare("Search", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"select.aspx?a=" + auth + "&db=" + db + "&tbl=" + tbl + "\">Search</a></li>");
		if (!notbl) sb.Append("<li class=\"" + ((String.Compare("Insert", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"insert.aspx?a=" + auth + "&db=" + db + "&tbl=" + tbl + "\">Insert</a></li>");
		if (nodb) {
			sb.Append("<li class=\"" + ((String.Compare("Configuration", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"configuration.aspx?a=" + auth + "\">Configuration</a></li>");
			sb.Append("<li class=\"" + ((String.Compare("Status", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"status.aspx?a=" + auth + "\">Status</a></li>");
			sb.Append("<li class=\"" + ((String.Compare("Providers", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"providers.aspx?a=" + auth + "\">Providers</a></li>");
			sb.Append("<li class=\"" + ((String.Compare("Charsets", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"charsets.aspx?a=" + auth + "\">Charsets</a></li>");
			sb.Append("<li class=\"" + ((String.Compare("Processes", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"processes.aspx?a=" + auth + "\">Processes</a></li>");
		}
		if (notbl) {
			sb.Append("<li class=\"" + ((String.Compare("Permissions", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"permissions.aspx?a=" + auth + "&db=" + db + "&tbl=" + tbl + "\">Permissions</a></li>");
			sb.Append("<li class=\"" + ((String.Compare("Operations", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"operations.aspx?a=" + auth + "&db=" + db + "&tbl=" + tbl + "\">Operations</a></li>");
			sb.Append("<li class=\"" + ((String.Compare("Backup", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"backup.aspx?a=" + auth + "&db=" + db + "&tbl=" + tbl + "\">Backup</a></li>");
			sb.Append("<li class=\"" + ((String.Compare("Restore", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"restore.aspx?a=" + auth + "&db=" + db + "&tbl=" + tbl + "\">Restore</a></li>");
		}
		if (!notbl) sb.Append("<li class=\"caution_tab\"><a href=\"query.aspx?a=" + auth + "&db=" + db + "&tbl=" + tbl + "&q=" + HttpUtility.UrlEncode("TRUNCATE TABLE " + tbl) + "\">Empty</a></li>");
		if (!String.IsNullOrEmpty(db)) sb.Append("<li class=\"caution_tab\"><a href=\"query.aspx?a=" + auth + "&db=&tbl=&q=" + HttpUtility.UrlEncode("DROP " + ((notbl) ? "DATABASE " + db : "TABLE " + tbl)) + "\">Drop</a></li></ul>");
		sb.Append("<div class=\"clearfloat\"></div></div>");
		return sb.ToString();
	}

	public static string getQueryBox(string auth, string db, string tbl, string q) {
		return "<div class=\"query_box\">" + q.Replace("FROM", "<br/>FROM").Replace("WHERE", "<br />WHERE").Replace("AND", "<br />AND").Replace("OR", "<br />OR").Replace(";", ";<br />") + "<br />" +
				"<div class=\"right_container\">[ <a href=\"query.aspx?a=" + auth + "&sp=1&db=" + db + "&tbl=" + tbl + "&q=" + HttpContext.Current.Server.UrlEncode(q) + "\">Show Plan</a> ]" +
				" [ <a href=\"query.aspx?a=" + auth + "&e=1&db=" + db + "&tbl=" + tbl + "&q=" + HttpContext.Current.Server.UrlEncode(q) + "\">Edit</a> ]</div></div>";
	}

	public static string getQueryInput(string db, string tbl, string q) {
		return "<form method=\"get\"><input type=\"hidden\" name=\"db\" value=\"" + db + "\" /><input type=\"hidden\" name=\"tbl\" value=\"" + tbl + "\" /><textarea name=\"q\" id=\"query_input\">" + q + "</textarea><br /><input type=\"submit\" id=\"query_execute\" value=\"Execute\" /></form>";
	}

	public static string getBrowseTableNavigation(string auth, string db, string tbl, int rows, int page, int count) {
		sb = new StringBuilder();
		double last = Math.Ceiling((double)(rows / count));
		if (page > 0) sb.Append("<a href=\"browse.aspx?a=" + auth + "&db=" + db + "&tbl=" + tbl + "&c=" + Convert.ToString(count) + "\">&laquo;</a> <a href=\"browse.aspx?a=" + auth + "&db=" + db + "&tbl=" + tbl + "&p=" + Convert.ToString(page - 1) + "&c=" + Convert.ToString(count) + "\">&lsaquo;</a>");
		if (rows > count * (page + 1)) sb.Append("<a href=\"browse.aspx?a=" + auth + "&db=" + db + "&tbl=" + tbl + "&p=" + Convert.ToString(page + 1) + "&c=" + Convert.ToString(count) + "\">&rsaquo;</a> <a href=\"browse.aspx?a=" + auth + "&db=" + db + "&tbl=" + tbl + "&p=" + Convert.ToString(last) + "&c=" + Convert.ToString(count) + "\">&raquo;</a>");
		return sb.ToString();
	}

	public static string getCreateNewDatabase() {
		return "<form method=\"post\">Name: <input type=\"text\" name=\"name\" />&nbsp;<input type=\"submit\" name=\"create_db\" value=\"Create\" /></form>";
	}

	public static string getCreateNewTable(string db) {
		return "<form method=\"post\"><input type=\"hidden\" name=\"db\" value=\"" + db + "\" />Name: <input type=\"text\" name=\"name\" />&nbsp;# of Fields: <input type=\"text\" name=\"fields\" size=\"2\" />&nbsp;<input type=\"submit\" name=\"create_tbl\" value=\"Create\" /></form>";
	}

	  //////////////////
	 // form selects //
	//////////////////

	public static string getServerSelect(string n) {
		StringBuilder sb = new StringBuilder();
		DBLayer dbl = new DBLayer();
		DataRowCollection servers = dbl.getServers();

		sb.Append("<select name=\"" + n + "\">");
		for (int i = 0, l = servers.Count; i < l; i++)
			sb.Append("<option value=\"" + servers[i][0] + "\" >" + servers[i][1] + "</option>");
		sb.Append("</select>");
		return sb.ToString();
	}

	public static string getTypeSelect(string n, DataRowCollection names) {
		sb = new StringBuilder();
		sb.Append("<select name=\"" + n + "\">");
		for (int i = 0, l = names.Count; i < l; i++)
			sb.Append("<option value=\"" + names[i][0] + "\" >" + names[i][0] + "</option>");
		sb.Append("</select>");
		return sb.ToString();
	}

	public static string getCollationSelect(string n, DataRowCollection collations) {
		sb = new StringBuilder();
		sb.Append("<select name=\"" + n + "\"><option />");
		for (int i = 0, l = collations.Count; i < l; i++) sb.Append("<option value=\"" + collations[i][0] + "\">" + collations[i][0] + "</option>");
		sb.Append("</select>");
		return sb.ToString();
	}

	public static string getNavigationSelect(string n, ArrayList nvs, string sel) {
		if (nvs == null) return "";

		sb = new StringBuilder();
		sb.Append("<select name=\"" + n + "\" onchange=\"checkNav(this)\" onkeyup=\"checkNav(this)\">");
		if (String.IsNullOrEmpty(sel)) sb.Append("<option />");
		for (int i = 0, l = nvs.Count; i < l; i++)
			sb.Append("<option name=\"" + nvs[i] + "\"" + ((nvs[i].Equals(sel)) ? " selected=\"selected\"" : "") + ">" + nvs[i] + "</option>");
		sb.Append("</select>");
		return sb.ToString();
	}

	public static string getThemeSelect() {
		string[] themes = FileSystemLayer.getThemes();
		sb = new StringBuilder();
		sb.Append("<form name=\"theme\" method=\"post\"><select name=\"theme\" onchange=\"switchTheme()\" onkeyup=\"switchTheme()\">");
		for (int i = 0, l = themes.Length; i < l; i++)
			sb.Append("<option value=\"" + themes[i] + "\"" + ((themes[i].Equals(HttpContext.Current.Session["theme"].ToString())) ? " selected=\"selected\"" : "") + ">" + themes[i] + "</option>");
		sb.Append("</select></form>");
		return sb.ToString();
	}

	  /////////
	 // etc //
	/////////
	public static string getDataType(object name, object charmaxlen, object numprecision, object numscale, object datetimeprecision) {
			if (String.IsNullOrEmpty(name.ToString())) return "NULL";
			  
			return name.ToString() +
					((charmaxlen == DBNull.Value) ? "" : // strings and binary
						"(" + ((int.Parse(charmaxlen.ToString()) == -1) ? "MAX" : charmaxlen.ToString()) + ")") +
					((numprecision == DBNull.Value) ? "" : // numbers
						"(" + numprecision.ToString() + ((numscale == DBNull.Value || int.Parse(numscale.ToString()) == 0) ? "" : ", " + numscale.ToString()) + ")") +
					((datetimeprecision == DBNull.Value) ? "" : // datetimes
						"(" + datetimeprecision + ")");
	}
	public static string getDataType(object name, object maxlen, object precision, object scale) {
		if (String.IsNullOrEmpty(name.ToString())) return "NULL";

		string t = name.ToString();
		if (int.Parse(precision.ToString()) > 0) { // numbers and datetime
			t += "(" + precision.ToString() + ((int.Parse(scale.ToString()) == 0 || int.Parse(scale.ToString()) == 0) ? "" : ", " + scale.ToString()) + ")";
		} else if (int.Parse(maxlen.ToString()) > 0) { // strings and binary (maxlen also exists for integers, hence reverse order from above)
			t += "(" + ((int.Parse(maxlen.ToString()) == -1) ? "MAX" : maxlen.ToString()) + ")";
		}

		return t;
	}

	public static string stripDefaults(string d) {
		if (String.IsNullOrEmpty(d)) return d;
		while (d[0].Equals('(') && d[d.Length - 1].Equals(')')) d = d.Substring(1, d.Length -2);
		return d;
	}
}
