using System;
using ScenarioRuleLibrary;

public static class CActorExtensions
{
	public static bool IsEnemyByDefault(this CActor actor)
	{
		if (actor != null)
		{
			return actor.Class is CMonsterClass;
		}
		return false;
	}

	public static bool IsPlayerByDefault(this CActor actor)
	{
		if (actor != null)
		{
			return actor.Class is CCharacterClass;
		}
		return false;
	}

	public static bool IsHeroSummonByDefault(this CActor actor)
	{
		if (actor != null)
		{
			return actor.Class is CHeroSummonClass;
		}
		return false;
	}

	public static bool IsPropActorByDefault(this CActor actor, bool checkAttachedToProp = true)
	{
		if (actor != null && actor is CObjectActor cObjectActor)
		{
			if (checkAttachedToProp)
			{
				if (checkAttachedToProp)
				{
					return cObjectActor.IsAttachedToProp;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public static CActor.EType GetDefaultType(this CActor actor)
	{
		if (actor.IsPlayerByDefault())
		{
			return CActor.EType.Player;
		}
		if (actor.IsEnemyByDefault())
		{
			return CActor.EType.Enemy;
		}
		if (actor.IsHeroSummonByDefault())
		{
			return CActor.EType.HeroSummon;
		}
		throw new Exception("Invalid actor default type");
	}
}
