using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace SRF.Helpers;

public sealed class PropertyReference
{
	[CanBeNull]
	private readonly PropertyInfo _property;

	[CanBeNull]
	private readonly object _target;

	[CanBeNull]
	private readonly Attribute[] _attributes;

	[CanBeNull]
	private readonly Func<object> _getter;

	[CanBeNull]
	private readonly Action<object> _setter;

	[CanBeNull]
	private List<PropertyValueChangedHandler> _valueChangedListeners;

	public Type PropertyType { get; private set; }

	public bool CanRead => _getter != null;

	public bool CanWrite => _setter != null;

	public event PropertyValueChangedHandler ValueChanged
	{
		add
		{
			if (_valueChangedListeners == null)
			{
				_valueChangedListeners = new List<PropertyValueChangedHandler>();
			}
			_valueChangedListeners.Add(value);
			if (_valueChangedListeners.Count == 1 && _target is INotifyPropertyChanged)
			{
				((INotifyPropertyChanged)_target).PropertyChanged += OnTargetPropertyChanged;
			}
		}
		remove
		{
			if (_valueChangedListeners != null && _valueChangedListeners.Remove(value) && _valueChangedListeners.Count == 0 && _target is INotifyPropertyChanged)
			{
				((INotifyPropertyChanged)_target).PropertyChanged -= OnTargetPropertyChanged;
			}
		}
	}

	public static PropertyReference FromLambda<T>(Func<T> getter, Action<T> setter = null, params Attribute[] attributes)
	{
		Action<object> setter2 = null;
		if (setter != null)
		{
			setter2 = delegate(object o)
			{
				setter((T)o);
			};
		}
		return new PropertyReference(typeof(T), () => getter(), setter2, attributes);
	}

	public PropertyReference(object target, PropertyInfo property)
	{
		SRDebugUtil.AssertNotNull(target);
		SRDebugUtil.AssertNotNull(property);
		PropertyType = property.PropertyType;
		_property = property;
		_target = target;
		if (property.GetGetMethod() != null)
		{
			_getter = () => SRReflection.GetPropertyValue(target, property);
		}
		if (property.GetSetMethod() != null)
		{
			_setter = delegate(object v)
			{
				SRReflection.SetPropertyValue(target, property, v);
			};
		}
	}

	public PropertyReference(Type type, Func<object> getter = null, Action<object> setter = null, Attribute[] attributes = null)
	{
		SRDebugUtil.AssertNotNull(type);
		PropertyType = type;
		_attributes = attributes;
		_getter = getter;
		_setter = setter;
	}

	public void NotifyValueChanged()
	{
		if (_valueChangedListeners == null)
		{
			return;
		}
		foreach (PropertyValueChangedHandler valueChangedListener in _valueChangedListeners)
		{
			valueChangedListener(this);
		}
	}

	public object GetValue()
	{
		if (_getter != null)
		{
			return _getter();
		}
		return null;
	}

	public void SetValue(object value)
	{
		if (_setter != null)
		{
			_setter(value);
			return;
		}
		throw new InvalidOperationException("Can not write to property");
	}

	public T GetAttribute<T>() where T : Attribute
	{
		if (_attributes != null)
		{
			return _attributes.FirstOrDefault((Attribute p) => p is T) as T;
		}
		if (_property != null)
		{
			return _property.GetCustomAttributes(typeof(T), inherit: true).FirstOrDefault() as T;
		}
		return null;
	}

	private void OnTargetPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (_valueChangedListeners == null || _valueChangedListeners.Count == 0)
		{
			Debug.LogWarning("[PropertyReference] Received property value changed event when there are no listeners. Did the event not get unsubscribed correctly?");
		}
		else
		{
			NotifyValueChanged();
		}
	}

	public override string ToString()
	{
		if (_property != null)
		{
			return "{0}.{1}".Fmt(_property.DeclaringType.Name, _property.Name);
		}
		return "<delegate>";
	}
}
