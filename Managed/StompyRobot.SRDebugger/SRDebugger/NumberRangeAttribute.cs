using System;

namespace SRDebugger;

[AttributeUsage(AttributeTargets.Property)]
public class NumberRangeAttribute : Attribute
{
	public readonly double Max;

	public readonly double Min;

	public NumberRangeAttribute(double min, double max)
	{
		Min = min;
		Max = max;
	}
}
