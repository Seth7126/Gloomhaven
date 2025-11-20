using StateCodeGenerator;

namespace ScenarioRuleLibrary;

public class CAdjustInitiativeActiveBonus_AdjustInitiative : CBespokeBehaviour
{
	public CAdjustInitiativeActiveBonus_AdjustInitiative(CActor actor, CAbility ability, CActiveBonus activeBonus)
		: base(actor, ability, activeBonus)
	{
	}

	public void OnToggleInitiativeBonus(bool positive)
	{
		if (positive)
		{
			if (m_Actor.Class is CCharacterClass cCharacterClass)
			{
				cCharacterClass.UpdateInitiativeBonus(m_Strength);
			}
		}
		else if (m_Actor.Class is CCharacterClass cCharacterClass2)
		{
			cCharacterClass2.UpdateInitiativeBonus(-m_Strength);
		}
		GameState.SortIntoInitiativeAndIDOrder();
		CUpdateInitiativeTrack_MessageData message = new CUpdateInitiativeTrack_MessageData(GameState.InternalCurrentActor);
		ScenarioRuleClient.MessageHandler(message);
	}

	public void OnUntoggleInitiativeBonus()
	{
		if (m_Actor.Class is CCharacterClass cCharacterClass)
		{
			cCharacterClass.UpdateInitiativeBonus(0);
		}
		GameState.SortIntoInitiativeAndIDOrder();
		CUpdateInitiativeTrack_MessageData message = new CUpdateInitiativeTrack_MessageData(GameState.InternalCurrentActor);
		ScenarioRuleClient.MessageHandler(message);
	}

	public CAdjustInitiativeActiveBonus_AdjustInitiative()
	{
	}

	public CAdjustInitiativeActiveBonus_AdjustInitiative(CAdjustInitiativeActiveBonus_AdjustInitiative state, ReferenceDictionary references)
		: base(state, references)
	{
	}
}
