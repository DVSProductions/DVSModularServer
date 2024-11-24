using System.Diagnostics;
using System.Text;
namespace DVSModularServer;
internal sealed partial class Interactive : IInteractive {
	private bool printed = false, queryModeActive = false, InCommand = false;
	private readonly int prefixLength = prefix.Length;
	private int historyIndex = -1, typedIndex = 0;
	public int CursorX => prefixLength + typedIndex;

	/// <summary>
	/// Default console prompt symbol
	/// </summary>
	private const string prefix = "> ";
	private const string backspace = "\b \b";
	private string? hiddenText = null;
	private string typedText = "";
	private static readonly HashSet<char> valid =
		[
			.. "abcdefghijklmnopqrstuvwxyz" +
					"ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
					"0123456789 " +
					"!?$@%&€#/|\\=(){}[]<>:;,._-~+\'\"*°^´`ÄÖÜäöüß",
		];
	private readonly List<string> commandHistory;
	private readonly Stopwatch watch;
	private readonly Action deleteSelf;
	private readonly SortedDictionary<string, ICommand> Commands;
	private readonly CancellationToken cancellationToken;
	private Action<ConsoleKeyInfo>? onQueryModeKeyPress;
	public Func<string>? getPromptText;
	/// <summary>
	/// Play the beep via the console interface
	/// </summary>
	public static void Beep() => ServerFrameWork.QUWI("Beep", Console.Beep);
	/// <summary>
	/// play the notification sound
	/// </summary>
	public static void Notify() => ServerFrameWork.QUWI("Beep", () => Console.Write((byte)7));
	public string GetTypedText() { printed = true; return queryModeActive ? getPromptText?.Invoke() ?? string.Empty : prefix + typedText; }
	/// <summary>
	/// internal echo command. prints all list elements
	/// </summary>
	/// <param name="l">list to be concatenated with strings</param>
	private void Echo(List<string> l) {
		var s = new StringBuilder();
		foreach(var k in l)
			s.Append(k).Append(' ');
		C.WriteLine(s);
	}
	/// <summary>
	/// Adds a command to the command registry
	/// </summary>
	/// <param name="name">name of the command</param>
	/// <param name="command">Command to be stored</param>
	public void AddCommand(string name, ICommand command) {
		command.AssignedName = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("A command needs a name!", nameof(name)) : name.ToLower();
		Commands.Add(name.ToUpperInvariant(), command);
	}
	/// <summary>
	/// A simple tool to make a inject a <see cref="SimpleCommand"/> into the command list
	/// </summary>
	/// <param name="name">name of the simple command</param>
	/// <param name="command">Actual action of the command</param>
	/// <param name="help">help message</param>
	public void CreateSimpleCommand(string name, Action<List<string>> command, string help) => AddCommand(name, new SimpleCommand(name, help, command));
	/// <summary>
	/// Creates the shell and prepares the internal commands
	/// </summary>
	public Interactive(CancellationTokenSource source) {
		commandHistory = [];
		Commands = [];
		watch = Stopwatch.StartNew();
		CreateSimpleCommand("echo", Echo, "Prints a string");
		CreateSimpleCommand("uptime", (l) => C.WriteLine($"Uptime: {watch.Elapsed}"), "Outputs the current uptime");
		CreateSimpleCommand("exit", (l) => { Program.cleanexit = true; source.Cancel(); }, "Stops the server softly");
		CreateSimpleCommand("ping", (l) => C.WriteLine("Pong!"), "You know what it does");
		AddCommand("help", new Help(this));
		deleteSelf = C.SetInput(this);
		this.cancellationToken = source.Token;
	}

