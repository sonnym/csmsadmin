using System;
using System.Collections;
using System.IO;
using System.Web;

namespace CSMSAdmin {
	public class FileSystemLayer {
	public FileSystemLayer() { }

	public static string[] getThemes() {
		DirectoryInfo[] dirs = new DirectoryInfo(Path.Combine(HttpContext.Current.Server.MapPath("~"), "themes")).GetDirectories();
		string[] themes = new string[dirs.Length];
		for (int i = 0, l = dirs.Length; i < l; i++) themes[i] = dirs[i].Name;
		return themes;
	}

	public static ArrayList getFolderContents(string dir) {
		DirectoryInfo d_info = new DirectoryInfo(dir);
		if (!d_info.Exists) d_info = new DirectoryInfo(HttpContext.Current.Server.MapPath("~"));

			ArrayList contents = new ArrayList();

			if (!d_info.ToString().Equals(d_info.Root.ToString())) contents.Add(new string[] { "..", "0", d_info.Parent.FullName });

			foreach (DirectoryInfo d in d_info.GetDirectories()) contents.Add(new string[] { d.Name, "0", d.FullName });
			foreach (FileInfo f in d_info.GetFiles()) contents.Add(new string[] { f.Name, "1", f.FullName });

			return contents;
		}
	}
}
