#define ENABLE_LOGS
using System.Collections.Generic;
using AStar;
using ScenarioRuleLibrary;
using SharedLibrary.SimpleLog;
using UnityEngine;

public class ActorBehaviour : MonoBehaviour
{
	public GameObject m_Hilight;

	public GameObject m_Base;

	[HideInInspector]
	public WorldspacePanelUIController m_WorldspacePanelUI;

	public GameObject WorldspacePanelUIPrefabKeyboard;

	public GameObject WorldspacePanelUIPrefabGamepad;

	private const float c_MinimumAnimSpeed = 0.2f;

	private const float c_AnimSpeedRampScalar = 1f;

	private const float c_LookAtScalar = 100f;

	private CActor m_Actor;

	private Vector3 m_LocoIntermediateTarget;

	private Vector3 m_LocoLastWaypointTarget;

	private bool m_NewLocoTarget;

	private bool m_Teleport;

	private bool m_PushPull;

	private float m_StartingDistanceToIntermediateTarget;

	private GameObject m_RootGameObject;

	private GameObject m_AnimatedGameObject;

	private Animator m_Animator;

	private Vector3 m_VectorToTarget;

	private Vector3 m_StartPosition;

	private float m_TargetAnimSpeed;

	private bool m_StartedWithMidpoints;

	private float m_MidpointTargetAnimSpeed;

	private bool m_Jump;

	private Renderer[] m_Renderers;

	private Cloth[] m_Clothes;

	private bool m_PauseLoco;

	private CharacterManager m_ActorToFade;

	private int m_ForcingPositionChangeCounter;

	private float m_InvisibleControl;

	private bool m_WasInvisible;

	private GameObject m_InvisibilityIdleEffect;

	private bool m_IsPushPullInProgress;

	private bool m_InitialisedWorldspaceUI;

	private static readonly int _runBlend = Animator.StringToHash("RunBlend");

	public CActor Actor
	{
		get
		{
			return m_Actor;
		}
		set
		{
			m_Actor = value;
		}
	}

	public bool IsMoving { get; private set; }

	public GameObject WorldspacePanelUIPrefab
	{
		get
		{
			if (!InputManager.GamePadInUse)
			{
				return WorldspacePanelUIPrefabKeyboard;
			}
			return WorldspacePanelUIPrefabGamepad;
		}
	}

	private float c_FixUpdateTimeStep => Time.fixedDeltaTime;

	public static void SetActor(GameObject gameObject, CActor actor)
	{
		ActorBehaviour actorBehaviour = GetActorBehaviour(gameObject);
		actorBehaviour.m_Actor = actor;
		actorBehaviour.m_RootGameObject = gameObject;
		actorBehaviour.m_Animator = MF.GetGameObjectAnimator(actorBehaviour.m_RootGameObject);
		actorBehaviour.m_AnimatedGameObject = (actorBehaviour.m_Animator ? actorBehaviour.m_Animator.gameObject : null);
		actorBehaviour.m_Animator.enabled = true;
		actorBehaviour.m_LocoIntermediateTarget = gameObject.transform.position;
		actorBehaviour.CreateWorldSpaceGUIElements(gameObject, actor);
		actorBehaviour.m_WorldspacePanelUI.UpdateHealth(actor.MaxHealth, actor.Health, actor.OriginalMaxHealth, animate: false);
		actorBehaviour.UpdateWorldspaceConditionsUI();
		actorBehaviour.m_Renderers = actorBehaviour.m_Animator.gameObject.GetComponentsInChildren<Renderer>();
		actorBehaviour.m_Clothes = actorBehaviour.m_Animator.gameObject.GetComponentsInChildren<Cloth>();
		Cloth[] clothes = actorBehaviour.m_Clothes;
		foreach (Cloth cloth in clothes)
		{
			if (PlatformLayer.Setting.SimplifyPhysics)
			{
				cloth.clothSolverFrequency = Singleton<PhysicsController>.Instance.SimplifyPhysicsRate;
			}
		}
	}

	public static bool IsPaused(GameObject gameObject)
	{
		ActorBehaviour actorBehaviour = GetActorBehaviour(gameObject);
		if (!(actorBehaviour == null))
		{
			return actorBehaviour.m_PauseLoco;
		}
		return false;
	}

