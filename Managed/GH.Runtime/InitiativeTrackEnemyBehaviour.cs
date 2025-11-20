using System;
using System.Collections;
using Chronos;
using ScenarioRuleLibrary;
using Script.GUI.Configuration;
using UnityEngine;
using UnityEngine.UI;

public class InitiativeTrackEnemyBehaviour : InitiativeTrackActorBehaviour
{
	[SerializeField]
	private GameObject allyMark;

	[SerializeField]
	private GameObject neutralMark;

	[SerializeField]
	private GameObject enemy2Mark;

	[Header("Card appearance animation settings")]
	[SerializeField]
	private LeanTweenType easeType;

	[SerializeField]
	private Vector3 movementAmount = new Vector3(-5f, -5f);

	[SerializeField]
	private float moveTime = 0.3f;

	[SerializeField]
	private Vector3 scaleAnimationOrigin = new Vector3(0.8f, 0.8f);

	[SerializeField]
	private float scaleTime = 0.5f;

	[SerializeField]
	private float fadeTime = 0.5f;

	[SerializeField]
	private GameObject monsterBasePrefab;

	[SerializeField]
	private GameObject monsterBasePrefab_gamepad;

	[SerializeField]
	private Transform monsterBaseHolder;

	private MonsterBaseUI monsterBaseUI;

	private CMonsterAbilityCard monsterAbilityCard;

	private RectTransform _selfRectTransform;

	[Header("Minimization with a large number of actors")]
	[SerializeField]
	private InitiativeTrackConfigUI _config;

	[SerializeField]
	private RectTransform _avatar;

	[SerializeField]
	private MonoBehaviour _mask;

	private float _startWidth;

	private Action _highlightChanged;

	private float MinimalWidth => _startWidth * _config.MinimalEnemyAvatarDesiredWidth;

	private float LeftAvatarBound => (_startWidth - MinimalWidth) * -0.5f;

	private float RightAvatarBound => (_startWidth - MinimalWidth) * -0.5f;

	private void Awake()
	{
		GameObject original = (InputManager.GamePadInUse ? monsterBasePrefab_gamepad : monsterBasePrefab);
		monsterBaseUI = UnityEngine.Object.Instantiate(original, monsterBaseHolder).GetComponent<MonsterBaseUI>();
		monsterBaseUI.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		LayoutElement obj = monsterBaseUI.gameObject.AddComponent<LayoutElement>();
		obj.preferredWidth = monsterBaseUI.GetComponent<RectTransform>().sizeDelta.x;
		obj.preferredHeight = monsterBaseUI.GetComponent<RectTransform>().sizeDelta.y;
		_selfRectTransform = base.transform as RectTransform;
		_startWidth = _selfRectTransform.sizeDelta.x;
		Minimize();
	}

	public override CActor.EType ActorType()
	{
		return CActor.EType.Enemy;
	}

	public override void Deselect()
	{
		base.Deselect();
		monsterBaseUI.gameObject.SetActive(value: false);
	}

	protected override void OnAvatarHighlight(bool active)
	{
		if (PhaseManager.PhaseType == CPhase.PhaseType.MonsterClassesSelectAbilityCards)
		{
			return;
		}
		if (active && monsterAbilityCard != null && PhaseManager.PhaseType >= CPhase.PhaseType.MonsterClassesSelectAbilityCards)
		{
			if (DebugMenu.displayCardsId)
			{
				UIManager.Instance.DisplayDebugMessage(monsterAbilityCard.Name + " card ID: " + monsterAbilityCard.ID);
			}
			monsterBaseUI.TogglePreview(active: true);
			Maximize();
		}
		else
		{
			monsterBaseUI.TogglePreview(active: false);
			Minimize();
		}
	}

	public override void SetAttributes(CActor actor, bool activePlayerButton, bool changeHilight, bool generateCard = true)
	{
		base.SetAttributes(actor, activePlayerButton, changeHilight);
		CEnemyActor cEnemyActor = (CEnemyActor)actor;
		monsterAbilityCard = cEnemyActor.MonsterClass.RoundAbilityCard;
		if (generateCard)
		{
			monsterBaseUI.GenerateCard(monsterAbilityCard, cEnemyActor, generationProgressive: true);
		}
		else
		{
			monsterBaseUI.DeferGenerateCard(monsterAbilityCard, cEnemyActor);
		}
		allyMark.SetActive(actor.Type == CActor.EType.Ally);
		enemy2Mark.SetActive(actor.Type == CActor.EType.Enemy2);
		neutralMark.SetActive(actor.Type == CActor.EType.Neutral);
	}

