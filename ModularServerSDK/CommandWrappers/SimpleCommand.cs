using System.Collections.Generic;

namespace System {
	/// <summary>
	/// A premade command class so you don't have to make them yourself
	/// <para>
	/// The intention of a <see cref="SimpleCommand"/> is to be a action that requires few to no parameters,
	/// which can be easily described using one help message.
	/// </para>
	/// </summary>
	public class SimpleCommand : ICommand {
		public string AssignedName { get; set; }
		public string Verb { get; }
		/// <summary>
		/// Help message
		/// </summary>
		readonly string msg;
		/// <summary>
		/// on Run Action
		/// </summary>
		readonly Action<List<string>> a;
		/// <summary>
		/// Create a simple Command. Enter the alias of the command, the help message and what should be done upon exceution
		/// </summary>
		/// <param name="verb">The name of your command. as an example: echo</param>
		/// <param name="helpmsg">the message which to display when the user uses the help command. lines are \n terminated</param>
		/// <param name="comm">The actual command that gets executed.</param>
		public SimpleCommand(string verb, string helpmsg, Action<List<string>> comm) {
			if (string.IsNullOrWhiteSpace(helpmsg))
				throw new ArgumentException("Helpmsg Cannot be empty. Tell your user about yourself!", nameof(helpmsg));
			if (string.IsNullOrWhiteSpace(verb))
				throw new ArgumentNullException(nameof(verb), "Commands need a name. How do you expect them to be called otherwise?");
			Verb = verb.ToUpperInvariant();
			a = comm ?? throw new ArgumentNullException(nameof(comm), "You need to provide a command");
			msg = helpmsg;
		}
		public void Execute(List<string> parameters) => a(parameters);
		public void Help(List<string> parameters) {
			foreach (var s in msg.Split('\n'))
				C.WriteLine($"{s}");
		}
	}
}
