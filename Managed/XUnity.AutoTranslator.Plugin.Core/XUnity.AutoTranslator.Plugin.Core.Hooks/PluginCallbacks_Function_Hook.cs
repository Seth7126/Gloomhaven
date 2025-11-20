using System.Reflection;

namespace XUnity.AutoTranslator.Plugin.Core.Hooks;

internal static class PluginCallbacks_Function_Hook<T>
{
	private static MethodInfo _nextMethod;

	private static IReadOnlyTextTranslationCache _cache;

	public static void SetTextCache(IReadOnlyTextTranslationCache cache)
	{
		_cache = cache;
	}

	public static void Register(MethodInfo method)
	{
		_nextMethod = method;
	}

	public static void Clean()
	{
		_nextMethod = null;
	}

	private static bool Prepare(object instance)
	{
		return true;
	}

	private static MethodBase TargetMethod(object instance)
	{
		return _nextMethod;
	}

	private static void Prefix()
	{
		CallOrigin.TextCache = _cache;
	}

	private static void Finalizer()
	{
		CallOrigin.TextCache = null;
	}
}
