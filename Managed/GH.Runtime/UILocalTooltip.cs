using UnityEngine;
using UnityEngine.UI;

public class UILocalTooltip : MonoBehaviour
{
	[Tooltip("If window is null, it will enable/disable the object when this toolip is shown/hidden. If a window is assigned, it will show/hide that window when this tooltip is shown/hidden")]
	[SerializeField]
	private UIWindow window;

	[Header("Target Position")]
	[SerializeField]
	private RectTransform target;

	[SerializeField]
	private Vector2 targetOffset;

	[Header("Screen bound")]
	[SerializeField]
	private bool trackScreenBound;

	[ConditionalField("trackScreenBound", null, true)]
	[SerializeField]
	private float offsetScreen = 20f;

	public bool IsShown
	{
		get
		{
			if (base.gameObject.activeSelf)
			{
				if (!(window == null))
				{
					return window.IsOpen;
				}
				return true;
			}
			return false;
		}
	}

	public virtual void SetTarget(RectTransform newTarget)
	{
		target = newTarget;
		RefreshPosition();
	}

	public void SetTarget(RectTransform newTarget, Vector2 newTargetOffset)
	{
		targetOffset = newTargetOffset;
		SetTarget(newTarget);
	}

	public void Show()
	{
		if (window != null)
		{
			window.Show();
		}
		else
		{
			base.gameObject.SetActive(value: true);
		}
		RefreshPosition();
		base.enabled = trackScreenBound;
	}

	public virtual void Hide()
	{
		if (window != null)
		{
			window.Hide();
			base.enabled = false;
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void LateUpdate()
	{
		RefreshPosition();
	}

	public void RefreshPosition()
	{
		if (!(target == null) && IsShown)
		{
			SetPosition(target);
			(base.transform as RectTransform).anchoredPosition += targetOffset;
			base.transform.position += (base.transform as RectTransform).DeltaWorldPositionToFitTheScreen(UIManager.Instance.UICamera, offsetScreen);
		}
	}

	protected virtual void SetPosition(RectTransform targetPosition)
	{
		base.transform.position = target.position;
	}
}
