using System.Text;
using System.Xml.Serialization;
namespace ModularServerSDK;
/// <summary>
/// A tool for generating XML Configuration files using <see cref="IConfig"/> instances
/// Step1: Generate your own config class that implements <see cref="IConfig"/>
/// Step2: Use the static methods of this class to load the data into your class
/// </summary>
public static class ConfigLoader {
	/// <summary>
	/// Writes a IConfig to a specified file
	/// </summary>
	/// <typeparam name="T">Your <see cref="IConfig"/> implementation</typeparam>
	/// <param name="what">IConfig Class instance</param>
	/// <param name="path">Full path to the config file</param>
	public static void Save<T>(T what, string path) where T : IConfig {
		if(!what.ValidateSettings()) {
			what.FixSettings();
			if(!what.ValidateSettings())
				throw new InvalidDataException("ConfigLoader.Save: Fixed Settings are still invalid!");
		}
		try {
			var directory = Path.GetDirectoryName(Path.GetFullPath(path));
			if(directory == null) {
				C.WriteLineE("Path was null");
				return;
			}
			if(!Directory.Exists(directory))
				Directory.CreateDirectory(directory);
			Stream? fs = null;
			try {
				fs = new FileStream(path, FileMode.Create);
				using var writer = new StreamWriter(fs, new UTF8Encoding());
				fs = null;
				new XmlSerializer(typeof(T)).Serialize(writer, what);
			}
			finally {
				fs?.Dispose();
			}
			C.WriteLineS("wrote config file successfully");
		}
		catch(Exception ex) {
			C.WriteLineE($"ERROR WRITING CONFIG FILE: {ex}");
		}
	}
	/// <summary>
	/// Loads your config from a file. If this fails for any reason, the alternative instance is used.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="path">Full path to the config file</param>
	/// <param name="alternative">Alternative instance as backup</param>
	public static T Load<T>(string path, T alternative) where T : class, IConfig {
		C.WriteLine("Loading config...");
		T? settings = default;
		try {
			Stream? fs = null;
			try {
				fs = new FileStream(path, FileMode.Open);
				using var reader = new StreamReader(fs);
				fs = null;
				settings = new XmlSerializer(typeof(T)).Deserialize(reader) as T;
			}
			finally {
				fs?.Dispose();
			}
			if(settings?.ValidateSettings() != true)
				throw new FormatException("Config validation failed!");
		}
		catch(DirectoryNotFoundException) {
			C.WriteLine("Creating config folder and new config");
			settings = alternative;
			Save(settings, path);
		}
		catch(FileNotFoundException) {
			C.WriteLine("Creating default config");
			settings = alternative;
			Save(settings, path);
		}
		catch(Exception ex) {
			C.WriteLineE($"Invalid config: {ex}");
			C.WriteLine("Using default config");
			settings = alternative;
		}
		C.WriteLine("Config loaded");
		return settings;
	}
}
