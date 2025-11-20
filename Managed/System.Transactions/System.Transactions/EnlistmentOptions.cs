namespace System.Transactions;

/// <summary>Determines whether the object should be enlisted during the prepare phase.</summary>
[Flags]
public enum EnlistmentOptions
{
	/// <summary>The object does not require enlistment during the initial phase of the commitment process.</summary>
	None = 0,
	/// <summary>The object must enlist during the initial phase of the commitment process.</summary>
	EnlistDuringPrepareRequired = 1
}
