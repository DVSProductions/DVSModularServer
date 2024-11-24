using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ModularServerSDK; 
internal class HtmlFile {
	private static readonly Dictionary<Assembly, Dictionary<string, string>> fileBuffer = [];
	private readonly string myFile = "";
	public HtmlFile(Assembly caller, string fileKey, Func<string> FileContentRetrieval) {
		if (!fileBuffer.ContainsKey(caller))
			fileBuffer.Add(caller, []);
		var cont = fileBuffer[caller];
		myFile = cont.ContainsKey(fileKey) ? cont[fileKey] : (cont[fileKey] = FileContentRetrieval());

	}
	public HtmlFile(Assembly caller, string filename) : this(caller, filename, () => File.ReadAllText(filename)) { }
	/// <summary>
	/// replaces all matches
	/// </summary>
	/// <param name="processors"></param>
	/// <returns></returns>
	public string Process(Dictionary<string,Func<string>> processors) {
		var s = myFile[..];
		foreach (var kv in processors)
			s = Regex.Replace(s, "%" + kv.Key + "%", kv.Value());
		return s;
	}
}