	public void PauseLoco(bool pause)
	{
		m_Animator.enabled = !pause;
		m_PauseLoco = pause;
	}

	public static void SetHilighted(GameObject gameObject, bool hilight)
	{
		if (gameObject != null)
		{
			ActorBehaviour actorBehaviour = GetActorBehaviour(gameObject);
			if (actorBehaviour != null)
			{
				if (actorBehaviour.m_Hilight != null)
				{
					actorBehaviour.m_Hilight.SetActive(hilight);
				}
				else
				{
					Debug.LogWarning("Unable to get actor behaviour highlight");
				}
			}
		}
		else
		{
			Debug.LogWarning("Unable to get actor behaviour for highlighting");
		}
	}

	public static bool IsHilighted(GameObject gameObject)
	{
		return GetActorBehaviour(gameObject).m_Hilight.activeSelf;
	}

	public static CActor GetActor(GameObject gameObject)
	{
		return GetActorBehaviour(gameObject).m_Actor;
	}

	public static void UpdateHealth(GameObject gameObject, int actorOriginalHealth = 0, bool updateUI = false)
	{
		GetActorBehaviour(gameObject).UpdateHealth(actorOriginalHealth, updateUI);
	}

	public void UpdateHealth(int actorOriginalHealth = 0, bool updateUI = false)
	{
		m_WorldspacePanelUI.UpdateHealth(m_Actor.MaxHealth, m_Actor.Health, m_Actor.OriginalMaxHealth, animate: true, delegate
		{
			if (updateUI)
			{
				int num = Mathf.Max(m_Actor.Health, 0) - actorOriginalHealth;
				if (num > 0)
				{
					NotifyUIHealthIncrease(num, m_Actor.Health > m_Actor.OriginalMaxHealth && m_Actor.MaxHealth > m_Actor.OriginalMaxHealth);
				}
				else if (num < 0)
				{
					NotifyUIHealthDecrease(num);
				}
			}
		});
	}

	private void NotifyUIHealthIncrease(int amount, bool isOverheal)
	{
		m_WorldspacePanelUI.ShowHeal(amount, isOverheal);
	}

	private void NotifyUIHealthDecrease(int amount)
	{
		m_WorldspacePanelUI.ShowDamage(amount);
	}

	public void UpdateRetaliateDamage(int retaliateDamage, bool forceUpdate = false)
	{
		m_WorldspacePanelUI.UpdateRetaliateDamage(retaliateDamage, forceUpdate);
	}

	public static ActorBehaviour GetActorBehaviour(GameObject gameObject)
	{
		ActorBehaviour actorBehaviour = gameObject.GetComponent<ActorBehaviour>();
		if (actorBehaviour == null)
		{
			actorBehaviour = gameObject.GetComponentInChildren<ActorBehaviour>();
		}
		if (actorBehaviour == null)
		{
			actorBehaviour = gameObject.GetComponentInChildren<ActorBehaviour>(includeInactive: true);
		}
		return actorBehaviour;
	}

	public void CreateWorldSpaceGUIElements(GameObject actorGO, CActor actor)
	{
		CharacterManager component = actorGO.GetComponent<CharacterManager>();
		GameObject gameObject = ObjectPool.Spawn(WorldspacePanelUIPrefab, null, actorGO.transform.position, Quaternion.identity);
		gameObject.transform.SetParent(WorldspaceUITools.Instance.WorldspaceGUIPrefabLevel.transform, worldPositionStays: false);
		m_WorldspacePanelUI = gameObject.GetComponent<WorldspacePanelUIController>();
		m_WorldspacePanelUI.Init(actorGO, actor, component.Height);
		m_InitialisedWorldspaceUI = true;
	}

	public void ForceSetLocoIntermediateTarget(Vector3 target)
	{
		m_ForcingPositionChangeCounter = 2;
		m_LocoIntermediateTarget = target;
		Cloth[] clothes = m_Clothes;
		for (int i = 0; i < clothes.Length; i++)
		{
			clothes[i].enabled = false;
		}
	}

	public bool AtLocoIntermediateTarget()
	{
		return m_AnimatedGameObject.transform.position == m_LocoIntermediateTarget;
	}

