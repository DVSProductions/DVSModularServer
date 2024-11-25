namespace System;
/// <summary>
/// Sets Console Color by Exploiting <see cref="IDisposable"/> for minimal code:
/// <c>
/// <para><see langword="using"/> (<see langword="var"/> _ = <see langword="new"/> <see cref="C.ColorHandler"/>(foreground, background)){
/// <para>
///		/* Print stuff */
/// </para>
/// }
/// </para>
/// </c>
/// </summary>
public sealed class ColorHandler : IDisposable {
	private readonly ConsoleColor? bg, fg;
	/// <summary>
	/// Sets the console Color until Disposed
	/// <para>
	/// Use <see langword="null"/> if you don't want to modify a color
	/// </para>
	/// </summary>
	/// <param name="foreground">Is <see cref="Nullable"/>!</param>
	/// <param name="background">Is <see cref="Nullable"/>!</param>
	public ColorHandler(ConsoleColor? foreground, ConsoleColor? background) {
		if(foreground.HasValue) {
			fg = Console.ForegroundColor;
			Console.ForegroundColor = foreground.Value;
		}
		if(background.HasValue) {
			bg = Console.BackgroundColor;
			Console.BackgroundColor = background.Value;
		}
	}
	/// <summary>
	/// Handler for the <see cref="using"/> directive
	/// </summary>
	public void Dispose() {
		if(fg.HasValue)
			Console.ForegroundColor = fg.Value;
		if(bg.HasValue)
			Console.BackgroundColor = bg.Value;
	}
}
