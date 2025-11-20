using System;
using System.Collections.Generic;
using System.Linq;
using AsmodeeNet.Utils.Extensions;
using GLOOM;
using ScenarioRuleLibrary;
using SpriteMemoryManagement;
using UnityEngine;
using UnityEngine.UI;

namespace WorldspaceUI;

public class EffectsBar : MonoBehaviour
{
	[Flags]
	public enum FEffect
	{
		None = 0,
		Blessed = 1,
		Cursed = 2,
		Disarmed = 4,
		Immobilized = 8,
		Invisible = 0x10,
		Muddled = 0x20,
		Pierce = 0x40,
		Poisoned = 0x80,
		Retaliate = 0x100,
		Strengthened = 0x200,
		Stunned = 0x400,
		Wounded = 0x800,
		Advantage = 0x1000,
		Disadvantage = 0x2000,
		Controlled = 0x4000,
		StopFlying = 0x8000,
		ImprovedShortRest = 0x10000,
		Flying = 0x20000,
		Sleep = 0x40000,
		BlockHealing = 0x80000,
		NeutralizeShield = 0x100000
	}

	[Serializable]
	private class EffectRef
	{
		public FEffect m_EffectType;

		public GameObject m_EffectObject;

		private const string DebugCancel = "EffectRef";

		private int? animationId;

		private Action onCancel;

		public bool IsAnimating => animationId.HasValue;

		public void SetAnimation(int id, Action onCancel = null)
		{
			CancelAnimation();
			animationId = id;
			this.onCancel = onCancel;
		}

		public void CancelAnimation()
		{
			if (animationId.HasValue)
			{
				LeanTween.cancel(animationId.Value, "EffectRef");
				animationId = null;
				m_EffectObject.transform.localScale = Vector3.one;
				onCancel?.Invoke();
			}
		}
	}

	private static readonly int _mask = Shader.PropertyToID("_Mask");

	[SerializeField]
	private List<EffectRef> m_EffectsRefs;

	private FEffect m_EffectState;

	[SerializeField]
	private EffectRetaliate _retaliateEffect;

	[SerializeField]
	private EffectRetaliate _stunEffect;

	[SerializeField]
	private EffectRetaliate _immobilizeEffect;

	[SerializeField]
	private EffectRetaliate _disarmEffect;

	[SerializeField]
	private EffectRetaliate _sleepingEffect;

	[SerializeField]
	private CustomEffect customEffectPrefab;

	[SerializeField]
	private List<CustomEffect> m_CustomEffectsSlots = new List<CustomEffect>();

	private Dictionary<string, CustomEffect> m_CustomEffectsRefs = new Dictionary<string, CustomEffect>();

	private List<string> doomedEffects = new List<string>();

	private List<string> itemEffects = new List<string>();

	private List<string> resourcesEffects = new List<string>();

	[Header("Replace condition")]
	[SerializeField]
	private float replaceShowConditionDuration = 1f;

	[SerializeField]
	private float replaceShowReplaceIconDelay;

	[SerializeField]
	private float replaceShowReplaceIconDuration = 1f;

	[SerializeField]
	private float replaceIconHighlightFadeOutDuration = 1f;

	[SerializeField]
	private float replaceConditionRotateDelay;

	[SerializeField]
	private float replaceConditionRotateDuration = 1f;

	[SerializeField]
	private float replaceIconShineDelay;

	[SerializeField]
	private float replaceIconShineDuration = 1f;

	[SerializeField]
	private float replaceAnimationSpeed = 1f;

	[SerializeField]
	private GameObject replaceGameObject;

	[SerializeField]
	private Image replaceShine;

	[SerializeField]
	private Image replaceGlow;

	[SerializeField]
	private Image replaceConditionIcon;

	[SerializeField]
	private CanvasGroup replaceIconCanvasGroup;

	private void Awake()
	{
		SetBlinkActive(FEffect.Poisoned, isActive: false, forceAction: true);
		SetBlinkActive(FEffect.Wounded, isActive: false, forceAction: true);
		if (_retaliateEffect != null)
		{
			_retaliateEffect.Initialise();
		}
		if (_stunEffect != null)
		{
			_stunEffect.Initialise();
		}
		if (_immobilizeEffect != null)
		{
			_immobilizeEffect.Initialise();
		}
		if (_disarmEffect != null)
		{
			_disarmEffect.Initialise();
		}
		if (_sleepingEffect != null)
		{
			_sleepingEffect.Initialise();
		}
		if (replaceShine != null)
		{
			replaceShine.material = new Material(replaceShine.material);
		}
	}

