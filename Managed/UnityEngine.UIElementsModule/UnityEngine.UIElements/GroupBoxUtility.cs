using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal static class GroupBoxUtility
{
	private static Dictionary<IGroupBox, IGroupManager> s_GroupManagers = new Dictionary<IGroupBox, IGroupManager>();

	private static Dictionary<IGroupBoxOption, IGroupManager> s_GroupOptionManagerCache = new Dictionary<IGroupBoxOption, IGroupManager>();

	private static readonly Type k_GenericGroupBoxType = typeof(IGroupBox<>);

	public static void RegisterGroupBoxOptionCallbacks<T>(this T option) where T : VisualElement, IGroupBoxOption
	{
		option.RegisterCallback<AttachToPanelEvent>(OnOptionAttachToPanel);
		option.RegisterCallback<DetachFromPanelEvent>(OnOptionDetachFromPanel);
	}

	public static void OnOptionSelected<T>(this T selectedOption) where T : VisualElement, IGroupBoxOption
	{
		if (s_GroupOptionManagerCache.ContainsKey(selectedOption))
		{
			s_GroupOptionManagerCache[selectedOption].OnOptionSelectionChanged(selectedOption);
		}
	}

	public static IGroupBoxOption GetSelectedOption(this IGroupBox groupBox)
	{
		return (!s_GroupManagers.ContainsKey(groupBox)) ? null : s_GroupManagers[groupBox].GetSelectedOption();
	}

	public static IGroupManager GetGroupManager(this IGroupBox groupBox)
	{
		return s_GroupManagers.ContainsKey(groupBox) ? s_GroupManagers[groupBox] : null;
	}

	private static void OnOptionAttachToPanel(AttachToPanelEvent evt)
	{
		VisualElement visualElement = evt.currentTarget as VisualElement;
		IGroupBoxOption groupBoxOption = evt.currentTarget as IGroupBoxOption;
		IGroupManager groupManager = null;
		for (VisualElement parent = visualElement.hierarchy.parent; parent != null; parent = parent.hierarchy.parent)
		{
			if (parent is IGroupBox groupBox)
			{
				groupManager = FindOrCreateGroupManager(groupBox);
				break;
			}
		}
		if (groupManager == null)
		{
			groupManager = FindOrCreateGroupManager(visualElement.elementPanel);
		}
		groupManager.RegisterOption(groupBoxOption);
		s_GroupOptionManagerCache[groupBoxOption] = groupManager;
	}

	private static void OnOptionDetachFromPanel(DetachFromPanelEvent evt)
	{
		IGroupBoxOption groupBoxOption = evt.currentTarget as IGroupBoxOption;
		if (s_GroupOptionManagerCache.ContainsKey(groupBoxOption))
		{
			s_GroupOptionManagerCache[groupBoxOption].UnregisterOption(groupBoxOption);
			s_GroupOptionManagerCache.Remove(groupBoxOption);
		}
	}

	private static IGroupManager FindOrCreateGroupManager(IGroupBox groupBox)
	{
		if (s_GroupManagers.ContainsKey(groupBox))
		{
			return s_GroupManagers[groupBox];
		}
		Type type = null;
		Type[] interfaces = groupBox.GetType().GetInterfaces();
		foreach (Type type2 in interfaces)
		{
			if (type2.IsGenericType && k_GenericGroupBoxType.IsAssignableFrom(type2.GetGenericTypeDefinition()))
			{
				type = type2.GetGenericArguments()[0];
				break;
			}
		}
		object obj;
		if ((object)type == null)
		{
			IGroupManager groupManager = new DefaultGroupManager();
			obj = groupManager;
		}
		else
		{
			obj = (IGroupManager)Activator.CreateInstance(type);
		}
		IGroupManager groupManager2 = (IGroupManager)obj;
		if (groupBox is BaseVisualElementPanel baseVisualElementPanel)
		{
			baseVisualElementPanel.panelDisposed += OnPanelDestroyed;
		}
		else if (groupBox is VisualElement visualElement)
		{
			visualElement.RegisterCallback<DetachFromPanelEvent>(OnGroupBoxDetachedFromPanel);
		}
		s_GroupManagers[groupBox] = groupManager2;
		return groupManager2;
	}

	private static void OnGroupBoxDetachedFromPanel(DetachFromPanelEvent evt)
	{
		s_GroupManagers.Remove(evt.currentTarget as IGroupBox);
	}

	private static void OnPanelDestroyed(BaseVisualElementPanel panel)
	{
		s_GroupManagers.Remove(panel);
		panel.panelDisposed -= OnPanelDestroyed;
	}
}
