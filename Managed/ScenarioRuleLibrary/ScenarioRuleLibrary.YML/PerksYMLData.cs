using System.Collections.Generic;
using System.Linq;
using SharedLibrary.Client;

namespace ScenarioRuleLibrary.YML;

public class PerksYMLData
{
	public string ID { get; set; }

	public string Name { get; set; }

	public string Description { get; set; }

	public string CharacterID { get; set; }

	public int Available { get; set; }

	public List<string> CardsToAddStrings { get; set; }

	public List<string> CardsToRemoveStrings { get; set; }

	public bool? IgnoreNegativeItemEffectsNullable { get; set; }

	public bool? IgnoreNegativeScenarioEffectsNullable { get; set; }

	public string FileName { get; private set; }

	public List<AttackModifierYMLData> CardsToAdd => CardsToAddStrings?.Select((string it) => ScenarioRuleClient.SRLYML.AttackModifiers.First((AttackModifierYMLData w) => it == w.Name)).ToList() ?? new List<AttackModifierYMLData>();

	public List<AttackModifierYMLData> CardsToRemove => CardsToRemoveStrings?.Select((string it) => ScenarioRuleClient.SRLYML.AttackModifiers.First((AttackModifierYMLData w) => it == w.Name)).ToList() ?? new List<AttackModifierYMLData>();

	public bool IgnoreNegativeItemEffects
	{
		get
		{
			if (!IgnoreNegativeItemEffectsNullable.HasValue)
			{
				return false;
			}
			return IgnoreNegativeItemEffectsNullable.Value;
		}
	}

	public bool IgnoreNegativeScenarioEffects
	{
		get
		{
			if (!IgnoreNegativeScenarioEffectsNullable.HasValue)
			{
				return false;
			}
			return IgnoreNegativeScenarioEffectsNullable.Value;
		}
	}

	public PerksYMLData(string fileName)
	{
		FileName = fileName;
		ID = null;
		Name = null;
		Description = null;
		CharacterID = null;
		Available = int.MaxValue;
		CardsToAddStrings = null;
		CardsToRemoveStrings = null;
		IgnoreNegativeItemEffectsNullable = null;
		IgnoreNegativeScenarioEffectsNullable = null;
	}

	public bool Validate()
	{
		if (ID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No ID specified for perk in file " + FileName);
			return false;
		}
		if (Name == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Name specified for perk in file " + FileName);
			return false;
		}
		if (Description == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Description specified for perk in file " + FileName);
			return false;
		}
		if (CharacterID == null)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No CharacterID specified for perk in file " + FileName);
			return false;
		}
		if (Available == int.MaxValue)
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "No Available amount specified for perk in file " + FileName);
			return false;
		}
		if ((CardsToAddStrings == null || CardsToAddStrings.Count == 0) && (CardsToRemoveStrings == null || CardsToRemoveStrings.Count == 0) && (!IgnoreNegativeItemEffectsNullable.HasValue || !IgnoreNegativeItemEffectsNullable.Value) && (!IgnoreNegativeScenarioEffectsNullable.HasValue || !IgnoreNegativeScenarioEffectsNullable.Value))
		{
			SharedClient.ValidationRecord.RecordParseFailure(FileName, "Invalid perk.  It does nothing. File " + FileName);
			return false;
		}
		return true;
	}

	public void UpdateData(PerksYMLData perkData)
	{
		if (perkData.Name != null)
		{
			Name = perkData.Name;
		}
		if (perkData.Description != null)
		{
			Description = perkData.Description;
		}
		if (perkData.CharacterID != null)
		{
			CharacterID = perkData.CharacterID;
		}
		if (perkData.Available != int.MaxValue)
		{
			Available = perkData.Available;
		}
		if (perkData.CardsToAddStrings != null)
		{
			CardsToAddStrings = perkData.CardsToAddStrings;
		}
		if (perkData.CardsToRemoveStrings != null)
		{
			CardsToRemoveStrings = perkData.CardsToRemoveStrings;
		}
		if (perkData.IgnoreNegativeItemEffectsNullable.HasValue)
		{
			IgnoreNegativeItemEffectsNullable = perkData.IgnoreNegativeItemEffectsNullable;
		}
		if (perkData.IgnoreNegativeScenarioEffectsNullable.HasValue)
		{
			IgnoreNegativeScenarioEffectsNullable = perkData.IgnoreNegativeScenarioEffectsNullable;
		}
	}
}