	/// <summary>
	/// Checks if c is a valid char
	/// </summary>
	/// <param name="c">char to validate</param>
	private static bool IsValidChar(char c) => valid.Contains(c);
	/// <summary>
	/// Splits the command into command and Arguments
	/// </summary>
	/// <param name="s">prompt input</param>
	static public (string? Command, List<string> Arguments) SplitCommand(string s) {
		string? command = null;
		var ret = new List<string>();
		var escape = false;
		var start = 0;
		void add(int target) {
			var sub = s.Substring(start, target).TrimStart('\"').TrimEnd('\"');
			if(!string.IsNullOrWhiteSpace(sub))
				if(command == null)
					command = sub;
				else
					ret.Add(sub);
		}
		for(var n = 0; n < s.Length; n++) {
			if(!escape && s[n] == ' ') {
				add(n - start);
				start = n + 1;
			}
			else if(s[n] == '\"')
				escape = !escape;
		}
		add(s.Length - start);
		return (command, ret);
	}
	/// <summary>
	/// generates a string that clears the current prompt text and writes it
	/// </summary>
	/// <param name="pref">prefix. the prompt before the data</param>
	/// <param name="str">current string in the prompt</param>
	private static void HideStr(string pref, string str) {
		if(string.IsNullOrEmpty(str))
			return;
		var s = new StringBuilder("\r");
		s.Append(pref);
		foreach(var _ in str)
			s.Append(' ');
		s.Append(" \r");
		s.Append(pref);
		Console.Write(s);
	}
	/// <summary>
	/// Hides the date in <see cref="typedText"/>
	/// </summary>
	private void HideTypedText() => HideStr(prefix, typedText);
	/// <summary>
	/// The main interactive logic
	/// </summary>
	public void InteractiveThread() {
		if(queryModeActive) {
			while(queryModeActive) {
				while(!Console.KeyAvailable) {
					Thread.Sleep(50);
					cancellationToken.ThrowIfCancellationRequested();
				}
				QueryMode(Console.ReadKey(true));
			}
			return;
		}
		try {
			while(!Program.stop) {
				while(!Console.KeyAvailable) {
					Thread.Sleep(50);
					cancellationToken.ThrowIfCancellationRequested();
				}
				var k = Console.ReadKey(true);
				if(queryModeActive)
					QueryMode(k);
				else
					CommandInputMode(k, CommandModeExecute);
			}
		}
		catch(ThreadAbortException) {
			deleteSelf();
			Program.stop = true;
		}
		catch(OperationCanceledException) {
			deleteSelf();
			Program.stop = true;
		}
		catch(Exception ex) {
			Debug.WriteLine(ex.ToString());
			throw;
		}
	}
	/// <summary>
	/// Input handler for commands
	/// </summary>
	/// <param name="k">Pressed Key</param>
	/// <param name="onEnter">What happens when enter is pressed?</param>
	/// <param name="SpecialCaseHandler">Add Custom keyhandlers here. Will always be called</param>
	private void CommandInputMode(ConsoleKeyInfo k, Action onEnter, Action<ConsoleKeyInfo>? SpecialCaseHandler = null) {
		var ch = k.KeyChar;
		switch(k.Key) {
			case ConsoleKey.Enter: {
					onEnter();
					break;
				}
			case ConsoleKey.Backspace:
				if(typedText.Length != 0) {
					typedText = typedText.Remove(typedIndex - 1, 1);
					typedIndex--;
					if(typedIndex != typedText.Length) {
						HideTypedText();
						Console.Write(typedText);
						Console.CursorLeft = CursorX;
					}
					else
						Console.Write(backspace);
				}
				else
					Beep();
				break;
			case ConsoleKey.Delete:
				if(typedIndex != typedText.Length && typedText.Length != 0) {
					typedText = typedText.Remove(typedIndex, 1);
					HideTypedText();
					Console.Write(typedText);
					Console.CursorLeft = CursorX;
				}
				else
					Beep();
				break;
			case ConsoleKey.UpArrow:
				if((historyIndex <= 0 && hiddenText != null) || commandHistory.Count == 0)
					Beep();
				else {
					if(hiddenText == null)
						hiddenText = typedText;
					else
						historyIndex--;
					while(historyIndex >= commandHistory.Count)
						historyIndex--;
					HideTypedText();
					typedText = commandHistory[historyIndex];
					typedIndex = typedText.Length;
					Console.Write(typedText);
				}
				break;
			case ConsoleKey.DownArrow:
				if(historyIndex == commandHistory.Count && hiddenText != null) {
					HideTypedText();
					typedText = hiddenText;
					hiddenText = null;
					Console.Write(typedText);
				}
				else if(historyIndex >= commandHistory.Count || commandHistory.Count == 0)
					Beep();
				else {
					if(hiddenText == null)
						hiddenText = typedText;
					else
						historyIndex++;
					HideTypedText();
					if(historyIndex == commandHistory.Count) {
						typedText = hiddenText;
						hiddenText = null;
					}
					else
						typedText = commandHistory[historyIndex];
					typedIndex = typedText.Length;
					Console.Write(typedText);
				}
				break;
			case ConsoleKey.Escape:
				if(hiddenText != null) {
					HideTypedText();
					typedText = hiddenText;
					typedIndex = typedText.Length;
				}
				else {
					HideTypedText();
					typedText = "";
					typedIndex = 0;
				}
				break;
			case ConsoleKey.LeftArrow:
				if(typedIndex == 0)
					Beep();
				else {
					typedIndex--;
					Console.CursorLeft = CursorX;
				}
				break;
			case ConsoleKey.RightArrow:
				if(typedIndex >= typedText.Length)
					Beep();
				else {
					typedIndex++;
					Console.CursorLeft = CursorX;
				}
				break;
			default:
				if(!IsValidChar(ch))
					Beep();
				else {
					if(hiddenText != null)
						hiddenText = null;
					if(typedIndex != typedText.Length)
						HideTypedText();
					historyIndex = commandHistory.Count;
					typedText = typedText.Insert(typedIndex, "" + ch);
					typedIndex++;
					if(typedIndex == typedText.Length)
						Console.Write(ch);
					else {
						Console.Write(typedText);
						Console.CursorLeft = CursorX;
					}
				}
				break;
		}
		SpecialCaseHandler?.Invoke(k);
	}

