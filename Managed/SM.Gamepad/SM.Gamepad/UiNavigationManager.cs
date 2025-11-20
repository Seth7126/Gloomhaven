#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SM.Utils;
using UnityEngine.EventSystems;

namespace SM.Gamepad;

public class UiNavigationManager : IUiNavigationManager
{
	private readonly IUIActionsInput _uiActionsInput;

	private IUiNavigationRoot _currentRoot;

	private IUiNavigationSelectable _currentSelectable;

	private Dictionary<IUiNavigationSelectable, List<IUiNavigationNode>> _knownSelectablesMap = new Dictionary<IUiNavigationSelectable, List<IUiNavigationNode>>();

	private List<IUiNavigationRoot> _knownRoots = new List<IUiNavigationRoot>();

	private Dictionary<IUiNavigationRoot, HashSet<IUiNavigationElement>> _rootElements = new Dictionary<IUiNavigationRoot, HashSet<IUiNavigationElement>>();

	private Dictionary<IUiNavigationRoot, HashSet<IUiNavigationSelectable>> _rootSelectables = new Dictionary<IUiNavigationRoot, HashSet<IUiNavigationSelectable>>();

	private Dictionary<string, IUiNavigationElement> _elementsByNameMap = new Dictionary<string, IUiNavigationElement>();

	private Dictionary<IUiNavigationNode, IUiNavigationSelectable> _previouslySelectedMap = new Dictionary<IUiNavigationNode, IUiNavigationSelectable>();

	private Dictionary<IUiNavigationNode, IUiNavigationElement> _previouslyPickedMap = new Dictionary<IUiNavigationNode, IUiNavigationElement>();

	private Dictionary<string, List<IUiNavigationElement>> _taggedElementsMap = new Dictionary<string, List<IUiNavigationElement>>();

	private HashSet<IUiNavigationNode> _proceededNavigationNodes = new HashSet<IUiNavigationNode>();

	private List<UiNavigationBlocker> _navigationBlockers = new List<UiNavigationBlocker>();

	private HashSet<IUiNavigationElement> _elementsBuffer = new HashSet<IUiNavigationElement>();

	private Dictionary<IUiNavigationSelectable, List<IUiNavigationNode>> _knownSelectablesMapBuffer = new Dictionary<IUiNavigationSelectable, List<IUiNavigationNode>>();

	private Dictionary<string, IUiNavigationElement> _elementsByNameMapBuffer = new Dictionary<string, IUiNavigationElement>();

	internal static UiNavigationManager Instance { get; private set; }

	private IUiNavigationNode CurrentNode => _currentSelectable?.Parent;

	public UiNavigationRoot CurrentNavigationRoot => _currentRoot as UiNavigationRoot;

	public IUiNavigationSelectable CurrentlySelectedElement => _currentSelectable;

	public bool SetRootsOnEnable { get; set; } = true;

	public event Action<IUiNavigationSelectable> OnElementSelectedEvent;

	public event Action OnTrySelectPreviousEvent;

	public UiNavigationManager(IUIActionsInput uiActionsInput)
	{
		_uiActionsInput = uiActionsInput;
		Instance = this;
		_uiActionsInput.MoveSelectionEvent += OnMoveSelectionInput;
	}

	public void DeInit()
	{
		_uiActionsInput.MoveSelectionEvent -= OnMoveSelectionInput;
	}

	public void RefreshCurrentSelectable()
	{
		if (_currentSelectable != null)
		{
			InnerSelect(_currentSelectable);
		}
	}

	public IUiNavigationRoot RootByName(string navigationName)
	{
		if (!_elementsByNameMap.ContainsKey(navigationName) || !(_elementsByNameMap[navigationName] is IUiNavigationRoot))
		{
			LogUtils.LogWarning("[UiNavigationManager] There isn't known root with navigation name \"" + navigationName + "\";");
			return null;
		}
		return _elementsByNameMap[navigationName] as IUiNavigationRoot;
	}

	public IUiNavigationElement ElementByName(string navigationName)
	{
		if (!_elementsByNameMap.ContainsKey(navigationName))
		{
			LogUtils.LogWarning("[UiNavigationManager] There isn't known element with navigation name \"" + navigationName + "\";");
			return null;
		}
		return _elementsByNameMap[navigationName];
	}

