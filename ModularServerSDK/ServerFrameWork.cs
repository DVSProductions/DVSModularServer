using System.Net;
using System.Reflection;
using System.Text;
namespace System;
/// <summary>
/// A collection of helpful functions and delegates in order to make your life easier
/// </summary>
public static class ServerFrameWork {
	/// <summary>
	/// Delegate for errorPage generators. If you want to implement a custom error Page for the server to display, use
	/// this delegate for functions that generate such pages.
	/// Used in the default error page displays
	/// </summary>
	/// <param name="errorcode">Current error code</param>
	/// <param name="additionalInfo">Any string information related to the error. Probably an error message</param>
	public delegate string errorPage(HttpStatusCode errorcode, string additionalInfo);
	/// <summary>
	/// Delegate defining what a Method should look like, that generates responses from requests.
	/// returns the response body
	/// </summary>
	/// <param name="request">incoming request</param>
	/// <param name="response">outgoing response</param>
	/// <returns>the response body</returns>
	public delegate string respoMethod(HttpListenerRequest request, HttpListenerResponse response);
	/// <summary>
	/// QueueUserWorkItem shorthand with automatic thread naming for actions.
	/// </summary>
	/// <param name="name">Name for the created <see cref="Thread"/></param>
	/// <param name="action">code for the thread</param>
	public static void QUWI(string name, Action action) => ThreadPool.QueueUserWorkItem((_) => { Thread.CurrentThread.Name = name; action(); });
	/// <summary>
	/// QueueUserWorkItem shorthand with automatic thread naming for actions
	/// </summary>
	/// <param name="name">Name for the created  <see cref="Thread"/></param>
	/// <param name="action">code for the thread</param>
	/// <param name="passThis">parameters for the action</param>
	public static void QUWI<T>(string name, Action<T> action, T passThis) => ThreadPool.QueueUserWorkItem(_ => { Thread.CurrentThread.Name = name; action(passThis); });
	/// <summary>
	/// Start Thread with a given Action
	/// </summary>
	/// <param name="name">Name for the created <see cref="Thread"/></param>
	/// <param name="action">code for the thread</param>
	public static Thread ST(string name, ThreadStart action) => ST(name, action, ThreadPriority.Normal);
	/// <summary>
	/// Start Thread with a given Action and Priority
	/// </summary>
	/// <param name="name">Name for the created <see cref="Thread"/></param>
	/// <param name="action">code for the thread</param>
	/// <param name="priority">Thread Priority of the created thread</param>
	public static Thread ST(string name, ThreadStart action, ThreadPriority priority) {
		var t = new Thread(action) {
			Name = name,
			Priority = priority
		};
		t.Start();
		return t;
	}
	/// <summary>
	/// Inspired by http://bettermotherfuckingwebsite.com/
	/// </summary>
	public static string MOTHERFUCKINGCSS => "<HEAD><style type=\"text/css\"> body{margin:40px auto;max-width:650px;line-height:1.6;font-size:18px;color:#444;padding:0 10px} h1,h2,h3{line-height:1.2}</style></HEAD>";
	/// <summary>
	/// Extracts queries from a <see cref="Collections.Specialized.NameValueCollection"/> and turns them into a string list
	/// </summary>
	/// <param name="collection">source</param>
	public static string ReadQuery(Collections.Specialized.NameValueCollection collection) {
		if(collection == null)
			return "";
		var ret = new StringBuilder().Append('[');
		foreach(var k in collection.AllKeys)
			ret.Append($"{k} = {collection.Get(k)}, ");
		return ret.ToString().TrimEnd([',', ' ']) + "]";
	}
	/// <summary>
	/// Gets the name of the current thread. A bit quicker and easier to read than the direct option
	/// </summary>
	public static string ThreadName { get => Thread.CurrentThread.Name ?? string.Empty; set => Thread.CurrentThread.Name = value; }
	/// <summary>
	/// Generates A full String representation of objects. 
	/// Basically Serializes them for debug printing
	/// </summary>
	/// <param name="o">object you want to serialize</param>
	/// <param name="maxdepth">
	/// How many layers shall be unwrapped. 
	/// Set this if you either only need a brief overview over a object, or if the loop detection fails and you need to stop it
	/// </param>
	public static string RecursiveToString(object? o, int maxdepth = Int32.MaxValue) {
		var butler = new HashSet<object>();
		return RecursiveToString(o, ref butler, maxdepth);
	}
	/// <summary>
	/// Helper function for the recursion. Don't worry about it
	/// </summary>
	private static string RecursiveToString(object? o, ref HashSet<object> knownOBJS, int maxdepth = Int32.MaxValue) {
		knownOBJS ??= [];
		if(o is null)
			return "null";
		else {
			var t = o.GetType();
			if(maxdepth <= 0)
				return t.ToString();
			var ret = new StringBuilder();
			const string exmsg = "--EX--";
			const string seperator = ", ";
			if(o is IEnumerable<object> oAsEnum) {
				ret.Append("ArrayElements = [");
				foreach(var e in oAsEnum) {
					try {
						ret.Append(RecursiveToString(e, ref knownOBJS, maxdepth - 1));
					}
					catch {
						ret.Append(exmsg);
					}
					ret.Append(seperator);
				}
				return ret.ToString().TrimEnd([',', ' ']) + "]";
			}
			if(o is string s)
				return s;
			if(t.IsPrimitive) {
				return o.ToString() ?? "null";
			}
			if(knownOBJS.Contains(o))
				return o.ToString() ?? "null";
			//foreach(var obj in knownOBJS) {
			//	if(ReferenceEquals(obj, o))
			//		return o.ToString();
			//}
			knownOBJS.Add(o);
			ret.Append('{');
			foreach(var p in o.GetType().GetRuntimeProperties()) {
				ret.Append($"{p.Name} = ");
				try {
					ret.Append(RecursiveToString(p.GetValue(o), ref knownOBJS, maxdepth - 1));
				}
				catch {
					ret.Append(exmsg);
				}
				ret.Append(seperator);
			}
			return ret.ToString().TrimEnd([',', ' ']) + "}";
		}
	}
}
