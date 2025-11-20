namespace XUnity.AutoTranslator.Plugin.Core;

internal static class TextTranslationInfoExtensions
{
	public static bool GetIsKnownTextComponent(this TextTranslationInfo info)
	{
		return info?.IsKnownTextComponent ?? false;
	}

	public static bool GetSupportsStabilization(this TextTranslationInfo info)
	{
		return info?.SupportsStabilization ?? false;
	}
}
