namespace System.Security;

/// <summary>Specifies the type of a managed code policy level.</summary>
public enum PolicyLevelType
{
	/// <summary>Security policy for all managed code in an application.</summary>
	AppDomain = 3,
	/// <summary>Security policy for all managed code in an enterprise.</summary>
	Enterprise = 2,
	/// <summary>Security policy for all managed code that is run on the computer.</summary>
	Machine = 1,
	/// <summary>Security policy for all managed code that is run by the user.</summary>
	User = 0
}
