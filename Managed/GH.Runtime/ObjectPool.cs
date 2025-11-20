#define ENABLE_LOGS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using JetBrains.Annotations;
using SM.Utils;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using UnityEngine;
using UnityEngine.UI;

public class ObjectPool : MonoBehaviour
{
	public enum StartupPoolMode
	{
		Awake,
		Start,
		CallManually
	}

	public enum ECardType
	{
		None,
		Ability,
		Item,
		Monster
	}

	[Serializable]
	public class Pool
	{
		public GameObject Prefab;

		public int InitialPoolSize;

		public StartupPoolMode StartupMode;
	}

	public class CardPool
	{
		public int CardID { get; private set; }

		public ECardType CardType { get; private set; }

		public List<GameObject> Instances { get; private set; }

		public CardPool(int cardID, ECardType cardType, List<GameObject> instances)
		{
			CardID = cardID;
			CardType = cardType;
			Instances = instances;
		}
	}

	private class RecycleArgContainer
	{
		public GameObject ObjInstance;

		public GameObject Prefab;

		public bool Destroy;
	}

	public static ObjectPool instance;

	public Pool[] StartupPools;

	private Dictionary<GameObject, List<GameObject>> pooledObjects = new Dictionary<GameObject, List<GameObject>>();

	private Dictionary<GameObject, GameObject> spawnedObjects = new Dictionary<GameObject, GameObject>();

	private Dictionary<int, KeyValuePair<GameObject, Vector3>> pooledByID = new Dictionary<int, KeyValuePair<GameObject, Vector3>>();

	private List<CardPool> cardPools = new List<CardPool>();

	private Dictionary<GameObject, GameObject> waitingToBeRecycled = new Dictionary<GameObject, GameObject>();

	private List<GameObject> enhancedCards = new List<GameObject>();

	private HashSet<AbilityCardUI> _abilityCards = new HashSet<AbilityCardUI>();

	private static bool _clearingSpawnedObjects = false;

	private static Queue<RecycleArgContainer> _recycleQueue = new Queue<RecycleArgContainer>();

	public static List<GameObject> StartupPoolGameObjects => instance.StartupPools.Select((Pool o) => o.Prefab).ToList();

	public static bool ItemsWaitingToBeRecyled => instance.waitingToBeRecycled.Count > 0;

	[UsedImplicitly]
	private void Awake()
	{
		instance = this;
		Pool[] startupPools = StartupPools;
		foreach (Pool pool in startupPools)
		{
			if (pool.StartupMode == StartupPoolMode.Awake)
			{
				CreatePool(pool.Prefab, pool.InitialPoolSize);
			}
		}
	}

	[UsedImplicitly]
	private void Start()
	{
		Pool[] startupPools = StartupPools;
		foreach (Pool pool in startupPools)
		{
			if (pool.StartupMode == StartupPoolMode.Start)
			{
				CreatePool(pool.Prefab, pool.InitialPoolSize);
			}
		}
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		instance = null;
		pooledObjects.Clear();
		spawnedObjects.Clear();
		pooledByID.Clear();
		cardPools.Clear();
		waitingToBeRecycled.Clear();
		enhancedCards.Clear();
	}

	public static void RemoveAbilityCard(int cardID)
	{
		instance.cardPools.RemoveAll((CardPool r) => r.CardType == ECardType.Ability && r.CardID == cardID);
	}

	public static void RemoveItemCard(int cardID)
	{
		instance.cardPools.RemoveAll((CardPool r) => r.CardType == ECardType.Item && r.CardID == cardID);
	}

