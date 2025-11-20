#define ENABLE_LOGS
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterRoundCardUI : MonoBehaviour, IPooleable
{
	[SerializeField]
	private TextMeshProUGUI initiativeText;

	[SerializeField]
	private RectTransform contentHolder;

	[SerializeField]
	private RectTransform baseStatsVersionHolder;

	[SerializeField]
	private Image background;

	public int CardID;

	[SerializeField]
	protected string cardName;

	public string CardName => cardName;

	[UsedImplicitly]
	private void Awake()
	{
		if (background == null)
		{
			background = GetComponent<Image>();
		}
	}

	public void AnimateAppearance(Vector3 startPosition, int fullRotations, float rotationTime, LeanTweenType easeType)
	{
		base.transform.localPosition = startPosition;
		base.transform.localRotation = Quaternion.Euler(0f, -360 * fullRotations + 90, 0f);
		base.gameObject.SetActive(value: true);
		LeanTween.rotateAround(GetComponent<RectTransform>(), Vector3.up, 360 * fullRotations - 90, rotationTime).setEase(easeType);
	}

	public void MakeCard(MonsterCardYMLData cardYML, CEnemyActor enemyForBaseStats = null)
	{
		RectTransform rectTransform = ((enemyForBaseStats == null) ? contentHolder : baseStatsVersionHolder);
		initiativeText.text = cardYML.Initiative.ToString();
		cardName = cardYML.ID.ToString();
		CreateLayout createLayout = new CreateLayout(cardYML, rectTransform.rect, enemyForBaseStats);
		createLayout.FullLayout.transform.SetParent(rectTransform);
		createLayout.FullLayout.transform.localScale = Vector3.one;
		createLayout.FullLayout.transform.localPosition = Vector3.zero;
		RectTransform obj = createLayout.FullLayout.transform as RectTransform;
		obj.anchoredPosition = Vector2.zero;
		obj.sizeDelta = Vector2.zero;
		if (enemyForBaseStats != null)
		{
			GameObject gameObject = Choreographer.s_Choreographer.FindClientActorGameObject(enemyForBaseStats);
			if (gameObject != null)
			{
				CharacterManager characterManager = CharacterManager.GetCharacterManager(gameObject);
				background.sprite = characterManager.CharacterCardBackgroundSprite;
			}
		}
	}

	public GameObject MakeVersionWithBaseStats(CEnemyActor enemyForBaseStats, int ID)
	{
		if (baseStatsVersionHolder.childCount == 0)
		{
			MonsterCardYMLData monsterCardYMLData = ScenarioRuleClient.SRLYML.MonsterCards.Find((MonsterCardYMLData x) => x.ID == ID);
			if (monsterCardYMLData != null)
			{
				MakeCard(monsterCardYMLData, enemyForBaseStats);
			}
			else
			{
				Debug.Log("MonsterCardYML with ID " + ID + " not found, cannot generate layout with base stat lines");
			}
		}
		return baseStatsVersionHolder.gameObject;
	}

	public GameObject GetDescription()
	{
		return contentHolder.gameObject;
	}

	public void OnReturnedToPool()
	{
		Clean(instant: false);
	}

	public void Clean(bool instant)
	{
		if (instant)
		{
			while (baseStatsVersionHolder.childCount > 0)
			{
				Object.DestroyImmediate(baseStatsVersionHolder.transform.GetChild(0).gameObject);
			}
			return;
		}
		for (int num = baseStatsVersionHolder.childCount - 1; num >= 0; num--)
		{
			Object.Destroy(baseStatsVersionHolder.transform.GetChild(num).gameObject);
		}
	}

	public void OnRemovedFromPool()
	{
	}
}
