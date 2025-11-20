using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class InfuseElement : MonoBehaviour, IInfuseElement
{
	private bool animating;

	private CanvasGroup canvasGroup;

	[SerializeField]
	private UIFX_MaterialFX_Control effectsControl;

	[SerializeField]
	private Image elementImage;

	[SerializeField]
	private Sprite[] elementSprites;

	private bool highlighted;

	[SerializeField]
	[HideInInspector]
	private ElementInfusionBoardManager.EElement initialElement;

	[SerializeField]
	[HideInInspector]
	private bool isAnyElement;

	private bool isEnabled;

	[SerializeField]
	[HideInInspector]
	private bool isInitialized;

	[SerializeField]
	[HideInInspector]
	private ElementInfusionBoardManager.EElement selectedElement;

	private Material effectsMaterial;

	private UITextTooltipTarget tooltipTarget;

	public bool IsSelected
	{
		get
		{
			if (isAnyElement)
			{
				return selectedElement != ElementInfusionBoardManager.EElement.Any;
			}
			return true;
		}
	}

	public bool IsAnyElement => isAnyElement;

	public ElementInfusionBoardManager.EElement SelectedElement => selectedElement;

	private void Awake()
	{
		effectsMaterial = ((elementImage.material == elementImage.defaultMaterial) ? null : elementImage.material);
		canvasGroup = GetComponent<CanvasGroup>();
		tooltipTarget = GetComponent<UITextTooltipTarget>();
		if (tooltipTarget != null)
		{
			tooltipTarget.Initialize(UITooltip.Corner.Auto, new Vector2(27.5f, 0f), anchorToExactMouseTargetInstead: false, 300f, 50f, autoAdjustHeight: false, hideBackground: false);
			tooltipTarget.SetText(CreateLayout.LocaliseText("$Glossary_" + initialElement.ToString() + "$"));
		}
	}

	private void Start()
	{
		effectsControl.ToggleEnable(active: false);
		effectsControl.ApplyEffectType(selectedElement);
	}

	public void Init(ElementInfusionBoardManager.EElement element)
	{
		initialElement = element;
		selectedElement = element;
		elementImage.sprite = elementSprites[(int)element];
		isAnyElement = element == ElementInfusionBoardManager.EElement.Any;
		isInitialized = true;
		LayoutElement component = GetComponent<LayoutElement>();
		component.preferredWidth = GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize;
		component.preferredHeight = GlobalSettings.Instance.m_AbilityCardSettings.ElementIconSize;
	}

	public void Enable()
	{
		if (isAnyElement && isInitialized)
		{
			isEnabled = true;
			elementImage.sprite = elementSprites[(int)initialElement];
			selectedElement = initialElement;
			effectsControl.ApplyEffectType(selectedElement);
		}
	}

	public void Disable()
	{
		isEnabled = false;
	}

	public void PickElement(ElementInfusionBoardManager.EElement element)
	{
		AnimateGeneratedElement();
		selectedElement = element;
		elementImage.sprite = elementSprites[(int)element];
		effectsControl.ApplyEffectType(selectedElement);
	}

	public void ResetElementToInitial()
	{
		selectedElement = initialElement;
		elementImage.sprite = elementSprites[(int)initialElement];
		effectsControl.ToggleEnable(active: false);
		effectsControl.ApplyEffectType(selectedElement);
	}

	public void ToggleHoverEffect(bool active)
	{
		UpdateEffectsView(active);
	}

	public void TogglePermanentHighlight(bool active)
	{
		highlighted = active;
		UpdateEffectsView(active);
	}

	public void AnimateGeneratedElement()
	{
		if (base.gameObject.activeInHierarchy && !animating)
		{
			animating = true;
			effectsControl.ConsumableSelect(delegate
			{
				animating = false;
				TogglePermanentHighlight(active: false);
			});
			UpdateEffectsView(active: true);
		}
	}

	private void UpdateEffectsView(bool active)
	{
		if (!animating)
		{
			bool flag = active || highlighted;
			if (canvasGroup != null)
			{
				canvasGroup.ignoreParentGroups = flag;
			}
			elementImage.material = (flag ? effectsMaterial : null);
			effectsControl.ToggleEnable(flag);
		}
	}

	private void OnDisable()
	{
		animating = false;
		TogglePermanentHighlight(active: false);
	}
}
