using AsmodeeNet.Foundation;
using UnityEngine;
using UnityEngine.UI;

public class UIMenuOptionButton : UIMenuOption
{
	[SerializeField]
	private Image selectedMask;

	[Space]
	[SerializeField]
	protected ExtendedButton button;

	[SerializeField]
	protected bool toggleSelectedOnClick = true;

	protected virtual void Awake()
	{
		button.onClick.AddListener(OnClick);
		button.onMouseEnter.AddListener(delegate
		{
			OnHovered(hovered: true);
		});
		button.onMouseExit.AddListener(delegate
		{
			OnHovered(hovered: false);
		});
	}

	protected override void OnDestroy()
	{
		button.onClick.RemoveListener(OnClick);
		button.onMouseEnter.RemoveAllListeners();
		button.onMouseExit.RemoveAllListeners();
		base.OnDestroy();
	}

	protected void OnClick()
	{
		if (toggleSelectedOnClick)
		{
			ToggleSelection(!isSelected);
		}
		else
		{
			Select();
		}
	}

	public override void SetSelected(bool selected)
	{
		base.SetSelected(selected);
		if (selectedMask != null)
		{
			selectedMask.enabled = selected;
		}
	}

	public void DisableNavigation()
	{
		button.DisableNavigation();
	}

	public void EnableNavigation(bool select = false)
	{
		button.SetNavigation(Navigation.Mode.Vertical);
		if (select)
		{
			button.Select();
		}
	}

	protected override void OnDisable()
	{
		if (!CoreApplication.IsQuitting)
		{
			DisableNavigation();
			base.OnDisable();
		}
	}
}
