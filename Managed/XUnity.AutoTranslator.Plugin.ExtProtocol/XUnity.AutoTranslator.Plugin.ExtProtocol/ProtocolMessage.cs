using System;
using System.IO;

namespace XUnity.AutoTranslator.Plugin.ExtProtocol;

public abstract class ProtocolMessage
{
	public Guid Id { get; set; }

	internal abstract void Decode(TextReader reader);

	internal abstract void Encode(TextWriter writer);
}
