using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;

/*
exec sp_monitor
DBCC PERFMON

sys.all_parameters

DBCC SHRINKDATABASE
sp_estimate_data_compression_savings

sys.dm_exec_sql_text
sys.dm_os_performance_counters
*/

public class DBLayer {
	private string cs;
	private SqlConnection con;
	private SqlCommand com;

	private bool showPlan = false;
	private bool saveLastQuery = false;
	private string lastQuery = "";

	  //////////////////
	 // Constructors //
	//////////////////

	public DBLayer() {
		cs = (HttpContext.Current.Session != null && HttpContext.Current.Session["cs"] != null) ? HttpContext.Current.Session["cs"].ToString() : String.Empty;
	}

	// sp indicates if SHOW_PLAN is to be on
	public DBLayer(bool sp) : this() { 
		this.showPlan = sp;
	}

	  ////////////////
	 // Properties //
	////////////////
	public bool ShowPlan {
		get { return this.showPlan; }
		set { this.showPlan = value; }
	}
	public bool SaveLastQuery {
		get { return this.saveLastQuery; }
		set { this.saveLastQuery = value; }
	}
	public string LastQuery {
		get { return this.lastQuery; }
	}

	  ////////////////////
	 // Public methods //
	////////////////////
	public bool testConnectionString(string s) {
		bool success = true;
		try {
			using (con = new SqlConnection(s)) { con.Open(); }
		} catch { success = false; }

		return success;
	}

	public DataRowCollection getServers() {
		using (con = new SqlConnection(Settings.ConnectionString)) { // default connection string necessary here
			con.Open();
			using (com = new SqlCommand("SELECT server_id, name FROM sys.servers", con)) {
				SqlDataReader r = com.ExecuteReader();
				return __sqlDataReaderToDataSet(ref r, 1).Tables[0].Rows;
			}
		}
	}

	public string getServerDataSource(int id) {
		string ds = String.Empty;

		using (con = new SqlConnection(Settings.ConnectionString)) {
			con.Open();
			using (com = new SqlCommand("SELECT data_source FROM sys.servers WHERE server_id = @i", con)) {
				com.Parameters.AddWithValue("@i", id);
				ds = com.ExecuteScalar().ToString();
			}
		}
		return ds;
	}

	public string getServerVersion() { // alternatively: SELECT @@VERSION
		using (con = __initConnection(false)) {
			con.Open();
			return con.ServerVersion;
		}
	}

	public string getServerName() {
		DataSet t = getServerInformation();
		return t.Tables[0].Rows[0][1].ToString();
	}

	public DataSet getServerInformation() {
		using (con = __initConnection(false)) {
			con.Open();
			using (com = new SqlCommand("SELECT * FROM sys.servers", con)) {
				SqlDataReader r = com.ExecuteReader();
				return __sqlDataReaderToDataSet(ref r, 1);
			}
		}
	}

	public ArrayList getDatabaseNames() {
		using (con = __initConnection(false)) {
			con.Open();
			using (com = new SqlCommand("SELECT name FROM sys.databases", con)) {
				SqlDataReader r = com.ExecuteReader();
				if (!r.HasRows) return null;

				ArrayList a = new ArrayList();
				while (r.Read()) a.Add(r["name"]);
				return a;
			}
		}
	}

	public ArrayList getTableNames(string db) {
		using (con = __initConnection(false)) {
			con.Open();
			con.ChangeDatabase(db);
			using (com = new SqlCommand("EXEC sp_tables @table_type = \"'table'\"", con)) {
				SqlDataReader r = com.ExecuteReader();
				if (!r.HasRows) return null;

				ArrayList a = new ArrayList();
				while (r.Read()) a.Add(r["TABLE_NAME"]);
				return a;
			}
		}
	}

	public DataTable getStoredProcedures(string db) {
		return executeQuery(db, "SELECT objects.name, objects.type, routines.DATA_TYPE, routines.CHARACTER_MAXIMUM_LENGTH, routines.NUMERIC_PRECISION, routines.NUMERIC_SCALE, routines.DATETIME_PRECISION " +
								"FROM sys.objects AS objects LEFT OUTER JOIN information_schema.routines AS routines ON objects.name = routines.specific_name " +
								"WHERE objects.type IN ('P', 'FN') AND objects.is_ms_shipped = 0").Tables[0];
	}

	public DataTable getViews(string db) {
		return executeQuery(db, "SELECT object_id, name FROM sys.views").Tables[0];
	}

	public Dictionary<string, string> getTableSize(string db, string tbl) {
		using (con = __initConnection(false)) {
			con.Open();
			con.ChangeDatabase(db);
			using (com = new SqlCommand("EXEC sp_spaceused @tbl", con)) {
				com.Parameters.AddWithValue("@tbl", tbl);
				SqlDataReader r = com.ExecuteReader();
				if (!r.HasRows) return null;

				r.Read(); Dictionary<string, string> d = new Dictionary<string, string>();
				for (int i = 0; i < r.FieldCount; i++) d.Add(r.GetName(i), r[i].ToString());
				return d;
			}
		}
	}