	private FEffect ToEffect(CCondition.EPositiveCondition condition)
	{
		return condition switch
		{
			CCondition.EPositiveCondition.Advantage => FEffect.Advantage, 
			CCondition.EPositiveCondition.Bless => FEffect.Blessed, 
			CCondition.EPositiveCondition.Invisible => FEffect.Invisible, 
			CCondition.EPositiveCondition.Strengthen => FEffect.Strengthened, 
			_ => FEffect.None, 
		};
	}

	public void UpdateEffects(bool blessed = false, bool controlled = false, bool cursed = false, bool disarmed = false, bool immobilized = false, bool invisible = false, bool muddled = false, bool pierce = false, bool poisoned = false, bool retaliate = false, bool strengthened = false, bool stunned = false, bool wounded = false, bool immovable = false, bool improvedShortRest = false, bool fly = false, List<CActiveBonus> doomed = null, List<CActiveBonus> activeItemEffectsOnMonsters = null, bool sleep = false, bool carryingQuestItem = false, bool blockHealing = false, bool neutralizeShield = false, List<CCharacterResource> characterResources = null)
	{
		if (blessed != m_EffectState.HasFlag(FEffect.Blessed))
		{
			ToggleEffect(FEffect.Blessed);
		}
		if (controlled != m_EffectState.HasFlag(FEffect.Controlled))
		{
			ToggleEffect(FEffect.Controlled);
		}
		if (cursed != m_EffectState.HasFlag(FEffect.Cursed))
		{
			ToggleEffect(FEffect.Cursed);
		}
		if (disarmed != m_EffectState.HasFlag(FEffect.Disarmed))
		{
			ToggleEffect(FEffect.Disarmed);
		}
		if (immobilized != m_EffectState.HasFlag(FEffect.Immobilized))
		{
			ToggleEffect(FEffect.Immobilized);
		}
		if (invisible != m_EffectState.HasFlag(FEffect.Invisible))
		{
			ToggleEffect(FEffect.Invisible);
		}
		if (muddled != m_EffectState.HasFlag(FEffect.Muddled))
		{
			ToggleEffect(FEffect.Muddled);
		}
		if (pierce != m_EffectState.HasFlag(FEffect.Pierce))
		{
			ToggleEffect(FEffect.Pierce);
		}
		if (poisoned != m_EffectState.HasFlag(FEffect.Poisoned))
		{
			ToggleEffect(FEffect.Poisoned);
		}
		if (retaliate != m_EffectState.HasFlag(FEffect.Retaliate))
		{
			ToggleEffect(FEffect.Retaliate);
		}
		if (strengthened != m_EffectState.HasFlag(FEffect.Strengthened))
		{
			ToggleEffect(FEffect.Strengthened);
		}
		if (stunned != m_EffectState.HasFlag(FEffect.Stunned))
		{
			ToggleEffect(FEffect.Stunned);
		}
		if (wounded != m_EffectState.HasFlag(FEffect.Wounded))
		{
			ToggleEffect(FEffect.Wounded);
		}
		if (improvedShortRest != m_EffectState.HasFlag(FEffect.ImprovedShortRest))
		{
			ToggleEffect(FEffect.ImprovedShortRest);
		}
		if (fly != m_EffectState.HasFlag(FEffect.Flying))
		{
			ToggleEffect(FEffect.Flying);
		}
		if (sleep != m_EffectState.HasFlag(FEffect.Sleep))
		{
			ToggleEffect(FEffect.Sleep);
		}
		if (blockHealing != m_EffectState.HasFlag(FEffect.BlockHealing))
		{
			ToggleEffect(FEffect.BlockHealing);
		}
		if (neutralizeShield != m_EffectState.HasFlag(FEffect.NeutralizeShield))
		{
			ToggleEffect(FEffect.NeutralizeShield);
		}
		if (doomedEffects.Count != 0 || doomed.Count != 0)
		{
			List<string> list = doomedEffects.ToList();
			foreach (CActiveBonus item in doomed)
			{
				string text = item.BaseCard.ID.ToString();
				list.Remove(text);
				ToggleDoomEffect(text, item);
			}
			foreach (string item2 in list)
			{
				UntoggleDoomEffect(item2);
			}
		}
		if (itemEffects.Count != 0 || !activeItemEffectsOnMonsters.IsNullOrEmpty())
		{
			List<string> list2 = itemEffects.ToList();
			foreach (CActiveBonus activeItemEffectsOnMonster in activeItemEffectsOnMonsters)
			{
				string text2 = activeItemEffectsOnMonster.BaseCard.ID.ToString();
				list2.Remove(text2);
				ToggleItemEffect(text2, activeItemEffectsOnMonster);
			}
			foreach (string item3 in list2)
			{
				UntoggleItemEffect(item3);
			}
		}
		if (!characterResources.IsNullOrEmpty())
		{
			List<string> list3 = resourcesEffects.ToList();
			foreach (CCharacterResource characterResource in characterResources)
			{
				if (characterResource.Amount > 0)
				{
					list3.Remove(characterResource.ID);
					ToggleCharacterResourceEffect(characterResource);
				}
			}
			foreach (string item4 in list3)
			{
				UntoggleCharacterResourceEffect(item4);
			}
		}
		if (carryingQuestItem)
		{
			ToggleCarryingQuestItemEffect();
		}
		else
		{
			UntoggleCarryingQuestItemEffect();
		}
	}