	public override void SetAttributesDirect(CActor actor, bool activeIDBackplate, bool activeID, string textID, bool activeInitiative, int initiative, int currentHealth, int maxHealth, bool isActive, bool activeHilight, bool activeButton)
	{
		base.SetAttributesDirect(actor, activeIDBackplate, activeID, textID, activeInitiative, initiative, currentHealth, maxHealth, isActive, activeHilight, activeButton);
		CEnemyActor cEnemyActor = (CEnemyActor)actor;
		monsterAbilityCard = cEnemyActor.MonsterClass.RoundAbilityCard;
		monsterBaseUI.GenerateCard(monsterAbilityCard, cEnemyActor, generationProgressive: true);
		allyMark.SetActive(actor.Type == CActor.EType.Ally);
		enemy2Mark.SetActive(actor.Type == CActor.EType.Enemy2);
		neutralMark.SetActive(actor.Type == CActor.EType.Neutral);
	}

	public void Init(Transform enemyCardsHolder, Action highlightChangedCallback, InitiativeTrackConfigUI initiativeTrackConfig)
	{
		SetCardHolder(enemyCardsHolder);
		_highlightChanged = highlightChangedCallback;
		_config = initiativeTrackConfig;
	}

	public void AnimateCardHighlight(float orderNumber, float cardsTotal, Action onCompleteCallback, float delay = 0f)
	{
		m_Avatar.SetHilighted(hilight: true);
		monsterBaseUI.transform.SetAsFirstSibling();
		monsterBaseUI.AnimateAppearance(monsterAbilityCard, actor, fadeTime, moveTime, scaleTime, delay, scaleAnimationOrigin, movementAmount, easeType, delegate
		{
			onCompleteCallback?.Invoke();
		});
	}

	public void FinishCardHighlightAnimation(float orderNumber, float cardsTotal, Action onCompleteCallback)
	{
		StopAllCoroutines();
		monsterBaseUI.FinishAnimateAppearance();
		OnCardHighlightAnimationComplete(animated: false);
		onCompleteCallback();
	}

	public void OnCardHighlightAnimationComplete(bool animated = true)
	{
		RefreshInitiative();
		if (animated)
		{
			m_Avatar.PlayEffect(InitiativeTrackActorAvatar.InitiativeEffects.Select);
			StartCoroutine(HideInitiativeEffect());
		}
		else
		{
			m_Avatar.PlayEffect(InitiativeTrackActorAvatar.InitiativeEffects.None);
		}
	}

	private void SetCardHolder(Transform cardsHolder)
	{
		monsterBaseHolder = cardsHolder;
		if (monsterBaseUI != null)
		{
			monsterBaseUI.transform.SetParent(monsterBaseHolder, worldPositionStays: false);
		}
	}

	private void Maximize()
	{
		_selfRectTransform.sizeDelta = new Vector2(_startWidth, _selfRectTransform.sizeDelta.y);
		_mask.enabled = false;
		_avatar.SetLeft(_config.StartLeftEnemyAvatarBound);
		_avatar.SetRight(_config.StartRightEnemyAvatarBound);
		_highlightChanged?.Invoke();
	}

	private void Minimize()
	{
		if (InitiativeTrack.Instance.NeedMinimizeEnemyInitiativeTrackAvatars)
		{
			_selfRectTransform.sizeDelta = new Vector2(MinimalWidth, _selfRectTransform.sizeDelta.y);
			_mask.enabled = true;
			_avatar.SetLeft(LeftAvatarBound);
			_avatar.SetRight(RightAvatarBound);
			_highlightChanged?.Invoke();
		}
	}

	private IEnumerator HideInitiativeEffect()
	{
		yield return Timekeeper.instance.WaitForSeconds(1f);
		m_Avatar.PlayEffect(InitiativeTrackActorAvatar.InitiativeEffects.None);
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(monsterBaseUI.gameObject);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		OnAvatarHighlight(active: false);
	}
}
