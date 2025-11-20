using System.Threading.Tasks;
using AsmodeeNet.Foundation;
using Assets.Script.Checkers;
using JetBrains.Annotations;
using UnityEngine;

public class UIMenuOptionToggle : UIMenuOption
{
	[SerializeField]
	protected ExtendedToggle toggle;

	[Header("Hover")]
	[SerializeField]
	private RectTransform hoverRect;

	[SerializeField]
	private Vector2 hoverDisplacement;

	[SerializeField]
	private bool _useStartAnchoredPosition;

	[Header("Check")]
	[Tooltip("Is some check needed befor OnToggleValueChanged invoke")]
	[SerializeField]
	private bool _needCheck;

	[SerializeField]
	private CheckerType _checkerType = CheckerType.AlwaysOk;

	private Vector2 _startAnchoredPosition;

	private static Vector2 zero = Vector2.zero;

	private IChecker _checker;

	[UsedImplicitly]
	private void Awake()
	{
		if (_useStartAnchoredPosition)
		{
			_startAnchoredPosition = hoverRect.anchoredPosition;
			hoverDisplacement += hoverRect.anchoredPosition;
		}
		toggle.onMouseEnter.AddListener(delegate
		{
			OnHovered(hovered: true);
		});
		toggle.onMouseExit.AddListener(delegate
		{
			OnHovered(hovered: false);
		});
		toggle.onValueChanged.AddListener(delegate(bool isOn)
		{
			OnToggleValueChanged(isOn);
		});
		_checker = CheckerFactory.Create(_checkerType);
	}

	private async void OnToggleValueChanged(bool isOn)
	{
		if (!isOn || await CheckSucсess())
		{
			if (isOn)
			{
				base.Select();
			}
			else
			{
				base.Deselect();
			}
		}
		async Task<bool> CheckSucсess()
		{
			if (_needCheck)
			{
				if (_checker.Type != _checkerType)
				{
					_checker = CheckerFactory.Create(_checkerType);
				}
				if (!(await _checker.IsOk()))
				{
					return false;
				}
				_needCheck = false;
				return true;
			}
			return true;
		}
	}

	[UsedImplicitly]
	protected override void OnDestroy()
	{
		base.OnDestroy();
		toggle.onMouseEnter.RemoveAllListeners();
		toggle.onMouseExit.RemoveAllListeners();
		toggle.onValueChanged.RemoveAllListeners();
	}

	protected override void OnHovered(bool hovered)
	{
		if (_useStartAnchoredPosition)
		{
			hoverRect.anchoredPosition = (hovered ? hoverDisplacement : _startAnchoredPosition);
		}
		else
		{
			hoverRect.anchoredPosition = (hovered ? hoverDisplacement : zero);
		}
		base.OnHovered(hovered);
	}

	public override void Select()
	{
		toggle.isOn = true;
	}

	public override void Deselect()
	{
		toggle.isOn = false;
	}

	public override void SetSelected(bool isSelected)
	{
		base.SetSelected(isSelected);
		toggle.SetValue(isSelected);
	}

	public void EnableNavigation()
	{
	}

	public void DisableNavigation()
	{
		toggle.DisableNavigation();
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
