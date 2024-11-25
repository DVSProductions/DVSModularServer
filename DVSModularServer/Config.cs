using ModularServerSDK;
using System.Text.RegularExpressions;
namespace DVSModularServer;
internal partial class Config : IConfig {
	public const string ConfigFile = "Config\\ServerConfig.xml";
	public bool UseHttps { get; set; }
	public ushort Port { get; set; }
	public string Logfile { get; set; } = string.Empty;
	/// <summary>
	/// Domain name that all urls should point to
	/// </summary>
	public string ReportBackDomain { get; set; } = string.Empty;
	/// <summary>
	/// Downloads a list of all top level domains and validates whether the current one is included.
	/// This is just to help diagnose errors in case
	/// A: There is no internet. (this function will fail)
	/// B: A random or misspelled TLD has been used, making it impossible for this server to be reached
	/// </summary>
	private bool TLDValidity() {
		try {
			using var httpClient = new HttpClient();
			var response = httpClient.GetStringAsync(new Uri("http://data.iana.org/TLD/tlds-alpha-by-domain.txt")).Result;
			var lines = response.Split([Environment.NewLine, "\r\n", "\r", "\n"], StringSplitOptions.None);
			var s = ReportBackDomain.Split('.');
			var tld = s[^1].ToUpperInvariant();
			foreach(var l in lines)
				if(tld == l)
					return true;
		}
		catch { }
		return false;
	}
	/// <summary>
	/// Prints a error message in case the TLD check fails and asks whether the check results should be ignored
	/// </summary>
	private bool AskForValidity() {
		if(TLDValidity())
			return true;
		C.WriteLineE($"ERROR: Top level domain not found! ({ReportBackDomain})");
		C.WriteLine("This means that your Server might not be reachable!");
		return C.Input?.PromptYN("Do you wish to continue?") == true;
	}
	/// <summary>
	/// Ensures that the <see cref="ReportBackDomain"/> is actually a domain
	/// </summary>
	public bool ValidateSettings() =>
		DomainValidationRegex().IsMatch(ReportBackDomain) && AskForValidity();
	/// <summary>
	/// Creates the default configureation
	/// </summary>
	public Config() => FixSettings();
	/// <summary>
	/// Stores the current configuration in the <see cref="ConfigFile"/>
	/// </summary>
	public void Save() => ConfigLoader.Save(this, ConfigFile);
	/// <summary>
	/// Loads the current Configuration file and returns it.
	/// </summary>
	public static Config Load() => ConfigLoader.Load(ConfigFile, new Config());
	/// <summary>
	/// in case the settings are broken, this will be used
	/// </summary>
	public void FixSettings() {
		Port = 50001;
		Logfile = "ServerLogs.log";
		ReportBackDomain = "dvsproductions.de";
		UseHttps = false;
	}

	[GeneratedRegex(@"^([\w\.\-]+)((\.(\w){2,63})+)$")]
	private static partial Regex DomainValidationRegex();
}
