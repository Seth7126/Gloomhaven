using System.Collections;
using System.Collections.Generic;
using AsmodeeNet.Foundation;
using BeautifyEffect;
using MapRuleLibrary.Adventure;
using ScenarioRuleLibrary;
using Script.GUI.PartyDisplay.Assembly;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Character3DDisplayManager : Singleton<Character3DDisplayManager>
{
	private class Character3D
	{
		private List<CharDisplayInfo> models;

		public ECharacter TypeCharacter { get; set; }

		public string Skin { get; set; }

		public Character3D(List<CharDisplayInfo> models)
		{
			this.models = models;
		}

		public void Show(string animation)
		{
			for (int i = 0; i < models.Count; i++)
			{
				models[i].CharacterManager.gameObject.SetActive(value: true);
				if (animation != string.Empty)
				{
					MF.GameObjectAnimatorPlay(models[i].CharacterManager.gameObject, animation);
				}
			}
		}

		public void Hide()
		{
			for (int i = 0; i < models.Count; i++)
			{
				models[i].CharacterManager.gameObject.SetActive(value: false);
			}
		}

		public void Destroy()
		{
			if (CoreApplication.IsQuitting)
			{
				return;
			}
			for (int i = 0; i < models.Count; i++)
			{
				if (models[i] != null)
				{
					if ((bool)models[i].CharacterManager)
					{
						models[i].CharacterManager.gameObject.SetActive(value: false);
						models[i].CharacterManager.DeinitializeCharacter();
					}
					AssetBundleManager.Instance.UnloadAsyncCharacterPrefabFromBundle(models[i].EType, models[i].Character, models[i].Skin);
				}
			}
			models.Clear();
		}
	}

	private class LoadedNewCharacter
	{
		public Character3D Character { get; set; }
	}

	private class LoadedCharacterManager
	{
		public CharacterManager Manager { get; set; }

		public CActor.EType Type { get; set; }

		public string Character { get; set; }

		public string Skin { get; set; }
	}

	[SerializeField]
	private Transform character3DHolder;

	[SerializeField]
	private Beautify beautify;

	[SerializeField]
	private Character3DDisplayCameraSettings _cameraSettings;

	private bool isHidden = true;

	private Character3D character3D;

	private Dictionary<string, Character3D> characterModels = new Dictionary<string, Character3D>();

	private HashSet<Component> showRequests = new HashSet<Component>();

	private ECharacter m_CurrentCharacterToDisplay;

	private string _currentSkin;

	private readonly Dictionary<ECharacter, Coroutine> _characterLoadingCoroutines = new Dictionary<ECharacter, Coroutine>();

	private void Start()
	{
		SetInstance(this);
		Singleton<Character3DDisplayManager>.Instance.gameObject.SetActive(value: true);
	}

	public void Display(Component request, ECharacter character, string skin = "", string playAnimation = "")
	{
		isHidden = false;
		if (skin == null)
		{
			skin = string.Empty;
		}
		base.gameObject.SetActive(value: true);
		beautify.enabled = true;
		if (character3D != null && character3D.TypeCharacter == character && character3D.Skin == skin)
		{
			character3D.Show(playAnimation);
			return;
		}
		showRequests.Add(request);
		if (character3D != null && (character3D.TypeCharacter != character || character3D.Skin != skin))
		{
			character3D.Hide();
			if (PlatformLayer.Setting.ForceFreeMemoryCharacter3DDisplay)
			{
				UnloadCurrentCharacter();
			}
		}
		if (!_characterLoadingCoroutines.ContainsKey(character))
		{
			m_CurrentCharacterToDisplay = character;
			_currentSkin = skin;
			Coroutine value = StartCoroutine(LoadCharacter(character, skin, playAnimation));
			_characterLoadingCoroutines.Add(character, value);
		}
	}

	private IEnumerator LoadCharacter(ECharacter character, string skin, string playAnimation)
	{
		LoadedNewCharacter loadedNewCharacter = new LoadedNewCharacter();
		yield return CreateCharacter(character, loadedNewCharacter, skin);
		Character3D character2 = loadedNewCharacter.Character;
		if (character2.TypeCharacter != m_CurrentCharacterToDisplay || character2.Skin != _currentSkin)
		{
			if (PlatformLayer.Setting.ForceFreeMemoryCharacter3DDisplay)
			{
				character2.Destroy();
			}
			else
			{
				character2.Hide();
			}
			_characterLoadingCoroutines.Remove(character);
			yield break;
		}
		if (character3D != null && (character3D.TypeCharacter != character2.TypeCharacter || character3D.Skin != character2.Skin))
		{
			if (PlatformLayer.Setting.ForceFreeMemoryCharacter3DDisplay)
			{
				UnloadCurrentCharacter();
			}
			else
			{
				character3D.Hide();
			}
		}
		character3D = character2;
		if (!isHidden)
		{
			character2.Show(playAnimation);
		}
		_characterLoadingCoroutines.Remove(character);
	}

	private IEnumerator CreateCharacter(ECharacter character, LoadedNewCharacter loadedNewCharacter, string skin = null)
	{
		string id = character.ToString() + (skin ?? string.Empty);
		if (!PlatformLayer.Setting.ForceFreeMemoryCharacter3DDisplay && characterModels.ContainsKey(id))
		{
			loadedNewCharacter.Character = characterModels[id];
			yield break;
		}
		CharacterConfigUI config = UIInfoTools.Instance.GetCharacterConfigUI(character, useDefault: false);
		List<CharDisplayInfo> models = new List<CharDisplayInfo>();
		LoadedCharacterManager main = new LoadedCharacterManager();
		yield return CreateModel(CActor.EType.Player, character.ToString(), main, skin, config?.GetAssemblyCharacter3DPlacement(AdventureState.MapState == null || !AdventureState.MapState.IsCampaign)?.position, config?.GetAssemblyCharacter3DPlacement(AdventureState.MapState == null || !AdventureState.MapState.IsCampaign)?.rotation);
		models.Add(new CharDisplayInfo
		{
			CharacterManager = main.Manager,
			EType = main.Type,
			Character = main.Character,
			Skin = main.Skin
		});
		if (config != null)
		{
			for (int i = 0; i < config.assemblyCharacter3DCompanions.Count; i++)
			{
				CharacterConfigUI.Object3DPlacement object3DPlacement = config.assemblyCharacter3DCompanions[i].GetObject3DPlacement(AdventureState.MapState == null || !AdventureState.MapState.IsCampaign);
				LoadedCharacterManager companion = new LoadedCharacterManager();
				yield return CreateModel(CActor.EType.HeroSummon, config.assemblyCharacter3DCompanions[i].summon.ToString(), companion, skin, object3DPlacement.position, object3DPlacement.rotation);
				if (companion.Manager != null)
				{
					models.Add(new CharDisplayInfo
					{
						CharacterManager = companion.Manager,
						EType = companion.Type,
						Character = companion.Character,
						Skin = companion.Skin
					});
				}
			}
		}
		characterModels[id] = new Character3D(models)
		{
			TypeCharacter = character,
			Skin = skin
		};
		loadedNewCharacter.Character = characterModels[id];
	}

	private IEnumerator CreateModel(CActor.EType type, string character, LoadedCharacterManager loadedCharacterManager, string skin = null, Vector3? position = null, Vector3? rotation = null)
	{
		AsyncOperationHandle<GameObject> handle = AssetBundleManager.Instance.GetAsyncCharacterPrefabFromBundle(type, character, skin, lazyLoadIfMissing: true, character3DHolder);
		if (PlatformLayer.Setting.ForceFreeMemoryCharacter3DDisplay)
		{
			yield return handle;
		}
		else
		{
			handle.WaitForCompletion();
		}
		switch (handle.Status)
		{
		case AsyncOperationStatus.Succeeded:
		{
			CharacterManager characterManager = CharacterManager.GetCharacterManager(handle.Result);
			if (PlatformLayer.Setting.ForceFreeMemoryCharacter3DDisplay)
			{
				yield return characterManager.InitialiseCharacterAsync();
			}
			else
			{
				characterManager.InitialiseCharacter();
			}
			characterManager.transform.localEulerAngles = rotation ?? Vector3.zero;
			if (!position.HasValue)
			{
				characterManager.transform.localPosition = new Vector3(0f, (0f - characterManager.Height) / 2f, 0f);
			}
			else
			{
				characterManager.transform.position = position.Value;
			}
			loadedCharacterManager.Manager = characterManager;
			loadedCharacterManager.Type = type;
			loadedCharacterManager.Character = character;
			loadedCharacterManager.Skin = skin;
			_cameraSettings.UpdateCharacter(character);
			break;
		}
		case AsyncOperationStatus.None:
		case AsyncOperationStatus.Failed:
			Debug.LogError($"Failed to load character {handle.Status} {handle.OperationException}");
			loadedCharacterManager.Manager = null;
			loadedCharacterManager.Type = CActor.EType.Unknown;
			loadedCharacterManager.Character = string.Empty;
			loadedCharacterManager.Skin = string.Empty;
			break;
		}
	}

	protected override void OnDestroy()
	{
		if (CoreApplication.IsQuitting)
		{
			return;
		}
		foreach (Character3D value in characterModels.Values)
		{
			value.Destroy();
		}
		base.OnDestroy();
	}

	public void Hide(Component request)
	{
		showRequests.Remove(request);
		isHidden = true;
		if (showRequests.Count == 0)
		{
			beautify.enabled = false;
		}
		if (character3D != null)
		{
			character3D.Hide();
		}
	}

	public void HideAll(Component request)
	{
		showRequests.Remove(request);
		isHidden = true;
		if (showRequests.Count == 0)
		{
			base.gameObject.SetActive(value: false);
			beautify.enabled = false;
		}
		if (character3D != null)
		{
			character3D.Hide();
			if (PlatformLayer.Setting.ForceFreeMemoryCharacter3DDisplay)
			{
				UnloadCurrentCharacter();
			}
		}
	}

	private void UnloadCurrentCharacter()
	{
		if (character3D != null)
		{
			string key = m_CurrentCharacterToDisplay.ToString() + (_currentSkin ?? string.Empty);
			if (characterModels.ContainsKey(key))
			{
				characterModels[key].Destroy();
				characterModels.Remove(key);
			}
			character3D.Destroy();
			character3D = null;
		}
	}
}
