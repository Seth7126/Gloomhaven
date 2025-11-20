#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using ScenarioRuleLibrary;
using UnityEngine;

public class VFXShared
{
	public enum AreaEffectAlignments
	{
		None,
		Radial,
		Linear,
		CasterToTarget
	}

	public enum ActorType
	{
		EnemyAndAllyAndEmpty,
		EnemyOnly,
		AllyOnly,
		EmptyOnly,
		EnemyAndEmpty,
		AllyAndEmpty,
		EnemyAndAlly
	}

	public class ActorEventsObject : ScriptableObject
	{
		public ActorEvents ActorEvents { get; set; }
	}

	[Serializable]
	public class Progress
	{
		public enum ProgressTargets
		{
			Caster,
			MainTarget
		}

		public string Name;

		public float ProgressTime;

		public ProgressTargets ProgressTarget;

		[NonSerialized]
		public float OriginalProgressTime;
	}

	public class HitEffectTargets
	{
		public GameObject CurrentActor;

		public List<HitTime> HitTimes;

		public HitEffectTargets(GameObject currentActor)
		{
			CurrentActor = currentActor;
			HitTimes = new List<HitTime>();
		}
	}

	public class HitTime
	{
		public float Time;

		public List<CharacterManager> targets;

		public HitTime(float time, bool useCoroutine = false)
		{
			Time = time;
			targets = new List<CharacterManager>();
		}
	}

	[Serializable]
	public class BasicEffect
	{
		public GameObject Effect;

		public float StartTime;

		[NonSerialized]
		public Transform Parent;

		[NonSerialized]
		public Vector3 Position = Vector3.zero;

		[NonSerialized]
		public Quaternion Rotation = Quaternion.identity;

		[NonSerialized]
		public float OriginalStartTime;
	}

	[Serializable]
	public class SpawnedEffect : BasicEffect
	{
		public string EffectAttachPoint;

		public bool AttachToRoot;
	}

	[Serializable]
	public class BasicEffectTyped : BasicEffect
	{
		public ActorType TargetType = ActorType.EnemyAndAlly;

		[NonSerialized]
		public CActor.EType? TargetActorType;

		[NonSerialized]
		public CActor.EType CasterType;

		public BasicEffectTyped(GameObject effect, float startTime, ActorType targetType, CActor.EType casterType)
		{
			Effect = effect;
			StartTime = startTime;
			TargetType = targetType;
			CasterType = casterType;
		}

		public BasicEffectTyped Clone()
		{
			return new BasicEffectTyped(Effect, StartTime, TargetType, CasterType);
		}
	}

	[Serializable]
	public class TargetHexEffects
	{
		public float HitTime;

		public List<BasicEffectTyped> Effects;

		[NonSerialized]
		public float OriginalHitTime;
	}

	[Serializable]
	public class AuraHexEffectData
	{
		public Animator CasterAnimator;

		public TargetHexEffects AuraEffects;

		public AreaEffectAlignments AreaEffectAlignments;

		public bool CasterIsCentre;

		public CActor.EType ActorType;
	}

	public const string AnimFXFunction = "TriggerFX";

	public const string AnimHitEffectFunction = "TriggerHitEffectTarget";

	public const string AnimProgressChoreographerFunction = "TriggerProgressChoreographer";

	public static Dictionary<int, AuraHexEffectData> RegisteredAuraHexEffects = new Dictionary<int, AuraHexEffectData>();

	public static Dictionary<int, List<GameObject>> RegisteredAuraEffectObjects = new Dictionary<int, List<GameObject>>();

	private static List<EffectOnEnable> s_PersistentEffects = new List<EffectOnEnable>();

	public static float GetEffectLifetime(GameObject effect)
	{
		if (effect == null)
		{
			return 0f;
		}
		EffectOnEnable component = effect.GetComponent<EffectOnEnable>();
		if ((bool)component)
		{
			if (component.InfiniteLifeTime)
			{
				return float.MaxValue;
			}
			return component.LifeTime;
		}
		Debug.LogError("Effect " + effect.name + " does not have the EffectOnEnable component attached.  Lifetime is defaulting to " + 6f + " seconds");
		return 6f;
	}

	public static IEnumerator PlayEffectCoroutine(GameObject effect, Transform parent, Vector3 position, Quaternion rotation, float startTime)
	{
		if (startTime > 0f)
		{
			yield return Timekeeper.instance.WaitForSeconds(startTime);
		}
		PlayEffect(effect, parent, position, rotation);
	}

