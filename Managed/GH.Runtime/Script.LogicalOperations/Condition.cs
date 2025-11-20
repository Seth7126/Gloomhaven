using System;
using UnityEngine;

namespace Script.LogicalOperations;

public abstract class Condition : MonoBehaviour, ICondition
{
	[SerializeField]
	private bool _inverseCondition;

	protected abstract bool ConditionValue { get; }

	public bool Value => ConditionValue ^ _inverseCondition;

	public event Action<bool> OnValueChanged;

	private void OnEnable()
	{
		Subscribe();
	}

	private void OnDisable()
	{
		Unsubscribe();
	}

	protected abstract void Subscribe();

	protected abstract void Unsubscribe();

	protected void InvokeOnValueChanged()
	{
		this.OnValueChanged?.Invoke(Value);
	}
}
