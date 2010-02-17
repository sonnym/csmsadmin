using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;

/*
use master;
SELECT * FROM sys.subsystems
exec sys.xp_msver
exec sys.server_info
exec sys.sp_enum_oledb_providers
exec sys.sp_configure
exec sys.xp_logininfo
exec sys.sp_who
exec sys.sp_who2

USE Northwind;
DBCC SHOW_STATISTICS (N'Products', ProductName)

http://www.databasejournal.com/features/mssql/article.php/2244381/Examining-SQL-Servers-IO-Statistics.htm
exec sp_monitor
DBCC PERFMON

SELECT * FROM fn_helpcollations()

SELECT @@VERSION

SELECT USER_NAME()
SELECT * FROM sysusers
*/

public class DBLayer {
	private string cs = Settings.ConnectionString;
	private SqlConnection con;
	private SqlCommand com;

	private bool showPlan = false;
	private bool saveLastQuery = false;
	private string lastQuery = "";

	  //////////////////
	 // Constructors //
	//////////////////

	public DBLayer() { }

	// sp indicates if SHOW_PLAN is to be on
	public DBLayer(bool sp) {
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
	public string getServerVersion() {
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
			using (com = new SqlCommand("USE " + db + "; EXEC sp_tables @table_type = \"'table'\"", con)) {
				SqlDataReader r = com.ExecuteReader();
				if (!r.HasRows) return null;

				ArrayList a = new ArrayList();
				while (r.Read()) a.Add(r["TABLE_NAME"]);
				return a;
			}
		}
	}

	public Dictionary<string, string> getStoredProcedures(string db) {
		using (con = __initConnection(false)) {
			con.Open();
			con.ChangeDatabase(db);
			using (com = new SqlCommand("select name, type from sys.objects where (type = 'P' OR type = 'FN') AND is_ms_shipped = 0", con)) { // will not return anything for master db
				SqlDataReader r = com.ExecuteReader();
				if (!r.HasRows) return null;

				Dictionary<string, string> d = new Dictionary<string, string>();
				while (r.Read()) d.Add(r["name"].ToString(), r["type"].ToString());
				return d;
			}
		}
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
			using (com = new SqlCommand("USE " + db + "; EXEC sp_spaceused " + tbl, con)) {
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
			using (com = new SqlCommand("SELECT columns.COLUMN_NAME, COLUMN_DEFAULT, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE, DATETIME_PRECISION, " +
										"COLLATION_NAME, CONSTRAINT_NAME FROM information_schema.columns LEFT OUTER JOIN information_schema.key_column_usage ON columns.column_name = " +
										"key_column_usage.column_name AND columns.table_catalog = key_column_usage.table_catalog AND columns.table_name = key_column_usage.table_name " +
										"WHERE columns.table_name = @tbl", con)) {
				com.Parameters.AddWithValue("@tbl", tbl);
				SqlDataReader r = com.ExecuteReader();
				return __sqlDataReaderToDataSet(ref r, 1);
			}
		}
	}

	public DataSet executeQuery(string db, string q) {
		using (con = __initConnection(false)) {
			con.Open();
			if (String.IsNullOrEmpty(db)) con.ChangeDatabase("master");
			else con.ChangeDatabase(db);
			if (this.showPlan) {
				using (com = new SqlCommand("SET SHOWPLAN_ALL ON", con)) {
					com.ExecuteNonQuery();
				}
			}
			using (com = new SqlCommand(q, con)) {
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

	public string restoreDatabase(string db, string f) {
		using (con = __initConnection(false)) {
			con.Open();
			con.ChangeDatabase("master");
			using (com = new SqlCommand("ALTER DATABASE " + db + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE;" +
										"RESTORE DATABASE @db FROM DISK = @f;" +
										"ALTER DATABASE " + db + " SET MULTI_USER WITH ROLLBACK IMMEDIATE", con)) {
				com.Parameters.AddWithValue("@db", db);
				com.Parameters.AddWithValue("@f", f);
				object temp = com.ExecuteScalar();
				HttpContext.Current.Response.Write(temp.GetType().Name);
				/*
				SqlDataReader tmp = com.ExecuteReader();
				HttpContext.Current.Response.Write(tmp.HasRows.ToString() + " " + tmp.RecordsAffected.ToString());
				*/
				return "";
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

	  /////////////////////
	 // Private methods //
	/////////////////////
	private string __getPrimaryKey(string db, string tbl) {
		using (con = __initConnection(false)) {
			con.Open();
			// http://blogs.x2line.com/al/articles/175.aspx
			using (com = new SqlCommand("USE " + db + "; SELECT name FROM syscolumns WHERE id IN (SELECT [id] FROM sysobjects WHERE [name] = @tbl) AND colid IN (SELECT sysindexkeys.colid FROM sysindexkeys JOIN sysobjects ON sysindexkeys.id = sysobjects.id WHERE sysindexkeys.indid = 1 AND sysobjects.name = @tbl)", con)) {
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
			using (com = new SqlCommand("USE " + db + "; SELECT TOP 1 COLUMN_NAME FROM information_schema.columns WHERE TABLE_NAME = @tbl ORDER BY ORDINAL_POSITION", con)) {
				com.Parameters.AddWithValue("@tbl", tbl);
				return com.ExecuteScalar().ToString();
			}
		}

	}

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

	private void __storeLastQuery() {
		// expand for parameterization?
		this.lastQuery = com.CommandText;
	}
}
