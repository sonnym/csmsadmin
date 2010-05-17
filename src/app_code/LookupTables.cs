namespace CSMSAdmin {
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

		public static string permissionState(string s) {
			switch(s.ToCharArray()[0]) {
				case 'D': return "Deny";
				case 'R': return "Revoke";
				case 'G': return "Grant";
				case 'W': return "Grant with Grant Option";
				default:  return null;
			}
		}

		/*
		public static string serverPermissionType(string s) {
			switch(s.Trim()) {
				case "DBO":  return "ADMINISTER BULK OPERATIONS";
				case "AL":   return "ALTER";
				case "ALCD": return "ALTER ANY CREDENTIAL";
				case "ALCO": return "ALTER ANY CONNECTION";
				case "ALDB": return "ALTER ANY DATABASE";
				case "ALES": return "ALTER ANY EVENT NOTIFICATION";
				case "ALHE": return "ALTER ANY ENDPOINT";
				case "ALLG": return "ALTER ANY LOGIN";
				case "ALLS": return "ALTER ANY LINKED SERVER";
				case "ALRS": return "ALTER RESOURCES";
				case "ALSS": return "ALTER SERVER STATE";
				case "ALST": return "ALTER SETTINGS";
				case "ALTR": return "ALTER TRACE";
				case "AUTH": return "AUTHENTICATE SERVER";
				case "CL":   return "CONTROL"; // when applies to sever, should return CONTROL SERVER
				case "CO":   return "CONNECT";
				case "COSQ": return "CONNECT SQL";
				case "CRDB": return "CREATE ANY DATABASE";
				case "CRDE": return "CREATE DDL EVENT NOTIFICATION";
				case "CRHE": return "CREATE ENDPOINT";
				case "CRTE": return "CREATE TRACE EVENT NOTIFICATION";
				case "IM":   return "IMPERSONATE";
				case "SHDN": return "SHUTDOWN";
				case "TO":   return "TAKE OWNERSHIP";
				case "VW":   return "VIEW DEFINITION";
				case "VWAD": return "VIEW ANY DEFINITION";
				case "VWDB": return "VIEW ANY DATABASE";
				case "VWSS": return "VIEW SERVER STATE";
				case "XA":   return "EXTERNAL ACCESS";
				default:     return null;
			}
		}

		public static string databasePermissionType(string s) {
			switch(s.Trim()) {
				case "AL":   return "ALTER";
				case "ALAK": return "ALTER ANY ASYMMETRIC KEY";
				case "ALAR": return "ALTER ANY APPLICATION ROLE";
				case "ALAS": return "ALTER ANY ASSEMBLY";
				case "ALCF": return "ALTER ANY CERTIFICATE";
				case "ALDS": return "ALTER ANY DATASPACE";
				case "ALED": return "ALTER ANY DATABASE EVENT NOTIFICATION";
				case "ALFT": return "ALTER ANY FULLTEXT CATALOG";
				case "ALMT": return "ALTER ANY MESSAGE TYPE";
				case "ALRL": return "ALTER ANY ROLE";
				case "ALRT": return "ALTER ANY ROUTE";
				case "ALSB": return "ALTER ANY REMOTE SERVICE BINDING";
				case "ALSC": return "ALTER ANY CONTRACT";
				case "ALSK": return "ALTER ANY SYMMETRIC KEY";
				case "ALSM": return "ALTER ANY SCHEMA";
				case "ALSV": return "ALTER ANY SERVICE";
				case "ALTG": return "ALTER ANY DATABASE DDL TRIGGER";
				case "ALUS": return "ALTER ANY USER";
				case "AUTH": return "AUTHENTICATE";
				case "BADB": return "BACKUP DATABASE";
				case "BALO": return "BACKUP LOG";
				case "CL":   return "CONTROL";
				case "CO":   return "CONNECT";
				case "CORP": return "CONNECT REPLICATION";
				case "CP":   return "CHECKPOINT";
				case "CRAG": return "CREATE AGGREGATE";
				case "CRAK": return "CREATE ASYMMETRIC KEY";
				case "CRAS": return "CREATE ASSEMBLY";
				case "CRCF": return "CREATE CERTIFICATE";
				case "CRDB": return "CREATE DATABASE";
				case "CRDF": return "CREATE DEFAULT";
				case "CRED": return "CREATE DATABASE DDL EVENT NOTIFICATION";
				case "CRFN": return "CREATE FUNCTION";
				case "CRFT": return "CREATE FULLTEXT CATALOG";
				case "CRMT": return "CREATE MESSAGE TYPE";
				case "CRPR": return "CREATE PROCEDURE";
				case "CRQU": return "CREATE QUEUE";
				case "CRRL": return "CREATE ROLE";
				case "CRRT": return "CREATE ROUTE";
				case "CRRU": return "CREATE RULE";
				case "CRSB": return "CREATE REMOTE SERVICE BINDING";
				case "CRSC": return "CREATE CONTRACT";
				case "CRSK": return "CREATE SYMMETRIC KEY";
				case "CRSM": return "CREATE SCHEMA";
				case "CRSN": return "CREATE SYNONYM";
				case "CRSV": return "CREATE SERVICE";
				case "CRTB": return "CREATE TABLE";
				case "CRTY": return "CREATE TYPE";
				case "CRVW": return "CREATE VIEW";
				case "CRXS": return "CREATE XML SCHEMA COLLECTION";
				case "DL":   return "DELETE";
				case "EX":   return "EXECUTE";
				case "IM":   return "IMPERSONATE";
				case "IN":   return "INSERT";
				case "RC":   return "RECEIVE";
				case "RF":   return "REFERENCES";
				case "SL":   return "SELECT";
				case "SN":   return "SEND";
				case "SPLN": return "SHOWPLAN";
				case "SUQN": return "SUBSCRIBE QUERY NOTIFICATIONS";
				case "TO":   return "TAKE OWNERSHIP";
				case "UP":   return "UPDATE";
				case "VW":   return "VIEW DEFINITION";
				case "VWCT": return "VIEW CHANGE TRACKING";
				case "VWDS": return "VIEW DATABASE STATE";
				default:     return null;
			}
		}
		*/
	}
}
