using UnityEngine;
using UnityEngine.UI;

public class UIDistributePointsToggle : UIDistributePointsCounter
{
	[SerializeField]
	private Toggle toggle;

	[SerializeField]
	private GameObject activeMask;

	[SerializeField]
	private GameObject disabledMask;

	private void Awake()
	{
		toggle.onValueChanged.AddListener(delegate
		{
			RefreshMasks();
		});
	}

	private void OnDestroy()
	{
		toggle.onValueChanged.RemoveAllListeners();
	}

	public override void SetCurrentPoints(int currentPoints)
	{
	}

	public override void SetExtendedPoints(int extendedPoints)
	{
		toggle.isOn = extendedPoints > 0;
		RefreshMasks();
	}

	private void RefreshMasks()
	{
		activeMask.SetActive(toggle.isOn);
		disabledMask.SetActive(!toggle.isOn);
	}
}
