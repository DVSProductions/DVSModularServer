namespace System; 
/// <summary>
/// A helper class designed to help create html while minimizing code
/// </summary>
public static class Html {
	/// <summary>
	/// HTML line break
	/// </summary>
	public readonly static string br = "</br>";

	/// <summary>
	/// HTML escape substitutions
	/// </summary>
	private static readonly Collections.Generic.List<Tuple<string, string>> EscapeChars = [
		new Tuple<string,string>("ä","&auml;"),
		new Tuple<string,string>("ö","&ouml;"),
		new Tuple<string,string>("ü","&uuml;"),
		new Tuple<string,string>("Ä","&Auml;"),
		new Tuple<string,string>("Ö","&Ouml;"),
		new Tuple<string,string>("Ü","&Uuml;")
	];
	/// <summary>
	/// Escape text using html escape characters
	/// </summary>
	/// <param name="stringToEscape">text to escape</param>
	public static string Escape(string stringToEscape) {
		foreach(var t in EscapeChars)
			stringToEscape = stringToEscape.Replace(t.Item1, t.Item2);
		return stringToEscape;
	}
	/// <summary>
	/// Creates a html tag with content. 
	/// The content gets automatically escaped
	/// </summary>
	/// <param name="htmlTag">Html tag. as an example </param>
	/// <param name="content">text between the start and end tag</param>
	/// <returns>the compound string</returns>
	public static string Tag(string htmlTag, string content) => $"<{htmlTag}>{Escape(content)}</{htmlTag}>";
	/// <summary>
	/// Creates a html tag with content. 
	/// The content gets automatically escaped.
	/// Allows for parameters in the tag. such like href="". but you have to create that yourself
	/// </summary>
	/// <param name="htmlTag">Html tag. as an example </param>
	/// <param name="parameters">Stuff thats inside the start tag</param>
	/// <param name="content">text between the start and end tag</param>
	public static string Tag(string htmlTag, string parameters, string content) => $"<{htmlTag} {parameters}>{Escape(content)}</{htmlTag}>";
#pragma warning disable IDE1006 // Tags are allowed to violate naming schemes
	/// <summary>
	/// A shorthand for creating links
	/// </summary>
	/// <param name="href">URL of the link</param>
	/// <param name="content">shown text</param>
	public static string a(string href, string content) => Tag("a", $"href=\"{href}\"", $" {content} ");
	/// <summary>
	/// HTML paragraph
	/// </summary>
	public static string p(string content) => Tag("p", content);
	/// <summary>
	/// Html bold text
	/// </summary>
	public static string b(string content) => Tag("b", content);
	/// <summary>
	/// HTML Headline 1
	/// </summary>
	public static string h1(string content) => Tag("h1", content);
	/// <summary>
	/// HTML Headline 2
	/// </summary>
	public static string h2(string content) => Tag("h2", content);
	/// <summary>
	/// HTML Headline 3
	/// </summary>
	public static string h3(string content) => Tag("h3", content);
	/// <summary>
	/// Plain text. Escapes the text for correct display
	/// </summary>
	public static string txt(string content) => Escape(content);
#pragma warning restore IDE1006 // Tags are allowed to violate naming schemes
	/// <summary>
	/// A Quick template for setting up a website.
	/// Generates a HTML tag, implements <see cref="ServerFrameWork.MOTHERFUCKINGCSS"/> 
	/// and a body in which your content goes
	/// </summary>
	/// <param name="content">Body content</param>
	public static string BodyBuilder(string content) => "<!DOCTYPE html>" + Tag("HTML", $"{$"{ServerFrameWork.MOTHERFUCKINGCSS}"}{Tag("BODY", content)}");
}