	/// <summary>
	/// Special Type of function that gets called when no recognized special key is beeing pressed
	/// </summary>
	/// <param name="prompt">current prompt</param>
	/// <param name="typedText">reference to the current text(in order to manipulate it)</param>
	/// <param name="typedIndex">Cursor position</param>
	/// <param name="k">pressed key</param>
	private delegate void KeydownAction(string prompt, ref string typedText, ref int typedIndex, ConsoleKeyInfo k);
	/// <summary>
	/// Default keydownAction. Prints the char if it's valid and adds it to the typedtext
	/// </summary>
	private static void DefaultKeydown(string prompt, ref string typedText, ref int typedIndex, ConsoleKeyInfo k) {
		if(IsValidChar(k.KeyChar)) {
			if(typedIndex != typedText.Length)
				HideStr(prompt, typedText);
			typedText = typedText.Insert(typedIndex, "" + k.KeyChar);
			typedIndex++;
			if(typedIndex == typedText.Length)
				Console.Write(k.KeyChar);
			else {
				Console.Write(typedText);
				Console.CursorLeft = prompt.Length + typedIndex;
			}
		}
	}
	/// <summary>
	/// TextInputMode is for accepting general text using the <see cref="KeydownAction"/> delegate
	/// </summary>
	/// <param name="k">pressed key</param>
	/// <param name="prompt">current prompt</param>
	/// <param name="typedText">previously typed text</param>
	/// <param name="typedIndex">cursorindex</param>
	/// <param name="onEnter"></param>
	/// <param name="keydown">Custom keydown event</param>
	/// <param name="SpecialCaseHandler">Add Custom keyhandlers here. Will always be called</param>
	private static void TextInputMode(ConsoleKeyInfo k, string prompt, ref string typedText, ref int typedIndex, Action<string> onEnter, KeydownAction keydown, Action<ConsoleKeyInfo>? SpecialCaseHandler = null) {
		var ch = k.KeyChar;
		switch(k.Key) {
			case ConsoleKey.Enter: {
					onEnter(typedText);
					break;
				}
			case ConsoleKey.Backspace:
				if(typedText.Length != 0) {
					typedText = typedText.Remove(typedIndex - 1, 1);
					typedIndex--;
					if(typedIndex != typedText.Length) {
						HideStr(prompt, typedText);
						Console.Write(typedText);
						Console.CursorLeft = prompt.Length + typedIndex;
					}
					else
						Console.Write(backspace);
				}
				else
					Beep();
				break;
			case ConsoleKey.Delete:
				if(typedIndex != typedText.Length && typedText.Length != 0) {
					typedText = typedText.Remove(typedIndex, 1);
					HideStr(prompt, typedText);
					Console.Write(typedText);
					Console.CursorLeft = prompt.Length + typedIndex;
				}
				else
					Beep();
				break;
			case ConsoleKey.Escape:
				HideStr(prompt, typedText);
				typedText = "";
				typedIndex = 0;
				break;
			case ConsoleKey.LeftArrow:
				if(typedIndex == 0)
					Beep();
				else {
					typedIndex--;
					Console.CursorLeft = prompt.Length + typedIndex;
				}
				break;
			case ConsoleKey.RightArrow:
				if(typedIndex >= typedText.Length)
					Beep();
				else {
					typedIndex++;
					Console.CursorLeft = prompt.Length + typedIndex;
				}
				break;
			default:
				keydown(prompt, ref typedText, ref typedIndex, k);
				break;
		}
		SpecialCaseHandler?.Invoke(k);
	}
	/// <summary>
	/// Prepares textmode to be used
	/// </summary>
	/// <param name="prompt"></param>
	/// <param name="onEnter">What happens when enter is pressed?</param>
	/// <param name="keydown">Custom keydown event. <see cref="Interactive.DefaultKeydown(string, ref string, ref int, ConsoleKeyInfo)"/> is used if no keydown is specified(null)</param>
	/// <param name="SpecialCaseHandler"></param>
	private void TextModeWrapper(string prompt, Action<string> onEnter, KeydownAction? keydown = null, Action<ConsoleKeyInfo>? SpecialCaseHandler = null) {
		var tT = "";
		getPromptText = () => prompt + tT;
		var typedIndex = 0;
		keydown ??= DefaultKeydown;
		onQueryModeKeyPress = (k) => TextInputMode(k, prompt, ref tT, ref typedIndex, onEnter, keydown, SpecialCaseHandler);
	}
	/// <summary>
	/// Switches the input into Password mode.
	/// <para>
	/// This means typed characters won't be visible but instead substituted by <paramref name="disguiseChar"/>.</para>
	/// Passwords won't be stored in memory by the password mode, instead a <paramref name="storeChar"/> function 
	/// has to be specified, which should store them safely and only as temporarily as possible. 
	/// Adding the char to a <see cref="System.Security.SecureString"/> is a good implementation.
	/// </summary>
	/// <param name="prompt">current prompt</param>
	/// <param name="onEnter">What happens on enter press?</param>
	/// <param name="storeChar">Char storage function. Try to avoid <see cref="string"/>s</param>
	/// <param name="disguiseChar">the char to display instead of the actual character. defaults to *</param>
	/// <param name="keydown">Custom keydown event. A secure default implementation is used, if no custom keydown is specified(null)</param>
	/// <param name="SpecialCaseHandler"></param>
	private void PasswordModeWrapper(string prompt, Action<string> onEnter, Action<char> storeChar, char disguiseChar = '*', KeydownAction? keydown = null, Action<ConsoleKeyInfo>? SpecialCaseHandler = null) {
		keydown ??= (string p, ref string tT, ref int tI, ConsoleKeyInfo k) => {
			if(tI != tT.Length)
				HideStr(p, tT);
			tT = tT.Insert(tI, disguiseChar.ToString());
			tI++;
			if(tI == tT.Length)
				Console.Write(disguiseChar.ToString());
			else {
				Console.Write(tT);
				Console.CursorLeft = p.Length + tI;
			}
			storeChar?.Invoke(k.KeyChar);
		};
		TextModeWrapper(prompt, onEnter, keydown, SpecialCaseHandler);
	}
	/// <summary>
	/// Locates and runs the current command in typedText. also prints an error message if no command was found
	/// </summary>
	private void CommandModeExecute() {
		var (command, arguments) = SplitCommand(typedText);
		if(command == null)
			Beep();
		else if(!Commands.TryGetValue(command.ToUpperInvariant(), out var commandFunc))
			C.WriteLineE($"Command \"{command}\" not found. Type \"help\" for a list of all commands");
		else {
			try {
				InCommand = true;
				commandHistory.Add(typedText);
				printed = false;
				Console.WriteLine();
				typedText = "";
				typedIndex = 0;
				commandFunc.Execute(arguments);
				if(!printed)
					Console.Write(prefix);
				InCommand = false;
				while(queryModeActive)
					Thread.Sleep(100);
			}
			catch(Exception ex) {
				if(ex is not ThreadAbortException)
					C.WriteLineE($"{command} encountered a fatal error: {ex}");
			}
		}
	}

