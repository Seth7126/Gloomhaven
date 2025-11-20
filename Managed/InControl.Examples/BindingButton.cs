using System.Collections;
using InControl;
using UnityEngine;
using UnityEngine.UI;

public class BindingButton : MonoBehaviour
{
	private PlayerAction action;

	private CanvasGroup canvasGroup;

	private Button button;

	private Text bindingText;

	public void Setup(PlayerAction action, CanvasGroup canvasGroup)
	{
		this.action = action;
		this.canvasGroup = canvasGroup;
		button = GetComponent<Button>();
		button.onClick.AddListener(OnClick);
		base.transform.Find("Control").GetComponent<Text>().text = action.Name;
		bindingText = base.transform.Find("Binding").GetComponent<Text>();
		bindingText.text = GetBindingName();
		action.ListenOptions = new BindingListenOptions
		{
			MaxAllowedBindings = 1u,
			UnsetDuplicateBindingsOnSet = true,
			IncludeModifiersAsFirstClassKeys = true,
			OnBindingFound = OnBindingFound,
			OnBindingRejected = OnBindingRejected,
			OnBindingEnded = OnBindingEnded
		};
		action.OnBindingsChanged -= OnBindingsChanged;
		action.OnBindingsChanged += OnBindingsChanged;
	}

	private void OnClick()
	{
		if (!action.IsListeningForBinding)
		{
			canvasGroup.interactable = false;
			action.ListenForBinding();
			StartCoroutine(AnimateButtonText());
			StartCoroutine(AnimateButtonColor());
		}
	}

	private void Update()
	{
		if (MenuManager.Instance.MenuActions.Cancel.IsPressed)
		{
			action.StopListeningForBinding();
		}
	}

	private static bool OnBindingFound(PlayerAction action, BindingSource binding)
	{
		if (MenuManager.Instance.MenuActions.Cancel.IsPressed)
		{
			action.StopListeningForBinding();
			return false;
		}
		if (binding == new KeyBindingSource(Key.Escape))
		{
			action.StopListeningForBinding();
			return false;
		}
		return true;
	}

	private static void OnBindingRejected(PlayerAction action, BindingSource binding, BindingSourceRejectionType rejectionType)
	{
		action.StopListeningForBinding();
	}

	private void OnBindingEnded(PlayerAction action)
	{
		bindingText.text = GetBindingName();
		canvasGroup.interactable = true;
	}

	private void OnBindingsChanged()
	{
		bindingText.text = GetBindingName();
	}

	private string GetBindingName()
	{
		if (action.Bindings.Count > 0)
		{
			return action.Bindings[0].Name;
		}
		return "N/A";
	}

	public IEnumerator AnimateButtonText()
	{
		while (action.IsListeningForBinding)
		{
			int count = Mathf.FloorToInt(Time.realtimeSinceStartup * 5f % 4f);
			bindingText.text = new string('.', count);
			yield return new WaitForEndOfFrame();
		}
	}

	public IEnumerator AnimateButtonColor()
	{
		ColorBlock savedColors = button.colors;
		while (action.IsListeningForBinding)
		{
			ColorBlock colors = button.colors;
			colors.disabledColor = new Color(1f, 1f, 1f, Mathf.Sin(Time.realtimeSinceStartup * 8f) * 0.1f + 0.6f);
			button.colors = colors;
			yield return new WaitForEndOfFrame();
		}
		button.colors = savedColors;
	}
}