	public string getTableReservedSize(string db, string tbl) {
		using (con = __initConnection(false)) {
			con.Open();
			con.ChangeDatabase(db);
			using (com = new SqlCommand("EXEC sp_spaceused " + tbl, con)) {
				SqlDataReader r = com.ExecuteReader();
				if (!r.HasRows) return null;

				r.Read();
				return r["reserved"].ToString();
			}
		}
	}

	public DataSet getColumnInformation(string db, string tbl) {
		using (con = __initConnection(false)) {
			con.Open();
			con.ChangeDatabase(db);
			using (com = new SqlCommand("SELECT columns.name, defaults.definition AS [default], types.name AS type, is_nullable, max_length, columns.scale, precision, collation_name, " +
										"is_identity FROM sys.columns AS columns LEFT OUTER JOIN sys.objects ON columns.object_id = sys.objects.object_id " +
										"LEFT OUTER JOIN sys.systypes AS types ON columns.system_type_id = types.xtype " +
										"LEFT OUTER JOIN sys.default_constraints AS defaults ON columns.default_object_id = defaults.object_id " +
										"WHERE sys.objects.name = @tbl AND sys.objects.type = 'U'", con)) {
				com.Parameters.AddWithValue("@tbl", tbl);
				SqlDataReader r = com.ExecuteReader();
				return __sqlDataReaderToDataSet(ref r, 1);
			}
		}
	}

	public string getUsername() {
		return executeQuery("SELECT USER_NAME()").Tables[0].Rows[0][0].ToString();
	}

	public DataTable getProviders() {
		return executeQuery("EXEC sys.sp_enum_oledb_providers").Tables[0];
	}

	// server permissions

	public DataTable getServerPrincipalTypes() {
		return executeQuery("SELECT DISTINCT(type) FROM sys.server_principals WHERE type != 'C' ORDER BY type ASC").Tables[0];
	}

	public DataTable getServerPrincipals() { // case insensitive under current configuration - perhaps should include a collation here
		return executeQuery("SELECT type, SUBSTRING(name, 1, 1) AS first, COUNT(SUBSTRING(name, 1, 1)) AS count " +
								"FROM sys.server_principals WHERE type != 'C' GROUP BY SUBSTRING(name, 1, 1), type ORDER BY type ASC, first ASC").Tables[0];
	}

	public DataTable getServerPrincipals(string type, string letter) {
		string where = String.Empty;
		SqlParameterCollection p = __getEmptyParameterCollection();

		if (!String.IsNullOrEmpty(type)) {
			where += "WHERE type = @type ";
			p.AddWithValue("@type", type);
		}
		if (!String.IsNullOrEmpty(letter)) {
			where += (String.IsNullOrEmpty(where) ? "WHERE " : "AND ") + "SUBSTRING(name, 1, 1) = @letter ";
			p.AddWithValue("@letter", letter);
		}

		return executeQuery("SELECT principal_id, name, is_disabled, create_date, modify_date, default_database_name " +
								"FROM sys.server_principals " + where + " ORDER BY name ASC", p).Tables[0];
	}

	// database permissions

	public DataTable getDatabasePrincipalTypes(string db) {
		return executeQuery(db, "SELECT DISTINCT(type) FROM sys.database_principals WHERE type != 'C' ORDER BY type ASC").Tables[0];
	}

	public DataTable getDatabasePrincipals(string db) {
		return executeQuery(db, "SELECT type, SUBSTRING(name, 1, 1) AS first, COUNT(SUBSTRING(NAME, 1, 1)) AS count " +
									"FROM sys.database_principals WHERE type != 'C' GROUP BY SUBSTRING(name, 1, 1), type ORDER BY type ASC, first ASC").Tables[0];
	}
	
	public DataTable getDatabasePrincipals(string db, string type, string letter) {
		string where = String.Empty;
		SqlParameterCollection p = __getEmptyParameterCollection();

		if (!String.IsNullOrEmpty(type)) {
			where += "WHERE type = @type ";
			p.AddWithValue("@type", type);
		}
		if (!String.IsNullOrEmpty(letter)) {
			where += (String.IsNullOrEmpty(where) ? "WHERE " : "AND ") + "SUBSTRING(name, 1, 1) = @letter ";
			p.AddWithValue("@letter", letter);
		}

		return executeQuery(db, "SELECT principal_id, name, create_date, modify_date FROM sys.database_principals " + where + " ORDER BY name ASC", p).Tables[0];
	}

