using System;
using System.Data;
using System.Text;
using System.Web;

namespace CSMSAdmin {
	public class Restore : CSMSAdmin.Page {

		public override string Render() {
			if (!String.IsNullOrEmpty(post["init"])) getMetadata();
			else if (!String.IsNullOrEmpty(request.QueryString["fn"]) && request.QueryString["fn"].Equals("rh")) restoreHeader();
			else if (!String.IsNullOrEmpty(request.QueryString["fn"]) && request.QueryString["fn"].Equals("rfl")) restoreFileList();
			else if (!String.IsNullOrEmpty(request.QueryString["fn"]) && request.QueryString["fn"].Equals("rdb")) restoreDatabase();
			else displayForm();

			base.Render();
			return body;
		}

		private void displayForm() {
			body += "<div class=\"container\"><span class=\"con_title\">Select a File</span><br />" +
					   "<input type=\"button\" value=\"Browse Server\" onclick=\"openFileBrowser()\" /><br /><br />" +
					   "<div id=\"filewrapper\"><span class=\"bold\">Currently selected file: </span><span id=\"filedisp\"></span><br /><br />" +
					   "<form method=\"post\"><input type=\"hidden\" id=\"file\" name=\"file\" value=\"\" />" +
					   "<input type=\"submit\" name=\"init\" value=\"Continue\" /></form></div>" +
					"</div>";
		}

		private void getMetadata() {
			string file = HttpUtility.UrlDecode(post["file"]);
			DataRow label = dbl.restoreLabelOnly(file).Rows[0];
			body += "<div class=\"container\"><span class=\"con_title\">File Information</span><br />" +
							  "File: " + file + "<br />" +
							  "Media Name: " + ((label["MediaName"] == DBNull.Value) ? "n/a" : label["MediaName"]) + "<br />" +
							  "Created On: " + label["MediaDate"] + "<br />" +
							  "Created With: " + label["SoftwareName"] + "<br />" +
							  "Media Families: " + label["FamilyCount"] + "<br /><br />" +
							  "</div>" +
							  "<div class=\"container\" id=\"restoreHeader\" style=\"display: none\"><span class=\"con_title\">Header</span></div>" +
							  "<div class=\"container\" id=\"restoreFileList\" style=\"display: none\"><span class=\"con_title\">File List</span><table><tbody></tbody></table></div>" +
							  "<div class=\"container\"><span class=\"con_title\">Actions</span><br />" +
							  "<input type=\"hidden\" id=\"file\" value=\"" + file + "\" />" +
							  "<input type=\"button\" value=\"Get Header\" onclick=\"restoreHeaderOnly()\" />&nbsp;&nbsp;" +
							  "<input type=\"button\" value=\"Get File List\" onclick=\"restoreFileListOnly()\" /><br /><br />" +
							  "Database Name: <input type=\"text\" id=\"dbname\" value=\"\" /><br /><br />" +
							  "<input type=\"button\" value=\"Restore Database\" onclick=\"restoreDatabase()\" />" +
							  "</div>";
		}

		private void restoreHeader() {
			isAJAX = true;

			DataRow header = dbl.restoreHeaderOnly(HttpUtility.UrlDecode(request.QueryString["f"]));

			StringBuilder json = new StringBuilder();
			json.Append("{ \"errors\": { \"warnings\": [], \"failures\": [] }, \"payload\": [ ");
			json.Append(" { \"dbn\": \"" + Utilities.jsonEncode(header["DatabaseName"].ToString()) + "\" }");
			json.Append(", { \"dbv\": \"" + Utilities.jsonEncode(header["DatabaseVersion"].ToString()) + "\" }");
			json.Append(", { \"bs\": \"" + Utilities.jsonEncode(header["BackupSize"].ToString()) + "\" }");
			json.Append(", { \"cl\": \"" + Utilities.jsonEncode(header["CompatibilityLevel"].ToString()) + "\" }");
			json.Append(", { \"col\": \"" + Utilities.jsonEncode(header["Collation"].ToString()) + "\" }");
			json.Append(", { \"iro\": \"" + Utilities.jsonEncode(header["IsReadOnly"].ToString()) + "\" }");
			json.Append(", { \"id\": \"" + Utilities.jsonEncode(header["IsDamaged"].ToString()) + "\" }");
			json.Append("] }");
			response.Write(json.ToString());
		}

		private void restoreFileList() {
			isAJAX = true;

			DataTable fl = dbl.restoreFileListOnly(HttpUtility.UrlDecode(request.QueryString["f"]));

			StringBuilder json = new StringBuilder();
			json.Append("{ \"errors\": { \"warnings\": [], \"failures\": [] }, \"payload\": [ ");

			for (int i = 0, l = fl.Rows.Count; i < l; i++) json.Append(((i > 0) ? ", " : "") + "{ " +
																		"\"fid\": \"" + fl.Rows[i]["FileId"].ToString() + "\", " +
																		"\"ln\": \"" + Utilities.jsonEncode(fl.Rows[i]["LogicalName"].ToString()) + "\", " +
																		"\"pn\": \"" + Utilities.jsonEncode(fl.Rows[i]["PhysicalName"].ToString()) + "\", " +
																		"\"t\": \"" + LookupTables.backupFileType(fl.Rows[i]["Type"].ToString()) + "\"" +
																		" }");

			json.Append("] }");
			response.Write(json.ToString());
		}

		private void restoreDatabase() {
			isAJAX = true;

			string wth = String.Empty;
			string fname = HttpUtility.UrlDecode(request.QueryString["f"]);
			string[] chckd = HttpUtility.UrlDecode(request.QueryString["ids"]).Split(new char[] {','});
			bool resume = !String.IsNullOrEmpty(request.QueryString["r"]) && request.QueryString["r"].Equals("1");

			if (chckd.Length > 0) {
				DataTable fl = dbl.restoreFileListOnly(fname);
				for (int i = 0, l = fl.Rows.Count; i < l; i++) {
					DataRow r = fl.Rows[i];
					if (String.IsNullOrEmpty(HttpUtility.UrlDecode(request.QueryString["pn_" + r["FileId"]]))) continue;
						
					if (wth.Length > 0) wth += ", ";
					wth += "MOVE " + r["LogicalName"] + " TO " + HttpUtility.UrlDecode(request.QueryString["pn_" + r["FileId"]]);
				}
			}

			response.Write("RESTORE DATABASE " + HttpUtility.UrlDecode(request.QueryString["dbname"]) + " FROM DISK = '" + fname + "' " + ((wth.Length > 0) ? "WITH " + wth : ""));
		}
	}
}
