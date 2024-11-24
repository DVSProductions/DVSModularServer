using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Reflection;
using System.Security;
/// <summary>
/// a class that decrypts servers
/// </summary>
internal class ServerDecryptor {
	/// <summary>
	/// Encryption Scheme
	/// </summary>
	[Import]
	[AllowNull]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "<Pending>")]
	private IServerCrypto crypto;

	/// <summary>
	/// unzipped files and their encrypted content
	/// </summary>
	private readonly Dictionary<string, byte[]> files;
	private bool succesfullyDecrypted = false;

	/// <summary>
	/// unzipped files and their decrypted content
	/// </summary>
	private readonly Dictionary<string, byte[]> decryptedFiles;
	/// <summary>
	/// Generate a <see cref="ServerDecryptor"/> Instance from a given config and archive
	/// </summary>
	/// <param name="a">Archive to decrypt</param>
	/// <param name="cfg">config file from the archive</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	public ServerDecryptor(ZipArchive a, EncryptedServerConfig cfg) {
		/// <summary>
		/// Detects the encryption algorithm and loads the custom one if present and required
		/// </summary>
		/// <param name="a">zip archive</param>
		/// <param name="cfg">loaded config</param>
		void CryptoLoader(ZipArchive a, EncryptedServerConfig cfg) {
			switch(cfg.Encryption) {
				case EncryptedServerConfig.EncryptionType.AES:
					crypto = new ServerCryptoAES();
					break;
				case EncryptedServerConfig.EncryptionType.CUSTOM:
					using(var cat = new AggregateCatalog()) {
						var e = a.GetEntry(cfg.CryptoFileName);
						if(e == null) {
							C.WriteLineE("Custom Crypto not found!");
							return;
						}
						using var ac = new AssemblyCatalog(Assembly.Load(ReadZipEntry(e)));
						cat.Catalogs.Add(ac);
						using var loader = new CompositionContainer(cat);
						loader.ComposeParts(this);
					}
					break;
				default:
					break;
			}
		}
		files = [];
		decryptedFiles = [];
		CryptoLoader(a, cfg);
		foreach(var f in cfg.EncryptedFiles) {
			var e = a.GetEntry(f);
			if(e == null) {
				C.WriteLineE($"File {f} not found!");
				continue;
			}
			files.Add(f, ReadZipEntry(e));
		}

		var serverFile = a.GetEntry(cfg.ServerFileName);
		if(serverFile == null) {
			C.WriteLineE($"Server File {cfg.ServerFileName} not found!");
			return;
		}
		files.Add(cfg.ServerFileName, ReadZipEntry(serverFile));
	}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	/// <summary>
	/// Get the data from a zip entry
	/// </summary>
	private static byte[] ReadZipEntry(ZipArchiveEntry e) {
		using var ms = new MemoryStream();
		e.Open().CopyTo(ms);
		return ms.ToArray();
	}
	/// <summary>
	/// Decrypts all files using the password
	/// </summary>
	/// <param name="password"></param>
	public bool DecryptAll(SecureString password) {
		try {
			succesfullyDecrypted = false;
			decryptedFiles.Clear();
			using(var mut = crypto.KeyMutation(password))
				foreach(var f in files)
					decryptedFiles.Add(f.Key, crypto.Decrypt(f.Value, mut));
			succesfullyDecrypted = true;
			return true;
		}
		catch(System.Security.Cryptography.CryptographicException) {
			C.WriteLineE("Invalid Password!");
			return false;
		}
		catch(Exception ex) {
			C.WriteLineE($"Server Decryption error: {ex}");
			return false;
		}
	}
	/// <summary>
	/// If everything has been successfully decrypted this returns the files and their names.
	/// Otherwise this returns null
	/// </summary>
	public Dictionary<string, byte[]>? DecryptedFiles => succesfullyDecrypted ? decryptedFiles : null;
}