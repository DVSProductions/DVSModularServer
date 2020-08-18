using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
namespace System {
	/// <summary>
	/// Configuration file for encrypted server modules. rawr
	/// This config file is included in the compressed result archive as metadata
	/// </summary>
	public class EncryptedServerConfig {
		/// <summary>
		/// Filename in the archive
		/// </summary>
		public const string ConfigFileName = "encServ.cfg";
		/// <summary>
		/// Options for Encryption Schemes. 
		/// May be extended beyond AES and Custom if more Encryption Schemes are supported by default
		/// </summary>
		public enum EncryptionType { AES, CUSTOM }
		/// <summary>
		/// Stores what encryption is used
		/// </summary>
		public EncryptionType Encryption { get; set; }
		/// <summary>
		/// If custom encryption is used, this file is the dll that implements <see cref="System.Security.IServerCrypto"/>
		/// in order to decrypt the files included
		/// </summary>
		public string CryptoFileName { get; set; }
		/// <summary>
		/// A list of additional files that were encrypted
		/// </summary>
		public List<string> EncryptedFiles { get; private set; }
		/// <summary>
		/// Filename of the encrypted server
		/// </summary>
		public string ServerFileName { get; set; }
		/// <summary>
		/// Write the Configfile to disk in the current directory
		/// </summary>
		public void Save() {
			try {
				using(var fs = new FileStream(ConfigFileName, FileMode.Create))
					Save(fs);
				C.WriteLineS("wrote config file succesfully");
			}
			catch(Exception ex) {
				C.WriteLineE($"ERROR WRITING CONFIG FILE: {ex}");
			}
		}
		/// <summary>
		/// Save the Config to a given stream using <see cref="UTF8Encoding"/>
		/// </summary>
		/// <param name="target">Target stream</param>
		public void Save(Stream target) {
			using(var writer = new StreamWriter(target, new UTF8Encoding())) {
				new XmlSerializer(typeof(EncryptedServerConfig)).Serialize(writer, this);
			}
		}
		/// <summary>
		/// Generate a blanked <see cref="EncryptedServerConfig"/>
		/// </summary>
		public EncryptedServerConfig() => EncryptedFiles = new List<string>();
		/// <summary>
		/// Load a <see cref="EncryptedServerConfig"/> from disk 
		/// </summary>
		/// <param name="path">Path where the config is located</param>
		public static EncryptedServerConfig Load(string path) {
			using(var fs = new FileStream(path, FileMode.Open))
				return Load(fs);
		}
		/// <summary>
		/// Load the config from a given <see cref="Stream"/>
		/// </summary>
		/// <param name="from">Source Stream</param>
		public static EncryptedServerConfig Load(Stream from) {
			using(var reader = new StreamReader(from))
				return Load(reader);
		}
		/// <summary>
		/// Read the config from a given <see cref="TextReader"/>
		/// </summary>
		/// <param name="reader">Source reader</param>
		/// <returns></returns>
		public static EncryptedServerConfig Load(TextReader reader) => (EncryptedServerConfig) new XmlSerializer(typeof(EncryptedServerConfig)).Deserialize(reader);
	}
}
