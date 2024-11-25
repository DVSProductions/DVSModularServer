using ModularServerSDK.Tools;

namespace System;
/// <summary>
/// This is a universal hub for anyone to get information about the server software.
/// </summary>
public static class ServerInfo {
	/// <summary>
	/// The Port on which the server is listening
	/// </summary>
	public static int Port { get; private set; } = 0;
	/// <summary>
	/// Internal use for initialization
	/// </summary>
	public static void SetPort(int p) => Port = Port == 0 ? p : throw new InvalidOperationException("Hands off");
	/// <summary>
	/// Generates a full url for you since subservers only know their own name this will make a link to your server,
	/// that can be accesssed to on the browser.
	/// </summary>
	public static SetOnceObject<Func<IServer, string>> CreateURL { get; } = new SetOnceObject<Func<IServer, string>>();
	/// <summary>
	/// Get the domain of the server
	/// </summary>
	public static readonly SetOnceObject<string> Domain = new();
}
