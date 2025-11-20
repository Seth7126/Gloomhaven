using System;

namespace SRDebugger;

[AttributeUsage(AttributeTargets.Property)]
public class IncrementAttribute : Attribute
{
	public readonly double Increment;

	public IncrementAttribute(double increment)
	{
		Increment = increment;
	}
}
