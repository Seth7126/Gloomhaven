#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using Photon.Bolt;
using ScenarioRuleLibrary;
using SharedLibrary;
using SharedLibrary.Client;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CharacterManager : MonoBehaviour, IControllable
{
	public enum SurfaceTypes
	{
		None,
		Flesh,
		Stone,
		Bone
	}

	[Serializable]
	public class CharacterWeaponList
	{
		[Tooltip("This should be set and should be unique.  It can be referenced by other scripts (such as the Weapon Trails) to find a specific Weapon List")]
		public string name;

		[Tooltip("The name of the bone that this weapon will be attached to")]
		public string AttachToBone;

		[Tooltip("A list of potential weapons to equip to the specified bone.  One will be randomly selected at runtime.")]
		public List<GameObject> Weapons;
	}

	public AssetReference CharacterChildPrefabReference;

	[Tooltip("The type of surface that this character has.  Used to determine what hit sound effect to play.")]
	public SurfaceTypes m_SurfaceType;

	[Tooltip("The Hit Effect to play when this character receives standard damage.  If this is left empty the default from the GlobalSettings will be used.")]
	public GameObject StandardHitEffect;

	[Tooltip("The Hit Effect to play when this character receives zero damage.  If this is left empty the default from the GlobalSettings will be used.")]
	public GameObject ShieldHitEffect;

	[Tooltip("The Hit Effect to play when this character receives critical damage.  If this is left empty the default from the GlobalSettings will be used.")]
	public GameObject CriticalHitEffect;

	[Tooltip("The name of the bone that should be targeted by projectiles fired at this character")]
	public string TargetBone = "C_spineChestHook_LOC";

	[Tooltip("The radius of a circle around the target area.  The projectile will hit somewhere within this circle")]
	public float HitRadius = 0.1f;

	[Tooltip("The height of the character, used in overhead UI")]
	public float Height = 1.8f;

	[Tooltip("The list of weapons that are equipped by this character")]
	public List<CharacterWeaponList> WeaponLists = new List<CharacterWeaponList>();

	public bool NonMercenaryPrefab;

	[ConditionalField("NonMercenaryPrefab", null, true)]
	public Sprite CharacterCardBackgroundSprite;

	[Header("Preview")]
	public float glowMaterialPreview = 10f;

	public float oppacityMaterialPreview = 0.588f;

	[NonSerialized]
	public AsyncOperationHandle<GameObject> CharacterChildPrefabInstance;

	[NonSerialized]
	public CActor CharacterActor;

	[NonSerialized]
	public GameObject CharacterActorGO;

	[NonSerialized]
	public List<GameObject> EquippedWeapons = new List<GameObject>();

	[NonSerialized]
	public GameObject TargetBoneInstance;

	[NonSerialized]
	public GameObject CharacterPrefab;

	[NonSerialized]
	public List<GameObject> GoldPilesToCollect = new List<GameObject>();

	[NonSerialized]
	public List<GameObject> ChestsToCollect = new List<GameObject>();

	private static Dictionary<string, HashSet<int>> s_WeaponAssignment = new Dictionary<string, HashSet<int>>();

	private Renderer[] m_Renderers = new Renderer[0];

	private LTDescr m_FadeOutTween;

	private LTDescr m_FadeInTween;

	private float m_InvisibilityValue;

	private float m_GlowValue;

	private float m_OpacityValue;

	private const string DebugCancel = "CharacterManager";

	public bool IsParticipant => true;

	public bool IsAlive => !CharacterActor.IsDead;

	private void OnDestroy()
	{
		if (m_FadeOutTween != null)
		{
			LeanTween.cancel(m_FadeOutTween.id);
			m_FadeOutTween = null;
		}
		if (m_FadeInTween != null)
		{
			LeanTween.cancel(m_FadeInTween.id);
			m_FadeInTween = null;
		}
		ReleaseAddressables();
	}

	public static void WarmUpCharacter(GameObject prefab)
	{
		ObjectPool.CreateOrAddToPool(prefab, 1);
		ObjectPool.CreateOrAddToPool(Choreographer.s_Choreographer.m_ActorPrefab, 1);
		ActorBehaviour actorBehaviour = Choreographer.s_Choreographer.m_ActorPrefab?.GetComponent<ActorBehaviour>();
		if (actorBehaviour != null)
		{
			ObjectPool.CreateOrAddToPool(actorBehaviour.WorldspacePanelUIPrefab, 1);
		}
		CharacterManager component = prefab.GetComponent<CharacterManager>();
		if (component == null)
		{
			return;
		}
		if (component.StandardHitEffect != null)
		{
			ObjectPool.CreatePool(component.StandardHitEffect, 1);
		}
		if (component.ShieldHitEffect != null)
		{
			ObjectPool.CreatePool(component.ShieldHitEffect, 1);
		}
		if (component.CriticalHitEffect != null)
		{
			ObjectPool.CreatePool(component.CriticalHitEffect, 1);
		}
		foreach (GameObject item in component.WeaponLists.SelectMany((CharacterWeaponList wl) => wl.Weapons))
		{
			ObjectPool.CreateOrAddToPool(item, 1);
		}
	}

	public void InitialiseCharacter(bool isPreview = false)
	{
		ReleaseAddressables();
		CharacterChildPrefabInstance = Addressables.InstantiateAsync(CharacterChildPrefabReference.RuntimeKey, base.transform, instantiateInWorldSpace: false, trackHandle: false);
		CharacterChildPrefabInstance.WaitForCompletion();
		if (CharacterChildPrefabInstance.Result == null)
		{
			Debug.LogError("Character child " + base.gameObject.name + " no loaded.");
		}
		else
		{
			InitialiseCharacterStepTwo(isPreview);
		}
	}

	public IEnumerator InitialiseCharacterAsync(bool isPreview = false)
	{
		ReleaseAddressables();
		CharacterChildPrefabInstance = Addressables.InstantiateAsync(CharacterChildPrefabReference.RuntimeKey, base.transform, instantiateInWorldSpace: false, trackHandle: false);
		while (CharacterChildPrefabInstance.IsValid() && !CharacterChildPrefabInstance.IsDone)
		{
			yield return null;
		}
		if (CharacterChildPrefabInstance.Status != AsyncOperationStatus.Succeeded)
		{
			Debug.LogWarning("Character child " + base.gameObject.name + " no loaded.");
		}
		else
		{
			InitialiseCharacterStepTwo(isPreview);
		}
	}

	public void InitialiseCharacterStepTwo(bool isPreview = false)
	{
		TargetBoneInstance = base.gameObject.FindInChildren(TargetBone);
		if (TargetBoneInstance == null)
		{
			Debug.LogError("Error: Unable to find bone " + TargetBone + " on character " + base.gameObject.name + ".  Make sure this property is set correctly on this characters prefab.");
		}
		foreach (CharacterWeaponList weaponList in WeaponLists)
		{
			int nextWeaponIndex = GetNextWeaponIndex(base.gameObject.name + weaponList.name, weaponList.Weapons.Count);
			if (weaponList.Weapons.Count < nextWeaponIndex + 1)
			{
				Debug.LogError("Error: Unable to find weapon " + nextWeaponIndex + " on character " + base.gameObject.name + ".  Make sure this property is set correctly on this characters prefab.");
			}
			else
			{
				DoEquip(weaponList.Weapons[nextWeaponIndex], weaponList.AttachToBone);
			}
		}
		if (!isPreview && SaveData.Instance.Global.CurrentGameState == EGameState.Scenario && CharacterActor.OriginalType == CActor.EType.Player && (SaveData.Instance.Global.GameMode != EGameMode.Guildmaster || SaveData.Instance.Global.CurrentAdventureData.AdventureMapState.HeadquartersState.MultiplayerUnlocked))
		{
			CPlayerActor playerActor = (CPlayerActor)CharacterActor;
			if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
			{
				if (playerActor.CharacterName == null)
				{
					CMapCharacter cMapCharacter = AdventureState.MapState.MapParty.SelectedCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == playerActor.Class.ID);
					playerActor.CharacterName = cMapCharacter.CharacterName;
					ControllableRegistry.CreateControllable(cMapCharacter.CharacterName.GetHashCode(), this);
				}
				else
				{
					ControllableRegistry.CreateControllable(playerActor.CharacterName.GetHashCode(), this);
				}
			}
			else
			{
				ControllableRegistry.CreateControllable((CharacterActor.Class as CCharacterClass).ModelInstanceID, this);
			}
		}
		m_Renderers = GetComponentsInChildren<Renderer>();
		m_InvisibilityValue = (isPreview ? 1 : 0);
		m_GlowValue = (isPreview ? glowMaterialPreview : 0f);
		m_OpacityValue = (isPreview ? oppacityMaterialPreview : 1f);
		RefreshVisibility();
	}

	public void DeinitializeCharacter()
	{
		if (WorldspaceUITools.Instance != null)
		{
			WorldspaceUITools.Instance.DeregisterActorOutlinable(CharacterActor);
		}
		if (!PlatformLayer.Setting.LessObjectPooling)
		{
			foreach (GameObject equippedWeapon in EquippedWeapons)
			{
				if (equippedWeapon != null)
				{
					PlayableDirector[] componentsInChildren = equippedWeapon.GetComponentsInChildren<PlayableDirector>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						ObjectPool.Recycle(componentsInChildren[i].gameObject, null, destroy: true);
					}
					ObjectPool.Recycle(equippedWeapon);
				}
			}
		}
		EquippedWeapons.Clear();
		ReleaseAddressables();
		if (CharacterActorGO != null)
		{
			UnityEngine.Object.Destroy(CharacterActorGO);
		}
		StopFadeAnimations();
		m_Renderers = null;
	}

	private void ReleaseAddressables()
	{
		if (CharacterChildPrefabInstance.IsValid())
		{
			AssetBundleManager.ReleaseHandle(CharacterChildPrefabInstance, releaseInstance: true);
			CharacterChildPrefabInstance = default(AsyncOperationHandle<GameObject>);
		}
	}

	public void DoEquip(GameObject weapon, string attachToBone)
	{
		Transform bone = base.gameObject.GetComponentsInChildren<Transform>(includeInactive: true).FirstOrDefault((Transform x) => x.name == attachToBone);
		DoEquip(weapon, bone);
	}

	public void DoEquip(GameObject weapon, Transform bone)
	{
		try
		{
			if (!(weapon != null))
			{
				return;
			}
			if (bone != null)
			{
				Weapon component = weapon.GetComponent<Weapon>();
				if (component == null)
				{
					Debug.LogError("Error: Weapon " + weapon.name + " does not have the Weapon component attached.  Unable to load.");
					return;
				}
				GameObject gameObject = ObjectPool.Spawn(weapon, bone, weapon.transform.position, Quaternion.LookRotation(bone.forward, bone.up));
				if (component.WeaponType == Weapon.WeaponTypes.Melee)
				{
					WeaponCollision weaponCollision = gameObject.gameObject.GetComponent<WeaponCollision>();
					if (weaponCollision == null)
					{
						weaponCollision = gameObject.gameObject.AddComponent<WeaponCollision>();
					}
					weaponCollision.enabled = false;
				}
				gameObject.name = weapon.name;
				EquippedWeapons.Add(gameObject);
			}
			else
			{
				Debug.LogError("Error: Null bone passed to DoEquip for GameObject " + base.gameObject.name + ".  Weapon has not been attached");
			}
		}
		catch (Exception ex)
		{
			Debug.Log("An exception occurred while trying to run DoEquip. \n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public Vector3 GetTargetZone()
	{
		if (TargetBoneInstance == null)
		{
			Debug.LogError("Error: Target Bone " + TargetBone + " in character " + base.gameObject.name + " is null.  Using characters transform as target position.");
			return base.gameObject.transform.position;
		}
		float num = ((SharedClient.GlobalRNG.Next(2) == 0) ? ((float)SharedClient.GlobalRNG.NextDouble() * HitRadius) : ((0f - (float)SharedClient.GlobalRNG.NextDouble()) * HitRadius));
		return new Vector3(y: ((SharedClient.GlobalRNG.Next(2) == 0) ? ((float)SharedClient.GlobalRNG.NextDouble() * HitRadius) : ((0f - (float)SharedClient.GlobalRNG.NextDouble()) * HitRadius)) + TargetBoneInstance.transform.position.y, x: num + TargetBoneInstance.transform.position.x, z: TargetBoneInstance.transform.position.z);
	}

	public static CharacterManager GetCharacterManager(GameObject character, bool suppressError = false)
	{
		if (character == null)
		{
			return null;
		}
		CharacterManager characterManager = character.GetComponent<CharacterManager>();
		if (characterManager == null)
		{
			characterManager = character.GetComponentInParent<CharacterManager>();
		}
		if (!suppressError && characterManager == null)
		{
			Debug.LogError("Error: GameObject " + character.name + " does not have a character manager component attached.");
		}
		return characterManager;
	}

	public static void EnableWeaponColliderForAttack(CActor actor)
	{
		EnableWeaponColliderForAttack(Choreographer.s_Choreographer.FindClientActorGameObject(actor));
	}

	public static void EnableWeaponColliderForAttack(GameObject actorGO)
	{
		WeaponCollision[] array = actorGO?.GetComponentsInChildren<WeaponCollision>();
		if (array != null)
		{
			WeaponCollision[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].enabled = true;
			}
		}
	}

	public static GameObject FindTargetAtTile(CClientTile tile)
	{
		if (ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex) != null)
		{
			return Choreographer.s_Choreographer.FindClientActorGameObject(ScenarioManager.Scenario.FindActorAt(tile.m_Tile.m_ArrayIndex));
		}
		return null;
	}

	private int GetNextWeaponIndex(string characterName, int Count)
	{
		int num = 0;
		SharedLibrary.Random random = new SharedLibrary.Random();
		if (s_WeaponAssignment.TryGetValue(characterName, out var value))
		{
			if (value.Count == Count)
			{
				value.Clear();
			}
			num = GloomUtility.ExclusiveRandomNumber(random, value, Count - 1);
			value.Add(num);
		}
		else
		{
			num = random.Next(Count);
			s_WeaponAssignment.Add(characterName, new HashSet<int>());
			s_WeaponAssignment[characterName].Add(num);
		}
		return num;
	}

	public void RefreshVisibility()
	{
		if (m_Renderers == null)
		{
			return;
		}
		Renderer[] renderers = m_Renderers;
		foreach (Renderer renderer in renderers)
		{
			if (!(renderer == null))
			{
				for (int j = 0; j < renderer.materials.Length; j++)
				{
					Material material = renderer.materials[j];
					material.SetFloat("_InvisibilityControl", m_InvisibilityValue);
					material.SetFloat("_Glow", m_GlowValue);
					material.SetFloat("_Opacity", m_OpacityValue);
					float value = (Mathf.Approximately(0f, m_InvisibilityValue) ? 0f : 1f);
					material.SetFloat("_Toggle_Dissolve", value);
					renderer.materials[j] = material;
				}
			}
		}
	}

	public void FadeOutVisibility()
	{
		StopFadeAnimations();
		m_FadeOutTween = LeanTween.value(m_InvisibilityValue, 1f, 0.5f).setOnUpdate(delegate(float value)
		{
			m_InvisibilityValue = value;
			RefreshVisibility();
		}).setOnComplete((Action)delegate
		{
			m_FadeOutTween = null;
		});
	}

	public void FadeInVisibility()
	{
		StopFadeAnimations();
		m_FadeInTween = LeanTween.value(m_InvisibilityValue, 0f, 0.5f).setOnUpdate(delegate(float value)
		{
			m_InvisibilityValue = value;
			RefreshVisibility();
		}).setEaseInCubic()
			.setOnComplete((Action)delegate
			{
				m_FadeInTween = null;
			});
	}

	private void StopFadeAnimations()
	{
		if (m_FadeInTween != null)
		{
			LeanTween.cancel(m_FadeInTween.id, "CharacterManager");
		}
		m_FadeInTween = null;
		if (m_FadeOutTween != null)
		{
			LeanTween.cancel(m_FadeOutTween.id, "CharacterManager");
		}
		m_FadeOutTween = null;
	}

	public void OnControlAssigned(NetworkPlayer controller)
	{
		if (!(controller == null) && !(PlayerRegistry.MyPlayer == null) && PlayerRegistry.MyPlayer.PlayerID == controller.PlayerID)
		{
			CharacterActor.IsUnderMyControl = true;
			FFSNet.Console.LogInfo(LocalizationManager.GetTranslation(CharacterActor.ActorLocKey()) + "(SCENARIO) is now under my control.");
		}
	}

	public void OnControlReleased()
	{
		if (CharacterActor.IsUnderMyControl)
		{
			CharacterActor.IsUnderMyControl = false;
			FFSNet.Console.LogInfo(LocalizationManager.GetTranslation(CharacterActor.ActorLocKey()) + "(SCENARIO) is no longer under my control.");
		}
	}

	public PrefabId GetNetworkEntityPrefabID()
	{
		return BoltPrefabs.GHControllableState;
	}

	public string GetName()
	{
		if (CharacterActor == null)
		{
			return "UNINITIALIZED";
		}
		return CharacterActor.Class.ID;
	}
}
