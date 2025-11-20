using UnityEngine;
using UnityEngine.EventSystems;

public class LevelEditorDragHandler : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	public static GameObject itemBeingDragged;

	private Vector3 m_StartPosition;

	public void OnBeginDrag(PointerEventData eventData)
	{
		itemBeingDragged = base.gameObject;
		m_StartPosition = base.transform.position;
	}

	public void OnDrag(PointerEventData eventData)
	{
		base.transform.position = InputManager.CursorPosition;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		itemBeingDragged = null;
		base.transform.position = m_StartPosition;
	}
}
