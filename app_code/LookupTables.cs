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
			case "/browse.aspx":     return "Browse";
			case "/charsets.aspx":   return "Charsets";
			case "/home.aspx":       return "Structure";
			case "/processes.aspx":  return "Processes";
			case "/query.aspx":      return "SQL";
			case "/restore.aspx":    return "Restore";
			case "/struct.aspx":     return "Structure";
			case "/select.aspx":     return "Search";
			default:                 return null;
		}
	}
}
