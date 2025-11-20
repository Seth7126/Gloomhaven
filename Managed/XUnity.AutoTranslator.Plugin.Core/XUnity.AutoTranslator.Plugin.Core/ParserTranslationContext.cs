using System.Collections.Generic;
using System.Linq;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Parsing;

namespace XUnity.AutoTranslator.Plugin.Core;

internal class ParserTranslationContext
{
	public ParserResult Result { get; private set; }

	public HashSet<TranslationJob> Jobs { get; private set; }

	public InternalTranslationResult TranslationResult { get; private set; }

	public object Component { get; private set; }

	public TranslationEndpointManager Endpoint { get; private set; }

	public ParserTranslationContext ParentContext { get; private set; }

	public List<ParserTranslationContext> ChildrenContexts { get; private set; }

	public int LevelsOfRecursion { get; private set; }

	public ParserTranslationContext(object component, TranslationEndpointManager endpoint, InternalTranslationResult translationResult, ParserResult result, ParserTranslationContext parentContext)
	{
		Jobs = new HashSet<TranslationJob>();
		ChildrenContexts = new List<ParserTranslationContext>();
		Component = component;
		Result = result;
		Endpoint = endpoint;
		TranslationResult = translationResult;
		ParentContext = parentContext;
		parentContext?.ChildrenContexts.Add(this);
		ParserTranslationContext parserTranslationContext = this;
		while (parserTranslationContext != null)
		{
			parserTranslationContext = parserTranslationContext.ParentContext;
			LevelsOfRecursion++;
		}
	}

	private ParserResult GetHighestPriorityResult()
	{
		ParserResult parserResult = Result;
		int num = parserResult.Priority;
		ParserTranslationContext parserTranslationContext = this;
		while ((parserTranslationContext = parserTranslationContext.ParentContext) != null)
		{
			ParserResult result = parserTranslationContext.Result;
			int priority = result.Priority;
			if (priority > num)
			{
				num = priority;
				parserResult = result;
			}
		}
		return parserResult;
	}

	public bool CachedCombinedResult()
	{
		return GetHighestPriorityResult().CacheCombinedResult;
	}

	public bool PersistCombinedResult()
	{
		return GetHighestPriorityResult().PersistCombinedResult;
	}

	public bool HasAllJobsCompleted()
	{
		bool flag = Jobs.All((TranslationJob x) => x.State == TranslationJobState.Succeeded);
		if (flag)
		{
			foreach (ParserTranslationContext childrenContext in ChildrenContexts)
			{
				flag = childrenContext.HasAllJobsCompleted();
				if (!flag)
				{
					return false;
				}
			}
		}
		return flag;
	}
}