	public static void PlaySpawnedEffectsFromTimeline(List<SpawnedEffect> spawnedEffects, GameObject caster)
	{
		List<BasicEffect> list = new List<BasicEffect>();
		foreach (SpawnedEffect spawnedEffect in spawnedEffects)
		{
			GameObject gameObject = ((!spawnedEffect.AttachToRoot) ? caster.FindInChildren(spawnedEffect.EffectAttachPoint) : caster);
			if (gameObject == null)
			{
				spawnedEffect.Position = caster.transform.position;
			}
			else
			{
				spawnedEffect.Position = spawnedEffect.Effect.transform.position;
			}
			spawnedEffect.Parent = gameObject?.transform;
			spawnedEffect.Rotation = Quaternion.LookRotation(caster.transform.forward, caster.transform.up);
			list.Add(spawnedEffect);
		}
		if (list.Count > 0)
		{
			CoroutineHelper.RunCoroutine(PlayBasicEffects(list));
		}
	}

	public static void PlayAreaEffectsFromTimeline(List<BasicEffectTyped> basicEffects)
	{
		List<BasicEffect> list = new List<BasicEffect>();
		foreach (BasicEffectTyped basicEffect in basicEffects)
		{
			list.Add(basicEffect);
		}
		if (list.Count > 0)
		{
			CoroutineHelper.RunCoroutine(PlayBasicEffects(list));
		}
	}

	public static void PlayAuraEffectsFromTimeline(List<BasicEffectTyped> basicAuraEffects, int auraAbilityId)
	{
		List<BasicEffect> list = new List<BasicEffect>();
		foreach (BasicEffectTyped basicAuraEffect in basicAuraEffects)
		{
			list.Add(basicAuraEffect);
		}
		if (list.Count > 0)
		{
			CoroutineHelper.RunCoroutine(PlayAndRegisterAuraBasicEffects(list, auraAbilityId));
		}
	}

	public static IEnumerator PlayBasicEffects(List<BasicEffect> basicEffects)
	{
		List<BasicEffect> tempList = new List<BasicEffect>(basicEffects);
		float elapsedTime = 0f;
		while (tempList.Count > 0)
		{
			float startTime = tempList.Min((BasicEffect x) => x.StartTime) - elapsedTime;
			if (startTime > 0f)
			{
				yield return Timekeeper.instance.WaitForSeconds(startTime);
				elapsedTime += startTime;
			}
			foreach (BasicEffect item in tempList.Where((BasicEffect x) => x.StartTime == startTime + elapsedTime).ToList())
			{
				PlayEffect(item.Effect, item.Parent, item.Position, item.Rotation);
				tempList.Remove(item);
			}
		}
	}

	public static IEnumerator PlayAndRegisterAuraBasicEffects(List<BasicEffect> basicEffects, int auraAbilityId)
	{
		if (RegisteredAuraEffectObjects.Any((KeyValuePair<int, List<GameObject>> x) => x.Key == auraAbilityId))
		{
			yield break;
		}
		List<BasicEffect> tempList = new List<BasicEffect>(basicEffects);
		List<GameObject> auraEffects = new List<GameObject>();
		float elapsedTime = 0f;
		while (tempList.Count > 0)
		{
			float startTime = tempList.Min((BasicEffect x) => x.StartTime) - elapsedTime;
			if (startTime > 0f)
			{
				yield return Timekeeper.instance.WaitForSeconds(startTime);
				elapsedTime += startTime;
			}
			foreach (BasicEffect item2 in tempList.Where((BasicEffect x) => x.StartTime == startTime + elapsedTime).ToList())
			{
				GameObject item = PlayAndReturnEffect(item2.Effect, item2.Parent, item2.Position, item2.Rotation);
				tempList.Remove(item2);
				auraEffects.Add(item);
			}
		}
		RegisteredAuraEffectObjects.Add(auraAbilityId, auraEffects);
	}

	public static void StopRegisteredAuraEffects(int auraAbilityId, bool pause = false)
	{
		if (!RegisteredAuraEffectObjects.TryGetValue(auraAbilityId, out var value))
		{
			return;
		}
		if (value.Count > 0)
		{
			foreach (GameObject item in value)
			{
				StopEffect(item);
			}
		}
		RegisteredAuraEffectObjects.Remove(auraAbilityId);
		if (!pause)
		{
			RegisteredAuraHexEffects.Remove(auraAbilityId);
		}
	}

