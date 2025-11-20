using System;
using UnityEngine.Pool;
using UnityEngine.UIElements.Experimental;

namespace UnityEngine.UIElements;

internal class ReusableTreeViewItem : ReusableCollectionItem
{
	private Toggle m_Toggle;

	private VisualElement m_Container;

	private VisualElement m_IndentContainer;

	private VisualElement m_BindableContainer;

	private UnityEngine.Pool.ObjectPool<VisualElement> m_IndentPool = new UnityEngine.Pool.ObjectPool<VisualElement>(delegate
	{
		VisualElement visualElement = new VisualElement();
		visualElement.AddToClassList(UnityEngine.UIElements.Experimental.TreeView.itemIndentUssClassName);
		return visualElement;
	});

	protected EventCallback<PointerUpEvent> m_PointerUpCallback;

	protected EventCallback<ChangeEvent<bool>> m_ToggleValueChangedCallback;

	public override VisualElement rootElement => m_Container ?? base.bindableElement;

	public event Action<PointerUpEvent> onPointerUp;

	public event Action<ChangeEvent<bool>> onToggleValueChanged;

	public ReusableTreeViewItem()
	{
		m_PointerUpCallback = OnPointerUp;
		m_ToggleValueChangedCallback = OnToggleValueChanged;
	}

	public override void Init(VisualElement item)
	{
		base.Init(item);
		VisualElement visualElement = new VisualElement();
		visualElement.name = UnityEngine.UIElements.Experimental.TreeView.itemUssClassName;
		visualElement.style.flexDirection = FlexDirection.Row;
		m_Container = visualElement;
		m_Container.AddToClassList(UnityEngine.UIElements.Experimental.TreeView.itemUssClassName);
		VisualElement visualElement2 = new VisualElement();
		visualElement2.name = UnityEngine.UIElements.Experimental.TreeView.itemIndentsContainerUssClassName;
		visualElement2.style.flexDirection = FlexDirection.Row;
		m_IndentContainer = visualElement2;
		m_IndentContainer.AddToClassList(UnityEngine.UIElements.Experimental.TreeView.itemIndentsContainerUssClassName);
		m_Container.hierarchy.Add(m_IndentContainer);
		m_Toggle = new Toggle
		{
			name = UnityEngine.UIElements.Experimental.TreeView.itemToggleUssClassName
		};
		m_Toggle.userData = this;
		m_Toggle.AddToClassList(Foldout.toggleUssClassName);
		m_Toggle.visualInput.AddToClassList(Foldout.inputUssClassName);
		m_Toggle.visualInput.Q(null, Toggle.checkmarkUssClassName).AddToClassList(Foldout.checkmarkUssClassName);
		m_Container.hierarchy.Add(m_Toggle);
		VisualElement visualElement3 = new VisualElement();
		visualElement3.name = UnityEngine.UIElements.Experimental.TreeView.itemContentContainerUssClassName;
		visualElement3.style.flexGrow = 1f;
		m_BindableContainer = visualElement3;
		m_BindableContainer.AddToClassList(UnityEngine.UIElements.Experimental.TreeView.itemContentContainerUssClassName);
		m_Container.Add(m_BindableContainer);
		m_BindableContainer.Add(item);
	}

	public override void PreAttachElement()
	{
		base.PreAttachElement();
		rootElement.AddToClassList(UnityEngine.UIElements.Experimental.TreeView.itemUssClassName);
		m_Container.RegisterCallback(m_PointerUpCallback);
		m_Toggle.RegisterValueChangedCallback(m_ToggleValueChangedCallback);
	}

	public override void DetachElement()
	{
		base.DetachElement();
		rootElement.RemoveFromClassList(UnityEngine.UIElements.Experimental.TreeView.itemUssClassName);
		m_Container.UnregisterCallback(m_PointerUpCallback);
		m_Toggle.UnregisterValueChangedCallback(m_ToggleValueChangedCallback);
	}

	public void Indent(int depth)
	{
		for (int i = 0; i < m_IndentContainer.childCount; i++)
		{
			m_IndentPool.Release(m_IndentContainer[i]);
		}
		m_IndentContainer.Clear();
		for (int j = 0; j < depth; j++)
		{
			VisualElement child = m_IndentPool.Get();
			m_IndentContainer.Add(child);
		}
	}

	public void SetExpandedWithoutNotify(bool expanded)
	{
		m_Toggle.SetValueWithoutNotify(expanded);
	}

	public void SetToggleVisibility(bool visible)
	{
		m_Toggle.visible = visible;
	}

	private void OnPointerUp(PointerUpEvent evt)
	{
		this.onPointerUp?.Invoke(evt);
	}

	private void OnToggleValueChanged(ChangeEvent<bool> evt)
	{
		this.onToggleValueChanged?.Invoke(evt);
	}
}
