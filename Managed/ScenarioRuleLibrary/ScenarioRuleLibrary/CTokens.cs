using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary.YML;
using SharedLibrary.SimpleLog;

namespace ScenarioRuleLibrary;

public class CTokens
{
	private volatile List<PositiveConditionPair> PositiveTokens;

	private volatile List<NegativeConditionPair> NegativeTokens;

	public CActor Actor { get; private set; }

	public CCondition.EPositiveCondition ModifiedPositiveCondition { get; set; }

	public CCondition.ENegativeCondition ModifiedNegativeCondition { get; set; }

	public List<PositiveConditionPair> CheckPositiveTokens
	{
		get
		{
			lock (PositiveTokens)
			{
				return PositiveTokens.ToList();
			}
		}
	}

	public List<NegativeConditionPair> CheckNegativeTokens
	{
		get
		{
			lock (NegativeTokens)
			{
				return NegativeTokens.ToList();
			}
		}
	}

	public CTokens(CActor actor)
	{
		Actor = actor;
		PositiveTokens = new List<PositiveConditionPair>();
		NegativeTokens = new List<NegativeConditionPair>();
	}

	public CTokens(CActor actor, List<PositiveConditionPair> posCons, List<NegativeConditionPair> negCons)
	{
		Actor = actor;
		PositiveTokens = posCons.ToList();
		NegativeTokens = negCons.ToList();
	}

	public void AddPositiveToken(CCondition.EPositiveCondition token, int duration, EConditionDecTrigger decTrigger, CActor actor, bool recall = false)
	{
		bool flag = true;
		ModifiedPositiveCondition = CCondition.EPositiveCondition.NA;
		if (!recall)
		{
			List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(actor, CAbility.EAbilityType.ChangeCondition);
			if (list != null)
			{
				foreach (CChangeConditionActiveBonus item in list)
				{
					flag = item.ChangeCondition(this, token, duration, decTrigger, actor);
				}
			}
		}
		if (flag)
		{
			lock (PositiveTokens)
			{
				PositiveTokens.Add(new PositiveConditionPair(token, RewardCondition.EConditionMapDuration.Now, duration, decTrigger));
			}
			SimpleLog.AddToSimpleLog("[TOKENS] Added positive token: " + token.ToString() + " to actor: " + Actor.ActorLocKey() + "(" + Actor.ID + ")");
		}
		CUpdateWorldspaceConditionsUI_MessageData message = new CUpdateWorldspaceConditionsUI_MessageData(Actor);
		ScenarioRuleClient.MessageHandler(message);
	}

	public bool RemovePositiveToken(CCondition.EPositiveCondition token, EConditionDecTrigger decTrigger = EConditionDecTrigger.None)
	{
		List<PositiveConditionPair> list = PositiveTokens.FindAll((PositiveConditionPair s) => s.PositiveCondition == token);
		bool result = false;
		foreach (PositiveConditionPair item in list)
		{
			if (item != null && (decTrigger == EConditionDecTrigger.None || item.ConditionDecTrigger == decTrigger))
			{
				lock (PositiveTokens)
				{
					PositiveTokens.Remove(item);
				}
				SimpleLog.AddToSimpleLog("[TOKENS] Removed positive token: " + token.ToString() + " from actor: " + Actor.ActorLocKey() + "(" + Actor.ID + ")");
			}
		}
		CUpdateWorldspaceConditionsUI_MessageData message = new CUpdateWorldspaceConditionsUI_MessageData(Actor);
		ScenarioRuleClient.MessageHandler(message);
		return result;
	}

	public void AddNegativeToken(CCondition.ENegativeCondition token, int duration, EConditionDecTrigger decTrigger, CActor actor, bool recall = false)
	{
		bool flag = true;
		ModifiedNegativeCondition = CCondition.ENegativeCondition.NA;
		if (!recall)
		{
			List<CActiveBonus> list = CActiveBonus.FindApplicableActiveBonuses(actor, CAbility.EAbilityType.ChangeCondition);
			if (list != null)
			{
				foreach (CChangeConditionActiveBonus item in list)
				{
					flag = item.ChangeCondition(this, token, duration, decTrigger, actor);
				}
			}
		}
		if (flag)
		{
			lock (NegativeTokens)
			{
				NegativeTokens.Add(new NegativeConditionPair(token, RewardCondition.EConditionMapDuration.Now, duration, decTrigger));
			}
			SimpleLog.AddToSimpleLog("[TOKENS] Added negative token: " + token.ToString() + " to actor: " + Actor.ActorLocKey() + "(" + Actor.ID + ")");
		}
		CUpdateWorldspaceConditionsUI_MessageData message = new CUpdateWorldspaceConditionsUI_MessageData(Actor);
		ScenarioRuleClient.MessageHandler(message);
	}