	public void SetLocoTarget(Vector3 intermediateTarget, bool jump, List<CTile> waypoints)
	{
		m_LocoIntermediateTarget = intermediateTarget;
		if (waypoints != null && waypoints.Count > 0)
		{
			m_LocoLastWaypointTarget = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[waypoints[waypoints.Count - 1].m_ArrayIndex.X, waypoints[waypoints.Count - 1].m_ArrayIndex.Y].m_GameObject.transform.position;
		}
		else
		{
			m_LocoLastWaypointTarget = m_LocoIntermediateTarget;
		}
		m_StartingDistanceToIntermediateTarget = (m_LocoIntermediateTarget - base.transform.position).magnitude;
		m_StartPosition = base.transform.position;
		m_VectorToTarget = (intermediateTarget - base.transform.position).normalized;
		IsMoving = true;
		SaveData.Instance.Global.InvokeGameSpeedChanged();
		if (jump && !m_Jump && m_Animator.HasState(0, Animator.StringToHash("Jump Into")))
		{
			m_Jump = jump;
			m_Animator.Play("Jump Into");
			m_Animator.SetBool("ExitJump", value: false);
		}
		if (m_StartingDistanceToIntermediateTarget != 0f)
		{
			m_NewLocoTarget = true;
		}
		SimpleLog.AddToSimpleLog("Loco Target set for actor: " + Choreographer.GetActorIDForCombatLogIfNeeded(m_Actor));
	}

	public void FadeCharacterOnPosition(Point point)
	{
		if (m_ActorToFade != null && !m_ActorToFade.CharacterActor.Tokens.HasKey(CCondition.EPositiveCondition.Invisible))
		{
			m_ActorToFade.FadeInVisibility();
		}
		CActor cActor = ScenarioManager.Scenario.FindActorAt(point, m_Actor);
		if (cActor == null)
		{
			if (m_Actor != null && !m_Actor.Tokens.HasKey(CCondition.EPositiveCondition.Invisible))
			{
				GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(m_Actor);
				if (gameObject != null)
				{
					CharacterManager.GetCharacterManager(gameObject).FadeInVisibility();
				}
			}
			return;
		}
		GameObject gameObject2 = Choreographer.s_Choreographer.FindClientActorGameObject(cActor);
		if (!(gameObject2 == null))
		{
			m_ActorToFade = CharacterManager.GetCharacterManager(gameObject2);
			if (m_ActorToFade != null && !m_ActorToFade.CharacterActor.Tokens.HasKey(CCondition.EPositiveCondition.Invisible))
			{
				m_ActorToFade.FadeOutVisibility();
			}
		}
	}

	public void TeleportToLocation(Vector3 targetLocation)
	{
		m_LocoIntermediateTarget = targetLocation;
		m_Teleport = true;
		m_IsPushPullInProgress = false;
	}

	public void TeleportToCurrentLocoTarget()
	{
		_ = m_LocoIntermediateTarget;
		m_Teleport = true;
		m_IsPushPullInProgress = false;
		m_NewLocoTarget = false;
	}

	public void PushPullToLocation(Vector3 targetLocation)
	{
		m_IsPushPullInProgress = true;
		m_LocoLastWaypointTarget = (m_LocoIntermediateTarget = targetLocation);
		if (m_Animator.HasState(0, Animator.StringToHash("PushPull")))
		{
			m_Animator.Play("PushPull");
			m_PushPull = true;
		}
		else
		{
			TeleportToLocation(targetLocation);
		}
	}

