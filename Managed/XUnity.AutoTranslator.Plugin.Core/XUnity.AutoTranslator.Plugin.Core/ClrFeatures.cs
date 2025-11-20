using System;
using System.Reflection.Emit;
using XUnity.Common.Constants;

namespace XUnity.AutoTranslator.Plugin.Core;

internal static class ClrFeatures
{
	internal static bool SupportsNet4x { get; }

	internal static bool SupportsReflectionEmit { get; }

	static ClrFeatures()
	{
		try
		{
			SupportsNet4x = ClrTypes.Task != null;
		}
		catch (Exception)
		{
		}
		try
		{
			TestReflectionEmit();
			SupportsReflectionEmit = true;
		}
		catch (Exception)
		{
			SupportsReflectionEmit = false;
		}
	}

	private static void TestReflectionEmit()
	{
		_ = default(Label) == default(Label);
	}
}
