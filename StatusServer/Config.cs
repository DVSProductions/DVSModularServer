using ModularServerSDK;
using System;
namespace StatusServer {
	public class Config : IConfig {
		public TimeSpan UpdateInterval { get; set; }
		public const string filename = "statusConfig.xml";
		public void FixSettings() => UpdateInterval = new TimeSpan(0, 0, 5);
		public bool ValidateSettings() => UpdateInterval != null && UpdateInterval.Ticks > 0;
	}
}
