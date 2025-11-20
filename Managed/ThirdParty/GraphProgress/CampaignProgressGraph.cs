#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using SM.Utils;

namespace GraphProgress;

public class CampaignProgressGraph
{
	private static CampaignProgressGraph _instance;

	private QuestVertex[] _questVertices = new QuestVertex[51];

	private List<QuestVertex> _mainQuestVertices = new List<QuestVertex>();

	private List<QuestEdge> _edges;

	public static CampaignProgressGraph Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new CampaignProgressGraph();
			}
			return _instance;
		}
	}

	public IReadOnlyList<QuestVertex> QuestVertices => _questVertices;

	public IReadOnlyList<QuestVertex> MainQuestVertices => _mainQuestVertices;

	public event Action<int, string> QuestCompleteEvent;

	public event Action<int, string> QuestBlockedEvent;

	private CampaignProgressGraph()
	{
		CreateQuestVertexes();
		CreateEdgesAndCondition();
	}

	public void TryCompleteQuest(string stringID)
	{
		int num = 3;
		string s = stringID.Substring(stringID.Length - num, num);
		TryCompleteQuest(int.Parse(s));
	}

	public void TryCompleteQuest(int questId)
	{
		QuestVertex vertex = GetVertex(questId);
		if (vertex != null)
		{
			vertex.Complete();
			QuestVertex[] questVertices = _questVertices;
			for (int i = 0; i < questVertices.Length; i++)
			{
				questVertices[i].UpdateQuestState();
			}
		}
	}

	public float CalculateProgress()
	{
		float num = 0f;
		QuestVertex[] questVertices = _questVertices;
		foreach (QuestVertex questVertex in questVertices)
		{
			num += questVertex.GetCost();
		}
		return num;
	}

	private void CreateQuestVertexes()
	{
		for (int i = 0; i < _questVertices.Length; i++)
		{
			_questVertices[i] = new QuestVertex(i + 1);
			_questVertices[i].TypeChangedEvent += OnQuestTypeChanged;
		}
	}

	private void CreateEdgesAndCondition()
	{
		_edges = new List<QuestEdge>();
		_edges.Add(QuestEdge.New(GetVertex(1), GetVertex(2)));
		_edges.Add(QuestEdge.New(GetVertex(2), GetVertex(3)));
		_edges.Add(QuestEdge.New(GetVertex(8), GetVertex(3)).AddConditions(Condition.New(GetVertex(8), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(2), GetVertex(4)));
		_edges.Add(QuestEdge.New(GetVertex(4), GetVertex(5)));
		_edges.Add(QuestEdge.New(GetVertex(4), GetVertex(6)));
		_edges.Add(QuestEdge.New(GetVertex(8), GetVertex(7)));
		_edges.Add(QuestEdge.New(GetVertex(14), GetVertex(7)).AddConditions(Condition.New(GetVertex(14), ValidType.IsDone), ConditionType.Required));
		_edges.Add(QuestEdge.New(GetVertex(3), GetVertex(8)));
		_edges.Add(QuestEdge.New(GetVertex(6), GetVertex(8)));
		_edges.Add(QuestEdge.New(GetVertex(9), GetVertex(8)).AddConditions(Condition.New(GetVertex(9), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(3), GetVertex(9)));
		_edges.Add(QuestEdge.New(GetVertex(8), GetVertex(9)).AddConditions(Condition.New(GetVertex(8), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(5), GetVertex(10)));
		_edges.Add(QuestEdge.New(GetVertex(27), GetVertex(10)).AddConditions(Condition.New(GetVertex(27), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(9), GetVertex(11)));
		_edges.Add(QuestEdge.New(GetVertex(12), GetVertex(11)).AddConditions(Condition.New(GetVertex(12), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(9), GetVertex(12)));
		_edges.Add(QuestEdge.New(GetVertex(11), GetVertex(12)).AddConditions(Condition.New(GetVertex(11), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(8), GetVertex(13)));
		_edges.Add(QuestEdge.New(GetVertex(8), GetVertex(14)));
		_edges.Add(QuestEdge.New(GetVertex(5), GetVertex(14)));
		_edges.Add(QuestEdge.New(GetVertex(18), GetVertex(14)));
		_edges.Add(QuestEdge.New(GetVertex(13), GetVertex(15)));
		_edges.Add(QuestEdge.New(GetVertex(39), GetVertex(15)));
		_edges.Add(QuestEdge.New(GetVertex(11), GetVertex(16)));
		_edges.Add(QuestEdge.New(GetVertex(12), GetVertex(16)));
		_edges.Add(QuestEdge.New(GetVertex(20), GetVertex(16)));
		_edges.Add(QuestEdge.New(GetVertex(13), GetVertex(17)));
		_edges.Add(QuestEdge.New(GetVertex(11), GetVertex(18)));
		_edges.Add(QuestEdge.New(GetVertex(12), GetVertex(18)));
		_edges.Add(QuestEdge.New(GetVertex(20), GetVertex(18)));
		_edges.Add(QuestEdge.New(GetVertex(5), GetVertex(19)));
		_edges.Add(QuestEdge.New(GetVertex(14), GetVertex(19)).AddConditions(Condition.New(GetVertex(14), ValidType.IsDone), ConditionType.Required));
		_edges.Add(QuestEdge.New(GetVertex(7), GetVertex(20)));
		_edges.Add(QuestEdge.New(GetVertex(13), GetVertex(20)));
		_edges.Add(QuestEdge.New(GetVertex(10), GetVertex(21)));
		_edges.Add(QuestEdge.New(GetVertex(27), GetVertex(21)).AddConditions(Condition.New(GetVertex(27), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(36), GetVertex(21)).AddConditions(Condition.New(GetVertex(36), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(10), GetVertex(22)));
		_edges.Add(QuestEdge.New(GetVertex(26), GetVertex(22)));
		_edges.Add(QuestEdge.New(GetVertex(18), GetVertex(23)));
		_edges.Add(QuestEdge.New(GetVertex(16), GetVertex(24)));
		_edges.Add(QuestEdge.New(GetVertex(16), GetVertex(25)));
		_edges.Add(QuestEdge.New(GetVertex(18), GetVertex(26)));
		_edges.Add(QuestEdge.New(GetVertex(23), GetVertex(26)).AddConditions(Condition.New(GetVertex(23), ValidType.IsDone), ConditionType.Required));
		_edges.Add(QuestEdge.New(GetVertex(43), GetVertex(26)).AddConditions(Condition.New(GetVertex(43), ValidType.IsDone), ConditionType.Required));
		_edges.Add(QuestEdge.New(GetVertex(19), GetVertex(27)));
		_edges.Add(QuestEdge.New(GetVertex(35), GetVertex(27)).AddConditions(Condition.New(GetVertex(21), ValidType.IsNotDone) & Condition.New(GetVertex(35), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(20), GetVertex(28)));
		_edges.Add(QuestEdge.New(GetVertex(12), GetVertex(28)));
		_edges.Add(QuestEdge.New(GetVertex(6), GetVertex(28)).AddConditions(Condition.New(GetVertex(6), ValidType.IsDone), ConditionType.Required));
		_edges.Add(QuestEdge.New(GetVertex(28), GetVertex(29)));
		_edges.Add(QuestEdge.New(GetVertex(24), GetVertex(30)));
		_edges.Add(QuestEdge.New(GetVertex(22), GetVertex(31)));
		_edges.Add(QuestEdge.New(GetVertex(14), GetVertex(31)).AddConditions(Condition.New(GetVertex(14), ValidType.IsDone), ConditionType.Required));
		_edges.Add(QuestEdge.New(GetVertex(21), GetVertex(31)).AddConditions(Condition.New(GetVertex(21), ValidType.IsDone), ConditionType.Required));
		_edges.Add(QuestEdge.New(GetVertex(35), GetVertex(31)).AddConditions(Condition.New(GetVertex(35), ValidType.IsDone) & Condition.New(GetVertex(21), ValidType.IsNotDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(24), GetVertex(32)));
		_edges.Add(QuestEdge.New(GetVertex(42), GetVertex(32)).AddConditions(Condition.New(GetVertex(42), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(25), GetVertex(33)));
		_edges.Add(QuestEdge.New(GetVertex(32), GetVertex(33)));
		_edges.Add(QuestEdge.New(GetVertex(42), GetVertex(33)).AddConditions(Condition.New(GetVertex(25), ValidType.IsNotDone) & Condition.New(GetVertex(42), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(34), GetVertex(33)).AddConditions(Condition.New(GetVertex(24), ValidType.IsNotDone) & Condition.New(GetVertex(34), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(34), GetVertex(33)).AddConditions(Condition.New(GetVertex(42), ValidType.IsDone) & Condition.New(GetVertex(34), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(25), GetVertex(34)));
		_edges.Add(QuestEdge.New(GetVertex(33), GetVertex(34)).AddConditions(Condition.New(GetVertex(33), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(22), GetVertex(35)));
		_edges.Add(QuestEdge.New(GetVertex(10), GetVertex(35)).AddConditions(Condition.New(GetVertex(10), ValidType.IsDone), ConditionType.Required));
		_edges.Add(QuestEdge.New(GetVertex(21), GetVertex(35)).AddConditions(Condition.New(GetVertex(21), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(36), GetVertex(35)).AddConditions(Condition.New(GetVertex(36), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(27), GetVertex(35)).AddConditions(Condition.New(GetVertex(27), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(22), GetVertex(36)));
		_edges.Add(QuestEdge.New(GetVertex(10), GetVertex(36)).AddConditions(Condition.New(GetVertex(10), ValidType.IsDone), ConditionType.Required));
		_edges.Add(QuestEdge.New(GetVertex(21), GetVertex(36)).AddConditions(Condition.New(GetVertex(21), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(35), GetVertex(36)).AddConditions(Condition.New(GetVertex(35), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(27), GetVertex(36)).AddConditions(Condition.New(GetVertex(27), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(31), GetVertex(37)));
		_edges.Add(QuestEdge.New(GetVertex(43), GetVertex(37)).AddConditions(Condition.New(GetVertex(43), ValidType.IsDone), ConditionType.Required));
		_edges.Add(QuestEdge.New(GetVertex(31), GetVertex(38)));
		_edges.Add(QuestEdge.New(GetVertex(31), GetVertex(39)));
		_edges.Add(QuestEdge.New(GetVertex(32), GetVertex(40)));
		_edges.Add(QuestEdge.New(GetVertex(33), GetVertex(40)));
		_edges.Add(QuestEdge.New(GetVertex(42), GetVertex(40)).AddConditions(Condition.New(GetVertex(42), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(40), GetVertex(41)));
		_edges.Add(QuestEdge.New(GetVertex(42), GetVertex(41)).AddConditions(Condition.New(GetVertex(42), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(30), GetVertex(42)));
		_edges.Add(QuestEdge.New(GetVertex(41), GetVertex(42)).AddConditions(Condition.New(GetVertex(41), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(18), GetVertex(43)));
		_edges.Add(QuestEdge.New(GetVertex(31), GetVertex(43)));
		_edges.Add(QuestEdge.New(GetVertex(14), GetVertex(43)).AddConditions(Condition.New(GetVertex(14), ValidType.IsDone), ConditionType.Required));
		_edges.Add(QuestEdge.New(GetVertex(38), GetVertex(44)));
		_edges.Add(QuestEdge.New(GetVertex(35), GetVertex(45)));
		_edges.Add(QuestEdge.New(GetVertex(39), GetVertex(46)));
		_edges.Add(QuestEdge.New(GetVertex(37), GetVertex(47)));
		_edges.Add(QuestEdge.New(GetVertex(38), GetVertex(48)));
		_edges.Add(QuestEdge.New(GetVertex(45), GetVertex(49)));
		_edges.Add(QuestEdge.New(GetVertex(50), GetVertex(49)).AddConditions(Condition.New(GetVertex(50), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(45), GetVertex(50)));
		_edges.Add(QuestEdge.New(GetVertex(49), GetVertex(50)).AddConditions(Condition.New(GetVertex(49), ValidType.IsDone), ConditionType.Block));
		_edges.Add(QuestEdge.New(GetVertex(47), GetVertex(51)));
		_edges.Add(QuestEdge.New(GetVertex(46), GetVertex(51)).AddConditions(Condition.New(GetVertex(46), ValidType.IsDone), ConditionType.Required));
		_edges.Add(QuestEdge.New(GetVertex(48), GetVertex(51)).AddConditions(Condition.New(GetVertex(48), ValidType.IsDone), ConditionType.Required));
		GetVertex(51).CheckAllRequired();
		List<int> list = new List<int>();
		QuestVertices[50].GetNeededQuestsToUnlock(list);
		foreach (int item in list)
		{
			_mainQuestVertices.Add(GetVertex(item));
		}
		_mainQuestVertices.Add(GetVertex(51));
	}

	private QuestVertex GetVertex(int id)
	{
		if (id > _questVertices.Length)
		{
			return null;
		}
		return _questVertices[id - 1];
	}

	private void OnQuestTypeChanged(int questVertexID, QuestType newType)
	{
		string questId = GetVertex(questVertexID).QuestId;
		switch (newType)
		{
		case QuestType.Completed:
			LogUtils.Log(string.Format("[{0}] Quest complete: {1} - {2};", "CampaignProgressGraph", questVertexID, questId));
			this.QuestCompleteEvent?.Invoke(questVertexID, questId);
			break;
		case QuestType.Blocked:
			LogUtils.Log(string.Format("[{0}] Quest blocked: {1} - {2};", "CampaignProgressGraph", questVertexID, questId));
			this.QuestBlockedEvent?.Invoke(questVertexID, questId);
			break;
		}
	}
}