	public List<IUiNavigationNode> PathToSelectable(IUiNavigationSelectable uiNavigationSelectable)
	{
		if (_knownSelectablesMap.ContainsKey(uiNavigationSelectable))
		{
			return _knownSelectablesMap[uiNavigationSelectable];
		}
		LogUtils.LogWarning("[UiNavigationManager] Trying to get path to unknown selectable \"" + uiNavigationSelectable?.GameObject.name + "(" + uiNavigationSelectable?.NavigationName + ")\". You should add it to navigation system first");
		return null;
	}

	public IUiNavigationSelectable GetPreviouslySelectedIn(IUiNavigationNode navigationNode)
	{
		if (!_previouslySelectedMap.ContainsKey(navigationNode))
		{
			return null;
		}
		return _previouslySelectedMap[navigationNode];
	}

	public IUiNavigationElement GetPreviouslyPickedIn(IUiNavigationNode navigationNode)
	{
		if (!_previouslyPickedMap.ContainsKey(navigationNode))
		{
			return null;
		}
		return _previouslyPickedMap[navigationNode];
	}

	public bool SetCurrentRoot(string navigationName, bool selectFirst = true, IUiNavigationSelectable selectConcrete = null)
	{
		LogUtils.Log("[UiNavigationManager] Trying to set current root to \"" + navigationName + "\" ...");
		IUiNavigationRoot root = RootByName(navigationName);
		return SetCurrentRoot(root, selectFirst, selectConcrete);
	}

	public bool SetCurrentRoot(IUiNavigationRoot root, bool selectFirst = true, IUiNavigationSelectable selectConcrete = null)
	{
		if (root == null)
		{
			LogUtils.LogWarning("[UiNavigationManager] Trying to set root to NULL => deselecting all");
			DeselectAll();
			return true;
		}
		LogUtils.Log("[UiNavigationManager] Trying to set current root to " + root.GameObject.name + "(\"" + root.NavigationName + "\") ...");
		_currentRoot = root;
		if (selectFirst)
		{
			return TrySelectFirstIn(_currentRoot);
		}
		if (selectConcrete != null)
		{
			return TrySelectConcreteIn(_currentRoot, selectConcrete);
		}
		return TrySelectPreviousIn(_currentRoot);
	}

	public bool TrySelect(IUiNavigationSelectable uiNavigationSelectable)
	{
		if (uiNavigationSelectable == null)
		{
			LogUtils.LogWarning("[UiNavigationManager] Trying to jump to NULL element => deselect current selectable");
			DeselectCurrentSelectable();
			return true;
		}
		LogUtils.Log("[UiNavigationManager] Trying to select " + uiNavigationSelectable.GameObject.name + "(\"" + uiNavigationSelectable.NavigationName + "\") ...");
		if (!_knownSelectablesMap.ContainsKey(uiNavigationSelectable))
		{
			LogUtils.LogWarning("[UiNavigationManager] Trying to jump to unknown selectable. You should add it to navigation system first");
			return false;
		}
		return InnerSelect(uiNavigationSelectable);
	}

	public bool TrySelectFirstIn(IUiNavigationNode navigationNode)
	{
		if (navigationNode == null)
		{
			LogUtils.LogWarning("[UiNavigationManager] Trying to select first element in NULL node");
			return false;
		}
		LogUtils.Log("[UiNavigationManager] Trying to select first in " + navigationNode.GameObject.name + "(\"" + navigationNode.NavigationName + "\") ...");
		if (navigationNode.Elements.Count == 0)
		{
			LogUtils.LogWarning("[UiNavigationManager] Node " + navigationNode.GameObject.name + "(\"" + navigationNode.NavigationName + "\") doesn't have any elements");
			return false;
		}
		IUiNavigationSelectable uiNavigationSelectable = ((!navigationNode.AutoSelectFirstElement && navigationNode.DefaultElementToSelect != null) ? navigationNode.DefaultElementToSelect : FindFirstSelectable(navigationNode));
		return TrySelect(uiNavigationSelectable);
	}

