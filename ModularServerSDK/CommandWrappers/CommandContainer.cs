using System.Collections.Generic;
namespace System; 
/// <summary>
/// A class that will hold your commands <para>
/// allows you to avoid creating your own class</para>
/// Similar to the <see cref="SimpleCommand"/> but allows for custom help handling
/// </summary>
public class CommandContainer : ICommand {
	/// <summary>
	/// Actions
	/// </summary>
	private readonly Action<List<string>> ex, he;
	public string Verb { get; }
	public string AssignedName { get; set; } = string.Empty;
	/// <summary>
	/// Generates a new Instance
	/// </summary>
	/// <param name="name">Name of the command</param>
	/// <param name="onRun">Action to run on call</param>
	/// <param name="onHelp">Action to run on help</param>
	public CommandContainer(string name, Action<List<string>> onRun, Action<List<string>> onHelp) {
		if(string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("A command needs a name", nameof(name));
		Verb = name.ToUpperInvariant();
		ex = onRun ?? throw new ArgumentNullException(nameof(onRun));
		he = onHelp ?? throw new ArgumentNullException(nameof(onHelp));
	}
	public void Execute(List<string> parameters) => ex(parameters);
	public void Help(List<string> parameters) => he(parameters);
}
