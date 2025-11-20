using System.Threading;

namespace UnityEngine.Formats.Alembic.Sdk;

internal static class AbcAPI
{
	public static void aeWaitMaxDeltaTime()
	{
		float num = Time.unscaledTime + Time.maximumDeltaTime;
		while (Time.realtimeSinceStartup < num)
		{
			Thread.Sleep(1);
		}
	}
}
