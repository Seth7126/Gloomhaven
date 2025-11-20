using System.Linq;
using FFSNet;
using GLOOM;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterStats : MonoBehaviour
{
	[SerializeField]
	private TMP_Text characterName;

	[SerializeField]
	private TMP_Text characterLevel;

	[SerializeField]
	private ImageProgressBar characterXP;

	[SerializeField]
	private LayoutElementExtended layoutElement;

	private CPlayerActor _actor;

	public void Init(CPlayerActor cPlayer)
	{
		_actor = cPlayer;
		UpdateCharText();
		characterLevel.text = string.Format("{0} {1}", LocalizationManager.GetTranslation("GUI_LEVEL"), cPlayer.Level);
		int num = 0;
		int num2 = 0;
		CMapCharacter cMapCharacter = null;
		if (SaveData.Instance.Global.GameMode == EGameMode.Campaign)
		{
			cMapCharacter = SaveData.Instance.Global.CampaignData?.AdventureMapState?.MapParty.SelectedCharacters.ToList().Find((CMapCharacter x) => x.CharacterID == cPlayer.Class.ID);
		}
		else if (SaveData.Instance.Global.GameMode == EGameMode.Guildmaster)
		{
			cMapCharacter = SaveData.Instance.Global.AdventureData?.AdventureMapState?.MapParty.SelectedCharacters.ToList().Find((CMapCharacter x) => x.CharacterID == cPlayer.Class.ID);
		}
		if (cMapCharacter != null)
		{
			num = cMapCharacter.GetNextXPThreshold();
			if (num == int.MaxValue)
			{
				num = cMapCharacter.XPTable[cMapCharacter.XPTable.Count - 1];
			}
			num2 = cMapCharacter.EXP + cPlayer.XP;
		}
		else
		{
			num = 1000;
		}
		characterXP.SetAmount(num2, num);
	}

	public void UpdateCharText()
	{
		if (_actor == null)
		{
			return;
		}
		if (FFSNetwork.IsOnline)
		{
			NetworkPlayer controller = ControllableRegistry.GetController((SaveData.Instance.Global.GameMode == EGameMode.Campaign) ? _actor.CharacterName.GetHashCode() : _actor.CharacterClass.ModelInstanceID);
			if (controller != null)
			{
				string text = ((controller.Username.Length <= 16) ? controller.Username : controller.Username.Substring(0, 16));
				characterName.text = PlatformTextSpriteProvider.GetPlatformIcon(controller.PlatformName) + "<color=white>" + text + "</color> (" + LocalizationManager.GetTranslation(_actor.ActorLocKey()) + ")";
				return;
			}
		}
		characterName.text = LocalizationManager.GetTranslation(_actor.ActorLocKey());
	}

	public void Show(bool show)
	{
		base.gameObject.SetActive(show);
		if (layoutElement != null)
		{
			layoutElement.overrideMinHeightWithPrefered = show;
		}
	}
}
