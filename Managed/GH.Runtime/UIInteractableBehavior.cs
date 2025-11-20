using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIInteractableBehavior : UIBehaviour
{
	private Canvas m_Canvas;

	private readonly List<CanvasGroup> m_CanvasGroupCache = new List<CanvasGroup>();

	public bool IsInteractable { get; private set; }

	protected override void OnCanvasHierarchyChanged()
	{
		base.OnCanvasHierarchyChanged();
		if (base.isActiveAndEnabled)
		{
			Refresh();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		CalculateCanvas();
		Refresh();
	}

	protected override void OnCanvasGroupChanged()
	{
		base.OnCanvasGroupChanged();
		if (base.isActiveAndEnabled)
		{
			Refresh();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (IsInteractable)
		{
			SetInteractable(interactable: false);
		}
	}

	private void Refresh()
	{
		if (m_Canvas == null)
		{
			CalculateCanvas();
		}
		if (m_Canvas == null || !m_Canvas.isActiveAndEnabled)
		{
			SetInteractable(interactable: false);
			return;
		}
		Transform parent = base.transform;
		while (parent != null)
		{
			parent.GetComponents(m_CanvasGroupCache);
			for (int i = 0; i < m_CanvasGroupCache.Count; i++)
			{
				if (!m_CanvasGroupCache[i].interactable || !m_CanvasGroupCache[i].blocksRaycasts)
				{
					SetInteractable(interactable: false);
					return;
				}
				if (m_CanvasGroupCache[i].ignoreParentGroups)
				{
					break;
				}
			}
			parent = parent.parent;
		}
		SetInteractable(interactable: true);
	}

	private void SetInteractable(bool interactable)
	{
		if (IsInteractable != interactable)
		{
			IsInteractable = interactable;
			OnInteractionChanged(IsInteractable);
		}
	}

	protected virtual void OnInteractionChanged(bool interactable)
	{
	}

	protected override void OnTransformParentChanged()
	{
		base.OnTransformParentChanged();
		if (base.isActiveAndEnabled)
		{
			CalculateCanvas();
			Refresh();
		}
	}

	private void CalculateCanvas()
	{
		m_Canvas = GetComponentInParent<Canvas>();
	}
}
