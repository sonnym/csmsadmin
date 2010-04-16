public class LookupTables {
	public static string comparisonOperators(string s) {
		switch(s) {
			case "eq":  return " = ";
			case "neq": return " != ";
			case "lt":  return " < ";
			case "lte": return " <= ";
			case "gt":  return " > ";
			case "gte": return " >= ";
			case "bet": return " BETWEEN ";
			case "l":   return " LIKE ";
			case "nl":  return " NOT LIKE ";
			case "all": return " ALL ";
			case "any": return " ANY ";
			case "exi": return " EXISTS ";
			case "i":   return " IN ";
			case "som": return " SOME ";
			case "isn": return " IS NULL ";
			case "inn": return " IS NULL ";
			default:    return " ";
		}
	}

	public static string pages(string s) {
		switch(s) {
			case "/browse.aspx":         return "Browse";
			case "/backup.aspx":         return "Backup";
			case "/charsets.aspx":       return "Charsets";
			case "/configuration.aspx":  return "Configuration";
			case "/insert.aspx":         return "Insert";
			case "/operations.aspx":     return "Operations";
			case "/permissions.aspx":    return "Permissions";
			case "/processes.aspx":      return "Processes";
			case "/providers.aspx":      return "Providers";
			case "/query.aspx":          return "SQL";
			case "/restore.aspx":        return "Restore";
			case "/select.aspx":         return "Search";
			case "/status.aspx":         return "Status";
			case "/struct.aspx":         return "Structure";
			default:                     return "Structure"; // default.aspx, /
		}
	}

	public static string principalType(string s) {
		switch(s.ToCharArray()[0]) {
			case 'S': return "SQL login";
			case 'U': return "Windows login";
			case 'G': return "Windows group";
			case 'R': return "Server role";
			case 'C': return "Login mapped to a certificate";
			case 'K': return "Login mapped to an asymmetric key";
			default:  return null;
		}
	}

	public static string backupFileType(string s) {
		switch (s.ToCharArray()[0]) {
			case 'L': return "Microsoft SQL Server log file";
			case 'D': return "SQL Server data file";
			case 'F': return "Full Text Catalog";
			default:  return null;
		}
	}
}
