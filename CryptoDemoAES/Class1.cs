using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
namespace CryptoDemoAES;
[Export(typeof(IServerCrypto))]
public class Class1 : IServerCrypto {
	/// <summary>
	/// example salt
	/// </summary>
	private static readonly byte[] salt = [102, 47, 130, 95, 103, 207, 197, 222, 30, 36, 210, 92, 254, 61, 55, 173, 118, 15, 57, 15, 200, 118, 224, 203, 218, 160, 178, 183, 180, 112,];
	/// <summary>
	/// demo encryption method
	/// </summary>
	public byte[] Decrypt(byte[] data, SecureString mutatedKey) {
		using var encryptor = Aes.Create();
		using(var pdb = CryptoTools.DeriveBytes(mutatedKey, salt)) {
			encryptor.Key = pdb.GetBytes(32);
			encryptor.IV = pdb.GetBytes(16);
		}
		using var ms = new MemoryStream();
		using(var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
			cs.Write(data, 0, data.Length);
		return ms.ToArray();
	}
	/// <summary>
	/// demo encryption method
	/// </summary>
	public byte[] Encrypt(byte[] data, SecureString mutatedKey) {
		using var encryptor = Aes.Create();
		using(var pdb = CryptoTools.DeriveBytes(mutatedKey, salt)) {
			encryptor.Key = pdb.GetBytes(32);
			encryptor.IV = pdb.GetBytes(16);
		}
		using var ms = new MemoryStream();
		using(var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
			cs.Write(data, 0, data.Length);
		return ms.ToArray();
	}
	/// <summary>
	/// Demo KEy mutation algorithm
	/// </summary>
	public SecureString KeyMutation(SecureString key) {
		ArgumentNullException.ThrowIfNull(key);
		var s = new SecureString();
		var bstr = Marshal.SecureStringToBSTR(key);
		UInt16 b;
		var i = 0;
		do {
			b = (UInt16)Marshal.ReadInt16(bstr, i);
			s.AppendChar((char)(UInt16.MaxValue - b));
			i += 2;
		} while(b != '\0');
		Marshal.ZeroFreeBSTR(bstr);
		return key;
	}
}
