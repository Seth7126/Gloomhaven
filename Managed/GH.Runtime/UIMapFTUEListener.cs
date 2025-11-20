using UnityEngine;

public class UIMapFTUEListener : MonoBehaviour
{
	protected virtual void Awake()
	{
		if (Singleton<MapFTUEManager>.Instance != null && Singleton<MapFTUEManager>.Instance.HasToShowFTUE)
		{
			Singleton<MapFTUEManager>.Instance.OnStarted.AddListener(OnStartedFTUE);
			Singleton<MapFTUEManager>.Instance.OnFinished.AddListener(OnFinishedFTUE);
		}
	}

	protected virtual void OnStartedFTUE()
	{
	}

	protected virtual void OnFinishedFTUE()
	{
		Clear();
	}

	protected virtual void Clear()
	{
		Singleton<MapFTUEManager>.Instance.OnFinished.RemoveListener(OnStartedFTUE);
		Singleton<MapFTUEManager>.Instance.OnStarted.RemoveListener(OnStartedFTUE);
	}
}
