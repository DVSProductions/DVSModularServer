using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ModularServerSDK {

	class HtmlFile {
		static readonly Dictionary<Assembly, Dictionary<string, string>> fileBuffer = new Dictionary<Assembly, Dictionary<string, string>>();
		readonly string myFile = "";
		public HtmlFile(Assembly caller, string fileKey, Func<string> FileContentRetrieval) {
			if (!fileBuffer.ContainsKey(caller))
				fileBuffer.Add(caller, new Dictionary<string, string>());
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
			var s = myFile.Substring(0);
			foreach (var kv in processors)
				s = Regex.Replace(s, "%" + kv.Key + "%", kv.Value());
			return s;
		}
	}
}
