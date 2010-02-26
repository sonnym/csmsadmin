using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Web;

public class DisplayLayer {
	private static StringBuilder sb;

	public DisplayLayer() { }

	// Generates the bread crumb navigation for the top of the page
	public static string GetLocation(string srv, string db, string tbl) {
		sb = new StringBuilder("<div class=\"loc\">Server: <a href=\"default.aspx\" target=\"_top\">" + srv + "</a>");
		if (db != null && db.Length > 0) sb.Append(" &gt; Database: <a href=\"default.aspx?db=" + db + "\" target=\"_top\">" + db + "</a>");
		if (tbl != null && tbl.Length > 0) sb.Append(" &gt; Table: <a href=\"default.aspx?db=" + db + "&tbl=" + tbl + "\" target=\"_top\">" + tbl + "</a>");
		sb.Append("</div>");
		return sb.ToString();
	}
	
	// Generates the tab navigation for the top of the page - currently only functional for tables view
	public static string GetTopTabs(string selected, string db, string tbl) {
		bool nodb = String.IsNullOrEmpty(db), notbl = String.IsNullOrEmpty(tbl);
		sb = new StringBuilder();
		sb.Append("<div class=\"toptabs\"><ul>");
		sb.Append("<li class=\"" + ((String.Compare("Structure", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"struct.aspx?db=" + db + "&tbl=" + tbl + "\">Structure</a></li>");
		if (!notbl) sb.Append("<li class=\"" + ((String.Compare("Browse", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"browse.aspx?db=" + db + "&tbl=" + tbl + "\">Browse</a></li>");
		sb.Append("<li class=\"" + ((String.Compare("SQL", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"query.aspx?db=" + db + "&tbl=" + tbl + "\">SQL</a></li>");
		if (!nodb) sb.Append("<li class=\"" + ((String.Compare("Search", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"select.aspx?db=" + db + "&tbl=" + tbl + "\">Search</a></li>");
		if (!notbl) sb.Append("<li class=\"" + ((String.Compare("Insert", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"insert.aspx?db=" + db + "&tbl=" + tbl + "\">Insert</a></li>");
		if (nodb) {
			sb.Append("<li class=\"" + ((String.Compare("Configuration", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"configuration.aspx\">Configuration</a></li>");
			sb.Append("<li class=\"" + ((String.Compare("Status", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"status.aspx\">Status</a></li>");
			sb.Append("<li class=\"" + ((String.Compare("Providers", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"providers.aspx\">Providers</a></li>");
			sb.Append("<li class=\"" + ((String.Compare("Charsets", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"charsets.aspx\">Charsets</a></li>");
			sb.Append("<li class=\"" + ((String.Compare("Processes", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"processes.aspx\">Processes</a></li>");
		}
		if (notbl) {
			sb.Append("<li class=\"" + ((String.Compare("Permissions", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"permissions.aspx?db=" + db + "&tbl=" + tbl + "\">Permissions</a></li>");
			sb.Append("<li class=\"" + ((String.Compare("Operations", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"operations.aspx?db=" + db + "&tbl=" + tbl + "\">Operations</a></li>");
			if (!nodb) {
				sb.Append("<li class=\"" + ((String.Compare("Backup", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"backup.aspx?db=" + db + "&tbl=" + tbl + "\">Backup</a></li>");
				sb.Append("<li class=\"" + ((String.Compare("Restore", selected) == 0) ? "active_tab" : "inactive_tab") + "\"><a href=\"restore.aspx?db=" + db + "&tbl=" + tbl + "\">Restore</a></li>");
			}
		}
		if (!notbl) sb.Append("<li class=\"caution_tab\"><a href=\"query.aspx?db=" + db + "&tbl=" + tbl + "&q=" + HttpUtility.UrlEncode("TRUNCATE TABLE " + tbl) + "\">Empty</a></li>");
		if (!String.IsNullOrEmpty(db)) sb.Append("<li class=\"caution_tab\"><a href=\"query.aspx?db=&tbl=&q=" + HttpUtility.UrlEncode("DROP " + ((notbl) ? "DATABASE " + db : "TABLE " + tbl)) + "\">Drop</a></li></ul>");
		sb.Append("<div class=\"clearfloat\"></div></div>");
		return sb.ToString();
	}

	// Generates the box which contains a query string after execution
	public static string GetQueryBox(string db, string tbl, string q) {
		return "<div class=\"query_box\">" + q.Replace("FROM", "<br/>FROM").Replace("WHERE", "<br />WHERE").Replace("AND", "<br />AND").Replace("OR", "<br />OR").Replace(";", ";<br />") + "<br />" +
				"<div class=\"right_container\">[ <a href=\"query.aspx?sp=1&db=" + db + "&tbl=" + tbl + "&q=" + HttpContext.Current.Server.UrlEncode(q) + "\">Show Plan</a> ]" +
				" [ <a href=\"query.aspx?e=1&db=" + db + "&tbl=" + tbl + "&q=" + HttpContext.Current.Server.UrlEncode(q) + "\">Edit</a> ]</div></div>";
	}

	// Generates the controls for browsing through a table - currently only pagination is supported
	public static string GetBrowseTableNavigation(string db, string tbl, int rows, int page, int count) {
		sb = new StringBuilder();
		double last = Math.Ceiling((double)(rows / count));
		if (page > 0) sb.Append("<a href=\"browse.aspx?db=" + db + "&tbl=" + tbl + "&c=" + Convert.ToString(count) + "\">&laquo;</a> <a href=\"browse.aspx?db=" + db + "&tbl=" + tbl + "&p=" + Convert.ToString(page - 1) + "&c=" + Convert.ToString(count) + "\">&lsaquo;</a>");
		if (rows > count * (page + 1)) sb.Append("<a href=\"browse.aspx?db=" + db + "&tbl=" + tbl + "&p=" + Convert.ToString(page + 1) + "&c=" + Convert.ToString(count) + "\">&rsaquo;</a> <a href=\"browse.aspx?db=" + db + "&tbl=" + tbl + "&p=" + Convert.ToString(last) + "&c=" + Convert.ToString(count) + "\">&raquo;</a>");
		return sb.ToString();
	}

	// Generates the form for creating an arbitrary query
	public static string GetQueryInput(string db, string tbl, string q) {
		return "<form method=\"get\" action=\"query.aspx\"><input type=\"hidden\" name=\"db\" value=\"" + db + "\" /><input type=\"hidden\" name=\"tbl\" value=\"" + tbl + "\" /><textarea name=\"q\" id=\"query_input\">" + q + "</textarea><br /><input type=\"submit\" id=\"query_execute\" value=\"Execute\" /></form>";
	}

	// Generates the entire form for the file selection and restore process
	public static string GetRestoreArea(string db) {
		return "<input type=\"button\" value=\"Select a File\" onclick=\"window.open('server_browser.aspx', '_blank', 'location=0,scrollbars=0,status=0,toolbar=0,left=0,top=0,width=600,height=400')\" /><br />" +
			   "<span class=\"bold\">Currently selected file: </span><span id=\"cur_disp\">C:\\Users\\Administrator\\share\\vn\\db\\production.bak</span><br /><br />" +
			   "<form method=\"post\" action=\"restore.aspx\"><input type=\"hidden\" id=\"r_file\" name=\"r_file\" value=\"C:\\Users\\Administrator\\share\\vn\\db\\production.bak\" /><input type=\"hidden\" name=\"db\" value=\"" + db + "\" />" +
			   "<input type=\"submit\" name=\"r_execute\" value=\"Restore Database\" /></form>";
	}

	public static string GetCreateNewDatabase() {
		return "<form method=\"post\">Name: <input type=\"text\" name=\"name\" />&nbsp;<input type=\"submit\" name=\"create_db\" value=\"Create\" /></form>";
	}

	public static string GetCreateNewTable(string db) {
		return "<form method=\"post\"><input type=\"hidden\" name=\"db\" value=\"" + db + "\" />Name: <input type=\"text\" name=\"name\" />&nbsp;# of Fields: <input type=\"text\" name=\"fields\" size=\"2\" />&nbsp;<input type=\"submit\" name=\"create_tbl\" value=\"Create\" /></form>";
	}

	public static string GetTypeSelect(string n, DataRowCollection names) {
		sb = new StringBuilder();
		sb.Append("<select name=\"" + n + "\">");
		for (int i = 0, l = names.Count; i < l; i++)
			sb.Append("<option value=\"" + names[i][0] + "\" >" + names[i][0] + "</option>");
		sb.Append("</select>");
		return sb.ToString();
	}

	public static string GetCollationSelect(string n, DataRowCollection collations) {
		sb = new StringBuilder();
		sb.Append("<select name=\"" + n + "\"><option />");
		for (int i = 0, l = collations.Count; i < l; i++) sb.Append("<option value=\"" + collations[i][0] + "\">" + collations[i][0] + "</option>");
		sb.Append("</select>");
		return sb.ToString();
	}

	public static string GetNavigationSelect(string n, ArrayList nvs, string sel) {
		sb = new StringBuilder();
		sb.Append("<select name=\"" + n + "\" onchange=\"checkNav(this)\" onkeyup=\"checkNav(this)\">");
		if (String.IsNullOrEmpty(sel)) sb.Append("<option />");
		for (int i = 0, l = nvs.Count; i < l; i++)
			sb.Append("<option name=\"" + nvs[i] + "\"" + ((nvs[i].Equals(sel)) ? " selected=\"selected\"" : "") + ">" + nvs[i] + "</option>");
		sb.Append("</select>");
		return sb.ToString();
	}

	public static string GetThemeSelect() {
		string[] themes = FileSystemLayer.GetThemes();
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
	public static string GetDataType(object name, object charmaxlen, object numprecision, object numscale, object datetimeprecision) {
		if (String.IsNullOrEmpty(name.ToString())) return "NULL";

		return name.ToString() + ((charmaxlen == DBNull.Value) ? "" : // strings and binary
									"(" + ((int.Parse(charmaxlen.ToString()) == -1) ? "MAX" : charmaxlen.ToString()) + ")") +
								  ((numprecision == DBNull.Value) ? "" : // numbers
									"(" + numprecision.ToString() + ((numscale == DBNull.Value || int.Parse(numscale.ToString()) == 0) ? "" : ", " + numscale.ToString()) + ")") +
								  ((datetimeprecision == DBNull.Value) ? "" : // datetimes
									"(" + datetimeprecision + ")");
	}
}
