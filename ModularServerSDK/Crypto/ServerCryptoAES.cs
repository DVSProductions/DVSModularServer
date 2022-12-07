using System.IO;
using System.Security.Cryptography;
namespace System.Security {
	/// <summary>
	/// This is the underlying Encryption Implementation for the <see cref="CryptoWrapper{TCryptoProvider}"/>
	/// <para>
	/// It is recommended to not use this class directly but rather a <see cref="CryptoWrapper{TCryptoProvider}"/> with this class
	/// </para>
	/// </summary>
	public class ServerCryptoAES : IServerCrypto {
		/// <summary>
		/// default salt
		/// </summary>
		private static readonly byte[] salt = new byte[] { 49, 103, 216, 55, 237, 139, 38, 192, 142, 81, 178, 208, 84 };
		/// <summary>
		/// Default Decryption algorithm using AES
		/// </summary>
		/// <param name="data">Data to decrypt</param>
		/// <param name="mutatedKey">The password</param>
		public byte[] Decrypt(byte[] data, SecureString mutatedKey) {
			if (data == null) return null;
			using (var encryptor = Aes.Create()) {
				using (var pdb = CryptoTools.DeriveBytes(mutatedKey, salt)) {
					encryptor.Key = pdb.GetBytes(32);
					encryptor.IV = pdb.GetBytes(16);
				}
				using (var ms = new MemoryStream()) {
					using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
						cs.Write(data, 0, data.Length);
					return ms.ToArray();
				}
			}
		}

		/// <summary>
		/// Default Decryption algorithm using AES
		/// </summary>
		/// <param name="data">Data to decrypt</param>
		/// <param name="mutatedKey">The password</param>
		[Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Member als statisch markieren", Justification = "<Ausstehend>")]
		public byte[] Decrypt(byte[] data, SecureString mutatedKey, byte[] salt) {
			if (data == null) return null;
			using (var encryptor = Aes.Create()) {
				using (var pdb = CryptoTools.DeriveBytes(mutatedKey, salt)) {
					encryptor.Key = pdb.GetBytes(32);
					encryptor.IV = pdb.GetBytes(16);
				}
				using (var ms = new MemoryStream()) {
					using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
						cs.Write(data, 0, data.Length);
					return ms.ToArray();
				}
			}
		}
		/// <summary>
		/// Default Encryption algorithm using AES
		/// </summary>
		/// <param name="data">Data to encrypt</param>
		/// <param name="mutatedKey">The password</param>
		public byte[] Encrypt(byte[] data, SecureString mutatedKey) {
			if (data == null) return null;
			using (var encryptor = Aes.Create()) {
				using (var pdb = CryptoTools.DeriveBytes(mutatedKey, salt)) {
					encryptor.Key = pdb.GetBytes(32);
					encryptor.IV = pdb.GetBytes(16);
				}
				using (var ms = new MemoryStream()) {
					using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
						cs.Write(data, 0, data.Length);
					return ms.ToArray();
				}
			}
		}

		/// <summary>
		/// Default Encryption algorithm using AES
		/// </summary>
		/// <param name="data">Data to encrypt</param>
		/// <param name="mutatedKey">The password</param>
		[Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Member als statisch markieren", Justification = "<Ausstehend>")]
		public byte[] Encrypt(byte[] data, SecureString mutatedKey, byte[] salt) {
			if (data == null) return null;
			using (var encryptor = Aes.Create()) {
				using (var pdb = CryptoTools.DeriveBytes(mutatedKey, salt)) {
					encryptor.Key = pdb.GetBytes(32);
					encryptor.IV = pdb.GetBytes(16);
				}
				using (var ms = new MemoryStream()) {
					using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
						cs.Write(data, 0, data.Length);
					return ms.ToArray();
				}
			}
		}
		/// <summary>
		/// Mutate the key. This implementation just returns the key
		/// </summary>
		public SecureString KeyMutation(SecureString key) => key ?? throw new ArgumentNullException(nameof(key));
	}
}
