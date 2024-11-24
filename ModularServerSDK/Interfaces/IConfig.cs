namespace ModularServerSDK; 
/// <summary>
/// Interface which all configuration classes need to implement
/// in order for them to be used by the <see cref="ConfigLoader"/>
/// </summary>
public interface IConfig {
	/// <summary>
	/// If <see cref="ValidateSettings"/> returned false, this function is called by the <see cref="ConfigLoader"/>. 
	/// If possible this function should detect invalid values and correct them.
	/// Alternatively this could just default every setting. (not recommended)
	/// </summary>
	void FixSettings();
	/// <summary>
	/// Checks that all parsed values are valid.
	/// These checks should include value range checks etc.
	/// Returns true if all settings are valid.
	/// </summary>
	bool ValidateSettings();
}
