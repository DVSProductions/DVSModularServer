using System.Runtime.InteropServices;
using System.Security.Cryptography;
namespace System.Security; 
/// <summary>
/// Contains some methods to make Cryptography easier
/// </summary>
public static class CryptoTools {
	/// <summary>
	/// Salts a Securestring with a given salt
	/// </summary>
	/// <param name="str"></param>
	/// <param name="salt"></param>
	public static Rfc2898DeriveBytes DeriveBytes(SecureString str, byte[] salt) {
		var bstr = Marshal.SecureStringToBSTR(str);
		try {
			return new Rfc2898DeriveBytes(Marshal.PtrToStringBSTR(bstr), salt, 1234567, HashAlgorithmName.SHA512);
		}
		finally {
			Marshal.FreeBSTR(bstr);
		}
	}
	/// <summary>
	/// DONT USE THIS UNLESS YOU ARE AWARE OF THE SECURITY RISKS!
	/// <para>
	/// This will remove all security features of the <see cref="SecureString"/> by converting it back to a regular <see cref="string"/>
	/// </para>
	/// This is very dangerous, because objects such as strings can remain in memory for a very long time, allowing attackers to read memory and extract 
	/// the stored password. Only use this if you REAAALLLYYYY have to. Otherwise use char by char processing of the <see cref="SecureString"/>
	/// </summary>
	/// <param name="str">Victim of security loss</param>
	/// <returns>Securityless string</returns>
	public static string GetString(this SecureString str) {
		IntPtr bstr = default;
		try {
			bstr = Marshal.SecureStringToBSTR(str);
			return Marshal.PtrToStringBSTR(bstr);
		}
		finally {
			Marshal.FreeBSTR(bstr);
		}
	}
}
