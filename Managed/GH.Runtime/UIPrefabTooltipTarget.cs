using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UIRectangularRaycastFilter))]
public sealed class UIPrefabTooltipTarget : UITooltipTarget
{
	private GameObject prefab;

	private GameObject tooltipObject;

	private UnityAction<GameObject> onObjectCreatedCallback;

	public void Initialize(GameObject prefab, UnityAction<GameObject> onObjectCreatedCallback = null)
	{
		this.prefab = prefab;
		this.onObjectCreatedCallback = onObjectCreatedCallback;
	}

	protected override void ShowTooltip(float delay = -1f)
	{
		if (prefab != null)
		{
			tooltipObject = Object.Instantiate(prefab, UITooltip.GetTransform(), worldPositionStays: false);
			_ = tooltipObject.transform;
			onObjectCreatedCallback?.Invoke(tooltipObject);
			base.ShowTooltip(delay);
		}
		else
		{
			Debug.LogError("Trying to show a prefab based tooltip but the prefab is null.");
		}
	}

	protected override void SetControls()
	{
		UITooltip.SetVerticalControls(autoAdjustHeight, tempIsPrefabTooltip: true);
	}

	public override void HideTooltip(float delay = -1f)
	{
		base.HideTooltip(delay);
		Object.Destroy(tooltipObject);
	}
}
