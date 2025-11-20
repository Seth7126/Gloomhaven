using System.Configuration;

namespace System.Transactions.Configuration;

/// <summary>Represents an XML section in a configuration file encapsulating all settings that can be modified only at the machine level. This class cannot be inherited.</summary>
public class MachineSettingsSection : ConfigurationSection
{
	/// <summary>Gets a maximum amount of time allowed before a transaction times out.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> object that contains the maximum allowable time. The default value is 00:10:00.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">An attempt to set this property to negative values.</exception>
	[TimeSpanValidator(MinValueString = "00:00:00", MaxValueString = "10675199.02:48:05.4775807")]
	[ConfigurationProperty("maxTimeout", DefaultValue = "00:10:00")]
	public TimeSpan MaxTimeout
	{
		get
		{
			return (TimeSpan)base["maxTimeout"];
		}
		set
		{
			base["maxTimeout"] = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Transactions.Configuration.MachineSettingsSection" /> class. </summary>
	public MachineSettingsSection()
	{
	}
}