	//private object k;
	/// <summary>
	/// Handles Keypresses in Query Mode
	/// </summary>
	/// <param name="k"></param>
	private void QueryMode(ConsoleKeyInfo k) {
		try {
			onQueryModeKeyPress?.Invoke(k);
		}
		catch(Exception ex) {
			C.WriteLineE("ERROR" + ex.ToString());
		}
	}
	/// <summary>
	/// throws an exception if <paramref name="msg"/> is empty.
	/// <para>
	/// This is because it will throw the exception into the callers code, thereby causing a fail-fast moment in the infringing server. 
	/// Ensures that mistakes during development get caught ASAP
	/// </para> 
	/// </summary>
	/// <param name="msg">prompt message</param>
	private static void PreparePrompt(string msg) {
		if(string.IsNullOrWhiteSpace(msg))
			throw new ArgumentException("Message cannot be empty", nameof(msg));
	}
	/// <summary>
	/// Ensures Security and stability of our the prompt call.
	/// Guarantees that <paramref name="msg"/> contains something sensible and that at least two <paramref name="options"/> are given. (a prompt without options doesn't make any sense)
	///<para>
	///This is because it will throw the exception into the callers code, thereby causing a fail-fast moment in the infringing server.
	/// Ensures that mistakes during development get caught ASAP
	/// </para> 
	/// </summary>
	private void PreparePrompt<T>(string msg, IList<T> options) {
		PreparePrompt(msg);
		if(options == null || options.Count < 2)
			throw new ArgumentNullException(nameof(options));
		while(queryModeActive)
			Task.Delay(500).Wait();
	}
	/// <summary>
	/// Shorthand for a few prompt setup tasks
	/// </summary>
	private void PostPreparePrompt() {
		printed = false;
		queryModeActive = true;
		if(InCommand)
			ServerFrameWork.QUWI("Prompt", InteractiveThread);
		if(!printed)
			Console.Write($"\r{GetTypedText()}");
	}

