using ModularServerSDK;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Threading;
namespace StatusServer {
	[Export(typeof(IServer))]
	[ExportMetadata("Name", "StatusServer")]
	[ExportMetadata("BasePath", dir)]
	public class StatusServer : IServer {
		private const string dir = "Status";
		public Dictionary<string, ServerFrameWork.respoMethod> PathsWithResponders { get; } = new Dictionary<string, ServerFrameWork.respoMethod>();
		private readonly Dictionary<string, byte[]> fileContent = new Dictionary<string, byte[]>();
		private readonly Dictionary<string, DateTime> lastWrite = new Dictionary<string, DateTime>();
		Config cfg;
		Thread checkThread;
		private string StatusHandler(HttpListenerRequest request, HttpListenerResponse response) => Convert.ToBase64String(fileContent[request.Url.Segments[request.Url.Segments.Length]]);
		private void CheckThreadWorker() {
			while(true) {
				foreach(var f in lastWrite.Keys) {
					var p = dir + "\\" + f;
					if(File.Exists(p)) {
						var nt = File.GetLastWriteTime(p);
						var lt = lastWrite[f];
						if(nt != lt) {
							try {
								lock(fileContent) {
									fileContent[f] = File.ReadAllBytes(p);
								}
							}
							catch(Exception ex) {
								C.WriteLineI($"Cannot read Statusfile \"{f}\" because of: {ex}");
							}
							lastWrite[f] = File.GetLastWriteTime(p);
						}
					}
				}
				Thread.Sleep(cfg.UpdateInterval);
			}
		}
		private string StartPage(HttpListenerRequest request, HttpListenerResponse response) => Html.BodyBuilder(
			Html.h1("Status subserver") +
			Html.b("This server and its sub\"directories\" contain Software & Development information about my software") +
			Html.br + Html.p(
				Html.txt("These subsites are being requested by my software to let you know about updates etc..") + Html.br +
			Html.txt("Such data may be encrypted or encoded therefore trying to go any further than this info page is pretty useless.")));
		public ServerFrameWork.respoMethod Catchall => StartPage;
		public ServerFrameWork.errorPage ErrorPage => null;
		public List<ICommand> AvaliableCommands => new List<ICommand>();
		public void Init() {
			PathsWithResponders.Add("", StartPage);
			if(!Directory.Exists(dir)) Directory.CreateDirectory(dir);
			cfg = ConfigLoader.Load(dir + "\\" + Config.filename, new Config());
			foreach(var f in Directory.EnumerateFiles(dir)) {
				if(f == dir + "\\" + Config.filename) 				
					continue;
				lastWrite.Add(f, File.GetLastWriteTime(dir + "\\" + f));
				PathsWithResponders.Add(f, StatusHandler);
			}
			checkThread = new Thread(CheckThreadWorker) {
				Name = "StatusServer.CheckThreadWorker",
				Priority = ThreadPriority.BelowNormal
			};
			checkThread.Start();
		}
		public void Stop() => checkThread.Abort();
	}
}