	private void ToggleCharacterResourceEffect(CCharacterResource characterResource)
	{
		if (!resourcesEffects.Contains(characterResource.ID))
		{
			AddCustomEffect(characterResource.ID, new ReferenceToSprite(UIInfoTools.Instance.GetActiveAbilityIcon(characterResource.ResourceData.Sprite)), LocalizationManager.GetTranslation(characterResource.ResourceData.LocKey));
			resourcesEffects.Add(characterResource.ID);
		}
	}

	public void UntoggleCharacterResourceEffect(string id)
	{
		if (resourcesEffects.Contains(id))
		{
			resourcesEffects.Remove(id);
			RemoveCustomEffect(id);
		}
	}

	private void ToggleItemEffect(string id, CActiveBonus bonus)
	{
		if (!itemEffects.Contains(id))
		{
			string descriptionTooltip = null;
			if (bonus.Ability.AbilityText.IsNOTNullOrEmpty())
			{
				descriptionTooltip = string.Format(CreateLayout.LocaliseText(bonus.Ability.AbilityText), CreateLayout.LocaliseText(bonus.Caster.Class.LocKey));
			}
			else if (bonus.Ability.PreviewEffectText.IsNOTNullOrEmpty())
			{
				descriptionTooltip = LocalizationManager.GetTranslation(bonus.Ability.PreviewEffectText);
			}
			AddCustomEffect(id, new ReferenceToSprite(UIInfoTools.Instance.GetActiveAbilityIcon("Hover" + bonus.Caster.GetPrefabName(), bonus.Caster.GetPrefabName())), "<size=+2><color=#" + UIInfoTools.Instance.basicTextColor.ToHex() + ">" + LocalizationManager.GetTranslation(bonus.BaseCard.Name) + "</color></size>", descriptionTooltip);
			itemEffects.Add(id);
		}
	}

	public void UntoggleItemEffect(string id)
	{
		if (itemEffects.Contains(id))
		{
			RemoveCustomEffect(id);
		}
	}

	public void ToggleDoomEffect(string id, CAbility ability, CDoom doom)
	{
		ToggleDoomEffect(id, ability, ability.AbilityBaseCard, doom.ActiveBonusLayout);
	}

	public void ToggleDoomEffect(string id, CActiveBonus bonus)
	{
		if (bonus.Layout?.ListLayouts != null)
		{
			ToggleDoomEffect(id, bonus.Ability, bonus.BaseCard, bonus.Layout.ListLayouts.FirstOrDefault());
		}
	}

