using System.Collections.Generic;
namespace System {
	/// <summary>
	/// Interface for generating your own commands to be used in the interactive 
	/// command prompt
	/// </summary>
	public interface ICommand {
		/// <summary>
		/// Don't set this yourself. The Interactive shell will assign you one!
		/// </summary>
		string AssignedName { get; set; }
		/// <summary>
		/// Name of your command used to call it
		/// </summary>
		string Verb { get; }
		/// <summary>
		/// Print a Help page about yourself!
		/// </summary>
		/// <param name="parameters">Extend your help by allowing additional help for specific subcommands</param>
		void Help(List<string> parameters);
		/// <summary>
		/// Run your command with any amount of parameters
		/// </summary>
		/// <param name="parameters">start parameters of your command</param>
		void Execute(List<string> parameters);
	}
}