	/// <summary>
	/// Generates the prompt by combining the message and a list of all options.
	/// The list gets generated using the .ToString() method, so ensure that this is overloaded if a custom type is used
	/// </summary>
	private static string GenPromptStr<T>(string msg, IList<T> options) {
		var prompt = msg + " (";
		for(var n = 0; n < options.Count; n++) {
			var o = options[n];
			if(o != null)
				prompt += (n != 0 ? "/" : "") + o.ToString();
		}
		return prompt + ")" + prefix;
	}
	/// <summary>
	/// Generates the prompt by combining the message and a list of all options.
	/// The list gets generated using the .ToString() method, so ensure that this is overloaded if a custom type is used
	/// </summary>
	private static string GenPromptStr(string msg, IList<char> options) {
		var prompt = msg + " (";
		for(var n = 0; n < options.Count; n++)
			prompt += $"{(n != 0 ? "/" : "")}{options[n]}";
		return prompt + ")" + prefix;
	}
	/// <summary>
	/// Simply generates a Y or N prompt for a given question
	/// </summary>
	/// <param name="msg">your question</param>
	/// <returns>the answer as a boolean</returns>
	public bool PromptYN(string msg) => Prompt(msg, ['y', 'n']) == 0;
	/// <summary>
	/// Asks the user a question, This interrupts anything else.
	/// Pass your answer options as individual chars. 
	/// The advantage of the single char input is, that Action resumes as soon as the key is being pressed without the need for a the user to press enter
	/// </summary>
	/// <param name="msg">your question</param>
	/// <param name="options">possible answers. at least 2!</param>
	/// <returns>returns the index in <paramref name="options"/> which has been chosen by the user</returns>
	public int Prompt(string msg, IList<char> options) {
		PreparePrompt(msg, options);
		int? promptReturn = null;
		onQueryModeKeyPress = (k) => {
			if(!IsValidChar(k.KeyChar)) {
				Beep();
				return;
			}
			var c = k.KeyChar.ToString().ToUpperInvariant()[0];
			for(var n = 0; n < options.Count; n++) {
				if(options[n].ToString().ToUpperInvariant()[0] == c) {
					promptReturn = n;
					Console.WriteLine(c);
					queryModeActive = false;
				}
			}
		};
		getPromptText = () => GenPromptStr(msg, options);
		PostPreparePrompt();
		while(!promptReturn.HasValue)
			Task.Delay(100).Wait();
		return promptReturn.Value;
	}
	/// <summary>
	/// Asks the user a question, This interrupts anything else.
	/// Pass your answer options as text options the user has to type 1:1. 
	/// This version requires the user to type your answers 1:1 (ignoring capitalization) and then pressing enter.
	/// </summary>
	/// <param name="msg">your question</param>
	/// <param name="options">possible answers. at least 2!</param>
	/// <returns>returns the index in <paramref name="options"/> which has been chosen by the user</returns>
	public int Prompt(string msg, IList<string> options) {
		PreparePrompt(msg, options);
		int? promptReturn = null;
		var prompt = GenPromptStr(msg, options);
		TextModeWrapper(
			prompt,
			(typedPrompt) => {
				for(var n = 0; n < options.Count; n++) {
					if(options[n].Equals(typedPrompt, StringComparison.InvariantCultureIgnoreCase)) {
						typedPrompt = "";
						promptReturn = n;
						Console.WriteLine();
						queryModeActive = false;
						return;
					}
				}
				HideStr(prompt + prefix, typedPrompt);
				typedPrompt = "";
				Beep();
			});
		PostPreparePrompt();
		while(!promptReturn.HasValue)
			Task.Delay(100).Wait();
		return promptReturn.Value;
	}
	/// <summary>
	/// Asks the user a question, This interrupts anything else.
	/// Accepts any answer.
	/// </summary>
	/// <param name="msg">your question</param>
	/// <returns>the reply of the user</returns>
	public string Prompt(string msg) {
		PreparePrompt(msg);
		var result = "";
		TextModeWrapper(
			msg, (typedPrompt) => {
				if(string.IsNullOrWhiteSpace(typedPrompt)) {
					HideStr(msg + prefix, typedPrompt);
					typedPrompt = "";
					Beep();
				}
				else {
					result = typedPrompt;
					typedPrompt = "";
					Console.WriteLine();
					queryModeActive = false;
				}
			});
		PostPreparePrompt();
		while(result.Length == 0)
			Task.Delay(100).Wait();
		return result;
	}
	/// <summary>
	///  Asks the user for a password. This interrupts anything else.
	///  Passwords are hidden using stars
	///  We keep the password in a safe <see cref="System.Security.SecureString"/> so the full password never retained in memory.
	///  (this works because all steps in adding to the <see cref="System.Security.SecureString"/> involve atomics that get flushed immediately)
	/// </summary>
	/// <param name="msg">Passwort prompt</param>
	public System.Security.SecureString PWPrompt(string msg) {
		PreparePrompt(msg);
		var result = new System.Security.SecureString();
		var typed = 0;
		PasswordModeWrapper(msg, (typedPrompt) => {
			typed = -1;
			Console.WriteLine();
			queryModeActive = false;
		},
		(k) => {
			result.InsertAt(typed, k);
			typed++;
		},
		'*',
		null,
		(k) => {
			switch(k.Key) {
				case ConsoleKey.Backspace:
					if(typed != 0) {
						result.RemoveAt(typed - 1);
						typed--;
					}
					break;
				case ConsoleKey.Delete:
					if(typed != result.Length && result.Length != 0) {
						result.RemoveAt(typed);
					}
					else
						Beep();
					break;
				case ConsoleKey.LeftArrow:
					if(typed != 0) {
						typed--;
					}
					break;
				case ConsoleKey.RightArrow:
					if(typed < result.Length) {
						typed++;
					}
					break;
				default:
					break;
			}
		});
		PostPreparePrompt();
		while(typed != -1)
			Task.Delay(100).Wait();
		result.MakeReadOnly();
		return result;
	}
}
