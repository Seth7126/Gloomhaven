using UnityEngine.Events;

public class DialogOption
{
	public UnityAction onMouseClickAction;

	public UnityAction onMouseEnterAction;

	public UnityAction onMouseExitAction;

	public KeyAction keyAction;

	public string text;

	private DialogOption()
	{
	}

	public DialogOption(string text, KeyAction keyAction, UnityAction action = null)
	{
		onMouseClickAction = action;
		this.text = text;
		onMouseEnterAction = null;
		onMouseExitAction = null;
		this.keyAction = keyAction;
	}

	public DialogOption(string text, KeyAction keyAction, UnityAction onClick, UnityAction onEnter, UnityAction onExit)
	{
		onMouseClickAction = onClick;
		onMouseEnterAction = onEnter;
		onMouseExitAction = onExit;
		this.text = text;
		this.keyAction = keyAction;
	}
}
