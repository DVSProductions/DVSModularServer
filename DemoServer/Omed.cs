using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Security;
namespace System.Net {
	[Export(typeof(IServer))]
	[ExportMetadata("Name", "DemoServer")]
	[ExportMetadata("BasePath", "Demo")]
	public class Omed : IServer {
		//public static string GetString(System.Security.SecureString str) {
		//	var bstr = Runtime.InteropServices.Marshal.SecureStringToBSTR(str);
		//	var s = Runtime.InteropServices.Marshal.PtrToStringBSTR(bstr);
		//	Runtime.InteropServices.Marshal.FreeBSTR(bstr);
		//	return s;
		//}
		/// <summary>
		/// This is the catchall page.
		/// It shows when a random page has been triggered that we don't actually serve. (kinda like a 404)
		/// This server doesn't actually serve any pages besides this, so this is everything you will ever get.
		/// </summary>
		public ServerFrameWork.respoMethod Catchall { get; } = (request, special) =>
				Html.BodyBuilder(Html.h1("Modular Server Demo") +
				Html.p(Html.b("This is your user Agent: ") + Html.txt(ServerFrameWork.RecursiveToString(request.UserAgent))) +
				Html.p(Html.b("This is your URL: ") + Html.txt(ServerFrameWork.RecursiveToString(request.Url))) +
				Html.p(Html.b("This is your QueryInfo: ") + Html.txt(ServerFrameWork.RecursiveToString(request.QueryString))) +
				Html.p(Html.b("Which means: ") + Html.txt(ServerFrameWork.ReadQuery(request.QueryString))) +
				Html.p(Html.b("This is your Endpoint: ") + Html.txt(ServerFrameWork.RecursiveToString(request.RemoteEndPoint))) +
				Html.p(Html.b("Which means your IP is: ") + Html.txt(request.RemoteEndPoint.Address + "and you came from port" + request.RemoteEndPoint.Port)) +
				Html.br + 
				Html.h2("Now Feck off!") + 
				Html.p("by the way, this info is not stored in any way"));// (HttpListenerRequest request, HttpListenerResponse special) => "Ello, it's me. The Catchall";
		public ServerFrameWork.errorPage ErrorPage => null;
		public Dictionary<string, ServerFrameWork.respoMethod> PathsWithResponders =>
			new Dictionary<string, ServerFrameWork.respoMethod>() {
				{ "", Catchall }
			};
		public void Init() => C.WriteLine("Demo Server Initializing");
		public void Stop() => C.WriteLine("Demo server stopping");
		public List<ICommand> AvaliableCommands => new List<ICommand>() {
			new SimpleCommand("demo", "A demo help message", (l) => C.WriteLine("Demo command Executed!")),
			new SimpleCommand("random", "Prints a random number!", (l) => C.WriteLine(new Random().Next())),
			new SimpleCommand("randLen", "Prints a random number with a given length", (l) => {
				if(l.Count == 0 || !int.TryParse(l[0], out var num))
					C.WriteLine(new Random().Next());
				else {
					var s = new Text.StringBuilder();
					var rng = new Random();
					for(var n = 0; n < num; n++)
						s.Append(rng.Next(0, 10));
					C.WriteLine(s);
				}
			}),
			new CommandContainer("question", (l) => {
				string[] a;
				if(l.Count == 0) {
					a=new []{"REE", "ok", "nah"};
					C.WriteLine($"You selected \"{a[C.Input.Prompt("Hello World", a)]}\"");
					return;
				}
				if(l.Count < 3) {
					C.WriteLineE("Question requires at least 3 arguments! see help for details");
					return;
				}
				a = new string[l.Count-1];
				for(var n = 1; n < l.Count; n++) a[n - 1] = l[n];
				C.WriteLine($"You selected \"{a[C.Input.Prompt(l[0], a)]}\"");
			},(l)=>{
				C.WriteLine("Creates a prompt and returns answer");
				C.WriteLine("If you call without parameters you get a example question");
				C.WriteLine("Usage: question option1 option2 [option3 ... optionN]");
			}),
			new CommandContainer("password", (l) => C.WriteLine($"Your password was: \"{CryptoTools.GetString(C.Input.PWPrompt("Enter your Password: "))}\""), (l) => {
				C.WriteLine("Creates a prompt and returns answer");
				C.WriteLine("If you call without parameters you get a example question");
				C.WriteLine("Usage: question option1 option2 [option3 ... optionN]");
			})
		};
	}
}
