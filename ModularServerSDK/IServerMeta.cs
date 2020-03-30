namespace System {
	/// <summary>
	/// All servers need to implement metadata
	/// </summary>
	public interface IServerMeta {
		/// <summary>
		/// Mock foldername for the server
		/// </summary>
		string BasePath { get; }
		/// <summary>
		/// Name of the sever just to name it.
		/// </summary>
		string Name { get; }
	}
}
