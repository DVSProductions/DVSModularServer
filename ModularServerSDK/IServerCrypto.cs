namespace System.Security {
	public interface IServerCrypto {
		SecureString KeyMutation(SecureString key);
		byte[] Decrypt(byte[] data, SecureString mutatedKey);
		byte[] Encrypt(byte[] data, SecureString mutatedKey);
	}
}
