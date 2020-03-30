namespace System {
	/// <summary>
	/// This is your colorful and amazing console interface. Use it!
	/// <para>
	/// Please refrain from using any direct access to the default <see cref="Console"/> <see langword="class"/> , as this 
	/// will break a lot of stuff, and we wouldn't want that, would we?
	/// </para>
	/// Features:
	/// <para>
	/// <see cref="C"/> will keep track of time, and thread that sent messages for easier display. 
	/// It even shows when the day has changed to minimize console output while not leaving out anything.
	/// <see cref="C"/> will write everything (without colors) into a logfile using <see cref="L"/>
	/// 
	/// </para>
	/// </summary>
	public static class C {
		/// <summary>
		/// Date Pinter is a class that will print the current date when it has changed 
		/// between prints.
		/// </summary>
		private static class DatePrinter {
			/// <summary>
			/// Last printed date.
			/// </summary>
			private static DateTime known;
			/// <summary>
			/// Print the date if it has changed since we last printed the date
			/// </summary>
			public static void PrintIfChanged() {
				if(known.Date != DateTime.Now.Date) {
					known = DateTime.Now.Date;
					using(var _ = new ColorHandler(null, ConsoleColor.DarkGray))
						PrintLine($" -={{{known.Date.ToLongDateString()}}}=- ");
				}
			}
		}
		/// <summary>
		/// console width
		/// </summary>
		private static int width = 0;
		/// <summary>
		/// Prints the server title.
		/// (can only be done once.)
		/// </summary>
		public static readonly CallOnceAction<string> createTitle = new CallOnceAction<string>(
			(msg) => {
				width = 0;
				using(var _ = new ColorHandler(ConsoleColor.Black, ConsoleColor.DarkGray)) {
					do {
						width++;
						Console.Write("-");
					} while(Console.CursorLeft != 0);
					Console.CursorTop--;
					Console.CursorLeft = (width / 2) - (msg.Length / 2);
					Console.WriteLine(msg);
				}
			}
			);
		/// <summary>
		/// Generates a full line with the given character. Used to create clear visual separation
		/// </summary>
		/// <param name="character">the character to fill the line with</param>
		public static void FillLine(char character) {
			var s = new Text.StringBuilder("\r", width + 2);
			for(var n = 0; n < width; n++) s.Append(character);
			s.Append("\b\n");
			Console.Write(s);
		}
		/// <summary>
		/// Generates a full line with the given character and color. Used to create clear visual separation
		/// </summary>
		/// <param name="character">the character to fill the line with</param>
		/// <param name="foreground"></param>
		/// <param name="background"></param>
		public static void FillLineC(char character, ConsoleColor? foreground, ConsoleColor? background) {
			using(var _ = new ColorHandler(foreground, background))
				FillLine(character);
		}
		/// <summary>
		/// used for output synchronization. If this is locked, someone is writing to the output
		/// </summary>
		private static IInteractive lockme;
		/// <summary>
		/// Accesss to the Input handler. Once set, use this to do all your Input from the console
		/// </summary>
		public static IInteractive Input { get; private set; }
		/// <summary>
		/// Sets the Input Once. (during object creation.)
		/// DON'T TOUCH THIS IF YOU AREN'T AUTHORIZED! (you will receive an exception)
		/// </summary>
		/// <param name="newInteractive">The <see cref="IInteractive"/> to use</param>
		public static Action SetInput(IInteractive newInteractive) {
			if(Input == null) Input = newInteractive;
			else throw new AccessViolationException("Keep your hands of Integral Parts!");
			lockme = newInteractive;
			return () => Input = null;
		}
		/// <summary>
		/// Current Thread Name.
		/// Returns **your** thread name
		/// </summary>
		private static string CTN => Threading.Thread.CurrentThread.Name;
		/// <summary>
		/// <para>Sets the Color to the default color Scheme</para>
		/// <see cref="ConsoleColor.Black"/> as Background
		/// <para><see cref="ConsoleColor.White"/> as Foreground</para>
		/// </summary>
		public static void ResetColor() {
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.White;
		}
		/// <summary>
		/// Print something on the console
		/// </summary>
		/// <param name="partialMessage"></param>
		private static void Print(string partialMessage) {
			Console.Write(partialMessage);
			L.Write(partialMessage);
		}
		private static void PrintLine(string message) {
			Console.WriteLine(message);
			L.WriteLine(message);
		}
		/// <summary>
		/// default color
		/// </summary>
		/// <param name="value">Object to print. will use <see cref="object.ToString()"/></param>
		public static void WriteLine(object value) {
			lock(lockme) {
				var str = Input?.GetTypedText();
				if(str != null) Console.Write('\r');
				DatePrinter.PrintIfChanged();
				PrintLine($"[{DateTime.Now.ToLongTimeString()}] {CTN}: {(value ?? "NULL").ToString()}");
				if(str != null) {
					Console.Write(str);
					Console.CursorLeft = Input.CursorX;
				}
			}
		}
		/// <summary>
		/// Custom color
		/// </summary>
		/// <param name="value">Object to print. will use <see cref="object.ToString()"/></param>
		/// <param name="foreground"></param>
		public static void WriteLineC(object value, ConsoleColor? foreground, ConsoleColor? background = null) {
			lock(lockme) {
				var str = Input?.GetTypedText();
				if(str != null) Console.Write('\r');
				DatePrinter.PrintIfChanged();
				Print($"[{DateTime.Now.ToLongTimeString()}] {CTN}: ");
				using(var _ = new ColorHandler(background, foreground))
					PrintLine($"{(value??"NULL").ToString()} ");
				if(str != null) {
					Console.Write(str);
					Console.CursorLeft = Input.CursorX;
				}
			}
		}
		/// <summary>
		/// Error
		/// </summary>
		/// <param name="value">Object to print. will use <see cref="object.ToString()"/></param>
		public static void WriteLineE(object value) =>
			WriteLineC(value, ConsoleColor.DarkRed, ConsoleColor.White);
		/// <summary>
		/// information
		/// </summary>
		/// <param name="value">Object to print. will use <see cref="object.ToString()"/></param>
		public static void WriteLineI(object value) =>
			WriteLineC(value, ConsoleColor.DarkYellow, ConsoleColor.Black);
		/// <summary>
		/// Success
		/// </summary>
		/// <param name="value">Object to print. will use <see cref="object.ToString()"/></param>
		public static void WriteLineS(object value) =>
			WriteLineC(value, ConsoleColor.Green, ConsoleColor.Black);
	}
}