using System.Collections.Generic;
using AsmodeeNet.Foundation.Localization;
using UnityEngine;

namespace AsmodeeNet.Foundation;

[CreateAssetMenu(fileName = "PushNotificationManagerData", menuName = "Asmodee.net/Create/PushNotificationManagerData", order = 1)]
public class PushNotificationManagerData : ScriptableObject
{
	[Header("Global")]
	public string[] KeyToTransfer = new string[10] { "YOUR_TURN_ACTION", "YT_INVITE_ACTION", "YT_ROBOT_ACTION", "INVITATION_ACTION", "GAME_OVER_ACTION", "YOUR_TURN", "YOUR_TURN_INVITE", "YOUR_TURN_ROBOT", "CONFIRM_INVITATION", "GAME_OVER" };

	public List<LocalizationManager.Language> SupportedLanguages = new List<LocalizationManager.Language>
	{
		LocalizationManager.Language.en_US,
		LocalizationManager.Language.fr_FR,
		LocalizationManager.Language.de_DE,
		LocalizationManager.Language.es_ES,
		LocalizationManager.Language.pl_PL
	};

	[Header("Android")]
	public string PathToTextResourcesAndroid = "./Assets/Plugins/Android/TextResources.java";

	public string ChannelID = "UNITYSDKID";

	public string ChannelName = "UNITYSDK";

	public bool EnableVibration = true;
}