	public bool TrySelectIn(IUiNavigationNode navigationNode, IUiNavigationSelectable uiNavigationSelectable)
	{
		if (navigationNode == null)
		{
			LogUtils.LogWarning("[UiNavigationManager] Trying to select first element in NULL node");
			return false;
		}
		LogUtils.Log("[UiNavigationManager] Trying to select first in " + navigationNode.GameObject.name + "(\"" + navigationNode.NavigationName + "\") ...");
		if (navigationNode.Elements.Count == 0)
		{
			LogUtils.LogWarning("[UiNavigationManager] Node " + navigationNode.GameObject.name + "(\"" + navigationNode.NavigationName + "\") doesn't have any elements");
			return false;
		}
		return TrySelect(uiNavigationSelectable);
	}

	public bool TrySelectConcreteIn(IUiNavigationNode navigationNode, IUiNavigationSelectable selectable)
	{
		if (navigationNode == null)
		{
			LogUtils.LogWarning("[UiNavigationManager] Trying to select concrete element in NULL node");
			return false;
		}
		LogUtils.Log("[UiNavigationManager] Trying to select concrete in " + navigationNode.GameObject.name + "(\"" + navigationNode.NavigationName + "\") ...");
		if (navigationNode.Elements.Count == 0)
		{
			LogUtils.LogWarning("[UiNavigationManager] Node " + navigationNode.GameObject.name + "(\"" + navigationNode.NavigationName + "\") doesn't have any elements");
			return false;
		}
		IUiNavigationSelectable uiNavigationSelectable = FindConcreteSelectable(navigationNode, selectable);
		return TrySelect(uiNavigationSelectable);
	}

	private IUiNavigationSelectable FindConcreteSelectable(IUiNavigationNode navigationNode, IUiNavigationSelectable concreteSelectable, bool ignoreInteractable = true)
	{
		foreach (IUiNavigationElement element in navigationNode.Elements)
		{
			if (!element.GameObject.activeInHierarchy)
			{
				continue;
			}
			if (element is IUiNavigationSelectable uiNavigationSelectable)
			{
				if (uiNavigationSelectable == concreteSelectable && (ignoreInteractable || uiNavigationSelectable.ControlledSelectable.interactable))
				{
					return uiNavigationSelectable;
				}
			}
			else if (element is IUiNavigationNode navigationNode2)
			{
				IUiNavigationSelectable uiNavigationSelectable2 = FindConcreteSelectable(navigationNode2, concreteSelectable);
				if (uiNavigationSelectable2 != null)
				{
					return uiNavigationSelectable2;
				}
			}
		}
		return null;
	}

	public bool TrySelectPreviousIn(IUiNavigationNode navigationNode)
	{
		if (navigationNode == null)
		{
			LogUtils.LogWarning("[UiNavigationManager] Trying to select previously selected element in NULL node");
			return false;
		}
		LogUtils.Log("[UiNavigationManager] Trying to select previously selected element in " + navigationNode.GameObject.name + "(\"" + navigationNode.NavigationName + "\") ...");
		IUiNavigationSelectable uiNavigationSelectable = (_previouslySelectedMap.ContainsKey(navigationNode) ? _previouslySelectedMap[navigationNode] : null);
		if (uiNavigationSelectable == null || !uiNavigationSelectable.GameObject.activeInHierarchy || !uiNavigationSelectable.ControlledSelectable.interactable)
		{
			uiNavigationSelectable = FindFirstSelectable(navigationNode);
		}
		bool num = TrySelect(uiNavigationSelectable);
		if (num)
		{
			Action action = this.OnTrySelectPreviousEvent;
			if (action == null)
			{
				return num;
			}
			action();
		}
		return num;
	}

	public void DeselectCurrentSelectable()
	{
		LogUtils.Log("[UiNavigationManager] Deselecting current selectable ...");
		InnerDeselectCurrentSelectable();
	}

	public void DeselectAll()
	{
		LogUtils.Log("[UiNavigationManager] Deselecting all ...");
		InnerDeselectCurrentRoot();
	}

