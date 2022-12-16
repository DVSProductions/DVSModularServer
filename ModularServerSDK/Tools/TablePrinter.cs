using System;
using System.Collections.Generic;
using System.Text;

namespace ModularServerSDK.Tools {
	public static class TablePrinter {
		const char h = '=', c = '+', v = '|';
		public static StringBuilder GenerateTable<T>(IEnumerable<T> data, Func<T, string[]> RowGenerator, string[] Header) {
			var buffer = new List<string[]>();
			var widths = new int[Header.Length];
			for(var n = 0; n < Header.Length; n++)
				widths[n] = Header[n].Length;
			//generate and measure
			foreach(var d in data) {
				var row = RowGenerator(d);
				for(var n = 0; n < Header.Length && n < row.Length; n++)
					if(row[n].Length > widths[n])
						widths[n] = row[n].Length;
				buffer.Add(row);
			}
			//print
			var result = new StringBuilder();
			void addRow(string[] arr) {
				for(var n = 0; n < arr.Length; n++) {
					result.Append(arr[n].PadRight(widths[n] + 1).PadLeft(widths[n] + 2));
					if(n < arr.Length - 1)
						result.Append(v);
				}
				result.AppendLine();
			}
			addRow(Header);
			for(var n = 0; n < Header.Length; n++) {
				result.Append(h, widths[n] + 2);
				if(n < Header.Length - 1)
					result.Append(c);
			}
			result.AppendLine();
			foreach(var row in buffer) {
				addRow(row);
			}
			return result;
		}
		public static void PrintTable<T>(IEnumerable<T> data, Func<T, string[]> RowGenerator, string[] Header) {
			var print = new StringBuilder();
			print.AppendLine();
			var gen = GenerateTable<T>(data, RowGenerator, Header);
			print.Append(gen);
			C.WriteLine(print.ToString());
		}
	}
}
