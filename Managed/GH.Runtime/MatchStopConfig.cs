using System.Collections.Generic;

public class MatchStopConfig
{
	public List<string> m_MissionGoal;

	public List<string> m_MissionGoalPassed;

	public string m_MissionID;

	public AWGameMode m_GameMode;

	public string m_RunDifficulty;

	public string m_ActivatedDLC;

	public int m_PlayerCountHuman;

	public int m_TimeActiveSeconds;

	public AWMatchEndReason m_EndReason;

	public AWPlayerResult m_PlayerResult;

	public int m_TurnCount;

	public int m_RosterLevel;

	public List<string> m_ClassesSelected;

	public List<List<string>> m_ClassesCardsSelected;

	public List<string> m_CharacterStatus;

	public Dictionary<string, List<string>> m_CollectedItems;

	public int m_RoomOpenCount;

	public int m_RoomTotalCount;

	public Dictionary<string, int> m_EnemyEncounters;

	public Dictionary<string, int> m_EnemyKilled;

	public int m_TimeActiveSecRunLTD;

	public int m_GoldEarningsRunLTD;

	public int m_GoldBalance;
}
