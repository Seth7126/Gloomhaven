using System;
using System.Runtime.Serialization;

namespace XUnity.AutoTranslator.Plugin.Core.Endpoints;

[Serializable]
internal class TranslationContextException : Exception
{
	public TranslationContextException()
	{
	}

	public TranslationContextException(string message)
		: base(message)
	{
	}

	public TranslationContextException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected TranslationContextException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
