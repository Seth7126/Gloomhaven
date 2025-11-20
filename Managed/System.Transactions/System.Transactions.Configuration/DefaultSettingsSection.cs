using System.Configuration;

namespace System.Transactions.Configuration;

/// <summary>Represents an XML section in a configuration file that contains default values of a transaction. This class cannot be inherited.</summary>
public class DefaultSettingsSection : ConfigurationSection
{
	/// <summary>Gets or sets a default time after which a transaction times out.</summary>
	/// <returns>A <see cref="T:System.TimeSpan" /> object. The default property is 00:01:00. </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">An attempt to set this property to negative values.</exception>
	[TimeSpanValidator(MinValueString = "00:00:00", MaxValueString = "10675199.02:48:05.4775807")]
	[ConfigurationProperty("timeout", DefaultValue = "00:01:00")]
	public TimeSpan Timeout
	{
		get
		{
			return (TimeSpan)base["timeout"];
		}
		set
		{
			base["timeout"] = value;
		}
	}

	/// <summary>Gets the name of the transaction manager.</summary>
	/// <returns>The name of the transaction manager. The default value is an empty string.</returns>
	/// <exception cref="T:System.NotSupportedException">An attempt to set this property to fully qualified domain names or IP addresses.</exception>
	/// <exception cref="T:System.Transactions.TransactionAbortedException">An attempt to set this property to localhost.</exception>
	[ConfigurationProperty("distributedTransactionManagerName", DefaultValue = "")]
	public string DistributedTransactionManagerName
	{
		get
		{
			return base["distributedTransactionManagerName"] as string;
		}
		set
		{
			base["distributedTransactionManagerName"] = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Transactions.Configuration.DefaultSettingsSection" /> class. </summary>
	public DefaultSettingsSection()
	{
	}
}
