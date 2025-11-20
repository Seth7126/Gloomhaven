namespace System.Diagnostics.Contracts;

/// <summary>Defines abbreviations that you can use in place of the full contract syntax.</summary>
[Conditional("CONTRACTS_FULL")]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class ContractAbbreviatorAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Contracts.ContractAbbreviatorAttribute" /> class.</summary>
	public ContractAbbreviatorAttribute()
	{
	}
}
