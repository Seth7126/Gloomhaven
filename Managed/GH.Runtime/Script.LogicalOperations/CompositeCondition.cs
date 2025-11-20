using System.Collections.Generic;
using UnityEngine;

namespace Script.LogicalOperations;

public class CompositeCondition : Condition
{
	private enum CheckConditionsMode
	{
		AllTrue = 1,
		AnyTrue,
		AllEqual
	}

	[SerializeField]
	private Condition[] _conditions;

	[SerializeField]
	private CheckConditionsMode _checkConditionsMode;

	private IBooleanOperation _checkOperation;

	private IBooleanOperation CheckOperation
	{
		get
		{
			if (_checkOperation == null)
			{
				_checkOperation = GetCheckOperation(_checkConditionsMode);
			}
			return _checkOperation;
		}
	}

	protected override bool ConditionValue
	{
		get
		{
			if (_conditions.Length != 0)
			{
				return CheckOperation.Execute(GetConditionValues());
			}
			return true;
		}
	}

	private IEnumerable<bool> GetConditionValues()
	{
		for (int i = 0; i < _conditions.Length; i++)
		{
			yield return _conditions[i].Value;
		}
	}

	protected override void Subscribe()
	{
		for (int i = 0; i < _conditions.Length; i++)
		{
			_conditions[i].OnValueChanged += OnConditionValueChanged;
		}
	}

	protected override void Unsubscribe()
	{
		for (int i = 0; i < _conditions.Length; i++)
		{
			_conditions[i].OnValueChanged -= OnConditionValueChanged;
		}
	}

	private void OnConditionValueChanged(bool value)
	{
		InvokeOnValueChanged();
	}

	private IBooleanOperation GetCheckOperation(CheckConditionsMode checkConditionsMode)
	{
		return checkConditionsMode switch
		{
			CheckConditionsMode.AllTrue => new AllBooleanOperation(targetValue: true), 
			CheckConditionsMode.AnyTrue => new AnyBooleanOperation(targetValue: true), 
			CheckConditionsMode.AllEqual => new AllEqualBooleanOperation(), 
			_ => null, 
		};
	}
}