	/*
	public DataTable getServerPermissions() {
		return executeQuery("SELECT state_desc, permission_name, name, type_desc, is_disabled from sys.server_permissions AS permissions " +
										"LEFT JOIN sys.server_principals AS principals ON permissions.grantee_principal_id = principals.principal_id").Tables[0];
	}

	public DataTable getDatabasePermissions(string db, strying type, string letter) {
		return executeQuery(db, " SELECT permissions.state_desc, permission_name, schemas.name AS [schema], objects.name AS object, principals.name AS principal " +
									"FROM sys.database_permissions AS permissions " +
									"JOIN sys.objects AS objects ON permissions.major_id = objects.object_id " +
									"JOIN sys.schemas AS schemas ON objects.schema_id = schemas.schema_id " +
									"JOIN sys.database_principals AS principals ON permissions.grantee_principal_id = principals.principal_id").Tables[0];
	}
	*/

	// restore functions
	public DataTable restoreLabelOnly(string f) {
		SqlParameterCollection p = __getEmptyParameterCollection();
		p.AddWithValue("@f", f);
		return executeQuery("RESTORE LABELONLY FROM DISK = @f", p).Tables[0];
	}

	public DataRow restoreHeaderOnly(string f) {
		SqlParameterCollection p = __getEmptyParameterCollection();
		p.AddWithValue("@f", f);
		return executeQuery("RESTORE HEADERONLY FROM DISK = @f", p).Tables[0].Rows[0];
	}

	public DataTable restoreFileListOnly(string f) {
		SqlParameterCollection p = __getEmptyParameterCollection();
		p.AddWithValue("@f", f);
		return executeQuery("RESTORE FILELISTONLY FROM DISK = @f", p).Tables[0];
	}

	public string restoreDatabase(string db, string f, bool resume) {
		string result = String.Empty;

		try {
			using (con = __initConnection(false)) {
				con.Open();
				con.ChangeDatabase("master");
				using (com = new SqlCommand("RESTORE DATABASE @db FROM DISK = @f" + ((resume) ? " WITH RESTART" : ""), con)) {
					com.Parameters.AddWithValue("@db", db);
					com.Parameters.AddWithValue("@f", f);
					result = com.ExecuteScalar().ToString();
				}
			}
		} catch (Exception ex) {
			result = ex.ToString();
		}
		return result;
	}

	// configuration and statistics
	public DataTable getConfiguration() {
		return executeQuery("SELECT name, value, value_in_use, minimum, maximum, description FROM sys.configurations ORDER BY name").Tables[0];
	}

	public DataSet getCharsets() {
		return executeQuery("SELECT id, name, description FROM sys.syscharsets WHERE type = 1001 ORDER BY name; " +
								"SELECT sortorders.csid, sortorders.name, sortorders.description FROM sys.syscharsets AS sortorders " +
								"LEFT OUTER JOIN (SELECT id, name FROM sys.syscharsets WHERE type = 1001)charsets ON sortorders.csid = charsets.id " +
								"WHERE sortorders.type = 2001 ORDER BY charsets.name, sortorders.name");
	}

	public DataTable getCollations() {
		return executeQuery("SELECT * FROM fn_helpcollations() ORDER BY name").Tables[0];
	}

	public DataTable getProcesses() {
		return executeQuery("SELECT spid, waittime, hostname, dbs.name AS db, procs.loginame, procs.status, cmd FROM sys.sysprocesses AS procs " +
								"LEFT OUTER JOIN sys.sysdatabases AS dbs ON procs.dbid = dbs.dbid").Tables[0];
	}

	public DataTable getOptimizations() {
		return executeQuery("SELECT * FROM sys.dm_exec_query_optimizer_info").Tables[0];
	}

	public DataRowCollection getTypes() {
		return executeQuery("SELECT name FROM sys.systypes").Tables[0].Rows;
	}

	public DataSet executeQuery(string q) {
		return executeQuery("master", q, null);
	}
	public DataSet executeQuery(string db, string q) {
		return executeQuery(db, q, null);
	}
	public DataSet executeQuery(string q, SqlParameterCollection p) {
		return executeQuery("master", q, p);
	}
	public DataSet executeQuery(string db, string q, SqlParameterCollection p) {
		using (con = __initConnection(false)) {
			con.Open();
			if (!String.IsNullOrEmpty(db)) con.ChangeDatabase(db);
			if (this.showPlan) {
				using (com = new SqlCommand("SET SHOWPLAN_ALL ON", con)) {
					com.ExecuteNonQuery();
				}
			}
			using (com = new SqlCommand(q, con)) {
				if (p != null) for (int i = 0, l = p.Count; i < l; i++) com.Parameters.AddWithValue(p[i].ParameterName, p[i].Value);
				SqlDataReader r = com.ExecuteReader();
				return __sqlDataReaderToDataSet(ref r, q.TrimEnd(new char[] {';'}).Split(new char[] {';'}).Length);
			}
		}
	}

