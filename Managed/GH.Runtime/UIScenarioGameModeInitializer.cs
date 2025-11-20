using UnityEngine;

public class UIScenarioGameModeInitializer : MonoBehaviour
{
	[SerializeField]
	private ScenarioRewardManager campaignRewardManager;

	public void Awake()
	{
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			Object.Instantiate(campaignRewardManager, base.transform);
		}
		else
		{
			base.gameObject.AddComponent<GuildmasterScenarioRewardManager>();
		}
	}
}