	public void BlockNavigation(UiNavigationBlocker blocker)
	{
		if (blocker == null)
		{
			throw new Exception("[UiNavigationManager] NULL is can't be used as blocker");
		}
		if (_navigationBlockers.Contains(blocker))
		{
			LogUtils.LogWarning("[UiNavigationManager] Navigation already blocked by \"" + blocker.Tag + "\"");
			return;
		}
		LogUtils.Log("[UiNavigationManager] Navigation blocked by \"" + blocker.Tag + "\"");
		_navigationBlockers.Add(blocker);
	}

	public void UnblockNavigation(UiNavigationBlocker blocker)
	{
		if (blocker == null)
		{
			throw new Exception("[UiNavigationManager] NULL is can't be used as blocker");
		}
		if (!_navigationBlockers.Contains(blocker))
		{
			LogUtils.LogWarning("[UiNavigationManager] Navigation was not blocked by \"" + blocker.Tag + "\"");
			return;
		}
		LogUtils.Log("[UiNavigationManager] Navigation unblocked by \"" + blocker.Tag + "\"");
		_navigationBlockers.Remove(blocker);
	}

	public void GetTaggedElements(string navigationTag, List<IUiNavigationElement> result, Func<IUiNavigationElement, bool> additionalFilter = null)
	{
		result.Clear();
		if (!_taggedElementsMap.ContainsKey(navigationTag))
		{
			_taggedElementsMap.Add(navigationTag, new List<IUiNavigationElement>());
		}
		foreach (IUiNavigationElement item in _taggedElementsMap[navigationTag])
		{
			if (additionalFilter == null || additionalFilter(item))
			{
				result.Add(item);
			}
		}
	}

	private void InitRootElement(IUiNavigationRoot rootElement)
	{
		UiNavigationElementsUtils.InsertElementsByPriorityDescending(_knownRoots, rootElement);
		HashSet<IUiNavigationElement> value = new HashSet<IUiNavigationElement>();
		_rootElements.Add(rootElement, value);
		Dictionary<IUiNavigationSelectable, List<IUiNavigationNode>> dictionary = new Dictionary<IUiNavigationSelectable, List<IUiNavigationNode>>();
		_rootSelectables.Add(rootElement, new HashSet<IUiNavigationSelectable>(dictionary.Keys));
		rootElement.OnRootElementEnabledEvent += OnUiNavigationRootElementEnabled;
		rootElement.OnRootElementDisabledEvent += OnUiNavigationRootElementDisabled;
		AddElementInternal(rootElement, rootElement);
	}

	private void DeInitRootElement(IUiNavigationRoot rootElement)
	{
		if (_currentRoot == rootElement)
		{
			InnerDeselectCurrentRoot();
		}
		rootElement.OnRootElementEnabledEvent -= OnUiNavigationRootElementEnabled;
		rootElement.OnRootElementDisabledEvent -= OnUiNavigationRootElementDisabled;
		HashSet<IUiNavigationElement> hashSet = _rootElements[rootElement];
		foreach (IUiNavigationSelectable item in _rootSelectables[rootElement])
		{
			DeInitSelectable(item);
		}
		foreach (IUiNavigationElement item2 in hashSet)
		{
			DeInitElement(item2);
		}
		_previouslySelectedMap.Remove(rootElement);
		_previouslyPickedMap.Remove(rootElement);
		_rootElements.Remove(rootElement);
		_rootSelectables.Remove(rootElement);
		_knownRoots.Remove(rootElement);
	}

	public void ClearRootCache(IUiNavigationRoot rootElement)
	{
		if (!_rootElements.ContainsKey(rootElement))
		{
			return;
		}
		if (_rootSelectables.TryGetValue(rootElement, out var value))
		{
			foreach (IUiNavigationSelectable item in value)
			{
				DeInitSelectable(item);
			}
			value.Clear();
		}
		if (_rootElements.TryGetValue(rootElement, out var value2))
		{
			foreach (IUiNavigationElement item2 in value2)
			{
				if (item2 != rootElement)
				{
					DeInitElement(item2);
				}
			}
			value2.RemoveWhere((IUiNavigationElement x) => x != rootElement);
		}
		_previouslySelectedMap[rootElement] = null;
		_previouslyPickedMap[rootElement] = null;
	}

