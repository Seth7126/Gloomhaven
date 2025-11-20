using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace SM.Gamepad;

public class UiNavigationBase : MonoBehaviour, IUiNavigationElement, ISerializationCallbackReceiver
{
	[Serializable]
	public struct ArgNavigationAction
	{
		public NavigationAction NavigationAction;

		public MonoBehaviour[] Components;
	}

	[Header("Base Properties")]
	[SerializeField]
	private UiNavigationCustomizablePositionCalculatorHolder positionCalculatorHolder;

	public string Name = string.Empty;

	[SerializeField]
	private string[] _navigationTags;

	public int Priority;

	[Header("Navigation Actions")]
	public ArgNavigationAction[] OnNavigationEnterExitArgActions;

	public ArgNavigationAction[] OnNavigationEnterArgActions;

	public ArgNavigationAction[] OnNavigationExitArgActions;

	[HideInInspector]
	public NavigationAction[] OnNavigationEnterActions;

	[HideInInspector]
	public NavigationAction[] OnNavigationExitActions;

	private IInteractable _interactable;

	private IUiNavigationPositionCalculator _positionCalculator;

	private RectTransform _rectTransform;

	public virtual string NavigationName => Name;

	public string[] NavigationTags => _navigationTags;

	public int NavigationPriority => Priority;

	public GameObject GameObject => base.gameObject;

	public string ElementID => base.gameObject.GetInstanceID().ToString();

	public Vector2 NavigationPosition => _positionCalculator.CalculateNavigationPosition(this);

	public UiNavigationGroup Parent { get; private set; }

	public IUiNavigationRoot Root { get; private set; }

	public IUiNavigationManager UiNavigationManager { get; private set; }

	public RectTransform RectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = (RectTransform)base.gameObject.transform;
			}
			return _rectTransform;
		}
	}

	public virtual bool IsInteractable
	{
		get
		{
			if (_interactable == null)
			{
				_interactable = GetComponent<IInteractable>();
			}
			if (_interactable != null)
			{
				return _interactable.IsInteractable;
			}
			return false;
		}
	}

	public event Action<IUiNavigationElement> DestroyedEvent;

	[UsedImplicitly]
	protected virtual void Awake()
	{
		RefreshParent();
	}

	[UsedImplicitly]
	protected virtual void OnDestroy()
	{
		this.DestroyedEvent?.Invoke(this);
		if (Parent != null)
		{
			Parent.RemoveElementFromLists(this);
			Parent = null;
		}
	}

	[UsedImplicitly]
	protected virtual void OnBeforeTransformParentChanged()
	{
	}

	[UsedImplicitly]
	protected virtual void OnTransformParentChanged()
	{
		RefreshParent();
	}

	protected void InitializeBaseElement(IUiNavigationManager uiNavigationManager, IUiNavigationRoot root, Dictionary<string, IUiNavigationElement> elementsByNameMap, Dictionary<IUiNavigationSelectable, List<IUiNavigationNode>> knownSelectablesMap)
	{
		if (positionCalculatorHolder == null)
		{
			_positionCalculator = new UiNavigationPositionFromRect(GetComponent<RectTransform>());
		}
		else
		{
			_positionCalculator = positionCalculatorHolder.GetCalculator();
		}
		UiNavigationManager = uiNavigationManager;
		Root = root;
		UiNavigationElementsUtils.MarkNameOnMap(this, elementsByNameMap);
		UiNavigationElementsUtils.MarkSelectableOnMap(this, knownSelectablesMap);
	}

	public virtual void RefreshParent()
	{
		if (Parent != null)
		{
			Parent.RemoveElement(this);
		}
		UiNavigationGroup uiNavigationGroup = (Parent = UiNavigationElementsUtils.FindGroupForObjectSlow(this));
		Root = null;
		if (Parent != null)
		{
			uiNavigationGroup.AddElementInternal(this);
		}
	}

	public virtual void ClearParent()
	{
		Parent = null;
	}

	public virtual void ClearRoot()
	{
		Root = null;
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		TryRebuildActionField(ref OnNavigationEnterActions, ref OnNavigationEnterArgActions);
		TryRebuildActionField(ref OnNavigationExitActions, ref OnNavigationExitArgActions);
	}

	private void TryRebuildActionField(ref NavigationAction[] oldActionField, ref ArgNavigationAction[] newActionField)
	{
		if (oldActionField != null && newActionField == null)
		{
			int num = oldActionField.Length;
			newActionField = new ArgNavigationAction[num];
			for (int i = 0; i < num; i++)
			{
				newActionField[i].NavigationAction = oldActionField[i];
			}
			oldActionField = null;
		}
	}
}