	public static void CreatePool(GameObject prefab, int initialPoolSize)
	{
		if (!(prefab != null) || instance.pooledObjects.ContainsKey(prefab))
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		instance.pooledObjects.Add(prefab, list);
		if (initialPoolSize > 0)
		{
			while (list.Count < initialPoolSize)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(prefab, instance.transform);
				gameObject.name = gameObject.name + "_" + list.Count;
				gameObject.SetActive(value: false);
				list.Add(gameObject);
			}
		}
	}

	public static void CreateOrAddToPool(GameObject prefab, int addAmount)
	{
		if (prefab == null)
		{
			return;
		}
		if (instance.pooledObjects.ContainsKey(prefab))
		{
			while (addAmount-- > 0)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(prefab, instance.transform);
				gameObject.SetActive(value: false);
				int num = instance.pooledObjects[prefab].Count + instance.spawnedObjects.Values.Count((GameObject x) => x == prefab);
				gameObject.name = gameObject.name + "_" + num;
				instance.pooledObjects[prefab].Add(gameObject);
			}
		}
		else
		{
			CreatePool(prefab, addAmount);
		}
	}

	public static int AddToPoolByID(GameObject instance)
	{
		int count = ObjectPool.instance.pooledByID.Count;
		ObjectPool.instance.pooledByID.Add(count, new KeyValuePair<GameObject, Vector3>(instance, instance.transform.position));
		instance.transform.SetParent(ObjectPool.instance.transform);
		instance.SetActive(value: false);
		return count;
	}

	public static GameObject GetFromPoolByID(int id, Transform parent)
	{
		if (id < 0)
		{
			return null;
		}
		if (instance.pooledByID.TryGetValue(id, out var value))
		{
			GameObject obj = UnityEngine.Object.Instantiate(value.Key);
			obj.transform.SetParent(parent);
			obj.transform.localPosition = value.Value;
			obj.SetActive(value: true);
			return obj;
		}
		Debug.LogError("[OBJECTPOOL] Error: Unable to find object with id " + id + " in Object Pool");
		return null;
	}

	public static void CreateCardPool(int cardID, ECardType cardType, GameObject cardInstance, int amountToCache)
	{
		List<GameObject> list = new List<GameObject>();
		cardInstance.transform.SetParent(instance.transform);
		list.Add(cardInstance);
		for (int i = 1; i < amountToCache; i++)
		{
			GameObject item = UnityEngine.Object.Instantiate(cardInstance, instance.transform);
			list.Add(item);
		}
		foreach (GameObject item2 in list)
		{
			item2.SetActive(value: false);
		}
		instance.cardPools.Add(new CardPool(cardID, cardType, list));
	}

	public static void DestroyCardPools()
	{
		foreach (CardPool cardPool in instance.cardPools)
		{
			foreach (GameObject instance in cardPool.Instances)
			{
				UnityEngine.Object.Destroy(instance);
			}
			cardPool.Instances.Clear();
		}
		ObjectPool.instance.cardPools.Clear();
	}

	public static void AddEnhancedCard(GameObject enhancedCard)
	{
		if (!instance.enhancedCards.Contains(enhancedCard))
		{
			instance.enhancedCards.Add(enhancedCard);
		}
	}

	private static void ClearEnhancement(GameObject card)
	{
		if (!(card != null))
		{
			return;
		}
		try
		{
			bool activeSelf = card.activeSelf;
			card.SetActive(value: true);
			AbilityCardUI component = card.GetComponent<AbilityCardUI>();
			if (component != null)
			{
				foreach (EnhancementButton button in component.EnhancementElements.Buttons)
				{
					button.Enhancement.Enhancement = EEnhancement.NoEnhancement;
					button.Enhancement.PaidPrice = 0;
					button.UpdateEnhancement(EEnhancement.NoEnhancement);
				}
				foreach (EnhancedAreaHex areaHex in component.EnhancementElements.AreaHexes)
				{
					areaHex.RemoveEnhancement();
				}
			}
			card.SetActive(activeSelf);
		}
		catch (Exception ex)
		{
			Debug.LogError("[OBJECTPOOL] An exception occurred while removing all enhancements.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public static void ClearEnhancements(List<string> cardNames)
	{
		try
		{
			foreach (GameObject item in instance.enhancedCards.ToList())
			{
				if (item == null)
				{
					instance.enhancedCards.Remove(item);
					continue;
				}
				AbilityCardUI component = item.GetComponent<AbilityCardUI>();
				if (component != null && cardNames.Contains(component.CardName))
				{
					ClearEnhancement(item);
					instance.enhancedCards.Remove(item);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[OBJECTPOOL] Exception while running ClearEnhancements.\ncardNames: " + string.Join(", ", cardNames.ToArray()) + "\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public static void ClearEnhancements(string cardName)
	{
		ClearEnhancements(new List<string> { cardName });
	}

	public static void ClearEnhancements()
	{
		if (instance == null)
		{
			return;
		}
		foreach (GameObject enhancedCard in instance.enhancedCards)
		{
			ClearEnhancement(enhancedCard);
		}
		instance.enhancedCards.Clear();
	}

	public static List<GameObject> GetAllCachedAbilityCards(int cardID)
	{
		List<GameObject> list = new List<GameObject>();
		CardPool cardPool = instance.cardPools.SingleOrDefault((CardPool s) => s.CardID == cardID && s.CardType == ECardType.Ability);
		if (cardPool != null)
		{
			list.AddRange(cardPool.Instances);
			foreach (AbilityCardUI abilityCard in instance._abilityCards)
			{
				if (abilityCard.CardID == cardID)
				{
					list.Add(abilityCard.gameObject);
				}
			}
		}
		return list;
	}

	public static void ClearAllMonsterCards()
	{
		foreach (CardPool item in instance.cardPools.FindAll((CardPool r) => r.CardType == ECardType.Monster))
		{
			foreach (GameObject instance in item.Instances)
			{
				UnityEngine.Object.Destroy(instance);
			}
		}
		ObjectPool.instance.cardPools.RemoveAll((CardPool r) => r.CardType == ECardType.Monster);
	}

	public static void ClearAllAbilityCards()
	{
		foreach (CardPool item in instance.cardPools.FindAll((CardPool r) => r.CardType == ECardType.Ability))
		{
			foreach (GameObject instance in item.Instances)
			{
				UnityEngine.Object.Destroy(instance);
			}
		}
		ObjectPool.instance.cardPools.RemoveAll((CardPool r) => r.CardType == ECardType.Ability);
	}

	public static void ClearAllItemCards()
	{
		foreach (CardPool item in instance.cardPools.FindAll((CardPool r) => r.CardType == ECardType.Item))
		{
			foreach (GameObject instance in item.Instances)
			{
				UnityEngine.Object.Destroy(instance);
			}
		}
		ObjectPool.instance.cardPools.RemoveAll((CardPool r) => r.CardType == ECardType.Item);
	}

	private static void CreateAbilityCardInternal(int cardID, int count = 1)
	{
		CAbilityCard cAbilityCard = CharacterClassManager.AllAbilityCards.Find((CAbilityCard x) => x.ID == cardID);
		PersistentData.CreateAbilityCard1(cAbilityCard.GetAbilityCardYML, out var newAbilityCard, out var card);
		card.MakeAbilityCardContinued();
		CreateCardPool(cAbilityCard.ID, ECardType.Ability, newAbilityCard, count);
	}

	private static void CreateMonsterCardInternal(int cardID)
	{
		PersistentData.CreateMonsterCard(ScenarioRuleClient.SRLYML.MonsterCards.Find((MonsterCardYMLData x) => x.ID == cardID));
	}

	private static void CreateItemCardInternal(int cardID)
	{
		PersistentData.CreateItemCard1(ScenarioRuleClient.SRLYML.ItemCards.Find((ItemCardYMLData x) => x.ID == cardID), out var newItemCard, out var card);
		card.layout.GenerateFullLayout();
		card.CreateCard();
		CreateCardPool(card.item.ID, ECardType.Item, newItemCard, 1);
	}

	public static void CreatePooledAbilityCard(int cardID, int count = 1)
	{
		if (!instance.cardPools.Exists((CardPool e) => e.CardID == cardID && e.CardType == ECardType.Ability))
		{
			CreateAbilityCardInternal(cardID, count);
		}
	}

	public static void CreatePooledItemCard(int cardID)
	{
		if (!instance.cardPools.Exists((CardPool e) => e.CardID == cardID && e.CardType == ECardType.Item))
		{
			CreateItemCardInternal(cardID);
		}
	}

	public static GameObject SpawnCard(int cardID, ECardType cardType, Transform parent, bool resetLocalScale = false, bool resetToMiddle = false, bool resetLocalRotation = false, bool activate = true)
	{
		try
		{
			if (!instance.cardPools.Exists((CardPool e) => e.CardID == cardID && e.CardType == cardType))
			{
				switch (cardType)
				{
				case ECardType.None:
					Debug.LogError("[OBJECTPOOL] Error: Unable to spawn card with ID " + cardID);
					return null;
				case ECardType.Ability:
					CreateAbilityCardInternal(cardID);
					break;
				case ECardType.Item:
					CreateItemCardInternal(cardID);
					break;
				case ECardType.Monster:
					CreateMonsterCardInternal(cardID);
					break;
				default:
					throw new ArgumentOutOfRangeException("cardType", cardType, null);
				}
			}
			CardPool cardPool;
			try
			{
				cardPool = instance.cardPools.Single((CardPool s) => s.CardID == cardID && s.CardType == cardType);
			}
			catch
			{
				Debug.LogError("[OBJECTPOOL] Duplicate pools with the same ID and card type exist!  CardID: " + cardID + "  Card Type: " + cardType.ToString() + ".  Unable to spawn card.");
				return null;
			}
			if (cardPool.Instances.Count == 0)
			{
				Debug.LogError("[OBJECTPOOL] Card pool " + cardID + " is empty.  Unable to spawn new card.");
				return null;
			}
			GameObject cardInstance = GetCardInstance(cardPool);
			AbilityCardUI component = cardInstance.GetComponent<AbilityCardUI>();
			if (component != null)
			{
				foreach (EnhancementButton button in component.EnhancementElements.Buttons)
				{
					if (button.Enhancement.Enhancement != EEnhancement.NoEnhancement)
					{
						AddEnhancedCard(cardInstance);
						break;
					}
				}
			}
			LayoutRebuilder.Enable = false;
			cardInstance.transform.SetParent(parent);
			if (resetLocalScale)
			{
				cardInstance.transform.localScale = Vector3.one;
			}
			if (resetToMiddle)
			{
				RectTransform obj2 = cardInstance.transform as RectTransform;
				Vector2 vector = (obj2.pivot = new Vector2(0.5f, 0.5f));
				Vector2 anchorMin = (obj2.anchorMax = vector);
				obj2.anchorMin = anchorMin;
				obj2.anchoredPosition = Vector2.zero;
			}
			if (resetLocalRotation)
			{
				cardInstance.transform.localRotation = Quaternion.identity;
			}
			Vector3 localPosition = cardInstance.transform.localPosition;
			localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
			cardInstance.transform.localPosition = localPosition;
			LayoutRebuilder.Enable = true;
			if (activate)
			{
				cardInstance.SetActive(value: true);
			}
			((IPooleable)cardInstance.GetComponent(typeof(IPooleable)))?.OnRemovedFromPool();
			return cardInstance;
		}
		catch (Exception ex)
		{
			Debug.LogError("[OBJECTPOOL] An exception occurred generating a card.  \n" + ex.Message + "\n" + ex.StackTrace);
			return null;
		}
	}

	private static GameObject GetCardInstance(CardPool pool)
	{
		if (pool.Instances.Count > 1)
		{
			for (int num = pool.Instances.Count - 1; num > 0; num--)
			{
				GameObject gameObject = pool.Instances[num];
				pool.Instances.RemoveAt(num);
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		return UnityEngine.Object.Instantiate(pool.Instances[0]);
	}

	public static void RecycleCard(int cardID, ECardType cardType, GameObject cardInstance)
	{
		if (!(instance != null))
		{
			return;
		}
		if (!instance.cardPools.Exists((CardPool e) => e.CardID == cardID && e.CardType == cardType))
		{
			Debug.LogError("[OBJECTPOOL] Error: Unable to Recycle card with ID " + cardID);
			return;
		}
		CardPool cardPool;
		try
		{
			cardPool = instance.cardPools.Single((CardPool s) => s.CardID == cardID && s.CardType == cardType);
		}
		catch
		{
			Debug.LogError("[OBJECTPOOL] Duplicate pools with the same ID and card type exist!  CardID: " + cardID + "  Card Type: " + cardType.ToString() + ".  Unable to spawn card.");
			return;
		}
		LayoutRebuilder.Enable = false;
		AbilityCardUI component = cardInstance.GetComponent<AbilityCardUI>();
		if (component != null)
		{
			if (component.fullAbilityCard != null && component.fullAbilityCard.transform.parent != component.transform)
			{
				component.fullAbilityCard.transform.SetParent(component.transform);
				component.CancelLostAnimation();
			}
			component.SetParent(instance.transform);
		}
		RectTransform obj2 = cardInstance.transform as RectTransform;
		cardInstance.transform.localRotation = Quaternion.identity;
		obj2.anchoredPosition = Vector2.zero;
		Vector2 vector = (obj2.pivot = new Vector2(0.5f, 0.5f));
		Vector2 anchorMin = (obj2.anchorMax = vector);
		obj2.anchorMin = anchorMin;
		cardInstance.SetActive(value: false);
		cardPool.Instances.Add(cardInstance);
		((IPooleable)cardInstance.GetComponent(typeof(IPooleable)))?.OnReturnedToPool();
		LayoutRebuilder.Enable = true;
	}

	public static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation, bool forceInstantiate = false)
	{
		try
		{
			if (prefab == null)
			{
				Debug.LogError("[OBJECTPOOL] Attempting to spawn null object from Object Pool");
				return null;
			}
			GameObject gameObject = null;
			if (!forceInstantiate && instance.pooledObjects.TryGetValue(prefab, out var value))
			{
				if (value.Count > 0)
				{
					while (gameObject == null && value.Count > 0)
					{
						gameObject = value[0];
						value.RemoveAt(0);
					}
					if (gameObject != null && instance.spawnedObjects.ContainsKey(gameObject))
					{
						while (gameObject == null && value.Count > 0)
						{
							gameObject = value[0];
							value.RemoveAt(0);
						}
					}
					if (gameObject != null)
					{
						gameObject.transform.SetParent(parent, worldPositionStays: false);
						gameObject.transform.localPosition = position;
						gameObject.transform.rotation = rotation;
						IPooleable obj = (IPooleable)gameObject.GetComponent(typeof(IPooleable));
						gameObject.SetActive(value: true);
						obj?.OnRemovedFromPool();
						instance.spawnedObjects.Add(gameObject, prefab);
						return gameObject;
					}
				}
				gameObject = UnityEngine.Object.Instantiate(prefab, parent);
				gameObject.transform.localPosition = position;
				gameObject.transform.rotation = rotation;
				int num = instance.pooledObjects[prefab].Count + instance.spawnedObjects.Values.Count((GameObject x) => x == prefab);
				GameObject obj2 = gameObject;
				obj2.name = obj2.name + "_" + num;
				instance.spawnedObjects.Add(gameObject, prefab);
				return gameObject;
			}
			gameObject = UnityEngine.Object.Instantiate(prefab, parent);
			gameObject.transform.localPosition = position;
			gameObject.transform.rotation = rotation;
			int num2 = (instance.pooledObjects.ContainsKey(prefab) ? instance.pooledObjects[prefab].Count : instance.spawnedObjects.Values.Count((GameObject x) => x == prefab));
			GameObject obj3 = gameObject;
			obj3.name = obj3.name + "_" + num2;
			instance.spawnedObjects.Add(gameObject, prefab);
			if (!instance.pooledObjects.ContainsKey(prefab))
			{
				instance.pooledObjects.Add(prefab, new List<GameObject>());
			}
			return gameObject;
		}
		catch (Exception ex)
		{
			Debug.LogError("[OBJECTPOOL] An exception occurred spawning an object from the object pool.\n" + ex.Message + "\n" + ex.StackTrace);
			SceneController.Instance.GlobalErrorMessage.ShowMessageDefaultTitle("ERROR_OBJECTPOOL_00001", "GUI_ERROR_MAIN_MENU_BUTTON", ex.StackTrace, UnityGameEditorRuntime.ErrorHandlingUnloadSceneAndLoadMainMenu, ex.Message);
			return null;
		}
	}

	public static GameObject Spawn(GameObject prefab, Transform parent, bool forceInstantiate = false)
	{
		return Spawn(prefab, parent, Vector3.zero, Quaternion.identity, forceInstantiate);
	}

	public static T Spawn<T>(T prefab, Transform parent, bool forceInstantiate = false) where T : MonoBehaviour
	{
		return Spawn(prefab.gameObject, parent, Vector3.zero, Quaternion.identity, forceInstantiate).GetComponent<T>();
	}

	public static GameObject GetSpawned(GameObject prefab)
	{
		if (instance.spawnedObjects.TryGetValue(prefab, out var value))
		{
			return value;
		}
		return null;
	}

	public static bool SpawnedContains(GameObject prefab)
	{
		return instance.spawnedObjects.ContainsValue(prefab);
	}

	public static IEnumerable<GameObject> GetMatches(GameObject prefab)
	{
		return from pair in instance.spawnedObjects
			where pair.Value == prefab
			select pair.Key;
	}

	public static void Recycle(GameObject objInstance, GameObject prefab = null, bool destroy = false)
	{
		try
		{
			if (_clearingSpawnedObjects)
			{
				Debug.LogWarning("[OBJECTPOOL] Warning: Spawned objects clearing in progress => abort Recycle");
				RecycleArgContainer item = new RecycleArgContainer
				{
					ObjInstance = objInstance,
					Prefab = prefab,
					Destroy = destroy
				};
				_recycleQueue.Enqueue(item);
				return;
			}
			if (prefab == null && !instance.spawnedObjects.TryGetValue(objInstance, out prefab))
			{
				Debug.LogWarning("[OBJECTPOOL] Warning: GameObject " + objInstance.name + " was not found in spawned object pool.  It will be destroyed.");
				UnityEngine.Object.Destroy(objInstance);
				return;
			}
			instance.spawnedObjects.Remove(objInstance);
			if (destroy)
			{
				UnityEngine.Object.Destroy(objInstance);
				return;
			}
			if (instance.pooledObjects.TryGetValue(prefab, out var value))
			{
				if (!value.Contains(objInstance))
				{
					value.Add(objInstance);
				}
				else
				{
					Debug.Log("[OBJECTPOOL] Attempting to recycle an object instance that is already in the pool.  Obj Name = " + objInstance.name);
				}
				objInstance.transform.SetParent(instance.transform, worldPositionStays: false);
				objInstance.SetActive(value: false);
			}
			else
			{
				instance.pooledObjects.Add(prefab, new List<GameObject> { objInstance });
			}
			((IPooleable)prefab.GetComponent(typeof(IPooleable)))?.OnReturnedToPool();
		}
		catch (Exception ex)
		{
			Debug.LogError("[OBJECTPOOL] An exception occurred recycling an object to the object pool.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public static void Recycle(GameObject objInstance, float waitForRecycle, GameObject prefab = null, bool destroy = false)
	{
		instance.StartCoroutine(RecycleCoroutine(objInstance, waitForRecycle, prefab, destroy));
	}

	public static void RecyclePendingObjects()
	{
		if (!(instance != null) || instance.waitingToBeRecycled == null)
		{
			return;
		}
		foreach (KeyValuePair<GameObject, GameObject> item in instance.waitingToBeRecycled)
		{
			if (item.Key == null)
			{
				if (item.Value != null)
				{
					LogUtils.LogWarning("Failed to recycle " + item.Value.name);
				}
			}
			else
			{
				Recycle(item.Key, item.Value);
			}
		}
		instance.waitingToBeRecycled.Clear();
	}

	public static IEnumerator RecycleCoroutine(GameObject objInstance, float waitForRecycle, GameObject prefab = null, bool destroy = false)
	{
		if (Mathf.Approximately(waitForRecycle, float.MaxValue) || instance.waitingToBeRecycled.ContainsKey(objInstance))
		{
			yield break;
		}
		instance.waitingToBeRecycled.Add(objInstance, prefab);
		yield return Timekeeper.instance.WaitForSeconds(waitForRecycle);
		if (prefab == null || objInstance == null)
		{
			if (prefab != null)
			{
				LogUtils.LogWarning("Failed to recycle " + prefab.name);
			}
			instance.waitingToBeRecycled.Remove(objInstance);
		}
		else if (instance.waitingToBeRecycled.ContainsKey(objInstance))
		{
			if (objInstance != null)
			{
				Recycle(objInstance, prefab, destroy);
				instance.waitingToBeRecycled.Remove(objInstance);
			}
			else
			{
				Debug.LogWarning("[OBJECTPOOL] Object Pool attempted to recycle an object instance that has already been destroyed");
			}
		}
	}

	public static void RecycleAll(GameObject prefab)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (KeyValuePair<GameObject, GameObject> spawnedObject in instance.spawnedObjects)
		{
			if (spawnedObject.Value == prefab)
			{
				list.Add(spawnedObject.Key);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			Recycle(list[i], prefab);
		}
	}

	public static void DestroyPooled(GameObject prefab)
	{
		if (instance.pooledObjects.TryGetValue(prefab, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				UnityEngine.Object.Destroy(value[i]);
			}
			value.Clear();
		}
		instance.pooledObjects.Remove(prefab);
	}

	public static void DestroyAll(GameObject prefab)
	{
		RecycleAll(prefab);
		DestroyPooled(prefab);
	}

	public static void ClearAllExceptCards()
	{
		_clearingSpawnedObjects = true;
		RecyclePendingObjects();
		try
		{
			instance.StopAllCoroutines();
		}
		catch
		{
		}
		List<GameObject> list = instance.pooledObjects.Keys.Except(StartupPoolGameObjects).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			List<GameObject> list2 = instance.pooledObjects[list[i]];
			for (int j = 0; j < list2.Count; j++)
			{
				UnityEngine.Object.Destroy(list2[j]);
			}
			instance.pooledObjects.Remove(list[i]);
		}
		foreach (GameObject key in instance.spawnedObjects.Keys)
		{
			UnityEngine.Object.Destroy(key);
		}
		instance.spawnedObjects.Clear();
		_clearingSpawnedObjects = false;
		RecycleArgContainer result;
		while (_recycleQueue.TryDequeue(out result))
		{
			Recycle(result.ObjInstance, result.Prefab, result.Destroy);
		}
	}

	public static void RecycleAndDestroyObjects()
	{
		try
		{
			RecyclePendingObjects();
			instance.spawnedObjects.Clear();
			try
			{
				instance.StopAllCoroutines();
			}
			catch
			{
			}
			foreach (GameObject item in instance.pooledObjects.Keys.ToList().Except(StartupPoolGameObjects))
			{
				DestroyPooled(item);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception during RecycleAndDestroyObjects.\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public void RegisterAbilityCardUI(AbilityCardUI abilityCardUI)
	{
		_abilityCards.Add(abilityCardUI);
	}

	public void RemoveAbilityCardUI(AbilityCardUI abilityCardUI)
	{
		_abilityCards.Remove(abilityCardUI);
	}
}
