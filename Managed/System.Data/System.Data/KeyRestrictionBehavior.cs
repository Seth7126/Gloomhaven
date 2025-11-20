namespace System.Data;

/// <summary>Identifies a list of connection string parameters identified by the KeyRestrictions property that are either allowed or not allowed.</summary>
/// <filterpriority>2</filterpriority>
public enum KeyRestrictionBehavior
{
	/// <summary>Default. Identifies the only additional connection string parameters that are allowed.</summary>
	AllowOnly,
	/// <summary>Identifies additional connection string parameters that are not allowed.</summary>
	PreventUsage
}
