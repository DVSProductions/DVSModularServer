using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security;
/// <summary>
/// a class that decrypts servers
/// </summary>
class ServerDecryptor {
	/// <summary>
	/// Encryption Scheme
	/// </summary>
	[Import]
	IServerCrypto crypto;
	/// <summary>
	/// unzipped files and their encrypted content
	/// </summary>
	readonly Dictionary<string, byte[]> files;
	bool succesfullyDecrypted = false;
	/// <summary>
	/// unzipped files and their decrypted content
	/// </summary>
	readonly Dictionary<string, byte[]> decryptedFiles;
	/// <summary>
	/// Generate a <see cref="ServerDecryptor"/> Instance from a given config and archive
	/// </summary>
	/// <param name="a">Archive to decrypt</param>
	/// <param name="cfg">config file from the archive</param>
	public ServerDecryptor(ZipArchive a, EncryptedServerConfig cfg) {
		files = new Dictionary<string, byte[]>();
		decryptedFiles = new Dictionary<string, byte[]>();
		CryptoLoader(a, cfg);
		foreach (var f in cfg.EncryptedFiles)
			files.Add(f, ReadZipEntry(a.GetEntry(f)));
		files.Add(cfg.ServerFileName, ReadZipEntry(a.GetEntry(cfg.ServerFileName)));
	}
	/// <summary>
	/// Get the data from a zip entry
	/// </summary>
	private static byte[] ReadZipEntry(ZipArchiveEntry e) {
		using (var ms = new MemoryStream()) {
			e.Open().CopyTo(ms);
			return ms.ToArray();
		}
	}
	/// <summary>
	/// Detects the encryption algorithm and loads the custom one if present and required
	/// </summary>
	/// <param name="a">zip archive</param>
	/// <param name="cfg">loaded config</param>
	private void CryptoLoader(ZipArchive a, EncryptedServerConfig cfg) {
		switch (cfg.Encryption) {
			case EncryptedServerConfig.EncryptionType.AES:
				crypto = new ServerCryptoAES();
				break;
			case EncryptedServerConfig.EncryptionType.CUSTOM:
				using (var cat = new AggregateCatalog()) {
					using (var ac = new AssemblyCatalog(Assembly.Load(ReadZipEntry(a.GetEntry(cfg.CryptoFileName))))) {
						cat.Catalogs.Add(ac);
						using (var loader = new CompositionContainer(cat))
							loader.ComposeParts(this);
					}
				}
				break;
			default:
				break;
		}
	}
	/// <summary>
	/// Decrypts all files using the password
	/// </summary>
	/// <param name="password"></param>
	public bool DecryptAll(SecureString password) {
		try {
			succesfullyDecrypted = false;
			decryptedFiles.Clear();
			using (var mut = crypto.KeyMutation(password))
				foreach (var f in files)
					decryptedFiles.Add(f.Key, crypto.Decrypt(f.Value, mut));
			succesfullyDecrypted = true;
			return true;
		}
		catch (System.Security.Cryptography.CryptographicException) {
			C.WriteLineE("Invalid Password!");
			return false;
		}
		catch (Exception ex) {
			C.WriteLineE($"Server Decryption error: {ex.ToString()}");
			return false;
		}
	}
	/// <summary>
	/// If everything has been successfully decrypted this returns the files and their names.
	/// Otherwise this returns null
	/// </summary>
	public Dictionary<string, byte[]> DecryptedFiles => succesfullyDecrypted ? decryptedFiles : null;
}