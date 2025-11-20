using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.Adventure;
using MapRuleLibrary.Party;
using MapRuleLibrary.YML.Shared;
using SharedLibrary.Logger;
using StateCodeGenerator;

namespace MapRuleLibrary.MapState;

[Serializable]
public class CUnlockConditionState : ISerializable
{
	[Serializable]
	public class CUnlockConditionTargetState : ISerializable
	{
		public int CompletedValue;

		public CUnlockConditionTarget UnlockConditionTarget { get; private set; }

		public CUnlockConditionTargetState()
		{
		}

		public CUnlockConditionTargetState(CUnlockConditionTargetState state, ReferenceDictionary references)
		{
			CompletedValue = state.CompletedValue;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("CompletedValue", CompletedValue);
		}

		public CUnlockConditionTargetState(SerializationInfo info, StreamingContext context)
		{
			SerializationInfoEnumerator enumerator = info.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SerializationEntry current = enumerator.Current;
				try
				{
					if (current.Name == "CompletedValue")
					{
						CompletedValue = info.GetInt32("CompletedValue");
					}
				}
				catch (Exception ex)
				{
					DLLDebug.LogError("Exception while trying to deserialize CUnlockConditionTargetState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
					throw ex;
				}
			}
		}

		public CUnlockConditionTargetState(CUnlockConditionTarget unlockConditionTarget)
		{
			UnlockConditionTarget = unlockConditionTarget;
			CompletedValue = 0;
		}

		public void CacheUnlockConditionTarget(CUnlockConditionTarget unlockConditionTarget)
		{
			UnlockConditionTarget = unlockConditionTarget;
		}
	}

	public int TotalConditions;

	public int ConditionsMet;

	public bool IsInitialised { get; private set; }

	public List<CUnlockConditionTargetState> UnlockConditionTargetStates { get; private set; }

	public List<CUnlockConditionTargetState> FailConditionTargetStates { get; private set; }

	public int PreviousProgress { get; private set; }

	public bool Ordered { get; private set; }

	public bool SingleScenario { get; private set; }

	public bool Failed { get; set; }

	public string OverrideCharacterID { get; set; }

	public string OverrideCharacterName { get; set; }

	public bool NegativeCondition { get; set; }

	public bool HasProgressed { get; private set; }

	public int TotalConditionsAndTargets
	{
		get
		{
			int num = 0;
			if (TotalConditions == 0 && UnlockCondition != null)
			{
				if (UnlockCondition.RequiredChoiceContainer != null && UnlockCondition.RequiredChoiceContainer.Count > 0)
				{
					foreach (CUnlockChoiceContainer item in UnlockCondition.RequiredChoiceContainer)
					{
						num = ((!item.OROperator) ? (num + ((IEnumerable<Tuple<EUnlockConditionType, int>>)item.RequiredConditionsTotal).Sum((Func<Tuple<EUnlockConditionType, int>, int>)Sum)) : ((IEnumerable<Tuple<EUnlockConditionType, int>>)item.RequiredConditionsTotal).Sum((Func<Tuple<EUnlockConditionType, int>, int>)Sum));
					}
				}
				if (UnlockCondition.RequiredConditionsTotal != null && UnlockCondition.RequiredConditionsTotal.Count > 0)
				{
					num += ((IEnumerable<Tuple<EUnlockConditionType, int>>)UnlockCondition.RequiredConditionsTotal).Sum((Func<Tuple<EUnlockConditionType, int>, int>)Sum);
				}
			}
			return TotalConditions + num + ((UnlockCondition != null) ? ((UnlockCondition.TargetsRequired == 0) ? (((IEnumerable<CUnlockConditionTarget>)UnlockCondition.Targets).Sum((Func<CUnlockConditionTarget, int>)SumValue) + ((IEnumerable<CUnlockConditionTarget>)UnlockCondition.Targets).Sum((Func<CUnlockConditionTarget, int>)SumAmount)) : 0) : 0);
			static int Sum(Tuple<EUnlockConditionType, int> tuple)
			{
				return tuple.Item2;
			}
			static int SumAmount(CUnlockConditionTarget target)
			{
				return target.Amount;
			}
			static int SumValue(CUnlockConditionTarget target)
			{
				return target.Value;
			}
		}
	}

