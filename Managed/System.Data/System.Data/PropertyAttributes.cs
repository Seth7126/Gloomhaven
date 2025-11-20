using System.ComponentModel;

namespace System.Data;

/// <summary>Specifies the attributes of a property.</summary>
/// <filterpriority>2</filterpriority>
[Flags]
[Obsolete("PropertyAttributes has been deprecated.  http://go.microsoft.com/fwlink/?linkid=14202")]
[EditorBrowsable(EditorBrowsableState.Never)]
public enum PropertyAttributes
{
	/// <summary>The property is not supported by the provider.</summary>
	NotSupported = 0,
	/// <summary>The user must specify a value for this property before the data source is initialized.</summary>
	Required = 1,
	/// <summary>The user does not need to specify a value for this property before the data source is initialized.</summary>
	Optional = 2,
	/// <summary>The user can read the property.</summary>
	Read = 0x200,
	/// <summary>The user can write to the property.</summary>
	Write = 0x400
}
