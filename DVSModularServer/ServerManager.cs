using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
namespace DVSModularServer {
	class ServerManager : IDisposable {
		/// <summary>
		/// Server List. 
		/// Populated during runtime using reflection and composition
		/// </summary>
		[ImportMany]
#pragma warning disable IDE0044 // Readonly-Modifizierer hinzufügen
#pragma warning disable CS0649 //Never assigned. Warning! it is. Just only by the import
		IEnumerable<Lazy<IServer, IServerMeta>> lazyServerList;
#pragma warning restore CS0649
#pragma warning restore IDE0044
		/// <summary>
		/// Http Server
		/// </summary>
		private readonly HttpListener listener;

		/// <summary>
		/// internal server class for fast access to metadata
		/// </summary>
		readonly struct Server {
			/// <summary>
			/// the actual server
			/// </summary>
			public readonly IServer serv;
			/// <summary>
			/// Responders associated to this server
			/// </summary>
			public readonly SortedDictionary<string, ServerFrameWork.respoMethod> responders;
			/// <summary>
			/// Server "Directory"
			/// </summary>
			public readonly string BasePath;
			/// <summary>
			/// Name by which to address this server
			/// </summary>
			public readonly string Name;
			public Server(IServer s, IServerMeta m) {
				serv = s;
				responders = new SortedDictionary<string, ServerFrameWork.respoMethod>();
				Name = m.Name;
				BasePath = m.BasePath;
			}
		}
		/// <summary>
		/// Servers with metadata
		/// </summary>
		private readonly SortedDictionary<string, Server> Servers;
		/// <summary>
		/// all known servers and their names
		/// </summary>
		private readonly Dictionary<IServer, string> ServerNames;
		/// <summary>
		/// "https://" or "http://"
		/// </summary>
		private readonly string protocol;
		public ServerManager() {
			listener = new HttpListener();
			Servers = new SortedDictionary<string, Server>();
			ServerNames = new Dictionary<IServer, string>();
			ServerInfo.SetPort(Program.Config.Port);
			ServerInfo.Domain.Set(Program.Config.ReportBackDomain);
			protocol = Program.Config.UseHttps ? "https://" : "http://";
			ServerInfo.CreateURL.Set((they) => $"{protocol}{Program.Config.ReportBackDomain}:{Program.Config.Port}/{Servers[ServerNames[they]].BasePath}/");
			SystemEvents.SessionEnding += SystemEvents_SessionEnding;
		}
		private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e) {
			SystemEvents.SessionEnding -= SystemEvents_SessionEnding;
			C.WriteLineE($"Shutdown detected!. Reason: {e.Reason}");
			Stop();
		}
		/// <summary>
		/// Stops all servers and then destroys all resources
		/// </summary>
		public void Stop() {
			listener.Stop();
			if (Servers.Count != 0 && lazyServerList != null) {
				var enders = new Stack<System.Threading.Thread>();
				C.WriteLineI($"Sending stop message to Servers");
				foreach (var s in Servers) {
					var t = new System.Threading.Thread((a) => { try { s.Value.serv.Stop(); } catch (Exception ex) { C.WriteLineE(ex); } }) {
						Name = $"~{s.Key}"
					};
					t.Start();
					enders.Push(t);
				}
				C.WriteLineI("Waiting for servers to stop...");
				while (enders.Count != 0) {
					if (enders.Peek().ThreadState != System.Threading.ThreadState.Running)
						enders.Pop().Join();
					else System.Threading.Tasks.Task.Delay(100);
				}
				C.WriteLineI("Servers stopped!");
			}
		}
		/// <summary>
		/// Attempt to import and load all Servers
		/// </summary>
		public void Load() {
			LoadModules();
			C.WriteLine("Preparing Servers...");
			int success = 0, failed = 0;
			foreach (var s in lazyServerList) {
				try {
					if (ImportServer(s)) success++;
					else failed++;
				}
				catch (Exception ex) {
					C.WriteLineE($"Exception in \"{s.Metadata.Name}\": {ex}");
					failed++;
				}
			}
			C.WriteLine($"Servers Loaded: {success} Successfully and {failed} Failed.");
			if (success == 0) throw new DllNotFoundException("Cannot Find any servers");
			listener.Start();
		}
		/// <summary>
		/// Loads a given server and all its commands
		/// </summary>
		/// <param name="s">server to load</param>
		private bool ImportServer(Lazy<IServer, IServerMeta> s) {
			try {
				var store = new Server(s.Value, s.Metadata);
				store.serv.Init();
				var pairs = s.Value.PathsWithResponders;
				if (pairs == null || pairs.Count == 0)
					throw new MissingFieldException("Pairs are required (Is the server file damaged?)");
				foreach (var p in pairs) {
					var path = $"/{store.BasePath}{(string.IsNullOrEmpty(p.Key) ? p.Key : $"/{p.Key}")}";
					var url = $"{protocol}*:{Program.Config.Port}{path}/";
					listener.Prefixes.Add(url);
					store.responders.Add(path, p.Value);
				}
				Servers.Add(store.BasePath, store);
				ServerNames.Add(store.serv, store.BasePath);
				var i = C.Input as Interactive;
				var serverVerbName = store.BasePath.Trim().ToLowerInvariant().Replace(" ", "_");
				foreach (var comm in store.serv.AvaliableCommands) {
					if (string.IsNullOrWhiteSpace(comm.Verb)) {
						C.WriteLineE($"Command name Violation! \"{comm.Verb ?? "null"}\" is not a valid name!");
						continue;
					}
					if (comm.Verb.Contains(" "))
						C.WriteLineI($"Commands should not contain spaces: \"{comm.Verb}\"");
					if (comm.Verb.Contains("\t"))
						C.WriteLineI($"Commands should not contain tabs: \"{comm.Verb}\"");
					i.AddCommand($"{serverVerbName}.{comm.Verb.Trim().ToLowerInvariant().Replace(" ", "_").Replace("\t", "_")}", comm);
				}
				C.WriteLineS($"\tLoaded {store.Name}.");
				return true;
			}
			catch (MissingFieldException ex) {
				try {
					C.WriteLineE($"\tLoading {s.Metadata.Name} failed!: {ex}");
				}
				catch {
					C.WriteLineE($"\tLoading name of server failed. Add metadata! {ex}");
				}
				return false;
			}
		}
		/// <summary>
		/// load all encrypted servers
		/// </summary>
		private static List<Assembly> EncryptedLoader() {
			var ret = new List<Assembly>();
			C.WriteLine("Loading Encrypted Servers...");
			foreach (var module in Directory.EnumerateFiles("Servers/", "*.edll")) {
				try {
					using (var fs = new FileStream(module, FileMode.Open)) {
#pragma warning disable CA2000 //Intentionally leaving the archive open so the garbage collector doesn't kill the module
						using (var a = new System.IO.Compression.ZipArchive(fs, System.IO.Compression.ZipArchiveMode.Read)) {
#pragma warning restore CA2000 
							var files = a.Entries;
							EncryptedServerConfig esc = null;
							foreach (var file in files) {
								if (file.Name == EncryptedServerConfig.ConfigFileName) {
									using (var r = new StreamReader(file.Open())) {
										esc = EncryptedServerConfig.Load(r);
									}
								}
							}
							if (esc == null)
								throw new FileNotFoundException("Server Config file Missing in package");
							var decr = new ServerDecryptor(a, esc);
							System.Security.SecureString password;
							do {
								password = C.Input.PWPrompt($"Password for {module}: ");
								if (password.Length == 0) {
									if (C.Input.PromptYN("Do you wish to abort Decrypting this module?"))
										throw new KeyNotFoundException("Decryption was aborted.");
								}
							} while (!decr.DecryptAll(password));
							var decrypted = decr.DecryptedFiles;
							ret.Add(Assembly.Load(decrypted[esc.ServerFileName]));
						}
					}
				}
				catch (Exception ex) {
					C.WriteLineE($"Error loading encrypted module \"{module}\": {ex}");
				}
			}
			return ret;
		}
		/// <summary>
		/// Load Servers starting with encrypted ones
		/// </summary>
		private void LoadModules() {
			var lst = EncryptedLoader();
			C.WriteLine("Loading Modules...");
			using (var catalog = new AggregateCatalog()) {
				using (var pAS = new AssemblyCatalog(typeof(Program).Assembly)) {
					catalog.Catalogs.Add(pAS);
					var toDispose = new List<AssemblyCatalog>();
					foreach (var a in lst) {
						toDispose.Add(new AssemblyCatalog(a));
						catalog.Catalogs.Add(toDispose[toDispose.Count - 1]);
					}
					using (var DC = new DirectoryCatalog("Servers/", "*.dll")) {
						catalog.Catalogs.Add(DC);
#pragma warning disable CA2000 //THIS MUST NOT BE DISPOSED!
						var _container = new CompositionContainer(catalog);
#pragma warning restore CA2000
						try {
							_container.ComposeParts(this);
						}
						catch (CompositionException compositionException) {
							C.WriteLineE(compositionException.ToString());
						}
						finally {
							foreach (var ac in toDispose)
								ac.Dispose();
						}
					}
				}
			}
		}
		/// <summary>
		/// creates a simple Html error message for a given status code and message
		/// </summary>
		/// <param name="statusCode">HTML status code to show</param>
		/// <param name="msg"></param>
		static string BuildErrorPage(HttpStatusCode statusCode, string msg) =>
			Html.BodyBuilder(Html.h1($"There was an Error: {(int)statusCode} {statusCode}" + (string.IsNullOrWhiteSpace(msg) ? "" : Html.p(Html.b("Here is some additional Info: ") + msg))));
		/// <summary>
		/// Returns an error page, either provided by the server or the default one
		/// </summary>
		/// <param name="response">Source response</param>
		/// <param name="p">Server Error Page</param>
		/// <param name="statusCode">error status code</param>
		/// <param name="msg">Error Message</param>
		static string ShowErrorPage(HttpListenerResponse response, ServerFrameWork.errorPage p, HttpStatusCode statusCode, string msg) {
			response.StatusCode = (int)statusCode;
			return p != null ? p(statusCode, msg) : BuildErrorPage(statusCode, msg);
		}
		/// <summary>
		/// Show the default error page
		/// </summary>
		/// <param name="response">source response</param>
		/// <param name="statusCode">error code</param>
		/// <param name="msg">Error Message</param>
		static string ShowErrorPage(HttpListenerResponse response, HttpStatusCode statusCode, string msg) => ShowErrorPage(response, null, statusCode, msg);
		/// <summary>
		/// Gets the containing folder from an url
		/// </summary>
		/// <param name="url">Server message</param>
		private static string GetServerString(string url) {
			if (string.IsNullOrWhiteSpace(url))
				return "";
			url = url.TrimStart('/');
			var n = url.Length;
			while (n > 0 && url[--n] != '/') ;
			return n == 0 ? url : url.Substring(0, n);
		}
		/// <summary>
		/// Starts the server
		/// </summary>
		public void Run() {
			C.WriteLine("Listening...");
			try {
				while (listener.IsListening)
					ServerFrameWork.QUWI("Handler", Handler, listener.GetContext());
			}
			catch (Exception e) {
				try {
					if (listener.IsListening != false)
						C.WriteLineE(e);
				}
				catch (Exception ex) { C.WriteLineE(ex); }
			}
		}
		/// <summary>
		/// Handles a request from a client
		/// </summary>
		/// <param name="context"></param>
		private void Handler(HttpListenerContext context) {
			var pat = context.Request.Url.AbsolutePath;
			if (!Servers.TryGetValue(GetServerString(pat), out var ThisServer)) {
				var buf = Encoding.UTF8.GetBytes(ShowErrorPage(context.Response, HttpStatusCode.NotFound, ""));
				context.Response.ContentLength64 = buf.Length;
				context.Response.OutputStream.Write(buf, 0, buf.Length);
				return;
			}
			if (!ThisServer.responders.TryGetValue(pat, out var ServerMethod)) {
				ServerMethod = ThisServer.serv.Catchall ?? ((a, b) => ShowErrorPage(b, ThisServer.serv.ErrorPage, HttpStatusCode.NotFound, ""));
			}
			try {
				string msg;
				try {
					msg = ServerMethod(context.Request, context.Response);
				}
				catch (Exception ex) {
					msg = ShowErrorPage(context.Response, ThisServer.serv.ErrorPage, HttpStatusCode.InternalServerError, ex.Message);
				}
				if(msg != null) {
					var buf = Encoding.UTF8.GetBytes(msg);
					context.Response.ContentLength64 = buf.LongLength;
					context.Response.OutputStream.Write(buf, 0, buf.Length);
				}
			}
			catch {
				context.Response.StatusCode = 500;
				try {
					context.Response.Close();
					context = null;
				}
				catch { }
			}
			finally {
				// always close the stream
				context?.Response?.OutputStream?.Close();
			}
		}
		public void Dispose() => (listener as IDisposable).Dispose();
	}
}
