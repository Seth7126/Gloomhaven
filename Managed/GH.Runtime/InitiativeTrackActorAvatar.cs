#define ENABLE_LOGS
using System;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class InitiativeTrackActorAvatar : MonoBehaviour, ICharacterPortraitUser
{
	public enum InitiativeEffects
	{
		None,
		Hover,
		Select,
		Active
	}

	private const string Unknown = "?";

	[SerializeField]
	protected RawImage m_AvatarImage;

	[SerializeField]
	protected Image selectionImage;

	[SerializeField]
	protected GameObject selectionObject;

	[SerializeField]
	protected CanvasGroup canvasGroup;

	[SerializeField]
	public TextMeshProUGUI m_InitiativeText;

	[SerializeField]
	public DigitViewer m_DigitViewer;

	[SerializeField]
	protected TextMeshProUGUI nameText;

	[SerializeField]
	protected Material grayscaleMaterial;

	[SerializeField]
	protected Material regularMaterial;

	[SerializeField]
	protected PersistentAbilitiesUI persistentAbilities;

	[SerializeField]
	protected UIFX_MaterialFX_Control fxControls;

	[SerializeField]
	protected GameObject header;

	[SerializeField]
	protected GameObject deadImage;

	private bool highlighted;

	protected bool isSelected;

	protected CActor m_Actor;

	protected Action<bool> onAvatarHighlightAction;

	private InitiativeEffects effect;

	private bool UseTextInitiative
	{
		get
		{
			if (PlatformLayer.Setting.SimplifiedUI)
			{
				return m_DigitViewer == null;
			}
			return true;
		}
	}

	public virtual bool IsActiveBonusInteractable
	{
		get
		{
			return persistentAbilities.IsInteractable;
		}
		set
		{
			persistentAbilities.IsInteractable = value;
		}
	}

	public abstract CActor.EType ActorType();

	private void OnDestroy()
	{
		Singleton<CharacterPortraitsProvider>.Instance?.RemoveUser(this);
	}

	public CActor GetActorData()
	{
		return m_Actor;
	}

	public void OnPointerEnter()
	{
		SetHilighted(hilight: true);
		UIEventManager.LogUIEvent(new UIEvent(UIEvent.EUIEventType.InitiativeAvatarHovered, null, m_Actor.GetPrefabName()));
	}

	public void OnPointerExit()
	{
		SetHilighted(hilight: false);
	}

	public void SetHilighted(bool hilight)
	{
		highlighted = hilight;
		onAvatarHighlightAction(hilight);
	}

	public void SetGrayscale(bool active)
	{
		m_AvatarImage.material = (active ? grayscaleMaterial : regularMaterial);
	}

	protected void ToggleSelection(bool active)
	{
		bool active2 = active && !m_Actor.IsTakingExtraTurn;
		if (selectionObject != null)
		{
			selectionObject.SetActive(active2);
		}
	}

	public bool IsSelected()
	{
		return isSelected;
	}

	public virtual void Select()
	{
		isSelected = true;
		ToggleSelection(active: true);
	}

	public virtual void Deselect()
	{
		isSelected = false;
		ToggleSelection(active: false);
	}

	public virtual void OnClick(InitiativeTrackActorBehaviour actorUI)
	{
		Debug.Log("Select by click");
		InitiativeTrack.Instance.Select(actorUI);
	}

	public virtual void Init(Action<bool> onHighlightCallback, CActor actor, bool interactable)
	{
		m_Actor = actor;
		onAvatarHighlightAction = onHighlightCallback;
		ShowDetails(active: false);
		SetHilighted(hilight: false);
		if (m_DigitViewer != null)
		{
			m_DigitViewer.Initialize();
			if (PlatformLayer.Setting.SimplifiedUI)
			{
				m_InitiativeText.gameObject.SetActive(value: false);
			}
		}
		persistentAbilities.Init(actor, interactable);
	}

	public virtual void ShowDetails(bool active)
	{
		nameText.enabled = active;
	}

	public void RefreshInitiative()
	{
		if (m_Actor == null)
		{
			return;
		}
		if (UseTextInitiative)
		{
			m_InitiativeText.text = CalculateInitiative(m_Actor);
		}
		else
		{
			ViewDigitInitiative();
		}
		if (ActorType() != CActor.EType.Player)
		{
			return;
		}
		CPlayerActor cPlayerActor = (CPlayerActor)m_Actor;
		_ = cPlayerActor.CharacterClass.InitiativeAbilityCard;
		int count = cPlayerActor.CharacterClass.RoundAbilityCards.Count;
		bool flag = !FFSNetwork.IsOnline || PhaseManager.CurrentPhase.Type != CPhase.PhaseType.SelectAbilityCardsOrLongRest || cPlayerActor.IsUnderMyControl;
		if (!flag)
		{
			if (UseTextInitiative)
			{
				m_InitiativeText.text = "?";
			}
			else
			{
				m_DigitViewer.ViewSpecialSymbol("?");
			}
		}
		if (flag && (count == 2 || cPlayerActor.CharacterClass.LongRest))
		{
			PlayEffect(InitiativeEffects.None);
		}
		else if (count == 0 || !flag)
		{
			PlayEffect(InitiativeEffects.Hover);
			PlayEffect(InitiativeEffects.Active);
		}
		else
		{
			PlayEffect(InitiativeEffects.Select);
		}
	}

	protected virtual string CalculateInitiative(CActor actor)
	{
		if (actor.Initiative() != 0)
		{
			return actor.Initiative().ToString();
		}
		return "?";
	}

	private void ViewDigitInitiative()
	{
		int num = m_Actor.Initiative();
		if (num < 0)
		{
			m_DigitViewer.ViewSpecialSymbol(string.Empty);
		}
		else if (num == 0)
		{
			m_DigitViewer.ViewSpecialSymbol("?");
		}
		else
		{
			m_DigitViewer.ShowValue(num);
		}
	}

	public virtual void PlayEffect(InitiativeEffects effect)
	{
		if ((this.effect != effect || effect == InitiativeEffects.None) && fxControls.gameObject.activeInHierarchy)
		{
			this.effect = effect;
			switch (effect)
			{
			case InitiativeEffects.Active:
				fxControls.InitiativeActive();
				break;
			case InitiativeEffects.Hover:
				fxControls.InitiativeHover();
				break;
			case InitiativeEffects.None:
				fxControls.InitiativeNone();
				break;
			case InitiativeEffects.Select:
				fxControls.InitiativeSelect();
				break;
			}
		}
	}

	public void SetAttributes(CActor actor, bool activePlayerButton, bool changeHilight)
	{
		bool activeHilight = highlighted;
		if (changeHilight)
		{
			activeHilight = false;
		}
		int currentHealth = 0;
		int maxHealth = 0;
		if (actor is CPlayerActor cPlayerActor)
		{
			currentHealth = cPlayerActor.Health;
			maxHealth = cPlayerActor.MaxHealth;
		}
		if (actor is CEnemyActor cEnemyActor)
		{
			currentHealth = cEnemyActor.Health;
			maxHealth = cEnemyActor.MaxHealth;
		}
		SetAttributesDirect(actor, activeIDBackplate: true, activeID: true, actor.ID.ToString(), activeInitiative: true, actor.Initiative(), currentHealth, maxHealth, isActive: true, activeHilight, activePlayerButton);
	}

	public virtual void SetAttributesDirect(CActor actor, bool activeIDBackplate, bool activeID, string textID, bool activeInitiative, int initiative, int currentHealth, int maxHealth, bool isActive, bool activeHilight, bool activeButton)
	{
		Debug.LogWarning($"SetAttributesDirect {textID} {actor.Class?.DefaultModel} {actor.GetPrefabName()} {actor.Health} {actor.Level}");
		m_Actor = actor;
		nameText.text = LocalizationManager.GetTranslation(m_Actor.ActorLocKey());
		if (UseTextInitiative)
		{
			m_InitiativeText.enabled = activeInitiative;
			m_InitiativeText.text = ((initiative < 0) ? string.Empty : ((initiative == 0) ? "?" : initiative.ToString()));
		}
		else if (!activeInitiative)
		{
			m_DigitViewer.Hide();
		}
		else
		{
			ViewDigitInitiative();
		}
		m_AvatarImage.enabled = isActive;
		GetComponent<Button>().interactable = activeButton;
		Singleton<CharacterPortraitsProvider>.Instance.RegisterNewUser(this, m_Actor);
	}

	public void UpdateTexture(Texture texture, Rect coords)
	{
		if (texture == null)
		{
			Debug.LogWarningFormat("[ASSET BUNDLE MANAGER] - Unable to load asset: {0} in asset bundle: misc_characterportraits  folder: characterportraits", m_Actor.Class.DefaultModel.ToLowerInvariant());
		}
		m_AvatarImage.texture = texture;
		m_AvatarImage.uvRect = coords;
	}

	public void UnloadedTexture()
	{
		m_AvatarImage.texture = null;
	}

	public void OnActiveBonusTriggered(CActiveBonus activeBonus)
	{
		persistentAbilities.OnActiveBonusTriggered(activeBonus);
		RefreshAbilities();
	}

	public void RefreshAbilities()
	{
		persistentAbilities.RefreshAbilities();
	}

	public void RefreshActiveInteractable()
	{
		persistentAbilities.RefreshInteractable(isInteractable: true);
	}

	public void SetActiveHeader(bool value)
	{
		header.SetActive(value);
	}

	public void SetActiveDeadImage(bool value)
	{
		deadImage.SetActive(value);
	}

	public void EnableNavigation(Selectable left, Selectable right)
	{
		persistentAbilities.EnableNavigation(left, right);
	}

	public void DisableNavigation()
	{
		persistentAbilities.DisableNavigation();
	}

	public Selectable GetFirstBonus()
	{
		return persistentAbilities.GetFirstBonus();
	}
}
