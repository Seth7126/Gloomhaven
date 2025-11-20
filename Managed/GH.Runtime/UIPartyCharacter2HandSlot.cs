using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPartyCharacter2HandSlot : MonoBehaviour
{
	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private Image buttonImage;

	[SerializeField]
	private List<GameObject> elementsToDisable;

	[SerializeField]
	private List<GameObject> elementsToEnable;

	[SerializeField]
	private UIPartyCharacterEquippementSlot button1;

	[SerializeField]
	private Image button1Image;

	[SerializeField]
	private UIPartyCharacterEquippementSlot button2;

	[SerializeField]
	private Image button2Image;

	private bool isMarked;

	private CanvasGroup _canvasGroup;

	public CanvasGroup CanvasGroup
	{
		get
		{
			if ((object)_canvasGroup == null)
			{
				_canvasGroup = GetComponent<CanvasGroup>();
			}
			return _canvasGroup;
		}
	}

	private void Awake()
	{
		if (!InputManager.GamePadInUse)
		{
			button.onClick.AddListener(Select);
		}
		button.onMouseEnter.AddListener(OnHovered);
		button.onMouseExit.AddListener(OnUnhovered);
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, Select).AddBlocker(new ExtendedButtonSelectKeyActionHandlerBlocker(button)));
	}

	private void OnDestroy()
	{
		if (!InputManager.GamePadInUse)
		{
			button.onClick.RemoveAllListeners();
		}
	}

	private void Select()
	{
		button1.Select();
	}

	public void SetMarked()
	{
		isMarked = true;
	}

	public void Enable(bool enable)
	{
		isMarked = false;
		button.interactable = enable;
		buttonImage.raycastTarget = enable;
		button1.SetInteractable(!enable);
		button1Image.raycastTarget = !enable;
		button2.SetInteractable(!enable);
		button2Image.raycastTarget = !enable;
		for (int i = 0; i < elementsToDisable.Count; i++)
		{
			elementsToDisable[i].SetActive(!enable);
		}
		for (int j = 0; j < elementsToEnable.Count; j++)
		{
			elementsToEnable[j].SetActive(enable);
		}
	}

	private void OnUnhovered()
	{
		if (!isMarked)
		{
			button1.OnPointerExit();
			button2.OnPointerExit();
		}
	}

	private void OnHovered()
	{
		isMarked = false;
		button1.OnPointerEnter();
		button2.OnPointerEnter();
	}
}
