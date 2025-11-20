#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FFSNet;
using I2.Loc;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using SharedLibrary.Logger;
using SharedLibrary.SimpleLog;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PersistentData : MonoBehaviour
{
	public static PersistentData s_Instance;

	public GameObject ObjectPoolPrefab;

	private const string MonsterCardPrefab = "MonsterRoundCard";

	private const string MonsterCardConsolePrefab = "MonsterRoundCard_gamepad";

	private const string GeneratedCardsListPath = "GeneratedCardsList";

	private const string AbilityCardPrefab = "AbilityCard";

	private const string AbilityCardConsolePrefab = "AbilityCard_gamepad";

	private const string LongRestPrefab = "LongRest card";

	private const string LongRestConsolePrefab = "LongRest card_gamepad";

	private const string ItemCardPrefab = "ItemCard";

	private const string ItemCardConsolePrefab = "ItemCard_gamepad";

	private List<DLLDebug.CDLLDebugLog_MessageData> m_MessageQueue = new List<DLLDebug.CDLLDebugLog_MessageData>();

	private DLCRegistry.EDLCKey _cachedDLCs;

	private DiagnosticsStopwatchFacade _diagnosticsStopwatchFacade;

	public bool IsDataLoaded { get; private set; }

	public bool FailedLoading { get; set; }

	public DLCRegistry.EDLCKey CachedDLCs => _cachedDLCs;

	[UsedImplicitly]
	private void Awake()
	{
		s_Instance = this;
		IsDataLoaded = false;
		FailedLoading = false;
		SharedClient.SetDLLDebugHandler(DLLDebugHandler);
		FFSNet.Console.OnLogDebug = (UnityAction<string, bool>)Delegate.Combine(FFSNet.Console.OnLogDebug, new UnityAction<string, bool>(SimpleLog.OnFFSNetConsoleLinePrinted));
		FFSNet.Console.OnLog = (UnityAction<string, bool>)Delegate.Combine(FFSNet.Console.OnLog, new UnityAction<string, bool>(SimpleLog.OnFFSNetConsoleLinePrinted));
		FFSNet.Console.OnLogInfo = (UnityAction<string, bool>)Delegate.Combine(FFSNet.Console.OnLogInfo, new UnityAction<string, bool>(SimpleLog.OnFFSNetConsoleLinePrinted));
		FFSNet.Console.OnLogCoreInfo = (UnityAction<string, bool>)Delegate.Combine(FFSNet.Console.OnLogCoreInfo, new UnityAction<string, bool>(SimpleLog.OnFFSNetConsoleLinePrinted));
		FFSNet.Console.OnLogWarning = (UnityAction<string, bool>)Delegate.Combine(FFSNet.Console.OnLogWarning, new UnityAction<string, bool>(SimpleLog.OnFFSNetConsoleLinePrinted));
		_diagnosticsStopwatchFacade = new DiagnosticsStopwatchFacade(new DiagnosticsConsoleLogger());
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		FFSNet.Console.OnLogDebug = (UnityAction<string, bool>)Delegate.Remove(FFSNet.Console.OnLogDebug, new UnityAction<string, bool>(SimpleLog.OnFFSNetConsoleLinePrinted));
		FFSNet.Console.OnLog = (UnityAction<string, bool>)Delegate.Remove(FFSNet.Console.OnLog, new UnityAction<string, bool>(SimpleLog.OnFFSNetConsoleLinePrinted));
		FFSNet.Console.OnLogInfo = (UnityAction<string, bool>)Delegate.Remove(FFSNet.Console.OnLogInfo, new UnityAction<string, bool>(SimpleLog.OnFFSNetConsoleLinePrinted));
		FFSNet.Console.OnLogCoreInfo = (UnityAction<string, bool>)Delegate.Remove(FFSNet.Console.OnLogCoreInfo, new UnityAction<string, bool>(SimpleLog.OnFFSNetConsoleLinePrinted));
		FFSNet.Console.OnLogWarning = (UnityAction<string, bool>)Delegate.Remove(FFSNet.Console.OnLogWarning, new UnityAction<string, bool>(SimpleLog.OnFFSNetConsoleLinePrinted));
		s_Instance = null;
	}

	private void Update()
	{
		lock (m_MessageQueue)
		{
			if (m_MessageQueue.Count <= 0)
			{
				return;
			}
			foreach (DLLDebug.CDLLDebugLog_MessageData item in m_MessageQueue)
			{
				if (item != null)
				{
					switch (item.m_LogType)
					{
					case EDLLDebug.Warning:
						Debug.LogWarning(item.m_Message + "\n" + item.m_Stack);
						break;
					case EDLLDebug.Error:
						Debug.LogError(item.m_Message + "\n" + item.m_Stack);
						break;
					case EDLLDebug.SimpleLog:
						Debug.Log(item.m_Message + "\n" + item.m_Stack);
						break;
					default:
						Debug.Log(item.m_Message);
						break;
					}
				}
			}
			DLLDebug.CDLLDebugLog_MessageData cDLLDebugLog_MessageData = m_MessageQueue.FirstOrDefault((DLLDebug.CDLLDebugLog_MessageData f) => f.m_LogType == EDLLDebug.Error && f.m_ShowError);
			if (cDLLDebugLog_MessageData != null && !SceneController.Instance.GlobalErrorMessage.ShowingMessage)
			{
				if (SceneController.Instance.InitialLoadingComplete)
				{
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00217", "GUI_ERROR_MAIN_MENU_BUTTON", cDLLDebugLog_MessageData.m_Stack, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, cDLLDebugLog_MessageData.m_Message);
				}
				else
				{
					SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_CHOREO_00217", "GUI_ERROR_EXIT_GAME_BUTTON", cDLLDebugLog_MessageData.m_Stack, Application.Quit, cDLLDebugLog_MessageData.m_Message);
				}
			}
			m_MessageQueue.Clear();
		}
	}

	public IEnumerator LoadDLCsBundles()
	{
		yield return InitBundles(dlcOnly: true);
	}

	public IEnumerator InitDLCs()
	{
		while (PlatformLayer.UserData == null || PlatformLayer.UserData.CurrentAuthStatus != PlatformUserData.EPlatformAuthStatus.Authorised)
		{
			yield return null;
			if (PlatformLayer.UserData.CurrentAuthStatus == PlatformUserData.EPlatformAuthStatus.NotAuthorised)
			{
				FailedLoading = true;
				string text = $"Failed authorise user in Platform layer. Platform: {PlatformLayer.Instance.PlatformID}";
				Debug.LogError(text);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", "", Application.Quit, text);
				yield break;
			}
		}
		DLCRegistry.FillDLCSpecificMatrices();
		bool flag = SaveData.Instance.Global.IsJotlDlcSaveExists();
		foreach (KeyValuePair<DLCRegistry.EDLCKey, List<ECharacter>> dLCCharactersWithConfigUI in DLCRegistry.DLCCharactersWithConfigUIs)
		{
			if (!flag && !PlatformLayer.DLC.CanPlayDLC(dLCCharactersWithConfigUI.Key))
			{
				continue;
			}
			for (int i = 0; i < dLCCharactersWithConfigUI.Value.Count; i++)
			{
				string fileName = $"{dLCCharactersWithConfigUI.Value[i]}_ConfigUI";
				CharacterConfigUI dLCGUIAssetFromBundle = PlatformLayer.DLC.GetDLCGUIAssetFromBundle<CharacterConfigUI>(dLCCharactersWithConfigUI.Key, fileName, "Config GUI/Characters", "asset", suppressError: false, flag, alwaysLoaded: true);
				if (dLCGUIAssetFromBundle != null && !UIInfoTools.Instance.characterConfigsUI.Contains(dLCGUIAssetFromBundle))
				{
					UIInfoTools.Instance.characterConfigsUI.Add(dLCGUIAssetFromBundle);
					continue;
				}
				Debug.LogWarningFormat("DLC {0} doesn't contain the {1} character config", dLCCharactersWithConfigUI.Key, dLCCharactersWithConfigUI.Value[i]);
			}
		}
		foreach (KeyValuePair<DLCRegistry.EDLCKey, List<string>> dLCSpecificSpriteAsset in DLCRegistry.DLCSpecificSpriteAssets)
		{
			if (!PlatformLayer.DLC.CanPlayDLC(dLCSpecificSpriteAsset.Key))
			{
				continue;
			}
			for (int j = 0; j < dLCSpecificSpriteAsset.Value.Count; j++)
			{
				TMP_SpriteAsset dLCGUIAssetFromBundle2 = PlatformLayer.DLC.GetDLCGUIAssetFromBundle<TMP_SpriteAsset>(dLCSpecificSpriteAsset.Key, dLCSpecificSpriteAsset.Value[j], "Content/GUI/AbilityCardIcons/Atlas", "asset", suppressError: true);
				if (dLCGUIAssetFromBundle2 != null)
				{
					UIInfoTools.Instance.AddFallbackAbilityCardSpriteAsset(dLCGUIAssetFromBundle2);
				}
			}
		}
		_cachedDLCs = DLCRegistry.EDLCKey.None;
		foreach (DLCRegistry.EDLCKey value in Enum.GetValues(typeof(DLCRegistry.EDLCKey)))
		{
			if (PlatformLayer.DLC.UserInstalledDLC(value))
			{
				_cachedDLCs |= value;
			}
		}
	}

	private IEnumerator InitBundles(bool dlcOnly)
	{
		float initialAssetBundleLoadPortion = 15f;
		try
		{
			if (!AssetBundleManager.Instance.InitialLoadComplete)
			{
				AssetBundleManager.Instance.LoadAllInitiallyRequiredBundles(dlcOnly);
			}
		}
		catch (Exception ex)
		{
			FailedLoading = true;
			Debug.LogError("An exception occurred while loading Persistent Data\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
			yield break;
		}
		while (!AssetBundleManager.Instance.InitialLoadComplete)
		{
			SceneController.Instance.LoadingScreenInstance.UpdateProgressBar(initialAssetBundleLoadPortion * AssetBundleManager.Instance.InitialLoadProgress);
			yield return null;
		}
		SceneController.Instance.LoadingScreenInstance.UpdateProgressBar(initialAssetBundleLoadPortion);
	}

	private IEnumerator InitCards()
	{
		float num = 15f;
		try
		{
			if (ObjectPool.instance == null)
			{
				UnityEngine.Object.Instantiate(ObjectPoolPrefab, base.transform);
			}
			else
			{
				ObjectPool.DestroyCardPools();
			}
		}
		catch (Exception ex)
		{
			FailedLoading = true;
			Debug.LogError("An exception occurred while loading Persistent Data\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", ex.StackTrace, Application.Quit, ex.Message);
			yield break;
		}
		SceneController.Instance.LoadingScreenInstance.UpdateProgressBar(num + 15f);
		yield return null;
		bool flag;
		GameObject newAbilityCard;
		AbilityCardUI card;
		try
		{
			flag = CreateAbilityCard1(CharacterClassManager.LongRestLayout, out newAbilityCard, out card);
		}
		catch (Exception ex2)
		{
			FailedLoading = true;
			Debug.LogError("An exception occurred while loading Persistent Data\n" + ex2.Message + "\n" + ex2.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", ex2.StackTrace, Application.Quit, ex2.Message);
			yield break;
		}
		if (flag)
		{
			try
			{
				CreateAbilityCard2(CharacterClassManager.LongRestLayout, newAbilityCard, card);
			}
			catch (Exception ex3)
			{
				FailedLoading = true;
				Debug.LogError("An exception occurred while loading Persistent Data\n" + ex3.Message + "\n" + ex3.StackTrace);
				SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", ex3.StackTrace, Application.Quit, ex3.Message);
				yield break;
			}
			SceneController.Instance.LoadingScreenInstance.UpdateProgressBar(95f);
			IsDataLoaded = true;
		}
		else
		{
			FailedLoading = true;
			Debug.LogError("Failed to create Long Rest card");
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_SCENE_00023", "GUI_ERROR_EXIT_GAME_BUTTON", Environment.StackTrace, Application.Quit);
		}
	}

	public IEnumerator InitGlobalData()
	{
		_diagnosticsStopwatchFacade.Start();
		yield return InitBundles(dlcOnly: false);
		_diagnosticsStopwatchFacade.StopAndLog("Bundle Init");
		_diagnosticsStopwatchFacade.Start();
		yield return InitDLCs();
		_diagnosticsStopwatchFacade.StopAndLog("Init DLCs");
		_diagnosticsStopwatchFacade.Start();
		yield return InitCards();
		_diagnosticsStopwatchFacade.StopAndLog("Init Cards");
	}

	public IEnumerator InitMonsterCards()
	{
		FailedLoading = false;
		ObjectPool.ClearAllMonsterCards();
		yield break;
	}

	public void ClearCardPools()
	{
		if (ObjectPool.instance != null)
		{
			ObjectPool.DestroyCardPools();
			if (CreateAbilityCard1(CharacterClassManager.LongRestLayout, out var newAbilityCard, out var card))
			{
				CreateAbilityCard2(CharacterClassManager.LongRestLayout, newAbilityCard, card);
			}
		}
	}

	public static bool CreateAbilityCard1(AbilityCardYMLData abilityCard, out GameObject newAbilityCard, out AbilityCardUI card)
	{
		newAbilityCard = null;
		card = null;
		try
		{
			if (abilityCard == null)
			{
				Debug.LogError("Ability card data is null!");
				return false;
			}
			CCharacterClass cCharacterClass = CharacterClassManager.Classes.SingleOrDefault((CCharacterClass x) => x.ID == abilityCard.CharacterID);
			string text = (InputManager.GamePadInUse ? "AbilityCard_gamepad" : "AbilityCard");
			string text2 = (InputManager.GamePadInUse ? "LongRest card_gamepad" : "LongRest card");
			newAbilityCard = UnityEngine.Object.Instantiate(AssetBundleManager.Instance.LoadAssetFromBundle<GameObject>("misc_gui", (abilityCard.ID == -1) ? text2 : text, "gui"));
			if (cCharacterClass != null)
			{
				newAbilityCard.name = cCharacterClass.DefaultModel + " " + ConvertCardIDToPaddedString(abilityCard.ID) + " - " + LocalizationManager.GetTranslation(abilityCard.Name) + "(Clone)";
			}
			if (abilityCard.ID == -1)
			{
				newAbilityCard.name = "All - Long Rest(Clone)";
			}
			card = newAbilityCard.GetComponent<AbilityCardUI>();
			card.transform.localScale = Vector3.one;
			card.MakeAbilityCard(abilityCard, alwaysShow: false, abilityCard.ID == -1);
			if (cCharacterClass != null)
			{
				card.EditorInit(cCharacterClass);
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("An error occurred creating the ability card " + abilityCard?.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
			return false;
		}
	}

	public static void CreateAbilityCard2(AbilityCardYMLData abilityCard, GameObject newAbilityCard, AbilityCardUI card, bool usePool = true)
	{
		try
		{
			card.MakeAbilityCardContinued();
			if (usePool)
			{
				ObjectPool.CreateCardPool(abilityCard.ID, ObjectPool.ECardType.Ability, newAbilityCard, 1);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An error occurred creating the ability card " + abilityCard?.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public static bool CreateItemCard1(ItemCardYMLData itemCard, out GameObject newItemCard, out ItemCardUI card)
	{
		newItemCard = null;
		card = null;
		try
		{
			if (itemCard == null)
			{
				Debug.LogError("Ability card data is null!");
				return false;
			}
			string fileName = (InputManager.GamePadInUse ? "ItemCard_gamepad" : "ItemCard");
			newItemCard = UnityEngine.Object.Instantiate(AssetBundleManager.Instance.LoadAssetFromBundle<GameObject>("misc_gui", fileName, "gui"));
			card = newItemCard.GetComponent<ItemCardUI>();
			card.CardID = itemCard.ID;
			card.CardName = itemCard.Name;
			card.item = itemCard.GetItem;
			RectTransform rectTransform = card.layoutHolder.transform as RectTransform;
			card.layout = new CreateLayout(itemCard.CardLayout.ParentGroup, rectTransform.rect, itemCard.ID, isLongRest: false, null, inConsume: false, rectTransform, isItemCard: true);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("An error occurred creating the item card " + itemCard?.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
			return false;
		}
	}

	public static void CreateItemCard2(ItemCardYMLData itemCard, GameObject newItemCard, ItemCardUI card, bool usePool = true)
	{
		try
		{
			card.layout.GenerateFullLayout();
			card.CreateCard();
			if (usePool)
			{
				ObjectPool.CreateCardPool(card.item.ID, ObjectPool.ECardType.Item, newItemCard, 1);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("An error occurred creating the item card " + itemCard?.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public static GameObject CreateMonsterCard(MonsterCardYMLData monsterCard, bool usePool = true)
	{
		try
		{
			string fileName = (InputManager.GamePadInUse ? "MonsterRoundCard_gamepad" : "MonsterRoundCard");
			GameObject gameObject = UnityEngine.Object.Instantiate(AssetBundleManager.Instance.LoadAssetFromBundle<GameObject>("misc_gui", fileName, "gui"));
			MonsterRoundCardUI component = gameObject.GetComponent<MonsterRoundCardUI>();
			component.transform.localScale = Vector3.one;
			component.CardID = monsterCard.ID;
			component.MakeCard(monsterCard);
			if (usePool)
			{
				ObjectPool.CreateCardPool(component.CardID, ObjectPool.ECardType.Monster, gameObject, 2);
			}
			return gameObject;
		}
		catch (Exception ex)
		{
			Debug.LogError("An error occurred creating the monster card " + monsterCard?.FileName + "\n" + ex.Message + "\n" + ex.StackTrace);
		}
		return null;
	}

	public void DLLDebugHandler(DLLDebug.CDLLDebugLog_MessageData message)
	{
		lock (m_MessageQueue)
		{
			m_MessageQueue.Add(message);
		}
	}

	public static void AutoGenObjectPools()
	{
		List<GameObject> list = new List<GameObject>();
		List<GameObject> summons = new List<GameObject>();
		List<GameObject> enemies = new List<GameObject>();
		foreach (CCharacterClass @class in CharacterClassManager.Classes)
		{
			GameObject characterPrefabFromBundle = AssetBundleManager.Instance.GetCharacterPrefabFromBundle(CActor.EType.Player, @class.DefaultModel);
			if ((bool)characterPrefabFromBundle)
			{
				list.Add(characterPrefabFromBundle);
			}
			List<string> characterAppearanceSkin = UIInfoTools.Instance.GetCharacterAppearanceSkin(@class.DefaultModel, @class.CharacterYML.CustomCharacterConfig);
			if (characterAppearanceSkin == null)
			{
				continue;
			}
			foreach (string item in characterAppearanceSkin)
			{
				if (!string.IsNullOrEmpty(item))
				{
					GameObject characterPrefabFromBundle2 = AssetBundleManager.Instance.GetCharacterPrefabFromBundle(CActor.EType.Player, @class.DefaultModel, item);
					if ((bool)characterPrefabFromBundle2)
					{
						list.Add(characterPrefabFromBundle2);
					}
				}
			}
		}
		CoroutineHelper.RunCoroutine(AutoGenerateObjectPools.GenerateObjectPools(list, summons, enemies));
	}

	private static string ConvertCardIDToPaddedString(int cardID)
	{
		string text = cardID.ToString();
		while (text.Length < 3)
		{
			text = "0" + text;
		}
		return text;
	}
}
