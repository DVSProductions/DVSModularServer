using System.Collections.Generic;
namespace System{
	/// <summary>
	/// Interface for all custom servers 
	/// </summary>
	public interface IServer {
		/// <summary>
		/// Use This as your actual constructor!<para>
		/// Prepare all data that you need here
		/// </para>
		/// Leave your constructor empty!
		/// </summary>
		void Init();
		/// <summary>
		/// Dictionary of "folders" and the respo methods associated with them
		/// </summary>
		Dictionary<string, ServerFrameWork.respoMethod> PathsWithResponders { get; }
		/// <summary>
		/// <see langword="null"/> if your Server ignores Bad calls.
		/// </summary>
		ServerFrameWork.respoMethod Catchall { get; }
		/// <summary>
		/// <see langword="null"/> to use Parents error page.
		/// </summary>
		ServerFrameWork.errorPage ErrorPage { get; }
		/// <summary>
		/// Called when the server is shutting down softly <para>
		/// The function should return only once ALL pending operations are complete / aborted and all resources (that need to be released) are released
		/// </para>
		/// Also all Connections, Streams and open Files should be closed
		/// </summary>
		void Stop();
		/// <summary>
		/// Returns all commands accepted by this Server <para>
		/// DON'T add your own prefixes to command names. The Modular Server System will take care of that
		/// </para>
		/// </summary>
		List<ICommand> AvaliableCommands { get; }
	}
}
