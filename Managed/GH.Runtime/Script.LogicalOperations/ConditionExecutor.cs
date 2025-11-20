using UnityEngine;
using UnityEngine.Events;

namespace Script.LogicalOperations;

public class ConditionExecutor : MonoBehaviour
{
	[SerializeField]
	private Condition _condition;

	[SerializeField]
	private UnityEvent _trueActions;

	[SerializeField]
	private UnityEvent _falseActions;

	public void Execute()
	{
		if (_condition.Value)
		{
			_trueActions?.Invoke();
		}
		else
		{
			_falseActions?.Invoke();
		}
	}
}
