using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
namespace DVSModularServer {
	sealed partial class Interactive {
		class Help : ICommand {
			public string Verb => "help";
			public string AssignedName { get; set; }
			public Interactive parent;
			public Help(Interactive parent) => this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
			public void Execute(List<string> parameters) {
				if(parameters == null || parameters.Count == 0) {
					C.WriteLine("Known commands: ");
					var s = new StringBuilder();
					foreach(var k in parent.Commands.Keys)
						s.Append($"{k}, ");
					s.Remove(s.Length - 2, 2);
					C.WriteLine(s.ToString());
				}
				else {
					if(!parent.Commands.TryGetValue(parameters[0], out var tcom)) {
						C.WriteLineI($"Command \"{parameters[0]}\" not found.");
						Execute(null);
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
}
