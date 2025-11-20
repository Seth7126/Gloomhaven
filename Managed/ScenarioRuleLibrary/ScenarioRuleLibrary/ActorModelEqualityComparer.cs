using System;
using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class ActorModelEqualityComparer : IEqualityComparer<ActorState>
{
	public bool Equals(ActorState x, ActorState y)
	{
		if (x == y)
		{
			return true;
		}
		if (x == null)
		{
			return false;
		}
		if (y == null)
		{
			return false;
		}
		if (x.GetType() != y.GetType())
		{
			return false;
		}
		if (x.ClassID == y.ClassID)
		{
			return x.ChosenModelIndex == y.ChosenModelIndex;
		}
		return false;
	}

	public int GetHashCode(ActorState obj)
	{
		return HashCode.Combine(obj.ClassID, obj.ChosenModelIndex);
	}
}
