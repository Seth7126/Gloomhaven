#define ENABLE_LOGS
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Misc;
using FFSNet;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using UnityEngine;

public class MultiplayerImportProgressManager : Singleton<MultiplayerImportProgressManager>
{
	[SerializeField]
	private UIMultiplayerImportProgressWindow importWindow;

	private ICallbackPromise promise = CallbackPromise.Resolved();

	public ICallbackPromise Import(NetworkPlayer player, List<QuestCompletionToken> questCompletionList)
	{
		if (questCompletionList.IsNullOrEmpty())
		{
			return CallbackPromise.Resolved();
		}
		promise = promise.Then(() => ProcessImport(player, questCompletionList));
		return promise;
	}

	private ICallbackPromise ProcessImport(NetworkPlayer player, List<QuestCompletionToken> questCompletionList)
	{
		List<CMapCharacter> checkCharacters = AdventureState.MapState.MapParty.CheckCharacters;
		for (int num = questCompletionList.Count - 1; num >= 0; num--)
		{
			QuestCompletionToken questCompletionToken = questCompletionList[num];
			CMapCharacter cMapCharacter = checkCharacters.SingleOrDefault((CMapCharacter x) => x.CharacterID == questCompletionToken.CharacterID && x.CharacterName == questCompletionToken.CharacterName);
			if (cMapCharacter != null)
			{
				for (int num2 = questCompletionToken.QuestCompletionsList.Count - 1; num2 >= 0; num2--)
				{
					QuestCompletionToken.QuestCompletionData questCompletionData = questCompletionToken.QuestCompletionsList[num2];
					if (cMapCharacter.CompletedSoloQuestData.Find((CCompletedSoloQuestData x) => x.QuestID == questCompletionData.QuestID) != null)
					{
						questCompletionToken.QuestCompletionsList.RemoveAt(num2);
					}
				}
				if (questCompletionToken.QuestCompletionsList.Count <= 0)
				{
					questCompletionList.Remove(questCompletionToken);
				}
			}
			else
			{
				questCompletionList.Remove(questCompletionToken);
			}
		}
		if (questCompletionList.Count > 0)
		{
			CallbackPromise importPromise = new CallbackPromise();
			List<CharacterImportData> list = new List<CharacterImportData>();
			foreach (QuestCompletionToken questCompletionToken2 in questCompletionList)
			{
				list.AddRange(questCompletionToken2.QuestCompletionsList.ConvertAll((QuestCompletionToken.QuestCompletionData it) => new CharacterImportData(questCompletionToken2.CharacterID, questCompletionToken2.CharacterName, it)));
			}
			InputManager.RequestDisableInput(this, KeyAction.UI_CANCEL);
			importWindow.Show(player, list).Done(delegate(List<ICharacterImportData> questCompletionsToImport)
			{
				importWindow.Hide();
				ApplyImport(questCompletionsToImport);
				importPromise.Resolve();
			}, importPromise.Resolve);
			return importPromise.Then(delegate
			{
				InputManager.RequestEnableInput(this, KeyAction.UI_CANCEL);
			});
		}
		return CallbackPromise.Resolved();
	}

	public void Cancel()
	{
		InputManager.RequestEnableInput(this, KeyAction.UI_CANCEL);
		importWindow.Hide();
		promise = CallbackPromise.Resolved();
	}

	private void ApplyImport(List<ICharacterImportData> questCompletionsToImport)
	{
		if (questCompletionsToImport.IsNullOrEmpty())
		{
			return;
		}
		foreach (ICharacterImportData item in questCompletionsToImport)
		{
			item.Import();
		}
	}

	public void ProxySelectedImportSettings(GameAction action)
	{
		string[] iDs = (action.SupplementaryDataToken as IdListToken).IDs;
		Debug.Log("ProxySelectedImportSettings " + string.Join(", ", iDs));
		importWindow.EnableImportData(iDs);
	}

	public void ProxyConfirmImportSettings(GameAction action)
	{
		Debug.Log("ProxyConfirmImportSettings");
		importWindow.ConfirmImport();
	}
}
