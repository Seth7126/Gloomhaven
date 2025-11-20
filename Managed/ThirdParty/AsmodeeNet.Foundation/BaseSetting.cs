using System;
using System.Collections.Generic;
using UnityEngine;

namespace AsmodeeNet.Foundation;

[Serializable]
public abstract class BaseSetting<T> : IBaseSetting
{
	[HideInInspector]
	[SerializeField]
	private string _name;

	[SerializeField]
	private T _defaultValue;

	public string Name
	{
		get
		{
			return _name;
		}
		private set
		{
			_name = value;
		}
	}

	protected string _FullPath => "Settings." + Name;

	public T Value
	{
		get
		{
			if (KeyValueStore.HasKey(_FullPath))
			{
				return _ReadValue();
			}
			return DefaultValue;
		}
		set
		{
			T value2 = Value;
			if (!EqualityComparer<T>.Default.Equals(value, value2))
			{
				_WriteValue(value);
				KeyValueStore.Save();
				this.OnValueChanged?.Invoke(value2, value);
			}
		}
	}

	public T DefaultValue => _defaultValue;

	public event Action<T, T> OnValueChanged;

	protected BaseSetting(string name)
	{
		Name = name;
	}

	public virtual void Clear()
	{
		T value = Value;
		KeyValueStore.DeleteKey(_FullPath);
		KeyValueStore.Save();
		if (!EqualityComparer<T>.Default.Equals(DefaultValue, value))
		{
			this.OnValueChanged?.Invoke(value, DefaultValue);
		}
	}

	protected abstract T _ReadValue();

	protected abstract void _WriteValue(T value);

	public override string ToString()
	{
		return $"{Name}={Value} (default:{DefaultValue})";
	}
}
