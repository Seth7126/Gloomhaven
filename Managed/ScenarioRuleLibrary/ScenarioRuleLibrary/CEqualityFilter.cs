using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharedLibrary.Logger;

namespace ScenarioRuleLibrary;

public class CEqualityFilter : ISerializable
{
	public string EqualityString { get; private set; }

	public int Value { get; private set; }

	public bool ValueIsPercentage { get; internal set; }

	public bool Level { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("EqualityString", EqualityString);
		info.AddValue("Value", Value);
		info.AddValue("ValueIsPercentage", ValueIsPercentage);
		info.AddValue("Level", Level);
	}

	public CEqualityFilter(SerializationInfo info, StreamingContext context)
	{
		SerializationInfoEnumerator enumerator = info.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			try
			{
				switch (current.Name)
				{
				case "EqualityString":
					EqualityString = info.GetString("EqualityString");
					break;
				case "Value":
					Value = info.GetInt32("Value");
					break;
				case "ValueIsPercentage":
					ValueIsPercentage = info.GetBoolean("ValueIsPercentage");
					break;
				case "Level":
					Level = info.GetBoolean("Level");
					break;
				}
			}
			catch (Exception ex)
			{
				DLLDebug.LogError("Exception while trying to deserialize CEqualityFilter entry " + current.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public static bool IsValidEqualityString(string equalityString)
	{
		switch (equalityString)
		{
		default:
			return equalityString == "!=";
		case "==":
		case ">":
		case "<":
		case ">=":
		case "<=":
			return true;
		}
	}

	public CEqualityFilter(string equalityString, int value, bool valueIsPercentage, bool level = false)
	{
		if (!IsValidEqualityString(equalityString))
		{
			throw new Exception("Invalid equality string '" + equalityString + "' provided as comparer!");
		}
		EqualityString = equalityString;
		Value = value;
		ValueIsPercentage = valueIsPercentage;
		Level = level;
	}

	public CEqualityFilter Copy()
	{
		return new CEqualityFilter(EqualityString, Value, ValueIsPercentage);
	}

	public bool Equals(CEqualityFilter compare)
	{
		if (EqualityString == compare?.EqualityString)
		{
			return Value == compare?.Value;
		}
		return false;
	}

	public bool Compare(int compareValue, int filterValueIsPercentageOperand = -1, int level = 0)
	{
		int num = Value;
		if (Level)
		{
			num += level;
		}
		if (filterValueIsPercentageOperand >= 0 && Value >= 0 && Value <= 100)
		{
			num = (int)Math.Ceiling((float)filterValueIsPercentageOperand * ((float)Value / 100f));
		}
		switch (EqualityString)
		{
		case "==":
			return compareValue == num;
		case ">":
			return compareValue > num;
		case "<":
			return compareValue < num;
		case ">=":
			return compareValue >= num;
		case "<=":
			return compareValue <= num;
		case "!=":
			return compareValue != num;
		default:
			DLLDebug.LogError(" Invalid equality comparison " + EqualityString);
			return false;
		}
	}
}
