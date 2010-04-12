using System;

public static class Utilities {
	public static string jsonEncode(string s) {
		string str = (string) s;
		str = str.Replace("\\","\\\\");
		str = str.Replace("\"","\\\"");
		str = str.Replace("\r","\\r");
		str = str.Replace("\f","\\f");
		str = str.Replace("\n","\\n");
		str = str.Replace("\t","\\t");
		str = str.Replace("\b","\\b");
		return str;
	}
}
