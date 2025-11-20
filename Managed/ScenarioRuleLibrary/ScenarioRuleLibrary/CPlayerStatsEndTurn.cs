using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace ScenarioRuleLibrary;

[Serializable]
public class CPlayerStatsEndTurn : CPlayerStatsAction, ISerializable
{
	public int TileX { get; private set; }

	public int TileY { get; private set; }

	public CPlayerStatsEndTurn()
	{
	}

	public CPlayerStatsEndTurn(CPlayerStatsEndTurn state, ReferenceDictionary references)
		: base(state, references)
	{
		TileX = state.TileX;
		TileY = state.TileY;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("TileX", TileX);
		info.AddValue("TileY", TileY);
	}

	public CPlayerStatsEndTurn(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				string name = current.Name;
				if (!(name == "TileX"))
				{
					if (name == "TileY")
					{
						TileY = info.GetInt32("TileY");
					}
				}
				else
				{
					TileX = info.GetInt32("TileX");
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CPlayerStatsEndTurn entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CPlayerStatsEndTurn(int x, int y, string advGuid, string sceGuid, string questType, int round, string actingClass, string actedOnClass, string actingType, string actedOnType, List<ElementInfusionBoardManager.EElement> infused, List<PositiveConditionPair> positiveConditions, List<NegativeConditionPair> negativeConditions, List<PositiveConditionPair> actedOnpositiveConditions, List<NegativeConditionPair> actedOnnegativeConditions, string actingGUID, string actedOnGUID, int cardID = int.MaxValue, CBaseCard.ECardType cardType = CBaseCard.ECardType.None, CAbility.EAbilityType abilityType = CAbility.EAbilityType.None, string actingAbilityName = "", int abilityStrength = int.MaxValue, bool performedBySummons = false, bool rolledIntoSummoner = false, int allyAdjacent = 0, int enemyAdjacent = 0, int obstacleAdjacent = 0, bool wallAdjacent = false)
		: base(advGuid, sceGuid, questType, round, actingClass, actedOnClass, actingType, actedOnType, infused, positiveConditions, negativeConditions, actedOnpositiveConditions, actedOnnegativeConditions, actingGUID, actedOnGUID, cardID, cardType, abilityType, actingAbilityName, abilityStrength, performedBySummons, rolledIntoSummoner, 0, 0, allyAdjacent, enemyAdjacent, obstacleAdjacent, wallAdjacent)
	{
		TileX = x;
		TileY = y;
	}
}