	public static IEnumerator PlayAreaEffectsCoroutine(List<BasicEffectTyped> basicEffects)
	{
		List<BasicEffectTyped> tempList = new List<BasicEffectTyped>(basicEffects);
		float elapsedTime = 0f;
		while (tempList.Count > 0)
		{
			float startTime = tempList.Min((BasicEffectTyped x) => x.StartTime) - elapsedTime;
			if (startTime > 0f)
			{
				yield return Timekeeper.instance.WaitForSeconds(startTime);
				elapsedTime += startTime;
			}
			foreach (BasicEffectTyped item in tempList.Where((BasicEffectTyped x) => x.StartTime == startTime + elapsedTime).ToList())
			{
				PlayEffect(item.Effect, item.Parent, item.Position, item.Rotation);
				tempList.Remove(item);
			}
		}
	}

	public static void ClearTriggerFXEvents(AnimationClip clip)
	{
		List<AnimationEvent> list = clip.events.ToList();
		list.RemoveAll((AnimationEvent x) => x.functionName == "TriggerFX" || x.functionName == "TriggerHitEffectTarget" || x.functionName == "TriggerProgressChoreographer");
		clip.events = list.ToArray();
	}

	public static void ControlChoreographer(bool control, StateMachineBehaviour controlledByStateBehaviour = null)
	{
		Choreographer.s_Choreographer.m_SMB_Control_WaitingForActorBeenAttackedAnim = control;
		Choreographer.s_Choreographer.m_SMB_Control_WaitingForAttackAnim = control;
		if ((bool)controlledByStateBehaviour)
		{
			if (control)
			{
				Choreographer.s_Choreographer.m_SMB_Control_ControlledByStateBehaviour = controlledByStateBehaviour;
			}
			else
			{
				Choreographer.s_Choreographer.m_SMB_Control_ControlledByStateBehaviour = null;
			}
		}
	}

	private static void PlayEffect(GameObject effect, Transform parent, Vector3 position, Quaternion rotation)
	{
		GameObject gameObject = ObjectPool.Spawn(effect, parent, position, rotation);
		float effectLifetime = GetEffectLifetime(gameObject);
		if (Mathf.Approximately(effectLifetime, float.MaxValue))
		{
			s_PersistentEffects.Add(gameObject.GetComponent<EffectOnEnable>());
		}
		ObjectPool.Recycle(gameObject, effectLifetime, effect, gameObject.GetComponent<EffectOnEnable>().DestroyOnRecycle);
	}

	private static GameObject PlayAndReturnEffect(GameObject effect, Transform parent, Vector3 position, Quaternion rotation)
	{
		return ObjectPool.Spawn(effect, parent, position, rotation);
	}

	private static void StopEffect(GameObject particleInstance)
	{
		ObjectPool.Recycle(particleInstance);
	}

	public static void StopPersistentEffects()
	{
		foreach (EffectOnEnable s_PersistentEffect in s_PersistentEffects)
		{
			if (!(s_PersistentEffect == null))
			{
				if (s_PersistentEffect.OnDestroyEffect != null)
				{
					PlayEffect(s_PersistentEffect.OnDestroyEffect, s_PersistentEffect.transform.parent, s_PersistentEffect.transform.localPosition, s_PersistentEffect.transform.localRotation);
				}
				ObjectPool.Recycle(s_PersistentEffect.gameObject, s_PersistentEffect.LifeTime);
			}
		}
		s_PersistentEffects.Clear();
	}

