using System;
using UnityEngine.UIElements.Experimental;

namespace UnityEngine.UIElements;

internal class ReusableCollectionItem
{
	public const int UndefinedIndex = -1;

	protected EventCallback<GeometryChangedEvent> m_GeometryChangedEventCallback;

	public virtual VisualElement rootElement => bindableElement;

	public VisualElement bindableElement { get; protected set; }

	public ValueAnimation<StyleValues> animator { get; set; }

	public int index { get; set; }

	public int id { get; set; }

	public event Action<ReusableCollectionItem> onGeometryChanged;

	public ReusableCollectionItem()
	{
		index = (id = -1);
		m_GeometryChangedEventCallback = OnGeometryChanged;
	}

	public virtual void Init(VisualElement item)
	{
		bindableElement = item;
	}

	public virtual void PreAttachElement()
	{
		rootElement.AddToClassList(BaseVerticalCollectionView.itemUssClassName);
		rootElement.RegisterCallback(m_GeometryChangedEventCallback);
	}

	public virtual void DetachElement()
	{
		rootElement.RemoveFromClassList(BaseVerticalCollectionView.itemUssClassName);
		rootElement.UnregisterCallback(m_GeometryChangedEventCallback);
		rootElement?.RemoveFromHierarchy();
		SetSelected(selected: false);
		int num = (id = -1);
		index = num;
	}

	public virtual void SetSelected(bool selected)
	{
		if (selected)
		{
			rootElement.AddToClassList(BaseVerticalCollectionView.itemSelectedVariantUssClassName);
			rootElement.pseudoStates |= PseudoStates.Checked;
		}
		else
		{
			rootElement.RemoveFromClassList(BaseVerticalCollectionView.itemSelectedVariantUssClassName);
			rootElement.pseudoStates &= ~PseudoStates.Checked;
		}
	}

	protected void OnGeometryChanged(GeometryChangedEvent evt)
	{
		this.onGeometryChanged?.Invoke(this);
	}

	protected internal VisualElement GetRootElement()
	{
		return rootElement;
	}
}
