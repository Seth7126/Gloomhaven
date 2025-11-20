using System;
using System.Linq;
using System.Reflection;

public static class KeyActionExtension
{
	public static string GetGroupId(this KeyAction enumValue)
	{
		return enumValue.GetAttribute()?.GroupId;
	}

	public static EKeyActionTag[] GetTags(this KeyAction enumValue)
	{
		return enumValue.GetAttribute()?.Tags;
	}

	public static KeyAction[] GetRelatedActions(this KeyAction enumValue)
	{
		return enumValue.GetAttribute()?.KeyRelatedWith;
	}

	public static bool IsAssociated(this KeyAction enumValue, EKeyActionTag tag)
	{
		if (tag == EKeyActionTag.None)
		{
			return false;
		}
		if (tag == EKeyActionTag.All)
		{
			return true;
		}
		return enumValue.GetTags()?.Any((EKeyActionTag it) => it == tag || tag.HasFlag(it)) ?? false;
	}

	public static KeyActionAttribute GetAttribute(this KeyAction value)
	{
		Type type = value.GetType();
		string name = Enum.GetName(type, value);
		return type.GetField(name).GetCustomAttribute<KeyActionAttribute>();
	}
}