	public int CurrentProgress
	{
		get
		{
			int num = ConditionsMet;
			if (Failed)
			{
				return num;
			}
			if (UnlockCondition != null)
			{
				int targetsRequired = UnlockCondition.TargetsRequired;
				int num2 = UnlockConditionTargetStates.Sum((CUnlockConditionTargetState t) => Math.Max(0, t.CompletedValue));
				num = ((targetsRequired <= 0) ? (num + ((IEnumerable<CUnlockConditionTargetState>)UnlockConditionTargetStates).Sum((Func<CUnlockConditionTargetState, int>)Sum)) : ((targetsRequired <= num2) ? (num + targetsRequired) : (num + ((IEnumerable<CUnlockConditionTargetState>)UnlockConditionTargetStates).Sum((Func<CUnlockConditionTargetState, int>)Sum))));
			}
			return num;
			static int Sum(CUnlockConditionTargetState state)
			{
				return Math.Max(0, state.CompletedValue);
			}
		}
	}

	public CUnlockCondition UnlockCondition { get; private set; }

	public CUnlockCondition FailCondition { get; private set; }

	public CUnlockConditionState()
	{
	}

	public CUnlockConditionState(CUnlockConditionState state, ReferenceDictionary references)
	{
		IsInitialised = state.IsInitialised;
		UnlockConditionTargetStates = references.Get(state.UnlockConditionTargetStates);
		if (UnlockConditionTargetStates == null && state.UnlockConditionTargetStates != null)
		{
			UnlockConditionTargetStates = new List<CUnlockConditionTargetState>();
			for (int i = 0; i < state.UnlockConditionTargetStates.Count; i++)
			{
				CUnlockConditionTargetState cUnlockConditionTargetState = state.UnlockConditionTargetStates[i];
				CUnlockConditionTargetState cUnlockConditionTargetState2 = references.Get(cUnlockConditionTargetState);
				if (cUnlockConditionTargetState2 == null && cUnlockConditionTargetState != null)
				{
					cUnlockConditionTargetState2 = new CUnlockConditionTargetState(cUnlockConditionTargetState, references);
					references.Add(cUnlockConditionTargetState, cUnlockConditionTargetState2);
				}
				UnlockConditionTargetStates.Add(cUnlockConditionTargetState2);
			}
			references.Add(state.UnlockConditionTargetStates, UnlockConditionTargetStates);
		}
		FailConditionTargetStates = references.Get(state.FailConditionTargetStates);
		if (FailConditionTargetStates == null && state.FailConditionTargetStates != null)
		{
			FailConditionTargetStates = new List<CUnlockConditionTargetState>();
			for (int j = 0; j < state.FailConditionTargetStates.Count; j++)
			{
				CUnlockConditionTargetState cUnlockConditionTargetState3 = state.FailConditionTargetStates[j];
				CUnlockConditionTargetState cUnlockConditionTargetState4 = references.Get(cUnlockConditionTargetState3);
				if (cUnlockConditionTargetState4 == null && cUnlockConditionTargetState3 != null)
				{
					cUnlockConditionTargetState4 = new CUnlockConditionTargetState(cUnlockConditionTargetState3, references);
					references.Add(cUnlockConditionTargetState3, cUnlockConditionTargetState4);
				}
				FailConditionTargetStates.Add(cUnlockConditionTargetState4);
			}
			references.Add(state.FailConditionTargetStates, FailConditionTargetStates);
		}
		PreviousProgress = state.PreviousProgress;
		Ordered = state.Ordered;
		SingleScenario = state.SingleScenario;
		Failed = state.Failed;
		OverrideCharacterID = state.OverrideCharacterID;
		OverrideCharacterName = state.OverrideCharacterName;
		TotalConditions = state.TotalConditions;
		ConditionsMet = state.ConditionsMet;
		NegativeCondition = state.NegativeCondition;
		HasProgressed = state.HasProgressed;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("IsInitialised", IsInitialised);
		info.AddValue("UnlockConditionTargetStates", UnlockConditionTargetStates);
		info.AddValue("FailConditionTargetStates", FailConditionTargetStates);
		info.AddValue("PreviousProgress", PreviousProgress);
		info.AddValue("TotalConditions", TotalConditions);
		info.AddValue("ConditionsMet", ConditionsMet);
		info.AddValue("Ordered", Ordered);
		info.AddValue("SingleScenario", SingleScenario);
		info.AddValue("Failed", Failed);
		info.AddValue("OverrideCharacterID", OverrideCharacterID);
		info.AddValue("OverrideCharacterName", OverrideCharacterName);
	}