	public DataSet getRecordsFromTable(string db, string tbl, int start, int count) {
		string q = "SELECT TOP " + count + " * FROM " + tbl;
		string sort = __getPrimaryKey(db, tbl);
		if (sort == null || sort.Length == 0) sort = __getFirstColumn(db, tbl);
		// pagination => comment from http://www.planet-source-code.com/vb/scripts/ShowCode.asp?txtCodeId=850&lngWId=5
		// alternatively => http://blogs.x2line.com/al/archive/2005/11/18/1323.aspx
		if (start != 0) q += " WHERE " + sort + " NOT IN (SELECT " + sort + " FROM (SELECT TOP " + start + " " + sort + " FROM " + tbl + ") AS t) ORDER BY " + sort + " ASC";
		else q += " ORDER BY " + sort + " ASC";

		using (con = __initConnection(false)) {
			con.Open();
			con.ChangeDatabase(db);
			using (com = new SqlCommand(q, con)) {
				SqlDataReader r = com.ExecuteReader();
				DataSet ds = __sqlDataReaderToDataSet(ref r, 1);

				if (saveLastQuery) __storeLastQuery();
				return ds;
			}
		}
	}

	public int getRowCount(string db, string tbl) {
		string key = __getPrimaryKey(db, tbl);
		if (key == null || key.Length == 0) key = __getFirstColumn(db, tbl);
		using (con = __initConnection(false)) {
			con.Open();
			con.ChangeDatabase(db);
			using (com = new SqlCommand("SELECT COUNT(" + key + ") AS c FROM " + tbl, con)) {
				object o = com.ExecuteScalar();
				return int.Parse(o.ToString());
			}
		}
	}

	public bool createDatabase(string n) {
		using (con = __initConnection(false)) {
			con.Open();
			con.ChangeDatabase("master");
			using (com = new SqlCommand("CREATE DATABASE " + n, con)) { // cannot parameterize without getting a syntax error => need to find where sanitization happens internally
				try {
					com.ExecuteNonQuery();
					return true;
				} catch (Exception ex) {
					HttpContext.Current.Response.Write(ex.ToString());
					return false;
				}
			}
		}
	}

	public bool createTable(string db, string tbl, string cols) {
		using (con = __initConnection(false)) {
			con.Open();
			con.ChangeDatabase(db);
			using (com = new SqlCommand("CREATE TABLE " + tbl + "(" + cols + ")", con)) {
				try {
					com.ExecuteNonQuery();
					return true;
				} catch { return false; }
			}
		}
	}

	  /////////////////////
	 // Private methods //
	/////////////////////
	private SqlConnection __initConnection(bool stats) {
		SqlConnection c = new SqlConnection(cs);
		c.StatisticsEnabled = stats;
		return c;
	}

	private DataSet __sqlDataReaderToDataSet(ref SqlDataReader r, int c) {
		int max = c; DataSet ds = new DataSet();

		try {
			do {
				DataTable dt = new DataTable("Table" + (max - c).ToString());
				dt.Load(r);
				ds.Tables.Add(dt);
				c -= 1;
			} while (c > 0);
		} catch { }

		return ds;
	}

	private SqlParameterCollection __getEmptyParameterCollection() {
		return new SqlCommand().Parameters;
	}

	private void __storeLastQuery() {
		// expand for parameterization?
		this.lastQuery = com.CommandText;
	}

	private string __getPrimaryKey(string db, string tbl) {
		using (con = __initConnection(false)) {
			con.Open();
			// http://blogs.x2line.com/al/articles/175.aspx
			con.ChangeDatabase(db);
			using (com = new SqlCommand("SELECT name FROM syscolumns WHERE id IN (SELECT [id] FROM sysobjects WHERE [name] = @tbl) AND colid IN (SELECT sysindexkeys.colid FROM sysindexkeys JOIN sysobjects ON sysindexkeys.id = sysobjects.id WHERE sysindexkeys.indid = 1 AND sysobjects.name = @tbl)", con)) {
				com.Parameters.AddWithValue("@tbl", tbl);
				SqlDataReader r = com.ExecuteReader();
				if (r.HasRows) {
					r.Read();
					return r[0].ToString();
				} else return null;
			}
		}	
	}

	private string __getFirstColumn(string db, string tbl) {
		using (con = __initConnection(false)) {
			con.Open();
			con.ChangeDatabase(db);
			using (com = new SqlCommand("SELECT TOP 1 COLUMN_NAME FROM information_schema.columns WHERE TABLE_NAME = @tbl ORDER BY ORDINAL_POSITION", con)) {
				com.Parameters.AddWithValue("@tbl", tbl);
				return com.ExecuteScalar().ToString();
			}
		}

	}
}
