using System;
using System.Collections.Generic;
using System.Linq;
using Script.GUI.SMNavigation.States.PopupStates;
using SpriteMemoryManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDistributePointsSlot : MonoBehaviour
{
	[SerializeField]
	private ExtendedButton slotButton;

	[SerializeField]
	private Image portrait;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private Color defaultTitleColor;

	[SerializeField]
	private UIDistributePointsCounter slider;

	[SerializeField]
	private ExtendedButton addPointButton;

	[SerializeField]
	private ExtendedButton removePointButton;

	[SerializeField]
	private Image removedAllPointsHighlight;

	[SerializeField]
	private Color removedAllPointsTitleColor;

	[SerializeField]
	private TextMeshProUGUI optionText;

	[SerializeField]
	private List<UIDistributePointsActorInfo> extraInfo;

	[Header("Utilites")]
	[SerializeField]
	private ImageSpriteLoader _imageSpriteLoader;

	[SerializeField]
	private List<Graphic> _selectedImages;

	[SerializeField]
	private List<TextMeshProUGUI> _selectedText;

	[SerializeField]
	private Color _unselectedTextColor;

	private Action onAddedPoint;

	private Action onRemovedPoint;

	private Action<bool> onHoveredSlot;

	private int currentPoints;

	private bool m_IsRewards;

	private List<Color> _textColors;

	private Action<Action> _selectWrapper;

	public IDistributePointsActor Actor { get; private set; }

	public UIDistributePointsCounter Counter => slider;

	public Selectable Selectable => slotButton;

	public ExtendedButton Button => slotButton;

	public PopupStateTag StateTag { get; set; }

	public bool IsSelected { get; private set; }

	public bool IsHoverd { get; private set; }

	private void Awake()
	{
		addPointButton.onClick.AddListener(AddPointDelegate);
		removePointButton.onClick.AddListener(RemovePointDelegate);
		_textColors = _selectedText.Select((TextMeshProUGUI text) => text.color).ToList();
		if (slotButton == null)
		{
			slotButton = GetComponent<ExtendedButton>();
		}
	}

	public void Subscribe()
	{
		slotButton.onMouseEnter.AddListener(delegate
		{
			OnHoveredSlot(hovered: true);
		});
		slotButton.onMouseExit.AddListener(delegate
		{
			OnHoveredSlot(hovered: false);
		});
	}

	public void Unsubscribe()
	{
		slotButton.onMouseEnter.RemoveAllListeners();
		slotButton.onMouseExit.RemoveAllListeners();
	}

	private void OnDestroy()
	{
		addPointButton.onClick.RemoveAllListeners();
		removePointButton.onClick.RemoveAllListeners();
		Unsubscribe();
	}

	public virtual void Init(IDistributePointsActor actor, int maxPoints, int currentPoints, Action onAddedPoint, Action onRemovedPoint, Action<bool> onHoveredSlot, int assignedPoints = 0, bool isRewards = false, string optionName = null)
	{
		this.onAddedPoint = onAddedPoint;
		this.onRemovedPoint = onRemovedPoint;
		this.onHoveredSlot = onHoveredSlot;
		this.currentPoints = currentPoints;
		Actor = actor;
		titleText.text = actor.Name;
		if (optionText != null)
		{
			optionText.text = optionName;
		}
		_imageSpriteLoader.AddReferenceToSpriteForImage(portrait, actor.Portrait);
		slider?.Setup(maxPoints, currentPoints);
		for (int i = 0; i < extraInfo.Count; i++)
		{
			extraInfo[i].Display(actor);
		}
		RefreshAssignedPoints(assignedPoints);
		EnableAddPoints(enable: false);
		EnableRemovePoints(enable: false);
		m_IsRewards = isRewards;
	}

	private void RemovePointDelegate()
	{
		RemovePoint(isProxyCall: false);
	}

	private void AddPointDelegate()
	{
		AddPoint(isProxyCall: false);
	}

	public void RemovePoint(bool isProxyCall)
	{
		if (isProxyCall || removePointButton.interactable)
		{
			onRemovedPoint?.Invoke();
		}
	}

	public void AddPoint(bool isProxyCall)
	{
		if (isProxyCall || addPointButton.interactable)
		{
			onAddedPoint?.Invoke();
		}
	}

	public void Release()
	{
		_imageSpriteLoader.Release();
	}

	public void SetClickWrapper(Action<Action> wrapper)
	{
		_selectWrapper = wrapper;
	}

	public void ResetClickWrapper()
	{
		_selectWrapper = null;
	}

	public void OnClick()
	{
		if (_selectWrapper != null)
		{
			_selectWrapper?.Invoke(ProcessClick);
		}
		else
		{
			ProcessClick();
		}
	}

	private void ProcessClick()
	{
		UnityEngine.Debug.Log("[DistributePopup] slot clicked");
		if (IsGamePadMode())
		{
			if (!IsSelected)
			{
				IsSelected = true;
				AddPointDelegate();
			}
			else
			{
				IsSelected = false;
				RemovePointDelegate();
			}
		}
	}

	private bool IsGamePadMode()
	{
		if (InputManager.GamePadInUse)
		{
			return StateTag != PopupStateTag.DistributeGoldRewards;
		}
		return false;
	}

	private void OnHoveredSlot(bool hovered)
	{
		IsHoverd = hovered;
		onHoveredSlot?.Invoke(hovered);
	}

	public void RefreshAssignedPoints(int points)
	{
		slider.SetExtendedPoints(points);
		IsSelected = points > 0;
		if (removedAllPointsHighlight != null)
		{
			removedAllPointsHighlight.enabled = currentPoints + points == 0;
		}
		titleText.color = ((currentPoints + points == 0) ? removedAllPointsTitleColor : defaultTitleColor);
		for (int i = 0; i < extraInfo.Count; i++)
		{
			extraInfo[i].RefreshAssignedPoints(currentPoints, points);
		}
	}

	public void EnableAddPoints(bool enable)
	{
		if (!m_IsRewards || !FFSNetwork.IsOnline || FFSNetwork.IsHost)
		{
			addPointButton.interactable = enable;
			addPointButton.ToggleFade(!enable);
		}
		else
		{
			addPointButton.interactable = false;
		}
	}

	public void EnableRemovePoints(bool enable)
	{
		if (!m_IsRewards || !FFSNetwork.IsOnline || FFSNetwork.IsHost)
		{
			removePointButton.interactable = enable;
			removePointButton.ToggleFade(!enable);
		}
		else
		{
			removePointButton.interactable = false;
		}
	}

	public void RefreshGrayHighlight(bool isHighlighted)
	{
		_selectedImages.ForEach(delegate(Graphic mask)
		{
			mask.material = (isHighlighted ? null : UIInfoTools.Instance.greyedOutMaterial);
		});
		for (int num = 0; num < _selectedText.Count; num++)
		{
			_selectedText[num].color = (isHighlighted ? _textColors[num] : _unselectedTextColor);
		}
	}
}
