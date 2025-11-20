namespace XUnity.AutoTranslator.Plugin.Core;

public interface ITime
{
	float realtimeSinceStartup { get; }

	int frameCount { get; }
}