	public CUnlockConditionState(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "IsInitialised":
					IsInitialised = info.GetBoolean("IsInitialised");
					break;
				case "UnlockConditionTargetStates":
					UnlockConditionTargetStates = (List<CUnlockConditionTargetState>)info.GetValue("UnlockConditionTargetStates", typeof(List<CUnlockConditionTargetState>));
					break;
				case "FailConditionTargetStates":
					FailConditionTargetStates = (List<CUnlockConditionTargetState>)info.GetValue("FailConditionTargetStates", typeof(List<CUnlockConditionTargetState>));
					break;
				case "PreviousProgress":
					PreviousProgress = info.GetInt32("PreviousProgress");
					break;
				case "TotalConditions":
					TotalConditions = info.GetInt32("TotalConditions");
					break;
				case "ConditionsMet":
					ConditionsMet = info.GetInt32("ConditionsMet");
					break;
				case "Ordered":
					Ordered = info.GetBoolean("Ordered");
					break;
				case "SingleScenario":
					SingleScenario = info.GetBoolean("SingleScenario");
					break;
				case "Failed":
					Failed = info.GetBoolean("Failed");
					break;
				case "OverrideCharacterID":
					OverrideCharacterID = info.GetString("OverrideCharacterID");
					break;
				case "OverrideCharacterName":
					OverrideCharacterName = info.GetString("OverrideCharacterName");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CUnlockConditionState entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (FailConditionTargetStates == null)
		{
			FailConditionTargetStates = new List<CUnlockConditionTargetState>();
		}
	}

	public CUnlockConditionState(CUnlockCondition unlockCondition, CUnlockCondition failCondition = null)
	{
		IsInitialised = false;
		Failed = false;
		UnlockCondition = unlockCondition;
		FailCondition = failCondition;
		Ordered = unlockCondition.Ordered;
		SingleScenario = unlockCondition.SingleScenario;
		UnlockConditionTargetStates = new List<CUnlockConditionTargetState>();
		FailConditionTargetStates = new List<CUnlockConditionTargetState>();
		HasProgressed = false;
		if (unlockCondition.Targets != null)
		{
			foreach (CUnlockConditionTarget target in unlockCondition.Targets)
			{
				UnlockConditionTargetStates.Add(new CUnlockConditionTargetState(target));
			}
		}
		if (failCondition == null || failCondition.Targets == null)
		{
			return;
		}
		foreach (CUnlockConditionTarget target2 in failCondition.Targets)
		{
			FailConditionTargetStates.Add(new CUnlockConditionTargetState(target2));
		}
	}

	public void CacheUnlockCondition(CUnlockCondition unlockCondition, CUnlockCondition failCondition = null)
	{
		UnlockCondition = unlockCondition;
		if (unlockCondition.Targets != null)
		{
			for (int i = 0; i < unlockCondition.Targets.Count; i++)
			{
				CUnlockConditionTarget unlockConditionTarget = unlockCondition.Targets[i];
				if (i < UnlockConditionTargetStates.Count)
				{
					UnlockConditionTargetStates[i].CacheUnlockConditionTarget(unlockConditionTarget);
				}
				else
				{
					UnlockConditionTargetStates.Add(new CUnlockConditionTargetState(unlockConditionTarget));
				}
			}
			UnlockConditionTargetStates.RemoveAll((CUnlockConditionTargetState x) => x.UnlockConditionTarget == null);
		}
		FailCondition = failCondition;
		if (FailConditionTargetStates == null)
		{
			FailConditionTargetStates = new List<CUnlockConditionTargetState>();
		}
		if (failCondition == null || failCondition.Targets == null)
		{
			return;
		}
		for (int num = 0; num < failCondition.Targets.Count; num++)
		{
			CUnlockConditionTarget unlockConditionTarget2 = failCondition.Targets[num];
			if (num < FailConditionTargetStates.Count)
			{
				FailConditionTargetStates[num].CacheUnlockConditionTarget(unlockConditionTarget2);
			}
			else
			{
				FailConditionTargetStates.Add(new CUnlockConditionTargetState(unlockConditionTarget2));
			}
		}
		FailConditionTargetStates.RemoveAll((CUnlockConditionTargetState x) => x.UnlockConditionTarget == null);
	}

	public void UpdateGoal(string characterID)
	{
		if (UnlockCondition == null)
		{
			return;
		}
		List<CMapCharacter> checkCharacters = AdventureState.MapState.MapParty.CheckCharacters;
		foreach (CUnlockConditionTarget target in UnlockCondition.Targets)
		{
			if (target.TargetFilter == null)
			{
				continue;
			}
			if (target.TargetFilter.EqualityString.Contains("<") && target.Value > 1)
			{
				NegativeCondition = true;
			}
			if (target.TargetFilter.Level && characterID != null)
			{
				CMapCharacter cMapCharacter = checkCharacters.FirstOrDefault((CMapCharacter x) => x.CharacterID == characterID);
				if (cMapCharacter != null)
				{
					target.Value = target.TargetFilter.Value + cMapCharacter.Level;
				}
			}
		}
	}

