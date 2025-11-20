using System;
using System.Text;
using AsmodeeNet.Utils.Extensions;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(ClientButtonLocker))]
public class EventButton : MonoBehaviour
{
	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private GUIAnimator revealTextAnimator;

	[SerializeField]
	private Color normalTextColor;

	[SerializeField]
	private Color disabledTextColor;

	private Action onButtonClick;

	private ClientButtonLocker clientButtonLocker;

	private Action<bool> onHovered;

	private SimpleKeyActionHandlerBlocker selectBlocker;

	private SimpleKeyActionHandlerBlocker enabledBlocker;

	private SimpleKeyActionHandlerBlocker clickBlocker;

	public int ID { get; set; }

	public string Name { get; set; }

	private void Awake()
	{
		selectBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);
		enabledBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);
		clickBlocker = new SimpleKeyActionHandlerBlocker(isBlock: true);
		clientButtonLocker = GetComponent<ClientButtonLocker>();
		Singleton<KeyActionHandlerController>.Instance.AddHandler(new KeyActionHandler(KeyAction.UI_SUBMIT, Click).AddBlocker(selectBlocker).AddBlocker(clickBlocker).AddBlocker(new ExtendedButtonActiveKeyActionHandlerBlocker(button))
			.AddBlocker(enabledBlocker));
	}

	private void OnEnable()
	{
		if (!InputManager.GamePadInUse)
		{
			button.onClick.AddListener(Click);
		}
		button.onMouseEnter.AddListener(OnHovered);
		button.onMouseExit.AddListener(OnUnhovered);
		enabledBlocker.SetBlock(value: false);
	}

	private void OnDisable()
	{
		revealTextAnimator.Stop();
		if (!InputManager.GamePadInUse)
		{
			button.onClick.RemoveListener(Click);
		}
		button.onMouseEnter.RemoveListener(OnHovered);
		button.onMouseExit.RemoveListener(OnUnhovered);
		enabledBlocker.SetBlock(value: true);
	}

	private void Init(int id, string optionName, string primaryText, string secondaryText, Action onButtonClick, Action<bool> onHovered, string unavailableText = null, bool isVisible = true, string overrideClickAudioItem = null)
	{
		ID = id;
		Name = optionName;
		text.text = BuildText(primaryText, secondaryText, unavailableText);
		this.onButtonClick = onButtonClick;
		this.onHovered = onHovered;
		button.mouseClickAudioItem = overrideClickAudioItem;
		if (isVisible)
		{
			ShowAnswer(instant: false);
		}
		else
		{
			HideAnswer();
		}
		base.gameObject.SetActive(value: true);
		clientButtonLocker.Initialize(button);
		clickBlocker.SetBlock(value: false);
	}

	public void SetAvailableOption(int id, string optionName, string primaryText, string secondaryText, Action onButtonClick, Action<bool> onHovered, bool isVisible = true, string overrideClickAudioItem = null)
	{
		SetAvailable(available: true);
		Init(id, optionName, primaryText, secondaryText, onButtonClick, onHovered, null, isVisible, overrideClickAudioItem);
	}

	public void SetUnvailableOption(int id, string optionName, string primaryText, string secondaryText, Action onButtonClick, Action<bool> onHovered, string unavailableText, bool isVisible = true)
	{
		SetAvailable(available: false);
		Init(id, optionName, primaryText, secondaryText, onButtonClick, onHovered, unavailableText, isVisible);
	}

	private void OnHovered()
	{
		onHovered?.Invoke(obj: true);
		selectBlocker.SetBlock(value: false);
	}

	private void OnUnhovered()
	{
		onHovered?.Invoke(obj: false);
		selectBlocker.SetBlock(value: true);
	}

	public void HideAnswer()
	{
		revealTextAnimator.Stop();
		revealTextAnimator.GoInitState();
	}

	public void ShowAnswer(bool instant = true)
	{
		revealTextAnimator.Stop();
		if (instant)
		{
			revealTextAnimator.GoToFinishState();
		}
		else
		{
			revealTextAnimator.Play();
		}
	}

	private string BuildText(string primaryText, string secondaryText, string unavailableText)
	{
		StringBuilder stringBuilder = new StringBuilder(primaryText);
		if (secondaryText.IsNOTNullOrEmpty())
		{
			stringBuilder.AppendLine();
			stringBuilder.Append(secondaryText);
		}
		if (unavailableText.IsNOTNullOrEmpty())
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("<color=#{1}>{0}</color>", unavailableText, UIInfoTools.Instance.warningColor.ToHex());
		}
		return stringBuilder.ToString();
	}

	private void SetAvailable(bool available)
	{
		button.interactable = available;
		text.color = (available ? normalTextColor : disabledTextColor);
	}

	public void Click()
	{
		clickBlocker.SetBlock(value: true);
		onButtonClick?.Invoke();
	}

	private void OnDestroy()
	{
		Singleton<KeyActionHandlerController>.Instance?.RemoveHandler(KeyAction.UI_SUBMIT, Click);
	}
}
