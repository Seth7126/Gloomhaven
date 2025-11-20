namespace GraphProgress;

public class CompositeCondition : ICondition
{
	public Condition[] Conditions { get; private set; }

	public OperationType OperationType { get; private set; }

	public CompositeCondition(Condition[] conditions, OperationType operationType)
	{
		Conditions = conditions;
		OperationType = operationType;
	}

	public bool IsValid()
	{
		OperationType operationType = OperationType;
		if (operationType != OperationType.Or)
		{
			_ = 2;
			return AndValid();
		}
		return OrValid();
	}

	public bool AndValid()
	{
		Condition[] conditions = Conditions;
		for (int i = 0; i < conditions.Length; i++)
		{
			if (!((ICondition)conditions[i]).IsValid())
			{
				return false;
			}
		}
		return true;
	}

	public bool OrValid()
	{
		Condition[] conditions = Conditions;
		for (int i = 0; i < conditions.Length; i++)
		{
			if (((ICondition)conditions[i]).IsValid())
			{
				return true;
			}
		}
		return false;
	}

	public static CompositeCondition operator &(CompositeCondition compositeCondition1, CompositeCondition compositeCondition2)
	{
		Condition[] array = new Condition[compositeCondition1.Conditions.Length + compositeCondition2.Conditions.Length];
		for (int i = 0; i < compositeCondition1.Conditions.Length; i++)
		{
			array[i] = compositeCondition1.Conditions[i];
		}
		for (int j = 0; j < compositeCondition2.Conditions.Length; j++)
		{
			array[compositeCondition1.Conditions.Length + j] = compositeCondition2.Conditions[j];
		}
		return new CompositeCondition(array, OperationType.And);
	}

	public static CompositeCondition operator |(CompositeCondition compositeCondition1, CompositeCondition compositeCondition2)
	{
		Condition[] array = new Condition[compositeCondition1.Conditions.Length + compositeCondition2.Conditions.Length];
		for (int i = 0; i < compositeCondition1.Conditions.Length; i++)
		{
			array[i] = compositeCondition1.Conditions[i];
		}
		for (int j = 0; j < compositeCondition2.Conditions.Length; j++)
		{
			array[compositeCondition1.Conditions.Length + j] = compositeCondition1.Conditions[j];
		}
		return new CompositeCondition(array, OperationType.Or);
	}
}
