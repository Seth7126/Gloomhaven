#define ENABLE_LOGS
using System.Collections.Generic;
using SM;
using UnityEngine;

[RequireComponent(typeof(DetailsDisabler))]
public sealed class DetailLevelDisableProvider : MonoDisableProviderBase
{
	[SerializeField]
	private SerializableDictionary<DetailsDisablingLevel, DetailsModel> _detailsLevel;

	public override void StartDisable()
	{
		if (PlatformLayer.Setting == null)
		{
			Debug.LogWarning("Platform layer is null! It shouldn't be null!");
			return;
		}
		DetailsDisablingLevel detailsDisablingLevel = PlatformLayer.Setting.GetApparenceSettingByCurrentLevel()._detailsDisablingLevel;
		DisableInternal(detailsDisablingLevel);
	}

	public void SetDetailsLevel(DetailsDisablingLevel detailsLevel, DetailsModel detailsModel)
	{
		_detailsLevel[detailsLevel] = detailsModel;
	}

	public List<GameObject> GetAllChildren()
	{
		List<GameObject> list = new List<GameObject>(base.gameObject.transform.childCount);
		foreach (Transform item in base.gameObject.transform)
		{
			list.Add(item.gameObject);
		}
		return list;
	}

	public void EnableAll()
	{
		foreach (GameObject allChild in GetAllChildren())
		{
			allChild.SetActive(value: true);
		}
	}

	public void ToggleDetailsLevel(DetailsDisablingLevel detailsDisablingLevel)
	{
		if (_detailsLevel.TryGetValue(detailsDisablingLevel, out var value))
		{
			value._objectsToDisable.ForEach(delegate(GameObject x)
			{
				x.SetActive(!x.activeSelf);
			});
		}
	}

	private void DisableInternal(DetailsDisablingLevel detailsDisablingLevel)
	{
		if (_detailsLevel.TryGetValue(detailsDisablingLevel, out var value))
		{
			value._objectsToDisable.ForEach(delegate(GameObject x)
			{
				x.SetActive(value: false);
			});
		}
	}
}
