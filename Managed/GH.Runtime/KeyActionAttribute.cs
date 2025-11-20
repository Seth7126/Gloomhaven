using System;

public class KeyActionAttribute : Attribute
{
	public string GroupId;

	public EKeyActionTag[] Tags;

	public KeyAction[] KeyRelatedWith;
}