	public static void ProcessAreaEffects(TargetHexEffects areaHexEffects, TargetHexEffects targetHexEffects, Animator animator, AreaEffectAlignments areaEffectAlignment, bool casterIsCentre, bool areaEffectAdjacentToTargets, ref HitEffectTargets hitEffectTargets, ref List<BasicEffectTyped> basicEffects, CActor.EType casterType)
	{
		try
		{
			if (areaHexEffects.Effects.Count <= 0)
			{
				return;
			}
			hitEffectTargets.HitTimes.Add(new HitTime(areaHexEffects.HitTime));
			List<CClientTile> list = new List<CClientTile>();
			if (areaEffectAdjacentToTargets)
			{
				foreach (CActor item in Choreographer.s_Choreographer.ActorsBeingTargetedForVFX)
				{
					foreach (CTile allAdjacentTile in ScenarioManager.GetAllAdjacentTiles(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item.ArrayIndex.X, item.ArrayIndex.Y].m_Tile))
					{
						list.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[allAdjacentTile.m_ArrayIndex.X, allAdjacentTile.m_ArrayIndex.Y]);
					}
				}
			}
			else
			{
				list = Choreographer.s_Choreographer.CurrentAttackArea.ToList();
			}
			if (list.Count <= 0)
			{
				return;
			}
			foreach (CClientTile item2 in list)
			{
				if (targetHexEffects.Effects.Count > 0 && item2 == Choreographer.s_Choreographer.m_lastSelectedTile)
				{
					continue;
				}
				GameObject gameObject = CharacterManager.FindTargetAtTile(item2);
				if (gameObject == animator.transform.parent.gameObject)
				{
					continue;
				}
				CharacterManager characterManager = null;
				if ((bool)gameObject)
				{
					characterManager = CharacterManager.GetCharacterManager(gameObject);
				}
				Quaternion rotation = Quaternion.identity;
				switch (areaEffectAlignment)
				{
				case AreaEffectAlignments.Linear:
					rotation = Quaternion.LookRotation(animator.gameObject.transform.forward, animator.gameObject.transform.up);
					break;
				case AreaEffectAlignments.Radial:
					rotation = ((!casterIsCentre && Choreographer.s_Choreographer.m_lastSelectedTile != null) ? Quaternion.LookRotation(item2.m_GameObject.transform.position - Choreographer.s_Choreographer.m_lastSelectedTile.m_GameObject.transform.position, animator.gameObject.transform.up) : Quaternion.LookRotation(item2.m_GameObject.transform.position - animator.gameObject.transform.position, animator.gameObject.transform.up));
					break;
				case AreaEffectAlignments.CasterToTarget:
					rotation = Quaternion.LookRotation(Choreographer.s_Choreographer.m_lastSelectedTile.m_GameObject.transform.position - animator.gameObject.transform.position, animator.gameObject.transform.up);
					break;
				}
				foreach (BasicEffectTyped effect in areaHexEffects.Effects)
				{
					BasicEffectTyped basicEffectTyped = effect.Clone();
					basicEffectTyped.Parent = item2.m_GameObject.transform;
					basicEffectTyped.Position = basicEffectTyped.Effect.transform.position;
					basicEffectTyped.Rotation = rotation;
					basicEffectTyped.CasterType = casterType;
					if ((bool)characterManager)
					{
						basicEffectTyped.TargetActorType = characterManager.CharacterActor.Type;
					}
					else
					{
						basicEffectTyped.TargetActorType = null;
					}
					if (ShouldPlayEffect(basicEffectTyped))
					{
						basicEffects.Add(basicEffectTyped);
					}
				}
				if ((bool)characterManager)
				{
					hitEffectTargets.HitTimes.Last().targets.Add(characterManager);
				}
			}
		}
		catch (NullReferenceException ex)
		{
			Debug.LogWarning("Null reference exection when running ProcessAreaEffects.\n" + ex.StackTrace);
		}
	}

	public static void RegisterAuraHexEffects(int auraAbilityId, TargetHexEffects auraHexEffects, Animator animator, AreaEffectAlignments areaEffectAlignment, bool casterIsCentre, CActor.EType actorType)
	{
		if (!RegisteredAuraHexEffects.Any((KeyValuePair<int, AuraHexEffectData> x) => x.Key == auraAbilityId))
		{
			AuraHexEffectData auraHexEffectData = new AuraHexEffectData();
			auraHexEffectData.AuraEffects = auraHexEffects;
			auraHexEffectData.CasterAnimator = animator;
			auraHexEffectData.AreaEffectAlignments = areaEffectAlignment;
			auraHexEffectData.CasterIsCentre = casterIsCentre;
			auraHexEffectData.ActorType = actorType;
			RegisteredAuraHexEffects.Add(auraAbilityId, auraHexEffectData);
		}
	}

	public static void ProcessAuraHexEffects(CAbility auraAbility, AuraHexEffectData auraHexEffectData, CActor casterActor, ref List<BasicEffectTyped> basicAuraEffects, ref HitEffectTargets hitEffectTargets)
	{
		if (auraHexEffectData.AuraEffects.Effects.Count <= 0)
		{
			return;
		}
		if (casterActor != null)
		{
			List<CClientTile> list = new List<CClientTile>();
			foreach (CTile item in GameState.GetTilesInRange(auraAbility.TargetingActor, auraAbility.Range, auraAbility.Targeting, emptyTilesOnly: false, ignoreBlocked: true))
			{
				list.Add(ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[item.m_ArrayIndex.X, item.m_ArrayIndex.Y]);
			}
			foreach (CClientTile item2 in list)
			{
				GameObject gameObject = CharacterManager.FindTargetAtTile(item2);
				if (gameObject == auraHexEffectData.CasterAnimator.transform.parent.gameObject)
				{
					continue;
				}
				CharacterManager characterManager = null;
				if ((bool)gameObject)
				{
					characterManager = CharacterManager.GetCharacterManager(gameObject);
				}
				Quaternion rotation = Quaternion.identity;
				if (auraHexEffectData.AreaEffectAlignments == AreaEffectAlignments.Linear)
				{
					rotation = Quaternion.LookRotation(auraHexEffectData.CasterAnimator.gameObject.transform.forward, auraHexEffectData.CasterAnimator.gameObject.transform.up);
				}
				else if (auraHexEffectData.AreaEffectAlignments == AreaEffectAlignments.Radial)
				{
					rotation = ((!auraHexEffectData.CasterIsCentre) ? Quaternion.LookRotation(item2.m_GameObject.transform.position - Choreographer.s_Choreographer.m_lastSelectedTile.m_GameObject.transform.position, auraHexEffectData.CasterAnimator.gameObject.transform.up) : Quaternion.LookRotation(item2.m_GameObject.transform.position - auraHexEffectData.CasterAnimator.gameObject.transform.position, auraHexEffectData.CasterAnimator.gameObject.transform.up));
				}
				else if (auraHexEffectData.AreaEffectAlignments == AreaEffectAlignments.CasterToTarget)
				{
					rotation = Quaternion.LookRotation(Choreographer.s_Choreographer.m_lastSelectedTile.m_GameObject.transform.position - auraHexEffectData.CasterAnimator.gameObject.transform.position, auraHexEffectData.CasterAnimator.gameObject.transform.up);
				}
				foreach (BasicEffectTyped effect in auraHexEffectData.AuraEffects.Effects)
				{
					BasicEffectTyped basicEffectTyped = effect.Clone();
					basicEffectTyped.Parent = item2.m_GameObject.transform;
					basicEffectTyped.Position = basicEffectTyped.Effect.transform.position;
					basicEffectTyped.Rotation = rotation;
					basicEffectTyped.CasterType = auraHexEffectData.ActorType;
					if ((bool)characterManager)
					{
						basicEffectTyped.TargetActorType = characterManager.CharacterActor.Type;
					}
					else
					{
						basicEffectTyped.TargetActorType = null;
					}
					basicAuraEffects.Add(basicEffectTyped);
				}
				if ((bool)characterManager)
				{
					hitEffectTargets.HitTimes.LastOrDefault()?.targets.Add(characterManager);
				}
			}
		}
		hitEffectTargets.HitTimes.Add(new HitTime(auraHexEffectData.AuraEffects.HitTime));
	}

	public static bool ShouldPlayEffect(BasicEffectTyped bet)
	{
		if (bet.TargetType == ActorType.EnemyAndAllyAndEmpty)
		{
			return true;
		}
		if (!bet.TargetActorType.HasValue)
		{
			return HasEmpty(bet.TargetType);
		}
		if (bet.TargetActorType == CActor.EType.Player || bet.TargetActorType == CActor.EType.HeroSummon)
		{
			if (bet.CasterType == CActor.EType.Player || bet.CasterType == CActor.EType.HeroSummon)
			{
				return HasAlly(bet.TargetType);
			}
			if (bet.CasterType == CActor.EType.Enemy)
			{
				return HasEnemy(bet.TargetType);
			}
		}
		else if (bet.TargetActorType == CActor.EType.Enemy)
		{
			if (bet.CasterType == CActor.EType.Player || bet.CasterType == CActor.EType.HeroSummon)
			{
				return HasEnemy(bet.TargetType);
			}
			if (bet.CasterType == CActor.EType.Enemy)
			{
				return HasAlly(bet.TargetType);
			}
		}
		return false;
	}

	private static bool HasAlly(ActorType filterType)
	{
		if (filterType == ActorType.EnemyAndAllyAndEmpty || filterType == ActorType.AllyOnly || filterType == ActorType.AllyAndEmpty || filterType == ActorType.EnemyAndAlly)
		{
			return true;
		}
		return false;
	}

	private static bool HasEnemy(ActorType filterType)
	{
		if (filterType == ActorType.EnemyAndAllyAndEmpty || filterType == ActorType.EnemyOnly || filterType == ActorType.EnemyAndEmpty || filterType == ActorType.EnemyAndAlly)
		{
			return true;
		}
		return false;
	}

	private static bool HasEmpty(ActorType filterType)
	{
		if (filterType == ActorType.EnemyAndAllyAndEmpty || filterType == ActorType.AllyAndEmpty || filterType == ActorType.EnemyAndEmpty || filterType == ActorType.EmptyOnly)
		{
			return true;
		}
		return false;
	}
}
