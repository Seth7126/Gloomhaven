using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Delegate)]
internal sealed class StringFormatMethodAttribute : Attribute
{
	public string FormatParameterName { get; }

	public StringFormatMethodAttribute(string formatParameterName)
	{
		FormatParameterName = formatParameterName;
	}
}