	private void ToggleDoomEffect(string id, CAbility ability, CBaseCard card, string activeBonusLayout)
	{
		if (!doomedEffects.Contains(id))
		{
			AddCustomEffect(id, new ReferenceToSprite(UIInfoTools.Instance.GetPreviewEffectConfig(ECharacter.Doomstalker.ToString()).previewEffectIcon), "<size=+2><color=#" + UIInfoTools.Instance.basicTextColor.ToHex() + ">" + LocalizationManager.GetTranslation(card.Name) + "</color></size>", "<color=#" + UIInfoTools.Instance.GetCharacterColor(ECharacter.Doomstalker).ToHex() + ">" + LocalizationManager.GetTranslation("Doom") + ":</color> " + ActivePropertyLookup(ability, LocalisationAndPropertyLookup(ability, activeBonusLayout)));
			doomedEffects.Add(id);
		}
	}

	public void UntoggleDoomEffect(string id)
	{
		if (doomedEffects.Contains(id))
		{
			RemoveCustomEffect(id);
		}
	}

	private string ActivePropertyLookup(CAbility ability, string text)
	{
		while (text.Contains('^'.ToString()))
		{
			string text2 = CardLayoutRow.GetKey(text, '^').Replace(" ", "");
			int num = 0;
			if (text2 == ability.Name)
			{
				num += ability.Strength;
			}
			text = CardLayoutRow.ReplaceKey(text, '^', num.ToString());
		}
		return text;
	}

	private string LocalisationAndPropertyLookup(CAbility ability, string desc)
	{
		if (desc == null)
		{
			desc = string.Empty;
		}
		desc = CreateLayout.LocaliseText(desc);
		desc = (desc.Contains('*'.ToString()) ? CardLayoutRow.ReplaceKey(desc, '*', ability.Strength.ToString()) : desc);
		return desc;
	}

	public void AccentuateEffect(FEffect effect)
	{
		if (m_EffectState.HasFlag(effect))
		{
			EffectRef effectRef = m_EffectsRefs.Single((EffectRef ef) => ef.m_EffectType == effect);
			if (effectRef != null && !effectRef.IsAnimating)
			{
				RectTransform component = effectRef.m_EffectObject.GetComponent<RectTransform>();
				component.localScale = new Vector3(0.4f, 0.4f, 0.4f);
				LeanTween.scale(component, Vector3.one, 0.4f).setEase(LeanTweenType.easeOutBack).setOvershoot(2.5f);
			}
		}
	}

	public void ImmobilizedWarnEffect(bool active)
	{
		if (_immobilizeEffect != null)
		{
			_immobilizeEffect.Warn(active);
		}
	}

	public void DisarmedWarnEffect(bool active)
	{
		if (_disarmEffect != null)
		{
			_disarmEffect.Warn(active);
		}
	}

	public void StunnedWarnEffect(bool active)
	{
		if (_stunEffect != null)
		{
			_stunEffect.Warn(active);
		}
	}

	public void SleepingWarnEffect(bool active)
	{
		if (_sleepingEffect != null)
		{
			_sleepingEffect.Warn(active);
		}
	}

	private void ToggleEffect(FEffect effect)
	{
		m_EffectState = (FEffect)m_EffectState.GetWithInvertedFlag(effect);
		EffectRef effectRef = m_EffectsRefs.Single((EffectRef ef) => ef.m_EffectType == effect);
		if (!effectRef.IsAnimating)
		{
			effectRef.m_EffectObject.SetActive(m_EffectState.HasFlag(effect));
		}
	}

	public void EnableFlag(FEffect flag)
	{
		EffectRef effectRef = m_EffectsRefs.Single((EffectRef ef) => ef.m_EffectType == flag);
		m_EffectState |= flag;
		effectRef.m_EffectObject.SetActive(value: true);
	}

	public void DisableFlag(FEffect flag)
	{
		m_EffectState &= ~flag;
		EffectRef effectRef = m_EffectsRefs.Single((EffectRef ef) => ef.m_EffectType == flag);
		effectRef.CancelAnimation();
		effectRef.m_EffectObject.SetActive(value: false);
	}

	public void SetBlinkActive(FEffect effect, bool isActive = false, bool forceAction = false)
	{
		if (m_EffectState.HasFlag(effect) || forceAction)
		{
			m_EffectsRefs.Single((EffectRef ef) => ef.m_EffectType == effect).m_EffectObject.GetComponent<IEffectBlink>().enabled = isActive;
		}
	}

