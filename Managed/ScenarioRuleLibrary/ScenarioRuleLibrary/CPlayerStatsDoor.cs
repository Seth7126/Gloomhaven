using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsDoor : CPlayerStatsAction, ISerializable
{
	public int Doors { get; private set; }

	public List<CMap> RevealedMaps { get; private set; }

	public CPlayerStatsDoor()
	{
	}

	public CPlayerStatsDoor(CPlayerStatsDoor state, ReferenceDictionary references)
		: base(state, references)
	{
		Doors = state.Doors;
		RevealedMaps = references.Get(state.RevealedMaps);
		if (RevealedMaps != null || state.RevealedMaps == null)
		{
			return;
		}
		RevealedMaps = new List<CMap>();
		for (int i = 0; i < state.RevealedMaps.Count; i++)
		{
			CMap cMap = state.RevealedMaps[i];
			CMap cMap2 = references.Get(cMap);
			if (cMap2 == null && cMap != null)
			{
				cMap2 = new CMap(cMap, references);
				references.Add(cMap, cMap2);
			}
			RevealedMaps.Add(cMap2);
		}
		references.Add(state.RevealedMaps, RevealedMaps);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Doors", Doors);
		info.AddValue("RevealedMaps", RevealedMaps);
	}

	public CPlayerStatsDoor(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "Doors"))
				{
					if (name == "RevealedMaps")
					{
						RevealedMaps = (List<CMap>)info.GetValue("RevealedMaps", typeof(List<CMap>));
					}
				}
				else
				{
					Doors = info.GetInt32("Doors");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CPlayerStatsDoor entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsDoor(int doors, List<CMap> revealedMaps, string advGuid, string sceGuid, string questType, int round, string actingClass, string actedOnClass, string actingType, string actedOnType, List<ElementInfusionBoardManager.EElement> infused, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, List<PositiveConditionPair> actedOnpositiveConditions, List<NegativeConditionPair> actedOnnegativeConditions, string actingGUID, string actedOnGUID, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = int.MaxValue, bool performedBySummons = false, bool rolledIntoSummoner = false)
		: base(advGuid, sceGuid, questType, round, actingClass, actedOnClass, actingType, actedOnType, infused, positiveConditions, negativeConditions, actedOnpositiveConditions, actedOnnegativeConditions, actingGUID, actedOnGUID, cardID, cardType, abilityType, actingAbilityName, abilityStrength, performedBySummons, rolledIntoSummoner)
	{
		Doors = doors;
		RevealedMaps = revealedMaps;
	}
}
