using System;
using System.IO;
using System.Web;

public class FileSystemLayer {
	public FileSystemLayer() { }

	public static string GetFolderContents(string dir) {
		DirectoryInfo d_info = new DirectoryInfo(dir);
		if (!d_info.Exists) d_info = new DirectoryInfo(HttpContext.Current.Server.MapPath("~"));
		/*
		d_info = new DirectoryInfo("C:\\");
		DirectoryInfo parent = d_info.Parent;
		if (parent == null)
		parent.ToString(); // NullReferenceExceptionThrown
		*/

		string contents = Path.GetDirectoryName(d_info.ToString());
		foreach (DirectoryInfo d in d_info.GetDirectories()) contents += "<br />" + d.Name;
		foreach (FileInfo f in d_info.GetFiles()) contents += "<br />" + f.Name;

		return contents;
	}
}
