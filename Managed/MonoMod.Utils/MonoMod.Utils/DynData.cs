using System;
using System.Collections.Generic;
using System.Reflection;

namespace MonoMod.Utils;

public sealed class DynData<TTarget> : IDisposable where TTarget : class
{
	private class _Data_
	{
		public readonly Dictionary<string, Func<TTarget, object>> Getters = new Dictionary<string, Func<TTarget, object>>();

		public readonly Dictionary<string, Action<TTarget, object>> Setters = new Dictionary<string, Action<TTarget, object>>();

		public readonly Dictionary<string, object> Data = new Dictionary<string, object>();
	}

	private sealed class WeakReferenceComparer : EqualityComparer<WeakReference>
	{
		public override bool Equals(WeakReference x, WeakReference y)
		{
			if (x.Target == y.Target)
			{
				return x.IsAlive == y.IsAlive;
			}
			return false;
		}

		public override int GetHashCode(WeakReference obj)
		{
			return obj.Target?.GetHashCode() ?? 0;
		}
	}

	private static readonly object[] _NoArgs;

	public static readonly HashSet<string> Disposable;

	private static readonly _Data_ _DataStatic;

	private static readonly Dictionary<WeakReference, _Data_> _DataMap;

	private static readonly Dictionary<string, Func<TTarget, object>> _SpecialGetters;

	private static readonly Dictionary<string, Action<TTarget, object>> _SpecialSetters;

	private readonly WeakReference Weak;

	private readonly _Data_ _Data;

	public Dictionary<string, Func<TTarget, object>> Getters => _Data.Getters;

	public Dictionary<string, Action<TTarget, object>> Setters => _Data.Setters;

	public Dictionary<string, object> Data => _Data.Data;

	public bool IsAlive => Weak.IsAlive;

	public TTarget Target => Weak.Target as TTarget;

	public object this[string name]
	{
		get
		{
			if (_SpecialGetters.TryGetValue(name, out var value) || Getters.TryGetValue(name, out value))
			{
				return value(Weak.Target as TTarget);
			}
			if (Data.TryGetValue(name, out var value2))
			{
				return value2;
			}
			return null;
		}
		set
		{
			if (_SpecialSetters.TryGetValue(name, out var value2) || Setters.TryGetValue(name, out value2))
			{
				value2(Weak.Target as TTarget, value);
				return;
			}
			object obj;
			if (Disposable.Contains(name) && (obj = this[name]) != null && obj is IDisposable disposable)
			{
				disposable.Dispose();
			}
			Data[name] = value;
		}
	}

	public static event Action<DynData<TTarget>, TTarget> OnInitialize;

	static DynData()
	{
		_NoArgs = new object[0];
		Disposable = new HashSet<string>();
		_DataStatic = new _Data_();
		_DataMap = new Dictionary<WeakReference, _Data_>(new WeakReferenceComparer());
		_SpecialGetters = new Dictionary<string, Func<TTarget, object>>();
		_SpecialSetters = new Dictionary<string, Action<TTarget, object>>();
		_DataHelper_.Collected += delegate
		{
			HashSet<WeakReference> hashSet = new HashSet<WeakReference>();
			foreach (WeakReference key in _DataMap.Keys)
			{
				if (!key.IsAlive)
				{
					hashSet.Add(key);
				}
			}
			foreach (WeakReference item in hashSet)
			{
				_DataMap.Remove(item);
			}
		};
		FieldInfo[] fields = typeof(TTarget).GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo field in fields)
		{
			string name = field.Name;
			_SpecialGetters[name] = (TTarget obj) => field.GetValue(obj);
			_SpecialSetters[name] = delegate(TTarget obj, object value)
			{
				field.SetValue(obj, value);
			};
		}
		PropertyInfo[] properties = typeof(TTarget).GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (PropertyInfo propertyInfo in properties)
		{
			string name2 = propertyInfo.Name;
			MethodInfo get = propertyInfo.GetGetMethod(nonPublic: true);
			if (get != null)
			{
				_SpecialGetters[name2] = (TTarget obj) => get.Invoke(obj, _NoArgs);
			}
			MethodInfo set = propertyInfo.GetSetMethod(nonPublic: true);
			if (set != null)
			{
				_SpecialSetters[name2] = delegate(TTarget obj, object value)
				{
					set.Invoke(obj, _NoArgs);
				};
			}
		}
	}

	public DynData(TTarget obj)
	{
		if (obj != null)
		{
			WeakReference weakReference = new WeakReference(obj);
			lock (_DataMap)
			{
				if (!_DataMap.TryGetValue(weakReference, out _Data))
				{
					_Data = new _Data_();
					_DataMap.Add(weakReference, _Data);
				}
			}
			Weak = weakReference;
		}
		else
		{
			_Data = _DataStatic;
		}
		DynData<TTarget>.OnInitialize?.Invoke(this, obj);
	}

	public T Get<T>(string name)
	{
		return (T)this[name];
	}

	public void Set<T>(string name, T value)
	{
		this[name] = value;
	}

	public void RegisterProperty(string name, Func<TTarget, object> getter, Action<TTarget, object> setter)
	{
		Getters[name] = getter;
		Setters[name] = setter;
	}

	public void UnregisterProperty(string name)
	{
		Getters.Remove(name);
		Setters.Remove(name);
	}

	private void Dispose(bool disposing)
	{
		foreach (string item in Disposable)
		{
			if (Data.TryGetValue(item, out var value) && value is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
		Data.Clear();
	}

	~DynData()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
