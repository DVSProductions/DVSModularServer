using System.Text;
namespace System.Security {
	/// <summary>
	/// This class wrappes a <see cref="IServerCrypto"/> crypto provider and exposes easy to use functions for it.
	/// </summary>
	/// <typeparam name="TCryptoProvider"></typeparam>
	public sealed class CryptoWrapper<TCryptoProvider> : IServerCrypto where TCryptoProvider : IServerCrypto {
		readonly TCryptoProvider CryptoProvider;
		public CryptoWrapper(TCryptoProvider provider) {
			if (provider.GetType().IsGenericType && provider.GetType().GetGenericTypeDefinition() == typeof(CryptoWrapper<>)) {
				throw new ArgumentException("A Crypto Wrapper must not contain a Crypto Wrapper!");
			}
			CryptoProvider = provider;
		}
		/// <summary>
		/// Decrypts a encrypted byte array
		/// </summary>
		public byte[] Decrypt(byte[] data, SecureString mutatedKey) =>
			CryptoProvider.Decrypt(data, mutatedKey);
		/// <summary>
		/// Decrypts a Base64String that contains encrypted data
		/// (don't worry we do the Base64 decoding for you <3)
		/// </summary>
		public byte[] DecryptB64(string b64Data, SecureString mutatedKey) =>
			Decrypt(Convert.FromBase64String(b64Data), mutatedKey);
		/// <summary>
		/// Encrypt a byte array
		/// </summary>
		public byte[] Encrypt(byte[] data, SecureString mutatedKey) =>
			CryptoProvider.Encrypt(data, mutatedKey);
		/// <summary>
		/// Encrypt a Unicode String
		/// (Warning: the string will be in Unicode format. So if you decrypt it you have to Convert it from Bytes to Unicode)
		/// </summary>
		public byte[] Encrypt(string data, SecureString mutatedKey) =>
			Encrypt(Encoding.Unicode.GetBytes(data), mutatedKey);
		/// <summary>
		/// Encrypt a byte array and store the result in a Base64 string
		/// </summary>
		public string EncryptB64(byte[] data, SecureString mutatedKey) =>
			Convert.ToBase64String(Encrypt(data, mutatedKey));
		/// <summary>
		/// Execute the key mutation on a key
		/// </summary>
		/// <param name="key"></param>
		public SecureString KeyMutation(SecureString key) => CryptoProvider.KeyMutation(key);
		/// <summary>
		/// Executes the keyMutation using a regular string. 
		/// Obviously returns a <see cref="SecureString"/>, because all crypto methods use <see cref="SecureString"/>s.
		/// <para>
		/// Please validate the usecase of this method before using it!
		/// Do you really need the key as a unsafe string?
		/// Are you leaking information by potentially exposing keys?
		/// </para>
		/// </summary>
		/// <param name="key"></param>
		public SecureString KeyMutation(string key) {
			if (string.IsNullOrEmpty(key)) return default;
			var ss = new SecureString();
			try {
				foreach (var c in key) ss.AppendChar(c);
				return CryptoProvider.KeyMutation(ss);
			}
			finally {
				ss.Dispose();
			}
		}
	}
}