	public void DisableFlags()
	{
		m_EffectsRefs.ForEach(delegate(EffectRef it)
		{
			m_EffectState &= ~it.m_EffectType;
			it.m_EffectObject.SetActive(value: false);
			it.CancelAnimation();
		});
		foreach (KeyValuePair<string, CustomEffect> customEffectsRef in m_CustomEffectsRefs)
		{
			customEffectsRef.Value.Hide();
			m_CustomEffectsSlots.Add(customEffectsRef.Value);
		}
		m_CustomEffectsRefs.Clear();
		doomedEffects.Clear();
		itemEffects.Clear();
	}

	public void ToggleEffect(FEffect effect, bool active)
	{
		if (active != m_EffectState.HasFlag(effect))
		{
			ToggleEffect(effect);
		}
	}

	public void RetaliateActiveEffect()
	{
		if (_retaliateEffect != null)
		{
			_retaliateEffect.Activate();
		}
	}

	public void RetaliateWarnEffect(bool active)
	{
		if (_retaliateEffect != null)
		{
			_retaliateEffect.Warn(active);
		}
	}

	public void CancelRetaliateEffect()
	{
		if (_retaliateEffect != null)
		{
			_retaliateEffect.CancelEffectRetaliateTweens();
		}
	}

	public void RemoveCustomEffect(string id)
	{
		if (m_CustomEffectsRefs.ContainsKey(id))
		{
			m_CustomEffectsRefs[id].Hide();
			m_CustomEffectsSlots.Add(m_CustomEffectsRefs[id]);
			m_CustomEffectsRefs.Remove(id);
		}
	}

	public void AddCustomEffect(string id, ReferenceToSprite icon, string titleTooltip = null, string descriptionTooltip = null, bool blink = false)
	{
		CustomEffect customEffect;
		if (m_CustomEffectsRefs.ContainsKey(id))
		{
			customEffect = m_CustomEffectsRefs[id];
		}
		else if (m_CustomEffectsSlots.Count > 0)
		{
			customEffect = m_CustomEffectsSlots[m_CustomEffectsSlots.Count - 1];
			m_CustomEffectsSlots.RemoveAt(m_CustomEffectsSlots.Count - 1);
		}
		else
		{
			customEffect = UnityEngine.Object.Instantiate(customEffectPrefab, base.transform);
		}
		m_CustomEffectsRefs[id] = customEffect;
		customEffect.ShowEffect(icon, titleTooltip, descriptionTooltip, blink);
	}

	public void SetCustomEffectBlinkActive(string id, bool active)
	{
		if (m_CustomEffectsRefs.ContainsKey(id))
		{
			m_CustomEffectsRefs[id].EnableBlink(active);
		}
	}

	public void ResetCustomEffectBlink()
	{
		foreach (CustomEffect value in m_CustomEffectsRefs.Values)
		{
			value.EnableBlink(blink: false);
		}
	}

	public void UntoggleCarryingQuestItemEffect()
	{
		RemoveCustomEffect(ScenarioManager.ObjectImportType.CarryableQuestItem.ToString());
	}

	public void ToggleCarryingQuestItemEffect()
	{
		AddCustomEffect(ScenarioManager.ObjectImportType.CarryableQuestItem.ToString(), new ReferenceToSprite(UIInfoTools.Instance.CarryableQuestItemWorldSprite));
	}

	public void ReplaceCondition(FEffect oldCondition, CCondition.EPositiveCondition newCondition)
	{
		ReplaceCondition(oldCondition, ToEffect(newCondition));
	}

