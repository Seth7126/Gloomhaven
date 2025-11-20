namespace XUnity.AutoTranslator.Plugin.Core;

public class TimeSupport
{
	public static ITime Time { get; set; } = new UnityEngineTime();
}
