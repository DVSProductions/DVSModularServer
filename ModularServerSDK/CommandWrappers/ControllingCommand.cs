using System.Collections.Generic;
namespace System {
	/// <summary>
	/// More advanced Command helper class that stores any type for your functions.
	/// This allows you to generate simple Lambdas / Actions that may manipulate a object and store data
	/// without having to manage this data on their own
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ControllingCommand<T> : ICommand {
		/// <summary>
		/// Storage for the object
		/// </summary>
		T obj;
		/// <summary>
		/// Controlling commands are like normal Commands with the exceptions that they 
		/// receive a reference to a object of the type <see cref="T"/> which they can manipulate freely
		/// </summary>
		/// <param name="parameters">Regular command parameters</param>
		/// <param name="toControl">reference to the object</param>
		public delegate void ctrlcommandAction(List<string> parameters, ref T toControl);
		/// <summary>
		/// our functions
		/// </summary>
		readonly ctrlcommandAction ex, he;
		public string AssignedName { get; set; }
		public string Verb { get; }
		/// <summary>
		/// Create a new <see cref="ControllingCommand{T}"/>
		/// </summary>
		/// <param name="name">Name of the Command</param>
		/// <param name="onExecute"><see cref="ctrlcommandAction"/>  to run on execution </param>
		/// <param name="onHelp"><see cref="ctrlcommandAction"/> to run on help</param>
		/// <param name="objectToControl">Reference to a instance of <see cref="T"/></param>
		public ControllingCommand(string name, ctrlcommandAction onExecute, ctrlcommandAction onHelp, ref T objectToControl) {
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Commands need a name. How do you expect them to be called otherwise?", nameof(name));
			Verb = name.ToUpperInvariant();
			if(objectToControl is object o && o == null)
				throw new ArgumentNullException(nameof(objectToControl));
			obj = objectToControl;
			ex = onExecute ?? throw new ArgumentNullException(nameof(onExecute));
			he = onHelp ?? throw new ArgumentNullException(nameof(onHelp));
		}
		public void Execute(List<string> parameters) => ex(parameters, ref obj);
		public void Help(List<string> parameters) => he(parameters, ref obj);
	}
}
