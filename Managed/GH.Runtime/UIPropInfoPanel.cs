using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIPropInfoPanel : Singleton<UIPropInfoPanel>
{
	public enum EPropType
	{
		None,
		Trap,
		QuestItem,
		HazardousTerrain,
		DifficultTerrain
	}

	[SerializeField]
	private TMP_Text propName;

	[SerializeField]
	private Transform conditionsHolder;

	[SerializeField]
	private AutoScrollRect conditionsScroll;

	[SerializeField]
	private GameObject uiConditionsPrefab;

	[SerializeField]
	private Sprite damageSprite;

	[SerializeField]
	private Sprite moveSprite;

	private UIWindow myWindow;

	private EPropType propType;

	private List<UIStatusEffect> conditions = new List<UIStatusEffect>();

	private bool _shouldBeHidden;

	private GameObject _lastSelectedTrap;

	private GameObject _lastSelectedHazardousTerrain;

	private string _lastSelectedItemTitle;

	private string _lastSelectedItemDescription;

	private GameObject _lastSelectedDifficultTerrain;

	protected override void Awake()
	{
		base.Awake();
		if (InputManager.GamePadInUse)
		{
			TooltipsVisibilityHelper instance = TooltipsVisibilityHelper.Instance;
			instance.HideTooltipsEvent = (Action)Delegate.Combine(instance.HideTooltipsEvent, new Action(OnHideTooltips));
			TooltipsVisibilityHelper instance2 = TooltipsVisibilityHelper.Instance;
			instance2.ShowTooltipsEvent = (Action)Delegate.Combine(instance2.ShowTooltipsEvent, new Action(OnShowTooltips));
		}
		myWindow = GetComponent<UIWindow>();
	}

	protected override void OnDestroy()
	{
		if (InputManager.GamePadInUse && TooltipsVisibilityHelper.Instance != null)
		{
			TooltipsVisibilityHelper instance = TooltipsVisibilityHelper.Instance;
			instance.HideTooltipsEvent = (Action)Delegate.Remove(instance.HideTooltipsEvent, new Action(OnHideTooltips));
			TooltipsVisibilityHelper instance2 = TooltipsVisibilityHelper.Instance;
			instance2.ShowTooltipsEvent = (Action)Delegate.Remove(instance2.ShowTooltipsEvent, new Action(OnShowTooltips));
		}
		base.OnDestroy();
	}

	public void ShowTrap(GameObject trap)
	{
		if ((TransitionManager.s_Instance != null && !TransitionManager.s_Instance.TransitionDone) || Singleton<UIResultsManager>.Instance.IsShown)
		{
			return;
		}
		if (trap == null)
		{
			Debug.LogError("Trying to display trap info but the trap object does not exist.");
			return;
		}
		if (trap.GetComponent<UnityGameEditorObject>() == null)
		{
			Debug.LogError("No UnityGameEditorObject script found on Trap");
			return;
		}
		CObjectTrap cObjectTrap = ScenarioManager.CurrentScenarioState.Props.OfType<CObjectTrap>().SingleOrDefault((CObjectTrap s) => s.InstanceName == trap.name);
		if (cObjectTrap == null)
		{
			return;
		}
		propType = EPropType.Trap;
		propName.text = LocalizationManager.GetTranslation(cObjectTrap.PrefabName);
		_lastSelectedTrap = trap;
		conditions.ForEach(delegate(UIStatusEffect x)
		{
			UnityEngine.Object.Destroy(x.gameObject);
		});
		conditions.Clear();
		if (cObjectTrap.Damage)
		{
			if (cObjectTrap.Owner != null || cObjectTrap.HasCustomDamageSet)
			{
				CreateDamageEffect(cObjectTrap.DamageValue);
			}
			else
			{
				CreateDamageEffect(ScenarioManager.Scenario.SLTE.TrapDamage);
			}
		}
		foreach (CCondition.ENegativeCondition condition in cObjectTrap.Conditions)
		{
			CreateNegativeEffect(condition);
		}
		if (cObjectTrap.Damage || cObjectTrap.Conditions.Count > 0)
		{
			conditionsScroll.StartAutoscroll();
		}
		else
		{
			conditionsScroll.StopAutoscroll();
		}
		if (!_shouldBeHidden)
		{
			myWindow.Show();
		}
	}

	public void ShowHazardousTerrain(GameObject hazardousTerrain)
	{
		if ((TransitionManager.s_Instance != null && !TransitionManager.s_Instance.TransitionDone) || Singleton<UIResultsManager>.Instance.IsShown)
		{
			return;
		}
		if (hazardousTerrain == null)
		{
			Debug.LogError("Trying to display hazardous terrain info but the hazardous object does not exist.");
		}
		else if (hazardousTerrain.GetComponent<UnityGameEditorObject>() == null)
		{
			Debug.LogError("No UnityGameEditorObject script found on hazardous terrain");
		}
		else if (ScenarioManager.CurrentScenarioState.Props.OfType<CObjectHazardousTerrain>().SingleOrDefault((CObjectHazardousTerrain s) => s.InstanceName == hazardousTerrain.name) != null)
		{
			propType = EPropType.HazardousTerrain;
			propName.text = LocalizationManager.GetTranslation("HAZARDOUS_TERRAIN_TOOLTIP");
			_lastSelectedHazardousTerrain = hazardousTerrain;
			conditions.ForEach(delegate(UIStatusEffect x)
			{
				UnityEngine.Object.Destroy(x.gameObject);
			});
			conditions.Clear();
			CreateDamageEffect(ScenarioManager.Scenario.SLTE.HazardousTerrainDamage);
			if (!_shouldBeHidden)
			{
				myWindow.Show();
			}
		}
	}

	public void ShowQuestItem(string title, string description)
	{
		if ((!(TransitionManager.s_Instance != null) || TransitionManager.s_Instance.TransitionDone) && !Singleton<UIResultsManager>.Instance.IsShown)
		{
			propType = EPropType.QuestItem;
			propName.text = title;
			_lastSelectedItemTitle = title;
			_lastSelectedItemDescription = description;
			conditions.ForEach(delegate(UIStatusEffect x)
			{
				UnityEngine.Object.Destroy(x.gameObject);
			});
			conditions.Clear();
			CreateQuestItemEffect(description);
			if (!_shouldBeHidden)
			{
				myWindow.Show();
			}
		}
	}

	public void ShowDifficultTerrain(GameObject difficultTerrain)
	{
		if ((TransitionManager.s_Instance != null && !TransitionManager.s_Instance.TransitionDone) || Singleton<UIResultsManager>.Instance.IsShown)
		{
			return;
		}
		if (difficultTerrain == null)
		{
			Debug.LogError("Trying to display hazardous terrain info but the difficult terrain object does not exist.");
		}
		else if (difficultTerrain.GetComponent<UnityGameEditorObject>() == null)
		{
			Debug.LogError("No UnityGameEditorObject script found on difficult terrain");
		}
		else if (ScenarioManager.CurrentScenarioState.Props.OfType<CObjectDifficultTerrain>().SingleOrDefault((CObjectDifficultTerrain s) => s.InstanceName == difficultTerrain.name) != null)
		{
			propType = EPropType.DifficultTerrain;
			propName.text = LocalizationManager.GetTranslation("DIFFICULT_TERRAIN_TOOLTIP");
			_lastSelectedDifficultTerrain = difficultTerrain;
			conditions.ForEach(delegate(UIStatusEffect x)
			{
				UnityEngine.Object.Destroy(x.gameObject);
			});
			conditions.Clear();
			CreateMovementEffect(2);
			if (!_shouldBeHidden)
			{
				myWindow.Show();
			}
		}
	}

	private void CreateNegativeEffect(CCondition.ENegativeCondition condition)
	{
		UIStatusEffect component = UnityEngine.Object.Instantiate(uiConditionsPrefab, conditionsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
		conditions.Add(component);
		component.Initialize(condition);
	}

	private void CreateDamageEffect(int damageAmount)
	{
		UIStatusEffect component = UnityEngine.Object.Instantiate(uiConditionsPrefab, conditionsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
		conditions.Add(component);
		component.Initialize("", damageSprite, UIInfoTools.Instance.basicTextColor, "$Damage$: " + damageAmount);
	}

	private void CreateMovementEffect(int moveAmount)
	{
		UIStatusEffect component = UnityEngine.Object.Instantiate(uiConditionsPrefab, conditionsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
		conditions.Add(component);
		component.Initialize("", moveSprite, UIInfoTools.Instance.basicTextColor, "$MoveCost$: " + moveAmount);
	}

	private void CreateQuestItemEffect(string description)
	{
		UIStatusEffect component = UnityEngine.Object.Instantiate(uiConditionsPrefab, conditionsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
		conditions.Add(component);
		component.Initialize(description, UIInfoTools.Instance.CarryableQuestItemIconSprite, UIInfoTools.Instance.neutralActionTextColor);
	}

	public void Hide()
	{
		_lastSelectedTrap = null;
		_lastSelectedHazardousTerrain = null;
		_lastSelectedItemTitle = null;
		_lastSelectedItemDescription = null;
		_lastSelectedDifficultTerrain = null;
		propType = EPropType.None;
		conditionsScroll.StopAutoscroll();
		myWindow.Hide(instant: true);
	}

	public void Hide(params EPropType[] types)
	{
		if (types.Contains(propType))
		{
			Hide();
		}
	}

	private void OnHideTooltips()
	{
		_shouldBeHidden = true;
		if (_lastSelectedTrap != null)
		{
			GameObject lastSelectedTrap = _lastSelectedTrap;
			Hide();
			_lastSelectedTrap = lastSelectedTrap;
		}
		if (_lastSelectedHazardousTerrain != null)
		{
			GameObject lastSelectedHazardousTerrain = _lastSelectedHazardousTerrain;
			Hide();
			_lastSelectedHazardousTerrain = lastSelectedHazardousTerrain;
		}
		if (_lastSelectedItemTitle != null)
		{
			string lastSelectedItemTitle = _lastSelectedItemTitle;
			string lastSelectedItemDescription = _lastSelectedItemDescription;
			Hide();
			_lastSelectedItemTitle = lastSelectedItemTitle;
			_lastSelectedItemDescription = lastSelectedItemDescription;
		}
		if (_lastSelectedDifficultTerrain != null)
		{
			GameObject lastSelectedDifficultTerrain = _lastSelectedDifficultTerrain;
			Hide();
			_lastSelectedDifficultTerrain = lastSelectedDifficultTerrain;
		}
	}

	private void OnShowTooltips()
	{
		_shouldBeHidden = false;
		if (_lastSelectedTrap != null)
		{
			ShowTrap(_lastSelectedTrap);
		}
		if (_lastSelectedHazardousTerrain != null)
		{
			ShowHazardousTerrain(_lastSelectedHazardousTerrain);
		}
		if (_lastSelectedItemTitle != null)
		{
			ShowQuestItem(_lastSelectedItemTitle, _lastSelectedItemDescription);
		}
		if (_lastSelectedDifficultTerrain != null)
		{
			ShowDifficultTerrain(_lastSelectedDifficultTerrain);
		}
	}
}
