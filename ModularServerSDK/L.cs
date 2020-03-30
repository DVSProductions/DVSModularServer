﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
namespace System {
	/// <summary>
	/// Your Personal LogMan.
	/// LogMan is used to store everything generated by <see cref="C"/>
	/// If you want to avoid console output and are only interested in writing to logs, you may use the Write Methods.
	/// These are completely raw tho, so you have to generate time-codes etc. yourself.
	/// LogMan will buffer all your inputs for increased speed and reduced IO load.
	/// This means that you can start writing to LogMan, even when he is not Initialized.
	/// LogMan will write the buffer to disk twice per second to allow ensure that even during a
	/// catastrophic crash as much data as possible is preserved and also that lots of I/O can be 
	/// handled at once.
	/// <para>
	/// LogMans Logfile cannot be changed after it has been set by the server.
	/// Please do not attempt to change it after the fact. 
	/// SetOnceObjects are for mutual protection.
	/// </para>
	/// </summary>
	public static class L {
		/// <summary>
		/// The logfile that is currently beeing logged to
		/// </summary>
		public static SetOnceObject<string> Logfile { get; } = new SetOnceObject<string>();
		/// <summary>
		/// Write Queue (contains lines)
		/// </summary>
		private static readonly Queue<string> toWrite = new Queue<string>();
		/// <summary>
		/// Line buffer. contains incomplete lines.
		/// </summary>
		private static readonly StringBuilder buff = new StringBuilder();
		/// <summary>
		/// Writes a full line to the log. Combined with everything from <see cref="Write(string)"/>.
		/// This has to be the final call in a series of <see cref="Write(string)"/> calls
		/// </summary>
		/// <param name="message">Message to log.</param>
		public static void WriteLine(string message) {
			lock(buff) {
				toWrite.Enqueue(buff.Append(message).ToString());
				buff.Clear();
			}
		}
		/// <summary>
		/// Write a partial line to a line. 
		/// Always complete these calls with a call to <see cref="WriteLine(string)"/>
		/// Otherwise your message might never end up in the logfile.
		/// </summary>
		/// <param name="message">A part of the message you want to log</param>
		public static void Write(string message) {
			lock(buff) {
				buff.Append(message);
			}
		}
		/// <summary>
		/// Initializes the static <see cref="L"/> class and launches a thread for logging.
		/// Returns a <see cref="Action"/> that will abort the logging thread for use during shutdown.
		/// <para>
		/// WARNING! <see cref="Init(string)"/> is protected by <see cref="SetOnceObject{TStorage}"/>.
		/// If you try to call this method after the initialization has been completed, you are going to receive a <see cref="InvalidOperationException"/>
		/// </para>
		/// </summary>
		/// <param name="logfileName">Logfile</param>
		public static Action Init(string logfileName) {
			Logfile.Set(logfileName);
			var t = ServerFrameWork.ST("LogMan", LoggingThread);
			return () => { t.Abort(); t.Join(); };
		}
		/// <summary>
		/// LogMan to the rescue!
		/// This thread uses the <see cref="toWrite"/> queue and writes everything in it to the drive.
		/// LogMan will perform two writes per second (if new data is constantly available) 
		/// It is also Important to note that when LogMan gets aborted, he will still write
		/// all remaining data in the <see cref="Queue{T}"/> to disk before quitting. 
		/// (independent of timeouts)
		/// </summary>
		private static void LoggingThread() {
			try {
				C.WriteLine("Logging to " + Logfile);
				while(true) {
					try {
						while(toWrite.Count == 0)
							Thread.Sleep(500);
						using(var fs = File.Open(Logfile, FileMode.Append))
							while(toWrite.Count > 0) {
								var s = Encoding.Default.GetBytes(toWrite.Dequeue() + "\n");
								fs.Write(s, 0, s.Length);
							}
					}
					catch(ThreadAbortException) {
						using(var fs = File.Open(Logfile, FileMode.Append)) {
							while(toWrite.Count > 0) {
								var s = Encoding.Default.GetBytes(toWrite.Dequeue() + "\n");
								fs.Write(s, 0, s.Length);
							}
						}
					}
				}
			}
			catch(ThreadAbortException) {
				C.WriteLine("stopped");
			}
			catch(Exception ex) {
				C.WriteLineE(ex);
			}
		}
	}
}
