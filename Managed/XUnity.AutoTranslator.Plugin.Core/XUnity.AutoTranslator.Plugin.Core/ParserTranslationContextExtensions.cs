using XUnity.AutoTranslator.Plugin.Core.Parsing;

namespace XUnity.AutoTranslator.Plugin.Core;

internal static class ParserTranslationContextExtensions
{
	public static bool HasBeenParsedBy(this ParserTranslationContext context, ParserResultOrigin parser)
	{
		for (ParserTranslationContext parserTranslationContext = context; parserTranslationContext != null; parserTranslationContext = parserTranslationContext.ParentContext)
		{
			if (parserTranslationContext.Result.Origin == parser)
			{
				return true;
			}
		}
		return false;
	}

	public static int GetLevelsOfRecursion(this ParserTranslationContext context)
	{
		return context?.LevelsOfRecursion ?? 0;
	}

	public static ParserTranslationContext GetAncestorContext(this ParserTranslationContext context)
	{
		ParserTranslationContext parserTranslationContext = context;
		for (ParserTranslationContext parentContext = parserTranslationContext.ParentContext; parentContext != null; parentContext = parserTranslationContext.ParentContext)
		{
			parserTranslationContext = parentContext;
		}
		return parserTranslationContext;
	}
}
