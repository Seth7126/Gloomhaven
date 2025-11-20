using System;
using System.Collections.Generic;

namespace Manatee.Json.Serialization;

public interface IResolver
{
	object Resolve(Type type, Dictionary<SerializationInfo, object?>? parameters);
}
