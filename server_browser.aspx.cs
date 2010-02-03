using System;
using System.Web;

partial class CSMSAdminServerBrowser : System.Web.UI.Page {
	private void Page_Load(Object sender, EventArgs e) {
		string request_dir;

		if (Settings.DefaultServerBrowserPath == null) {
			string request_path = Request.ServerVariables["PATH_TRANSLATED"];

			int idx = request_path.LastIndexOf('\\');
			if (idx < 0) idx = request_path.LastIndexOf('/'); // including compatability for mono in apache
			if (idx < 0) {
				request_path = ".";
				idx = 0;
			}
			request_dir = @request_path.Substring(0, idx + 1);
		} else request_dir = Settings.DefaultServerBrowserPath;

		body.Text = FileSystemLayer. GetFolderContents(request_dir);
	}
}
