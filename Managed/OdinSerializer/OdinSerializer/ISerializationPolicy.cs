using System.Reflection;

namespace OdinSerializer;

public interface ISerializationPolicy
{
	string ID { get; }

	bool AllowNonSerializableTypes { get; }

	bool ShouldSerializeMember(MemberInfo member);
}
