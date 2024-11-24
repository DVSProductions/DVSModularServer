namespace DVSModularServer;
internal class Program {
	public static ServerManager? man = null;
	/// <summary>
	/// Public access to the configuration file
	/// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	public static Config Config { get; private protected set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	/// <summary>
	/// Was the server stopping intentional?
	/// </summary>
	public static bool cleanexit = false;
	/// <summary>
	/// Stop everything that is currently running?
	/// </summary>
	public static bool stop = false;
	public static void CreateServerFolder() {
		if(!Directory.Exists("Servers"))
			Directory.CreateDirectory("Servers");
	}
	/// <summary>
	/// Ensure that another SDK version does not exsist locally
	/// </summary>
	private static void DestroySDK() {
		var target = "Servers/ModularServerSDK.dll";
		if(File.Exists(target)) {
			C.WriteLineE("Today is not bring your own functions to work day!");
			try {
				File.Delete(target);
			}
			catch {
				C.WriteLineE("Could not Remove SDK. Stopping...");
				Environment.Exit(1);
			}
		}
	}
	/// <summary>
	/// Create SDK dll from resources
	/// </summary>
	private static void ExtractDLL() => File.WriteAllBytes("ModularServerSDK.dll", Properties.Resources.SDKDLL);
	/// <summary>
	/// Shows a simple Yes/No prompt with a given message.
	/// Works without <see cref="C"/>
	/// </summary>
	/// <returns><see langword="true"/>or <see langword="false"/></returns>
	private static bool Prompt(string msg) {
		var c = new ConsoleKeyInfo(' ', ConsoleKey.Escape, false, false, false);
		try {
			while(true) {
				Console.Write($"{msg}(y/n)> ");
				c = Console.ReadKey(false);
				if(c.KeyChar is 'y' or 'Y') return true;
				else if(c.KeyChar is 'n' or 'N') return false;
			}
		}
		catch {
			return false;
		}
		finally {
			Console.WriteLine(c.KeyChar);
		}
	}
	private static void Main(string[] args) {
		Thread.CurrentThread.Name = "Main";
#if !DEBUG
		AskExtractDLL();
#endif
		Serv();
		///get rid of sdk after we are done
#if !DEBUG
		try {
			var n = new System.Diagnostics.ProcessStartInfo("cmd.exe", $"/Q /C timeout -t 1 >nul &&del /f ModularServerSDK.dll") {
				CreateNoWindow = true,
				UseShellExecute = true,
				WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
			};
			System.Diagnostics.Process.Start(n);
		}
		catch { }
#endif
	}
	/// <summary>
	/// Try to extract the lastest SDL dll
	/// </summary>
	private static void AskExtractDLL() {
		bool again;
		do {
			again = false;
			try {
				ExtractDLL();
			}
			catch(Exception ex) {
				Console.WriteLine("ERROR COULD NOT EXRACT DLL!!: " + ex.ToString());
				again = Prompt("Do you wish to try again?");
			}
		} while(again);
	}
	/// <summary>
	/// Required so we can get our own dll beforehand
	/// </summary>
	private static void Serv() {
		C.ResetColor();
		C.createTitle.Call("DVSPRODUCTIONS ModularServer");
		var (shellThread, cancelToken) = RunInteractiveShell();
		Config = Config.Load();
		var stopLog = L.Init(Config.Logfile);
		CreateServerFolder();
		DestroySDK();
		RunServerManager();
		if(!stop)
			shellThread.Join();
		else {
			cancelToken.Cancel();
			//interactive.Abort();
		}
		if(!cleanexit) {
			C.FillLineC('-', null, ConsoleColor.DarkGray);
			C.WriteLineE("Crashed");
		}
		else
			C.WriteLineS("Has exited");
		stopLog();
		Console.Write("Press any key to exit...");
		Console.ReadKey(true);
		Console.WriteLine();
	}
	/// <summary>
	/// Launches the Interactive Shell Thread and returns it
	/// </summary>
	private static (Thread thread,CancellationTokenSource cancelToken) RunInteractiveShell() {
		var source = new CancellationTokenSource();
		var t = new Thread(new Interactive(source).InteractiveThread) {
			Priority = ThreadPriority.BelowNormal,
			Name = "I"
		};
		t.Start();
		return (t,source);
	}
	/// <summary>
	/// Launches the server manager and waits for it to shut down
	/// </summary>
	private static void RunServerManager() {
		try {
			man = new ServerManager(); //Composition is performed in the constructor
		}
		catch(Exception ex) {
			C.WriteLineE($"FATAL COMPOSITION EXCEPTION!: {ex}");
			stop = true;
			return;
		}
		try {
			man.Load();
		}
		catch(Exception e) {
			C.WriteLineE($"Load Encountered a fatal error: {e}");
			stop = true;
			return;
		}
		var t = new Thread(man.Run) {
			Priority = ThreadPriority.Highest,
			Name = "SerMan"
		};
		t.Start();
		while((t.IsAlive || !cleanexit) && !stop)
			Task.Delay(1000).Wait();
		if(cleanexit) man.Stop();
		stop = true;
		t.Join();
	}
}
