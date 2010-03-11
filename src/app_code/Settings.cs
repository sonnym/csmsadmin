public class Settings {
	public static string Version = "0.4.1";

	// master connection string - required to access database before login
	public static string ConnectionString = @"Server=.\SQLEXPRESS;Trusted_Connection=True;MultipleActiveResultSets=True;Timeout=60;";

	// security options
	public static bool DisableLoginPage = false;
	public static string LoginConnectionString = "Server={0};Trusted_Connection=True;MultipleActiveResultSets=True;Timeout=60;User Id={1};Password={2};";

	public static string DefaultServerBrowserPath = @"C:\";
	public static string DefaultTheme = "sparse";
}
