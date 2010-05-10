using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Web;

namespace CSMSAdmin {
	public class ServerBrowser : CSMSAdmin.Page {
	
		public override string Render() {
			if (!String.IsNullOrEmpty(request.QueryString["d"])) {
				processDirectory();
				return body;
			}

			string request_dir;

			if (String.IsNullOrEmpty(Settings.DefaultServerBrowserPath)) {
				string request_path = request.ServerVariables["PATH_TRANSLATED"];

				int idx = request_path.LastIndexOf('\\');
				if (idx < 0) {
					request_path = ".";
					idx = 0;
				}
				request_dir = @request_path.Substring(0, idx + 1);
			} else request_dir = Settings.DefaultServerBrowserPath;

			body += "<b>Server Browser</b><br /><br />" +
						"<input type=\"hidden\" id=\"h_theme\" value=\"" + HttpUtility.UrlEncode(session["theme"].ToString()) + "\" />";

			ArrayList contents = FileSystemLayer.getFolderContents(request_dir);
			if (contents.Count == 0) {
				body += "Please contact a system administrator";
				return body;
			}

			body += "<table id=\"files\"><tbody>";
			for (int i = 0, l = contents.Count; i < l; i++) {
				string[] item = (string[])contents[i];
				bool folder = item[1].Equals("0");
				body += "<tr class=\"" + ((i % 2 == 0) ? "even" : "odd") + " pointer\" onclick=\"" + ((folder) ? "update" : "set") + "RestorePath('" + item[2].Replace(@"\", @"\\") + "')\">" +
									"<td><img src=\"themes/" + HttpUtility.UrlEncode(session["theme"].ToString()) + "/img/" + ((folder) ? "folder" : "file") + ".png\" />" +
									((string[])contents[i])[0] + "</td></tr>";
				}
			body += "</tbody></table>";

			return body;
		}

		private void processDirectory() {
			page.Visible = false;
			response.Expires = -1;

			ArrayList contents = FileSystemLayer.getFolderContents(HttpUtility.UrlDecode(request.QueryString["d"]));

			StringBuilder json = new StringBuilder();
			json.Append("{ \"errors\": { \"warnings\": [], \"failures\": [] }, \"payload\": [ ");
			for (int i = 0, l = contents.Count; i < l; i++) {
				string[] item = (string[])contents[i];
				json.Append(((i > 0) ? "," : "") + "{ \"dn\": \"" + Utilities.jsonEncode(item[0]) + "\", \"t\": " + item[1] + ", \"fn\": \"" + Utilities.jsonEncode(item[2].Replace(@"\", @"\\")) + "\" }");
			}
			json.Append("] }");
			response.Write(json.ToString());
		}
	}
}
