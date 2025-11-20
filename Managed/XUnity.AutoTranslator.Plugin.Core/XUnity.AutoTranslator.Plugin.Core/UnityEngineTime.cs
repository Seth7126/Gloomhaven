using UnityEngine;

namespace XUnity.AutoTranslator.Plugin.Core;

public class UnityEngineTime : ITime
{
	public float realtimeSinceStartup => Time.realtimeSinceStartup;

	public int frameCount => Time.frameCount;
}
