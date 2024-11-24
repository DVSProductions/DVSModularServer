using ModularServerSDK;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Threading;
namespace StatusServer; 
[Export(typeof(IServer))]
[ExportMetadata("Name", "StatusServer")]
[ExportMetadata("BasePath", dir)]
public class StatusServer : IServer {
	private const string dir = "Status";
	public Dictionary<string, ServerFrameWork.respoMethod> PathsWithResponders { get; } = [];
	private readonly Dictionary<string, byte[]> fileContent = [];
	private readonly Dictionary<string, DateTime> lastWrite = [];
	[AllowNull]
	private Config cfg;
	[AllowNull]
	private Thread checkThread;
	[AllowNull]
	private CancellationTokenSource cts;
	private string StatusHandler(HttpListenerRequest request, HttpListenerResponse response) {
		if(request.Url != null) {
			var segment = request.Url.Segments.LastOrDefault();
			if(segment != null && fileContent.TryGetValue(segment, out var contentArray)) 
				return Convert.ToBase64String(contentArray);
		}
		response.StatusCode = (int)HttpStatusCode.NotFound;
		return string.Empty;
	}

	private void CheckThreadWorker() {
		while(!cts.Token.IsCancellationRequested) {
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
			cts.Token.WaitHandle.WaitOne(cfg.UpdateInterval);
		}
	}
	private string StartPage(HttpListenerRequest request, HttpListenerResponse response) => Html.BodyBuilder(
		Html.h1("Status subserver") +
		Html.b("This server and its sub\"directories\" contain Software & Development information about my software") +
		Html.br + Html.p(
			Html.txt("These subsites are being requested by my software to let you know about updates etc..") + Html.br +
		Html.txt("Such data may be encrypted or encoded therefore trying to go any further than this info page is pretty useless.")));
	public ServerFrameWork.respoMethod? Catchall => StartPage;
	public ServerFrameWork.errorPage? ErrorPage => null;
	public List<ICommand> AvaliableCommands => [];
	public void Init() {
		PathsWithResponders.Add("", StartPage);
		if(!Directory.Exists(dir)) Directory.CreateDirectory(dir);
		cfg = ConfigLoader.Load(dir + "\\" + Config.filename, new Config());
		cts = new CancellationTokenSource();
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
	public void Stop() => cts.Cancel();
}
