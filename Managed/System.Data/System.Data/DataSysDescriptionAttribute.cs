using System.ComponentModel;

namespace System.Data;

/// <summary>Marks a property, event, or extender with a description. Visual designers can display this description when referencing the member.</summary>
/// <filterpriority>1</filterpriority>
[AttributeUsage(AttributeTargets.All)]
[Obsolete("DataSysDescriptionAttribute has been deprecated.  https://go.microsoft.com/fwlink/?linkid=14202", false)]
public class DataSysDescriptionAttribute : DescriptionAttribute
{
	private bool _replaced;

	/// <summary>Gets the text for the description. </summary>
	/// <returns>The description string.</returns>
	/// <filterpriority>2</filterpriority>
	public override string Description
	{
		get
		{
			if (!_replaced)
			{
				_replaced = true;
				base.DescriptionValue = base.Description;
			}
			return base.Description;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.DataSysDescriptionAttribute" /> class using the specified description string.</summary>
	/// <param name="description">The description string. </param>
	[Obsolete("DataSysDescriptionAttribute has been deprecated.  https://go.microsoft.com/fwlink/?linkid=14202", false)]
	public DataSysDescriptionAttribute(string description)
		: base(description)
	{
	}
}
