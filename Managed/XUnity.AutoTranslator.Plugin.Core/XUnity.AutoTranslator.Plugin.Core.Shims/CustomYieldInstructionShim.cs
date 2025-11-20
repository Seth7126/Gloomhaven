using System.Collections;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Constants;

namespace XUnity.AutoTranslator.Plugin.Core.Shims;

public abstract class CustomYieldInstructionShim : IEnumerator
{
	private float? _startTime;

	public object Current => null;

	public abstract bool keepWaiting { get; }

	public float? InGameTimeout { get; set; }

	public bool IsTimedOut { get; set; }

	internal bool DetermineKeepWaiting()
	{
		if (!keepWaiting || IsTimedOut)
		{
			return false;
		}
		if (InGameTimeout.HasValue)
		{
			if (!_startTime.HasValue)
			{
				_startTime = TimeSupport.Time.realtimeSinceStartup;
			}
			float value = _startTime.Value;
			if (TimeSupport.Time.realtimeSinceStartup - value > InGameTimeout)
			{
				IsTimedOut = true;
				return false;
			}
		}
		return true;
	}

	public bool MoveNext()
	{
		return DetermineKeepWaiting();
	}

	public void Reset()
	{
	}

	public IEnumerator GetSupportedEnumerator()
	{
		if (UnityFeatures.SupportsCustomYieldInstruction)
		{
			yield return this;
			yield break;
		}
		while (DetermineKeepWaiting())
		{
			yield return CoroutineHelper.CreateWaitForSeconds(0.2f);
		}
	}
}
