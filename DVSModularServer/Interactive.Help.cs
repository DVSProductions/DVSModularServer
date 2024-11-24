using System.Text;
namespace DVSModularServer;
internal sealed partial class Interactive {
	private class Help(Interactive parent) : ICommand {
		public string Verb => "help";
		public string AssignedName { get; set; } = string.Empty;
		public Interactive parent = parent;

		public void Execute(List<string> parameters) {
			if(parameters.Count == 0) {
				C.WriteLine("Known commands: ");
				var s = new StringBuilder();
				foreach(var k in parent.Commands.Keys)
					s.Append($"{k.ToLowerInvariant()}, ");
				s.Remove(s.Length - 2, 2);
				C.WriteLine(s.ToString());
			}
			else {
				if(!parent.Commands.TryGetValue(parameters[0].ToUpperInvariant(), out var tcom)) {
					C.WriteLineI($"Command \"{parameters[0]}\" not found.");
					Execute([]);
				}
				else {
					parameters.RemoveAt(0);
					tcom.Help(parameters);
				}
			}
		}
		void ICommand.Help(List<string> parameters) {
			if(parameters.Count == 0)
				C.WriteLine("The help Command prints the help pages for any given command or lists all commands if no Parameter was given");
			else
				Execute(parameters);
		}
	}
}
