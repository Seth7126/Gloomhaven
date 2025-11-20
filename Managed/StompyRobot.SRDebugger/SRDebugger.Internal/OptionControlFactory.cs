using System;
using System.Collections.Generic;
using System.Linq;
using SRDebugger.UI.Controls;
using SRDebugger.UI.Controls.Data;
using SRF;
using UnityEngine;

namespace SRDebugger.Internal;

public static class OptionControlFactory
{
	private struct OptionType
	{
		public readonly Type Type;

		public readonly bool IsReadyOnly;

		public OptionType(Type type, bool isReadyOnly)
		{
			Type = type;
			IsReadyOnly = isReadyOnly;
		}

		public bool Equals(OptionType other)
		{
			if (object.Equals(Type, other.Type))
			{
				return IsReadyOnly == other.IsReadyOnly;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is OptionType other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int num = ((Type != null) ? Type.GetHashCode() : 0) * 397;
			bool isReadyOnly = IsReadyOnly;
			return num ^ isReadyOnly.GetHashCode();
		}
	}

	private static IList<DataBoundControl> _dataControlPrefabs;

	private static ActionControl _actionControlPrefab;

	private static readonly Dictionary<OptionType, DataBoundControl> TypeCache = new Dictionary<OptionType, DataBoundControl>();

	public static bool CanCreateControl(OptionDefinition from)
	{
		PopulateDataControlPrefabs();
		if (from.Property != null)
		{
			return TryGetDataControlPrefab(from) != null;
		}
		return _actionControlPrefab != null;
	}

	public static OptionsControlBase CreateControl(OptionDefinition from, string categoryPrefix = null)
	{
		PopulateDataControlPrefabs();
		if (from.Property != null)
		{
			return CreateDataControl(from, categoryPrefix);
		}
		if (from.Method != null)
		{
			return CreateActionControl(from, categoryPrefix);
		}
		throw new Exception("OptionDefinition did not contain property or method.");
	}

	private static void PopulateDataControlPrefabs()
	{
		if (_dataControlPrefabs == null)
		{
			_dataControlPrefabs = Resources.LoadAll<DataBoundControl>("SRDebugger/UI/Prefabs/Options");
		}
		if (_actionControlPrefab == null)
		{
			_actionControlPrefab = Resources.LoadAll<ActionControl>("SRDebugger/UI/Prefabs/Options").FirstOrDefault();
		}
		if (_actionControlPrefab == null)
		{
			Debug.LogError("[SRDebugger.Options] Cannot find ActionControl prefab.");
		}
	}

	private static ActionControl CreateActionControl(OptionDefinition from, string categoryPrefix = null)
	{
		ActionControl actionControl = SRInstantiate.Instantiate(_actionControlPrefab);
		if (actionControl == null)
		{
			Debug.LogWarning("[SRDebugger.OptionsTab] Error creating action control from prefab");
			return null;
		}
		actionControl.SetMethod(from.Name, from.Method);
		actionControl.Option = from;
		return actionControl;
	}

	private static DataBoundControl CreateDataControl(OptionDefinition from, string categoryPrefix = null)
	{
		DataBoundControl dataBoundControl = TryGetDataControlPrefab(from);
		if (dataBoundControl == null)
		{
			Debug.LogWarning("[SRDebugger.OptionsTab] Can't find data control for type {0}".Fmt(from.Property.PropertyType));
			return null;
		}
		DataBoundControl dataBoundControl2 = SRInstantiate.Instantiate(dataBoundControl);
		try
		{
			string text = from.Name;
			if (!string.IsNullOrEmpty(categoryPrefix) && text.StartsWith(categoryPrefix))
			{
				text = text.Substring(categoryPrefix.Length);
			}
			dataBoundControl2.Bind(text, from.Property);
			dataBoundControl2.Option = from;
		}
		catch (Exception exception)
		{
			Debug.LogError("[SRDebugger.Options] Error binding to property {0}".Fmt(from.Name));
			Debug.LogException(exception);
			UnityEngine.Object.Destroy(dataBoundControl2);
			dataBoundControl2 = null;
		}
		return dataBoundControl2;
	}

	private static DataBoundControl TryGetDataControlPrefab(OptionDefinition from)
	{
		OptionType key = new OptionType(from.Property.PropertyType, !from.Property.CanWrite);
		if (!TypeCache.TryGetValue(key, out var value))
		{
			value = _dataControlPrefabs.FirstOrDefault((DataBoundControl p) => p.CanBind(from.Property.PropertyType, !from.Property.CanWrite));
			TypeCache.Add(key, value);
		}
		return value;
	}
}
