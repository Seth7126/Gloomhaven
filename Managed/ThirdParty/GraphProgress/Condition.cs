namespace GraphProgress;

public class Condition : ICondition
{
	private ValidType _validType;

	public QuestVertex Vertex { get; private set; }

	private Condition(QuestVertex vertex, ValidType validType)
	{
		Vertex = vertex;
		_validType = validType;
	}

	public bool IsValid()
	{
		return _validType switch
		{
			ValidType.IsDone => Vertex.IsComplete, 
			ValidType.IsNotDone => !Vertex.IsComplete, 
			_ => Vertex.IsComplete, 
		};
	}

	public static CompositeCondition New(QuestVertex vertex, ValidType validType)
	{
		return new CompositeCondition(new Condition[1]
		{
			new Condition(vertex, validType)
		}, OperationType.Simple);
	}
}
