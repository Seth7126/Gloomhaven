namespace GraphProgress;

public class QuestEdge
{
	public CompositeCondition Condition { get; private set; }

	public QuestVertex From { get; protected set; }

	public QuestVertex To { get; protected set; }

	public ConditionType ConditionType { get; protected set; }

	private QuestEdge(QuestVertex from, QuestVertex to)
	{
		From = from;
		To = to;
		To.SetQuestEdge(this);
		ConditionType = ConditionType.Unclock;
	}

	public QuestEdge AddConditions(CompositeCondition condition, ConditionType conditionType)
	{
		ConditionType = conditionType;
		if (Condition != null)
		{
			Condition &= condition;
		}
		else
		{
			Condition = condition;
		}
		return this;
	}

	public bool IsActive()
	{
		if (Condition != null)
		{
			if (!Condition.IsValid())
			{
				return false;
			}
			return true;
		}
		return From.IsComplete;
	}

	public static QuestEdge New(QuestVertex from, QuestVertex to)
	{
		return new QuestEdge(from, to);
	}
}