	internal void AddElementInternal(IUiNavigationElement newElement, IUiNavigationRoot root)
	{
		if (newElement != root && (root == null || !_rootElements.ContainsKey(root)))
		{
			return;
		}
		ResetBuffers();
		newElement.InitializeNavigationElement(this, (newElement == root) ? null : root, _elementsBuffer, _elementsByNameMap, _knownSelectablesMapBuffer);
		foreach (IUiNavigationElement item in _elementsBuffer)
		{
			item.DestroyedEvent += OnElementDestroying;
			if (item.NavigationTags == null)
			{
				continue;
			}
			string[] navigationTags = item.NavigationTags;
			foreach (string text in navigationTags)
			{
				if (!string.IsNullOrEmpty(text))
				{
					if (!_taggedElementsMap.ContainsKey(text))
					{
						_taggedElementsMap.Add(text, new List<IUiNavigationElement>());
					}
					_taggedElementsMap[text].Add(item);
				}
			}
		}
		_rootElements[root].UnionWith(_elementsBuffer);
		_rootSelectables[root].UnionWith(_knownSelectablesMapBuffer.Keys);
		foreach (KeyValuePair<IUiNavigationSelectable, List<IUiNavigationNode>> item2 in _knownSelectablesMapBuffer)
		{
			item2.Key.OnPointerEnterEvent += InnerSelectByPointer;
			item2.Key.OnPointerExitEvent += InnerDeselectByPointer;
			_knownSelectablesMap.Add(item2.Key, item2.Value);
		}
		ResetBuffers();
	}

	private void ResetBuffers()
	{
		_elementsBuffer.Clear();
		_knownSelectablesMapBuffer.Clear();
		_elementsByNameMapBuffer.Clear();
	}

	internal void RemoveElementInternal(IUiNavigationElement element, IUiNavigationRoot root)
	{
		if (root == null || !_rootElements.ContainsKey(root))
		{
			return;
		}
		ResetBuffers();
		element.InitializeNavigationElement(this, null, _elementsBuffer, _elementsByNameMapBuffer, _knownSelectablesMapBuffer);
		foreach (IUiNavigationElement item in _elementsBuffer)
		{
			if (item is IUiNavigationSelectable uiNavigationSelectable)
			{
				DeInitSelectable(uiNavigationSelectable);
				_rootSelectables[root].Remove(uiNavigationSelectable);
			}
			DeInitElement(item);
		}
		ResetBuffers();
	}

	private void DeInitSelectable(IUiNavigationSelectable selectable)
	{
		if (_currentSelectable == selectable)
		{
			DeselectCurrentSelectable();
		}
		UnsubscribeSelectable(selectable);
	}

	private void UnsubscribeSelectable(IUiNavigationSelectable selectable)
	{
		if (selectable.Parent != null && _previouslySelectedMap.TryGetValue(selectable.Parent, out var value) && selectable == value)
		{
			_previouslySelectedMap.Remove(selectable.Parent);
		}
		selectable.OnPointerEnterEvent -= InnerSelectByPointer;
		selectable.OnPointerExitEvent -= InnerDeselectByPointer;
		_knownSelectablesMap.Remove(selectable);
	}

	private void DeInitElement(IUiNavigationElement element)
	{
		element.DestroyedEvent -= OnElementDestroying;
		if (element is IUiNavigationNode key)
		{
			_previouslySelectedMap.Remove(key);
			_previouslyPickedMap.Remove(key);
		}
		if (!string.IsNullOrEmpty(element.NavigationName))
		{
			_elementsByNameMap.Remove(element.NavigationName);
		}
		if (element.Parent != null && _previouslyPickedMap.TryGetValue(element.Parent, out var value) && element == value)
		{
			_previouslyPickedMap.Remove(element.Parent);
		}
		if (element.NavigationTags == null)
		{
			return;
		}
		string[] navigationTags = element.NavigationTags;
		foreach (string text in navigationTags)
		{
			if (!string.IsNullOrEmpty(text) && _taggedElementsMap.TryGetValue(text, out var value2))
			{
				value2.Remove(element);
			}
		}
	}