	public void StopLoco(GameObject nextWaypoint)
	{
		m_NewLocoTarget = false;
		m_TargetAnimSpeed = 0f;
		m_StartedWithMidpoints = false;
		IsMoving = false;
		SaveData.Instance.Global.InvokeGameSpeedChanged();
		if (m_Jump && !nextWaypoint)
		{
			m_Animator.SetBool("ExitJump", value: true);
			m_Jump = false;
			Renderer[] componentsInChildren = m_Animator.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				if (renderer != null)
				{
					Material[] materials = renderer.materials;
					for (int j = 0; j < materials.Length; j++)
					{
						materials[j].SetFloat("_Opacity", 1f);
					}
				}
			}
		}
		SimpleLog.AddToSimpleLog("Loco stopped for actor: " + Choreographer.GetActorIDForCombatLogIfNeeded(m_Actor));
	}

	public bool AtLocoTarget()
	{
		if (!m_IsPushPullInProgress)
		{
			return !m_NewLocoTarget;
		}
		return false;
	}

	private void CheckInvisibility()
	{
		if (Actor == null)
		{
			return;
		}
		bool flag = Actor.Tokens.HasKey(CCondition.EPositiveCondition.Invisible);
		if (flag == m_WasInvisible)
		{
			return;
		}
		m_InvisibleControl += (flag ? c_FixUpdateTimeStep : (0f - c_FixUpdateTimeStep));
		m_InvisibleControl = Mathf.Min(Mathf.Max(m_InvisibleControl, 0f), 1f);
		Renderer[] renderers = m_Renderers;
		foreach (Renderer renderer in renderers)
		{
			if (!(renderer != null))
			{
				continue;
			}
			Material[] materials = renderer.materials;
			foreach (Material material in materials)
			{
				if (material.HasProperty("_Toggle_Dissolve"))
				{
					material.SetFloat("_Toggle_Dissolve", (m_InvisibleControl > 0f) ? 1 : 0);
				}
				if (material.HasProperty("_InvisibilityControl"))
				{
					material.SetFloat("_InvisibilityControl", m_InvisibleControl);
				}
			}
		}
		if ((flag && m_InvisibleControl == 1f) || (!flag && m_InvisibleControl == 0f))
		{
			m_WasInvisible = flag;
		}
		if (flag)
		{
			if (m_InvisibilityIdleEffect == null)
			{
				m_InvisibilityIdleEffect = ObjectPool.Spawn(GlobalSettings.Instance.m_JumpMoveParticleEffects.InvisibilityIdle, m_Animator.transform);
				ParticleSystem component = m_InvisibilityIdleEffect.GetComponent<ParticleSystem>();
				if (component != null)
				{
					component.Play();
				}
				component = ObjectPool.Spawn(GlobalSettings.Instance.m_JumpMoveParticleEffects.InvisibilitySmoke, m_Animator.transform).GetComponent<ParticleSystem>();
				if (component != null)
				{
					component.Play();
				}
			}
		}
		else if (m_InvisibilityIdleEffect != null)
		{
			ParticleSystem component2 = m_InvisibilityIdleEffect.GetComponent<ParticleSystem>();
			if (component2 != null)
			{
				component2.Stop();
			}
			ObjectPool.Recycle(m_InvisibilityIdleEffect, GlobalSettings.Instance.m_JumpMoveParticleEffects.InvisibilityIdle);
			m_InvisibilityIdleEffect = null;
		}
	}

	private void FixedUpdate()
	{
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario)
		{
			CheckInvisibility();
		}
	}

	private void Update()
	{
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario)
		{
			DoTransform(Time.deltaTime);
		}
	}

	private void DoTransform(float deltaTime)
	{
		if (m_AnimatedGameObject == null)
		{
			return;
		}
		float num = 1f;
		if (m_Teleport)
		{
			m_AnimatedGameObject.transform.position = m_LocoIntermediateTarget;
			m_RootGameObject.transform.position = m_LocoIntermediateTarget;
			num = 10f;
			Vector3 zero = Vector3.zero;
			foreach (CPlayerActor playerActor in ScenarioManager.Scenario.PlayerActors)
			{
				zero += Choreographer.s_Choreographer.FindClientActorGameObject(playerActor).transform.position;
			}
			zero /= (float)ScenarioManager.Scenario.PlayerActors.Count;
			m_RootGameObject.transform.LookAt(zero);
			m_Teleport = false;
		}
		else if (m_NewLocoTarget)
		{
			if ((m_LocoIntermediateTarget - m_LocoLastWaypointTarget).magnitude > 1f)
			{
				m_TargetAnimSpeed = Mathf.Min(m_TargetAnimSpeed + 1f * deltaTime, 1f);
				num = 10f;
				m_StartedWithMidpoints = true;
				m_MidpointTargetAnimSpeed = m_TargetAnimSpeed;
			}
			else
			{
				float num2 = Mathf.Min((m_LocoIntermediateTarget - m_RootGameObject.transform.position).magnitude / m_StartingDistanceToIntermediateTarget, 1f);
				if (m_StartedWithMidpoints)
				{
					m_TargetAnimSpeed = Mathf.Min(m_MidpointTargetAnimSpeed * num2 + 0.2f, 1f);
					m_MidpointTargetAnimSpeed = Mathf.Max(m_MidpointTargetAnimSpeed - deltaTime, 0f);
				}
				else
				{
					float num3 = ((num2 < 0.5f) ? (num2 * 2f) : (1f - (num2 - 0.5f) * 2f));
					m_TargetAnimSpeed = num3 * 1f + 0.2f;
				}
				num = 60f;
			}
			if (true)
			{
				Quaternion b = Quaternion.LookRotation((m_LocoIntermediateTarget - m_RootGameObject.transform.position).normalized, m_RootGameObject.transform.up);
				float t = Mathf.Max(1f, Mathf.Min(deltaTime * 100f * m_AnimatedGameObject.transform.localPosition.magnitude, 1f));
				m_RootGameObject.transform.rotation = Quaternion.Lerp(m_RootGameObject.transform.rotation, b, t);
			}
		}
		else if (m_PushPull)
		{
			num = 10f;
			m_TargetAnimSpeed = 0f;
			if ((m_AnimatedGameObject.transform.position - m_LocoLastWaypointTarget).magnitude > 0.1f)
			{
				m_AnimatedGameObject.transform.position = Vector3.Lerp(m_AnimatedGameObject.transform.position, m_LocoLastWaypointTarget, deltaTime * 10f);
			}
			else
			{
				m_IsPushPullInProgress = false;
				m_PushPull = false;
			}
		}
		else if (!m_StartedWithMidpoints)
		{
			m_AnimatedGameObject.transform.position = m_LocoIntermediateTarget;
			num = 10f;
			m_TargetAnimSpeed = 0f;
		}
		Vector3 euler = new Vector3(90f, 0f, -120f);
		m_Hilight.transform.rotation = Quaternion.Euler(euler);
		Quaternion rotation = Quaternion.FromToRotation(toDirection: new Vector3(0f, 1f, 0f), fromDirection: m_RootGameObject.transform.up) * m_RootGameObject.transform.rotation;
		m_RootGameObject.transform.rotation = rotation;
		if (!m_Jump && m_Animator.runtimeAnimatorController != null)
		{
			float num4 = m_Animator.GetFloat(_runBlend);
			m_Animator.SetFloat(_runBlend, num4 + (m_TargetAnimSpeed - num4) * Mathf.Min(deltaTime * num, 1f));
		}
	}

	private void LateUpdate()
	{
		if (m_ForcingPositionChangeCounter > 0)
		{
			m_ForcingPositionChangeCounter--;
			if (m_ForcingPositionChangeCounter == 0)
			{
				Cloth[] clothes = m_Clothes;
				for (int i = 0; i < clothes.Length; i++)
				{
					clothes[i].enabled = true;
				}
			}
		}
		if (SaveData.Instance.Global.CurrentGameState == EGameState.Scenario)
		{
			ApplyMotion();
		}
	}

	private void ApplyMotion()
	{
		if (m_AnimatedGameObject == null)
		{
			return;
		}
		Vector3 position = m_AnimatedGameObject.transform.position;
		if (m_NewLocoTarget)
		{
			Vector3 lhs = m_LocoIntermediateTarget - m_AnimatedGameObject.transform.position;
			bool flag = false;
			bool flag2 = Vector3.Dot(lhs, m_VectorToTarget) < 0f;
			if ((m_LocoIntermediateTarget - m_LocoLastWaypointTarget).magnitude > 1f)
			{
				if (flag2)
				{
					m_NewLocoTarget = false;
					flag = true;
				}
			}
			else if (flag2)
			{
				flag = true;
				StopLoco(Waypoint.GetNextWaypoint());
			}
			if (flag)
			{
				m_TargetAnimSpeed = 0f;
				position = m_LocoIntermediateTarget;
			}
			else
			{
				position = m_AnimatedGameObject.transform.position;
			}
		}
		else if (m_Jump || !Mathf.Approximately(m_Animator.GetFloat(_runBlend), 0f))
		{
			position = m_LocoIntermediateTarget;
		}
		m_RootGameObject.transform.position = position;
		m_AnimatedGameObject.transform.localPosition = Vector3.zero;
	}

	public void UpdateWorldspaceConditionsUI()
	{
		if (m_InitialisedWorldspaceUI && !m_Actor.IsDead)
		{
			bool blessed = false;
			bool poisoned = false;
			bool wounded = false;
			bool immobilized = false;
			bool disarmed = false;
			bool stunned = false;
			bool invisible = false;
			bool strengthened = false;
			bool muddled = false;
			bool cursed = false;
			bool pierce = false;
			bool immovable = false;
			bool sleep = false;
			bool blockHealing = false;
			bool flag = false;
			if (Actor.Tokens.HasKey(CCondition.ENegativeCondition.Poison))
			{
				poisoned = true;
			}
			if (Actor.Tokens.HasKey(CCondition.ENegativeCondition.Wound))
			{
				wounded = true;
			}
			if (Actor.Tokens.HasKey(CCondition.ENegativeCondition.Immobilize))
			{
				immobilized = true;
			}
			if (Actor.Tokens.HasKey(CCondition.ENegativeCondition.Disarm))
			{
				disarmed = true;
			}
			if (Actor.Tokens.HasKey(CCondition.ENegativeCondition.Stun))
			{
				stunned = true;
			}
			if (Actor.Tokens.HasKey(CCondition.ENegativeCondition.Muddle))
			{
				muddled = true;
			}
			if (Actor.Tokens.HasKey(CCondition.ENegativeCondition.Curse))
			{
				cursed = true;
			}
			if (Actor.Tokens.HasKey(CCondition.ENegativeCondition.Sleep))
			{
				sleep = true;
			}
			if (Actor.CachedHealingBlocked)
			{
				blockHealing = true;
			}
			if (Actor.CachedShieldNeutralized)
			{
				flag = true;
			}
			if (Actor.Tokens.HasKey(CCondition.EPositiveCondition.Strengthen))
			{
				strengthened = true;
			}
			if (Actor.Tokens.HasKey(CCondition.EPositiveCondition.Bless))
			{
				blessed = true;
			}
			if (Actor.Tokens.HasKey(CCondition.EPositiveCondition.Invisible))
			{
				invisible = true;
			}
			if (Actor.Tokens.HasKey(CCondition.EPositiveCondition.Immovable))
			{
				immovable = true;
			}
			bool retaliate = m_Actor.CachedHasRetaliate || (m_Actor.Class is CMonsterClass && ((CMonsterClass)m_Actor.Class).Retaliate > 0) || (m_Actor is CHeroSummonActor && ((CHeroSummonActor)m_Actor).SummonData.Retaliate > 0);
			bool controlled = m_Actor.MindControlDuration == CAbilityControlActor.EControlDurationType.ControlForOneAction || m_Actor.MindControlDuration == CAbilityControlActor.EControlDurationType.ControlForOneTurn;
			bool fly = m_Actor.CachedFlying || (m_Actor.Class is CMonsterClass && ((CMonsterClass)m_Actor.Class).Flying) || (m_Actor is CHeroSummonActor && ((CHeroSummonActor)m_Actor).Flying);
			int activeShields = ((m_Actor.CachedShieldValue == 0 && m_Actor.Class is CMonsterClass cMonsterClass) ? cMonsterClass.Shield : m_Actor.CachedShieldValue);
			List<CActiveBonus> cachedDoomActiveBonuses = m_Actor.CachedDoomActiveBonuses;
			List<CActiveBonus> cachedActiveItemEffectBonuses = m_Actor.CachedActiveItemEffectBonuses;
			m_WorldspacePanelUI.UpdateEffects(blessed, controlled, cursed, disarmed, immobilized, invisible, muddled, pierce, poisoned, retaliate, strengthened, stunned, wounded, immovable, Actor is CPlayerActor cPlayerActor && cPlayerActor.CharacterClass.ImprovedShortRest, fly, cachedDoomActiveBonuses, cachedActiveItemEffectBonuses, sleep, blockHealing, flag);
			m_WorldspacePanelUI.UpdateShields(activeShields, flag);
		}
	}
}
