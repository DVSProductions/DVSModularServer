using System.Collections.Generic;

namespace System {
	/// <summary>
	/// All public Functions that the interactive shell exposes for general use.
	/// Used in the <see cref="C"/> for you to interact with the user.
	/// </summary>
	public interface IInteractive {
		/// <summary>
		/// Returns everything the user has typed since the last command
		/// </summary>
		/// <returns></returns>
		string GetTypedText();
		/// <summary>
		/// returns the cursor position
		/// </summary>
		int CursorX { get; }
		/// <summary>
		/// Simply generates a Y or N prompt for a given question
		/// </summary>
		/// <param name="msg">your question</param>
		/// <returns>the answer as a boolean</returns>
		bool PromptYN(string message);
		/// <summary>
		/// Asks the user a question, This interrupts anything else.
		/// Accepts any answer.
		/// </summary>
		/// <param name="msg">your question</param>
		/// <returns>the reply of the user</returns>
		string Prompt(string message);
		/// <summary>
		///  Asks the user for a password. This interrupts anything else.
		///  Passwords are hidden using stars
		///  We keep the password in a safe <see cref="Security.SecureString"/> so the full password never retained in memory.
		///  (this works because all steps in adding to the <see cref="Security.SecureString"/> involve atomics that get flushed immediately)
		/// </summary>
		/// <param name="msg">Passwort prompt</param>
		Security.SecureString PWPrompt(string message);
		/// <summary>
		/// Asks the user a question, This interrupts anything else.
		/// Pass your answer options as individual chars. 
		/// The advantage of the single char input is, that Action resumes as soon as the key is being pressed without the need for a the user to press enter
		/// </summary>
		/// <param name="msg">your question</param>
		/// <param name="options">possible answers. at least 2!</param>
		/// <returns>returns the index in <paramref name="options"/> which has been chosen by the user</returns>
		int Prompt(string message, IList<char> options);
		/// <summary>
		/// Asks the user a question, This interrupts anything else.
		/// Pass your answer options as text options the user has to type 1:1. 
		/// This version requires the user to type your answers 1:1 (ignoring capitalization) and then pressing enter.
		/// </summary>
		/// <param name="msg">your question</param>
		/// <param name="options">possible answers. at least 2!</param>
		/// <returns>returns the index in <paramref name="options"/> which has been chosen by the user</returns>
		int Prompt(string message, IList<string> options);
	}
}