	private void InnerSelectByPointer([NotNull] IUiNavigationSelectable uiNavigationSelectable)
	{
		LogUtils.Log("[UiNavigationManager] " + uiNavigationSelectable.GameObject.name + "(\"" + uiNavigationSelectable.NavigationName + "\") selected by pointer");
		InnerSelect(uiNavigationSelectable);
	}

	private void InnerDeselectByPointer([NotNull] IUiNavigationSelectable uiNavigationSelectable)
	{
		LogUtils.Log("[UiNavigationManager] " + uiNavigationSelectable.GameObject.name + "(\"" + uiNavigationSelectable.NavigationName + "\") deselected by pointer");
		InnerDeselectCurrentSelectable();
	}

	private bool InnerSelect([NotNull] IUiNavigationSelectable uiNavigationSelectable)
	{
		LogUtils.Log("[UiNavigationManager] " + uiNavigationSelectable.GameObject.name + "(\"" + uiNavigationSelectable.NavigationName + "\") selected");
		if (_currentSelectable != null)
		{
			foreach (IUiNavigationNode item in _knownSelectablesMap[_currentSelectable])
			{
				if (item is IUiNavigationTransitMarkableNode uiNavigationTransitMarkableNode)
				{
					uiNavigationTransitMarkableNode.OnNavigationTransitUnmarked(_currentSelectable);
				}
			}
			_currentSelectable.OnNavigationDeselected();
		}
		_currentSelectable = uiNavigationSelectable;
		List<IUiNavigationNode> list = _knownSelectablesMap[_currentSelectable];
		foreach (IUiNavigationNode item2 in list)
		{
			if (!_previouslySelectedMap.ContainsKey(item2))
			{
				_previouslySelectedMap.Add(item2, null);
			}
			_previouslySelectedMap[item2] = _currentSelectable;
			if (item2 is IUiNavigationTransitMarkableNode uiNavigationTransitMarkableNode2)
			{
				uiNavigationTransitMarkableNode2.OnNavigationTransitMarked(_currentSelectable);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			IUiNavigationNode key = list[i];
			bool num = i == list.Count - 1;
			IUiNavigationElement uiNavigationElement = null;
			uiNavigationElement = ((!num) ? ((IUiNavigationElement)list[i + 1]) : ((IUiNavigationElement)_currentSelectable));
			if (!_previouslyPickedMap.ContainsKey(key))
			{
				_previouslyPickedMap.Add(key, null);
			}
			_previouslyPickedMap[key] = uiNavigationElement;
		}
		IUiNavigationSelectable currentSelectable = _currentSelectable;
		currentSelectable.ControlledSelectable.Select();
		this.OnElementSelectedEvent?.Invoke(currentSelectable);
		currentSelectable.OnNavigationSelected();
		return true;
	}

	private void InnerDeselectCurrentSelectable()
	{
		LogUtils.Log("[UiNavigationManager] " + _currentSelectable?.GameObject.name + "(\"" + _currentSelectable?.NavigationName + "\") deselected");
		if (_currentSelectable != null)
		{
			foreach (IUiNavigationNode item in _knownSelectablesMap[_currentSelectable])
			{
				if (item is IUiNavigationTransitMarkableNode uiNavigationTransitMarkableNode)
				{
					uiNavigationTransitMarkableNode.OnNavigationTransitUnmarked(_currentSelectable);
				}
			}
			_currentSelectable.OnNavigationDeselected();
		}
		_currentSelectable = null;
		if (EventSystem.current != null)
		{
			if (!EventSystem.current.alreadySelecting)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
			else
			{
				LogUtils.LogWarning("Already selecting with EventSystem");
			}
		}
	}

	private void InnerDeselectCurrentRoot()
	{
		InnerDeselectCurrentSelectable();
		_currentRoot = null;
	}

	private IUiNavigationSelectable FindFirstSelectable(IUiNavigationNode navigationNode)
	{
		foreach (IUiNavigationElement element in navigationNode.Elements)
		{
			if (!element.GameObject.activeInHierarchy)
			{
				continue;
			}
			if (element is IUiNavigationSelectable uiNavigationSelectable)
			{
				if (uiNavigationSelectable.ControlledSelectable.interactable)
				{
					return uiNavigationSelectable;
				}
			}
			else if (element is IUiNavigationNode uiNavigationNode)
			{
				IUiNavigationSelectable uiNavigationSelectable2 = ((!uiNavigationNode.AutoSelectFirstElement) ? (uiNavigationNode.DefaultElementToSelect ?? FindFirstSelectable(uiNavigationNode)) : FindFirstSelectable(uiNavigationNode));
				if (uiNavigationSelectable2 != null)
				{
					return uiNavigationSelectable2;
				}
			}
		}
		return null;
	}

	private void OnMoveSelectionInput(UIActionBaseEventData uiActionBaseEventData)
	{
		if (_navigationBlockers.Count > 0)
		{
			string text = string.Format("[{0}] Navigation blocked by {1} blockers:", "UiNavigationManager", _navigationBlockers.Count);
			foreach (UiNavigationBlocker navigationBlocker in _navigationBlockers)
			{
				text = string.Concat(text, "\n=> " + navigationBlocker.Tag);
			}
			text += "\n^^^";
			LogUtils.LogWarning(text);
		}
		else if (_currentRoot == null)
		{
			LogUtils.LogWarning("[UiNavigationManager] Current root is NULL => nowhere to navigate");
		}
		else if (CurrentNode == null)
		{
			if (_currentRoot.IsNavigationSourceAllowed(uiActionBaseEventData))
			{
				TrySelectPreviousIn(_currentRoot);
			}
		}
		else
		{
			if (CurrentNode.IsNavigationSourceAllowed(uiActionBaseEventData))
			{
				CurrentNode.TryToNavigate(_proceededNavigationNodes, uiActionBaseEventData.UINavigationDirection);
			}
			_proceededNavigationNodes.Clear();
		}
	}

	private void OnUiNavigationRootElementEnabled(IUiNavigationRoot enabledRoot)
	{
		if (SetRootsOnEnable)
		{
			if (_currentRoot == null)
			{
				SetCurrentRoot(enabledRoot);
			}
			else if (enabledRoot.NavigationPriority > _currentRoot.NavigationPriority)
			{
				SetCurrentRoot(enabledRoot);
			}
		}
	}

	private void OnUiNavigationRootElementDisabled(IUiNavigationRoot disabledRoot)
	{
		if (_currentRoot == disabledRoot)
		{
			IUiNavigationRoot uiNavigationRoot = _knownRoots.FirstOrDefault((IUiNavigationRoot rootElement) => rootElement.NavigationPriority < disabledRoot.NavigationPriority);
			if (uiNavigationRoot != null)
			{
				SetCurrentRoot(uiNavigationRoot);
			}
		}
	}

	private void OnElementDestroying(IUiNavigationElement destroyedElement)
	{
		if (destroyedElement == _currentSelectable)
		{
			LogUtils.Log("Current selectable \"" + destroyedElement.GameObject.name + "(" + destroyedElement.NavigationName + ")\" destroyed => set to NULL");
			_currentSelectable = null;
		}
		if (destroyedElement == _currentRoot)
		{
			LogUtils.Log("Current root \"" + destroyedElement.GameObject.name + "(" + destroyedElement.NavigationName + ")\" destroyed => set to NULL");
			_currentRoot = null;
			_currentSelectable = null;
		}
		if (!(destroyedElement is IUiNavigationRoot rootElement))
		{
			if (destroyedElement is IUiNavigationSelectable || destroyedElement is IUiNavigationNode)
			{
				RemoveElementInternal(destroyedElement, destroyedElement.Root);
			}
		}
		else
		{
			DeInitRootElement(rootElement);
		}
	}

	internal void RegisterRoot(UiNavigationRoot navigationRoot)
	{
		InitRootElement(navigationRoot);
	}

	internal void UnRegisterRoot(UiNavigationRoot navigationRoot)
	{
		DeInitRootElement(navigationRoot);
	}
}
