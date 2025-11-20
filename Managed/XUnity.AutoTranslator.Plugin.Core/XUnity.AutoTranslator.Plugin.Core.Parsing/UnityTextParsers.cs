namespace XUnity.AutoTranslator.Plugin.Core.Parsing;

internal static class UnityTextParsers
{
	public static RichTextParser RichTextParser;

	public static RegexSplittingTextParser RegexSplittingTextParser;

	public static GameLogTextParser GameLogTextParser;

	public static void Initialize()
	{
		RichTextParser = new RichTextParser();
		RegexSplittingTextParser = new RegexSplittingTextParser();
		GameLogTextParser = new GameLogTextParser();
	}
}
