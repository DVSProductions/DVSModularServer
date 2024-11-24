namespace System.Security; 
/// <summary>
/// Interface that all encryption algorithms have to implement
/// </summary>
public interface IServerCrypto {
	SecureString KeyMutation(SecureString key);
	byte[] Decrypt(byte[] data, SecureString mutatedKey);
	byte[] Encrypt(byte[] data, SecureString mutatedKey);
}