	public void ReplaceCondition(FEffect oldCondition, FEffect newCondition)
	{
		EffectRef effectRef = m_EffectsRefs.Single((EffectRef ef) => ef.m_EffectType == oldCondition);
		EffectRef newEffectRef = m_EffectsRefs.Single((EffectRef ef) => ef.m_EffectType == newCondition);
		effectRef.m_EffectObject.SetActive(value: true);
		effectRef.SetAnimation(LeanTween.scale(effectRef.m_EffectObject, Vector3.zero, replaceShowConditionDuration * replaceAnimationSpeed).setEaseInBack().setOnComplete((Action)delegate
		{
			effectRef.m_EffectObject.SetActive(m_EffectState.HasFlag(oldCondition));
			effectRef.CancelAnimation();
		})
			.id);
			replaceGameObject.SetActive(value: false);
			replaceGameObject.transform.localScale = Vector3.zero;
			replaceIconCanvasGroup.alpha = 0f;
			replaceIconCanvasGroup.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
			replaceGlow.SetAlpha(1f);
			replaceGlow.transform.localScale = Vector3.one;
			replaceShine.material.SetTextureOffset(_mask, new Vector2(0f, -1f));
			replaceConditionIcon.sprite = effectRef.m_EffectObject.GetComponent<Image>().sprite;
			newEffectRef.m_EffectObject.SetActive(value: false);
			newEffectRef.m_EffectObject.transform.localScale = Vector3.zero;
			newEffectRef.SetAnimation(LeanTween.sequence().append(replaceShowConditionDuration * replaceAnimationSpeed).append(delegate
			{
				replaceGameObject.SetActive(value: true);
			})
				.append(LeanTween.scale(replaceGameObject, Vector3.one, replaceShowConditionDuration * replaceAnimationSpeed).setEaseOutBack())
				.append(replaceShowReplaceIconDelay * replaceAnimationSpeed)
				.append(LeanTween.value(replaceIconCanvasGroup.gameObject, delegate(float val)
				{
					replaceIconCanvasGroup.alpha = 0.3f + val * 0.7f;
					replaceIconCanvasGroup.transform.localScale = Vector3.one * (0.3f + 0.7f * val);
				}, 0f, 1f, replaceShowReplaceIconDuration * replaceAnimationSpeed))
				.append(LeanTween.value(replaceGlow.gameObject, delegate(float val)
				{
					replaceGlow.SetAlpha(1f - val);
					replaceGlow.transform.localScale = new Vector3(1f + val * 0.3f, 1f + val * 0.3f);
				}, 0f, 1f, replaceIconHighlightFadeOutDuration * replaceAnimationSpeed))
				.append(replaceIconShineDelay * replaceAnimationSpeed)
				.append(LeanTween.value(replaceShine.gameObject, delegate(float val)
				{
					replaceShine.material.SetTextureOffset(_mask, new Vector2(0f, -1f + 2f * val));
				}, -1f, 1f, replaceIconShineDuration * replaceAnimationSpeed))
				.append(replaceConditionRotateDelay * replaceAnimationSpeed)
				.append(LeanTween.rotate(replaceGameObject, new Vector3(0f, 90f, 0f), replaceConditionRotateDuration / 2f * replaceAnimationSpeed))
				.append(delegate
				{
					replaceConditionIcon.sprite = newEffectRef.m_EffectObject.GetComponentInChildren<Image>().sprite;
					replaceIconCanvasGroup.alpha = 0f;
				})
				.append(LeanTween.rotate(replaceGameObject, new Vector3(0f, 0f, 0f), replaceConditionRotateDuration / 2f * replaceAnimationSpeed))
				.append(replaceShowReplaceIconDelay * replaceAnimationSpeed)
				.append(LeanTween.scale(replaceGameObject, Vector3.zero, replaceShowConditionDuration * replaceAnimationSpeed).setEaseInBack())
				.append(delegate
				{
					replaceGameObject.SetActive(value: false);
					newEffectRef.m_EffectObject.SetActive(value: true);
					newEffectRef.m_EffectObject.transform.localScale = Vector3.zero;
				})
				.append(LeanTween.scale(newEffectRef.m_EffectObject, Vector3.one, replaceShowConditionDuration * replaceAnimationSpeed).setEaseOutBack())
				.append(delegate
				{
					newEffectRef.m_EffectObject.SetActive(m_EffectState.HasFlag(newCondition));
					newEffectRef.CancelAnimation();
				})
				.id, delegate
				{
					replaceGameObject.SetActive(value: false);
				});
			}

			private void OnDisable()
			{
				m_EffectsRefs.ForEach(delegate(EffectRef it)
				{
					it.CancelAnimation();
				});
			}
		}
