using System;

namespace OdinSerializer;

public interface IAskIfCanFormatTypes
{
	bool CanFormatType(Type type);
}
