using System;
using System.Collections.Generic;

namespace GraphProgress;

public class QuestVertex
{
	private QuestType _questType;

	private bool _isCheckAllRequired;

	private float _cost;

	public int Id { get; protected set; }

	public string QuestId { get; protected set; }

	public bool IsComplete { get; protected set; }

	public List<QuestEdge> QuestEdges { get; private set; }

	public QuestType QuestType
	{
		get
		{
			return _questType;
		}
		private set
		{
			if (_questType != value)
			{
				_questType = value;
				this.TypeChangedEvent?.Invoke(Id, _questType);
			}
		}
	}

	public event Action<int, QuestType> TypeChangedEvent;

	public QuestVertex(int id)
	{
		Id = id;
		QuestEdges = new List<QuestEdge>();
		QuestId = GetQuestID(id);
	}

	public void SetQuestEdge(QuestEdge questEdge)
	{
		QuestEdges.Add(questEdge);
	}

	public bool IsVisible()
	{
		if (!IsBlock())
		{
			if (GetQuestEdgesWithConditionType(ConditionType.Unclock) == 0)
			{
				QuestType = QuestType.Available;
				return true;
			}
			foreach (QuestEdge questEdge in QuestEdges)
			{
				if (questEdge.ConditionType == ConditionType.Unclock && questEdge.IsActive())
				{
					QuestType = QuestType.Visible;
					return true;
				}
			}
		}
		return false;
	}

	public bool IsAvailable()
	{
		if (IsVisible())
		{
			if (GetQuestEdgesWithConditionType(ConditionType.Required) == 0)
			{
				QuestType = QuestType.Available;
				return true;
			}
			if (_isCheckAllRequired)
			{
				foreach (QuestEdge questEdge in QuestEdges)
				{
					if (questEdge.ConditionType == ConditionType.Required && !questEdge.IsActive())
					{
						return false;
					}
				}
				QuestType = QuestType.Available;
				return true;
			}
			foreach (QuestEdge questEdge2 in QuestEdges)
			{
				if (questEdge2.ConditionType == ConditionType.Required && questEdge2.IsActive())
				{
					QuestType = QuestType.Available;
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public bool IsBlock()
	{
		foreach (QuestEdge questEdge in QuestEdges)
		{
			if (questEdge.ConditionType == ConditionType.Block && questEdge.IsActive())
			{
				QuestType = QuestType.Blocked;
				return true;
			}
		}
		if (HasBlockParents())
		{
			QuestType = QuestType.Blocked;
			return true;
		}
		return false;
	}

	public void GetNeededQuestsToUnlock(List<int> questVertices)
	{
		foreach (QuestEdge questEdge in QuestEdges)
		{
			if (questEdge.ConditionType != ConditionType.Block && !questVertices.Contains(questEdge.From.Id))
			{
				questVertices.Add(questEdge.From.Id);
				questEdge.From.GetNeededQuestsToUnlock(questVertices);
			}
		}
	}

	public bool Complete()
	{
		if (IsAvailable())
		{
			IsComplete = true;
			QuestType = QuestType.Completed;
			return true;
		}
		return false;
	}

	public void UpdateQuestState()
	{
		if (QuestType != QuestType.Completed)
		{
			IsAvailable();
		}
		else
		{
			IsBlock();
		}
	}

	public void CheckAllRequired()
	{
		_isCheckAllRequired = true;
	}

	public void SetCost(float cost)
	{
		_cost = cost;
	}

	public float GetCost()
	{
		if (QuestType == QuestType.Blocked || QuestType == QuestType.Completed)
		{
			return _cost;
		}
		return 0f;
	}

	private int GetQuestEdgesWithConditionType(ConditionType condition)
	{
		int num = 0;
		foreach (QuestEdge questEdge in QuestEdges)
		{
			if (questEdge.ConditionType == condition)
			{
				num++;
			}
		}
		return num;
	}

	private bool HasBlockParents()
	{
		if (QuestEdges.Count == 0)
		{
			return false;
		}
		foreach (QuestEdge questEdge in QuestEdges)
		{
			if (questEdge.ConditionType == ConditionType.Unclock && questEdge.From.QuestType != QuestType.Blocked)
			{
				return false;
			}
		}
		return true;
	}

	private string GetQuestID(int questVertexID)
	{
		string text = "Quest_Campaign_0";
		string text2 = questVertexID.ToString();
		if (text2.Length == 1)
		{
			return text + "0" + text2;
		}
		return text + text2;
	}
}