	public bool RemoveNegativeToken(CCondition.ENegativeCondition token, EConditionDecTrigger decTrigger = EConditionDecTrigger.None)
	{
		List<NegativeConditionPair> list = NegativeTokens.FindAll((NegativeConditionPair s) => s.NegativeCondition == token);
		bool result = false;
		foreach (NegativeConditionPair item in list)
		{
			if (item != null && (decTrigger == EConditionDecTrigger.None || item.ConditionDecTrigger == decTrigger))
			{
				lock (NegativeTokens)
				{
					NegativeTokens.Remove(item);
				}
				SimpleLog.AddToSimpleLog("[TOKENS] Removed negative token: " + token.ToString() + " from actor: " + Actor.ActorLocKey() + "(" + Actor.ID + ")");
				result = true;
			}
		}
		CUpdateWorldspaceConditionsUI_MessageData message = new CUpdateWorldspaceConditionsUI_MessageData(Actor);
		ScenarioRuleClient.MessageHandler(message);
		return result;
	}

	public void ProcessTokens(EConditionDecTrigger trigger, out List<CCondition.EPositiveCondition> positiveConditionsRemoved, out List<CCondition.ENegativeCondition> negativeConditionsRemoved)
	{
		positiveConditionsRemoved = new List<CCondition.EPositiveCondition>();
		negativeConditionsRemoved = new List<CCondition.ENegativeCondition>();
		foreach (PositiveConditionPair checkPositiveToken in CheckPositiveTokens)
		{
			if (checkPositiveToken.ConditionDecTrigger != trigger)
			{
				continue;
			}
			checkPositiveToken.RoundDuration--;
			if (checkPositiveToken.RoundDuration <= 0)
			{
				lock (PositiveTokens)
				{
					PositiveTokens.Remove(checkPositiveToken);
				}
				positiveConditionsRemoved.Add(checkPositiveToken.PositiveCondition);
				SimpleLog.AddToSimpleLog("[TOKENS] Processed and removed positive token: " + checkPositiveToken.PositiveCondition.ToString() + " to actor: " + Actor.ActorLocKey() + "(" + Actor.ID + ")");
			}
		}
		foreach (NegativeConditionPair checkNegativeToken in CheckNegativeTokens)
		{
			if (checkNegativeToken.ConditionDecTrigger != trigger)
			{
				continue;
			}
			checkNegativeToken.RoundDuration--;
			if (checkNegativeToken.RoundDuration <= 0)
			{
				lock (NegativeTokens)
				{
					NegativeTokens.Remove(checkNegativeToken);
				}
				negativeConditionsRemoved.Add(checkNegativeToken.NegativeCondition);
				SimpleLog.AddToSimpleLog("[TOKENS] Processed and removed negative token: " + checkNegativeToken.NegativeCondition.ToString() + " to actor: " + Actor.ActorLocKey() + "(" + Actor.ID + ")");
			}
		}
		positiveConditionsRemoved = positiveConditionsRemoved.Distinct().ToList();
		negativeConditionsRemoved = negativeConditionsRemoved.Distinct().ToList();
		CUpdateWorldspaceConditionsUI_MessageData message = new CUpdateWorldspaceConditionsUI_MessageData(Actor);
		ScenarioRuleClient.MessageHandler(message);
	}

	public bool HasKey(CCondition.EPositiveCondition token)
	{
		PositiveTokens.RemoveAll((PositiveConditionPair r) => r == null);
		return CheckPositiveTokens.Any((PositiveConditionPair s) => s.PositiveCondition == token);
	}

	public bool HasKey(CCondition.ENegativeCondition token)
	{
		NegativeTokens.RemoveAll((NegativeConditionPair r) => r == null);
		return CheckNegativeTokens.Any((NegativeConditionPair s) => s.NegativeCondition == token);
	}

	public List<CCondition.EPositiveCondition> GetAllPositiveConditions()
	{
		List<CCondition.EPositiveCondition> list = new List<CCondition.EPositiveCondition>();
		foreach (PositiveConditionPair checkPositiveToken in CheckPositiveTokens)
		{
			if (!list.Contains(checkPositiveToken.PositiveCondition))
			{
				list.Add(checkPositiveToken.PositiveCondition);
			}
		}
		return list;
	}

	public List<CCondition.ENegativeCondition> GetAllNegativeConditions()
	{
		List<CCondition.ENegativeCondition> list = new List<CCondition.ENegativeCondition>();
		foreach (NegativeConditionPair checkNegativeToken in CheckNegativeTokens)
		{
			if (!list.Contains(checkNegativeToken.NegativeCondition))
			{
				list.Add(checkNegativeToken.NegativeCondition);
			}
		}
		return list;
	}

	public EAdvantageStatuses GetAdvantageStatus(bool addAdvantage, bool addDisadvantage)
	{
		bool flag = addAdvantage || CheckPositiveTokens.Any((PositiveConditionPair a) => a.PositiveCondition == CCondition.EPositiveCondition.Advantage || a.PositiveCondition == CCondition.EPositiveCondition.Strengthen);
		bool flag2 = addDisadvantage || CheckNegativeTokens.Any((NegativeConditionPair a) => a.NegativeCondition == CCondition.ENegativeCondition.Disadvantage || a.NegativeCondition == CCondition.ENegativeCondition.Muddle);
		if (flag && flag2)
		{
			return EAdvantageStatuses.None;
		}
		if (flag)
		{
			return EAdvantageStatuses.Advantage;
		}
		if (flag2)
		{
			return EAdvantageStatuses.Disadvantage;
		}
		return EAdvantageStatuses.None;
	}
}