	public bool IsUnlocked(bool useCurrentScenarioStats = false)
	{
		bool num = CheckUnlocked(useCurrentScenarioStats, UnlockCondition, UnlockConditionTargetStates);
		if (FailCondition != null)
		{
			Failed = CheckUnlocked(useCurrentScenarioStats, FailCondition, FailConditionTargetStates);
		}
		if (num)
		{
			return !Failed;
		}
		return false;
	}

	public bool CheckUnlocked(bool useCurrentScenarioStats, CUnlockCondition unlockCondition, List<CUnlockConditionTargetState> unlockConditionTargetStates, bool checkingFailCondition = false)
	{
		int currentProgress = CurrentProgress;
		bool flag = false;
		int num = 0;
		bool flag2 = true;
		if (unlockCondition != null)
		{
			flag = unlockCondition.CheckConditions(OverrideCharacterID, out TotalConditions, out ConditionsMet);
			if (SingleScenario)
			{
				bool flag3 = true;
				foreach (CUnlockConditionTargetState unlockConditionTargetState in unlockConditionTargetStates)
				{
					bool flag4 = unlockCondition.CheckTarget(unlockConditionTargetState.UnlockConditionTarget, unlockConditionTargetState.CompletedValue, out unlockConditionTargetState.CompletedValue, OverrideCharacterID, OverrideCharacterName, useCurrentScenarioStats, checkingFailCondition);
					unlockConditionTargetState.CompletedValue = Math.Min(unlockConditionTargetState.CompletedValue, NegativeCondition ? int.MaxValue : (unlockConditionTargetState.UnlockConditionTarget.Value + unlockConditionTargetState.UnlockConditionTarget.Amount));
					unlockConditionTargetState.CompletedValue = Math.Max(unlockConditionTargetState.CompletedValue, 0);
					if (flag4)
					{
						num++;
					}
					else
					{
						flag3 = false;
					}
					if (unlockCondition.TargetsRequired == 0 && !flag4)
					{
						flag2 = false;
					}
				}
				if (unlockCondition.TargetsRequired > 0 && num >= unlockCondition.TargetsRequired)
				{
					flag3 = true;
				}
				if (!flag3)
				{
					num = 0;
					foreach (CUnlockConditionTargetState unlockConditionTargetState2 in unlockConditionTargetStates)
					{
						unlockConditionTargetState2.CompletedValue = 0;
					}
				}
			}
			else
			{
				foreach (CUnlockConditionTargetState unlockConditionTargetState3 in unlockConditionTargetStates)
				{
					bool flag5 = unlockCondition.CheckTarget(unlockConditionTargetState3.UnlockConditionTarget, unlockConditionTargetState3.CompletedValue, out unlockConditionTargetState3.CompletedValue, OverrideCharacterID, OverrideCharacterName, useCurrentScenarioStats, checkingFailCondition);
					unlockConditionTargetState3.CompletedValue = Math.Min(unlockConditionTargetState3.CompletedValue, NegativeCondition ? int.MaxValue : (unlockConditionTargetState3.UnlockConditionTarget.Value + unlockConditionTargetState3.UnlockConditionTarget.Amount));
					unlockConditionTargetState3.CompletedValue = Math.Max(unlockConditionTargetState3.CompletedValue, 0);
					if (flag5)
					{
						num++;
					}
					if (unlockCondition.TargetsRequired == 0 && !flag5)
					{
						flag2 = false;
					}
				}
			}
			if (unlockCondition.TargetsRequired > 0)
			{
				TotalConditions = unlockCondition.TargetsRequired;
			}
			if (num < unlockCondition.TargetsRequired)
			{
				flag2 = false;
			}
			if (currentProgress != CurrentProgress)
			{
				HasProgressed = true;
				PreviousProgress = currentProgress;
			}
			else
			{
				HasProgressed = false;
			}
		}
		return flag && flag2;
	}

	public void ResetProgress()
	{
		ConditionsMet = 0;
		foreach (CUnlockConditionTargetState unlockConditionTargetState in UnlockConditionTargetStates)
		{
			unlockConditionTargetState.CompletedValue = 0;
		}
		foreach (CUnlockConditionTargetState failConditionTargetState in FailConditionTargetStates)
		{
			failConditionTargetState.CompletedValue = 0;
		}
	}

	public virtual void Init()
	{
		if (!IsInitialised)
		{
			IsInitialised = true;
		}
	}
}
