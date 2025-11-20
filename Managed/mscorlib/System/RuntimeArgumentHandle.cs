namespace System;

/// <summary>References a variable-length argument list.</summary>
/// <filterpriority>2</filterpriority>
public ref struct RuntimeArgumentHandle
{
	internal IntPtr args;
}
