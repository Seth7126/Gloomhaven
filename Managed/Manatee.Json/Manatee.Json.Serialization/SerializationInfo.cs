using System.Reflection;

namespace Manatee.Json.Serialization;

public class SerializationInfo
{
	public MemberInfo MemberInfo { get; }

	public string SerializationName { get; }

	public bool ShouldTransform { get; }

	internal SerializationInfo(MemberInfo memberInfo, string serializationName, bool shouldTransform)
	{
		MemberInfo = memberInfo;
		SerializationName = serializationName;
		ShouldTransform = shouldTransform;
	}

	public override bool Equals(object? obj)
	{
		if (obj != null)
		{
			return MemberInfo.Equals(((SerializationInfo)obj).MemberInfo);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return MemberInfo.GetHashCode();
	}
}
