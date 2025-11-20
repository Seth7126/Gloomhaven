using UnityEngine;

public class CInteractable : MonoBehaviour, IHoverable
{
	public virtual void ShowNormalInterface(bool disabled)
	{
	}

	public virtual void OnDoubleClicked()
	{
	}

	public virtual void OnCursorEnter()
	{
	}

	public virtual void OnCursorExit()
	{
	}
}
