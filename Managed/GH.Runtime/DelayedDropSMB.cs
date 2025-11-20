using System;
using System.Collections.Generic;
using Chronos;
using ScenarioRuleLibrary;
using Script.Controller;
using SharedLibrary.SimpleLog;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class DelayedDropSMB : StateMachineBehaviour
{
	public enum ItemType
	{
		General,
		MoneyToken,
		Obstacle,
		Trap
	}

	[Tooltip("The prefab which will be spawned")]
	[SerializeField]
	private GameObject m_ItemPrefab;

	[Tooltip("If enabled ignore SpawnDelay property and spawn always at the end of animation clip")]
	[SerializeField]
	private bool m_OnAnimationEnd;

	[Tooltip("Delay amount of seconds ater animation start playing")]
	[SerializeField]
	private float m_SpawnDelay;

	private float m_InstanceSpawnDelay;

	[Tooltip("Type of item to spawn")]
	[SerializeField]
	private ItemType m_Type;

	private bool m_NotSpawned;

	private List<CClientTile> m_TargetTiles;

	private AnimatorStateInfo m_StateInfo;

	private CActor m_ActorPreemptivelyLooting;

	private CActor m_ActorToRemoveGoldPileFrom;

	private GameObject m_GoldPileObjectWeWantToRemoveFromPlayer;

	private static List<DelayedDropSMB> s_DelayedDropsInProgress = new List<DelayedDropSMB>();

	private float m_OriginalSpawnDelay;

	private List<Tuple<CObjectProp, CClientTile>> m_DroppedProps;

	public AnimatorStateInfo StateInfo => m_StateInfo;

	private void Awake()
	{
		m_OriginalSpawnDelay = m_SpawnDelay;
	}

	private void OnDestroy()
	{
		if (m_NotSpawned)
		{
			Spawn();
		}
		if (s_DelayedDropsInProgress.Contains(this))
		{
			s_DelayedDropsInProgress.Remove(this);
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		float num = animatorStateInfo.length;
		if (float.IsInfinity(num))
		{
			num = 1f;
		}
		animator.speed = Timekeeper.instance.m_GlobalClock.timeScale;
		if (Timekeeper.instance.m_GlobalClock.timeScale != 0f)
		{
			num /= Timekeeper.instance.m_GlobalClock.timeScale;
		}
		s_DelayedDropsInProgress.Add(this);
		m_StateInfo = animatorStateInfo;
		m_NotSpawned = true;
		m_InstanceSpawnDelay = (m_OnAnimationEnd ? num : Mathf.Min(m_SpawnDelay, num));
		m_ActorPreemptivelyLooting = null;
		m_DroppedProps = new List<Tuple<CObjectProp, CClientTile>>();
		CharacterManager componentInParent = animator.gameObject.GetComponentInParent<CharacterManager>();
		CActor cActor = ((componentInParent != null) ? componentInParent.CharacterActor : null);
		ItemType type = m_Type;
		if (type != ItemType.MoneyToken && (uint)(type - 2) <= 1u)
		{
			m_TargetTiles = Choreographer.s_Choreographer.m_LastSelectedTiles;
			if (m_TargetTiles == null)
			{
				m_TargetTiles = new List<CClientTile> { Choreographer.s_Choreographer.m_lastSelectedTile };
			}
		}
		else if (componentInParent != null && cActor != null)
		{
			if (cActor.OriginalType == CActor.EType.Enemy && !cActor.NoGoldDrop && (ScenarioManager.HouseRulesSettings.HasFlag(StateShared.EHouseRulesFlag.FrosthavenSpawnGold) || !(cActor as CEnemyActor).IsSummon))
			{
				CClientTile item = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[cActor.ArrayIndex.X, cActor.ArrayIndex.Y];
				m_TargetTiles = new List<CClientTile> { item };
			}
			cActor.NoGoldDrop = false;
		}
		if (m_TargetTiles == null)
		{
			return;
		}
		foreach (CClientTile targetTile in m_TargetTiles)
		{
			CObjectProp item2 = null;
			switch (m_Type)
			{
			case ItemType.MoneyToken:
				item2 = new CObjectGoldPile(EPropType.GoldPile.ToString(), ScenarioManager.ObjectImportType.MoneyToken, targetTile.m_Tile.m_HexMap.MapGuid, cActor);
				if (componentInParent != null && cActor != null)
				{
					SimpleLog.AddToSimpleLog("Actor " + cActor.Class.ID + " dropped a Money Token");
				}
				break;
			case ItemType.Obstacle:
				item2 = new CObjectObstacle(EPropType.OneHexObstacle.ToString(), ScenarioManager.ObjectImportType.Obstacle, targetTile.m_Tile.m_HexMap.MapGuid);
				break;
			case ItemType.Trap:
				item2 = new CObjectTrap(EPropType.Trap.ToString(), ScenarioManager.ObjectImportType.Trap, targetTile.m_Tile.m_HexMap.MapGuid, new List<CCondition.ENegativeCondition>(), damage: true, ScenarioManager.Scenario.SLTE.TrapDamage);
				break;
			}
			m_DroppedProps.Add(new Tuple<CObjectProp, CClientTile>(item2, targetTile));
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		m_InstanceSpawnDelay -= Timekeeper.instance.m_GlobalClock.deltaTime;
		if (m_InstanceSpawnDelay <= 0f && m_NotSpawned)
		{
			Spawn();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (m_NotSpawned)
		{
			Spawn();
		}
	}

	private void Spawn()
	{
		m_NotSpawned = false;
		if (m_DroppedProps == null || m_DroppedProps.Count == 0)
		{
			s_DelayedDropsInProgress.Remove(this);
			return;
		}
		foreach (Tuple<CObjectProp, CClientTile> droppedProp in m_DroppedProps)
		{
			CObjectProp item = droppedProp.Item1;
			CClientTile item2 = droppedProp.Item2;
			if (item == null)
			{
				continue;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(m_ItemPrefab, item2.m_GameObject.transform.parent);
			if (gameObject.GetComponent<UnityGameEditorObject>() == null)
			{
				Debug.LogError("No UnityGameEditorObject script found on " + m_Type);
				UnityEngine.Object.Destroy(gameObject);
				continue;
			}
			Singleton<ObjectCacheService>.Instance.AddProp(item, gameObject);
			gameObject.name = item.InstanceName;
			gameObject.transform.position = item2.m_GameObject.transform.position;
			PropParent component = gameObject.GetComponent<PropParent>();
			if (component != null)
			{
				component.PlacedInScenario = false;
			}
			item.SetLocation(new TileIndex(item2.m_Tile.m_ArrayIndex), GloomUtility.VToCV(gameObject.transform.position), GloomUtility.VToCV(gameObject.transform.eulerAngles));
			item2.m_Tile.SpawnProp(item, notifyClient: false);
			switch (m_Type)
			{
			case ItemType.Trap:
			{
				SpawnPFXOnEnable componentInChildren4 = gameObject.GetComponentInChildren<SpawnPFXOnEnable>();
				if (componentInChildren4 != null)
				{
					componentInChildren4.enabled = true;
				}
				break;
			}
			case ItemType.MoneyToken:
			{
				PlayableDirector componentInChildren3 = gameObject.GetComponentInChildren<PlayableDirector>();
				componentInChildren3?.Play();
				if (m_ActorPreemptivelyLooting != null)
				{
					m_ActorToRemoveGoldPileFrom = m_ActorPreemptivelyLooting;
					m_GoldPileObjectWeWantToRemoveFromPlayer = Singleton<ObjectCacheService>.Instance.GetPropObject(item);
					m_ActorPreemptivelyLooting.LootTile(item2.m_Tile);
					componentInChildren3.stopped -= PreemptiveLootingPostDirectorAction;
					componentInChildren3.stopped += PreemptiveLootingPostDirectorAction;
				}
				break;
			}
			case ItemType.Obstacle:
			{
				PlayableDirector componentInChildren = gameObject.GetComponentInChildren<PlayableDirector>();
				TimelineAssets componentInChildren2 = gameObject.GetComponentInChildren<TimelineAssets>();
				if (!(componentInChildren == null) && !(componentInChildren2 == null))
				{
					componentInChildren.playableAsset = componentInChildren2.m_CreateTimeline;
					componentInChildren.Play();
				}
				break;
			}
			}
		}
		m_ActorPreemptivelyLooting = null;
		s_DelayedDropsInProgress.Remove(this);
	}

	private void PreemptiveLootingPostDirectorAction(PlayableDirector playableDir)
	{
		TimelineAsset asset = playableDir.GetComponentInChildren<TimelineAssets>().FindTimelineAsset("LOOTPickUpTimeline ");
		playableDir.Play(asset);
		playableDir.stopped -= PreemptiveLootingPostDirectorAction;
		if (m_ActorToRemoveGoldPileFrom != null && !(m_GoldPileObjectWeWantToRemoveFromPlayer == null))
		{
			CharacterManager characterManager = CharacterManager.GetCharacterManager(Choreographer.s_Choreographer.FindClientActorGameObject(m_ActorToRemoveGoldPileFrom));
			if (m_GoldPileObjectWeWantToRemoveFromPlayer != null)
			{
				characterManager.GoldPilesToCollect.Remove(m_GoldPileObjectWeWantToRemoveFromPlayer);
			}
		}
	}

	public void SetActorToPreemptivelyLootOnTiles(CActor actorWaitingToLoot)
	{
		m_ActorPreemptivelyLooting = null;
		if (m_Type == ItemType.MoneyToken && m_TargetTiles != null && m_TargetTiles.Count != 0)
		{
			m_ActorPreemptivelyLooting = actorWaitingToLoot;
		}
	}

	public static bool DelayedDropsAreInProgress()
	{
		if (s_DelayedDropsInProgress.Count > 0)
		{
			s_DelayedDropsInProgress.RemoveAll((DelayedDropSMB i) => i == null);
		}
		return s_DelayedDropsInProgress.Count > 0;
	}
}
