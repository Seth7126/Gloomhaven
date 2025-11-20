#define ENABLE_LOGS
using System.Collections.Generic;
using UnityEngine;

public sealed class EntranceUndergroundDisablerProvider : MonoDisableProviderBase
{
	[SerializeField]
	private List<GameObject> _objectsToDisable;

	public override void StartDisable()
	{
		if (PlatformLayer.Setting == null)
		{
			Debug.LogWarning("Platform layer is null! It shouldn't be null!");
		}
		else if (!PlatformLayer.Setting.GetApparenceSettingByCurrentLevel()._showUnderground)
		{
			_objectsToDisable.ForEach(delegate(GameObject x)
			{
				x.SetActive(value: false);
			});
		}
	}
}
